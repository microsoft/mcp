// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Normalizes the --job value for backup job lookups (NEW-6).
/// Agents commonly pass the full ARM job resource ID returned by job listings. Combining that with
/// <c>CreateResourceIdentifier(subscription, resourceGroup, vault, jobId)</c> yields an identifier whose
/// name segment is empty, which the SDK rejects with an <see cref="ArgumentException"/>.
/// </summary>
internal static class BackupJobIdParser
{
    private const string BackupJobsSegment = "backupJobs";
    private const string RsvJobResourceType = "Microsoft.RecoveryServices/vaults/backupJobs";
    private const string DppJobResourceType = "Microsoft.DataProtection/backupVaults/backupJobs";

    internal const string InvalidFormatMessage =
        "--job must be a backup job ID (for example, a GUID) or a full backup job resource ID " +
        "(/subscriptions/{subscription}/resourceGroups/{resource-group}/providers/Microsoft.RecoveryServices/vaults/{vault}/backupJobs/{job-id}).";

    internal const string SubscriptionMismatchTelemetryMessage = "The --job resource ID does not match --subscription.";
    internal const string ResourceGroupMismatchTelemetryMessage = "The --job resource ID does not match --resource-group.";
    internal const string VaultMismatchTelemetryMessage = "The --job resource ID does not match --vault.";
    internal const string VaultTypeMismatchTelemetryMessage = "The --job resource ID does not match --vault-type.";

    /// <summary>
    /// Parses a job ID or full ARM job resource ID, adding every validation error to <paramref name="validationResult"/>.
    /// </summary>
    /// <param name="job">The raw --job value.</param>
    /// <param name="subscription">
    /// The --subscription value. When it is a GUID, a full resource ID must reference this subscription.
    /// Subscription names are checked by <see cref="ValidateSubscription"/> after they are resolved to GUIDs.
    /// </param>
    /// <param name="vault">The --vault value. A full resource ID must reference this vault.</param>
    /// <param name="resourceGroup">The --resource-group value. A full resource ID must reference this resource group.</param>
    /// <param name="vaultType">The --vault-type value, if specified. A full resource ID must match it.</param>
    /// <param name="validationResult">Receives a client message and a telemetry-safe message for each error.</param>
    /// <returns>The parsed job reference, or <see langword="null"/> when any error was added.</returns>
    internal static BackupJobReference? Parse(
        string? job,
        string? subscription,
        string? vault,
        string? resourceGroup,
        string? vaultType,
        ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var value = job?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(value))
        {
            return AddInvalidFormat(validationResult);
        }

        if (!value.Contains('/'))
        {
            return new(value, null, null);
        }

        if (value.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
        {
            return ParseResourceId(value, subscription, vault, resourceGroup, vaultType, validationResult);
        }

        // Relative forms such as "/{id}" or "backupJobs/{id}".
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var jobName = segments switch
        {
            [var single] => single,
            [var type, var name] when type.Equals(BackupJobsSegment, StringComparison.OrdinalIgnoreCase) => name,
            _ => null
        };

        return string.IsNullOrWhiteSpace(jobName)
            ? AddInvalidFormat(validationResult)
            : new(jobName.Trim(), null, null);
    }

    /// <summary>
    /// Verifies that a job reference parsed from a full resource ID belongs to <paramref name="subscriptionId"/>.
    /// </summary>
    /// <param name="reference">The parsed job reference.</param>
    /// <param name="subscriptionId">The resolved --subscription GUID.</param>
    /// <param name="validationResult">Receives an error when the subscriptions differ.</param>
    internal static void ValidateSubscription(
        BackupJobReference reference,
        string subscriptionId,
        ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(validationResult);

        if (reference.SubscriptionId is not null &&
            !string.Equals(reference.SubscriptionId, subscriptionId, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"The --job resource ID references subscription '{reference.SubscriptionId}', which does not match --subscription '{subscriptionId}'.",
                SubscriptionMismatchTelemetryMessage);
        }
    }

    private static BackupJobReference? ParseResourceId(
        string value,
        string? subscription,
        string? vault,
        string? resourceGroup,
        string? vaultType,
        ValidationResult validationResult)
    {
        if (!ResourceIdentifier.TryParse(value, out var id) || id is null || string.IsNullOrEmpty(id.Name))
        {
            return AddInvalidFormat(validationResult);
        }

        string? inferredVaultType = id.ResourceType.ToString() switch
        {
            var type when type.Equals(RsvJobResourceType, StringComparison.OrdinalIgnoreCase) => VaultTypeResolver.Rsv,
            var type when type.Equals(DppJobResourceType, StringComparison.OrdinalIgnoreCase) => VaultTypeResolver.Dpp,
            _ => null
        };

        if (inferredVaultType is null)
        {
            return AddInvalidFormat(validationResult);
        }

        // A resource ID can disagree with several options at once; report every mismatch.
        var errorCount = validationResult.Errors.Count;
        var reference = new BackupJobReference(id.Name, inferredVaultType, id.SubscriptionId);

        if (Guid.TryParse(subscription, out _))
        {
            ValidateSubscription(reference, subscription!, validationResult);
        }

        if (!string.IsNullOrEmpty(resourceGroup) && !string.Equals(id.ResourceGroupName, resourceGroup, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"The --job resource ID references resource group '{id.ResourceGroupName}', which does not match --resource-group '{resourceGroup}'.",
                ResourceGroupMismatchTelemetryMessage);
        }

        if (!string.IsNullOrEmpty(vault) && !string.Equals(id.Parent?.Name, vault, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"The --job resource ID references vault '{id.Parent?.Name}', which does not match --vault '{vault}'.",
                VaultMismatchTelemetryMessage);
        }

        if ((VaultTypeResolver.IsRsv(vaultType) || VaultTypeResolver.IsDpp(vaultType)) &&
            !string.Equals(vaultType, inferredVaultType, StringComparison.OrdinalIgnoreCase))
        {
            validationResult.AddError(
                $"The --job resource ID is a '{inferredVaultType}' job, which does not match --vault-type '{vaultType}'.",
                VaultTypeMismatchTelemetryMessage);
        }

        return validationResult.Errors.Count == errorCount ? reference : null;
    }

    private static BackupJobReference? AddInvalidFormat(ValidationResult validationResult)
    {
        // The format message contains no user-supplied values, so it is safe for telemetry.
        validationResult.AddError(InvalidFormatMessage, InvalidFormatMessage);
        return null;
    }
}
