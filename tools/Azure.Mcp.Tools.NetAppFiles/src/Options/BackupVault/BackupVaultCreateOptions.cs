// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;

public class BackupVaultCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account where the backup vault will be created.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the Azure NetApp Files backup vault to create. Must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.")]
    public required string BackupVault { get; set; }

    [Option(Description = "The Azure region where the backup vault will be created (for example, 'eastus' or 'westus2').")]
    public required string Location { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
