namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Xml.Serialization;

internal class LackeyCCGMeta
{
    [XmlElement("game")]
    public string Game { get; set; } = "FECipher0";
}
