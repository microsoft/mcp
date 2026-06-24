// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public sealed class VaultGetOptions : ISubscriptionOption
{
    [Option(Description = AzureBackupOptionDefinitions.Vault)]
    public string? Vault { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.VaultType)]
    public string? VaultType { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
