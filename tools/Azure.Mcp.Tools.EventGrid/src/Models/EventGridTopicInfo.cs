// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Models;

// Lightweight projection of EventGridTopicData with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record EventGridTopicInfo(
    string Name,
    string? Location,
    string? Endpoint,
    string? ProvisioningState,
    string? PublicNetworkAccess,
    string? InputSchema);
