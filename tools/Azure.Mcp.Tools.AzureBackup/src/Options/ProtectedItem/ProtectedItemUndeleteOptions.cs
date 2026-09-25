// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public sealed class ProtectedItemUndeleteOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.DatasourceId)]
    public required string DatasourceId { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.Container)]
    public string? Container { get; set; }
}
