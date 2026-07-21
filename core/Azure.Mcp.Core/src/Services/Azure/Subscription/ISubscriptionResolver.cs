// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

/// <summary>
/// Resolves subscription values from command-line arguments, environment variables,
/// and Azure CLI profile, providing a testable seam for subscription resolution.
/// </summary>
public interface ISubscriptionResolver
{
    /// <summary>
    /// Resolves the subscription from the provided value, falling back to Azure CLI profile
    /// or AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    string? ResolveSubscription(string? subscription);

    /// <summary>
    /// Gets the default subscription from the Azure CLI profile (~/.azure/azureProfile.json),
    /// falling back to the AZURE_SUBSCRIPTION_ID environment variable.
    /// The CLI profile read is cached for the lifetime of the process to avoid redundant file I/O.
    /// </summary>
    string? GetDefaultSubscriptionId();

    /// <summary>
    /// Checks if a subscription is available from the command option, Azure CLI profile,
    /// or AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    bool HasSubscriptionAvailable(CommandResult commandResult);
}
