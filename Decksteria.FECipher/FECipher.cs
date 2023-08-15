namespace Decksteria.FECipher;

using Decksteria.Core;
using Decksteria.Core.Data;
using Decksteria.FECipher.LackeyCCG;
using System.Collections.Generic;

public sealed class FECipher : IDecksteriaGame
{
    public string Name => "FECipher";

    public string DisplayName => "Fire Emblem Cipher 0";

    public byte[]? Icon => Properties.Resources.GameIcon;

    public string Description => "Fire Emblem Cipher 0";

    public FECipher(IDecksteriaFileReader fileReader)
    {
        var cardlistService = new Services.FECardListService(fileReader);

        Formats = new IDecksteriaFormat[]
        {
            new FEStandard(cardlistService),
            new FEUnlimited(cardlistService)
        };

        Importers = new IDecksteriaImport[]
        {
            new LackeyCCGImport(cardlistService)
        };
        Exporters = Array.Empty<IDecksteriaExport>();
    }

    public IEnumerable<IDecksteriaFormat> Formats { get; }

    public IEnumerable<IDecksteriaImport> Importers { get; }

    public IEnumerable<IDecksteriaExport> Exporters { get; }
}
