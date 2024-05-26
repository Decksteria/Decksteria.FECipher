namespace Decksteria.FECipher;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Decksteria.FECipher.Constants;
using Decksteria.FECipher.Models;
using Decksteria.FECipher.Services;

internal sealed class FEStandard : FEFormat
{
    private readonly IFECardListService cardListService;

    public FEStandard(IFECardListService cardListService)
    {
        this.cardListService = cardListService;
    }

    public override string Name => FormatConstants.Standard;

    public override string DisplayName => "Standard Format";

    public override byte[]? Icon => Properties.Resources.StandardIcon;

    public override string Description => "The last Official Standard Format of Fire Emblem Cipher, cards from Series 1 to Series 4 are not allowed in this format.";

    protected override async Task<ReadOnlyDictionary<long, FECard>> GetCardDataAsync(CancellationToken cancellationToken = default!)
    {
        var cardlist = await cardListService.GetCardList(cancellationToken);
        cardlist = cardlist?.Where(card => card.AltArts.Any(art => art.SeriesNo > 4));
        return cardlist?.ToDictionary(card => card.CardId).AsReadOnly() ?? new(new Dictionary<long, FECard>());
    }
}
