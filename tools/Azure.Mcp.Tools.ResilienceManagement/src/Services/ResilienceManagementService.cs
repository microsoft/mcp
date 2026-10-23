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

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed partial class ResilienceManagementService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IResilienceManagementService
{
    private static readonly TimeSpan RecoveryPlanPollingInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RecoveryPlanOperationTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan UsagePlanOperationTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan DrillReportOperationTimeout = TimeSpan.FromMinutes(5);

    // The Drills backend reads the per-operation id from the operationId query parameter, but the generated SDK only
    // emits the operation-id header. Install a policy that mirrors the header into the query for long-running operations.
    private static ArmClientOptions CreateArmClientOptionsWithOperationIdPolicy()
    {
        var options = new ArmClientOptions();
        options.AddPolicy(new OperationIdQueryParameterPolicy(), HttpPipelinePosition.PerCall);
        return options;
    }

    public async Task<IEnumerable<ResourceSummary>> ListGoalAssignmentsAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<GoalAssignmentInfo> GetGoalAssignmentAsync(string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalAssignmentCollection goalAssignments = armClient.GetGoalAssignments(serviceGroupId);
        Response<GoalAssignmentResource> response = await goalAssignments.GetAsync(goalAssignment, cancellationToken);

        return MapGoalAssignment(response.Value.Data, response.GetRawResponse().Content);
    }

    internal static GoalAssignmentInfo MapGoalAssignment(GoalAssignmentData data, BinaryData responseContent)
    {
        var props = data.Properties;
        var systemData = data.SystemData;
        using JsonDocument document = JsonDocument.Parse(responseContent);
        JsonElement rawProperties = GetOptionalObject(document.RootElement, "properties");

        var mappedProperties = props is null
            ? null
            : new GoalAssignmentInfoProperties(
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty,
                RequireZonalResiliency: GetOptionalBoolean(rawProperties, "requireZonalResiliency"),
                ServiceLevelResources: HasOptionalValue(rawProperties, "serviceLevelResources", JsonValueKind.Array)
                    ? props.ServiceLevelResources
                        .Select(target => new GoalServiceLevelTargetInfo(target.ServiceLevelIndicatorResourceId?.ToString() ?? string.Empty))
                        .ToList()
                    : null,
                ErrorDetails: GetOptionalRawValue(rawProperties, "errorDetails", JsonValueKind.Object));

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

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlansAsync(string resourceGroup, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlansBySubscriptionAsync(string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<UsagePlanInfo> GetUsagePlanAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();
        UsagePlanResource resource = await usagePlans.GetAsync(usagePlan, cancellationToken);

        return MapUsagePlan(resource.Data);
    }

    public async Task<IEnumerable<ResourceSummary>> ListUsagePlanEnrollmentsAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<UsagePlanEnrollmentInfo> GetUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<IEnumerable<ResourceSummary>> ListGoalResourcesAsync(string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<GoalResourceInfo> GetGoalResourceAsync(string serviceGroup, string goalAssignment, string goalResource, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var goalMemberId = GoalMembersResource.CreateResourceIdentifier(serviceGroup, goalAssignment, goalResource);
        Response<GoalMembersResource> response = await armClient.GetGoalMembersResource(goalMemberId).GetAsync(cancellationToken);

        return MapGoalResource(response.Value.Data, response.GetRawResponse().Content);
    }

    internal static GoalResourceInfo MapGoalResource(GoalMembersData data, BinaryData responseContent)
    {
        var props = data.Properties;
        var systemData = data.SystemData;
        var mappedProperties = props is null
            ? null
            : new GoalResourceInfoProperties(
                ProvisioningState: props.ProvisioningState?.ToString() ?? string.Empty,
                ResourceArmId: props.ResourceArmId?.ToString() ?? string.Empty,
                ZonalResiliency: props.ZonalResiliency is not { } zonal ? null : new GoalResourceResiliencyInfo(
                    zonal.GoalParticipation?.ToString(),
                    zonal.AttestationStatus?.ToString(),
                    zonal.ExclusionReason?.ToString(),
                    zonal.UserConfirmation.Select(c => new GoalResourceConfirmationInfo(
                        c.SolutionDisplayName.ToString(), c.ConfirmationStatus.ToString(),
                        c.ReasonForRequestingConfirmation?.ToString())).ToList()));

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

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryPlansAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        Response<RecoveryPlanResource> response = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<RecoveryPlanInfo> CreateRecoveryPlanAsync(string serviceGroup, string recoveryPlan, RecoveryPlanKind planType, string? planDescription, RecoveryPlanIdentityKind identityType, string? userAssignedIdentity = null, string? defaultGroupDescription = null, string? tenant = null, IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = null, string? defaultGroupUniqueId = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        RecoveryPlanCollection recoveryPlans = armClient.GetRecoveryPlans(serviceGroupId);
        NullableResponse<RecoveryPlanResource> existingPlan = await recoveryPlans.GetIfExistsAsync(recoveryPlan, cancellationToken);
        string effectivePlanDescription = ResolveRecoveryPlanDescription(
            planDescription,
            existingPlan.HasValue ? existingPlan.Value?.Data?.Properties?.PlanDescription : null);
        RecoveryGroupsSetting? existingRecoveryGroups = existingPlan.HasValue
            ? existingPlan.Value?.Data?.Properties?.RecoveryGroupsSetting
            : null;
        RecoveryGroupsSetting recoveryGroups = CreateRecoveryGroupsSetting(existingRecoveryGroups, defaultGroupDescription, additionalGroups, defaultGroupPreActions, defaultGroupPostActions, defaultGroupUniqueId);
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
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);

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
            ?? throw new ArgumentException("--plan-description is required when creating a recoveryplan.", nameof(planDescription));

    public async Task<bool> DeleteRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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
            await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<RecoveryPlanUpdateResourcesResult> UpdateRecoveryPlanResourcesAsync(string serviceGroup, string recoveryPlan, UpdateRecoveryResourcesContent content, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);

        return CreateRecoveryPlanUpdateResourcesResult(operation.Value.FailedResources);
    }

    public Task<RecoveryPlanFailoverResult> FailoverRecoveryPlanAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds = null, string? userConsent = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => FailoverRecoveryPlanCoreAsync(serviceGroup, recoveryPlan, sourceLocations, selectedResourceIds, userConsent, tenant, token),
            "recoveryplan failover request",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanFailoverResult> FailoverRecoveryPlanCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds, string? userConsent, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        ResilienceManagementFailoverContent content = CreateFailoverRequestContent(sourceLocations, selectedResourceIds, userConsent);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<RecoveryPlanActionBaseResult> operation = await recoveryPlanResource.FailoverAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        string? jobId = operation.HasCompleted ? operation.Value.JobId : null;

        return new RecoveryPlanFailoverResult(
            operationId,
            jobId,
            "Accepted",
            CreateRecoveryPlanActionTrackingMessage("Failover", jobId));
    }

    public Task<RecoveryPlanFinalizeResult> FinalizeRecoveryPlanAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => FinalizeRecoveryPlanCoreAsync(serviceGroup, recoveryPlan, tenant, token),
            "recoveryplan finalize request",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanFinalizeResult> FinalizeRecoveryPlanCoreAsync(string serviceGroup, string recoveryPlan, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ArmResponseErrorResponseResult> operation = await recoveryPlanResource.FinalizeAsync(
            WaitUntil.Started,
            operationId,
            cancellationToken);
        try
        {
            await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 200)
        {
            // RO can complete finalize with warnings that the SDK surfaces as a status-200 exception.
            RecoveryPlanResource finalizedRecoveryPlan = await recoveryPlanResource.GetAsync(cancellationToken);
            RecoveryPlanState? planState = finalizedRecoveryPlan.Data.Properties?.PlanState;
            if (planState != RecoveryPlanState.Ready && planState != RecoveryPlanState.Warning)
            {
                throw;
            }
        }

        ThrowIfProviderError(operation.Value, "Recovery plan finalize");

        return new RecoveryPlanFinalizeResult(operationId);
    }

    public Task<RecoveryPlanReprotectResult> ReprotectRecoveryPlanAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ReprotectRecoveryPlanCoreAsync(serviceGroup, recoveryPlan, selectedResourceIds, tenant, token),
            "recoveryplan reprotect request",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanReprotectResult> ReprotectRecoveryPlanCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        ReprotectContent content = CreateReprotectRequestContent(selectedResourceIds);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<RecoveryPlanActionBaseResult> operation = await recoveryPlanResource.ReprotectAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        string? jobId = operation.HasCompleted ? operation.Value.JobId : null;

        return new RecoveryPlanReprotectResult(
            operationId,
            jobId,
            "Accepted",
            CreateRecoveryPlanActionTrackingMessage("Reprotect", jobId));
    }

    public Task<RecoveryPlanValidateForFailoverResult> ValidateRecoveryPlanForFailoverAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds = null, string? userConsent = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForFailoverCoreAsync(serviceGroup, recoveryPlan, sourceLocations, selectedResourceIds, userConsent, tenant, token),
            "recoveryplan failover validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForFailoverResult> ValidateRecoveryPlanForFailoverCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string> sourceLocations, IReadOnlyList<string>? selectedResourceIds, string? userConsent, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        ResilienceManagementFailoverContent content = CreateFailoverRequestContent(sourceLocations, selectedResourceIds, userConsent);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ValidateForRecoveryOperationBaseResult> operation = await recoveryPlanResource.ValidateForFailoverAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);

        return CreateRecoveryPlanValidateForFailoverResult(
            operationId,
            operation.GetRawResponse().Content,
            operation.Value.RecoveryResourceQualifications);
    }

    internal static ResilienceManagementFailoverContent CreateFailoverRequestContent(
        IReadOnlyList<string> sourceLocations,
        IReadOnlyList<string>? selectedResourceIds,
        string? userConsent)
    {
        var properties = new ResilienceManagementFailoverProperties(sourceLocations);
        foreach (string selectedResourceId in selectedResourceIds ?? [])
        {
            properties.SelectedResourceIds.Add(new ResourceIdentifier(selectedResourceId));
        }

        if (userConsent is not null)
        {
            properties.ExecutionConfigurationsUserConsent = new RecoveryActionUserConsent(userConsent);
        }

        return new ResilienceManagementFailoverContent(ResilienceManagementFailoverDirection.FromSpecificLocations)
        {
            FailoverRequestProperties = properties
        };
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

    // Clones so the value outlives the parsed response document.
    private static JsonElement? GetOptionalRawValue(JsonElement element, string propertyName, JsonValueKind kind) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == kind
            ? property.Clone()
            : null;

    private static bool HasOptionalValue(JsonElement element, string propertyName, JsonValueKind kind) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(propertyName, out JsonElement property) &&
        property.ValueKind == kind;

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
                RecoveryOperationQualificationDetails? details = qualification?.OperationQualificationDetails;
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

    public Task<RecoveryPlanValidateForReprotectResult> ValidateRecoveryPlanForReprotectAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForReprotectCoreAsync(serviceGroup, recoveryPlan, selectedResourceIds, tenant, token),
            "recoveryplan reprotect validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForReprotectResult> ValidateRecoveryPlanForReprotectCoreAsync(string serviceGroup, string recoveryPlan, IReadOnlyList<string>? selectedResourceIds, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        ReprotectContent content = CreateReprotectRequestContent(selectedResourceIds);

        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ValidateForRecoveryOperationBaseResult> operation = await recoveryPlanResource.ValidateForReprotectAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);

        RecoveryPlanValidateForFailoverResult result = CreateRecoveryPlanValidateForFailoverResult(
            operationId,
            operation.GetRawResponse().Content,
            operation.Value.RecoveryResourceQualifications);
        return new RecoveryPlanValidateForReprotectResult(
            result.OperationId,
            result.RecoveryResourceQualifications);
    }

    internal static ReprotectContent CreateReprotectRequestContent(IReadOnlyList<string>? selectedResourceIds)
    {
        var content = new ReprotectContent();
        foreach (string selectedResourceId in selectedResourceIds ?? [])
        {
            content.ReprotectRequestSelectedResourceIds.Add(new ResourceIdentifier(selectedResourceId));
        }

        return content;
    }

    public Task<RecoveryPlanValidateForOperationResult> ValidateRecoveryPlanForOperationAsync(string serviceGroup, string recoveryPlan, RecoveryOperationName operationName, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ValidateRecoveryPlanForOperationCoreAsync(serviceGroup, recoveryPlan, operationName, tenant, token),
            "recoveryplan operation validation",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanValidateForOperationResult> ValidateRecoveryPlanForOperationCoreAsync(string serviceGroup, string recoveryPlan, RecoveryOperationName operationName, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        var content = new ValidateForOperationContent(operationName);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ArmResponseErrorResponseResult> operation = await recoveryPlanResource.ValidateForOperationAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);

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

    public Task<RecoveryPlanReadinessResult> CheckRecoveryPlanReadinessAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => CheckRecoveryPlanReadinessCoreAsync(serviceGroup, recoveryPlan, tenant, token),
            "recoveryplan readiness check",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryPlanReadinessResult> CheckRecoveryPlanReadinessCoreAsync(string serviceGroup, string recoveryPlan, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryJobCollection recoveryJobs = recoveryPlanResource.GetRecoveryJobs();
        HashSet<string> existingRecoveryJobIds = await GetRecoveryJobIdsAsync(recoveryJobs, cancellationToken);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation operation = await recoveryPlanResource.CheckReadinessAsync(WaitUntil.Started, operationId, cancellationToken);
        await WaitForRecoveryPlanLroCompletionAsync(operation, RecoveryPlanOperationTimeout, cancellationToken);
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

    private static async Task WaitForUsagePlanLroCompletionAsync(Operation operation, string operationDescription, CancellationToken cancellationToken)
    {
        await ExecuteWithTimeoutAsync(
            async token =>
            {
                await WaitForLroCompletionAsync(operation, token);
                return true;
            },
            operationDescription,
            UsagePlanOperationTimeout,
            cancellationToken);
    }

    private static async Task WaitForRecoveryPlanLroCompletionAsync(Operation operation, TimeSpan timeout, CancellationToken cancellationToken)
    {
        await ExecuteWithTimeoutAsync(
            async token =>
            {
#if DEBUG
                if (EnvironmentHelpers.IsPlaybackTesting())
                {
                    await WaitForLroCompletionAsync(operation, token);
                    return true;
                }
#endif

                while (true)
                {
                    _ = await operation.UpdateStatusAsync(token);
                    if (operation.HasCompleted)
                    {
                        return true;
                    }

                    await Task.Delay(RecoveryPlanPollingInterval, token);
                }
            },
            "recoveryplan operation",
            timeout,
            cancellationToken);
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

    internal static RecoveryPlanReadinessError? CreateReadinessError(ResilienceManagementJobErrorInfo? error)
    {
        return error is null
            ? null
            : new RecoveryPlanReadinessError(error.ErrorCode, error.ErrorMessage, error.Recommendations ?? []);
    }

    private static List<RecoveryPlanReadinessFailedTask> GetFailedTasks(IReadOnlyList<ResilienceManagementJobTaskDetail>? tasks)
    {
        var failedTasks = new List<RecoveryPlanReadinessFailedTask>();
        if (tasks is not null)
        {
            AddFailedTasks(tasks, failedTasks);
        }

        return failedTasks;
    }

    private static void AddFailedTasks(IReadOnlyList<ResilienceManagementJobTaskDetail> tasks, List<RecoveryPlanReadinessFailedTask> failedTasks)
    {
        foreach (ResilienceManagementJobTaskDetail task in tasks)
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

        if (effectiveSolutionType == ResourceProtectionSolutionType.CrossZoneVMRecovery)
        {
            if (effectiveSolutionSetting is not ResourceCrossZoneVmRecoveryProtectionSetting crossZoneSetting)
            {
                throw new ArgumentException($"Recovery resource '{resourceId}' requires a CrossZoneVMRecovery selectedProtectionSolutionSetting that matches selectedProtectionSolutionType.");
            }

            if (!string.Equals(existing.ResourceId?.ResourceType.ToString(), "Microsoft.Compute/virtualMachines", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Recovery resource '{resourceId}' can use CrossZoneVMRecovery only for a Microsoft.Compute/virtualMachines resource.");
            }

            ValidateResourceType(crossZoneSetting.CapacityReservationGroupId, "Microsoft.Compute/capacityReservationGroups", resourceId, "capacityReservationGroupId");
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
            throw new ArgumentException($"Recovery resource '{resourceId}' cannot use AzureSiteRecovery because healthy Azure Site Recovery protection was not detected. Enable Azure Site Recovery protection, wait until its status is Protected, and refresh the recoveryplan.");
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

    internal static RecoveryGroupsSetting CreateRecoveryGroupsSetting(RecoveryGroupsSetting? existingRecoveryGroups, string? defaultGroupDescription, IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = null, IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = null, string? defaultGroupUniqueId = null)
    {
        RecoveryGroup? existingDefaultGroup = existingRecoveryGroups?.DefaultGroup;
        RecoveryGroupsSetting recoveryGroups;
        if (existingDefaultGroup?.Properties is { } existingDefaultGroupProperties)
        {
            if (defaultGroupUniqueId is not null &&
                !string.Equals(existingDefaultGroupProperties.GroupUniqueId, defaultGroupUniqueId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The default recovery group's groupUniqueId cannot be changed after the recoveryplan is created.");
            }

            if (defaultGroupDescription is not null)
            {
                existingDefaultGroupProperties.Description = defaultGroupDescription;
            }

            recoveryGroups = existingRecoveryGroups!;
        }
        else
        {
            // The service requires the default group's orderId to be 0.
            var defaultGroup = new RecoveryGroup
            {
                Properties = new RecoveryGroupProperties(defaultGroupUniqueId ?? Guid.NewGuid().ToString(), 0, defaultGroupDescription ?? "Default recovery group")
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
        if (action.AssociatedIdentityType is not null)
        {
            sdkAction.AssociatedIdentity = new ResilienceManagementAssociatedIdentity(new ManagedServiceIdentityType(action.AssociatedIdentityType))
            {
                UserAssignedIdentity = action.AssociatedUserAssignedIdentity is null ? null : new ResourceIdentifier(action.AssociatedUserAssignedIdentity)
            };
        }
        foreach ((string name, string value) in action.Parameters ?? new Dictionary<string, string>())
        {
            sdkAction.Parameters.Add(name, value);
        }

        return sdkAction;
    }

    internal static ManagedServiceIdentity CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind identityType, string? userAssignedIdentity, ManagedServiceIdentity? existingIdentity = null)
    {
        if (identityType == RecoveryPlanIdentityKind.None)
        {
            return new ManagedServiceIdentity(ManagedServiceIdentityType.None);
        }

        if (identityType == RecoveryPlanIdentityKind.SystemAssigned)
        {
            return new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned);
        }

        ResourceIdentifier identityResourceId = ParseUserAssignedIdentityResourceId(userAssignedIdentity!);
        if (existingIdentity?.UserAssignedIdentities.Keys.Any(existingIdentityResourceId => existingIdentityResourceId != identityResourceId) == true)
        {
            throw new ArgumentException("Changing the user-assigned managed identity of an existing recoveryplan is not supported.", nameof(userAssignedIdentity));
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

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryResourcesAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetRecoveryResourceAsync(string serviceGroup, string recoveryPlan, string recoveryResource, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryPlanId = RecoveryPlanResource.CreateResourceIdentifier(serviceGroup, recoveryPlan);
        RecoveryPlanResource recoveryPlanResource = await armClient.GetRecoveryPlanResource(recoveryPlanId).GetAsync(cancellationToken);
        RecoveryMembersCollection recoveryMembers = recoveryPlanResource.GetAllRecoveryMembers();
        Response<RecoveryMembersResource> response = await recoveryMembers.GetAsync(recoveryResource, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobsAsync(string serviceGroup, string recoveryPlan, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        Response<RecoveryJobResource> response = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public Task<RecoveryJobRetryResult> RetryRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => RetryRecoveryJobCoreAsync(serviceGroup, recoveryPlan, recoveryJob, tenant, token),
            "recovery job retry request",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryJobRetryResult> RetryRecoveryJobCoreAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        RecoveryJobResource recoveryJobResource = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ArmResponseErrorResponseResult> operation = await recoveryJobResource.RetryAsync(
            WaitUntil.Started,
            operationId,
            cancellationToken);
        if (operation.HasCompleted)
        {
            ThrowIfProviderError(operation.Value, "Recovery job retry");
        }

        return new RecoveryJobRetryResult(
            operationId,
            "Accepted",
            $"Recovery job retry was accepted. Use 'resiliency recoveryjob get --service-group {serviceGroup} --recoveryplan {recoveryPlan} --recoveryjob {recoveryJob}' to monitor progress.");
    }

    public Task<RecoveryJobResumeResult> ResumeRecoveryJobAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? description = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        return ExecuteWithTimeoutAsync(
            token => ResumeRecoveryJobCoreAsync(serviceGroup, recoveryPlan, recoveryJob, description, tenant, token),
            "recovery job resume request",
            RecoveryPlanOperationTimeout,
            cancellationToken);
    }

    private async Task<RecoveryJobResumeResult> ResumeRecoveryJobCoreAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? description, string? tenant, CancellationToken cancellationToken)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryJobId = RecoveryJobResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob);
        RecoveryJobResource recoveryJobResource = await armClient.GetRecoveryJobResource(recoveryJobId).GetAsync(cancellationToken);
        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ArmResponseErrorResponseResult> operation = await recoveryJobResource.ResumeAsync(
            WaitUntil.Started,
            operationId,
            new RecoveryActionContent { Description = description },
            cancellationToken);
        if (operation.HasCompleted)
        {
            ThrowIfProviderError(operation.Value, "Recovery job resume");
        }

        return new RecoveryJobResumeResult(
            operationId,
            "Accepted",
            $"Recovery job resume was accepted. Use 'resiliency recoveryjob get --service-group {serviceGroup} --recoveryplan {recoveryPlan} --recoveryjob {recoveryJob}' to monitor progress.");
    }

    private static string CreateRecoveryPlanActionTrackingMessage(string action, string? jobId)
    {
        return jobId is null
            ? $"{action} was accepted. Use 'resiliency recoveryjob get' to list recovery jobs, then provide --recoveryjob to monitor the new job."
            : $"{action} was accepted. Use 'resiliency recoveryjob get --recoveryjob {jobId}' to monitor progress.";
    }

    internal static void ThrowIfProviderError(ArmResponseErrorResponseResult result, string operationDescription)
    {
        ResponseError? error;
        try
        {
            error = result.BodyError;
        }
        catch (NullReferenceException)
        {
            // The generated SDK getter dereferences a missing response body for successful empty responses.
            return;
        }

        if (error is not null)
        {
            throw new InvalidOperationException($"{operationDescription} failed with provider error '{error.Code}': {error.Message}");
        }
    }

    public async Task<IEnumerable<ResourceSummary>> ListRecoveryJobResourcesAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetRecoveryJobResourceAsync(string serviceGroup, string recoveryPlan, string recoveryJob, string recoveryJobTarget, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var recoveryJobTargetId = RecoveryJobTargetResource.CreateResourceIdentifier(serviceGroup, recoveryPlan, recoveryJob, recoveryJobTarget);
        Response<RecoveryJobTargetResource> response = await armClient.GetRecoveryJobTargetResource(recoveryJobTargetId).GetAsync(cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillsAsync(string serviceGroup, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<DrillInfo> GetDrillAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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
            SystemData: root.TryGetProperty("systemData", out JsonElement systemDataElement) ? systemDataElement.Clone() : default,
            Identity: root.TryGetProperty("identity", out JsonElement identityElement) ? identityElement.Clone() : default);

    public async Task<DrillInfo> CreateDrillAsync(string serviceGroup, string drill, string subscription, string region, string? resourceGroup, DrillKind drillType, DrillRbacSetupMode rbacSetupMode, DrillProperties? properties = null, ManagedServiceIdentity? identity = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var serviceGroupId = CreateServiceGroupResourceIdentifier(serviceGroup);
        ResilienceManagementDrillCollection drills = armClient.GetResilienceManagementDrills(serviceGroupId);
        var associatedIdentity = new ResilienceManagementAssociatedIdentity(ManagedServiceIdentityType.SystemAssigned);
        properties ??= drillType switch
        {
            DrillKind.Zonal => new ZonalDrillProperties(),
            DrillKind.Regional => new RegionalDrillProperties(),
            _ => throw new ArgumentOutOfRangeException(nameof(drillType), drillType, "Unsupported drill type.")
        };

        properties.DrillAssetProperties = new AssetPropertiesOfDrill(subscriptionId, region)
        {
            ResourceGroup = resourceGroup
        };
        properties.ChaosResourceProperties ??= new ChaosResourcePropertiesOfDrill(associatedIdentity, associatedIdentity);
        properties.RbacSetupMode = rbacSetupMode switch
        {
            DrillRbacSetupMode.AutomatedCustomRole => new ResilienceManagementRbacSetupMode("AutomatedCustomRole"),
            DrillRbacSetupMode.AutomatedBuiltinRoles => new ResilienceManagementRbacSetupMode("AutomatedBuiltinRoles"),
            DrillRbacSetupMode.Manual => new ResilienceManagementRbacSetupMode("Manual"),
            _ => throw new ArgumentOutOfRangeException(nameof(rbacSetupMode), rbacSetupMode, "Unsupported RBAC setup mode.")
        };

        // recoveryPlanId is read-only; the service links the service group's recovery plan for the drill type.
        properties.RecoveryPlanProperties ??= new RecoveryPlanPropertiesOfDrill(associatedIdentity);
        properties.MonitoringProperties ??= ArmResilienceManagementModelFactory.MonitoringPropertiesOfDrill(
            associatedIdentity,
            logAnalyticsWorkspaceId: null,
            rawMetricsDataCollectionRuleId: null,
            serviceGroupMetricsDataCollectionRuleId: null,
            dataCollectionEndpointId: null);

        var drillData = new ResilienceManagementDrillData
        {
            Identity = identity ?? new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned),
            Properties = properties
        };

        ArmOperation<ResilienceManagementDrillResource> operation = await drills.CreateOrUpdateAsync(WaitUntil.Started, drill, drillData, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);
        Response<ResilienceManagementDrillResource> completedDrill = await drills.GetAsync(drill, cancellationToken);

        return MapDrill(completedDrill.Value.Data);
    }

    public async Task<DrillInfo> UpdateDrillAsync(string serviceGroup, string drill, string? subscription = null, string? region = null, DrillRbacSetupMode? rbacSetupMode = null, DrillUpdateProperties? properties = null, ManagedServiceIdentity? identity = null, string? resourceGroup = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = subscription is null || AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        properties ??= new DrillUpdateProperties();

        if (subscriptionId is not null && region is not null)
        {
            properties.DrillAssetProperties = new AssetPropertiesOfDrill(subscriptionId, region)
            {
                ResourceGroup = resourceGroup
            };
        }

        if (rbacSetupMode is not null)
        {
            properties.RbacSetupMode = new ResilienceManagementRbacSetupMode(rbacSetupMode.Value.ToString());
        }

        var patch = new ResilienceManagementDrillPatch
        {
            Properties = properties,
            Identity = identity
        };

        ArmOperation operation = await drillResource.UpdateAsync(WaitUntil.Started, patch, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        Response<ResilienceManagementDrillResource> response = await drillResource.GetAsync(cancellationToken);

        return MapDrill(response.Value.Data);
    }

    public async Task<DrillResyncReadinessResult> CheckDrillResyncReadinessAsync(
        string serviceGroup,
        string drill,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        string operationId = Guid.NewGuid().ToString();

        var operation = await drillResource.ResyncReadinessCheckAsync(
            WaitUntil.Started,
            operationId,
            cancellationToken);

        return new DrillResyncReadinessResult(operationId, operation.HasCompleted);
    }

    public async Task<DrillValidateForExecutionResult> ValidateDrillForExecutionAsync(
        string serviceGroup,
        string drill,
        IEnumerable<string>? sourceLocations = null,
        DrillRunTask? operationName = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        string[]? locations = sourceLocations?.ToArray();
        if (operationName != DrillRunTask.Reprotect && locations is not { Length: > 0 })
        {
            throw new ArgumentException("Source locations are required unless the validation operation is Reprotect.", nameof(sourceLocations));
        }
        if (locations is not null && (locations.Length == 0 || locations.Any(string.IsNullOrWhiteSpace)))
        {
            throw new ArgumentException("Source locations must contain at least one non-empty location.", nameof(sourceLocations));
        }

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        var properties = new ValidateForExecutionProperties();
        if (operationName.HasValue)
        {
            properties.OperationName = operationName.Value;
        }
        if (locations is not null)
        {
            foreach (string location in locations)
            {
                properties.SourceLocations.Add(location);
            }
        }
        var content = new ValidateForExecutionContent { ValidateForExecutionProperties = properties };
        string operationId = Guid.NewGuid().ToString();

        var operation = await drillResource.ValidateForExecutionAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);

        return new DrillValidateForExecutionResult(operationId, operation.HasCompleted);
    }

    public async Task<DrillAddOrUpdateResourcesResult> AddOrUpdateDrillResourcesAsync(
        string serviceGroup,
        string drill,
        AddOrUpdateResourcesContent content,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        string operationId = Guid.NewGuid().ToString();

        var operation = await drillResource.AddOrUpdateResourcesAsync(
            WaitUntil.Started,
            operationId,
            content,
            cancellationToken);

        return new DrillAddOrUpdateResourcesResult(operationId, operation.HasCompleted);
    }

    public async Task DeleteDrillAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        var operation = await drillResource.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);
    }

    public async Task<string> StartDrillAsync(string serviceGroup, string drill, string mode, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        string operationId = Guid.NewGuid().ToString();

        await drillResource.StartAsync(
            WaitUntil.Started,
            operationId,
            new DrillStartContent(new DrillMode(mode)),
            cancellationToken);

        return operationId;
    }

    public async Task<string> EndDrillAsync(string serviceGroup, string drill, string attestation, string attestationNotes, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        string operationId = Guid.NewGuid().ToString();

        await drillResource.EndAsync(
            WaitUntil.Started,
            operationId,
            new DrillEndContent(new DrillAttestation(attestation), attestationNotes),
            cancellationToken);

        return operationId;
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillResourcesAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<DrillResourceInfo> GetDrillResourceAsync(string serviceGroup, string drill, string drillResource, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillInstanceResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillTargetCollection drillTargets = drillInstanceResource.GetDrillTargets();
        Response<DrillTargetResource> response = await drillTargets.GetAsync(drillResource, cancellationToken);

        return MapDrillResource(response.GetRawResponse().Content);
    }

    internal static DrillResourceInfo MapDrillResource(BinaryData responseContent)
    {
        using JsonDocument document = JsonDocument.Parse(responseContent.ToMemory());
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

    public async Task<IEnumerable<ResourceSummary>> ListDrillRunsAsync(string serviceGroup, string drill, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetDrillRunAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var drillId = ResilienceManagementDrillResource.CreateResourceIdentifier(serviceGroup, drill);
        ResilienceManagementDrillResource drillResource = armClient.GetResilienceManagementDrillResource(drillId);
        DrillRunCollection drillRuns = drillResource.GetDrillRuns();
        Response<DrillRunResource> response = await drillRuns.GetAsync(drillRun, cancellationToken);

        using JsonDocument document = JsonDocument.Parse(response.GetRawResponse().Content.ToMemory());
        return document.RootElement.Clone();
    }

    public async Task<string> AddDrillRunNotesAsync(string serviceGroup, string drill, string drillRun, string notes, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        var content = new DrillRunAddNotesContent
        {
            Notes = notes
        };
        string operationId = Guid.NewGuid().ToString();

        await drillRunResource.AddNotesAsync(WaitUntil.Started, operationId, content, cancellationToken);

        return operationId;
    }

    public async Task<string> FailoverDrillRunAsync(string serviceGroup, string drill, string drillRun, IEnumerable<string> sourceLocations, IEnumerable<string>? selectedResourceIds = null, bool autoFailover = false, string? userConsent = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var failoverProperties = CreateFailoverRequestContent(sourceLocations.ToList(), selectedResourceIds?.ToList(), userConsent);
        var content = new DrillRunFailoverContent(
            autoFailover ? DrillRunAutoFailover.Enable : DrillRunAutoFailover.Disable,
            failoverProperties);
        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        string operationId = Guid.NewGuid().ToString();

        await drillRunResource.FailOverAsync(WaitUntil.Started, operationId, content, cancellationToken);

        return operationId;
    }

    public async Task<string> ResumeDrillRunAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        string operationId = Guid.NewGuid().ToString();

        await drillRunResource.ResumeAsync(WaitUntil.Started, operationId, cancellationToken);

        return operationId;
    }

    public async Task<DrillRunMarkCompleteResult> MarkDrillRunCompleteAsync(string serviceGroup, string drill, string drillRun, string stage, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        var content = new MarkAsCompleteContent(new DrillRunStage(stage));
        string operationId = Guid.NewGuid().ToString();

        var operation = await drillRunResource.MarkAsCompleteAsync(WaitUntil.Started, operationId, content, cancellationToken);

        return new DrillRunMarkCompleteResult(operationId, operation.HasCompleted);
    }

    public async Task<string> ReprotectDrillRunAsync(string serviceGroup, string drill, string drillRun, IReadOnlyList<string>? selectedResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        string operationId = Guid.NewGuid().ToString();

        DrillRunReprotectContent content = CreateDrillRunReprotectContent(selectedResourceIds);
        await drillRunResource.ReprotectAsync(WaitUntil.Started, operationId, content, cancellationToken);

        return operationId;
    }

    public async Task<DrillRunGenerateReportResult> GenerateDrillRunReportAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        string operationId = Guid.NewGuid().ToString();

        ArmOperation<DrillReportSummary> operation = await drillRunResource.GenerateReportAsync(WaitUntil.Started, operationId, cancellationToken);

        // The service replies with the DrillRun envelope, so the SDK model, which reads the summary from the
        // root object, has no value; the summary is nested at properties.report.
        DrillReportSummary? report = operation.HasValue
            ? operation.Value
            : ReadDrillRunReport(operation.GetRawResponse()?.Content);

        return new DrillRunGenerateReportResult(
            operationId,
            operation.HasCompleted,
            report is null ? null : MapDrillRunReport(report));
    }

    internal static DrillReportSummary? ReadDrillRunReport(BinaryData? rawOperationResponse)
    {
        if (rawOperationResponse is null)
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawOperationResponse);
            if (!document.RootElement.TryGetProperty("properties", out JsonElement properties) ||
                !properties.TryGetProperty("report", out JsonElement report) ||
                report.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return ModelReaderWriter.Read<DrillReportSummary>(
                BinaryData.FromString(report.GetRawText()),
                ModelReaderWriterOptions.Json,
                AzureResourceManagerResilienceManagementContext.Default);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<DrillRunReportDownloadInfo> GetDrillRunReportDownloadUrlAsync(string serviceGroup, string drill, string drillRun, string? format = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, armClientOptions: CreateArmClientOptionsWithOperationIdPolicy(), cancellationToken: cancellationToken);

        var drillRunId = DrillRunResource.CreateResourceIdentifier(serviceGroup, drill, drillRun);
        DrillRunResource drillRunResource = armClient.GetDrillRunResource(drillRunId);
        var content = new ListReportDownloadUrlContent();
        if (format is not null)
        {
            content.Format = new DrillReportFormat(format);
        }

        string operationId = Guid.NewGuid().ToString();
        ArmOperation<ListReportDownloadUrlResult> operation = await drillRunResource.GetReportDownloadUriAsync(WaitUntil.Started, operationId, content, cancellationToken);

        await ExecuteWithTimeoutAsync(
            async token =>
            {
                await WaitForLroCompletionAsync(operation, token);
                return true;
            },
            "drill run report download URL request",
            DrillReportOperationTimeout,
            cancellationToken);

        return MapDrillRunReportDownload(operation.Value, format, operation.GetRawResponse()?.Content);
    }

    internal static DrillRunReportInfo MapDrillRunReport(DrillReportSummary summary) => new(
        summary.GenerationStatus?.ToString(),
        summary.FinalizationState?.ToString(),
        summary.StageStatuses
            .Select(stage => new DrillRunReportStageInfo(
                stage.DrillRunStage.ToString(),
                stage.GenerationStatus?.ToString(),
                stage.LastAttemptOn,
                MapDrillRunReportError(stage.LastError)))
            .ToList(),
        summary.AvailableFormats.Select(format => format.ToString()).ToList(),
        summary.LastGeneratedOn,
        summary.SchemaVersion,
        MapDrillRunReportError(summary.LastError));

    internal static DrillRunReportDownloadInfo MapDrillRunReportDownload(ListReportDownloadUrlResult result, string? requestedFormat, BinaryData? rawOperationResponse = null)
    {
        // The service returns "downloadUrl" and "expiryTimestamp", which the SDK model does not read.
        JsonElement? properties = ReadReportDownloadProperties(rawOperationResponse);
        string? downloadUrl = result.DownloadUri?.ToString() ?? ReadString(properties, "downloadUrl");
        if (string.IsNullOrEmpty(downloadUrl))
        {
            throw new InvalidOperationException("The service returned no report download URL.");
        }

        DateTimeOffset? expiresOn = result.ExpiryOn;
        if (expiresOn is null &&
            DateTimeOffset.TryParse(ReadString(properties, "expiryTimestamp"), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedExpiry))
        {
            expiresOn = parsedExpiry;
        }

        return new(result.Format?.ToString() ?? ReadString(properties, "format") ?? requestedFormat, downloadUrl, expiresOn);
    }

    private static JsonElement? ReadReportDownloadProperties(BinaryData? rawOperationResponse)
    {
        if (rawOperationResponse is null)
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawOperationResponse);
            return document.RootElement.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object
                ? properties.Clone()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement? element, string propertyName) =>
        element is { } value && value.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static DrillRunReportErrorInfo? MapDrillRunReportError(ResilienceManagementErrorDetail? error) =>
        error is null ? null : new DrillRunReportErrorInfo(error.Code, error.Message, error.Recommendations.ToList());

    // The service accepts a reprotect without a request body but never processes it, so always send a JSON body.
    internal static DrillRunReprotectContent CreateDrillRunReprotectContent(IReadOnlyList<string>? selectedResourceIds)
    {
        var content = new DrillRunReprotectContent();
        foreach (string resourceId in selectedResourceIds ?? [])
        {
            content.ReprotectRequestSelectedResourceIds.Add(new ResourceIdentifier(resourceId));
        }
        return content;
    }

    public async Task<IEnumerable<ResourceSummary>> ListDrillRunResourcesAsync(string serviceGroup, string drill, string drillRun, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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

    public async Task<JsonElement> GetDrillRunResourceAsync(string serviceGroup, string drill, string drillRun, string drillRunResource, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

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
    public async Task<UsagePlanInfo> CreateUsagePlanAsync(string resourceGroup, string usagePlan, UsagePlanKind planType, string subscription, IReadOnlyDictionary<string, string>? tags = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        var resourceGroupResource = armClient.GetResourceGroupResource(resourceGroupId);
        UsagePlanCollection usagePlans = resourceGroupResource.GetUsagePlans();

        var usagePlanData = new UsagePlanData(new AzureLocation("global"))
        {
            Properties = new UsagePlanProperties
            {
                PlanType = planType == UsagePlanKind.Standard
                    ? UsagePlanType.Standard
                    : throw new ArgumentOutOfRangeException(nameof(planType), planType, "Unsupported plan type.")
            }
        };
        foreach (KeyValuePair<string, string> tag in tags ?? new Dictionary<string, string>())
        {
            usagePlanData.Tags[tag.Key] = tag.Value;
        }

        ArmOperation<UsagePlanResource> operation = await usagePlans.CreateOrUpdateAsync(WaitUntil.Started, usagePlan, usagePlanData, cancellationToken);
        await WaitForUsagePlanLroCompletionAsync(operation, "usage plan create or update", cancellationToken);

        UsagePlanResource createdPlan = operation.Value;
        if (usagePlanData.Tags.Count > 0)
        {
            // The service drops tags sent on the PUT when it completes the asynchronous create, but it persists tags
            // sent in a PATCH, so apply them once the plan exists.
            ArmOperation<UsagePlanResource> tagOperation = await createdPlan.UpdateAsync(
                WaitUntil.Started, CreateUsagePlanTagsPatch(usagePlanData.Tags), cancellationToken);
            await WaitForUsagePlanLroCompletionAsync(tagOperation, "usage plan tag update", cancellationToken);
            createdPlan = tagOperation.Value;
        }

        return MapUsagePlan(createdPlan.Data);
    }

    internal static UsagePlanPatch CreateUsagePlanTagsPatch(IEnumerable<KeyValuePair<string, string>> tags)
    {
        var patch = new UsagePlanPatch();
        foreach (KeyValuePair<string, string> tag in tags)
        {
            patch.Tags[tag.Key] = tag.Value;
        }

        return patch;
    }

    public async Task<bool> DeleteUsagePlanAsync(string resourceGroup, string usagePlan, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var resourceGroupId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");
        UsagePlanCollection usagePlans = armClient.GetResourceGroupResource(resourceGroupId).GetUsagePlans();
        NullableResponse<UsagePlanResource> existingPlan = await usagePlans.GetIfExistsAsync(usagePlan, cancellationToken);
        if (!existingPlan.HasValue || existingPlan.Value is null)
        {
            return false;
        }

        try
        {
            ArmOperation operation = await existingPlan.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForUsagePlanLroCompletionAsync(operation, "usage plan delete", cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public async Task<UsagePlanEnrollmentInfo> CreateUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string serviceGroup, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var usagePlanId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.AzureResilienceManagement/usagePlans/{usagePlan}");
        UsagePlanEnrollmentCollection enrollments = armClient.GetUsagePlanResource(usagePlanId).GetUsagePlanEnrollments();

        var serviceGroupId = new ResourceIdentifier($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        var enrollmentData = new UsagePlanEnrollmentData
        {
            Properties = new UsagePlanEnrollmentProperties(serviceGroupId)
        };

        ArmOperation<UsagePlanEnrollmentResource> operation = await enrollments.CreateOrUpdateAsync(WaitUntil.Started, enrollment, enrollmentData, cancellationToken);
        await WaitForUsagePlanLroCompletionAsync(operation, "usage plan enrollment create or update", cancellationToken);

        return MapUsagePlanEnrollment(operation.Value.Data);
    }

    public async Task<bool> DeleteUsagePlanEnrollmentAsync(string resourceGroup, string usagePlan, string enrollment, string subscription, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var subscriptionId = AzureService.IsSubscriptionId(subscription)
            ? subscription
            : (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Data.SubscriptionId;

        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);

        var usagePlanId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.AzureResilienceManagement/usagePlans/{usagePlan}");
        UsagePlanEnrollmentCollection enrollments = armClient.GetUsagePlanResource(usagePlanId).GetUsagePlanEnrollments();
        NullableResponse<UsagePlanEnrollmentResource> existingEnrollment = await enrollments.GetIfExistsAsync(enrollment, cancellationToken);
        if (!existingEnrollment.HasValue || existingEnrollment.Value is null)
        {
            return false;
        }

        try
        {
            ArmOperation operation = await existingEnrollment.Value.DeleteAsync(WaitUntil.Started, cancellationToken);
            await WaitForUsagePlanLroCompletionAsync(operation, "usage plan enrollment delete", cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
