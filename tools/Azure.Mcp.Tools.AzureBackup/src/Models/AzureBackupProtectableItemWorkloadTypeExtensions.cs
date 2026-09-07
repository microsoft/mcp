// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

internal static class AzureBackupProtectableItemWorkloadTypeExtensions
{
    internal static string ToValue(this AzureBackupProtectableItemWorkloadType workloadType) => workloadType switch
    {
        AzureBackupProtectableItemWorkloadType.Sql => "SQL",
        AzureBackupProtectableItemWorkloadType.SqlDatabase => "SQLDatabase",
        AzureBackupProtectableItemWorkloadType.SqlInstance => "SQLInstance",
        AzureBackupProtectableItemWorkloadType.SapHana => "SAPHana",
        AzureBackupProtectableItemWorkloadType.SapHanaDatabase => "SAPHanaDatabase",
        AzureBackupProtectableItemWorkloadType.SapHanaSystem => "SAPHanaSystem",
        AzureBackupProtectableItemWorkloadType.SapHanaDbInstance => "SAPHanaDBInstance",
        AzureBackupProtectableItemWorkloadType.SapHanaDbi => "SAPHanaDBI",
        AzureBackupProtectableItemWorkloadType.Vm => "VM",
        AzureBackupProtectableItemWorkloadType.IaasVm => "IaaSVM",
        AzureBackupProtectableItemWorkloadType.VirtualMachine => "VirtualMachine",
        AzureBackupProtectableItemWorkloadType.FileShare => "FileShare",
        AzureBackupProtectableItemWorkloadType.AzureFileShare => "AzureFileShare",
        AzureBackupProtectableItemWorkloadType.Afs => "AFS",
        AzureBackupProtectableItemWorkloadType.SapAse => "SAPAse",
        AzureBackupProtectableItemWorkloadType.SapAseDatabase => "SAPAseDatabase",
        AzureBackupProtectableItemWorkloadType.Ase => "ASE",
        AzureBackupProtectableItemWorkloadType.Sybase => "Sybase",
        _ => throw new ArgumentOutOfRangeException(nameof(workloadType), workloadType, null)
    };
}
