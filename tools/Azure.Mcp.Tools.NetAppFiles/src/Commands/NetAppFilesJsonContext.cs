// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Models;

namespace Azure.Mcp.Tools.NetAppFiles.Commands;

[JsonSerializable(typeof(AccountCreateCommand.AccountCreateResult))]
[JsonSerializable(typeof(AccountGetCommand.AccountGetResult))]
[JsonSerializable(typeof(AccountUpdateCommand.AccountUpdateResult))]
[JsonSerializable(typeof(BackupPolicyCreateCommand.BackupPolicyCreateResult))]
[JsonSerializable(typeof(BackupPolicyGetCommand.BackupPolicyGetResult))]
[JsonSerializable(typeof(BackupPolicyUpdateCommand.BackupPolicyUpdateResult))]
[JsonSerializable(typeof(BackupVaultCreateCommand.BackupVaultCreateResult))]
[JsonSerializable(typeof(BackupVaultGetCommand.BackupVaultGetResult))]
[JsonSerializable(typeof(BackupVaultUpdateCommand.BackupVaultUpdateResult))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(NetAppFilesAccount))]
[JsonSerializable(typeof(NetAppFilesBackupPolicy))]
[JsonSerializable(typeof(NetAppFilesBackupVault))]
[JsonSerializable(typeof(NetAppFilesVolume))]
[JsonSerializable(typeof(VolumeCreateCommand.VolumeCreateResult))]
[JsonSerializable(typeof(VolumeGetCommand.VolumeGetResult))]
[JsonSerializable(typeof(VolumeUpdateCommand.VolumeUpdateResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class NetAppFilesJsonContext : JsonSerializerContext;
