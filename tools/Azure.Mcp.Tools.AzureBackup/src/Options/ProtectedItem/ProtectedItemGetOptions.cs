// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public sealed class ProtectedItemGetOptions : BaseProtectedItemOptions
{
    [Option(Description = AzureBackupOptionDefinitions.ProtectedItem)]
    public string? ProtectedItem { get; set; }
}
