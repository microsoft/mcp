// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Commands.Backup;
using Azure.Mcp.Tools.AzureBackup.Commands.DisasterRecovery;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Commands.Job;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Commands.Security;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

[JsonSerializable(typeof(VaultGetCommand.VaultGetCommandResult))]
[JsonSerializable(typeof(VaultCreateCommand.VaultCreateCommandResult))]
[JsonSerializable(typeof(VaultUpdateCommand.VaultUpdateCommandResult))]
[JsonSerializable(typeof(PolicyGetCommand.PolicyGetCommandResult))]
[JsonSerializable(typeof(PolicyCreateCommand.PolicyCreateCommandResult))]
[JsonSerializable(typeof(PolicyUpdateCommand.PolicyUpdateCommandResult))]
[JsonSerializable(typeof(ProtectedItemGetCommand.ProtectedItemGetCommandResult))]
[JsonSerializable(typeof(ProtectedItemProtectCommand.ProtectedItemProtectCommandResult))]
[JsonSerializable(typeof(ProtectedItemUndeleteCommand.ProtectedItemUndeleteCommandResult))]
[JsonSerializable(typeof(ProtectableItemListCommand.ProtectableItemListCommandResult))]
[JsonSerializable(typeof(BackupStatusCommand.BackupStatusCommandResult))]
[JsonSerializable(typeof(JobGetCommand.JobGetCommandResult))]
[JsonSerializable(typeof(RecoveryPointGetCommand.RecoveryPointGetCommandResult))]
[JsonSerializable(typeof(GovernanceFindUnprotectedCommand.GovernanceFindUnprotectedCommandResult))]
[JsonSerializable(typeof(GovernanceImmutabilityCommand.GovernanceImmutabilityCommandResult))]
[JsonSerializable(typeof(GovernanceSoftDeleteCommand.GovernanceSoftDeleteCommandResult))]
[JsonSerializable(typeof(DisasterRecoveryEnableCrrCommand.DisasterRecoveryEnableCrrCommandResult))]
[JsonSerializable(typeof(SecurityConfigureMuaCommand.SecurityConfigureMuaCommandResult))]
[JsonSerializable(typeof(SecurityConfigureEncryptionCommand.SecurityConfigureEncryptionCommandResult))]
[JsonSerializable(typeof(BackupVaultInfo))]
[JsonSerializable(typeof(ProtectedItemInfo))]
[JsonSerializable(typeof(BackupPolicyInfo))]
[JsonSerializable(typeof(BackupJobInfo))]
[JsonSerializable(typeof(RecoveryPointInfo))]
[JsonSerializable(typeof(ProtectableItemInfo))]
[JsonSerializable(typeof(VaultCreateResult))]
[JsonSerializable(typeof(ProtectResult))]
[JsonSerializable(typeof(OperationResult))]
[JsonSerializable(typeof(BackupStatusResult))]
[JsonSerializable(typeof(UnprotectedResourceInfo))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class AzureBackupJsonContext : JsonSerializerContext
{
}
