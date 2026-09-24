// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;

public class BackupVaultGetOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account that contains the backup vault.")]
    public required string Account { get; set; }

    [Option(Description = "The name of the Azure NetApp Files backup vault to retrieve.")]
    public required string BackupVault { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
