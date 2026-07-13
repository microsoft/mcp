// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public sealed record SvmPeerCommandInfo(
    [property: JsonPropertyName("svmPeeringCommand")] string? SvmPeeringCommand);