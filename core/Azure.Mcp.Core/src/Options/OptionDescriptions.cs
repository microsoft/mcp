// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Options;

/// <summary>
/// Description constants for global options shared across all Azure MCP commands.
/// Used with <see cref="Microsoft.Mcp.Core.Options.OptionAttribute"/> on options classes.
/// </summary>
public static class OptionDescriptions
{
    public const string Subscription =
        "Azure subscription ID or name. Defaults to the Azure CLI or AZURE_SUBSCRIPTION_ID subscription.";

    public const string Tenant = "Microsoft Entra tenant ID or name.";

    public const string AuthMethod =
        "Authentication method: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'.";

    public const string ResourceGroup = "Resource group name.";

    public const string Scope =
        "Scope at which the role assignment or definition applies to, " +
        "e.g., /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333, " +
        "/subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup, " +
        "or /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup/providers/Microsoft.Compute/virtualMachines/myVM.";
}
