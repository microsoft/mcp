// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options;

public class BaseProtectedItemOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
