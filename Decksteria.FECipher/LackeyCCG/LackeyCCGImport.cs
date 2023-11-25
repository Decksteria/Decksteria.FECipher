namespace Decksteria.FECipher.LackeyCCG;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.LackeyCCG.Models;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;
using Decksteria.Service.DecksteriaFile.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

internal sealed class LackeyCCGImport(IFECardListService feCardlistService) : IDecksteriaImport

{
    public string Name => "LackeyCCG";

    public string FileType => ".dek";

    public string Label => "From LackeyCCG";

    public IFECardListService feCardlistService = feCardlistService;

    public async Task<Decklist> LoadDecklistAsync(MemoryStream memoryStream, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(memoryStream);
        var textFile = await reader.ReadToEndAsync(cancellationToken);

        // Change Format
        var currentFormatName = currentFormat.Name;
        var xmlSerializer = new XmlSerializer(typeof(LackeyCCGDeck));

        if (xmlSerializer.Deserialize(memoryStream) is not LackeyCCGDeck lackeyDeck)
        {
            return new Decklist(FECipher.PlugInName, currentFormatName, new Dictionary<string, IEnumerable<CardArtId>>());
        }

        var mcZone = lackeyDeck.Decks.First(deck => deck.Name == "MC");
        var mainZone = lackeyDeck.Decks.First(deck => deck.Name == "Deck");
        var lackeyDictionary = await GetLackeyCCGDictionary();

        var mainCharDeck = mcZone.Cards.Select(card => lackeyDictionary.GetValueOrDefault(card.Name.Id)).Where(card => card != null).Select(card => card!);
        var mainDeck = mainZone.Cards.Select(card => lackeyDictionary.GetValueOrDefault(card.Name.Id)).Where(card => card != null).Select(card => card!);

        var file = new Decklist(FECipher.PlugInName, currentFormatName, new Dictionary<string, IEnumerable<CardArtId>>()
        {
            { DeckConstants.MainCharacterDeck, mainCharDeck },
            { DeckConstants.MainDeck, mainDeck }
        });
        return file;
    }

    private async Task<IReadOnlyDictionary<string, CardArtId>> GetLackeyCCGDictionary()
    {
        var dictionary = new Dictionary<string, CardArtId>();
        var cardlist = await feCardlistService.GetCardList();
        var arts = cardlist.SelectMany(card => card.AltArts.Select(art => ToLackeyCCGKeyValue(card, art)));
        return arts.ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();

        static KeyValuePair<string, CardArtId> ToLackeyCCGKeyValue(FECard card, FEAlternateArts art)
        {
            return new KeyValuePair<string, CardArtId>(art.LackeyCCGId, new CardArtId(card.CardId, art.ArtId));
        }
    }
}