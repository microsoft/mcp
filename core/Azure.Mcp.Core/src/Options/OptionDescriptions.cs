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
        "Specifies the Azure subscription to use. Accepts either a subscription ID (GUID) or display name. " +
        "If not specified, the AZURE_SUBSCRIPTION_ID environment variable will be used instead.";

    public const string Tenant =
        "The Microsoft Entra ID tenant ID or name. " +
        "This can be either the GUID identifier or the display name of your Entra ID tenant.";

    public const string AuthMethod =
        "Authentication method to use. " +
        "Options: 'credential' (Azure CLI/managed identity), 'key' (access key), or 'connectionString'.";

    public const string ResourceGroup =
        "The name of the Azure resource group. This is a logical container for Azure resources.";

    public const string Scope =
        "Scope at which the role assignment or definition applies to, " +
        "e.g., /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333, " +
        "/subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup, " +
        "or /subscriptions/0b1f6471-1bf0-4dda-aec3-111122223333/resourceGroups/myGroup/providers/Microsoft.Compute/virtualMachines/myVM.";
}
