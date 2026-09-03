// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Latest validation state for the selected Chaos configuration.</summary>
public sealed record ChaosValidationStatus(
    string Status,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    int PermissionErrorCount,
    int ResourceErrorCount);
