// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Container;

public sealed class ContainerGetOptions : ISubscriptionOption
{
    [Option(Description = AzureBackupOptionDefinitions.Vault)]
    public required string Vault { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ContainerGetContainer)]
    public string? Container { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ContainerGetStorageAccount)]
    public string? StorageAccount { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
