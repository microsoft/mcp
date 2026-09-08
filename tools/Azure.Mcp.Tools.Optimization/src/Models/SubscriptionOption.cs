// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// A subscription candidate returned when a subscription name matches more than one subscription,
/// so the caller can re-run using the exact subscription id.
/// </summary>
public sealed record SubscriptionOption(
    string? SubscriptionId,
    string? Name,
    string? TenantId);
