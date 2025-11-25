// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Agents;

public class AgentsCreateOptions : GlobalOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.Endpoint)]
    public string? Endpoint { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.ModelDeploymentName)]
    public string? ModelDeploymentName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.AgentName)]
    public string? AgentName { get; set; }

    [JsonPropertyName(FoundryOptionDefinitions.SystemInstruction)]
    public string? SystemInstruction { get; set; }
}

