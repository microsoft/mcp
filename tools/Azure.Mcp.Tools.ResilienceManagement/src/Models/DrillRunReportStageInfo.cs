// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record DrillRunReportStageInfo(
    string DrillRunStage,
    string? GenerationStatus,
    DateTimeOffset? LastAttemptOn,
    DrillRunReportErrorInfo? LastError);
