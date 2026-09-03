// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Compatible Chaos configuration with its exact target contract.</summary>
public sealed record ChaosConfigurationCandidate(
    string Id,
    string Name,
    string ScenarioId,
    string ScenarioName,
    string Zone,
    string Location,
    string Duration,
    string? ProvisioningState,
    DateTimeOffset LastModifiedAt,
    IReadOnlyList<string> TargetResourceIds);
