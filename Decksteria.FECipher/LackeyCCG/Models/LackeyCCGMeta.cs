namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Xml.Serialization;

public sealed class LackeyCCGMeta
{
    [XmlElement("game")]
    public string Game { get; set; } = "FECipher0";
}
