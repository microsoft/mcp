// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record DrillRunReportInfo(
    string? GenerationStatus,
    string? FinalizationState,
    IReadOnlyList<DrillRunReportStageInfo> StageStatuses,
    IReadOnlyList<string> AvailableFormats,
    DateTimeOffset? LastGeneratedOn,
    string? SchemaVersion,
    DrillRunReportErrorInfo? LastError);
