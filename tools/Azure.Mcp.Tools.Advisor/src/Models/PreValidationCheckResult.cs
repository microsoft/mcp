// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record PreValidationCheckResult(
    string Id,
    string Name,
    string Goal,
    string Severity,
    string Outcome,
    string Evidence,
    IReadOnlyList<JsonElement>? Data = null,
    string? Guidance = null,
    int? DaysUnused = null);
