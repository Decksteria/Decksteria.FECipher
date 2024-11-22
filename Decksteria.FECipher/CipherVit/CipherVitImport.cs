namespace Decksteria.FECipher.CipherVit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;

internal sealed class CipherVitImport(IFECardListService feCardlistService) : IDecksteriaImport
{
    public string FileType => ".fe0d";

    public string Label => "CipherVit";

    public IFECardListService feCardlistService = feCardlistService;

    public async Task<Decklist> LoadDecklistAsync(MemoryStream memoryStream, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(memoryStream);
        var lackeyDictionary = await GetCipherVitDictionary();
        var mainDeck = new List<CardArtId>();

        var line = await reader.ReadLineAsync(cancellationToken);
        var standardFormat = currentFormat.Name == FormatConstants.Standard;
        while (line != null)
        {
            var newCard = lackeyDictionary.GetValueOrDefault(line.Trim());
            if (newCard != default)
            {
                mainDeck.Add(newCard.card);

                if (standardFormat && !newCard.standardAllowable)
                {
                    standardFormat = false;
                }
            }

            line = await reader.ReadLineAsync(cancellationToken);
        }

        var file = new Decklist(FECipher.PlugInName, standardFormat ? FormatConstants.Standard : FormatConstants.Unlimited, new Dictionary<string, IEnumerable<CardArtId>>()
        {
            { DeckConstants.MainCharacterDeck, Array.Empty<CardArtId>() },
            { DeckConstants.MainDeck, mainDeck }
        });
        return file;
    }

    private async Task<IReadOnlyDictionary<string, (CardArtId card, bool standardAllowable)>> GetCipherVitDictionary()
    {
        var dictionary = new Dictionary<string, CardArtId>();
        var cardlist = await feCardlistService.GetCardList();
        var arts = cardlist.SelectMany(card => card.AltArts.Select(art => ToCipherVitKeyValue(card, art)));
        return arts.ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();

        static KeyValuePair<string, (CardArtId, bool)> ToCipherVitKeyValue(FECard card, FEAlternateArts art)
        {
            return new(art.CipherVitId, (new CardArtId(card.CardId, art.ArtId), card.AltArts.Any(a => a.SeriesNo >= FormatConstants.StandardSeriesMinimum)));
        }
    }
}