// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Datasource type names, aliases, and ARM resource types supported by protection.</summary>
public enum AzureBackupDatasourceType
{
    [JsonStringEnumMemberName("VM")] Vm,
    [JsonStringEnumMemberName("IaaSVM")] IaasVm,
    [JsonStringEnumMemberName("AzureVM")] AzureVm,
    [JsonStringEnumMemberName("AzureIaaSVM")] AzureIaasVm,
    [JsonStringEnumMemberName("VirtualMachine")] VirtualMachine,
    [JsonStringEnumMemberName("IaaSVMContainer")] IaasVmContainer,
    [JsonStringEnumMemberName("SQL")] Sql,
    [JsonStringEnumMemberName("SQLDatabase")] SqlDatabase,
    [JsonStringEnumMemberName("SQLDB")] SqlDb,
    [JsonStringEnumMemberName("MSSQL")] MsSql,
    [JsonStringEnumMemberName("AzureSQL")] AzureSql,
    [JsonStringEnumMemberName("SAPHANA")] SapHana,
    [JsonStringEnumMemberName("SAPHANADatabase")] SapHanaDatabase,
    [JsonStringEnumMemberName("SAPHANADB")] SapHanaDb,
    [JsonStringEnumMemberName("HANA")] Hana,
    [JsonStringEnumMemberName("SAPASE")] SapAse,
    [JsonStringEnumMemberName("ASE")] Ase,
    [JsonStringEnumMemberName("Sybase")] Sybase,
    [JsonStringEnumMemberName("AzureFileShare")] AzureFileShare,
    [JsonStringEnumMemberName("FileShare")] FileShare,
    [JsonStringEnumMemberName("AFS")] Afs,
    [JsonStringEnumMemberName("AzureDisk")] AzureDisk,
    [JsonStringEnumMemberName("Disk")] Disk,
    [JsonStringEnumMemberName("Microsoft.Compute/disks")] MicrosoftComputeDisks,
    [JsonStringEnumMemberName("AzureBlob")] AzureBlob,
    [JsonStringEnumMemberName("Blob")] Blob,
    [JsonStringEnumMemberName("Microsoft.Storage/storageAccounts/blobServices")] MicrosoftStorageStorageAccountsBlobServices,
    [JsonStringEnumMemberName("Microsoft.Storage/storageAccounts")] MicrosoftStorageStorageAccounts,
    [JsonStringEnumMemberName("AKS")] Aks,
    [JsonStringEnumMemberName("Kubernetes")] Kubernetes,
    [JsonStringEnumMemberName("Microsoft.ContainerService/managedClusters")] MicrosoftContainerServiceManagedClusters,
    [JsonStringEnumMemberName("ElasticSAN")] ElasticSan,
    [JsonStringEnumMemberName("ESAN")] Esan,
    [JsonStringEnumMemberName("Microsoft.ElasticSan/elasticSans/volumeGroups")] MicrosoftElasticSanElasticSansVolumeGroups,
    [JsonStringEnumMemberName("PostgreSQLFlexible")] PostgreSqlFlexible,
    [JsonStringEnumMemberName("PGFlex")] PgFlex,
    [JsonStringEnumMemberName("PostgreSQL")] PostgreSql,
    [JsonStringEnumMemberName("Microsoft.DBforPostgreSQL/flexibleServers")] MicrosoftDbForPostgreSqlFlexibleServers,
    [JsonStringEnumMemberName("AzureDataLakeStorage")] AzureDataLakeStorage,
    [JsonStringEnumMemberName("ADLS")] Adls,
    [JsonStringEnumMemberName("DataLake")] DataLake,
    [JsonStringEnumMemberName("DataLakeStorage")] DataLakeStorage,
    [JsonStringEnumMemberName("CosmosDB")] CosmosDb,
    [JsonStringEnumMemberName("Cosmos")] Cosmos,
    [JsonStringEnumMemberName("Microsoft.DocumentDB/databaseAccounts")] MicrosoftDocumentDbDatabaseAccounts
}
