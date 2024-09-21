namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

public sealed class LackeyCCGSuperZone
{
    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    // This is the collection of elements that represents the cards in the superzone
    [XmlElement("card")]
    public List<LackeyCCGCard> Cards { get; set; } = [];
}
