// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore slis

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class ResilienceManagementServiceTests
{
    private const string UserAssignedIdentityResourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/testIdentity";
    private const string RecoveryJobName = "11111111-1111-1111-1111-111111111111";

    [Fact]
    public void CreateUsagePlanTagsPatch_CarriesEveryTag()
    {
        var tags = new Dictionary<string, string> { ["mcp-test"] = "usage-plan", ["env"] = "prod" };

        UsagePlanPatch patch = ResilienceManagementService.CreateUsagePlanTagsPatch(tags);

        Assert.Equal(2, patch.Tags.Count);
        Assert.Equal("usage-plan", patch.Tags["mcp-test"]);
        Assert.Equal("prod", patch.Tags["env"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MapGoalAssignment_PreservesGaResiliencyRequirement(bool requireZonalResiliency)
    {
        BinaryData response = BinaryData.FromObjectAsJson(new
        {
            name = "goal",
            properties = new { requireZonalResiliency, provisioningState = "Succeeded" }
        });
        GoalAssignmentData data = ModelReaderWriter.Read<GoalAssignmentData>(response)!;

        GoalAssignmentInfo result = ResilienceManagementService.MapGoalAssignment(data, response);

        Assert.NotNull(result.Properties);
        Assert.Equal(requireZonalResiliency, result.Properties.RequireZonalResiliency);
        Assert.Equal("Succeeded", result.Properties.ProvisioningState);
        Assert.Null(result.Properties.ServiceLevelResources);
        Assert.Null(result.Properties.ErrorDetails);
    }

    [Fact]
    public void MapGoalAssignment_PreservesGaServiceLevelResourcesAndErrorDetailsAfterResponseIsDisposed()
    {
        const string sliId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Monitor/slis/availability";
        BinaryData response = BinaryData.FromObjectAsJson(new
        {
            name = "goal",
            properties = new
            {
                requireZonalResiliency = true,
                serviceLevelResources = new[] { new { serviceLevelIndicatorResourceId = sliId } },
                provisioningState = "Failed",
                errorDetails = new { code = "GoalAssignmentFailed", message = "Assignment failed." }
            }
        });
        GoalAssignmentData data = ModelReaderWriter.Read<GoalAssignmentData>(response)!;

        GoalAssignmentInfo result = ResilienceManagementService.MapGoalAssignment(data, response);

        Assert.NotNull(result.Properties);
        Assert.NotNull(result.Properties.ServiceLevelResources);
        GoalServiceLevelTargetInfo serviceLevelResource = Assert.Single(result.Properties.ServiceLevelResources);
        Assert.Equal(sliId, serviceLevelResource.ServiceLevelIndicatorResourceId);
        Assert.NotNull(result.Properties.ErrorDetails);
        Assert.Equal("GoalAssignmentFailed", result.Properties.ErrorDetails.Value.GetProperty("code").GetString());
        Assert.Equal("Failed", result.Properties.ProvisioningState);
    }

    [Fact]
    public void MapGoalAssignment_DoesNotEmitPreviewOnlyFields()
    {
        BinaryData response = BinaryData.FromObjectAsJson(new
        {
            name = "goal",
            properties = new { goalAssignmentType = "System", goalTemplateId = "/providers/Microsoft.AzureResilienceManagement/goalTemplates/default" }
        });
        GoalAssignmentData data = ModelReaderWriter.Read<GoalAssignmentData>(response)!;

        GoalAssignmentInfo result = ResilienceManagementService.MapGoalAssignment(data, response);

        string json = JsonSerializer.Serialize(result.Properties, ResilienceManagementJsonContext.Default.GoalAssignmentInfoProperties);
        Assert.DoesNotContain("goalAssignmentType", json, StringComparison.Ordinal);
        Assert.DoesNotContain("goalTemplateId", json, StringComparison.Ordinal);
        Assert.Null(result.Properties!.RequireZonalResiliency);
    }

    [Fact]
    public void MapGoalResource_PreservesGaZonalResiliencyAfterResponseIsDisposed()
    {
        BinaryData response = BinaryData.FromObjectAsJson(new
        {
            name = "resource",
            properties = new
            {
                resourceArmId = UserAssignedIdentityResourceId,
                provisioningState = "NeedsAttention",
                zonalResiliency = new
                {
                    goalParticipation = "Excluded",
                    attestationStatus = "NotAttested",
                    exclusionReason = "UnsupportedResource",
                    userConfirmation = new[] { new { solutionDisplayName = "ZoneRedundantStorage", confirmationStatus = "Pending" } }
                },
                disasterRecoveryAttestationStatus = "Attested",
                serviceGroupMemberships = new[] { new { membershipType = "Direct", serviceGroupId = "/providers/Microsoft.Management/serviceGroups/sg1" } }
            }
        });
        GoalMembersData data = ModelReaderWriter.Read<GoalMembersData>(response)!;

        GoalResourceInfo result = ResilienceManagementService.MapGoalResource(data, response);

        Assert.NotNull(result.Properties);
        Assert.Equal("NeedsAttention", result.Properties.ProvisioningState);
        Assert.Equal(UserAssignedIdentityResourceId, result.Properties.ResourceArmId);
        Assert.NotNull(result.Properties.ZonalResiliency);
        Assert.Equal("Excluded", result.Properties.ZonalResiliency.GoalParticipation);
        Assert.Equal("NotAttested", result.Properties.ZonalResiliency.AttestationStatus);
        Assert.Equal("UnsupportedResource", result.Properties.ZonalResiliency.ExclusionReason);
        GoalResourceConfirmationInfo confirmation = Assert.Single(result.Properties.ZonalResiliency.UserConfirmation);
        Assert.Equal("ZoneRedundantStorage", confirmation.SolutionDisplayName);
        Assert.Equal("Pending", confirmation.ConfirmationStatus);
        string json = JsonSerializer.Serialize(result.Properties, ResilienceManagementJsonContext.Default.GoalResourceInfoProperties);
        Assert.DoesNotContain("disasterRecoveryAttestationStatus", json, StringComparison.Ordinal);
        Assert.DoesNotContain("serviceGroupMemberships", json, StringComparison.Ordinal);
    }

    [Fact]
    public void MapDrillResource_PreservesLegacyFieldsWithoutInventingSdkDiscriminator()
    {
        var content = BinaryData.FromString("""
            {
              "id":"/providers/Microsoft.Management/serviceGroups/group/providers/Microsoft.AzureResilienceManagement/drills/drill/drillResources/member",
              "name":"member",
              "properties":{
                "haStatus":"NotResilient",
                "activePhysicalZones":["zone1"],
                "recoveryPhysicalZones":["zone2"]
              }
            }
            """);
        var data = ModelReaderWriter.Read<DrillTargetData>(content, new ModelReaderWriterOptions("W"));
        Assert.NotNull(data);
        using var sdkJson = JsonDocument.Parse(ModelReaderWriter.Write(data, new ModelReaderWriterOptions("J")));
        var sdkProperties = sdkJson.RootElement.GetProperty("properties");
        Assert.False(sdkProperties.TryGetProperty("haStatus", out _));
        Assert.Equal("unknown", sdkProperties.GetProperty("drillType").GetString());

        DrillResourceInfo mapped = ResilienceManagementService.MapDrillResource(content);

        Assert.Equal("member", mapped.Name);
        Assert.Equal("NotResilient", mapped.Properties.GetProperty("haStatus").GetString());
        Assert.Equal("zone1", mapped.Properties.GetProperty("activePhysicalZones")[0].GetString());
        Assert.Equal("zone2", mapped.Properties.GetProperty("recoveryPhysicalZones")[0].GetString());
        Assert.False(mapped.Properties.TryGetProperty("drillType", out _));
    }

    [Theory]
    [InlineData("Recovery job retry")]
    [InlineData("Recovery job resume")]
    [InlineData("Recovery plan finalize")]
    public void ThrowIfProviderError_RejectsSuccessfulResponseContainingError(string operationDescription)
    {
        ArmResponseErrorResponseResult result = ArmResilienceManagementModelFactory.ArmResponseErrorResponseResult(
            new ResponseError("ProviderFailure", "The provider could not complete the operation."));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.ThrowIfProviderError(result, operationDescription));

        Assert.Contains(operationDescription, exception.Message, StringComparison.Ordinal);
        Assert.Contains("ProviderFailure", exception.Message, StringComparison.Ordinal);
        Assert.Contains("The provider could not complete the operation.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThrowIfProviderError_AllowsSuccessfulResponseWithoutBody()
    {
        ArmResponseErrorResponseResult result = ArmResilienceManagementModelFactory.ArmResponseErrorResponseResult(null!);

        ResilienceManagementService.ThrowIfProviderError(result, "Recovery job retry");
    }

    [Fact]
    public void GetRecoveryJobResourceId_UsesAbsoluteJobIdExactly()
    {
        ResourceIdentifier result = ResilienceManagementService.GetRecoveryJobResourceId(BinaryData.FromString("""
            {"jobId":"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryJobs/11111111-1111-1111-1111-111111111111"}
            """), "sg1", "plan1");

        Assert.Equal(
            $"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryJobs/{RecoveryJobName}",
            result.ToString());
    }

    [Fact]
    public void GetRecoveryJobResourceId_ResolvesBareJobIdUnderRequestedPlan()
    {
        ResourceIdentifier result = ResilienceManagementService.GetRecoveryJobResourceId(
            BinaryData.FromString($$"""{"jobId":"{{RecoveryJobName}}"}"""),
            "sg1",
            "plan1");

        Assert.Equal(
            $"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryJobs/{RecoveryJobName}",
            result.ToString());
    }

    [Fact]
    public void TryGetRecoveryJobResourceId_ReturnsNullWhenResponseDoesNotContainJobId()
    {
        ResourceIdentifier? result = ResilienceManagementService.TryGetRecoveryJobResourceId(
            BinaryData.FromString("""
                {"id":"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1"}
                """),
            "sg1",
            "plan1");

        Assert.Null(result);
    }

    [Fact]
    public void GetRecoveryJobResourceId_RejectsJobFromDifferentRecoveryPlan()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.GetRecoveryJobResourceId(BinaryData.FromString("""
                {"jobId":"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/other-plan/recoveryJobs/11111111-1111-1111-1111-111111111111"}
                """), "sg1", "plan1"));

        Assert.Contains("invalid recovery job identifier", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetRecoveryJobResourceId_RejectsInvalidJobName()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.GetRecoveryJobResourceId(
                BinaryData.FromString("""{"jobId":"job1"}"""),
                "sg1",
                "plan1"));

        Assert.Contains("invalid recovery job identifier", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("11111111111111111111111111111111")]
    [InlineData("{11111111-1111-1111-1111-111111111111}")]
    [InlineData("/not-an-arm-resource-id")]
    [InlineData("not-json")]
    public void GetRecoveryJobResourceId_RejectsMalformedProviderResponse(string jobId)
    {
        BinaryData response = jobId == "not-json"
            ? BinaryData.FromString(jobId)
            : BinaryData.FromObjectAsJson(new { jobId });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.GetRecoveryJobResourceId(response, "sg1", "plan1"));

        Assert.Contains("invalid recovery job identifier", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetRecoveryJobResourceId_RejectsNestedJobId()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.GetRecoveryJobResourceId(BinaryData.FromString("""
                {"details":{"jobId":"wrong-job"}}
                """), "sg1", "plan1"));

        Assert.Contains("without returning a recovery job identifier", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GetRequiredProperties_RejectsMissingProperties()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.GetRequiredProperties<RecoveryJobProperties>(null, "recovery job"));

        Assert.Contains("recovery job without required properties", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateReadinessError_UsesEmptyRecommendationsWhenProviderOmitsThem()
    {
        ResilienceManagementJobErrorInfo error = ModelReaderWriter.Read<ResilienceManagementJobErrorInfo>(BinaryData.FromObjectAsJson(new
        {
            errorCode = "NotReady",
            errorMessage = "Resource requires attention."
        }))!;

        RecoveryPlanReadinessError result = Assert.IsType<RecoveryPlanReadinessError>(
            ResilienceManagementService.CreateReadinessError(error));

        Assert.Empty(result.Recommendations);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_TimesOutOperation()
    {
        TimeoutException exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            ResilienceManagementService.ExecuteWithTimeoutAsync(
                async token =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return "completed";
                },
                "readiness operation",
                TimeSpan.FromMilliseconds(20),
                CancellationToken.None));

        Assert.Contains("readiness operation", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_PreservesCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResilienceManagementService.ExecuteWithTimeoutAsync(
                async token =>
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return "completed";
                },
                "readiness operation",
                TimeSpan.FromSeconds(1),
                cancellation.Token));
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_PreservesDownstreamCancellation()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResilienceManagementService.ExecuteWithTimeoutAsync<string>(
                _ => Task.FromCanceled<string>(new CancellationToken(canceled: true)),
                "readiness operation",
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
    }

    [Fact]
    public async Task WaitForCompletionAsync_RetriesUntilCompletion()
    {
        var states = new Queue<string?>([null, "InProgress", "Completed"]);
        int attempts = 0;

        string result = await ResilienceManagementService.WaitForCompletionAsync(
            _ =>
            {
                attempts++;
                return Task.FromResult(states.Dequeue());
            },
            state => state == "Completed",
            "test operation",
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.Equal("Completed", result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task WaitForCompletionAsync_TimesOut()
    {
        TimeoutException exception = await Assert.ThrowsAsync<TimeoutException>(() =>
            ResilienceManagementService.WaitForCompletionAsync<string>(
                _ => Task.FromResult<string?>(null),
                _ => false,
                "test operation",
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(20),
                CancellationToken.None));

        Assert.Contains("test operation", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WaitForCompletionAsync_PreservesCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResilienceManagementService.WaitForCompletionAsync<string>(
                _ => Task.FromResult<string?>(null),
                _ => false,
                "test operation",
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromSeconds(1),
                cancellation.Token));
    }

    [Fact]
    public async Task WaitForCompletionAsync_PreservesDownstreamCancellation()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ResilienceManagementService.WaitForCompletionAsync<string>(
                _ => Task.FromCanceled<string?>(new CancellationToken(canceled: true)),
                _ => false,
                "test operation",
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromSeconds(1),
                CancellationToken.None));
    }

    [Theory]
    [InlineData("NotApplicable", true)]
    [InlineData("Completed", true)]
    [InlineData("CompletedWithWarnings", true)]
    [InlineData("Failed", true)]
    [InlineData("Skipped", true)]
    [InlineData("Cancelled", true)]
    [InlineData("NotStarted", false)]
    [InlineData("Pending", false)]
    [InlineData("InProgress", false)]
    [InlineData("Cancelling", false)]
    [InlineData("Paused", false)]
    [InlineData("", false)]
    public void IsTerminalJobStatus_ClassifiesDocumentedStatuses(string status, bool expected)
    {
        Assert.Equal(expected, ResilienceManagementService.IsTerminalJobStatus(status));
    }

    [Fact]
    public void CreateDrillRunReprotectContent_WithoutSelection_SendsEmptyReprotectProperties()
    {
        DrillRunReprotectContent content = ResilienceManagementService.CreateDrillRunReprotectContent(null);

        using JsonDocument document = JsonDocument.Parse(ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W")));
        JsonElement reprotectProperties = document.RootElement.GetProperty("reprotectProperties");
        Assert.Equal(JsonValueKind.Object, reprotectProperties.ValueKind);
        Assert.Empty(reprotectProperties.EnumerateObject());
    }

    [Fact]
    public void CreateDrillRunReprotectContent_WithSelection_SendsSelectedResourceIds()
    {
        const string recoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan/recoveryResources/11111111-1111-1111-1111-111111111111";

        DrillRunReprotectContent content = ResilienceManagementService.CreateDrillRunReprotectContent([recoveryResourceId]);

        Assert.Equal(recoveryResourceId, Assert.Single(content.ReprotectRequestSelectedResourceIds).ToString());
        string json = ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W")).ToString();
        Assert.Contains(recoveryResourceId, json, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_ForNewPlan_GeneratesDefaultGroupId()
    {
        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(null, null);

        Assert.True(Guid.TryParse(result.DefaultGroup.Properties?.GroupUniqueId, out _));
        Assert.Equal("Default recovery group", result.DefaultGroup.Properties?.Description);
        Assert.Empty(result.AdditionalGroups);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_ForUpdate_PreservesExistingGroups()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var firstAdditionalGroup = CreateGroup("ddcfddaf-d15d-44fe-8472-0f3ee9f0179d", 1, "First additional group");
        var secondAdditionalGroup = CreateGroup("9db5bb96-68ab-443d-87b5-2a2555bf46e8", 2, "Second additional group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        existingGroups.AdditionalGroups.Add(firstAdditionalGroup);
        existingGroups.AdditionalGroups.Add(secondAdditionalGroup);

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null);

        Assert.Same(existingGroups, result);
        Assert.Equal(existingDefaultGroup.Properties?.GroupUniqueId, result.DefaultGroup.Properties?.GroupUniqueId);
        Assert.Equal(existingDefaultGroup.Properties?.Description, result.DefaultGroup.Properties?.Description);
        Assert.Equal([firstAdditionalGroup, secondAdditionalGroup], result.AdditionalGroups);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_ForUpdate_OverridesDefaultGroupDescription()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var preAction = new RecoveryGroupManualAction("pre-action", 10);
        var postAction = new RecoveryGroupManualAction("post-action", 10);
        existingDefaultGroup.Properties!.PreActions.Add(preAction);
        existingDefaultGroup.Properties.PostActions.Add(postAction);
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, "Updated default group");

        Assert.Same(existingDefaultGroup, result.DefaultGroup);
        Assert.Equal("7f35c9f5-bec2-455d-8161-c904b2532e5d", result.DefaultGroup.Properties?.GroupUniqueId);
        Assert.Equal("Updated default group", result.DefaultGroup.Properties?.Description);
        Assert.Equal([preAction], result.DefaultGroup.Properties?.PreActions);
        Assert.Equal([postAction], result.DefaultGroup.Properties?.PostActions);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_WithAdditionalGroups_ReplacesGroupsAndPreservesIdByOrder()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var existingAdditionalGroup = CreateGroup("ddcfddaf-d15d-44fe-8472-0f3ee9f0179d", 1, "Existing additional group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        existingGroups.AdditionalGroups.Add(existingAdditionalGroup);
        RecoveryPlanGroupInput[] requestedGroups =
        [
            new(null, 1, "Updated additional group"),
            new(null, 2, "New additional group")
        ];

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null, requestedGroups);

        Assert.Equal(2, result.AdditionalGroups.Count);
        Assert.Same(existingAdditionalGroup, result.AdditionalGroups[0]);
        Assert.Equal("ddcfddaf-d15d-44fe-8472-0f3ee9f0179d", result.AdditionalGroups[0].Properties?.GroupUniqueId);
        Assert.Equal("Updated additional group", result.AdditionalGroups[0].Properties?.Description);
        Assert.True(Guid.TryParse(result.AdditionalGroups[1].Properties?.GroupUniqueId, out _));
        Assert.Equal(2, result.AdditionalGroups[1].Properties?.OrderId);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_WithEmptyAdditionalGroups_RemovesExistingGroups()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        existingGroups.AdditionalGroups.Add(CreateGroup("ddcfddaf-d15d-44fe-8472-0f3ee9f0179d", 1, "Existing additional group"));

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null, []);

        Assert.Empty(result.AdditionalGroups);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_WithActions_MapsManualAndCustomRunbookActions()
    {
        const string runbookId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Automation/automationAccounts/account/runbooks/runbook";
        RecoveryPlanGroupActionInput[] preActions =
        [
            new(RecoveryPlanGroupActionKind.ManualAction, "Confirm failover", "Wait for approval", 60, null, null)
        ];
        RecoveryPlanGroupActionInput[] postActions =
        [
            new(RecoveryPlanGroupActionKind.CustomRunbook, "Prepare database", "Run preparation", 30, runbookId, new Dictionary<string, string> { ["mode"] = "safe" })
        ];

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(null, null, null, preActions, postActions);

        var manualAction = Assert.IsType<RecoveryGroupManualAction>(Assert.Single(result.DefaultGroup.Properties!.PreActions));
        Assert.Equal("Confirm failover", manualAction.Name);
        Assert.Equal("Wait for approval", manualAction.Description);
        Assert.Equal(60, manualAction.TimeoutInMinutes);
        var runbookAction = Assert.IsType<RecoveryGroupCustomRunbookAction>(Assert.Single(result.DefaultGroup.Properties.PostActions));
        Assert.Equal("Prepare database", runbookAction.Name);
        Assert.Equal("Run preparation", runbookAction.Description);
        Assert.Equal(runbookId, runbookAction.ActionResourceId.ToString());
        Assert.Equal("safe", runbookAction.Parameters["mode"]);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_WithOmittedActions_PreservesExistingActions()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var existingAction = new RecoveryGroupManualAction("Existing action", 10);
        existingDefaultGroup.Properties!.PreActions.Add(existingAction);
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null);

        Assert.Same(existingAction, Assert.Single(result.DefaultGroup.Properties.PreActions));
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_RejectsAdditionalGroupIdMatchingDefaultGroupId()
    {
        const string defaultGroupId = "7f35c9f5-bec2-455d-8161-c904b2532e5d";
        var existingDefaultGroup = CreateGroup(defaultGroupId, 0, "Existing default group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        RecoveryPlanGroupInput[] additionalGroups =
        [
            new(defaultGroupId, 1, "Additional recovery group", null, null)
        ];

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null, additionalGroups));

        Assert.Contains("cannot match the default recovery group", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_RejectsProvidedIdMatchingIdPreservedByOrder()
    {
        const string existingAdditionalGroupId = "ddcfddaf-d15d-44fe-8472-0f3ee9f0179d";
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        existingGroups.AdditionalGroups.Add(CreateGroup(existingAdditionalGroupId, 1, "Existing additional group"));
        RecoveryPlanGroupInput[] additionalGroups =
        [
            new(null, 1, "Preserve existing group by order", null, null),
            new(existingAdditionalGroupId, 2, "Reuse existing group by ID", null, null)
        ];

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null, additionalGroups));

        Assert.Contains("groupUniqueId values must be unique", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveRecoveryPlanDescription_ForUpdate_PreservesExistingDescription()
    {
        string result = ResilienceManagementService.ResolveRecoveryPlanDescription(null, "Existing description");

        Assert.Equal("Existing description", result);
    }

    [Fact]
    public void ResolveRecoveryPlanDescription_ForCreate_RequiresDescription()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ResolveRecoveryPlanDescription(null, null));

        Assert.Contains("--plan-description is required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateRecoveryPlanUpdateResourcesResult_MapsFailedResourceDetails()
    {
        RecoveryMembersData failedResource = ModelReaderWriter.Read<RecoveryMembersData>(BinaryData.FromObjectAsJson(new
        {
            id = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012",
            name = "12345678-9012-3456-7890-123456789012",
            type = "Microsoft.AzureResilienceManagement/recoveryPlans/recoveryResources",
            properties = new
            {
                recoveryResourceUniqueId = "12345678-9012-3456-7890-123456789012",
                resourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
                errorDetails = new
                {
                    code = "InvalidConfiguration",
                    message = "The recovery resource configuration is invalid."
                }
            }
        }))!;

        RecoveryPlanUpdateResourcesResult result = ResilienceManagementService.CreateRecoveryPlanUpdateResourcesResult([failedResource]);

        RecoveryPlanUpdateResourcesFailedResource failedResult = Assert.Single(result.FailedResources);
        Assert.Equal(failedResource.Id.ToString(), failedResult.RecoveryResourceId);
        Assert.Equal("12345678-9012-3456-7890-123456789012", failedResult.RecoveryResourceUniqueId);
        Assert.Equal(failedResource.Properties.ResourceId.ToString(), failedResult.AzureResourceId);
        Assert.Equal("InvalidConfiguration", failedResult.Error?.Code);
        Assert.Equal("The recovery resource configuration is invalid.", failedResult.Error?.Message);
    }

    [Fact]
    public void CreateReadinessError_DefaultsMissingRecommendationsToEmptyList()
    {
        ResilienceManagementJobErrorInfo providerError = ModelReaderWriter.Read<ResilienceManagementJobErrorInfo>(BinaryData.FromObjectAsJson(new
        {
            errorCode = "NotReady",
            errorMessage = "The recoveryplan is not ready."
        }))!;

        RecoveryPlanReadinessError? result = ResilienceManagementService.CreateReadinessError(providerError);

        Assert.NotNull(result);
        Assert.Empty(result.Recommendations);
    }

    [Fact]
    public void CreateFailoverRequestContent_MapsSelectorsAndConsent()
    {
        const string resourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";

        ResilienceManagementFailoverContent result = ResilienceManagementService.CreateFailoverRequestContent(
            ["eastus", "westus2-az3"],
            [resourceId],
            "Allowed");

        Assert.Equal(ResilienceManagementFailoverDirection.FromSpecificLocations, result.FailoverDirection);
        Assert.NotNull(result.FailoverRequestProperties);
        Assert.Equal(["eastus", "westus2-az3"], result.FailoverRequestProperties.SourceLocations);
        Assert.Equal(resourceId, Assert.Single(result.FailoverRequestProperties.SelectedResourceIds).ToString());
        Assert.Equal("Allowed", result.FailoverRequestProperties.ExecutionConfigurationsUserConsent?.ToString());
    }

    [Fact]
    public void CreateReprotectRequestContent_MapsSelectedResources()
    {
        const string firstResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";
        const string secondResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/23456789-0123-4567-8901-234567890123";

        ReprotectContent result = ResilienceManagementService.CreateReprotectRequestContent(
            [firstResourceId, secondResourceId]);

        Assert.Equal(
            [firstResourceId, secondResourceId],
            result.ReprotectRequestSelectedResourceIds.Select(resourceId => resourceId.ToString()));
    }

    [Fact]
    public void CreateRecoveryPlanValidateForFailoverResult_MapsQualificationDetails()
    {
        ValidateForRecoveryOperationBaseResult sdkResult = ModelReaderWriter.Read<ValidateForRecoveryOperationBaseResult>(BinaryData.FromObjectAsJson(new
        {
            recoveryResourceQualifications = new[]
            {
                new
                {
                    recoveryResource = new
                    {
                        id = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012",
                        name = "12345678-9012-3456-7890-123456789012",
                        type = "Microsoft.AzureResilienceManagement/recoveryPlans/recoveryResources",
                        properties = new
                        {
                            recoveryResourceUniqueId = "12345678-9012-3456-7890-123456789012",
                            resourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
                            resourceLocation = "eastus",
                            resourcePhysicalZones = new[] { "eastus-az1" },
                            inclusionState = "Included",
                            protectionStatus = "Protected",
                            needsAttention = true,
                            attentionReasons = new[] { "Replication lag" }
                        }
                    },
                    operationQualificationDetails = new
                    {
                        qualificationState = "NotQualified",
                        notQualifiedReasons = new[] { "ProtectionNotComplete" }
                    }
                }
            }
        }))!;

        RecoveryPlanValidateForFailoverResult result = ResilienceManagementService.CreateRecoveryPlanValidateForFailoverResult(
            "11111111-1111-1111-1111-111111111111",
            sdkResult.RecoveryResourceQualifications);

        Assert.Equal("11111111-1111-1111-1111-111111111111", result.OperationId);
        RecoveryPlanFailoverQualification qualification = Assert.Single(result.RecoveryResourceQualifications);
        Assert.Equal("12345678-9012-3456-7890-123456789012", qualification.RecoveryResourceUniqueId);
        Assert.Equal("eastus", qualification.AzureResourceLocation);
        Assert.Equal("NotQualified", qualification.QualificationState);
        Assert.Equal(["ProtectionNotComplete"], qualification.NotQualifiedReasons);
        Assert.Equal(["eastus-az1"], qualification.ResourcePhysicalZones);
        Assert.Equal("Included", qualification.InclusionState);
        Assert.Equal("Protected", qualification.ProtectionStatus);
        Assert.True(qualification.IsAttentionRequired);
        Assert.Equal(["Replication lag"], qualification.AttentionReasons);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForFailoverResult_ParsesOperationStatusProperties()
    {
        BinaryData operationResponse = BinaryData.FromObjectAsJson(new
        {
            status = "Succeeded",
            properties = """
                {"recoveryResourceQualifications":[{"recoveryResource":{"id":"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/resource1","name":"resource1","properties":{"recoveryResourceUniqueId":"resource1"}},"operationQualificationDetails":{"qualificationState":"Qualified","notQualifiedReasons":[]}}]}
                """
        });

        RecoveryPlanValidateForFailoverResult result = ResilienceManagementService.CreateRecoveryPlanValidateForFailoverResult(
            "11111111-1111-1111-1111-111111111111",
            operationResponse,
            []);

        RecoveryPlanFailoverQualification qualification = Assert.Single(result.RecoveryResourceQualifications);
        Assert.Equal("resource1", qualification.RecoveryResourceUniqueId);
        Assert.Equal("Qualified", qualification.QualificationState);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForOperationResult_ParsesSuccessfulStringProperties()
    {
        BinaryData operationResponse = BinaryData.FromObjectAsJson(new
        {
            status = "Succeeded",
            properties = """
                {"error":{"code":"None","message":"Operation validated successfully."}}
                """
        });

        RecoveryPlanValidateForOperationResult result = ResilienceManagementService.CreateRecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            "Failover",
            operationResponse);

        Assert.True(result.IsValid);
        Assert.Equal("Failover", result.OperationName);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForOperationResult_MapsMissingPropertiesAsValid()
    {
        BinaryData operationResponse = BinaryData.FromObjectAsJson(new
        {
            status = "Succeeded",
            properties = (object?)null
        });

        RecoveryPlanValidateForOperationResult result = ResilienceManagementService.CreateRecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            "Failover",
            operationResponse);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForOperationResult_MapsBlockedOperationFromObjectProperties()
    {
        BinaryData operationResponse = BinaryData.FromObjectAsJson(new
        {
            status = "Succeeded",
            properties = new
            {
                error = new
                {
                    code = "RecoveryPlanStateDoesNotSupportOperation",
                    message = "Operation Reprotect is not allowed for the current recoveryplan state."
                }
            }
        });

        RecoveryPlanValidateForOperationResult result = ResilienceManagementService.CreateRecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            "Reprotect",
            operationResponse);

        Assert.False(result.IsValid);
        Assert.Equal("RecoveryPlanStateDoesNotSupportOperation", result.ErrorCode);
        Assert.Equal("Operation Reprotect is not allowed for the current recoveryplan state.", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Success")]
    public void CreateRecoveryPlanValidateForOperationResult_MapsNonNoneErrorAsInvalid(string? errorCode)
    {
        BinaryData operationResponse = BinaryData.FromObjectAsJson(new
        {
            status = "Succeeded",
            properties = new
            {
                error = new
                {
                    code = errorCode,
                    message = "Operation validation failed."
                }
            }
        });

        RecoveryPlanValidateForOperationResult result = ResilienceManagementService.CreateRecoveryPlanValidateForOperationResult(
            "11111111-1111-1111-1111-111111111111",
            "Failover",
            operationResponse);

        Assert.False(result.IsValid);
        Assert.Equal(errorCode, result.ErrorCode);
        Assert.Equal("Operation validation failed.", result.ErrorMessage);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForFailoverResult_HandlesMissingQualifications()
    {
        RecoveryPlanValidateForFailoverResult result = ResilienceManagementService.CreateRecoveryPlanValidateForFailoverResult(
            "11111111-1111-1111-1111-111111111111",
            null);

        Assert.Empty(result.RecoveryResourceQualifications);
    }

    [Fact]
    public void CreateRecoveryPlanValidateForFailoverResult_HandlesMissingQualificationDetails()
    {
        ValidateForRecoveryOperationBaseResult sdkResult = ModelReaderWriter.Read<ValidateForRecoveryOperationBaseResult>(BinaryData.FromObjectAsJson(new
        {
            recoveryResourceQualifications = new[]
            {
                new
                {
                    recoveryResource = new
                    {
                        id = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012",
                        name = "12345678-9012-3456-7890-123456789012",
                        type = "Microsoft.AzureResilienceManagement/recoveryPlans/recoveryResources",
                        properties = new
                        {
                            recoveryResourceUniqueId = "12345678-9012-3456-7890-123456789012"
                        }
                    }
                }
            }
        }))!;

        RecoveryPlanValidateForFailoverResult result = ResilienceManagementService.CreateRecoveryPlanValidateForFailoverResult(
            "11111111-1111-1111-1111-111111111111",
            sdkResult.RecoveryResourceQualifications);

        RecoveryPlanFailoverQualification qualification = Assert.Single(result.RecoveryResourceQualifications);
        Assert.Equal("Unknown", qualification.QualificationState);
        Assert.Empty(qualification.NotQualifiedReasons);
        Assert.Empty(qualification.ResourcePhysicalZones);
        Assert.Empty(qualification.AttentionReasons);
    }

    [Fact]
    public void CreateRecoveryPlanInfo_MapsSupportedPlanFields()
    {
        RecoveryPlanData recoveryPlan = ModelReaderWriter.Read<RecoveryPlanData>(BinaryData.FromObjectAsJson(new
        {
            id = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1",
            name = "plan1",
            type = "Microsoft.AzureResilienceManagement/recoveryPlans",
            identity = new
            {
                type = "UserAssigned",
                userAssignedIdentities = new Dictionary<string, object>
                {
                    [UserAssignedIdentityResourceId] = new { }
                }
            },
            properties = new
            {
                provisioningState = "Succeeded",
                planType = "Zonal",
                planState = "UnderEdit",
                planDescription = "description",
                recoveryGroupsSetting = new
                {
                    defaultGroup = new
                    {
                        properties = new
                        {
                            groupUniqueId = "12345678-9012-3456-7890-123456789012",
                            orderId = 0,
                            description = "default"
                        }
                    },
                    additionalGroups = new[]
                    {
                        new
                        {
                            properties = new
                            {
                                groupUniqueId = "12345678-9012-3456-7890-123456789013",
                                orderId = 1,
                                description = "additional"
                            }
                        }
                    }
                }
            }
        }))!;

        RecoveryPlanInfo result = ResilienceManagementService.CreateRecoveryPlanInfo(recoveryPlan);

        Assert.Equal(recoveryPlan.Id.ToString(), result.Id);
        Assert.Equal("plan1", result.Name);
        Assert.Equal("Zonal", result.PlanType);
        Assert.Equal("description", result.PlanDescription);
        Assert.Equal("Succeeded", result.ProvisioningState);
        Assert.Equal("UnderEdit", result.PlanState);
        Assert.Equal("UserAssigned", result.Identity?.Type);
        Assert.Equal([UserAssignedIdentityResourceId], result.Identity?.UserAssignedIdentityResourceIds);
        Assert.Equal("12345678-9012-3456-7890-123456789012", result.DefaultGroup?.GroupUniqueId);
        RecoveryPlanGroupInfo additionalGroup = Assert.Single(result.AdditionalGroups);
        Assert.Equal("12345678-9012-3456-7890-123456789013", additionalGroup.GroupUniqueId);
        Assert.Equal(1, additionalGroup.OrderId);
        Assert.Equal("additional", additionalGroup.Description);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_FirstInclusionRequiresProtectionConfiguration()
    {
        RecoveryMembersData requested = CreateRecoveryResource(ResourceInclusionState.Included);
        RecoveryMembersData existing = CreateRecoveryResource();

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("selectedProtectionSolutionType", exception.Message, StringComparison.Ordinal);
        Assert.Contains("selectedProtectionSolutionSetting", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_ReinclusionRequiresProtectionConfigurationClearedByExclusion()
    {
        RecoveryMembersData requested = CreateRecoveryResource(ResourceInclusionState.Included);
        RecoveryMembersData existing = CreateRecoveryResource(ResourceInclusionState.Excluded);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("selectedProtectionSolutionType", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_NonePreservesExistingProtectionConfiguration()
    {
        RecoveryMembersData requested = CreateRecoveryResource(solutionType: ResourceProtectionSolutionType.None);
        RecoveryMembersData existing = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            CreateCustomRunbookSetting());

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_CustomRunbookRequiresMandatoryActions()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            new ResourceCustomProtectionSetting());
        RecoveryMembersData existing = CreateRecoveryResource();

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("failoverAction.resourceId", exception.Message, StringComparison.Ordinal);
        Assert.Contains("reprotectAction.resourceId", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_DoesNotRequireRecoveryGroupOrIdentity()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            CreateCustomRunbookSetting());
        RecoveryMembersData existing = CreateRecoveryResource();

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_ExcludedResourceDoesNotRequireProtectionConfiguration()
    {
        RecoveryMembersData requested = CreateRecoveryResource(ResourceInclusionState.Excluded);
        RecoveryMembersData existing = CreateRecoveryResource();

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_ExclusionPreservesExistingProtectionConfiguration()
    {
        ResourceCustomProtectionSetting existingSetting = CreateCustomRunbookSetting();
        RecoveryMembersData requested = CreateRecoveryResource(ResourceInclusionState.Excluded);
        RecoveryMembersData existing = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            existingSetting);

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);

        Assert.Equal(ResourceInclusionState.Included, existing.Properties!.InclusionState);
        Assert.Equal(ResourceProtectionSolutionType.CustomRunbook, existing.Properties.SelectedProtectionSolutionType);
        Assert.Same(existingSetting, existing.Properties.SelectedProtectionSolutionSetting);
        Assert.Null(requested.Properties!.SelectedProtectionSolutionType);
        Assert.Null(requested.Properties.SelectedProtectionSolutionSetting);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_DelegatesExcludedResourcePropertyValidationToService()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Excluded,
            ResourceProtectionSolutionType.CustomRunbook,
            CreateCustomRunbookSetting());
        requested.Properties!.RecoveryGroupId = "7f35c9f5-bec2-455d-8161-c904b2532e5d";
        requested.Properties.AssociatedIdentity = new ResilienceManagementAssociatedIdentity(ManagedServiceIdentityType.UserAssigned)
        {
            UserAssignedIdentity = new ResourceIdentifier(UserAssignedIdentityResourceId)
        };
        RecoveryMembersData existing = CreateRecoveryResource(ResourceInclusionState.Excluded);

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_CustomRunbookRequiresRunbookResourceIds()
    {
        ResourceCustomProtectionSetting setting = CreateCustomRunbookSetting();
        setting.FailoverActionResourceId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            setting);
        RecoveryMembersData existing = CreateRecoveryResource();

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("Microsoft.Automation/automationAccounts/runbooks", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_RejectsAzureNativeForIncludedResource()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureNative,
            new ResourceNativeProtectionSolutionSetting());
        RecoveryMembersData existing = CreateRecoveryResource();

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("cannot use AzureNative", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_RejectsMismatchedProtectionSetting()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CustomRunbook,
            new ResourceSiteRecoveryProtectionSetting());
        RecoveryMembersData existing = CreateRecoveryResource();

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("matches selectedProtectionSolutionType", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AllowsCrossZoneVmRecoveryForVirtualMachine()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CrossZoneVMRecovery,
            new ResourceCrossZoneVmRecoveryProtectionSetting
            {
                TargetZone = "2",
                CapacityReservationGroupId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/capacityReservationGroups/crg")
            });
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Theory]
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account", null, "only for a Microsoft.Compute/virtualMachines resource")]
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm", "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/net", "capacityReservationGroupId")]
    public void ValidateRecoveryResourceUpdate_RejectsInvalidCrossZoneVmRecovery(string azureResourceId, string? capacityReservationGroupId, string expectedMessage)
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.CrossZoneVMRecovery,
            new ResourceCrossZoneVmRecoveryProtectionSetting
            {
                CapacityReservationGroupId = capacityReservationGroupId is null ? null : new ResourceIdentifier(capacityReservationGroupId)
            });
        RecoveryMembersData existing = CreateRecoveryResource(azureResourceId: azureResourceId);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_RejectsAzureSiteRecoveryForNonVirtualMachine()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            CreateSiteRecoverySetting());
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("only for a Microsoft.Compute/virtualMachines resource", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AzureSiteRecoveryRequiresDiskReprotectDetails()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            new ResourceSiteRecoveryProtectionSetting());
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("diskReprotectInputDetails", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AzureSiteRecoveryRequiresTypedDiskAndStorageIds()
    {
        ResourceSiteRecoveryProtectionSetting setting = CreateSiteRecoverySetting();
        setting.DiskReprotectInputDetails[0].DiskResourceId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            setting);
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("Microsoft.Compute/disks", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AzureSiteRecoveryRequiresVirtualNetworkId()
    {
        ResourceSiteRecoveryProtectionSetting setting = CreateSiteRecoverySetting();
        setting.TestFailoverParamsNetworkResourceId = null;
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            setting);
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("testFailoverParams.networkResourceId", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AzureSiteRecoveryRequiresTypedVirtualNetworkId()
    {
        ResourceSiteRecoveryProtectionSetting setting = CreateSiteRecoverySetting();
        setting.TestFailoverParamsNetworkResourceId = new ResourceIdentifier(
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account");
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            setting);
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("Microsoft.Network/virtualNetworks", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_AllowsValidAzureSiteRecoveryConfiguration()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            CreateSiteRecoverySetting());
        RecoveryMembersData existing = CreateProtectedSiteRecoveryResource();

        ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing);
    }

    [Fact]
    public void ValidateRecoveryResourceUpdate_RejectsAzureSiteRecoveryWhenProtectionIsNotDetected()
    {
        RecoveryMembersData requested = CreateRecoveryResource(
            ResourceInclusionState.Included,
            ResourceProtectionSolutionType.AzureSiteRecovery,
            CreateSiteRecoverySetting());
        RecoveryMembersData existing = CreateRecoveryResource(
            azureResourceId: "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ValidateRecoveryResourceUpdate(requested, existing));

        Assert.Contains("healthy Azure Site Recovery protection was not detected", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesNoneIdentity()
    {
        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.None, null);

        Assert.Equal(ManagedServiceIdentityType.None, result.ManagedServiceIdentityType);
        Assert.Empty(result.UserAssignedIdentities);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_UsesRequestedDefaultGroupIdForNewPlan()
    {
        const string groupId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(null, "Default group", defaultGroupUniqueId: groupId);

        Assert.Equal(groupId, result.DefaultGroup.Properties!.GroupUniqueId);
        Assert.Equal(0, result.DefaultGroup.Properties.OrderId);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_RejectsChangingExistingDefaultGroupId()
    {
        RecoveryGroupsSetting existing = ResilienceManagementService.CreateRecoveryGroupsSetting(null, "Default group", defaultGroupUniqueId: "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            ResilienceManagementService.CreateRecoveryGroupsSetting(existing, null, defaultGroupUniqueId: "11111111-2222-3333-4444-555555555555"));

        Assert.Contains("cannot be changed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DrillConfiguration_CreateAppliesSuppliedIdentities()
    {
        string chaos = $"{{\"identity\":{{\"type\":\"UserAssigned\",\"userAssignedIdentity\":\"{UserAssignedIdentityResourceId}\"}},\"chaosResourceIdentityForFaults\":{{\"type\":\"SystemAssigned\"}}}}";
        string recoveryPlan = $"{{\"identity\":{{\"type\":\"UserAssigned\",\"userAssignedIdentity\":\"{UserAssignedIdentityResourceId}\"}}}}";
        string monitoring = "{\"identity\":{\"type\":\"SystemAssigned\"}}";

        DrillProperties properties = Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.DrillConfiguration.Create(
            DrillKind.Zonal, null, null, null, chaos, recoveryPlan, monitoring);

        Assert.Equal(ManagedServiceIdentityType.UserAssigned, properties.ChaosResourceProperties.Identity.Type);
        Assert.Equal(UserAssignedIdentityResourceId, properties.ChaosResourceProperties.Identity.UserAssignedIdentity.ToString(), ignoreCase: true);
        Assert.Equal(ManagedServiceIdentityType.SystemAssigned, properties.ChaosResourceProperties.ChaosResourceIdentityForFaults.Type);
        Assert.Equal(ManagedServiceIdentityType.UserAssigned, properties.RecoveryPlanProperties.Identity.Type);
        Assert.Equal(ManagedServiceIdentityType.SystemAssigned, properties.MonitoringProperties.Identity.Type);
    }

    [Theory]
    [InlineData("{\"identity\":{\"type\":\"SystemAssigned\"}}", "requires chaosResourceIdentityForFaults")]
    [InlineData("{\"identity\":{\"type\":\"SystemAssigned\"},\"chaosResourceIdentityForFaults\":{\"type\":\"Bogus\"}}", "identity type must be")]
    [InlineData("{\"identity\":{\"type\":\"SystemAssigned\"},\"chaosResourceIdentityForFaults\":{\"type\":\"SystemAssigned\"},\"extra\":{}}", "unsupported, read-only, or duplicate")]
    public void DrillConfiguration_CreateRejectsInvalidChaosIdentities(string chaos, string expectedMessage)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.DrillConfiguration.Create(DrillKind.Zonal, null, null, null, chaos));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DrillConfiguration_UpdateWritesOnlySuppliedIdentitySections()
    {
        DrillUpdateProperties properties = Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.DrillConfiguration.Update(
            null, null, null, chaos: "{\"chaosResourceIdentityForFaults\":{\"type\":\"SystemAssigned\"}}", monitoring: "{\"identity\":{\"type\":\"SystemAssigned\"}}");

        using JsonDocument json = JsonDocument.Parse(ModelReaderWriter.Write(properties, ModelReaderWriterOptions.Json).ToString());
        JsonElement chaos = json.RootElement.GetProperty("chaosResourceProperties");
        Assert.False(chaos.TryGetProperty("identity", out _));
        Assert.Equal("SystemAssigned", chaos.GetProperty("chaosResourceIdentityForFaults").GetProperty("type").GetString());
        Assert.True(json.RootElement.TryGetProperty("monitoringProperties", out _));
        Assert.False(json.RootElement.TryGetProperty("recoveryPlanProperties", out _));
    }

    [Fact]
    public void DrillConfiguration_ParseManagedIdentityBuildsUserAssignedIdentity()
    {
        ManagedServiceIdentity? identity = Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.DrillConfiguration.ParseManagedIdentity(
            $"{{\"type\":\"SystemAssigned, UserAssigned\",\"userAssignedIdentities\":[\"{UserAssignedIdentityResourceId}\"]}}");

        Assert.NotNull(identity);
        Assert.Equal(ManagedServiceIdentityType.SystemAssignedUserAssigned, identity.ManagedServiceIdentityType);
        Assert.Contains(identity.UserAssignedIdentities.Keys, key => string.Equals(key.ToString(), UserAssignedIdentityResourceId, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("{\"type\":\"UserAssigned\"}", "exactly when type includes UserAssigned")]
    [InlineData("{\"type\":\"SystemAssigned\",\"userAssignedIdentities\":[\"/subscriptions/x\"]}", "userAssignedIdentities")]
    [InlineData("{\"userAssignedIdentities\":[]}", "requires type")]
    [InlineData("{\"type\":\"Bogus\"}", "type must be")]
    public void DrillConfiguration_ParseManagedIdentityRejectsInvalidInput(string json, string expectedMessage)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.DrillConfiguration.ParseManagedIdentity(json));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesUserAssignedIdentity()
    {
        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.UserAssigned, UserAssignedIdentityResourceId);

        Assert.Equal(ManagedServiceIdentityType.UserAssigned, result.ManagedServiceIdentityType);
        Assert.Contains(new ResourceIdentifier(UserAssignedIdentityResourceId), result.UserAssignedIdentities.Keys);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_AllowsExistingUserAssignedIdentity()
    {
        var identityResourceId = new ResourceIdentifier(UserAssignedIdentityResourceId);
        var existingIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
        existingIdentity.UserAssignedIdentities.Add(identityResourceId, new UserAssignedIdentity());

        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.UserAssigned, UserAssignedIdentityResourceId, existingIdentity);

        Assert.Equal(ManagedServiceIdentityType.UserAssigned, result.ManagedServiceIdentityType);
        Assert.NotNull(result.UserAssignedIdentities[identityResourceId]);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_RejectsChangingExistingUserAssignedIdentity()
    {
        var existingIdentityResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/oldIdentity");
        var existingIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
        existingIdentity.UserAssignedIdentities.Add(existingIdentityResourceId, new UserAssignedIdentity());

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.UserAssigned, UserAssignedIdentityResourceId, existingIdentity));

        Assert.Contains("not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesSystemAndUserAssignedIdentity()
    {
        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.SystemAndUserAssigned, UserAssignedIdentityResourceId);

        Assert.Equal(ManagedServiceIdentityType.SystemAssignedUserAssigned, result.ManagedServiceIdentityType);
        Assert.NotNull(result.UserAssignedIdentities[new ResourceIdentifier(UserAssignedIdentityResourceId)]);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesSystemAssignedIdentityWhenResourceIdIsNull()
    {
        var existingIdentity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
        existingIdentity.UserAssignedIdentities.Add(new ResourceIdentifier(UserAssignedIdentityResourceId), new UserAssignedIdentity());

        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(RecoveryPlanIdentityKind.SystemAssigned, null, existingIdentity);

        Assert.Equal(ManagedServiceIdentityType.SystemAssigned, result.ManagedServiceIdentityType);
        Assert.Empty(result.UserAssignedIdentities);
    }

    [Theory]
    [InlineData("not-a-resource-id")]
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account")]
    public void ParseUserAssignedIdentityResourceId_RejectsInvalidResourceId(string resourceId)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ParseUserAssignedIdentityResourceId(resourceId));

        Assert.Contains("Microsoft.ManagedIdentity/userAssignedIdentities", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RecoveryGroup CreateGroup(string groupId, int sequenceNumber, string description)
        => new()
        {
            Properties = new RecoveryGroupProperties(groupId, sequenceNumber, description)
        };

    private static RecoveryMembersData CreateRecoveryResource(
        ResourceInclusionState? inclusionState = null,
        ResourceProtectionSolutionType? solutionType = null,
        ResourceBaseProtectionSolutionSetting? solutionSetting = null,
        string? azureResourceId = null)
    {
        RecoveryMembersData resource = azureResourceId is null
            ? new RecoveryMembersData()
            : ModelReaderWriter.Read<RecoveryMembersData>(BinaryData.FromObjectAsJson(new
            {
                properties = new
                {
                    recoveryResourceUniqueId = "12345678-9012-3456-7890-123456789012",
                    resourceId = azureResourceId
                }
            }))!;
        resource.Properties ??= new RecoveryResourceProperties("12345678-9012-3456-7890-123456789012");
        resource.Properties.InclusionState = inclusionState;
        resource.Properties.SelectedProtectionSolutionType = solutionType;
        resource.Properties.SelectedProtectionSolutionSetting = solutionSetting;
        return resource;
    }

    private static RecoveryMembersData CreateProtectedSiteRecoveryResource()
        => ModelReaderWriter.Read<RecoveryMembersData>(BinaryData.FromObjectAsJson(new
        {
            properties = new
            {
                recoveryResourceUniqueId = "12345678-9012-3456-7890-123456789012",
                resourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
                protectionStatus = "Protected",
                resourceProtectionSolutions = new[]
                {
                    new
                    {
                        protectionSolutionType = "AzureSiteRecovery",
                        protectionStatus = "Protected",
                        resourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm"
                    }
                }
            }
        }))!;

    private static ResourceCustomProtectionSetting CreateCustomRunbookSetting()
        => new()
        {
            FailoverActionResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Automation/automationAccounts/account/runbooks/failover"),
            ReprotectActionResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Automation/automationAccounts/account/runbooks/reprotect")
        };

    private static ResourceSiteRecoveryProtectionSetting CreateSiteRecoverySetting()
    {
        var setting = new ResourceSiteRecoveryProtectionSetting
        {
            TestFailoverParamsNetworkResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/network")
        };
        setting.DiskReprotectInputDetails.Add(new DiskReprotectInputDetails
        {
            DiskResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Compute/disks/disk"),
            StagingStorageAccountResourceId = new ResourceIdentifier("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/staging")
        });
        return setting;
    }
}
