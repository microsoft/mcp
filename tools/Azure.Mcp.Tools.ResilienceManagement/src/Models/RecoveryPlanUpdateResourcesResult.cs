// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanUpdateResourcesResult(
    [property: JsonPropertyName("failedResources")] IReadOnlyList<RecoveryPlanUpdateResourcesFailedResource> FailedResources);
