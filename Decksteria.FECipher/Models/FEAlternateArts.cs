namespace Decksteria.FECipher.Models;

using System.Text.Json.Serialization;
using Decksteria.Core;

public sealed class FEAlternateArts : IDecksteriaCardArt
{
    [JsonPropertyName("ID")]
    [JsonPropertyOrder(0)]
    public long ArtId { get; init; }

    [JsonPropertyName("CardCode")]
    [JsonPropertyOrder(1)]
    public string CardCode { get; init; } = string.Empty;

    [JsonPropertyName("SetCode")]
    [JsonPropertyOrder(1)]
    public string SetCode { get; init; } = string.Empty;

    [JsonPropertyName("Rarity")]
    [JsonPropertyOrder(2)]
    public string Rarity { get; init; } = string.Empty;

    [JsonPropertyName("FileName")]
    [JsonPropertyOrder(3)]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("DownloadURL")]
    [JsonPropertyOrder(4)]
    public string DownloadUrl { get; init; } = string.Empty;

    [JsonPropertyName("LackeyCCGID")]
    [JsonPropertyOrder(5)]
    public string LackeyCCGId { get; init; } = string.Empty;

    [JsonPropertyName("LackeyCCGName")]
    [JsonPropertyOrder(6)]
    public string LackeyCCGName { get; init; } = string.Empty;

    [JsonPropertyName("CipherVitId")]
    [JsonPropertyOrder(7)]
    public string CipherVitId { get; init; } = string.Empty;

    [JsonPropertyName("SeriesNo")]
    [JsonPropertyOrder(8)]
    public int SeriesNo { get; init; }
}