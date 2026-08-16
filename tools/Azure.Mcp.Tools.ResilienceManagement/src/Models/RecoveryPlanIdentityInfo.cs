// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanIdentityInfo(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("userAssignedIdentityResourceIds")] IReadOnlyList<string> UserAssignedIdentityResourceIds);