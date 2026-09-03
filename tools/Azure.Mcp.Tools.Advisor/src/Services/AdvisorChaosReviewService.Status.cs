// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Models.Chaos;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed partial class AdvisorChaosReviewService
{
    private async Task<ChaosRemediationStatus> ReviewStatusCoreAsync(
        ArmRequestContext context,
        ChaosRemediationTarget target,
        string? selectedWorkspace,
        string? selectedScenario,
        string? selectedConfiguration)
    {
        var targetReview = await ReviewTargetAsync(context, target);
        if (!targetReview.Eligible)
        {
            return Status(
                targetReview.Status,
                false,
                targetReview.ReasonCode ?? "TargetNotEligible",
                targetReview.Message,
                targetReview,
                requiredPermission: targetReview.RequiredPermission);
        }

        PagedResult<WorkspaceCandidate> workspaceResult;
        try
        {
            workspaceResult = await GetAllPagesAsync(
                context,
                $"/subscriptions/{target.SubscriptionId:D}/providers/Microsoft.Chaos/workspaces" +
                    $"?api-version={Uri.EscapeDataString(ChaosApiVersion)}",
                ParseWorkspaces);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Azure returned an invalid Chaos workspace response.");
            return Status(
                "Failed",
                false,
                "InvalidWorkspaceResponse",
                "Azure returned an invalid Chaos workspace response.",
                targetReview);
        }

        if (workspaceResult.Failure?.StatusCode == HttpStatusCode.Forbidden)
        {
            return Status(
                "Blocked",
                false,
                "WorkspaceReadAccessDenied",
                "The current identity cannot list Chaos workspaces in this subscription.",
                targetReview,
                requiredPermission: WorkspaceReadPermission);
        }

        if (workspaceResult.Failure is not null)
        {
            return Status(
                "Failed",
                false,
                "WorkspaceDiscoveryFailed",
                $"Azure returned HTTP {(int)workspaceResult.Failure.StatusCode} while listing Chaos workspaces.",
                targetReview);
        }

        if (workspaceResult.PageLimitExceeded)
        {
            return Status(
                "Failed",
                false,
                "WorkspacePageLimitExceeded",
                "Chaos workspace discovery exceeded the configured page limit.",
                targetReview);
        }

        var coveringWorkspaces = workspaceResult.Items
            .Where(workspace =>
                IsExactChaosWorkspaceResourceId(
                    workspace.Id,
                    target.SubscriptionId,
                    workspace.Name,
                    context.ManagementEndpoint) &&
                workspace.Scopes.Any(scope =>
                    DoesWorkspaceScopeCoverTarget(
                        scope,
                        target.ResourceId,
                        context.ManagementEndpoint)))
            .ToArray();

        if (coveringWorkspaces.Length == 0)
        {
            return Status(
                "SetupRequired",
                false,
                "CoveringWorkspaceNotFound",
                "No Chaos workspace scope covers the selected VMSS.",
                targetReview);
        }

        var workspaceCandidates = new List<ChaosWorkspaceCandidate>();
        foreach (var discoveredWorkspace in coveringWorkspaces)
        {
            workspaceCandidates.Add(
                await HydrateWorkspaceCandidateAsync(
                    context,
                    discoveredWorkspace,
                    target.SubscriptionId,
                    target.ResourceId));
        }

        var orderedWorkspaceCandidates = workspaceCandidates
            .OrderBy(candidate => candidate.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var selectableWorkspaces = orderedWorkspaceCandidates
            .Where(candidate => candidate.Selectable)
            .ToArray();
        if (selectableWorkspaces.Length == 0)
        {
            return Status(
                "Blocked",
                false,
                "WorkspaceNotReady",
                "No covering Chaos workspace could be verified as ready with a supported identity.",
                targetReview,
                workspaceCandidates: orderedWorkspaceCandidates);
        }

        var workspace = SelectById(
            selectableWorkspaces,
            selectedWorkspace,
            candidate => candidate.Id);
        if (selectedWorkspace is not null && workspace is null)
        {
            return Status(
                "Blocked",
                false,
                "WorkspaceSelectionInvalid",
                "The selected workspace is not a valid candidate for this VMSS.",
                targetReview,
                workspaceCandidates: orderedWorkspaceCandidates);
        }

        if (workspace is null)
        {
            return Status(
                "SelectionRequired",
                false,
                "WorkspaceSelectionRequired",
                "Multiple valid Chaos workspaces cover the selected VMSS.",
                targetReview,
                workspaceCandidates: orderedWorkspaceCandidates);
        }

        return await ReadScenarioStatusAsync(
            context,
            target,
            targetReview,
            workspace,
            orderedWorkspaceCandidates,
            selectedScenario,
            selectedConfiguration);
    }

    private async Task<ChaosRemediationStatus> ReadScenarioStatusAsync(
        ArmRequestContext context,
        ChaosRemediationTarget target,
        ChaosTargetReview targetReview,
        ChaosWorkspaceCandidate workspace,
        IReadOnlyList<ChaosWorkspaceCandidate> workspaceCandidates,
        string? selectedScenario,
        string? selectedConfiguration)
    {
        var scenarioResult = await GetAllPagesAsync(
            context,
            $"{workspace.Id}/scenarios?api-version={Uri.EscapeDataString(ChaosApiVersion)}",
            ParseScenarios);

        var scenarioFailure = HandleReadFailure(
            scenarioResult,
            "ScenarioReadAccessDenied",
            "ScenarioDiscoveryFailed",
            "Chaos scenario discovery exceeded the configured page limit.",
            targetReview,
            workspace,
            workspaceCandidates: workspaceCandidates);
        if (scenarioFailure is not null)
        {
            return scenarioFailure;
        }

        var matchingScenarios = scenarioResult.Items
            .Where(scenario =>
                IsExactChildResourceId(
                    scenario.Id,
                    workspace.Id,
                    "scenarios",
                    scenario.Name,
                    context.ManagementEndpoint) &&
                string.Equals(
                    scenario.RecommendationStatus,
                    "Recommended",
                    StringComparison.OrdinalIgnoreCase) &&
                scenario.ActionIds.Any(actionId =>
                    actionId.StartsWith(
                        RequiredActionIdPrefix,
                        StringComparison.OrdinalIgnoreCase)))
            .Select(scenario => new ChaosScenarioCandidate(
                scenario.Id,
                scenario.Name,
                scenario.RecommendationStatus,
                scenario.ActionIds))
            .OrderBy(scenario => scenario.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (matchingScenarios.Length == 0)
        {
            return Status(
                "SetupRequired",
                false,
                "ComputeZoneDownScenarioNotFound",
                "The selected workspace does not contain a recommended scenario with the required Compute Zone Down action contract.",
                targetReview,
                workspace,
                workspaceCandidates: workspaceCandidates);
        }

        var scenario = SelectById(
            matchingScenarios,
            selectedScenario,
            candidate => candidate.Id);
        if (selectedScenario is not null && scenario is null)
        {
            return Status(
                "Blocked",
                false,
                "ScenarioSelectionInvalid",
                "The selected scenario is not a compatible candidate in the selected workspace.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenarioCandidates: matchingScenarios);
        }

        if (scenario is null)
        {
            return Status(
                "SelectionRequired",
                false,
                "ScenarioSelectionRequired",
                "Multiple compatible Compute Zone Down scenarios are available.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenarioCandidates: matchingScenarios);
        }

        var runHistory = await ReadScenarioRunHistoryAsync(
            context,
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            matchingScenarios);
        if (runHistory.Failure is not null)
        {
            return runHistory.Failure;
        }

        var configurationResult = await GetAllPagesAsync(
            context,
            $"{scenario.Id}/configurations?api-version={Uri.EscapeDataString(ChaosApiVersion)}",
            ParseConfigurations);

        var configurationFailure = HandleReadFailure(
            configurationResult,
            "ConfigurationReadAccessDenied",
            "ConfigurationDiscoveryFailed",
            "Chaos configuration discovery exceeded the configured page limit.",
            targetReview,
            workspace,
            workspaceCandidates: workspaceCandidates,
            scenario: scenario,
            scenarioCandidates: matchingScenarios,
            runCandidates: runHistory.Candidates);
        if (configurationFailure is not null)
        {
            return configurationFailure;
        }

        var configurations = configurationResult.Items
            .Where(configuration =>
                IsExactChildResourceId(
                    configuration.Id,
                    scenario.Id,
                    "configurations",
                    configuration.Name,
                    context.ManagementEndpoint) &&
                string.Equals(
                    configuration.ScenarioId,
                    scenario.Id,
                    StringComparison.OrdinalIgnoreCase) &&
                configuration.TargetResourceIds.Count > 0 &&
                configuration.TargetResourceIds.All(resourceId =>
                    IsResourceAtOrBelowTarget(
                        resourceId,
                        target.ResourceId,
                        context.ManagementEndpoint)) &&
                configuration.Locations.Count == 1 &&
                string.Equals(
                    configuration.Locations[0],
                    targetReview.Location,
                    StringComparison.OrdinalIgnoreCase) &&
                configuration.Zones.Count == 1 &&
                configuration.HasExactParameterContract &&
                configuration.HasExactResourceTargetingContract &&
                configuration.LastModifiedAt is not null &&
                targetReview.Zones.Contains(
                    configuration.Zones[0],
                    StringComparer.OrdinalIgnoreCase) &&
                IsSupportedDuration(configuration.Duration) &&
                string.Equals(
                    configuration.ProvisioningState,
                    "Succeeded",
                    StringComparison.OrdinalIgnoreCase))
            .Select(configuration => new ChaosConfigurationCandidate(
                configuration.Id,
                configuration.Name,
                scenario.Id,
                scenario.Name,
                configuration.Zones[0],
                configuration.Locations[0],
                configuration.Duration!,
                configuration.ProvisioningState,
                configuration.LastModifiedAt!.Value,
                configuration.TargetResourceIds))
            .OrderBy(configuration => configuration.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (configurations.Length == 0)
        {
            return Status(
                "SetupRequired",
                false,
                "ExactConfigurationNotFound",
                "No succeeded Compute Zone Down configuration stays within the selected VMSS boundary.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                matchingScenarios,
                runCandidates: runHistory.Candidates);
        }

        var configuration = SelectById(
            configurations,
            selectedConfiguration,
            candidate => candidate.Id);
        if (selectedConfiguration is not null && configuration is null)
        {
            return Status(
                "Blocked",
                false,
                "ConfigurationSelectionInvalid",
                "The selected configuration is not a compatible candidate for the selected VMSS and scenario.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                matchingScenarios,
                configurationCandidates: configurations,
                runCandidates: runHistory.Candidates);
        }

        if (configuration is null)
        {
            return Status(
                "SelectionRequired",
                false,
                "ConfigurationSelectionRequired",
                "Multiple compatible Compute Zone Down configurations are available.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                matchingScenarios,
                configurationCandidates: configurations,
                runCandidates: runHistory.Candidates);
        }

        return await ReadValidationStatusAsync(
            context,
            target,
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            matchingScenarios,
            configuration,
            configurations,
            runHistory);
    }

    private async Task<ChaosRemediationStatus> ReadValidationStatusAsync(
        ArmRequestContext context,
        ChaosRemediationTarget target,
        ChaosTargetReview targetReview,
        ChaosWorkspaceCandidate workspace,
        IReadOnlyList<ChaosWorkspaceCandidate> workspaceCandidates,
        ChaosScenarioCandidate scenario,
        IReadOnlyList<ChaosScenarioCandidate> scenarioCandidates,
        ChaosConfigurationCandidate configuration,
        IReadOnlyList<ChaosConfigurationCandidate> configurationCandidates,
        ScenarioRunHistory runHistory)
    {
        var runStatus = EvaluateConfigurationRunStatus(
            context.ManagementEndpoint,
            target,
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            scenarioCandidates,
            configuration,
            configurationCandidates,
            runHistory,
            validation: null);
        if (runStatus.Status != "Ready")
        {
            return runStatus;
        }

        var runCandidates = runStatus.Runs;
        var validationId = $"{configuration.Id}/validations/latest";
        var response = await GetWithThrottleRetryAsync(
            context,
            WithChaosApiVersion(validationId));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Status(
                "ValidationRequired",
                false,
                "ConfigurationValidationRequired",
                "The exact Compute Zone Down configuration must be validated before execution.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return Status(
                "Blocked",
                false,
                "ValidationReadAccessDenied",
                "The current identity cannot read validation for the exact Chaos configuration.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                requiredPermission: ConfigurationReadPermission);
        }

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Accepted))
        {
            return Status(
                "Failed",
                false,
                "ValidationReadFailed",
                $"Azure returned HTTP {(int)response.StatusCode} while reading configuration validation.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates);
        }

        using var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        var validationState = GetString(root, "properties", "status");
        var startTime = ParseDateTime(GetString(root, "properties", "startTime"));
        var endTime = ParseDateTime(GetString(root, "properties", "endTime"));
        if (string.IsNullOrWhiteSpace(validationState) || startTime is null)
        {
            throw new JsonException("The latest configuration validation is incomplete.");
        }

        var (permissionErrorCount, resourceErrorCount) =
            GetValidationErrorCounts(root);
        var validation = new ChaosValidationStatus(
            validationState,
            startTime.Value,
            endTime,
            permissionErrorCount,
            resourceErrorCount);

        if (startTime.Value < configuration.LastModifiedAt)
        {
            return Status(
                "ValidationRequired",
                false,
                "ConfigurationValidationStale",
                "The exact configuration changed after its latest validation.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                validation: validation);
        }

        if (IsValidationPending(validationState))
        {
            return Status(
                "Validating",
                false,
                "ConfigurationValidationInProgress",
                "Validation of the exact Compute Zone Down configuration is still in progress.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                validation: validation);
        }

        if (string.Equals(
                validationState,
                "RequiresAttention",
                StringComparison.OrdinalIgnoreCase))
        {
            var permissionsOnly =
                permissionErrorCount > 0 &&
                resourceErrorCount == 0;
            return Status(
                permissionsOnly ? "PermissionsRequired" : "Blocked",
                false,
                permissionsOnly
                    ? "ScenarioPermissionsRequired"
                    : "ConfigurationValidationRequiresAttention",
                permissionsOnly
                    ? "The exact configuration requires scenario execution permissions."
                    : "The exact configuration has resource validation errors that permission repair cannot resolve.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                validation: validation);
        }

        if (string.Equals(
                validationState,
                "NoResolvedResources",
                StringComparison.OrdinalIgnoreCase))
        {
            return Status(
                "Blocked",
                false,
                "NoResolvedResources",
                "Validation found no resources for the exact VMSS configuration.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                validation: validation);
        }

        if (!string.Equals(
                validationState,
                "Succeeded",
                StringComparison.OrdinalIgnoreCase))
        {
            return Status(
                "Failed",
                false,
                "UnknownValidationState",
                "Azure returned an unrecognized configuration validation state.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                validation: validation);
        }

        return Status(
            "Ready",
            true,
            null,
            "The selected VMSS has a validated Compute Zone Down configuration and no active run.",
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            scenarioCandidates,
            configuration,
            configurationCandidates,
            runCandidates,
            validation: validation);
    }

    private async Task<ScenarioRunHistory> ReadScenarioRunHistoryAsync(
        ArmRequestContext context,
        ChaosTargetReview targetReview,
        ChaosWorkspaceCandidate workspace,
        IReadOnlyList<ChaosWorkspaceCandidate> workspaceCandidates,
        ChaosScenarioCandidate scenario,
        IReadOnlyList<ChaosScenarioCandidate> scenarioCandidates)
    {
        PagedResult<RunCandidate> runResult;
        try
        {
            runResult = await GetAllPagesAsync(
                context,
                $"{scenario.Id}/runs?api-version={Uri.EscapeDataString(ChaosApiVersion)}",
                ParseRuns);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Azure returned an invalid Chaos run response.");
            return ScenarioRunHistory.Failed(
                Status(
                    "Failed",
                    false,
                    "InvalidRunResponse",
                    "Azure returned an incomplete Chaos run.",
                    targetReview,
                    workspace,
                    workspaceCandidates,
                    scenario,
                    scenarioCandidates));
        }

        var runFailure = HandleReadFailure(
            runResult,
            "RunReadAccessDenied",
            "RunDiscoveryFailed",
            "Chaos run discovery exceeded the configured page limit.",
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            scenarioCandidates);
        if (runFailure is not null)
        {
            return ScenarioRunHistory.Failed(runFailure);
        }

        var scenarioRuns = runResult.Items
            .Where(run =>
                IsExactRunResource(
                    run.Id,
                    scenario.Id,
                    run.Name))
            .OrderByDescending(run => run.StartTime ?? DateTimeOffset.MinValue)
            .ThenBy(run => run.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var runCandidates = scenarioRuns
            .Select(run => ToRunCandidate(workspace, scenario, run))
            .ToArray();

        return new(scenarioRuns, runCandidates, Failure: null);
    }

    private static ChaosRemediationStatus EvaluateConfigurationRunStatus(
        Uri managementEndpoint,
        ChaosRemediationTarget target,
        ChaosTargetReview targetReview,
        ChaosWorkspaceCandidate workspace,
        IReadOnlyList<ChaosWorkspaceCandidate> workspaceCandidates,
        ChaosScenarioCandidate scenario,
        IReadOnlyList<ChaosScenarioCandidate> scenarioCandidates,
        ChaosConfigurationCandidate configuration,
        IReadOnlyList<ChaosConfigurationCandidate> configurationCandidates,
        ScenarioRunHistory runHistory,
        ChaosValidationStatus? validation)
    {
        var matchingRuns = runHistory.Runs
            .Where(run => string.Equals(
                run.ConfigurationName,
                configuration.Name,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingRuns
            .Where(run => IsActiveRun(run.Status))
            .Any(run =>
                !AreRunResourcesWithinTarget(
                    run,
                    target.ResourceId,
                    managementEndpoint)))
        {
            return Status(
                "Blocked",
                false,
                "RunScopeMismatch",
                "An active Chaos run contains resources outside the selected VMSS.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runHistory.Candidates,
                validation: validation);
        }

        var activeRuns = runHistory.Candidates
            .Where(run =>
                string.Equals(
                    run.ConfigurationResourceId,
                    configuration.Id,
                    StringComparison.OrdinalIgnoreCase) &&
                IsActiveRun(run.Status))
            .ToArray();
        if (activeRuns.Length > 0)
        {
            return Status(
                "Running",
                false,
                activeRuns.Length == 1
                    ? "ActiveRunExists"
                    : "MultipleActiveRunsExist",
                activeRuns.Length == 1
                    ? "An active Chaos run already targets the selected configuration and VMSS."
                    : "Multiple active Chaos runs target the selected configuration and VMSS.",
                targetReview,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runHistory.Candidates,
                validation: validation);
        }

        return Status(
            "Ready",
            true,
            null,
            "The selected VMSS has one exact, ready Compute Zone Down configuration and no active run.",
            targetReview,
            workspace,
            workspaceCandidates,
            scenario,
            scenarioCandidates,
            configuration,
            configurationCandidates,
            runHistory.Candidates,
            validation: validation);
    }

    private static ChaosRemediationStatus? HandleReadFailure<T>(
        PagedResult<T> result,
        string forbiddenReasonCode,
        string failureReasonCode,
        string pageLimitMessage,
        ChaosTargetReview target,
        ChaosWorkspaceCandidate? workspace = null,
        IReadOnlyList<ChaosWorkspaceCandidate>? workspaceCandidates = null,
        ChaosScenarioCandidate? scenario = null,
        IReadOnlyList<ChaosScenarioCandidate>? scenarioCandidates = null,
        ChaosConfigurationCandidate? configuration = null,
        IReadOnlyList<ChaosConfigurationCandidate>? configurationCandidates = null,
        IReadOnlyList<ChaosRunSummary>? runCandidates = null)
    {
        if (result.Failure?.StatusCode == HttpStatusCode.Forbidden)
        {
            return Status(
                "Blocked",
                false,
                forbiddenReasonCode,
                "The current identity cannot read the required Chaos resources.",
                target,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates,
                requiredPermission: ConfigurationReadPermission);
        }

        if (result.Failure is not null)
        {
            return Status(
                "Failed",
                false,
                failureReasonCode,
                $"Azure returned HTTP {(int)result.Failure.StatusCode} while reading Chaos resources.",
                target,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates);
        }

        return result.PageLimitExceeded
            ? Status(
                "Failed",
                false,
                failureReasonCode,
                pageLimitMessage,
                target,
                workspace,
                workspaceCandidates,
                scenario,
                scenarioCandidates,
                configuration,
                configurationCandidates,
                runCandidates)
            : null;
    }

    private async Task<ChaosWorkspaceCandidate> HydrateWorkspaceCandidateAsync(
        ArmRequestContext context,
        WorkspaceCandidate discoveredWorkspace,
        Guid subscriptionId,
        string targetResourceId)
    {
        var response = await GetWithThrottleRetryAsync(
            context,
            WithChaosApiVersion(discoveredWorkspace.Id));
        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Accepted))
        {
            return ToWorkspaceCandidate(
                discoveredWorkspace,
                selectable: false,
                response.StatusCode == HttpStatusCode.Forbidden
                    ? "WorkspaceExactReadAccessDenied"
                    : "WorkspaceExactReadFailed");
        }

        var exactWorkspace = ParseWorkspace(response.Body);
        if (!string.Equals(
                exactWorkspace.Id.TrimEnd('/'),
                discoveredWorkspace.Id.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                exactWorkspace.Name,
                discoveredWorkspace.Name,
                StringComparison.OrdinalIgnoreCase) ||
            !IsExactChaosWorkspaceResourceId(
                exactWorkspace.Id,
                subscriptionId,
                exactWorkspace.Name,
                context.ManagementEndpoint) ||
            !exactWorkspace.Scopes.Any(scope =>
                DoesWorkspaceScopeCoverTarget(
                    scope,
                    targetResourceId,
                    context.ManagementEndpoint)))
        {
            return ToWorkspaceCandidate(
                exactWorkspace,
                selectable: false,
                "WorkspaceExactReadMismatch");
        }

        if (!string.Equals(
                exactWorkspace.ProvisioningState,
                "Succeeded",
                StringComparison.OrdinalIgnoreCase))
        {
            return ToWorkspaceCandidate(
                exactWorkspace,
                selectable: false,
                "WorkspaceProvisioningNotSucceeded");
        }

        if (string.IsNullOrWhiteSpace(exactWorkspace.IdentityType) ||
            !exactWorkspace.IdentityType.Contains(
                "SystemAssigned",
                StringComparison.OrdinalIgnoreCase))
        {
            return ToWorkspaceCandidate(
                exactWorkspace,
                selectable: false,
                "UnsupportedWorkspaceIdentity");
        }

        if (!Guid.TryParse(exactWorkspace.PrincipalId, out var principalId) ||
            principalId == Guid.Empty)
        {
            return ToWorkspaceCandidate(
                exactWorkspace,
                selectable: false,
                "WorkspacePrincipalUnavailable");
        }

        return ToWorkspaceCandidate(exactWorkspace, selectable: true, null);
    }
}
