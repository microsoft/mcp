// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Agents;

public class AgentsGetSdkSampleOptions : GlobalOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.ProgrammingLanguage)]
    public string? ProgrammingLanguage { get; set; }
}
