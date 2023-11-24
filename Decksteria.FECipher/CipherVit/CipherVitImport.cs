namespace Decksteria.FECipher.LackeyCCG;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

internal sealed class CipherVitImport(IFECardListService feCardlistService) : IDecksteriaImport
{
    public string Name => "CipherVit";

    public string FileType => ".fe0d";

    public string Label => "From CipherVit";

    public IFECardListService feCardlistService = feCardlistService;

    public async Task<Decklist> LoadDecklistAsync(MemoryStream memoryStream, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(memoryStream);
        var lackeyDictionary = await GetCipherVitDictionary();
        var mainDeck = new List<CardArt>();

        var line = await reader.ReadLineAsync(cancellationToken);
        while (line != null)
        {
            var newCard = lackeyDictionary.GetValueOrDefault(line.Trim());
            if (newCard != null)
            {
                mainDeck.Add(newCard);
            }

            line = await reader.ReadLineAsync(cancellationToken);
        }

        var file = new Decklist(FECipher.PlugInName, FormatConstants.Unlimited, new Dictionary<string, IEnumerable<CardArt>>()
        {
            { DeckConstants.MainCharacterDeck, Array.Empty<CardArt>() },
            { DeckConstants.MainDeck, mainDeck }
        });
        return file;
    }

    private async Task<IReadOnlyDictionary<string, CardArt>> GetCipherVitDictionary()
    {
        var dictionary = new Dictionary<string, CardArt>();
        var cardlist = await feCardlistService.GetCardList();
        var arts = cardlist.SelectMany(card => card.AltArts.Select(art => ToCipherVitKeyValue(card, art)));
        return arts.ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();

        static KeyValuePair<string, CardArt> ToCipherVitKeyValue(FECard card, FEAlternateArts art)
        {
            return new KeyValuePair<string, CardArt>(art.CipherVitId, new CardArt(card.CardId, art.ArtId, art.DownloadUrl, art.FileName, card.Details));
        }
    }
}