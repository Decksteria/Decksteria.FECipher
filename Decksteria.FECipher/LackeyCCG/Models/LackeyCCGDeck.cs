﻿namespace Decksteria.FECipher.LackeyCCG.Models;

using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("deck")]
internal sealed class LackeyCCGDeck
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "0.8";

    [XmlElement("meta")]
    public LackeyCCGMeta Metadata { get; set; } = new LackeyCCGMeta();

    [XmlElement("superzone")]
    public List<LackeyCCGSuperZone> Decks { get; set; } =
    [
        new() { Name = "Deck", Cards = [] },
        new() { Name = "MC", Cards = [] }
    ];
}
