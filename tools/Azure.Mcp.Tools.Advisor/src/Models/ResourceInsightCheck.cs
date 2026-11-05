// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Advisor.Models;

internal sealed record ResourceInsightCheck(
    string Id,
    string Name,
    string Goal,
    string Kind,
    string Severity,
    string Interpretation,
    string? Expect = null,
    string? Query = null,
    string? Field = null,
    JsonValueKind ExpectedKind = JsonValueKind.Undefined,
    string? ExpectedValue = null,
    string? Guidance = null,
    IReadOnlyList<UnusedAgeSignal>? Signals = null);
