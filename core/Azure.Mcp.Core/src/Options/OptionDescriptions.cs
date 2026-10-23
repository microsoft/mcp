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
        "The Azure subscription GUID identifier or display name. " +
        "If not specified, the Azure CLI profile default subscription or AZURE_SUBSCRIPTION_ID environment variable will be used.";

    public const string Tenant = "The Microsoft Entra ID tenant GUID identifier or display name.";

    public const string AuthMethod =
        "Authentication method to use. " +
        "Options: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'.";

    public const string ResourceGroup = "The Azure resource group name.";

    public const string Scope =
        "Scope at which the role assignment or definition applies to, " +
        "e.g., /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333, " +
        "/subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup, " +
        "or /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup/providers/Microsoft.Compute/virtualMachines/myVM.";
}
