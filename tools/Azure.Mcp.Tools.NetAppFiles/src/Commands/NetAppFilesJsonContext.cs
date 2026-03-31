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

[JsonSerializable(typeof(AccountCreateCommand.AccountCreateCommandResult))]
[JsonSerializable(typeof(AccountGetCommand.AccountGetCommandResult))]
[JsonSerializable(typeof(BackupCreateCommand.BackupCreateCommandResult))]
[JsonSerializable(typeof(BackupPolicyCreateCommand.BackupPolicyCreateCommandResult))]
[JsonSerializable(typeof(BackupGetCommand.BackupGetCommandResult))]
[JsonSerializable(typeof(BackupPolicyGetCommand.BackupPolicyGetCommandResult))]
[JsonSerializable(typeof(BackupVaultGetCommand.BackupVaultGetCommandResult))]
[JsonSerializable(typeof(BackupVaultCreateCommand.BackupVaultCreateCommandResult))]
[JsonSerializable(typeof(PoolCreateCommand.PoolCreateCommandResult))]
[JsonSerializable(typeof(PoolGetCommand.PoolGetCommandResult))]
[JsonSerializable(typeof(ReplicationStatusGetCommand.ReplicationStatusGetCommandResult))]
[JsonSerializable(typeof(SnapshotCreateCommand.SnapshotCreateCommandResult))]
[JsonSerializable(typeof(SnapshotGetCommand.SnapshotGetCommandResult))]
[JsonSerializable(typeof(SnapshotPolicyCreateCommand.SnapshotPolicyCreateCommandResult))]
[JsonSerializable(typeof(SnapshotPolicyGetCommand.SnapshotPolicyGetCommandResult))]
[JsonSerializable(typeof(VolumeGetCommand.VolumeGetCommandResult))]
[JsonSerializable(typeof(VolumeGroupGetCommand.VolumeGroupGetCommandResult))]
[JsonSerializable(typeof(VolumeCreateCommand.VolumeCreateCommandResult))]
[JsonSerializable(typeof(VolumeGroupCreateCommand.VolumeGroupCreateCommandResult))]
[JsonSerializable(typeof(NetAppAccountCreateResult))]
[JsonSerializable(typeof(NetAppAccountInfo))]
[JsonSerializable(typeof(BackupCreateResult))]
[JsonSerializable(typeof(BackupPolicyCreateResult))]
[JsonSerializable(typeof(BackupInfo))]
[JsonSerializable(typeof(BackupPolicyInfo))]
[JsonSerializable(typeof(BackupVaultInfo))]
[JsonSerializable(typeof(BackupVaultCreateResult))]
[JsonSerializable(typeof(CapacityPoolCreateResult))]
[JsonSerializable(typeof(CapacityPoolInfo))]
[JsonSerializable(typeof(NetAppVolumeInfo))]
[JsonSerializable(typeof(NetAppVolumeCreateResult))]
[JsonSerializable(typeof(ReplicationStatusInfo))]
[JsonSerializable(typeof(SnapshotCreateResult))]
[JsonSerializable(typeof(SnapshotInfo))]
[JsonSerializable(typeof(SnapshotPolicyInfo))]
[JsonSerializable(typeof(VolumeGroupInfo))]
[JsonSerializable(typeof(BackupPolicyCreateOrUpdateContent))]
[JsonSerializable(typeof(BackupCreateOrUpdateContent))]
[JsonSerializable(typeof(NetAppAccountCreateOrUpdateContent))]
[JsonSerializable(typeof(NetAppAccountData))]
[JsonSerializable(typeof(BackupData))]
[JsonSerializable(typeof(BackupPolicyData))]
[JsonSerializable(typeof(BackupVaultData))]
[JsonSerializable(typeof(BackupVaultCreateOrUpdateContent))]
[JsonSerializable(typeof(CapacityPoolCreateOrUpdateContent))]
[JsonSerializable(typeof(CapacityPoolData))]
[JsonSerializable(typeof(NetAppVolumeData))]
[JsonSerializable(typeof(NetAppVolumeCreateOrUpdateContent))]
[JsonSerializable(typeof(ReplicationStatusData))]
[JsonSerializable(typeof(SnapshotPolicyCreateOrUpdateContent))]
[JsonSerializable(typeof(SnapshotCreateOrUpdateContent))]
[JsonSerializable(typeof(SnapshotData))]
[JsonSerializable(typeof(SnapshotPolicyCreateResult))]
[JsonSerializable(typeof(SnapshotPolicyData))]
[JsonSerializable(typeof(VolumeGroupCreateResult))]
[JsonSerializable(typeof(VolumeGroupData))]
[JsonSerializable(typeof(VolumeGroupCreateOrUpdateContent))]
[JsonSerializable(typeof(JsonElement))][JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class NetAppFilesJsonContext : JsonSerializerContext
{
}
