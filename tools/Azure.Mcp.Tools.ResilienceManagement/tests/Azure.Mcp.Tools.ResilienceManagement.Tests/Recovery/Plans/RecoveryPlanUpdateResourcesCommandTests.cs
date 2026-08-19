// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Recovery.Plans;

public sealed class RecoveryPlanUpdateResourcesCommandTests : CommandUnitTestsBase<RecoveryPlanUpdateResourcesCommand, IResilienceManagementService>
{
    private const string RecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012";
    private const string OtherRecoveryResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789013";
    private const string ResourcesToUpdate = """
        [{"id":"/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.AzureResilienceManagement/recoveryPlans/plan1/recoveryResources/12345678-9012-3456-7890-123456789012","properties":{"recoveryResourceUniqueId":"12345678-9012-3456-7890-123456789012","inclusionState":"Included","selectedProtectionSolutionType":"AzureNative","selectedProtectionSolutionSetting":{"protectionSolutionType":"AzureNative"}}}]
        """;
    private const string ResourcesToUpdateWithoutId = """
        [{"properties":{"recoveryResourceUniqueId":"12345678-9012-3456-7890-123456789012","inclusionState":"Included"}}]
        """;

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("update-resources", command.Name);
        Assert.NotNull(command.Description);
        Assert.Contains("includes and configures a resource", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("excludes it from recovery operations", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("removes a resource", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CustomRunbook", command.Description, StringComparison.Ordinal);
        Assert.Contains("AzureSiteRecovery", command.Description, StringComparison.Ordinal);
        Assert.Contains("only for virtual machines", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("protection settings", command.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresAnUpdateOrRemoval()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("at least one", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidRecoveryPlanName()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "invalid_plan",
            "--resources-to-update", ResourcesToUpdateWithoutId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("5 to 24 characters", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ASCII letters, numbers, or hyphens", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsResourceUpdates()
    {
        Service.UpdateRecoveryPlanResourcesAsync(
            "sg1",
            "plan1",
            Arg.Is<UpdateRecoveryResourcesContent>(content =>
                content.ResourcesToUpdate.Count == 1 &&
                content.ResourcesToRemove.Count == 0 &&
                content.ResourcesToUpdate[0].Id.ToString() == RecoveryResourceId &&
                content.ResourcesToUpdate[0].Properties.InclusionState == ResourceInclusionState.Included &&
                content.ResourcesToUpdate[0].Properties.SelectedProtectionSolutionType == ResourceProtectionSolutionType.AzureNative &&
                content.ResourcesToUpdate[0].Properties.SelectedProtectionSolutionSetting != null),
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdateResult());

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", ResourcesToUpdate);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.RecoveryPlanUpdateResourcesCommandResult);
        Assert.Empty(result.Result.FailedResources);
        await Service.Received(1).UpdateRecoveryPlanResourcesAsync(
            "sg1",
            "plan1",
            Arg.Any<UpdateRecoveryResourcesContent>(),
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AllowsRemovalOnly()
    {
        Service.UpdateRecoveryPlanResourcesAsync(
            "sg1",
            "plan1",
            Arg.Is<UpdateRecoveryResourcesContent>(content =>
                content.ResourcesToUpdate.Count == 0 &&
                content.ResourcesToRemove.Count == 1 &&
                content.ResourcesToRemove[0].ToString() == RecoveryResourceId),
            null,
            null,
            Arg.Any<CancellationToken>())
            .Returns(UpdateResult());

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-remove", $"[\"{RecoveryResourceId}\"]");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidInclusionState()
    {
        string updates = ResourcesToUpdate.Replace("Included", "Invalid", StringComparison.Ordinal);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", updates);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Included, Excluded, or null", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsNullInclusionState()
    {
        string updates = ResourcesToUpdate.Replace("\"Included\"", "null", StringComparison.Ordinal);
        Service.UpdateRecoveryPlanResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<UpdateRecoveryResourcesContent>(content => content.ResourcesToUpdate[0].Properties.InclusionState == null),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(UpdateResult());

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", updates);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsReadOnlyIdToBeOmitted()
    {
        Service.UpdateRecoveryPlanResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<UpdateRecoveryResourcesContent>(content =>
                content.ResourcesToUpdate.Count == 1 &&
                content.ResourcesToUpdate[0].Properties.RecoveryResourceUniqueId == "12345678-9012-3456-7890-123456789012"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(UpdateResult());

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", ResourcesToUpdateWithoutId);

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMissingRecoveryResourceUniqueId()
    {
        string updates = ResourcesToUpdate.Replace("\"recoveryResourceUniqueId\":\"12345678-9012-3456-7890-123456789012\",", string.Empty, StringComparison.Ordinal);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", updates);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("recoveryResourceUniqueId GUID", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsIdThatDoesNotMatchRecoveryResourceUniqueId()
    {
        string updates = ResourcesToUpdate.Replace("recoveryResources/12345678-9012-3456-7890-123456789012", "recoveryResources/12345678-9012-3456-7890-123456789013", StringComparison.Ordinal);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", updates);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("properties.recoveryResourceUniqueId", response.Message);
        Assert.Contains("selected recovery plan", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsResourceFromAnotherPlan()
    {
        string updates = ResourcesToUpdate.Replace("recoveryPlans/plan1", "recoveryPlans/plan2", StringComparison.Ordinal);

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", updates);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("selected recovery plan", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsResourceInUpdateAndRemoval()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", ResourcesToUpdate,
            "--resources-to-remove", $"[\"{RecoveryResourceId}\",\"{OtherRecoveryResourceId}\"]");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("cannot be updated and removed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_SanitizesProviderFailure()
    {
        Service.UpdateRecoveryPlanResourcesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UpdateRecoveryResourcesContent>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.BadRequest, "provider details"));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recovery-plan", "plan1",
            "--resources-to-update", ResourcesToUpdate);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Verify the resource IDs and protection settings", response.Message);
        Assert.DoesNotContain("provider details", response.Message);
    }

    private static RecoveryPlanUpdateResourcesResult UpdateResult() => new([]);
}
