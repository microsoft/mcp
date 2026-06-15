// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.RecoveryPoint;

public class RecoveryPointGetOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.ProtectedItem)]
    public required string ProtectedItem { get; set; }

    [Option(AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }

    [Option("The recovery point ID.")]
    public string? RecoveryPoint { get; set; }
}
