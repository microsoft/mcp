// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options;

public class BaseProtectedItemOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.ProtectedItem)]
    public string? ProtectedItem { get; set; }

    [Option(AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
