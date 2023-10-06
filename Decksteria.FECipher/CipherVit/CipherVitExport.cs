namespace Decksteria.FECipher.CipherVit;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CipherVitExport : IDecksteriaExport
{
    private IFECardListService feCardlistService;

    public string Name => "CipherVit";

    public string FileType => ".fe0d";

    public string Label => "From CipherVit";

    public CipherVitExport(IFECardListService feCardlistService)
    {
        this.feCardlistService = feCardlistService;
    }

    public async Task<MemoryStream> SaveDecklistAsync(Decklist decklist, IDecksteriaFormat currentFormat, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        var cardlist = (await feCardlistService.GetCardList()).ToDictionary(kv => kv.CardId);
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

        await streamWriter.FlushAsync();

        return memoryStream;
    }
}
