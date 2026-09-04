// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Container;

public sealed class ContainerRefreshOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ContainerRefreshFilter)]
    public string? Filter { get; set; }
}
