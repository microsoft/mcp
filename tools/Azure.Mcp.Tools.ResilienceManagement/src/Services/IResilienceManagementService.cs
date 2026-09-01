// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public interface IResilienceManagementService
{
    Task<IEnumerable<ResourceSummary>> ListGoalTemplatesAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default);

    Task<GoalTemplateInfo> GetGoalTemplateAsync(string serviceGroup, string goalTemplate, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListGoalAssignmentsAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default);

    Task<GoalAssignmentInfo> GetGoalAssignmentAsync(string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListUsagePlansAsync(string resourceGroup, string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<UsagePlanInfo> GetUsagePlanAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListUsagePlansBySubscriptionAsync(string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListUsagePlanEnrollmentsAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<UsagePlanEnrollmentInfo> GetUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListGoalResourcesAsync(string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default);

    Task<GoalResourceInfo> GetGoalResourceAsync(string serviceGroup, string goalAssignment, string goalResource, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListRecoveryPlansAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanInfo> CreateRecoveryPlanAsync(string serviceGroup, string recoveryPlan, RecoveryPlanKind planType, string? planDescription, RecoveryPlanIdentityKind identityType, string? userAssignedIdentity = null, string? defaultGroupDescription = null, string? tenant = null, IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = null, CancellationToken cancellationToken = default);

    Task<bool> DeleteRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanUpdateResourcesResult> UpdateRecoveryPlanResourcesAsync(string serviceGroup, string recoveryPlan, UpdateRecoveryResourcesContent content, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanFailoverResult> FailoverRecoveryPlanAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds = null, string? userConsent = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanFinalizeResult> FinalizeRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanReprotectResult> ReprotectRecoveryPlanAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanValidateForFailoverResult> ValidateRecoveryPlanForFailoverAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds = null, string? userConsent = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanValidateForReprotectResult> ValidateRecoveryPlanForReprotectAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanValidateForOperationResult> ValidateRecoveryPlanForOperationAsync(string serviceGroup, string recoveryPlan, RecoveryOperationNames operationName, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryPlanReadinessResult> CheckRecoveryPlanReadinessAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListRecoveryResourcesAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetRecoveryResourceAsync(string serviceGroup, string recoveryPlan, string recoveryResource, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListRecoveryJobsAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryJobRetryResult> RetryRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default);

    Task<RecoveryJobResumeResult> ResumeRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? description = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListRecoveryJobResourcesAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetRecoveryJobResourceAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string recoveryJobTarget, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListDrillsAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default);

    Task<DrillInfo> GetDrillAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default);

    Task<string> StartDrillAsync(string serviceGroup, string drill, string mode, string? tenant = null, CancellationToken cancellationToken = default);

    Task<string> EndDrillAsync(string serviceGroup, string drill, string attestation, string attestationNotes, string? tenant = null, CancellationToken cancellationToken = default);

    Task<DrillInfo> UpdateDrillAsync(string serviceGroup, string drill, string? subscription = null, string? region = null, DrillRbacSetupMode? rbacSetupMode = null, string? recoveryPlan = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task<DrillInfo> CreateDrillAsync(string serviceGroup, string drill, string subscription, string region, string? resourceGroup, DrillKind drillType, DrillRbacSetupMode rbacSetupMode, string? recoveryPlan = null, string? tenant = null, CancellationToken cancellationToken = default);

    Task DeleteDrillAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListDrillResourcesAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default);

    Task<DrillResourceInfo> GetDrillResourceAsync(string serviceGroup, string drill, string drillResource, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListDrillRunsAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetDrillRunAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceSummary>> ListDrillRunResourcesAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default);

    Task<JsonElement> GetDrillRunResourceAsync(string serviceGroup, string drill, string drillRun, string drillRunResource, string? tenant = null, CancellationToken cancellationToken = default);

    Task<UsagePlanInfo> CreateUsagePlanAsync(string resourceGroup, string usagePlan, UsagePlanKind planType, string subscription, string? tenant = null, CancellationToken cancellationToken = default);

    Task<UsagePlanEnrollmentInfo> CreateUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string serviceGroup, string subscription, string? tenant = null, CancellationToken cancellationToken = default);
}
