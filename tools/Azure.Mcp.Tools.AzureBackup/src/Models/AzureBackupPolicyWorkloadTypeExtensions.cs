// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

internal static class AzureBackupPolicyWorkloadTypeExtensions
{
    internal static string ToValue(this AzureBackupPolicyWorkloadType workloadType) => workloadType switch
    {
        AzureBackupPolicyWorkloadType.Vm => "VM",
        AzureBackupPolicyWorkloadType.AzureVm => "AzureVM",
        AzureBackupPolicyWorkloadType.IaasVm => "IaaSVM",
        AzureBackupPolicyWorkloadType.AzureIaasVm => "AzureIaaSVM",
        AzureBackupPolicyWorkloadType.VirtualMachine => "VirtualMachine",
        AzureBackupPolicyWorkloadType.IaasVmContainer => "IaaSVMContainer",
        AzureBackupPolicyWorkloadType.Sql => "SQL",
        AzureBackupPolicyWorkloadType.SqlDatabase => "SQLDatabase",
        AzureBackupPolicyWorkloadType.SqlDb => "SQLDB",
        AzureBackupPolicyWorkloadType.MsSql => "MSSQL",
        AzureBackupPolicyWorkloadType.AzureSql => "AzureSQL",
        AzureBackupPolicyWorkloadType.SapHana => "SAPHANA",
        AzureBackupPolicyWorkloadType.SapHanaDatabase => "SAPHANADatabase",
        AzureBackupPolicyWorkloadType.SapHanaDb => "SAPHANADB",
        AzureBackupPolicyWorkloadType.Hana => "HANA",
        AzureBackupPolicyWorkloadType.SapAse => "SAPASE",
        AzureBackupPolicyWorkloadType.Ase => "ASE",
        AzureBackupPolicyWorkloadType.Sybase => "Sybase",
        AzureBackupPolicyWorkloadType.AzureFileShare => "AzureFileShare",
        AzureBackupPolicyWorkloadType.FileShare => "FileShare",
        AzureBackupPolicyWorkloadType.Afs => "AFS",
        AzureBackupPolicyWorkloadType.AzureDisk => "AzureDisk",
        AzureBackupPolicyWorkloadType.Disk => "Disk",
        AzureBackupPolicyWorkloadType.ElasticSan => "ElasticSAN",
        AzureBackupPolicyWorkloadType.Esan => "ESAN",
        AzureBackupPolicyWorkloadType.PostgreSqlFlexible => "PostgreSQLFlexible",
        AzureBackupPolicyWorkloadType.Postgres => "Postgres",
        AzureBackupPolicyWorkloadType.PgFlex => "PGFlex",
        AzureBackupPolicyWorkloadType.CosmosDb => "CosmosDB",
        AzureBackupPolicyWorkloadType.Cosmos => "Cosmos",
        AzureBackupPolicyWorkloadType.Aks => "AKS",
        AzureBackupPolicyWorkloadType.Kubernetes => "Kubernetes",
        AzureBackupPolicyWorkloadType.KubernetesCluster => "KubernetesCluster",
        AzureBackupPolicyWorkloadType.AzureBlob => "AzureBlob",
        AzureBackupPolicyWorkloadType.Blob => "Blob",
        AzureBackupPolicyWorkloadType.Adls => "ADLS",
        AzureBackupPolicyWorkloadType.AzureDataLakeStorage => "AzureDataLakeStorage",
        AzureBackupPolicyWorkloadType.DataLake => "DataLake",
        AzureBackupPolicyWorkloadType.DataLakeStorage => "DataLakeStorage",
        _ => throw new ArgumentOutOfRangeException(nameof(workloadType), workloadType, null)
    };
}
