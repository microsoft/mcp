// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

internal static class AzureBackupDatasourceTypeExtensions
{
    internal static string ToValue(this AzureBackupDatasourceType datasourceType) => datasourceType switch
    {
        AzureBackupDatasourceType.Vm => "VM",
        AzureBackupDatasourceType.IaasVm => "IaaSVM",
        AzureBackupDatasourceType.AzureVm => "AzureVM",
        AzureBackupDatasourceType.AzureIaasVm => "AzureIaaSVM",
        AzureBackupDatasourceType.VirtualMachine => "VirtualMachine",
        AzureBackupDatasourceType.IaasVmContainer => "IaaSVMContainer",
        AzureBackupDatasourceType.Sql => "SQL",
        AzureBackupDatasourceType.SqlDatabase => "SQLDatabase",
        AzureBackupDatasourceType.SqlDb => "SQLDB",
        AzureBackupDatasourceType.MsSql => "MSSQL",
        AzureBackupDatasourceType.AzureSql => "AzureSQL",
        AzureBackupDatasourceType.SapHana => "SAPHANA",
        AzureBackupDatasourceType.SapHanaDatabase => "SAPHANADatabase",
        AzureBackupDatasourceType.SapHanaDb => "SAPHANADB",
        AzureBackupDatasourceType.Hana => "HANA",
        AzureBackupDatasourceType.SapAse => "SAPASE",
        AzureBackupDatasourceType.Ase => "ASE",
        AzureBackupDatasourceType.Sybase => "Sybase",
        AzureBackupDatasourceType.AzureFileShare => "AzureFileShare",
        AzureBackupDatasourceType.FileShare => "FileShare",
        AzureBackupDatasourceType.Afs => "AFS",
        AzureBackupDatasourceType.AzureDisk => "AzureDisk",
        AzureBackupDatasourceType.Disk => "Disk",
        AzureBackupDatasourceType.MicrosoftComputeDisks => "Microsoft.Compute/disks",
        AzureBackupDatasourceType.AzureBlob => "AzureBlob",
        AzureBackupDatasourceType.Blob => "Blob",
        AzureBackupDatasourceType.MicrosoftStorageStorageAccountsBlobServices => "Microsoft.Storage/storageAccounts/blobServices",
        AzureBackupDatasourceType.MicrosoftStorageStorageAccounts => "Microsoft.Storage/storageAccounts",
        AzureBackupDatasourceType.Aks => "AKS",
        AzureBackupDatasourceType.Kubernetes => "Kubernetes",
        AzureBackupDatasourceType.MicrosoftContainerServiceManagedClusters => "Microsoft.ContainerService/managedClusters",
        AzureBackupDatasourceType.ElasticSan => "ElasticSAN",
        AzureBackupDatasourceType.Esan => "ESAN",
        AzureBackupDatasourceType.MicrosoftElasticSanElasticSansVolumeGroups => "Microsoft.ElasticSan/elasticSans/volumeGroups",
        AzureBackupDatasourceType.PostgreSqlFlexible => "PostgreSQLFlexible",
        AzureBackupDatasourceType.PgFlex => "PGFlex",
        AzureBackupDatasourceType.PostgreSql => "PostgreSQL",
        AzureBackupDatasourceType.MicrosoftDbForPostgreSqlFlexibleServers => "Microsoft.DBforPostgreSQL/flexibleServers",
        AzureBackupDatasourceType.AzureDataLakeStorage => "AzureDataLakeStorage",
        AzureBackupDatasourceType.Adls => "ADLS",
        AzureBackupDatasourceType.DataLake => "DataLake",
        AzureBackupDatasourceType.DataLakeStorage => "DataLakeStorage",
        AzureBackupDatasourceType.CosmosDb => "CosmosDB",
        AzureBackupDatasourceType.Cosmos => "Cosmos",
        AzureBackupDatasourceType.MicrosoftDocumentDbDatabaseAccounts => "Microsoft.DocumentDB/databaseAccounts",
        _ => throw new ArgumentOutOfRangeException(nameof(datasourceType), datasourceType, null)
    };

    internal static bool IsRsv(this AzureBackupDatasourceType datasourceType) => datasourceType is
        AzureBackupDatasourceType.Vm or
        AzureBackupDatasourceType.IaasVm or
        AzureBackupDatasourceType.AzureVm or
        AzureBackupDatasourceType.AzureIaasVm or
        AzureBackupDatasourceType.VirtualMachine or
        AzureBackupDatasourceType.IaasVmContainer or
        AzureBackupDatasourceType.Sql or
        AzureBackupDatasourceType.SqlDatabase or
        AzureBackupDatasourceType.SqlDb or
        AzureBackupDatasourceType.MsSql or
        AzureBackupDatasourceType.AzureSql or
        AzureBackupDatasourceType.SapHana or
        AzureBackupDatasourceType.SapHanaDatabase or
        AzureBackupDatasourceType.SapHanaDb or
        AzureBackupDatasourceType.Hana or
        AzureBackupDatasourceType.SapAse or
        AzureBackupDatasourceType.Ase or
        AzureBackupDatasourceType.Sybase or
        AzureBackupDatasourceType.AzureFileShare or
        AzureBackupDatasourceType.FileShare or
        AzureBackupDatasourceType.Afs;

    internal static bool IsDpp(this AzureBackupDatasourceType datasourceType) => !datasourceType.IsRsv();
}
