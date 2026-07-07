// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.RecoveryPoint;

public sealed class RecoveryPointGetOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ProtectedItem)]
    public required string ProtectedItem { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }

    [Option(Description = "The recovery point ID.")]
    public string? RecoveryPoint { get; set; }
}
