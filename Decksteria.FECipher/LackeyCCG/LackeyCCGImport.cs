namespace Decksteria.FECipher.LackeyCCG;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.LackeyCCG.Models;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;

internal sealed class LackeyCCGImport(IFECardListService feCardlistService) : IDecksteriaImport
{
    public string FileType => ".dek";

    public string Label => "LackeyCCG";

    public IFECardListService feCardlistService = feCardlistService;

    public async Task<Decklist> LoadDecklistAsync(MemoryStream memoryStream, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(memoryStream);

        // Change Format
        var currentFormatName = currentFormat.Name;
        var xmlSerializer = new XmlSerializer(typeof(LackeyCCGDeck));
        if (xmlSerializer.Deserialize(memoryStream) is not LackeyCCGDeck lackeyDeck)
        {
            throw new InvalidDataException("File is not a valid LackeyCCG deck file.");
        }

        var mcZone = lackeyDeck.Decks.FirstOrDefault(deck => deck.Name == "MC");
        var mainZone = lackeyDeck.Decks.FirstOrDefault(deck => deck.Name == "Deck");

        var usedLackeyCCGIds = lackeyDeck.Decks.SelectMany(decks => decks.Cards.Select(card => card.Name.Id));
        var lackeyDictionary = await GetLackeyCCGDictionary(usedLackeyCCGIds);
        var mainCharDeck = mcZone?.Cards.Select(card => GetCardArtId(card.Name.Id)).Where(card => card != null).Select(card => card!) ?? [];
        var mainDeck = mainZone?.Cards.Select(card => GetCardArtId(card.Name.Id)).Where(card => card != null).Select(card => card!) ?? [];

        var file = new Decklist(FECipher.PlugInName, currentFormatName, new Dictionary<string, IEnumerable<CardArtId>>()
        {
            { DeckConstants.MainCharacterDeck, mainCharDeck },
            { DeckConstants.MainDeck, mainDeck }
        });
        return file;

        CardArtId? GetCardArtId(string lackeyCCGId) => lackeyDictionary.GetValueOrDefault(lackeyCCGId);
    }

    private async Task<IReadOnlyDictionary<string, CardArtId>> GetLackeyCCGDictionary(IEnumerable<string> lackeyIds)
    {
        var dictionary = new Dictionary<string, CardArtId>();
        var cardlist = await feCardlistService.GetCardList();
        var arts = cardlist.SelectMany(card => card.AltArts.Where(art => lackeyIds.Contains(art.LackeyCCGId)).Select(art => (card, art)));
        return arts.GroupBy(feCardArt => feCardArt.art.LackeyCCGId)
            .ToDictionary(kv => kv.Key, kv => ToCardArtId(kv.FirstOrDefault())).AsReadOnly();

        static CardArtId ToCardArtId((FECard card, FEAlternateArts art) feCardArt)
        {
            return new(feCardArt.card.CardId, feCardArt.art.ArtId);
        }
    }
}