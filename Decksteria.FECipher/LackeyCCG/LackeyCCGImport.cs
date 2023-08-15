namespace Decksteria.FECipher.LackeyCCG;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.LackeyCCG.Models;
using Decksteria.FECipher.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

internal class LackeyCCGImport : IDecksteriaImport

{
    public string Name => "LackeyCCG";

    public string FileType => ".dek";

    public string Label => "From LackeyCCG";

    public IFECardListService feCardlistService;

    public LackeyCCGImport(IFECardListService feCardlistService)
    {
        this.feCardlistService = feCardlistService;
    }

    public async Task<Decklist> LoadDecklistAsync(MemoryStream memoryStream, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(memoryStream);
        var textFile = await reader.ReadToEndAsync();

        // Change Format
        var currentFormatName = currentFormat.Name;
        var xmlSerializer = new XmlSerializer(typeof(LackeyCCGDeck));
        var lackeyDeck = xmlSerializer.Deserialize(memoryStream) as LackeyCCGDeck;

        if (lackeyDeck == null)
        {
            return new Decklist("FECipher", currentFormatName, new Dictionary<string, IEnumerable<CardArt>>());
        }

        var mcZone = lackeyDeck.Decks.First(deck => deck.Name == "MC");
        var mainZone = lackeyDeck.Decks.First(deck => deck.Name == "Deck");
        var lackeyDictionary = await GetLackeyCCGDictionary();

        var mainCharDeck = mcZone.Cards.Select(card => lackeyDictionary.GetValueOrDefault(card.Name.Id)).Where(card => card != null).Select(card => card!);
        var mainDeck = mainZone.Cards.Select(card => lackeyDictionary.GetValueOrDefault(card.Name.Id)).Where(card => card != null).Select(card => card!);

        var file = new Decklist("FECipher", currentFormatName, new Dictionary<string, IEnumerable<CardArt>>()
        {
            { DeckConstants.MainCharacterDeck, mainCharDeck },
            { DeckConstants.MainDeck, mainDeck }
        });
        return file;
    }

    private async Task<IReadOnlyDictionary<string, CardArt>> GetLackeyCCGDictionary()
    {
        var dictionary = new Dictionary<string, CardArt>();
        var cardlist = await feCardlistService.GetCardList();
        var arts = cardlist.SelectMany(card => card.AltArts.Select(art => new KeyValuePair<string, CardArt>(art.LackeyCCGId, new CardArt(card.CardId, art.ArtId, art.DownloadUrl, art.FileName, card.Details))));
        return arts.ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();
    }
}