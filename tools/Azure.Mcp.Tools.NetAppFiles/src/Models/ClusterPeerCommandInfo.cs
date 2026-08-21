// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public sealed record ClusterPeerCommandInfo(
    [property: JsonPropertyName("clusterPeeringCommand")] string? ClusterPeeringCommand,
    [property: JsonPropertyName("passphrase")] string? Passphrase);