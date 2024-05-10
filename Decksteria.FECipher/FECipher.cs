namespace Decksteria.FECipher;

using System.Collections.Generic;
using Decksteria.Core;
using Decksteria.Core.Data;
using Decksteria.FECipher.CipherVit;
using Decksteria.FECipher.LackeyCCG;
using Decksteria.FECipher.Services;
using Microsoft.Extensions.Logging;

public sealed class FECipher : IDecksteriaGame
{
    public const string PlugInName = nameof(FECipher);

    private readonly IDecksteriaFileReader fileReader;

    public string DisplayName => "Fire Emblem Cipher 0";

    public byte[]? Icon => Properties.Resources.GameIcon;

    public string Description => "Fire Emblem Cipher 0";

    public FECipher(IDecksteriaFileReader fileReader, ILoggerFactory loggerFactory)
    {
        var cardlistService = new FECardListService(fileReader, loggerFactory.CreateLogger<FECardListService>());

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
