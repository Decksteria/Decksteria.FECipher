namespace Decksteria.FECipher;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.Core;
using Decksteria.Core.Models;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Decks;
using Decksteria.FECipher.Models;

internal abstract partial class FEFormat : IDecksteriaFormat
{
    private readonly ReadOnlyDictionary<string, uint> Colors;

    private readonly ReadOnlyDictionary<string, IDecksteriaDeck> feDecks;

    private readonly uint colourlessValue;

    private ReadOnlyDictionary<long, FECard>? formatCards;

    public FEFormat()
    {
        feDecks = new Dictionary<string, IDecksteriaDeck>()
        {
            { DeckConstants.MainCharacterDeck, new FEMainCharacter(GetFECardAsync) },
            { DeckConstants.MainDeck, new FEMainDeck()}
        }.AsReadOnly();

        colourlessValue = ((uint) Enum.GetValues<Colour>().Max()) * 2;
        Colors = new Dictionary<string, uint>()
        {
            {nameof(Colour.Red), (uint) Colour.Red},
            {nameof(Colour.Blue), (uint) Colour.Blue},
            {nameof(Colour.Yellow), (uint) Colour.Yellow},
            {nameof(Colour.Purple), (uint) Colour.Purple},
            {nameof(Colour.Green), (uint) Colour.Green},
            {nameof(Colour.Black), (uint) Colour.Black},
            {nameof(Colour.White), (uint) Colour.White},
            {nameof(Colour.Brown), (uint) Colour.Brown},
            {nameof(Colour.Colourless), colourlessValue }
        }.AsReadOnly();

        SearchFields = new SearchField[]
        {
            new(SearchFieldConstants.CharacterField),
            new(SearchFieldConstants.TitleField),
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
    }

    protected virtual Lazy<ReadOnlyDictionary<string, int>> SpecialCardLimits { get; private set; } = new(() => LoadSpecialCardLimits().AsReadOnly());

    public abstract string Name { get; }

    public abstract string DisplayName { get; }

    public abstract byte[]? Icon { get; }

    public abstract string Description { get; }

    protected abstract Task<ReadOnlyDictionary<long, FECard>> GetCardDataAsync(CancellationToken cancellationToken = default!);

    public IEnumerable<IDecksteriaDeck> Decks => feDecks.Select(kv => kv.Value);

    public IEnumerable<SearchField> SearchFields { get; }

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

    public async Task<IQueryable<IDecksteriaCard>> GetCardsAsync(IEnumerable<ISearchFieldFilter>? filters = null, CancellationToken cancellationToken = default)
    {
        formatCards ??= await GetCardDataAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cardlist = formatCards.Select(kv => kv.Value).AsQueryable();

        if (filters == null)
        {
            return cardlist.AsQueryable();
        }

        var filterFuncs = filters.Select(GetFilterFunction);

        // Get all cards that match the conditions of all filter functions
        return cardlist.Where(card => filterFuncs.All(func => func(card))).AsQueryable();

        Func<FECard, bool> GetFilterFunction(ISearchFieldFilter filter) => filter.SearchField.FieldName switch
        {
            SearchFieldConstants.CharacterField => (card) => filter.MatchesFilter(card.CharacterName),
            SearchFieldConstants.TitleField => (card) => filter.MatchesFilter(card.CardTitle),
            SearchFieldConstants.ColorField => (card) => ColoursMatchOverride(card.ColoursValue, filter),
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

        bool ColoursMatchOverride(Colour cardColor, ISearchFieldFilter filter)
        {
            var colourValue = (uint) cardColor;
            if (cardColor != Colour.Colourless)
            {
                return filter.MatchesFilter(colourValue);
            }

            if (filter.Value is not uint uintValue || (uintValue & colourlessValue) == 0)
            {
                return false;
            }

            return filter.Comparison switch
            {
                ComparisonType.Equals or ComparisonType.Contains or ComparisonType.GreaterThanOrEqual => true,
                ComparisonType.NotEquals or ComparisonType.NotContains or ComparisonType.LessThan => false,
                _ => false
            };
        }
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

            mainCharacterNames += card!.CharacterName == mainCharacter?.CharacterName ? 1 : 0;

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


            if (!isDetailed)
            {
                continue;
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

        var returnDictionary = new Dictionary<string, int>
        {
            { "Main Characters", mainCharacterNames },
            { "Range 0", range0 },
            { "Range 1", range1 },
            { "Range 2", range2 },
            { "Range 3", range3 }
        };

        if (isDetailed)
        {
            returnDictionary.Add("Support 0", support0);
            returnDictionary.Add("Support 10", support10);
            returnDictionary.Add("Support 20", support20);
            returnDictionary.Add("Support 30", support30);
        }

        return returnDictionary;
    }

    public Task<IDecksteriaDeck> GetDefaultDeckAsync(long cardId, CancellationToken cancellationToken = default) => Task.FromResult(feDecks[DeckConstants.MainDeck]);

    public async Task<bool> IsDecklistLegalAsync(IReadOnlyDictionary<string, IEnumerable<long>> decklist, CancellationToken cancellationToken = default)
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

    private static bool RangeMatchesFilter(FECard card, ISearchFieldFilter filter)
    {
        if (filter.Value == null)
        {
            return true;
        }

        var value = Convert.ToInt32(filter.Value);

        return filter.Comparison switch
        {
            ComparisonType.Equals => value >= card.MinRange && value <= card.MaxRange,
            ComparisonType.NotEquals => value <= card.MinRange || value >= card.MaxRange,
            ComparisonType.GreaterThan => value < card.MaxRange,
            ComparisonType.GreaterThanOrEqual => value <= card.MaxRange,
            ComparisonType.LessThan => value > card.MinRange,
            ComparisonType.LessThanOrEqual => value >= card.MinRange,
            _ => false,
        };
    }
}