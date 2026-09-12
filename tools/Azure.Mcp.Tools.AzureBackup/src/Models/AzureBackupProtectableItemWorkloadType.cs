// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Workload type names and aliases supported by protectable-item listing.</summary>
public enum AzureBackupProtectableItemWorkloadType
{
    [JsonStringEnumMemberName("SQL")] Sql,
    [JsonStringEnumMemberName("SQLDatabase")] SqlDatabase,
    [JsonStringEnumMemberName("SQLInstance")] SqlInstance,
    [JsonStringEnumMemberName("SAPHana")] SapHana,
    [JsonStringEnumMemberName("SAPHanaDatabase")] SapHanaDatabase,
    [JsonStringEnumMemberName("SAPHanaSystem")] SapHanaSystem,
    [JsonStringEnumMemberName("SAPHanaDBInstance")] SapHanaDbInstance,
    [JsonStringEnumMemberName("SAPHanaDBI")] SapHanaDbi,
    [JsonStringEnumMemberName("VM")] Vm,
    [JsonStringEnumMemberName("IaaSVM")] IaasVm,
    [JsonStringEnumMemberName("VirtualMachine")] VirtualMachine,
    [JsonStringEnumMemberName("FileShare")] FileShare,
    [JsonStringEnumMemberName("AzureFileShare")] AzureFileShare,
    [JsonStringEnumMemberName("AFS")] Afs,
    [JsonStringEnumMemberName("SAPAse")] SapAse,
    [JsonStringEnumMemberName("SAPAseDatabase")] SapAseDatabase,
    [JsonStringEnumMemberName("ASE")] Ase,
    [JsonStringEnumMemberName("Sybase")] Sybase
}
