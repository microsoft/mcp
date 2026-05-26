// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// NEW-4 fix: centralised user-facing validation for the RSV protectable-item / policy
/// <c>--workload-type</c> token, mirroring exactly the alias set accepted by
/// <see cref="RsvBackupOperations"/>'s private <c>NormalizeWorkloadTypeForFilter</c>
/// switch. Used by the command-layer validator so that unknown values are rejected
/// at the boundary (400 ValidationError) instead of surfacing as a 500
/// <see cref="System.ArgumentException"/> from the service layer.
/// </summary>
/// <remarks>
/// Kept intentionally in sync (by hand) with the service-layer switch. If you add a new
/// alias to either side, add it here too.
/// </remarks>
internal static class WorkloadTypeNormalizer
{
    /// <summary>
    /// Human-readable supported-token summary, matching the message the service layer
    /// throws so that error text stays consistent regardless of which side rejects.
    /// </summary>
    public const string SupportedTokensDescription =
        "SQL or SQLDatabase, SQLInstance, SAPHana or SAPHanaDatabase, SAPHanaSystem, " +
        "SAPHanaDBInstance or SAPHanaDBI, VM, IaaSVM, or VirtualMachine, " +
        "FileShare, AzureFileShare, or AFS, SAPAse, SAPAseDatabase, ASE, or Sybase";

    /// <summary>
    /// Returns <c>true</c> when <paramref name="workloadType"/> is a recognised alias.
    /// Case-insensitive. Whitespace / null inputs return <c>false</c>.
    /// </summary>
    public static bool IsSupported(string? workloadType)
    {
        if (string.IsNullOrWhiteSpace(workloadType))
        {
            return false;
        }

        return workloadType.ToUpperInvariant() switch
        {
            "SQL" or "SQLDATABASE" => true,
            "SQLINSTANCE" => true,
            "SAPHANA" or "SAPHANADATABASE" => true,
            "SAPHANASYSTEM" => true,
            "SAPHANADBINSTANCE" or "SAPHANADBI" => true,
            "VM" or "IAASVM" or "VIRTUALMACHINE" => true,
            "FILESHARE" or "AZUREFILESHARE" or "AFS" => true,
            "SAPASE" or "SAPASEDATABASE" or "ASE" or "SYBASE" => true,
            _ => false
        };
    }

    /// <summary>
    /// Returns a human-readable error message naming the rejected value and the
    /// supported tokens, suitable for a System.CommandLine validator.
    /// </summary>
    public static string FormatUnknownMessage(string? workloadType) =>
        $"Unknown workload type '{workloadType}'. Supported values (case-insensitive): {SupportedTokensDescription}.";
}

