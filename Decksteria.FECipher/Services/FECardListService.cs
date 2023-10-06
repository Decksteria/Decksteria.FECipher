namespace Decksteria.FECipher.Services;

using Decksteria.Core.Data;
using Decksteria.FECipher.Models;
using System.Text.Json;

internal sealed class FECardListService : IFECardListService
{
    private Lazy<Task<IEnumerable<FECard>>> cardList;

    public FECardListService(IDecksteriaFileReader fileReader)
    {
        cardList = new(async () =>
        {
            var jsonText = await fileReader.ReadTextFileAsync("cardlist.json", "");
            var cardlist = JsonSerializer.Deserialize<IEnumerable<FECard>>(jsonText) ?? Array.Empty<FECard>();
            return cardlist ?? Array.Empty<FECard>();
        });
    }

    public Task<IEnumerable<FECard>> GetCardList()
    {
        return cardList.Value;
    }
}
