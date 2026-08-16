// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanGroupInfo(
    [property: JsonPropertyName("groupUniqueId")] string GroupUniqueId,
    [property: JsonPropertyName("orderId")] int OrderId,
    [property: JsonPropertyName("description")] string Description);