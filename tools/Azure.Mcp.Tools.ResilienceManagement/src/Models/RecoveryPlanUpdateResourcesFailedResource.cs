// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanUpdateResourcesFailedResource(
    [property: JsonPropertyName("recoveryResourceId")] string RecoveryResourceId,
    [property: JsonPropertyName("recoveryResourceUniqueId")] string RecoveryResourceUniqueId,
    [property: JsonPropertyName("azureResourceId")] string? AzureResourceId,
    [property: JsonPropertyName("error")] RecoveryPlanUpdateResourcesError? Error);