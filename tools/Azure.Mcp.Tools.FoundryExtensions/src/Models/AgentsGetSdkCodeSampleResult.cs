// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FoundryExtensions.Models;

public class AgentsGetSdkCodeSampleResult
{
    [JsonPropertyName("codeSampleText")]
    public string? CodeSampleText { get; set; }
}
