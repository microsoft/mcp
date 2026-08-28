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
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed class ResilienceManagementService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IResilienceManagementService
{
    private static readonly TimeSpan RecoveryPlanPollingInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RecoveryPlanOperationTimeout = TimeSpan.FromMinutes(10);

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

    public async Task<RecoveryPlanInfo> CreateRecoveryPlanAsync(string serviceGroup, string recoveryPlan, RecoveryPlanKind planType, string? planDescription, RecoveryPlanIdentityKind identityType, string? userAssignedIdentity = null, string? defaultGroupDescription = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        RecoveryPlanCollection recoveryPlans = armClient.GetRecoveryPlans(serviceGroupId);
        NullableResponse<RecoveryPlanResource> existingPlan = await recoveryPlans.GetIfExistsAsync(recoveryPlan, cancellationToken);
        string effectivePlanDescription = ResolveRecoveryPlanDescription(
            planDescription,
            existingPlan.HasValue ? existingPlan.Value?.Data?.Properties?.PlanDescription : null);
        RecoveryGroupsSetting? existingRecoveryGroups = existingPlan.HasValue
            ? existingPlan.Value?.Data?.Properties?.RecoveryGroupsSetting
            : null;
        RecoveryGroupsSetting recoveryGroups = CreateRecoveryGroupsSetting(existingRecoveryGroups, defaultGroupDescription, additionalGroups, defaultGroupPreActions, defaultGroupPostActions);
        ManagedServiceIdentity identity = CreateRecoveryPlanIdentity(identityType, userAssignedIdentity, existingPlan.HasValue ? existingPlan.Value?.Data?.Identity : null);
        var data = new RecoveryPlanData
        {
            Identity = identity,
            Properties = new RecoveryPlanProperties(
                new RecoveryPlanType(planType.ToString()),
                effectivePlanDescription,
                recoveryGroups)
        };

        ArmOperation<RecoveryPlanResource> operation = await recoveryPlans.CreateOrUpdateAsync(
            WaitUntil.Started,
            recoveryPlan,
            data,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);

        return CreateRecoveryPlanInfo(operation.Value.Data);
    }

    internal static RecoveryPlanInfo CreateRecoveryPlanInfo(RecoveryPlanData recoveryPlan)
    {
        RecoveryPlanProperties? properties = recoveryPlan.Properties;
        ManagedServiceIdentity? identity = recoveryPlan.Identity;
        RecoveryGroupsSetting? groups = properties?.RecoveryGroupsSetting;
        RecoveryPlanIdentityInfo? identityInfo = identity is null
            ? null
            : new RecoveryPlanIdentityInfo(
                identity.ManagedServiceIdentityType.ToString(),
                identity.UserAssignedIdentities.Keys
                    .Select(resourceId => resourceId.ToString())
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        return new RecoveryPlanInfo(
            recoveryPlan.Id?.ToString() ?? string.Empty,
            recoveryPlan.Name ?? string.Empty,
            properties?.PlanType.ToString() ?? string.Empty,
            properties?.PlanDescription ?? string.Empty,
            properties?.ProvisioningState?.ToString(),
            properties?.PlanState?.ToString(),
            identityInfo,
            CreateRecoveryPlanGroupInfo(groups?.DefaultGroup),
            groups?.AdditionalGroups.Select(CreateRecoveryPlanGroupInfo).OfType<RecoveryPlanGroupInfo>().ToList() ?? []);
    }

    private static RecoveryPlanGroupInfo? CreateRecoveryPlanGroupInfo(RecoveryGroup? group)
    {
        RecoveryGroupProperties? properties = group?.Properties;
        return properties is null
            ? null
            : new RecoveryPlanGroupInfo(properties.GroupUniqueId, properties.OrderId, properties.Description);
    }

    internal static string ResolveRecoveryPlanDescription(string? planDescription, string? existingPlanDescription)
        => planDescription ?? existingPlanDescription
            ?? throw new ArgumentException("--plan-description is required when creating a recovery plan.", nameof(planDescription));

    public async Task<bool> DeleteRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        RecoveryPlanCollection recoveryPlans = armClient.GetRecoveryPlans(serviceGroupId);
        NullableResponse<RecoveryPlanResource> existingPlan = await recoveryPlans.GetIfExistsAsync(recoveryPlan, cancellationToken);
        if (!existingPlan.HasValue || existingPlan.Value is null)
        {
            return false;
        }

        try
        {
            var operation = await existingPlan.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<RecoveryPlanUpdateResourcesResult> UpdateRecoveryPlanResourcesAsync(string serviceGroup, string recoveryPlan, UpdateRecoveryResourcesContent content, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryMembersCollection recoveryMembers = recoveryPlanResource.GetAllRecoveryMembers();
        foreach (RecoveryMembersData requestedResource in content.ResourcesToUpdate)
        {
            Response<RecoveryMembersResource> existingResource = await recoveryMembers.GetAsync(
                requestedResource.Properties.RecoveryResourceUniqueId,
                cancellationToken);
            ValidateRecoveryResourceUpdate(requestedResource, existingResource.Value.Data);
        }

        ArmOperation<UpdateRecoveryResourcesResult> operation = await recoveryPlanResource.UpdateResourcesAsync(
            WaitUntil.Started,
            Guid.NewGuid().ToString(),
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);

        return CreateRecoveryPlanUpdateResourcesResult(operation.Value.FailedResources);
    }

    public Task<RecoveryPlanValidateForFailoverResult> ValidateRecoveryPlanForFailoverAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds = null, string? userConsent = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForFailoverCoreAsync(serviceGroup, recoveryPlan, sourceLocations, selectedResourceIds, userConsent, tenant, retryPolicy, token),
            "recovery plan failover validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForFailoverResult> ValidateRecoveryPlanForFailoverCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds, string? userConsent, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        var properties = new FailoverRequestProperties(sourceLocations);
        foreach (string selectedResourceId in selectedResourceIds ?? [])
        {
            properties.SelectedResourceIds.Add(new ResourceIdentifier(selectedResourceId));
        }

        if (userConsent is not null)
        {
            properties.ExecutionConfigurationsUserConsent = new UserConsent(userConsent);
        }

        var content = new ResilienceManagementFailoverContent(FailoverDirectionTypes.FromSpecificLocations)
        {
            FailoverRequestProperties = properties
        };
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ValidateForRecoveryOperationBaseResult> operation = await recoveryPlanResource.ValidateForFailoverAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);

        return CreateRecoveryPlanValidateForFailoverResult(
            operationId,
            operation.GetRawResponse().Content,
            operation.Value.RecoveryResourceQualifications);
    }

    internal static RecoveryPlanValidateForFailoverResult CreateRecoveryPlanValidateForFailoverResult(
        string operationId,
        BinaryData operationResponse,
        IEnumerable<RecoveryResourceQualification>? fallbackQualifications)
    {
        using JsonDocument document = JsonDocument.Parse(operationResponse);
        if (!document.RootElement.TryGetProperty("properties", out JsonElement properties))
        {
            return CreateRecoveryPlanValidateForFailoverResult(operationId, fallbackQualifications);
        }

        string resultJson = properties.ValueKind switch
        {
            JsonValueKind.String => properties.GetString() ?? string.Empty,
            JsonValueKind.Object => properties.GetRawText(),
            _ => throw new JsonException("The ValidateForFailover operation result has an invalid properties value.")
        };

        using JsonDocument resultDocument = JsonDocument.Parse(resultJson);
        if (!resultDocument.RootElement.TryGetProperty("recoveryResourceQualifications", out JsonElement qualifications))
        {
            return CreateRecoveryPlanValidateForFailoverResult(operationId, fallbackQualifications);
        }

        List<RecoveryPlanFailoverQualification> mappedQualifications = qualifications
            .EnumerateArray()
            .Select(qualification =>
            {
                JsonElement resource = GetOptionalObject(qualification, "recoveryResource");
                JsonElement resourceProperties = GetOptionalObject(resource, "properties");
                JsonElement details = GetOptionalObject(qualification, "operationQualificationDetails");
                return new RecoveryPlanFailoverQualification(
                    GetOptionalString(resource, "id") ?? string.Empty,
                    GetOptionalString(resourceProperties, "recoveryResourceUniqueId") ?? GetOptionalString(resource, "name") ?? string.Empty,
                    GetOptionalString(resourceProperties, "resourceId"),
                    GetOptionalString(resourceProperties, "resourceLocation"),
                    GetOptionalString(details, "qualificationState") ?? "Unknown",
                    GetStringList(details, "notQualifiedReasons"),
                    GetStringList(resourceProperties, "resourcePhysicalZones"),
                    GetOptionalString(resourceProperties, "inclusionState"),
                    GetOptionalString(resourceProperties, "protectionStatus"),
                    GetOptionalBoolean(resourceProperties, "needsAttention"),
                    GetStringList(resourceProperties, "attentionReasons"));
            })
            .ToList();

        return new RecoveryPlanValidateForFailoverResult(operationId, mappedQualifications);
    }

    private static JsonElement GetOptionalObject(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == JsonValueKind.Object
            ? property
            : default;

    private static string? GetOptionalString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static bool? GetOptionalBoolean(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;

    private static IReadOnlyList<string> GetStringList(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                .Select(item => item.GetString()!)
                .ToList()
            : [];

    internal static RecoveryPlanValidateForFailoverResult CreateRecoveryPlanValidateForFailoverResult(
        string operationId,
        IEnumerable<RecoveryResourceQualification>? recoveryResourceQualifications)
    {
        List<RecoveryPlanFailoverQualification> qualifications = (recoveryResourceQualifications ?? [])
            .Select(qualification =>
            {
                RecoveryMembersData? resource = qualification?.RecoveryResource;
                RecoveryResourceProperties? resourceProperties = resource?.Properties;
                OperationQualificationDetails? details = qualification?.OperationQualificationDetails;
                return new RecoveryPlanFailoverQualification(
                    resource?.Id?.ToString() ?? string.Empty,
                    resourceProperties?.RecoveryResourceUniqueId ?? resource?.Name ?? string.Empty,
                    resourceProperties?.ResourceId?.ToString(),
                    resourceProperties?.ResourceLocation?.ToString(),
                    details?.QualificationState.ToString() ?? "Unknown",
                    details?.NotQualifiedReasons?.Where(reason => !string.IsNullOrWhiteSpace(reason)).ToList() ?? [],
                    resourceProperties?.ResourcePhysicalZones?.Where(zone => !string.IsNullOrWhiteSpace(zone)).ToList() ?? [],
                    resourceProperties?.InclusionState?.ToString(),
                    resourceProperties?.ProtectionStatus?.ToString(),
                    resourceProperties?.IsAttentionRequired,
                    resourceProperties?.AttentionReasons?.Where(reason => !string.IsNullOrWhiteSpace(reason)).ToList() ?? []);
            })
            .ToList();

        return new RecoveryPlanValidateForFailoverResult(operationId, qualifications);
    }

    public Task<RecoveryPlanValidateForReprotectResult> ValidateRecoveryPlanForReprotectAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForReprotectCoreAsync(serviceGroup, recoveryPlan, selectedResourceIds, tenant, retryPolicy, token),
            "recovery plan reprotect validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForReprotectResult> ValidateRecoveryPlanForReprotectCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        var content = new ReprotectContent();
        foreach (string selectedResourceId in selectedResourceIds ?? [])
        {
            content.ReprotectRequestSelectedResourceIds.Add(new ResourceIdentifier(selectedResourceId));
        }

        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ValidateForRecoveryOperationBaseResult> operation = await recoveryPlanResource.ValidateForReprotectAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);

        RecoveryPlanValidateForFailoverResult result = CreateRecoveryPlanValidateForFailoverResult(
            operationId,
            operation.GetRawResponse().Content,
            operation.Value.RecoveryResourceQualifications);
        return new RecoveryPlanValidateForReprotectResult(
            result.OperationId,
            result.RecoveryResourceQualifications);
    }

    public Task<RecoveryPlanValidateForOperationResult> ValidateRecoveryPlanForOperationAsync(string serviceGroup, string recoveryPlan, RecoveryOperationNames operationName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForOperationCoreAsync(serviceGroup, recoveryPlan, operationName, tenant, retryPolicy, token),
            "recovery plan operation validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForOperationResult> ValidateRecoveryPlanForOperationCoreAsync(string serviceGroup, string recoveryPlan, RecoveryOperationNames operationName, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        var content = new ValidateForOperationContent(operationName);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ArmResponseErrorResponseResult> operation = await recoveryPlanResource.ValidateForOperationAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);

        return CreateRecoveryPlanValidateForOperationResult(
            operationId,
            operationName.ToString(),
            operation.GetRawResponse().Content);
    }

    internal static RecoveryPlanValidateForOperationResult CreateRecoveryPlanValidateForOperationResult(
        string operationId,
        string operationName,
        BinaryData operationResponse)
    {
        using JsonDocument document = JsonDocument.Parse(operationResponse);
        JsonElement root = document.RootElement;
        if (root.TryGetProperty("properties", out JsonElement properties))
        {
            if (properties.ValueKind == JsonValueKind.String)
            {
                string resultJson = properties.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(resultJson))
                {
                    return new RecoveryPlanValidateForOperationResult(operationId, operationName, true, null, null);
                }

                using JsonDocument resultDocument = JsonDocument.Parse(resultJson);
                return CreateRecoveryPlanValidateForOperationResult(operationId, operationName, resultDocument.RootElement);
            }

            if (properties.ValueKind == JsonValueKind.Object)
            {
                return CreateRecoveryPlanValidateForOperationResult(operationId, operationName, properties);
            }
        }

        return CreateRecoveryPlanValidateForOperationResult(operationId, operationName, root);
    }

    private static RecoveryPlanValidateForOperationResult CreateRecoveryPlanValidateForOperationResult(
        string operationId,
        string operationName,
        JsonElement result)
    {
        JsonElement error = GetOptionalObject(result, "error");
        string? errorCode = GetOptionalString(error, "code");
        // RO returns no error object on success; its legacy response example uses code "None".
        // Any other error object represents a failed validation, even when its code is missing.
        bool isValid = error.ValueKind != JsonValueKind.Object ||
            string.Equals(errorCode, "None", StringComparison.OrdinalIgnoreCase);
        return new RecoveryPlanValidateForOperationResult(
            operationId,
            operationName,
            isValid,
            isValid ? null : errorCode,
            isValid ? null : GetOptionalString(error, "message"));
    }

    public Task<RecoveryPlanReadinessResult> CheckRecoveryPlanReadinessAsync(string serviceGroup, string recoveryPlan, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => CheckRecoveryPlanReadinessCoreAsync(serviceGroup, recoveryPlan, tenant, retryPolicy, token),
            "recovery plan readiness check",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanReadinessResult> CheckRecoveryPlanReadinessCoreAsync(string serviceGroup, string recoveryPlan, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryJobCollection recoveryJobs = recoveryPlanResource.GetRecoveryJobs();
        HashSet<string> existingRecoveryJobIds = await GetRecoveryJobIdsAsync(recoveryJobs, cancellationToken);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation operation = await recoveryPlanResource.CheckReadinessAsync(WaitUntil.Started, operationId, cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, cancellationToken);
        ResourceIdentifier recoveryJobResourceId = TryGetRecoveryJobResourceId(
                operation.GetRawResponse().Content,
                serviceGroup,
                recoveryPlan)
            ?? await WaitForNewReadinessJobAsync(recoveryJobs, existingRecoveryJobIds, cancellationToken);
        string recoveryJobId = recoveryJobResourceId.Name;

        RecoveryJobResource recoveryJob = armClient.GetRecoveryJobResource(recoveryJobResourceId);
        RecoveryJobResource recoveryJobResource = await WaitForRecoveryJobCompletionAsync(recoveryJob, recoveryJobId, cancellationToken);

        RecoveryJobProperties recoveryJobProperties = GetRequiredProperties(
            recoveryJobResource.Data.Properties,
            "recovery job");
        string status = recoveryJobProperties.Status?.ToString() ?? string.Empty;
        RecoveryPlanReadinessError? error = CreateReadinessError(recoveryJobProperties.ErrorDetails);
        List<RecoveryPlanReadinessFailedTask> failedTasks = GetFailedTasks(recoveryJobProperties.JobExtendedInfo?.TasksList);
        List<RecoveryPlanReadinessFailedResource> failedResources = [];

        await foreach (RecoveryJobTargetResource recoveryJobTarget in recoveryJobResource.GetRecoveryJobTargets().GetAllAsync(cancellationToken: cancellationToken))
        {
            RecoveryJobResourceProperties properties = GetRequiredProperties(
                recoveryJobTarget.Data.Properties,
                "recovery job target");
            string targetStatus = properties.Status?.ToString() ?? string.Empty;
            if (!string.Equals(targetStatus, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                failedResources.Add(new RecoveryPlanReadinessFailedResource(
                    recoveryJobTarget.Data.Id?.ToString() ?? string.Empty,
                    properties.ResourceId?.ToString(),
                    targetStatus,
                    properties.TaskName,
                    CreateReadinessError(properties.ErrorDetails)));
            }
        }

        bool isReady = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) &&
            failedTasks.Count == 0 && failedResources.Count == 0;

        return new RecoveryPlanReadinessResult(
            operationId,
            recoveryJobId,
            isReady,
            status,
            error,
            failedTasks,
            failedResources);
    }

    internal static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationDescription,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            return await operation(timeoutCancellation.Token);
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The {operationDescription} did not complete within {timeout.TotalMinutes} minutes.");
        }
    }

    private static async Task WaitForRecoveryPlanLroCompletionAsync(Operation operation, CancellationToken cancellationToken)
    {
#if DEBUG
        if (EnvironmentHelpers.IsPlaybackTesting())
        {
            await WaitForLroCompletionAsync(operation, cancellationToken);
            return;
        }
#endif

        while (true)
        {
            _ = await operation.UpdateStatusAsync(cancellationToken);
            if (operation.HasCompleted)
            {
                return;
            }

            await Task.Delay(RecoveryPlanPollingInterval, cancellationToken);
        }
    }

    internal static ResourceIdentifier GetRecoveryJobResourceId(BinaryData responseContent, string serviceGroup, string recoveryPlan)
    {
        return TryGetRecoveryJobResourceId(responseContent, serviceGroup, recoveryPlan)
            ?? throw new InvalidOperationException("The readiness operation completed without returning a recovery job identifier.");
    }

    internal static ResourceIdentifier? TryGetRecoveryJobResourceId(BinaryData responseContent, string serviceGroup, string recoveryPlan)
    {
        try
        {
            RecoveryPlanActionBaseResult? result = ModelReaderWriter.Read<RecoveryPlanActionBaseResult>(
                responseContent,
                ModelReaderWriterOptions.Json,
                AzureResourceManagerResilienceManagementContext.Default);
            if (string.IsNullOrWhiteSpace(result?.JobId))
            {
                return null;
            }

            ResourceIdentifier recoveryJobResourceId = result.JobId.StartsWith("/", StringComparison.Ordinal)
                ? new ResourceIdentifier(result.JobId)
                : RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, result.JobId);
            var expectedJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJobResourceId.Name);
            if (!Guid.TryParseExact(recoveryJobResourceId.Name, "D", out _) ||
                recoveryJobResourceId.ResourceType != RecoveryJobResource.ResourceType ||
                !string.Equals(recoveryJobResourceId.Parent?.ToString(), expectedJobId.Parent?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The readiness operation returned an invalid recovery job identifier.");
            }

            return recoveryJobResourceId;
        }
        catch (Exception ex) when (ex is JsonException or FormatException or ArgumentException)
        {
            throw new InvalidOperationException("The readiness operation returned an invalid recovery job identifier.", ex);
        }
    }

    private static async Task<HashSet<string>> GetRecoveryJobIdsAsync(RecoveryJobCollection recoveryJobs, CancellationToken cancellationToken)
    {
        HashSet<string> recoveryJobIds = new(StringComparer.OrdinalIgnoreCase);
        await foreach (RecoveryJobResource recoveryJob in recoveryJobs.GetAllAsync(cancellationToken: cancellationToken))
        {
            recoveryJobIds.Add(recoveryJob.Data.Id.ToString());
        }

        return recoveryJobIds;
    }

    private static async Task<ResourceIdentifier> WaitForNewReadinessJobAsync(
        RecoveryJobCollection recoveryJobs,
        HashSet<string> existingRecoveryJobIds,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            List<ResourceIdentifier> matchingJobIds = [];
            await foreach (RecoveryJobResource recoveryJob in recoveryJobs.GetAllAsync(cancellationToken: cancellationToken))
            {
                if (!existingRecoveryJobIds.Contains(recoveryJob.Data.Id.ToString()) &&
                    string.Equals(recoveryJob.Data.Properties?.Operation, "CheckRecoveryPlanReadiness", StringComparison.OrdinalIgnoreCase))
                {
                    matchingJobIds.Add(recoveryJob.Data.Id);
                }
            }

            if (matchingJobIds.Count == 1)
            {
                return matchingJobIds[0];
            }

            if (matchingJobIds.Count > 1)
            {
                throw new InvalidOperationException("Multiple recovery jobs were created by concurrent readiness operations.");
            }

            await Task.Delay(RecoveryPlanPollingInterval, cancellationToken);
        }
    }

    internal static bool IsTerminalJobStatus(string status) =>
        status.Equals("NotApplicable", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("CompletedWithWarnings", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("Skipped", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

    private static async Task<RecoveryJobResource> WaitForRecoveryJobCompletionAsync(
        RecoveryJobResource recoveryJob,
        string recoveryJobId,
        CancellationToken cancellationToken)
    {
        return await WaitForCompletionAsync(
            token => GetRecoveryJobIfAvailableAsync(recoveryJob, token),
            job => IsTerminalJobStatus(GetRequiredProperties(job.Data.Properties, "recovery job").Status?.ToString() ?? string.Empty),
            $"readiness recovery job '{recoveryJobId}'",
            RecoveryPlanPollingInterval,
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    internal static T GetRequiredProperties<T>(T? properties, string resourceDescription) where T : class
    {
        return properties ?? throw new InvalidOperationException(
            $"The readiness operation returned a {resourceDescription} without required properties.");
    }

    internal static async Task<T> WaitForCompletionAsync<T>(
        Func<CancellationToken, Task<T?>> getCurrent,
        Func<T, bool> isComplete,
        string operationDescription,
        TimeSpan pollingInterval,
        TimeSpan timeout,
        CancellationToken cancellationToken)
        where T : class
    {
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(timeout);

        try
        {
            while (true)
            {
                T? current = await getCurrent(timeoutCancellation.Token);
                if (current is not null && isComplete(current))
                {
                    return current;
                }

                await Task.Delay(pollingInterval, timeoutCancellation.Token);
            }
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"The {operationDescription} did not complete within {timeout.TotalMinutes} minutes.");
        }
    }

    private static async Task<RecoveryJobResource?> GetRecoveryJobIfAvailableAsync(
        RecoveryJobResource recoveryJob,
        CancellationToken cancellationToken)
    {
        try
        {
            Response<RecoveryJobResource> response = await recoveryJob.GetAsync(cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    internal static RecoveryPlanReadinessError? CreateReadinessError(JobErrorInfo? error)
    {
        return error is null
            ? null
            : new RecoveryPlanReadinessError(error.ErrorCode, error.ErrorMessage, error.Recommendations ?? []);
    }

    private static List<RecoveryPlanReadinessFailedTask> GetFailedTasks(IReadOnlyList<JobTaskDetail>? tasks)
    {
        var failedTasks = new List<RecoveryPlanReadinessFailedTask>();
        if (tasks is not null)
        {
            AddFailedTasks(tasks, failedTasks);
        }

        return failedTasks;
    }

    private static void AddFailedTasks(IReadOnlyList<JobTaskDetail> tasks, List<RecoveryPlanReadinessFailedTask> failedTasks)
    {
        foreach (JobTaskDetail task in tasks)
        {
            string status = task.Status?.ToString() ?? string.Empty;
            if (string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                failedTasks.Add(new RecoveryPlanReadinessFailedTask(
                    task.TaskId,
                    task.TaskName,
                    status,
                    CreateReadinessError(task.ErrorDetails)));
            }

            if (task.SubTasksList.Count > 0)
            {
                AddFailedTasks(task.SubTasksList, failedTasks);
            }
        }
    }

    internal static RecoveryPlanUpdateResourcesResult CreateRecoveryPlanUpdateResourcesResult(IEnumerable<RecoveryMembersData> failedResources)
    {
        List<RecoveryPlanUpdateResourcesFailedResource> resources = failedResources.Select(resource =>
        {
            RecoveryResourceProperties? properties = resource.Properties;
            ResponseError? error = properties?.ErrorDetails;
            RecoveryPlanUpdateResourcesError? resultError = error is null
                ? null
                : new RecoveryPlanUpdateResourcesError(error.Code, error.Message);

            return new RecoveryPlanUpdateResourcesFailedResource(
                resource.Id?.ToString() ?? string.Empty,
                properties?.RecoveryResourceUniqueId ?? resource.Name ?? string.Empty,
                properties?.ResourceId?.ToString(),
                resultError);
        }).ToList();

        return new RecoveryPlanUpdateResourcesResult(resources);
    }

    internal static void ValidateRecoveryResourceUpdate(RecoveryMembersData requestedResource, RecoveryMembersData existingResource)
    {
        RecoveryResourceProperties requested = requestedResource.Properties
            ?? throw new ArgumentException("Each recovery resource update must contain properties.");
        RecoveryResourceProperties existing = existingResource.Properties
            ?? throw new ArgumentException($"Recovery resource '{requested.RecoveryResourceUniqueId}' has no existing properties.");
        ResourceInclusionState? effectiveInclusionState = requested.InclusionState ?? existing.InclusionState;
        if (effectiveInclusionState == ResourceInclusionState.Excluded)
        {
            return;
        }

        if (effectiveInclusionState != ResourceInclusionState.Included)
        {
            return;
        }

        ResourceProtectionSolutionType? effectiveSolutionType = IsNullOrNone(requested.SelectedProtectionSolutionType)
            ? existing.SelectedProtectionSolutionType
            : requested.SelectedProtectionSolutionType;
        ResourceBaseProtectionSolutionSetting? effectiveSolutionSetting = requested.SelectedProtectionSolutionSetting ?? existing.SelectedProtectionSolutionSetting;
        string resourceId = requested.RecoveryResourceUniqueId;

        if (effectiveSolutionType is null || effectiveSolutionType == ResourceProtectionSolutionType.None || effectiveSolutionSetting is null)
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' requires selectedProtectionSolutionType and selectedProtectionSolutionSetting when it is first included.");
        }

        if (effectiveSolutionType == ResourceProtectionSolutionType.AzureNative)
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' cannot use AzureNative when included. Use AzureSiteRecovery for a virtual machine or CustomRunbook for another resource type.");
        }

        if (effectiveSolutionType == ResourceProtectionSolutionType.CustomRunbook)
        {
            if (effectiveSolutionSetting is not ResourceCustomProtectionSetting customRunbookSetting)
            {
                throw new ArgumentException($"Recovery resource '{resourceId}' requires a CustomRunbook selectedProtectionSolutionSetting that matches selectedProtectionSolutionType.");
            }

            if (customRunbookSetting.FailoverActionResourceId is null || customRunbookSetting.ReprotectActionResourceId is null)
            {
                throw new ArgumentException($"Recovery resource '{resourceId}' requires failoverAction.resourceId and reprotectAction.resourceId in its CustomRunbook selectedProtectionSolutionSetting.");
            }

            ValidateResourceType(customRunbookSetting.FailoverActionResourceId, "Microsoft.Automation/automationAccounts/runbooks", resourceId, "failoverAction.resourceId");
            ValidateResourceType(customRunbookSetting.FailoverCommitActionResourceId, "Microsoft.Automation/automationAccounts/runbooks", resourceId, "failoverCommitAction.resourceId");
            ValidateResourceType(customRunbookSetting.TestFailoverActionResourceId, "Microsoft.Automation/automationAccounts/runbooks", resourceId, "testFailoverAction.resourceId");
            ValidateResourceType(customRunbookSetting.TestFailoverCleanupActionResourceId, "Microsoft.Automation/automationAccounts/runbooks", resourceId, "testFailoverCleanupAction.resourceId");
            ValidateResourceType(customRunbookSetting.ReprotectActionResourceId, "Microsoft.Automation/automationAccounts/runbooks", resourceId, "reprotectAction.resourceId");
            return;
        }

        if (effectiveSolutionType != ResourceProtectionSolutionType.AzureSiteRecovery || effectiveSolutionSetting is not ResourceSiteRecoveryProtectionSetting siteRecoverySetting)
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' has inconsistent selectedProtectionSolutionType and selectedProtectionSolutionSetting values.");
        }

        if (!string.Equals(existing.ResourceId?.ResourceType.ToString(), "Microsoft.Compute/virtualMachines", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' can use AzureSiteRecovery only for a Microsoft.Compute/virtualMachines resource. Use CustomRunbook for this resource type.");
        }

        if (siteRecoverySetting.DiskReprotectInputDetails.Count == 0 ||
            siteRecoverySetting.DiskReprotectInputDetails.Any(detail => detail.DiskResourceId is null || detail.StagingStorageAccountResourceId is null))
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' requires at least one diskReprotectInputDetails entry with diskResourceId and stagingStorageAccountResourceId in its AzureSiteRecovery selectedProtectionSolutionSetting.");
        }

        foreach (DiskReprotectInputDetails detail in siteRecoverySetting.DiskReprotectInputDetails)
        {
            ValidateResourceType(detail.DiskResourceId, "Microsoft.Compute/disks", resourceId, "diskReprotectInputDetails.diskResourceId");
            ValidateResourceType(detail.StagingStorageAccountResourceId, "Microsoft.Storage/storageAccounts", resourceId, "diskReprotectInputDetails.stagingStorageAccountResourceId");
        }

        if (siteRecoverySetting.TestFailoverParamsNetworkResourceId is null)
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' requires testFailoverParams.networkResourceId in its AzureSiteRecovery selectedProtectionSolutionSetting.");
        }

        ValidateResourceType(siteRecoverySetting.TestFailoverParamsNetworkResourceId, "Microsoft.Network/virtualNetworks", resourceId, "testFailoverParams.networkResourceId");

        if (!existing.ResourceProtectionSolutions.Any(solution =>
            solution.ProtectionSolutionType == ResourceProtectionSolutionType.AzureSiteRecovery &&
            solution.ProtectionStatus == ResourceProtectionStatus.Protected))
        {
            throw new ArgumentException($"Recovery resource '{resourceId}' cannot use AzureSiteRecovery because healthy Azure Site Recovery protection was not detected. Enable Azure Site Recovery protection, wait until its status is Protected, and refresh the recovery plan.");
        }
    }

    private static bool IsNullOrNone(ResourceProtectionSolutionType? solutionType)
        => solutionType is null || solutionType == ResourceProtectionSolutionType.None || string.IsNullOrEmpty(solutionType.Value.ToString());

    private static void ValidateResourceType(ResourceIdentifier? resourceIdentifier, string expectedResourceType, string recoveryResourceId, string propertyName)
    {
        if (resourceIdentifier is not null &&
            !string.Equals(resourceIdentifier.ResourceType.ToString(), expectedResourceType, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Recovery resource '{recoveryResourceId}' requires {propertyName} to identify a {expectedResourceType} resource.");
        }
    }

    internal static RecoveryGroupsSetting CreateRecoveryGroupsSetting(RecoveryGroupsSetting? existingRecoveryGroups, string? defaultGroupDescription, IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = null)
    {
        RecoveryGroup? existingDefaultGroup = existingRecoveryGroups?.DefaultGroup;
        RecoveryGroupsSetting recoveryGroups;
        if (existingDefaultGroup?.Properties is { } existingDefaultGroupProperties)
        {
            if (defaultGroupDescription is not null)
            {
                existingDefaultGroupProperties.Description = defaultGroupDescription;
            }

            recoveryGroups = existingRecoveryGroups!;
        }
        else
        {
            var defaultGroup = new RecoveryGroup
            {
                Properties = new RecoveryGroupProperties(Guid.NewGuid().ToString(), 0, defaultGroupDescription ?? "Default recovery group")
            };
            recoveryGroups = new RecoveryGroupsSetting(defaultGroup);
        }

        ApplyRecoveryGroupActions(recoveryGroups.DefaultGroup.Properties!.PreActions, defaultGroupPreActions);
        ApplyRecoveryGroupActions(recoveryGroups.DefaultGroup.Properties.PostActions, defaultGroupPostActions);

        if (additionalGroups is null)
        {
            return recoveryGroups;
        }

        RecoveryGroup[] existingAdditionalGroups = recoveryGroups.AdditionalGroups.ToArray();
        recoveryGroups.AdditionalGroups.Clear();
        foreach (RecoveryPlanGroupInput groupInput in additionalGroups.OrderBy(group => group.OrderId))
        {
            RecoveryGroup? existingGroup = groupInput.GroupUniqueId is not null
                ? existingAdditionalGroups.FirstOrDefault(group => string.Equals(group.Properties?.GroupUniqueId, groupInput.GroupUniqueId, StringComparison.OrdinalIgnoreCase))
                : existingAdditionalGroups.FirstOrDefault(group => group.Properties?.OrderId == groupInput.OrderId);
            if (existingGroup?.Properties is { } existingProperties)
            {
                existingProperties.OrderId = groupInput.OrderId;
                existingProperties.Description = groupInput.Description;
                ApplyRecoveryGroupActions(existingProperties.PreActions, groupInput.PreActions);
                ApplyRecoveryGroupActions(existingProperties.PostActions, groupInput.PostActions);
                recoveryGroups.AdditionalGroups.Add(existingGroup);
                continue;
            }

            var newGroup = new RecoveryGroup
            {
                Properties = new RecoveryGroupProperties(groupInput.GroupUniqueId ?? Guid.NewGuid().ToString(), groupInput.OrderId, groupInput.Description)
            };
            ApplyRecoveryGroupActions(newGroup.Properties.PreActions, groupInput.PreActions);
            ApplyRecoveryGroupActions(newGroup.Properties.PostActions, groupInput.PostActions);
            recoveryGroups.AdditionalGroups.Add(newGroup);
        }

        ValidateRecoveryGroupIdentifiers(recoveryGroups);

        return recoveryGroups;
    }

    private static void ValidateRecoveryGroupIdentifiers(RecoveryGroupsSetting recoveryGroups)
    {
        string defaultGroupUniqueId = recoveryGroups.DefaultGroup.Properties!.GroupUniqueId;
        var additionalGroupUniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (RecoveryGroup additionalGroup in recoveryGroups.AdditionalGroups)
        {
            string? additionalGroupUniqueId = additionalGroup.Properties?.GroupUniqueId;
            if (string.Equals(additionalGroupUniqueId, defaultGroupUniqueId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("An additional recovery group groupUniqueId cannot match the default recovery group groupUniqueId.");
            }

            if (additionalGroupUniqueId is not null && !additionalGroupUniqueIds.Add(additionalGroupUniqueId))
            {
                throw new ArgumentException("Additional recovery group groupUniqueId values must be unique.");
            }
        }
    }

    private static void ApplyRecoveryGroupActions(IList<RecoveryGroupBaseAction> target, IReadOnlyList<RecoveryPlanGroupActionInput>? actions)
    {
        if (actions is null)
        {
            return;
        }

        target.Clear();
        foreach (RecoveryPlanGroupActionInput action in actions)
        {
            RecoveryGroupBaseAction sdkAction = action.Type switch
            {
                RecoveryPlanGroupActionKind.ManualAction => new RecoveryGroupManualAction(action.Name, action.TimeoutInMinutes),
                RecoveryPlanGroupActionKind.CustomRunbook => CreateCustomRunbookAction(action),
                _ => throw new ArgumentOutOfRangeException(nameof(action), action.Type, "Unsupported recovery group action type.")
            };
            sdkAction.Description = action.Description;
            target.Add(sdkAction);
        }
    }

    private static RecoveryGroupCustomRunbookAction CreateCustomRunbookAction(RecoveryPlanGroupActionInput action)
    {
        var sdkAction = new RecoveryGroupCustomRunbookAction(action.Name, action.TimeoutInMinutes)
        {
            ActionResourceId = new ResourceIdentifier(action.ActionResourceId!)
        };
        foreach ((string name, string value) in action.Parameters ?? new Dictionary<string, string>())
        {
            sdkAction.Parameters.Add(name, value);
        }

        return sdkAction;
    }

    internal static ManagedServiceIdentity CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind identityType, string? userAssignedIdentity, ManagedServiceIdentity? existingIdentity = null)
    {
        if (identityType == RecoveryPlanIdentityKind.SystemAssigned)
        {
            return new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned);
        }

        ResourceIdentifier identityResourceId = ParseUserAssignedIdentityResourceId(userAssignedIdentity!);
        if (existingIdentity?.UserAssignedIdentities.Keys.Any(existingIdentityResourceId => existingIdentityResourceId != identityResourceId) == true)
        {
            throw new ArgumentException("Changing the user-assigned managed identity of an existing recovery plan is not supported.", nameof(userAssignedIdentity));
        }

        ManagedServiceIdentityType managedServiceIdentityType = identityType == RecoveryPlanIdentityKind.SystemAndUserAssigned
            ? ManagedServiceIdentityType.SystemAssignedUserAssigned
            : ManagedServiceIdentityType.UserAssigned;
        var identity = new ManagedServiceIdentity(managedServiceIdentityType);
        foreach (ResourceIdentifier existingIdentityResourceId in existingIdentity?.UserAssignedIdentities.Keys ?? [])
        {
            if (existingIdentityResourceId != identityResourceId)
            {
                identity.UserAssignedIdentities.Add(existingIdentityResourceId, null!);
            }
        }

        identity.UserAssignedIdentities.Add(identityResourceId, new UserAssignedIdentity());
        return identity;
    }

    internal static ResourceIdentifier ParseUserAssignedIdentityResourceId(string userAssignedIdentity)
    {
        try
        {
            var identityResourceId = new ResourceIdentifier(userAssignedIdentity);
            if (!string.Equals(identityResourceId.ResourceType.ToString(), "Microsoft.ManagedIdentity/userAssignedIdentities", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(identityResourceId.SubscriptionId) ||
                string.IsNullOrWhiteSpace(identityResourceId.ResourceGroupName) ||
                string.IsNullOrWhiteSpace(identityResourceId.Name))
            {
                throw new FormatException();
            }

            return identityResourceId;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            throw new ArgumentException(
                "The user-assigned identity must be a valid Azure resource ID in the format /subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identity}.",
                nameof(userAssignedIdentity));
        }
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
        BinaryData serializedData = ((IPersistableModel<ResilienceManagementDrillData>)data).Write(new ModelReaderWriterOptions("J"));
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
        Response<ResilienceManagementDrillResource> completedDrill = await drills.GetAsync(drill, cancellationToken);

        return MapDrill(completedDrill.Value.Data);
    }

    public async Task<DrillInfo> UpdateDrillAsync(string serviceGroup, string drill, string? subscription = null, string? region = null, DrillRbacSetupMode? rbacSetupMode = null, string? recoveryPlan = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = subscription is null || AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        var properties = new DrillUpdateProperties();
        var associatedIdentity = new ResilienceManagementAssociatedIdentity(ManagedServiceIdentityType.SystemAssigned);

        if (subscriptionId is not null && region is not null)
        {
            properties.DrillAssetProperties = new AssetPropertiesOfDrill(subscriptionId, region);
        }

        if (rbacSetupMode is not null)
        {
            properties.RbacSetupMode = new ResilienceManagementRbacSetupMode(rbacSetupMode.Value.ToString());
        }

        if (recoveryPlan is not null)
        {
            properties.RecoveryPlanProperties = ArmResilienceManagementModelFactory.RecoveryPlanPropertiesOfDrill(
                associatedIdentity,
                RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan),
                recoveryPlanResourceExcludedCount: null);
        }

        var patch = new ResilienceManagementDrillPatch
        {
            Properties = properties
        };

        ArmOperation operation = await drillResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        Response<ResilienceManagementDrillResource> response = await drillResource.GetAsync(cancellationToken);

        return MapDrill(response.Value.Data);
    }

    public async Task DeleteDrillAsync(string serviceGroup, string drill, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        await drillResource.DeleteAsync(WaitUntil.Completed, cancellationToken);
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

    public async Task<IEnumerable<ResourceSummary>> ListDrillRunsAsync(string serviceGroup, string drill, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillRunCollection drillRuns = drillResource.GetDrillRuns();

        var result = new List<ResourceSummary>();
        await foreach (var drillRun in drillRuns.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: drillRun.Data.Id?.ToString() ?? string.Empty,
                Name: drillRun.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetDrillRunAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillRunCollection drillRuns = drillResource.GetDrillRuns();
        Response<DrillRunResource> response = await drillRuns.GetAsync(drillRun, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillRunResourcesAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        DrillRunTargetCollection drillRunTargets = drillRunResource.GetDrillRunTargets();

        var result = new List<ResourceSummary>();
        await foreach (var drillRunTarget in drillRunTargets.GetAllAsync(cancellationToken: cancellationToken))
        {
            result.Add(new ResourceSummary(
                Id: drillRunTarget.Data.Id?.ToString() ?? string.Empty,
                Name: drillRunTarget.Data.Name ?? string.Empty));
        }

        return result;
    }

    public async Task<JsonElement> GetDrillRunResourceAsync(string serviceGroup, string drill, string drillRun, string drillRunResource, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, retryPolicy: retryPolicy, cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResourceInstance = armClient.GetDrillRunResource(drillRunId);
        DrillRunTargetCollection drillRunTargets = drillRunResourceInstance.GetDrillRunTargets();
        Response<DrillRunTargetResource> response = await drillRunTargets.GetAsync(drillRunResource, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
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
