// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class AgentsGetSdkCodeSampleResult
{
    [JsonPropertyName("codeSampleText")]
    public string? CodeSampleText { get; set; }
}
