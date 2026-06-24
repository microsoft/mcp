// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;

public sealed class ProtectableItemListOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.WorkloadType)]
    public string? WorkloadType { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
