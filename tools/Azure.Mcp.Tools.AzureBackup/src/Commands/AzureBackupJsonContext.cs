// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Commands.Backup;
using Azure.Mcp.Tools.AzureBackup.Commands.Dr;
using Azure.Mcp.Tools.AzureBackup.Commands.Governance;
using Azure.Mcp.Tools.AzureBackup.Commands.Job;
using Azure.Mcp.Tools.AzureBackup.Commands.Policy;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Commands.Vault;
using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

// Vault
[JsonSerializable(typeof(VaultGetCommand.VaultGetCommandResult))]
[JsonSerializable(typeof(VaultCreateCommand.VaultCreateCommandResult))]
[JsonSerializable(typeof(VaultUpdateCommand.VaultUpdateCommandResult))]
// Policy
[JsonSerializable(typeof(PolicyGetCommand.PolicyGetCommandResult))]
[JsonSerializable(typeof(PolicyCreateCommand.PolicyCreateCommandResult))]
// Protected item
[JsonSerializable(typeof(ProtectedItemGetCommand.ProtectedItemGetCommandResult))]
[JsonSerializable(typeof(ProtectedItemProtectCommand.ProtectedItemProtectCommandResult))]
// Protectable item
[JsonSerializable(typeof(ProtectableItemListCommand.ProtectableItemListCommandResult))]
// Backup
[JsonSerializable(typeof(BackupStatusCommand.BackupStatusCommandResult))]
// Job
[JsonSerializable(typeof(JobGetCommand.JobGetCommandResult))]
// Recovery point
[JsonSerializable(typeof(RecoveryPointGetCommand.RecoveryPointGetCommandResult))]
// Governance
[JsonSerializable(typeof(GovernanceFindUnprotectedCommand.GovernanceFindUnprotectedCommandResult))]
[JsonSerializable(typeof(GovernanceImmutabilityCommand.GovernanceImmutabilityCommandResult))]
[JsonSerializable(typeof(GovernanceSoftDeleteCommand.GovernanceSoftDeleteCommandResult))]
// DR
[JsonSerializable(typeof(DrEnableCrrCommand.DrEnableCrrCommandResult))]
// Model types
[JsonSerializable(typeof(BackupVaultInfo))]
[JsonSerializable(typeof(ProtectedItemInfo))]
[JsonSerializable(typeof(BackupPolicyInfo))]
[JsonSerializable(typeof(BackupJobInfo))]
[JsonSerializable(typeof(RecoveryPointInfo))]
[JsonSerializable(typeof(ProtectableItemInfo))]
[JsonSerializable(typeof(ContainerInfo))]
[JsonSerializable(typeof(VaultCreateResult))]
[JsonSerializable(typeof(ProtectResult))]
[JsonSerializable(typeof(BackupTriggerResult))]
[JsonSerializable(typeof(RestoreTriggerResult))]
[JsonSerializable(typeof(OperationResult))]
[JsonSerializable(typeof(BackupStatusResult))]
[JsonSerializable(typeof(CostEstimateResult))]
[JsonSerializable(typeof(HealthCheckResult))]
[JsonSerializable(typeof(HealthCheckItemDetail))]
[JsonSerializable(typeof(UnprotectedResourceInfo))]
[JsonSerializable(typeof(DrValidationResult))]
[JsonSerializable(typeof(WorkflowResult))]
[JsonSerializable(typeof(WorkflowStep))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class AzureBackupJsonContext : JsonSerializerContext
{
}
