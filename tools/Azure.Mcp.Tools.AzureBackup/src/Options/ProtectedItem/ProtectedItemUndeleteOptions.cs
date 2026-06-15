// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public class ProtectedItemUndeleteOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.DatasourceId)]
    public required string DatasourceId { get; set; }

    [Option(AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
