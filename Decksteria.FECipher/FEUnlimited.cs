namespace Decksteria.FECipher;

using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

internal class FEUnlimited : FEFormat
{
    private readonly IFECardListService cardListService;

    public FEUnlimited(IFECardListService cardListService)
    {
        this.cardListService = cardListService;
    }

    public override string Name => FormatConstants.Unlimited;

    public override string DisplayName => "Unlimited Format";

    public override byte[]? Icon => Properties.Resources.StandardIcon;

    public override string Description => "The format in which all cards are legal.";

    protected override async Task<ReadOnlyDictionary<long, FECard>> GetCardDataAsync(CancellationToken cancellationToken = default!)
    {
        var cardlist = await cardListService.GetCardList();
        return cardlist?.ToDictionary(card => card.CardId).AsReadOnly() ?? new(new Dictionary<long, FECard>());
    }
}
