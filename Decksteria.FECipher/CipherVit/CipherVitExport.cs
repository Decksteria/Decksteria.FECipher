﻿namespace Decksteria.FECipher.CipherVit;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Services;

internal sealed class CipherVitExport(IFECardListService feCardlistService) : IDecksteriaExport
{
    private readonly IFECardListService feCardlistService = feCardlistService;

    public string FileType => ".fe0d";

    public string Label => "CipherVit";

    public async Task<MemoryStream> SaveDecklistAsync(Decklist decklist, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        var cardlist = (await feCardlistService.GetCardList(cancellationToken)).ToDictionary(kv => kv.CardId);
        var cipherVitText = string.Empty;

        foreach (var cardart in decklist.Decks.SelectMany(card => card.Value))
        {
            var card = cardlist.GetValueOrDefault(cardart.CardId);
            var art = card?.AltArts.FirstOrDefault(art => art.ArtId == cardart.ArtId);

            if (art != null)
            {
                await streamWriter.WriteLineAsync(art.CipherVitId);
            }
        }

        await streamWriter.FlushAsync(cancellationToken);

        return memoryStream;
    }
}
