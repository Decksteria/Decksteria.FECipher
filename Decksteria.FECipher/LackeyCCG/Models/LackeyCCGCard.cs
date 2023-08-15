namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Xml.Serialization;

internal class LackeyCCGCard
{
    // This is the element that represents the name of the card
    [XmlElement("name")]
    public LackeyCCGName Name { get; set; } = new();

    // This is the element that represents the set of the card
    [XmlElement("set")]
    public string Set { get; set; } = string.Empty;
}
