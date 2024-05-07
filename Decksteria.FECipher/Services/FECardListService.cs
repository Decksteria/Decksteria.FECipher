namespace Decksteria.FECipher.Services;

using Decksteria.Core.Data;
using Decksteria.FECipher.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

internal sealed class FECardListService : IFECardListService
{
    private const string checksumURL = "https://raw.githubusercontent.com/Decksteria/Decksteria.FECipher/users/ere/fix-file-corruption/Decksteria.FECipher/cardlist.json.md5";

    private const string cardlistURL = "https://raw.githubusercontent.com/Decksteria/Decksteria.FECipher/main/Decksteria.FECipher/cardlist.json";

    private readonly IDecksteriaFileReader fileReader;

    private readonly ILogger<FECardListService> logger;

    private IEnumerable<FECard>? cardlist;

    public FECardListService(IDecksteriaFileReader fileReader, ILogger<FECardListService> logger)
    {
        this.fileReader = fileReader;
        this.logger = logger;
    }

    public async Task<IEnumerable<FECard>> GetCardList(CancellationToken cancellationToken = default)
    {
        if (cardlist is null)
        {
            // Failing to get the MD5 Checksum should not result in failing to read the cardlist json file.
            string? cardListMD5 = null;
            try
            {
                cardListMD5 = await fileReader.ReadOnlineTextAsync(checksumURL, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }

            var jsonText = await fileReader.ReadTextFileAsync("cardlist.json", cardlistURL, cardListMD5, cancellationToken);
            var cardlist = JsonSerializer.Deserialize<IEnumerable<FECard>>(jsonText) ?? Array.Empty<FECard>();
            this.cardlist = cardlist ?? Array.Empty<FECard>();
        }

        return cardlist;
    }
}
