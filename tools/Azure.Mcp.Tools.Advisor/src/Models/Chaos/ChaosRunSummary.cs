// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Active or historical Chaos scenario run returned for the selected scenario.</summary>
public sealed record ChaosRunSummary(
    string Id,
    string RunId,
    string WorkspaceResourceId,
    string ScenarioResourceId,
    string ScenarioName,
    string ConfigurationResourceId,
    string ConfigurationName,
    string Status,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    IReadOnlyList<string> ResourceIds);
