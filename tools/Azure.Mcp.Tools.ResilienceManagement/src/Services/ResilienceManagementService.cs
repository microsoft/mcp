// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed class ResilienceManagementService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IResilienceManagementService
{
    public async Task<IEnumerable<ResourceSummary>> ListGoalTemplatesAsync(string serviceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<GoalTemplateInfo> GetGoalTemplateAsync(string serviceGroup, string goalTemplate, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<ResourceSummary>> ListGoalAssignmentsAsync(string serviceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<GoalAssignmentInfo> GetGoalAssignmentAsync(string serviceGroup, string goalAssignment, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

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
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

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
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();
        UsagePlanResource resource = await usagePlans.GetAsync(usagePlan, cancellationToken);

        return MapUsagePlan(resource.Data);
    }

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlanEnrollmentsAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

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
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

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

    public async Task<IEnumerable<ResourceSummary>> ListGoalResourcesAsync(string serviceGroup, string goalAssignment, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<GoalResourceInfo> GetGoalResourceAsync(string serviceGroup, string goalAssignment, string goalResource, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryPlansAsync(string serviceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<JsonElement> GetRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        Response<RecoveryPlanResource> response = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryResourcesAsync(string serviceGroup, string recoveryPlan, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<JsonElement> GetRecoveryResourceAsync(string serviceGroup, string recoveryPlan, string recoveryResource, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryMembersCollection recoveryMembers = recoveryPlanResource.GetAllRecoveryMembers();
        Response<RecoveryMembersResource> response = await recoveryMembers.GetAsync(recoveryResource, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobsAsync(string serviceGroup, string recoveryPlan, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<JsonElement> GetRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        Response<RecoveryJobResource> response = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobResourcesAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
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

    public async Task<JsonElement> GetRecoveryJobResourceAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string recoveryJobTarget, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryJobTargetId = RecoveryJobTargetResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob, recoveryJobTarget);
        Response<RecoveryJobTargetResource> response = await armClient.GetRecoveryJobTargetResource(recoveryJobTargetId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillsAsync(string serviceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = CreateServiceGroupResourceIdentifier(serviceGroup);
        ResilienceManagementDrillCollection drills = armClient.GetResilienceManagementDrills(serviceGroupId);

        var result = new List<ResourceSummary>();
        await foreach (var drill in drills.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: drill.Data.Id?.ToString() ?? string.Empty,
                Name: drill.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<DrillInfo> GetDrillAsync(string serviceGroup, string drill, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = CreateServiceGroupResourceIdentifier(serviceGroup);
        ResilienceManagementDrillCollection drills = armClient.GetResilienceManagementDrills(serviceGroupId);
        Response<ResilienceManagementDrillResource> response = await drills.GetAsync(drill, cancellationToken);

        return MapDrill(response.Value.Data);
    }

    private static DrillInfo MapDrill(ResilienceManagementDrillData data)
    {
        BinaryData serializedData = ((IPersistableModel<ResilienceManagementDrillData>)data).Write(new ModelReaderWriterOptions("W"));
        using JsonDocument document = JsonDocument.Parse(serializedData.ToMemory());
        return MapDrill(document.RootElement);
    }

    private static DrillInfo MapDrill(JsonElement root) =>
        new DrillInfo(
            Id: root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
            Name: root.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty,
            ResourceType: root.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null,
            Location: root.TryGetProperty("location", out JsonElement locationElement) ? locationElement.GetString() : null,
            Tags: GetTagsOrNull(root),
            Properties: root.TryGetProperty("properties", out JsonElement propertiesElement) ? propertiesElement.Clone() : default,
            SystemData: root.TryGetProperty("systemData", out JsonElement systemDataElement) ? systemDataElement.Clone() : default);

    public async Task<DrillInfo> CreateDrillAsync(string serviceGroup, string drill, string subscription, string region, string? resourceGroup, DrillKind drillType, DrillRbacSetupMode rbacSetupMode, string? recoveryPlan = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = CreateServiceGroupResourceIdentifier(serviceGroup);
        ResilienceManagementDrillCollection drills = armClient.GetResilienceManagementDrills(serviceGroupId);
        var associatedIdentity = new ResilienceManagementAssociatedIdentity(ManagedServiceIdentityType.SystemAssigned);
        DrillProperties properties = drillType switch
        {
            DrillKind.Zonal => new ZonalDrillProperties(),
            DrillKind.Regional => new RegionalDrillProperties(),
            _ => throw new ArgumentOutOfRangeException(nameof(drillType), drillType, "Unsupported drill type.")
        };

        properties.DrillAssetProperties = new AssetPropertiesOfDrill(subscriptionId, region)
        {
            ResourceGroup = resourceGroup
        };
        properties.ChaosResourceProperties = new ChaosResourcePropertiesOfDrill(associatedIdentity, associatedIdentity);
        properties.RbacSetupMode = rbacSetupMode switch
        {
            DrillRbacSetupMode.AutomatedCustomRole => new ResilienceManagementRbacSetupMode("AutomatedCustomRole"),
            DrillRbacSetupMode.AutomatedBuiltinRoles => new ResilienceManagementRbacSetupMode("AutomatedBuiltinRoles"),
            DrillRbacSetupMode.Manual => new ResilienceManagementRbacSetupMode("Manual"),
            _ => throw new ArgumentOutOfRangeException(nameof(rbacSetupMode), rbacSetupMode, "Unsupported RBAC setup mode.")
        };
        if (!string.IsNullOrWhiteSpace(recoveryPlan))
        {
            // The SDK model exposes no public constructor or setters for RecoveryPlanId, so the model factory is required.
            properties.RecoveryPlanProperties = ArmResilienceManagementModelFactory.RecoveryPlanPropertiesOfDrill(
                associatedIdentity,
                RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan),
                recoveryPlanResourceExcludedCount: null);
        }

        var drillData = new ResilienceManagementDrillData
        {
            Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned),
            Properties = properties
        };

        ArmOperation<ResilienceManagementDrillResource> operation = await drills.CreateOrUpdateAsync(WaitUntil.Started, drill, drillData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(operation.GetRawResponse().Content.ToMemory());
        return MapDrill(document.RootElement);
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillResourcesAsync(string serviceGroup, string drill, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillTargetCollection drillTargets = drillResource.GetDrillTargets();

        var result = new List<ResourceSummary>();
        await foreach (var drillTarget in drillTargets.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: drillTarget.Data.Id?.ToString() ?? string.Empty,
                Name: drillTarget.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<DrillResourceInfo> GetDrillResourceAsync(string serviceGroup, string drill, string drillResource, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillInstanceResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillTargetCollection drillTargets = drillInstanceResource.GetDrillTargets();
        Response<DrillTargetResource> response = await drillTargets.GetAsync(drillResource, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        JsonElement root = document.RootElement;

        return new DrillResourceInfo(
            Id: root.TryGetProperty("id", out JsonElement idElement) ? idElement.GetString() ?? string.Empty : string.Empty,
            Name: root.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty,
            ResourceType: root.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null,
            Location: root.TryGetProperty("location", out JsonElement locationElement) ? locationElement.GetString() : null,
            Tags: GetTagsOrNull(root),
            Properties: root.TryGetProperty("properties", out JsonElement propertiesElement) ? propertiesElement.Clone() : default,
            SystemData: root.TryGetProperty("systemData", out JsonElement systemDataElement) ? systemDataElement.Clone() : default);
    }

    private static ResourceIdentifier CreateServiceGroupResourceIdentifier(string serviceGroup)
    {
        // Validate the caller-supplied segment before interpolating it into a raw ARM resource path.
        if (string.IsNullOrWhiteSpace(serviceGroup) || serviceGroup.Contains('/'))
        {
            throw new ArgumentException("Service group name must be a single non-empty path segment.", nameof(serviceGroup));
        }

        return new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
    }

    private static Dictionary<string, string>? GetTagsOrNull(JsonElement root)
    {
        if (!root.TryGetProperty("tags", out JsonElement tagsElement) || tagsElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var tags = new Dictionary<string, string>();
        foreach (JsonProperty tagProperty in tagsElement.EnumerateObject())
        {
            tags[tagProperty.Name] = tagProperty.Value.GetString() ?? string.Empty;
        }

        return tags;
    }
    public async Task<UsagePlanInfo> CreateUsagePlanAsync(string resourceGroup, string usagePlan, UsagePlanKind planType, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();

        var usagePlanData = new UsagePlanData(new AzureLocation("global"))
        {
            Properties = new UsagePlanProperties
            {
                PlanType = planType switch
                {
                    UsagePlanKind.Standard => UsagePlanType.Standard,
                    UsagePlanKind.Basic => UsagePlanType.Basic,
                    _ => throw new ArgumentOutOfRangeException(nameof(planType), planType, "Unsupported plan type.")
                }
            }
        };

        ArmOperation<UsagePlanResource> operation = await usagePlans.CreateOrUpdateAsync(WaitUntil.Started, usagePlan, usagePlanData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return MapUsagePlan(operation.Value.Data);
    }

    public async Task<UsagePlanEnrollmentInfo> CreateUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string serviceGroup, string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var usagePlanId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.AzureResilienceManagement/usagePlans/{usagePlan}");
        UsagePlanEnrollmentCollection enrollments = armClient.GetUsagePlanResource(usagePlanId).GetUsagePlanEnrollments();

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        var enrollmentData = new UsagePlanEnrollmentData
        {
            Properties = new EnrollmentProperties(serviceGroupId)
        };

        ArmOperation<UsagePlanEnrollmentResource> operation = await enrollments.CreateOrUpdateAsync(WaitUntil.Started, enrollment, enrollmentData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return MapUsagePlanEnrollment(operation.Value.Data);
    }
}
