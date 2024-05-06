namespace Decksteria.FECipher.Services;

using Decksteria.Core.Data;
using Decksteria.FECipher.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

internal sealed class FECardListService : IFECardListService
{
    private readonly IDecksteriaFileReader fileReader;

    private IEnumerable<FECard>? cardList;

    public FECardListService(IDecksteriaFileReader fileReader)
    {
        this.fileReader = fileReader;
    }

    public async Task<IEnumerable<FECard>> GetCardList()
    {
        if (cardList is null)
        {
            var cardListMD5 = await fileReader.ReadTextFileAsync(null, "https://raw.githubusercontent.com/Decksteria/Decksteria.FECipher/main/Decksteria.FECipher/cardlist.json.md5");
            var jsonText = await fileReader.ReadTextFileAsync("cardlist.json", "https://raw.githubusercontent.com/Decksteria/Decksteria.FECipher/main/Decksteria.FECipher/cardlist.json", cardListMD5);
            var cardlist = JsonSerializer.Deserialize<IEnumerable<FECard>>(jsonText) ?? Array.Empty<FECard>();
            cardList = cardlist ?? Array.Empty<FECard>();
        }

        return cardList;
    }
}
