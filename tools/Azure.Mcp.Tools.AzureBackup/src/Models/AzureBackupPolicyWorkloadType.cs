// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Workload type names and aliases supported by backup policy creation.</summary>
public enum AzureBackupPolicyWorkloadType
{
    [JsonStringEnumMemberName("VM")] Vm,
    [JsonStringEnumMemberName("AzureVM")] AzureVm,
    [JsonStringEnumMemberName("IaaSVM")] IaasVm,
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
    [JsonStringEnumMemberName("ElasticSAN")] ElasticSan,
    [JsonStringEnumMemberName("ESAN")] Esan,
    [JsonStringEnumMemberName("PostgreSQLFlexible")] PostgreSqlFlexible,
    [JsonStringEnumMemberName("Postgres")] Postgres,
    [JsonStringEnumMemberName("PGFlex")] PgFlex,
    [JsonStringEnumMemberName("CosmosDB")] CosmosDb,
    [JsonStringEnumMemberName("Cosmos")] Cosmos,
    [JsonStringEnumMemberName("AKS")] Aks,
    [JsonStringEnumMemberName("Kubernetes")] Kubernetes,
    [JsonStringEnumMemberName("KubernetesCluster")] KubernetesCluster,
    [JsonStringEnumMemberName("AzureBlob")] AzureBlob,
    [JsonStringEnumMemberName("Blob")] Blob,
    [JsonStringEnumMemberName("ADLS")] Adls,
    [JsonStringEnumMemberName("AzureDataLakeStorage")] AzureDataLakeStorage,
    [JsonStringEnumMemberName("DataLake")] DataLake,
    [JsonStringEnumMemberName("DataLakeStorage")] DataLakeStorage
}
