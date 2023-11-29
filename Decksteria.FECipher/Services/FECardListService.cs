namespace Decksteria.FECipher.Services;

using Decksteria.Core.Data;
using Decksteria.FECipher.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

internal sealed class FECardListService : IFECardListService
{
    private readonly Lazy<Task<IEnumerable<FECard>>> cardList;

    public FECardListService(IDecksteriaFileReader fileReader)
    {
        cardList = new(GetCardList);

        async Task<IEnumerable<FECard>> GetCardList()
        {
            var jsonText = await fileReader.ReadTextFileAsync("cardlist.json", FECipher.PlugInName, "https://raw.githubusercontent.com/Decksteria/Decksteria.FECipher/main/Decksteria.FECipher/cardlist.json");
            var cardlist = JsonSerializer.Deserialize<IEnumerable<FECard>>(jsonText) ?? Array.Empty<FECard>();
            return cardlist ?? Array.Empty<FECard>();
        }
    }

    public Task<IEnumerable<FECard>> GetCardList()
    {
        return cardList.Value;
    }
}
