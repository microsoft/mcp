// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;

public class ProtectableItemListOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.WorkloadType)]
    public string? WorkloadType { get; set; }

    [Option(AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
