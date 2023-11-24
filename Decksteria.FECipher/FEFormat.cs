namespace Decksteria.FECipher;

using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Decks;
using Decksteria.FECipher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal abstract class FEFormat : IDecksteriaFormat
{
    private static readonly string[] Colors = ["All", "Red", "Blue", "Yellow", "Purple", "Green", "Black", "White", "Brown", "Colorless"];

    private readonly ReadOnlyDictionary<string, IDecksteriaDeck> feDecks;

    private ReadOnlyDictionary<long, FECard>? formatCards;

    public FEFormat()
    {
        feDecks = new Dictionary<string, IDecksteriaDeck>()
        {
            { DeckConstants.MainCharacterDeck, new FEMainCharacter(GetFECardAsync) },
            { DeckConstants.MainDeck, new FEMainDeck()}
        }.AsReadOnly();
    }

    protected virtual Lazy<ReadOnlyDictionary<string, int>> SpecialCardLimits { get; private set; } = new(() => LoadSpecialCardLimits().AsReadOnly());

    public abstract string Name { get; }

    public abstract string DisplayName { get; }

    public abstract byte[]? Icon { get; }

    public abstract string Description { get; }

    protected abstract Task<ReadOnlyDictionary<long, FECard>> GetCardDataAsync(CancellationToken cancellationToken = default!);

    public IEnumerable<IDecksteriaDeck> Decks => feDecks.Select(kv => kv.Value);

    public IEnumerable<SearchField> SearchFields { get; } = new SearchField[12]
    {
        new(SearchFieldConstants.CharacterField),
        new(SearchFieldConstants.TitleField),
        new(SearchFieldConstants.ColorField, Colors),
        new(SearchFieldConstants.ColorField, Colors),
        new(SearchFieldConstants.CostField, 1),
        new(SearchFieldConstants.ClassChangeCostField, 1),
        new(SearchFieldConstants.ClassField),
        new(SearchFieldConstants.TypeField),
        new(SearchFieldConstants.RangeField, 0, 3),
        new(SearchFieldConstants.AttackField, 3),
        new(SearchFieldConstants.SupportField, 3),
        new(SearchFieldConstants.SeriesField, 0, 12),
    };

    public async Task<bool> CheckCardCountAsync(long cardId, IReadOnlyDictionary<string, IEnumerable<long>> decklist, CancellationToken cancellationToken = default)
    {
        formatCards ??= await GetCardDataAsync(cancellationToken);

        // Don't add if card can't be found
        var card = formatCards.GetValueOrDefault(cardId);
        if (card == null)
        {
            return false;
        }

        // Cards that allow Infinite Copies
        var maximum = SpecialCardLimits.Value.GetValueOrDefault(card.Name);
        if (maximum == -1)
        {
            return true;
        }

        // Maximum of 4 copies per name and not ID
        return decklist.SelectMany(deck => deck.Value).Count(cId => formatCards.GetValueOrDefault(cId)?.Name == card.Name) < 4;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Code is not more readable when converted to a conditional expression.")]
    public async Task<int> CompareCardsAsync(long cardId1, long cardId2, CancellationToken cancellationToken = default)
    {
        if (cardId1 == cardId2)
        {
            return 0;
        }

        // Get FECards
        cancellationToken.ThrowIfCancellationRequested();
        var feCardX = await GetFECardAsync(cardId1);
        var feCardY = await GetFECardAsync(cardId2);

        if (feCardX.CharacterName != feCardY.CharacterName)
        {
            // First Comparison: Character Name
            return string.Compare(feCardX.CharacterName, feCardY.CharacterName);
        }
        else if (feCardX.Cost != feCardY.Cost)
        {
            // Third Comparison: Deployment Cost
            if (feCardX.Cost == "X")
            {
                return 1;
            }

            if (feCardY.Cost == "X")
            {
                return -1;
            }

            return string.Compare(feCardX.Cost, feCardY.Cost);
        }
        else if (feCardX.ClassChangeCost != feCardY.ClassChangeCost)
        {
            // Third Comparison: Class Change Cost
            return string.Compare(feCardX.ClassChangeCost, feCardY.ClassChangeCost);
        }
        else if (feCardX.Attack != feCardY.Attack)
        {
            // Fourth Comparison: Attack
            return string.Compare(feCardX.Attack, feCardY.Attack);
        }
        else if (feCardX.Support != feCardY.Support)
        {
            // Fourth Comparison: Attack
            return string.Compare(feCardX.Support, feCardY.Support);
        }
        else if (feCardX.CardTitle != feCardY.CardTitle)
        {
            // Fifth Comparison: Character Title
            return string.Compare(feCardX.CardTitle, feCardY.CardTitle);
        }
        else
        {
            // Sixth Comparison: Card ID
            return cardId1.CompareTo(cardId2);
        }
    }

    public async Task<IDecksteriaCard> GetCardAsync(long cardId, CancellationToken cancellationToken = default)
    {
        return await GetFECardAsync(cardId);
    }

    public async Task<IEnumerable<IDecksteriaCard>> GetCardsAsync(IEnumerable<SearchFieldFilter>? filters = null, CancellationToken cancellationToken = default)
    {
        formatCards ??= await GetCardDataAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cardlist = formatCards.Select(kv => kv.Value);

        if (filters == null)
        {
            return cardlist;
        }

        var filterFuncs = filters.Select(GetFilterFunction);

        // Get all cards that match the conditions of all filter functions
        return cardlist.Where(card => filterFuncs.All(func => func(card)));

        Func<FECard, bool> GetFilterFunction(SearchFieldFilter filter) => filter.SearchField.FieldName switch
        {
            SearchFieldConstants.CharacterField => (card) => filter.MatchesFilter(card.CharacterName),
            SearchFieldConstants.TitleField => (card) => filter.MatchesFilter(card.CardTitle),
            SearchFieldConstants.ColorField => (card) => card.Colors.Any(filter.MatchesFilter),
            SearchFieldConstants.CostField => (card) => filter.MatchesFilter(card.Cost),
            SearchFieldConstants.ClassChangeCostField => (card) => filter.MatchesFilter(card.ClassChangeCost),
            SearchFieldConstants.ClassField => (card) => filter.MatchesFilter(card.CharacterName),
            SearchFieldConstants.TypeField => (card) => card.Types.Any(filter.MatchesFilter),
            SearchFieldConstants.RangeField => (card) => RangeMatchesFilter(card, filter),
            SearchFieldConstants.AttackField => (card) => filter.MatchesFilter(card.Attack),
            SearchFieldConstants.SupportField => (card) => filter.MatchesFilter(card.Support),
            SearchFieldConstants.SeriesField => (card) => card.AltArts.Select(art => art.SeriesNo).Any(filter.MatchesFilter),
            _ => throw new ArgumentException("Invalid Search Field.", filter.SearchField.FieldName)
        };
    }

    public async Task<Dictionary<string, int>> GetDeckStatsAsync(IReadOnlyDictionary<string, IEnumerable<long>> decklist, bool isDetailed, CancellationToken cancellationToken = default)
    {
        formatCards ??= await GetCardDataAsync(cancellationToken);

        // Get Main Character Card
        var mainCharId = decklist.GetValueOrDefault(DeckConstants.MainCharacterDeck)?.FirstOrDefault();
        var mainCharacter = mainCharId.HasValue ? formatCards.GetValueOrDefault(mainCharId.Value) : null;

        // Get Counts
        var mainCharacterNames = 0;
        var range0 = 0;
        var range1 = 0;
        var range2 = 0;
        var range3 = 0;
        var support0 = 0;
        var support10 = 0;
        var support20 = 0;
        var support30 = 0;

        var allCards = decklist.SelectMany(kv => kv.Value.Select(id => formatCards.GetValueOrDefault(id)));

        // Count Cards
        foreach (var card in allCards)
        {
            if (card == null)
            {
                continue;
            }

            mainCharacterNames += card!.Name == mainCharacter?.Name ? 1 : 0;

            if (!isDetailed)
            {
                continue;
            }

            // Range Counting
            if (card.MinRange == 0 && card.MaxRange == 0)
            {
                range0++;
            }

            if (1 >= card.MinRange && 1 <= card.MaxRange)
            {
                range1++;
            }

            if (2 >= card.MinRange && 2 <= card.MaxRange)
            {
                range2++;
            }

            if (3 >= card.MinRange && 3 <= card.MaxRange)
            {
                range3++;
            }

            // Support Counting
            switch (card.Support)
            {
                case "0":
                    support0++;
                    break;
                case "10":
                    support10++;
                    break;
                case "20":
                    support20++;
                    break;
                case "30":
                    support30++;
                    break;
            }
        }

        var returnDictionary = new Dictionary<string, int>()
        {
            {"Main Characters", mainCharacterNames },
        };

        if (isDetailed)
        {
            returnDictionary.Add("Range 0", range0);
            returnDictionary.Add("Range 1", range1);
            returnDictionary.Add("Range 2", range2);
            returnDictionary.Add("Range 3", range3);
            returnDictionary.Add("Support 0", support0);
            returnDictionary.Add("Support 10", support10);
            returnDictionary.Add("Support 20", support20);
            returnDictionary.Add("Support 30", support30);
        }

        return returnDictionary;
    }

    public Task<IDecksteriaDeck> GetDefaultDeckAsync(long cardId, CancellationToken cancellationToken = default) => Task.FromResult(feDecks[DeckConstants.MainDeck]);

    public async Task<bool> IsDecklistLegal(IReadOnlyDictionary<string, IEnumerable<long>> decklist, CancellationToken cancellationToken = default)
    {
        var mainCharacterDeckLegal = await feDecks[DeckConstants.MainCharacterDeck].IsDeckValidAsync(decklist[DeckConstants.MainCharacterDeck], cancellationToken);
        return mainCharacterDeckLegal && decklist[DeckConstants.MainDeck].Any();
    }

    public virtual void Uninitialize()
    {
        formatCards = null;
        SpecialCardLimits = new(() => LoadSpecialCardLimits().AsReadOnly());
    }

    protected static Dictionary<string, int> LoadSpecialCardLimits()
    {
        var specialCardLimits = new Dictionary<string, int>()
        {
            { "Anna: Secret Seller", -1 },
            { "Anna: Peddler of Many Mysteries", -1 },
            { "Necrodragon: Resurrected Wyrm", -1 },
            { "Risen: Grotesque Soldier", -1 },
            { "Legion: Masked Assassin", -1 },
            { "Faceless: Heartless Monster", -1 },
            { "Witch: Sacrifice Fated for Puppetdom", -1 },
            { "Risen: Defiled Soldier", -1 },
            { "Bael: Giant Man-Eating Spider", -1 },
            { "Gorgon: Snake-Haired Demon", -1 },
        };

        return specialCardLimits;
    }

    private async Task<FECard> GetFECardAsync(long cardId)
    {
        formatCards ??= await GetCardDataAsync();
        return formatCards[cardId];
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "Range matching only uses listed Comparison Types.")]
    private static bool RangeMatchesFilter(FECard card, SearchFieldFilter filter)
    {
        if (filter.Value == null)
        {
            return true;
        }

        var value = Convert.ToInt32(filter.Value);

        switch (filter.Comparison)
        {
            case ComparisonType.Equals:
                return value >= card.MinRange && value <= card.MaxRange;
            case ComparisonType.NotEquals:
                return value <= card.MinRange || value >= card.MaxRange;
            case ComparisonType.GreaterThan:
                return value < card.MaxRange;
            case ComparisonType.GreaterThanOrEqual:
                return value <= card.MaxRange;
            case ComparisonType.LessThan:
                return value > card.MinRange;
            case ComparisonType.LessThanOrEqual:
                return value >= card.MinRange;
        }

        return false;
    }
}