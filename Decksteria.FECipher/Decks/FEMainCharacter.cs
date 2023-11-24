namespace Decksteria.FECipher.Decks;

using Decksteria.Core;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class FEMainCharacter(Func<long, Task<FECard>> getCardsFuncAsync) : IDecksteriaDeck
{
    private readonly Func<long, Task<FECard>> getCardsFuncAsync = getCardsFuncAsync;

    public string Name => DeckConstants.MainCharacterDeck;

    public string DisplayName => "Main Character";

    public async Task<bool> IsCardCanBeAddedAsync(long cardId, IEnumerable<long> cards, CancellationToken cancellationToken = default)
    {
        if (cards.Any())
        {
            return false;
        }

        var feCard = await getCardsFuncAsync(cardId);
        return feCard != null && feCard.Cost == "1";
    }

    public async Task<bool> IsDeckValidAsync(IEnumerable<long> cards, CancellationToken cancellationToken = default)
    {
        if (cards.Count() != 1)
        {
            return false;
        }

        var feCard = await getCardsFuncAsync(cards.First());
        return feCard == null || feCard.Cost == "1";
    }
}
