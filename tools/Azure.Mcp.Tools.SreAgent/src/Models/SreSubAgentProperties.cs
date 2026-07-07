// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class SreSubAgentProperties
{
    // Server-side ExtendedAgentView exposes `handoffDescription` as the sub-agent's
    // description on the agent canvas. There is no plain `description` field on
    // sub-agents — sending one would be silently dropped by the data plane.
    public string? HandoffDescription { get; set; }

    public string? Instructions { get; set; }

    public string? Model { get; set; }

    public List<string>? Tools { get; set; }

    public List<string>? Handoffs { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
