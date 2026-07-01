// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed class ResilienceManagementService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService)
    : BaseAzureResourceService(subscriptionService, tenantService), IResilienceManagementService
{

    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    public async Task<IEnumerable<ResourceSummary>> ListGoalTemplatesAsync(string serviceGroup, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalTemplateCollection goalTemplates = armClient.GetGoalTemplates(serviceGroupId);

        var result = new List<ResourceSummary>();
        await foreach (var goalTemplate in goalTemplates.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: goalTemplate.Data.Id?.ToString() ?? string.Empty,
                Name: goalTemplate.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<GoalTemplateInfo> GetGoalTemplateAsync(string serviceGroup, string goalTemplate, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalTemplateCollection goalTemplates = armClient.GetGoalTemplates(serviceGroupId);
        GoalTemplateResource resource = await goalTemplates.GetAsync(goalTemplate, cancellationToken);

        return MapGoalTemplate(resource.Data);
    }

    private static GoalTemplateInfo MapGoalTemplate(GoalTemplateData data)
    {
        var props = data.Properties;
        var systemData = data.SystemData;

        var mappedProperties = props is null
            ? null
            : new GoalTemplateInfoProperties(
                GoalType: props.GoalType.ToString(),
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty,
                RegionalRecoveryPointObjective: props.RegionalRecoveryPointObjective ?? string.Empty,
                RegionalRecoveryTimeObjective: props.RegionalRecoveryTimeObjective ?? string.Empty,
                RequireDisasterRecovery: props.RequireDisasterRecovery?.ToString() ?? string.Empty,
                RequireHighAvailability: props.RequireHighAvailability?.ToString() ?? string.Empty);

        var mappedSystemData = systemData is null
            ? null
            : new GoalTemplateInfoSystemData(
                CreatedAt: systemData.CreatedOn?.ToString("o") ?? string.Empty,
                CreatedBy: systemData.CreatedBy ?? string.Empty,
                CreatedByType: systemData.CreatedByType?.ToString() ?? string.Empty,
                LastModifiedAt: systemData.LastModifiedOn?.ToString("o") ?? string.Empty,
                LastModifiedBy: systemData.LastModifiedBy ?? string.Empty,
                LastModifiedByType: systemData.LastModifiedByType?.ToString() ?? string.Empty);

        return new GoalTemplateInfo(
            Id: data.Id?.ToString() ?? string.Empty,
            Name: data.Name ?? string.Empty,
            Properties: mappedProperties,
            SystemData: mappedSystemData);
    }

    public async Task<IEnumerable<ResourceSummary>> ListGoalAssignmentsAsync(string serviceGroup, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalAssignmentCollection goalAssignments = armClient.GetGoalAssignments(serviceGroupId);

        var result = new List<ResourceSummary>();
        await foreach (var goalAssignment in goalAssignments.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: goalAssignment.Data.Id?.ToString() ?? string.Empty,
                Name: goalAssignment.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<GoalAssignmentInfo> GetGoalAssignmentAsync(string serviceGroup, string goalAssignment, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalAssignmentCollection goalAssignments = armClient.GetGoalAssignments(serviceGroupId);
        GoalAssignmentResource resource = await goalAssignments.GetAsync(goalAssignment, cancellationToken);

        return MapGoalAssignment(resource.Data);
    }

    private static GoalAssignmentInfo MapGoalAssignment(GoalAssignmentData data)
    {
        var props = data.Properties;
        var systemData = data.SystemData;

        var mappedProperties = props is null
            ? null
            : new GoalAssignmentInfoProperties(
                GoalAssignmentType: props.GoalAssignmentType.ToString(),
                GoalTemplateId: props.GoalTemplateId?.ToString() ?? string.Empty,
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty);

        var mappedSystemData = systemData is null
            ? null
            : new GoalAssignmentInfoSystemData(
                CreatedAt: systemData.CreatedOn?.ToString("o") ?? string.Empty,
                CreatedBy: systemData.CreatedBy ?? string.Empty,
                CreatedByType: systemData.CreatedByType?.ToString() ?? string.Empty,
                LastModifiedAt: systemData.LastModifiedOn?.ToString("o") ?? string.Empty,
                LastModifiedBy: systemData.LastModifiedBy ?? string.Empty,
                LastModifiedByType: systemData.LastModifiedByType?.ToString() ?? string.Empty);

        return new GoalAssignmentInfo(
            Id: data.Id?.ToString() ?? string.Empty,
            Name: data.Name ?? string.Empty,
            Properties: mappedProperties,
            SystemData: mappedSystemData);
    }

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlansAsync(string resourceGroup, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : (await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();

        var result = new List<ResourceSummary>();
        await foreach (var usagePlan in usagePlans.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: usagePlan.Data.Id?.ToString() ?? string.Empty,
                Name: usagePlan.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlansBySubscriptionAsync(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : (await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var subscriptionId2 = new ResourceIdentifier($"/subscriptions/{subscriptionId}");
        var subscriptionResource = armClient.GetSubscriptionResource(subscriptionId2);

        var result = new List<ResourceSummary>();
        await foreach (var usagePlan in subscriptionResource.GetUsagePlansAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: usagePlan.Data.Id?.ToString() ?? string.Empty,
                Name: usagePlan.Data.Name ?? string.Empty));
        }

        return result;
    }

    private static UsagePlanInfo MapUsagePlan(UsagePlanData data)
    {
        var props = data.Properties;
        var systemData = data.SystemData;

        var mappedProperties = props is null
            ? null
            : new UsagePlanInfoProperties(
                PlanType: props.PlanType.ToString() ?? string.Empty,
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty);

        var mappedSystemData = systemData is null
            ? null
            : new UsagePlanInfoSystemData(
                CreatedAt: systemData.CreatedOn?.ToString("o") ?? string.Empty,
                CreatedBy: systemData.CreatedBy ?? string.Empty,
                LastModifiedAt: systemData.LastModifiedOn?.ToString("o") ?? string.Empty,
                LastModifiedBy: systemData.LastModifiedBy ?? string.Empty);

        return new UsagePlanInfo(
            Id: data.Id?.ToString() ?? string.Empty,
            Name: data.Name ?? string.Empty,
            ResourceType: data.ResourceType.ToString(),
            Location: data.Location.Name ?? string.Empty,
            Tags: data.Tags is null ? null : new Dictionary<string, string>(data.Tags),
            Properties: mappedProperties,
            SystemData: mappedSystemData);
    }

    public async Task<UsagePlanInfo> GetUsagePlanAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : (await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();
        UsagePlanResource resource = await usagePlans.GetAsync(usagePlan, cancellationToken);

        return MapUsagePlan(resource.Data);
    }

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlanEnrollmentsAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : (await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var usagePlanId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.AzureResilienceManagement/usagePlans/{usagePlan}");
        var usagePlanResource = armClient.GetUsagePlanResource(usagePlanId);
        UsagePlanEnrollmentCollection enrollments = usagePlanResource.GetUsagePlanEnrollments();

        var result = new List<ResourceSummary>();
        await foreach (var enrollment in enrollments.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: enrollment.Data.Id?.ToString() ?? string.Empty,
                Name: enrollment.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<UsagePlanEnrollmentInfo> GetUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = _subscriptionService.IsSubscriptionId(subscription)
            ? subscription
            : (await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var usagePlanId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.AzureResilienceManagement/usagePlans/{usagePlan}");
        var usagePlanResource = armClient.GetUsagePlanResource(usagePlanId);
        UsagePlanEnrollmentCollection enrollments = usagePlanResource.GetUsagePlanEnrollments();
        UsagePlanEnrollmentResource resource = await enrollments.GetAsync(enrollment, cancellationToken);

        return MapUsagePlanEnrollment(resource.Data);
    }

    private static UsagePlanEnrollmentInfo MapUsagePlanEnrollment(UsagePlanEnrollmentData data)
    {
        var props = data.Properties;
        var systemData = data.SystemData;

        var mappedProperties = props is null
            ? null
            : new UsagePlanEnrollmentInfoProperties(
                ServiceGroupId: props.ServiceGroupId?.ToString() ?? string.Empty,
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty,
                ErrorDetails: props.ErrorDetails is null
                    ? null
                    : new UsagePlanEnrollmentInfoErrorDetails(
                        Code: props.ErrorDetails.Code ?? string.Empty,
                        Message: props.ErrorDetails.Message ?? string.Empty));

        var mappedSystemData = systemData is null
            ? null
            : new UsagePlanEnrollmentInfoSystemData(
                CreatedAt: systemData.CreatedOn?.ToString("o") ?? string.Empty,
                CreatedBy: systemData.CreatedBy ?? string.Empty,
                CreatedByType: systemData.CreatedByType?.ToString() ?? string.Empty,
                LastModifiedAt: systemData.LastModifiedOn?.ToString("o") ?? string.Empty,
                LastModifiedBy: systemData.LastModifiedBy ?? string.Empty,
                LastModifiedByType: systemData.LastModifiedByType?.ToString() ?? string.Empty);

        return new UsagePlanEnrollmentInfo(
            Id: data.Id?.ToString() ?? string.Empty,
            Name: data.Name ?? string.Empty,
            Properties: mappedProperties,
            SystemData: mappedSystemData);
    }

    public async Task<IEnumerable<ResourceSummary>> ListGoalResourcesAsync(string serviceGroup, string goalAssignment, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var goalAssignmentId = GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment);
        GoalMembersCollection goalMembers = armClient.GetGoalAssignmentResource(goalAssignmentId).GetAllGoalMembers();

        var result = new List<ResourceSummary>();
        await foreach (var goalMember in goalMembers.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: goalMember.Data.Id?.ToString() ?? string.Empty,
                Name: goalMember.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<GoalResourceInfo> GetGoalResourceAsync(string serviceGroup, string goalAssignment, string goalResource, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var goalMemberId = GoalMembersResource.CreateResourceIdentifier(serviceGroup, goalAssignment, goalResource);
        GoalMembersResource resource = await armClient.GetGoalMembersResource(goalMemberId).GetAsync(cancellationToken);

        return MapGoalResource(resource.Data);
    }

    private static GoalResourceInfo MapGoalResource(GoalMembersData data)
    {
        var props = data.Properties;
        var systemData = data.SystemData;

        var mappedProperties = props is null
            ? null
            : new GoalResourceInfoProperties(
                DisasterRecoveryAttestationStatus: props.DisasterRecoveryAttestationStatus.ToString() ?? string.Empty,
                DisasterRecoveryGoalParticipation: props.DisasterRecoveryGoalParticipation.ToString() ?? string.Empty,
                ExclusionReasonForDisasterRecoveryGoals: props.ExclusionReasonForDisasterRecoveryGoals.ToString() ?? string.Empty,
                ExclusionReasonForHighAvailabilityGoals: props.ExclusionReasonForHighAvailabilityGoals.ToString() ?? string.Empty,
                HighAvailabilityAttestationStatus: props.HighAvailabilityAttestationStatus.ToString() ?? string.Empty,
                HighAvailabilityGoalParticipation: props.HighAvailabilityGoalParticipation.ToString() ?? string.Empty,
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty,
                ResourceArmId: props.ResourceArmId?.ToString() ?? string.Empty,
                ServiceGroupMemberships: props.ServiceGroupMemberships is null || props.ServiceGroupMemberships.Count == 0
                    ? null
                    : props.ServiceGroupMemberships
                        .Select(m => new GoalResourceServiceGroupMembership(
                            MembershipType: m.MembershipType.ToString() ?? string.Empty,
                            ServiceGroupId: m.ServiceGroupId?.ToString() ?? string.Empty))
                        .ToList());

        var mappedSystemData = systemData is null
            ? null
            : new GoalResourceInfoSystemData(
                CreatedAt: systemData.CreatedOn?.ToString("o") ?? string.Empty,
                CreatedBy: systemData.CreatedBy ?? string.Empty,
                CreatedByType: systemData.CreatedByType?.ToString() ?? string.Empty,
                LastModifiedAt: systemData.LastModifiedOn?.ToString("o") ?? string.Empty,
                LastModifiedBy: systemData.LastModifiedBy ?? string.Empty,
                LastModifiedByType: systemData.LastModifiedByType?.ToString() ?? string.Empty);

        return new GoalResourceInfo(
            Id: data.Id?.ToString() ?? string.Empty,
            Name: data.Name ?? string.Empty,
            Properties: mappedProperties,
            SystemData: mappedSystemData);
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryPlansAsync(string serviceGroup, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        RecoveryPlanCollection recoveryPlans = armClient.GetRecoveryPlans(serviceGroupId);

        var result = new List<ResourceSummary>();
        await foreach (var recoveryPlan in recoveryPlans.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: recoveryPlan.Data.Id?.ToString() ?? string.Empty,
                Name: recoveryPlan.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        Response<RecoveryPlanResource> response = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryResourcesAsync(string serviceGroup, string recoveryPlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryMembersCollection recoveryMembers = recoveryPlanResource.GetAllRecoveryMembers();

        var result = new List<ResourceSummary>();
        await foreach (var recoveryMember in recoveryMembers.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: recoveryMember.Data.Id?.ToString() ?? string.Empty,
                Name: recoveryMember.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetRecoveryResourceAsync(string serviceGroup, string recoveryPlan, string recoveryResource, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryMembersCollection recoveryMembers = recoveryPlanResource.GetAllRecoveryMembers();
        Response<RecoveryMembersResource> response = await recoveryMembers.GetAsync(recoveryResource, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobsAsync(string serviceGroup, string recoveryPlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryJobCollection recoveryJobs = armClient.GetRecoveryPlanResource(recoveryPlanId).GetRecoveryJobs();

        var result = new List<ResourceSummary>();
        await foreach (var recoveryJob in recoveryJobs.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: recoveryJob.Data.Id?.ToString() ?? string.Empty,
                Name: recoveryJob.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        Response<RecoveryJobResource> response = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobResourcesAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        RecoveryJobResource recoveryJobResource = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);
        RecoveryJobTargetCollection recoveryJobTargets = recoveryJobResource.GetRecoveryJobTargets();

        var result = new List<ResourceSummary>();
        await foreach (var recoveryJobTarget in recoveryJobTargets.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: recoveryJobTarget.Data.Id?.ToString() ?? string.Empty,
                Name: recoveryJobTarget.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetRecoveryJobResourceAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string recoveryJobTarget, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryJobTargetId = RecoveryJobTargetResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob, recoveryJobTarget);
        Response<RecoveryJobTargetResource> response = await armClient.GetRecoveryJobTargetResource(recoveryJobTargetId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }
}
