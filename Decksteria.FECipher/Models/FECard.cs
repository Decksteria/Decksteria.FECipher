namespace Decksteria.FECipher.Models;

using Decksteria.Core;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FECard : IDecksteriaCard
{
    [JsonPropertyName("ID")]
    [JsonPropertyOrder(0)]
    public long CardId { get; init; }

    [JsonIgnore]
    public IEnumerable<IDecksteriaCardArt> Arts => AltArts;

    [JsonIgnore]
    public string Details
    {
        get
        {
            string fullDetails = Name;
            fullDetails += string.Format("\nClass: {0}/Cost: {1}", CardClass, Cost);
            if (ClassChangeCost != null)
            {
                fullDetails += "(" + ClassChangeCost + ")";
            }

            fullDetails += string.Format("\nColors: {0}\nTypes: {1}\nAttack: {2}/Support: {3}/Range: {4}-{5}", string.Join('/', Colors), string.Join('/', Types), Attack, Support, MinRange, MaxRange);
            fullDetails += "\n---\nSkills:\n" + Skill;

            if (SupportSkill != null)
            {
                fullDetails += "\n---\nSupport:\n" + SupportSkill;
            }

            return fullDetails;
        }
    }

    [JsonPropertyName("Character")]
    [JsonPropertyOrder(1)]
    public string CharacterName { get; init; } = string.Empty;

    [JsonPropertyName("Title")]
    [JsonPropertyOrder(2)]
    public string CardTitle { get; init; } = string.Empty;

    [JsonPropertyName("Color")]
    [JsonPropertyOrder(3)]
    public IEnumerable<string> Colors { get; init; } = Array.Empty<string>();

    [JsonPropertyName("Cost")]
    [JsonPropertyOrder(4)]
    public string Cost { get; init; } = string.Empty;

    [JsonPropertyName("ClassChangeCost")]
    [JsonPropertyOrder(5)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClassChangeCost { get; init; } = string.Empty;

    [JsonPropertyName("Class")]
    [JsonPropertyOrder(6)]
    public string CardClass { get; init; } = string.Empty;

    [JsonPropertyName("Type")]
    [JsonPropertyOrder(7)]
    public IEnumerable<string> Types { get; init; } = Array.Empty<string>();

    [JsonPropertyName("MinRange")]
    [JsonPropertyOrder(8)]
    public int MinRange { get; init; }

    [JsonPropertyName("MaxRange")]
    [JsonPropertyOrder(9)]
    public int MaxRange { get; init; }

    [JsonPropertyName("Attack")]
    [JsonPropertyOrder(10)]
    public string Attack { get; init; } = string.Empty;

    [JsonPropertyName("Support")]
    [JsonPropertyOrder(11)]
    public string Support { get; init; } = string.Empty;

    [JsonPropertyName("Skill")]
    [JsonPropertyOrder(12)]
    public string Skill { get; init; } = string.Empty;

    [JsonPropertyName("SupportSkill")]
    [JsonPropertyOrder(13)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SupportSkill { get; init; }

    [JsonPropertyName("AlternateArts")]
    [JsonPropertyOrder(15)]
    public IEnumerable<FEAlternateArts> AltArts { get; init; } = Array.Empty<FEAlternateArts>();

    [JsonIgnore]
    public string Name
    {
        get { return CharacterName + ": " + CardTitle; }
    }
}