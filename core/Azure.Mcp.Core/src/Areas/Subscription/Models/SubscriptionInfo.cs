// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Subscription.Models;

/// <summary>
/// Represents an Azure subscription with default subscription indicator.
/// </summary>
/// <param name="SubscriptionId">The subscription ID.</param>
/// <param name="DisplayName">The display name of the subscription.</param>
/// <param name="State">The subscription state (e.g., Enabled, Disabled).</param>
/// <param name="TenantId">The tenant ID associated with the subscription.</param>
/// <param name="IsDefault">Whether this subscription is the default as configured via 'az account set' in the Azure CLI profile, or via the AZURE_SUBSCRIPTION_ID environment variable.</param>
internal record SubscriptionInfo(
    string SubscriptionId,
    string DisplayName,
    string? State,
    string? TenantId,
    bool IsDefault);
