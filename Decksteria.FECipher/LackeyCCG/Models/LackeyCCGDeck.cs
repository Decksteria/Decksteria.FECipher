﻿namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Xml.Linq;
using System.Xml.Serialization;

[XmlRoot("deck")]
internal class LackeyCCGDeck
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "0.8";

    [XmlElement("meta")]
    public LackeyCCGMeta Metadata { get; set; } = new LackeyCCGMeta();

    [XmlElement("superzone")]
    public List<LackeyCCGSuperZone> Decks { get; set; } = new()
    {
        new() { Name = "Deck", Cards = new() },
        new() { Name = "MC", Cards = new() }
    };
}
