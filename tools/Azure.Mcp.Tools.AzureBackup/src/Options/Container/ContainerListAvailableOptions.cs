// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Container;

public sealed class ContainerListAvailableOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ContainerListAvailableFilter)]
    public string? Filter { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ContainerStorageAccount)]
    public string? StorageAccount { get; set; }
}
