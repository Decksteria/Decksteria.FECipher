namespace Decksteria.FECipher.Decks;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.Core;
using Decksteria.FECipher.Constants;

internal class FEMainDeck : IDecksteriaDeck
{
    public string Name => DeckConstants.MainDeck;

    public string DisplayName => "Main Deck";

    public Task<bool> IsCardCanBeAddedAsync(long cardId, IEnumerable<long> cards, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsDeckValidAsync(IEnumerable<long> cards, CancellationToken cancellationToken = default)
    {
        if (cards.Count() < 49)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
