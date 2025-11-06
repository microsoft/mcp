// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Options.Agents;

public class AgentsGetSdkSampleOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.ProgrammingLanguage)]
    public string? ProgrammingLanguage { get; set; }
}
