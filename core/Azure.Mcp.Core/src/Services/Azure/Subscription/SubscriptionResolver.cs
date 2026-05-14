// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

/// <summary>
/// Default implementation that resolves subscriptions from command-line arguments,
/// Azure CLI profile, and the AZURE_SUBSCRIPTION_ID environment variable.
/// </summary>
public sealed class SubscriptionResolver : ISubscriptionResolver
{
    public string? ResolveSubscription(string? subscription)
    {
        subscription = subscription?.Trim('"', '\'');
        subscription = CommandHelper.GetSubscription(subscription);
        return subscription;
    }

    public bool HasSubscriptionAvailable(CommandResult commandResult) =>
        CommandHelper.HasSubscriptionAvailable(commandResult);
}
