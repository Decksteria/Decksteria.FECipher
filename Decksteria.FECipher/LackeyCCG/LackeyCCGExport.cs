namespace Decksteria.FECipher.LackeyCCG;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.LackeyCCG.Models;
using Decksteria.FECipher.Services;

internal sealed class LackeyCCGExport : IDecksteriaExport
{
    public string Name => "LackeyCCG";

    public string FileType => ".dek";

    public string Label => "To LackeyCCG";

    public IFECardListService feCardlistService;

    public LackeyCCGExport(IFECardListService feCardlistService)
    {
        this.feCardlistService = feCardlistService;
    }

    public async Task<MemoryStream> SaveDecklistAsync(Decklist decklist, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        var xmlSerializer = new XmlSerializer(typeof(LackeyCCGDeck));
        xmlSerializer.Serialize(memoryStream, await GetLackeyCCGDeck(decklist));
        return memoryStream;
    }

    private async Task<LackeyCCGDeck> GetLackeyCCGDeck(Decklist decklist)
    {
        var cardlist = await feCardlistService.GetCardList();

        return new()
        {
            Metadata = new LackeyCCGMeta
            {
                Game = "FECipher0"
            },
            Version = "0.8",
            Decks = new List<LackeyCCGSuperZone>
            {
                new() { Name = "Deck", Cards = decklist.Decks[DeckConstants.MainDeck].Select(GetLackeyCard).ToList() },
                new() { Name = "MC", Cards = decklist.Decks[DeckConstants.MainCharacterDeck].Select(GetLackeyCard).ToList() }
            }
        };

        LackeyCCGCard GetLackeyCard(CardArt cardArt)
        {
            var art = cardlist.First(card => card.CardId == cardArt.CardId).AltArts.First(art => art.ArtId == cardArt.ArtId);
            return new LackeyCCGCard
            {
                Name = new LackeyCCGName
                {
                    Id = art.LackeyCCGId,
                    CardName = art.LackeyCCGName
                },
                Set = art.SetCode
            };
        }
    }
}
