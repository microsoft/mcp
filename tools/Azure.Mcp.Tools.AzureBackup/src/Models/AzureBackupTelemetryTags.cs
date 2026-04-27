// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public static class AzureBackupTelemetryTags
{
    private static string AddPrefix(string tagName) => $"azurebackup/{tagName}";

    public static readonly string VaultType = AddPrefix("VaultType");
    public static readonly string WorkloadType = AddPrefix("WorkloadType");
    public static readonly string DatasourceType = AddPrefix("DatasourceType");
    public static readonly string OperationScope = AddPrefix("OperationScope");
}
