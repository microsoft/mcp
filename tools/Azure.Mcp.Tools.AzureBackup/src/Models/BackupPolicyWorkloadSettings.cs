// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Common workload settings returned by RSV workload (SQL/SAP HANA) backup policies.
/// </summary>
public sealed record BackupPolicyWorkloadSettings(
    string? TimeZone,
    bool? IsCompression,
    bool? IsSqlCompression);
