// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Speech.Models;

public record NBestResult
{
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("lexical")]
    public string? Lexical { get; set; }

    [JsonPropertyName("itn")]
    public string? ITN { get; set; }

    [JsonPropertyName("maskedITN")]
    public string? MaskedITN { get; set; }

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("words")]
    public List<WordResult>? Words { get; set; }
}
