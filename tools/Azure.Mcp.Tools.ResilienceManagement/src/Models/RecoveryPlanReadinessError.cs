// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanReadinessError(
    string? Code,
    string? Message,
    IReadOnlyList<string> Recommendations);
