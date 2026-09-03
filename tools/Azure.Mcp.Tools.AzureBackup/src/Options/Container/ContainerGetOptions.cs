// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Container;

public sealed class ContainerGetOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ContainerGetContainer)]
    public string? Container { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ContainerGetStorageAccount)]
    public string? StorageAccount { get; set; }
}
