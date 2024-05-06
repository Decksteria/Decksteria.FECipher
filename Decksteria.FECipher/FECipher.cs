namespace Decksteria.FECipher;

using Decksteria.Core;
using Decksteria.Core.Data;
using Decksteria.FECipher.CipherVit;
using Decksteria.FECipher.LackeyCCG;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class FECipher : IDecksteriaGame
{
    public const string PlugInName = "FECipher";

    private readonly IDecksteriaFileReader fileReader;

    public string Name => PlugInName;

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
            new LackeyCCGImport(cardlistService),
            new CipherVitImport(cardlistService)
        };
        Exporters = new IDecksteriaExport[]
        {
            new LackeyCCGExport(cardlistService),
            new CipherVitExport(cardlistService)
        };
        this.fileReader = fileReader;
    }

    public IEnumerable<IDecksteriaFormat> Formats { get; }

    public IEnumerable<IDecksteriaImport> Importers { get; }

    public IEnumerable<IDecksteriaExport> Exporters { get; }
}
