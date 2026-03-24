// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.Backup;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Commands.ReplicationStatus;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Commands;

[JsonSerializable(typeof(AccountGetCommand.AccountGetCommandResult))]
[JsonSerializable(typeof(BackupGetCommand.BackupGetCommandResult))]
[JsonSerializable(typeof(BackupPolicyGetCommand.BackupPolicyGetCommandResult))]
[JsonSerializable(typeof(BackupVaultGetCommand.BackupVaultGetCommandResult))]
[JsonSerializable(typeof(PoolGetCommand.PoolGetCommandResult))]
[JsonSerializable(typeof(ReplicationStatusGetCommand.ReplicationStatusGetCommandResult))]
[JsonSerializable(typeof(SnapshotGetCommand.SnapshotGetCommandResult))]
[JsonSerializable(typeof(SnapshotPolicyGetCommand.SnapshotPolicyGetCommandResult))]
[JsonSerializable(typeof(VolumeGetCommand.VolumeGetCommandResult))]
[JsonSerializable(typeof(VolumeGroupGetCommand.VolumeGroupGetCommandResult))]
[JsonSerializable(typeof(NetAppAccountInfo))]
[JsonSerializable(typeof(BackupInfo))]
[JsonSerializable(typeof(BackupPolicyInfo))]
[JsonSerializable(typeof(BackupVaultInfo))]
[JsonSerializable(typeof(CapacityPoolInfo))]
[JsonSerializable(typeof(NetAppVolumeInfo))]
[JsonSerializable(typeof(ReplicationStatusInfo))]
[JsonSerializable(typeof(SnapshotInfo))]
[JsonSerializable(typeof(SnapshotPolicyInfo))]
[JsonSerializable(typeof(VolumeGroupInfo))]
[JsonSerializable(typeof(NetAppAccountData))]
[JsonSerializable(typeof(BackupData))]
[JsonSerializable(typeof(BackupPolicyData))]
[JsonSerializable(typeof(BackupVaultData))]
[JsonSerializable(typeof(CapacityPoolData))]
[JsonSerializable(typeof(NetAppVolumeData))]
[JsonSerializable(typeof(ReplicationStatusData))]
[JsonSerializable(typeof(SnapshotData))]
[JsonSerializable(typeof(SnapshotPolicyData))]
[JsonSerializable(typeof(VolumeGroupData))]
[JsonSerializable(typeof(JsonElement))][JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class NetAppFilesJsonContext : JsonSerializerContext
{
}
