// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options;

public class BaseAzureBackupOptions : ISubscriptionOption
{
    [Option(Description = AzureBackupOptionDefinitions.Vault)]
    public required string Vault { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.VaultType)]
    public string? VaultType { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
