// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;

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

    /// <summary>
    /// Parses a job ID or full ARM job resource ID.
    /// </summary>
    /// <param name="job">The raw --job value.</param>
    /// <param name="vault">The --vault value. A full resource ID must reference this vault.</param>
    /// <param name="resourceGroup">The --resource-group value. A full resource ID must reference this resource group.</param>
    /// <param name="vaultType">The --vault-type value, if specified. A full resource ID must match it.</param>
    /// <param name="reference">The parsed job reference when successful.</param>
    /// <param name="error">A user-facing error when parsing fails.</param>
    internal static bool TryParse(
        string? job,
        string? vault,
        string? resourceGroup,
        string? vaultType,
        [NotNullWhen(true)] out BackupJobReference? reference,
        [NotNullWhen(false)] out string? error)
    {
        reference = null;
        var value = job?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(value))
        {
            error = InvalidFormatMessage;
            return false;
        }

        if (!value.Contains('/'))
        {
            reference = new(value, null);
            error = null;
            return true;
        }

        if (value.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseResourceId(value, vault, resourceGroup, vaultType, out reference, out error);
        }

        // Relative forms such as "/{id}" or "backupJobs/{id}".
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var jobName = segments switch
        {
            [var single] => single,
            [var type, var name] when type.Equals(BackupJobsSegment, StringComparison.OrdinalIgnoreCase) => name,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(jobName))
        {
            error = InvalidFormatMessage;
            return false;
        }

        reference = new(jobName.Trim(), null);
        error = null;
        return true;
    }

    private static bool TryParseResourceId(
        string value,
        string? vault,
        string? resourceGroup,
        string? vaultType,
        out BackupJobReference? reference,
        out string? error)
    {
        reference = null;
        if (!ResourceIdentifier.TryParse(value, out var id) || id is null || string.IsNullOrEmpty(id.Name))
        {
            error = InvalidFormatMessage;
            return false;
        }

        string? inferredVaultType = id.ResourceType.ToString() switch
        {
            var type when type.Equals(RsvJobResourceType, StringComparison.OrdinalIgnoreCase) => VaultTypeResolver.Rsv,
            var type when type.Equals(DppJobResourceType, StringComparison.OrdinalIgnoreCase) => VaultTypeResolver.Dpp,
            _ => null
        };

        if (inferredVaultType is null)
        {
            error = InvalidFormatMessage;
            return false;
        }

        if (!string.IsNullOrEmpty(vault) && !string.Equals(id.Parent?.Name, vault, StringComparison.OrdinalIgnoreCase))
        {
            error = $"The --job resource ID references vault '{id.Parent?.Name}', which does not match --vault '{vault}'.";
            return false;
        }

        if (!string.IsNullOrEmpty(resourceGroup) && !string.Equals(id.ResourceGroupName, resourceGroup, StringComparison.OrdinalIgnoreCase))
        {
            error = $"The --job resource ID references resource group '{id.ResourceGroupName}', which does not match --resource-group '{resourceGroup}'.";
            return false;
        }

        if ((VaultTypeResolver.IsRsv(vaultType) || VaultTypeResolver.IsDpp(vaultType)) &&
            !string.Equals(vaultType, inferredVaultType, StringComparison.OrdinalIgnoreCase))
        {
            error = $"The --job resource ID is a '{inferredVaultType}' job, which does not match --vault-type '{vaultType}'.";
            return false;
        }

        reference = new(id.Name, inferredVaultType);
        error = null;
        return true;
    }
}
