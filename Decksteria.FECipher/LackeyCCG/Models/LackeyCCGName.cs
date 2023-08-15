namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Xml.Serialization;

internal class LackeyCCGName
{
    // This is the attribute that represents the id of the name
    [XmlAttribute("id")]
    public string Id { get; set; } = string.Empty;

    // This is the text content that represents the value of the name
    [XmlText]
    public string CardName { get; set; } = string.Empty;
}