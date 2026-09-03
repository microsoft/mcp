// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Read-only Chaos readiness, candidates, and selected resources for one VMSS.</summary>
public sealed record ChaosRemediationStatus
{
    public string Status { get; init; } = string.Empty;

    public bool Ready { get; init; }

    public bool MutationPerformed { get; init; }

    public string? ReasonCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public ChaosTargetReview Target { get; init; } = null!;

    public IReadOnlyList<ChaosWorkspaceCandidate> WorkspaceCandidates { get; init; } = [];

    public ChaosWorkspaceCandidate? Workspace { get; init; }

    public IReadOnlyList<ChaosScenarioCandidate> ScenarioCandidates { get; init; } = [];

    public ChaosScenarioCandidate? Scenario { get; init; }

    public IReadOnlyList<ChaosConfigurationCandidate> ConfigurationCandidates { get; init; } = [];

    public ChaosConfigurationCandidate? Configuration { get; init; }

    public IReadOnlyList<ChaosRunSummary> Runs { get; init; } = [];

    public string? RequiredPermission { get; init; }

    public ChaosValidationStatus? Validation { get; init; }
}
