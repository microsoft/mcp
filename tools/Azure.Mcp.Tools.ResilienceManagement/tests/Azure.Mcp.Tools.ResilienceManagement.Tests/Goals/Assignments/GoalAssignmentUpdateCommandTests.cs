// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentUpdateCommandTests : CommandUnitTestsBase<GoalAssignmentUpdateCommand, IResilienceManagementService>
{
    private const string IndicatorResourceId = "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1";
    private const string ObjectiveResourceId = "/subscriptions/00000000-0000-0000-0000-000000000001/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm2";
    private const string ValidArgs = $"--service-group sg1 --goal-assignment assignment1 --service-level-indicator-resource-id {IndicatorResourceId} --service-level-objective-resource-id {ObjectiveResourceId} --tenant tenant1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData($"--goal-assignment assignment1 --service-level-indicator-resource-id {IndicatorResourceId} --service-level-objective-resource-id {ObjectiveResourceId}", false)]
    [InlineData($"--service-group sg1 --service-level-indicator-resource-id {IndicatorResourceId} --service-level-objective-resource-id {ObjectiveResourceId}", false)]
    [InlineData($"--service-group sg1 --goal-assignment assignment1 --service-level-objective-resource-id {ObjectiveResourceId}", false)]
    [InlineData($"--service-group sg1 --goal-assignment assignment1 --service-level-indicator-resource-id {IndicatorResourceId}", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.UpdateGoalAssignmentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new GoalAssignmentInfo("id1", "assignment1"));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData("../sg", "assignment1")]
    [InlineData("sg1", "folder/assignment1")]
    public async Task ExecuteAsync_RejectsNonPathSegmentNames(string serviceGroup, string goalAssignment)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--goal-assignment", goalAssignment,
            "--service-level-indicator-resource-id", IndicatorResourceId,
            "--service-level-objective-resource-id", ObjectiveResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
        await Service.DidNotReceiveWithAnyArgs().UpdateGoalAssignmentAsync(
            default!,
            default!,
            default!,
            default!,
            default,
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("not-an-arm-id", ObjectiveResourceId)]
    [InlineData(IndicatorResourceId, "/providers/Microsoft.Management/serviceGroups/sg1")]
    public async Task ExecuteAsync_RejectsInvalidServiceLevelResourceIds(string indicatorResourceId, string objectiveResourceId)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--goal-assignment", "assignment1",
            "--service-level-indicator-resource-id", indicatorResourceId,
            "--service-level-objective-resource-id", objectiveResourceId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("valid Azure resource ID", response.Message);
        await Service.DidNotReceiveWithAnyArgs().UpdateGoalAssignmentAsync(
            default!,
            default!,
            default!,
            default!,
            default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUpdatedGoalAssignmentAndPassesExactArguments()
    {
        var expected = new GoalAssignmentInfo(
            "id1",
            "assignment1",
            new GoalAssignmentInfoProperties("Resiliency", "template1-id", "Succeeded"));
        Service.UpdateGoalAssignmentAsync("sg1", "assignment1", IndicatorResourceId, ObjectiveResourceId, "tenant1", Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentUpdateCommandResult);
        Assert.Equal("assignment1", result.GoalAssignment.Name);
        Assert.Equal("template1-id", result.GoalAssignment.Properties?.GoalTemplateId);
        Assert.Equal("Succeeded", result.GoalAssignment.Properties?.ProvisioningState);
        await Service.Received(1).UpdateGoalAssignmentAsync("sg1", "assignment1", IndicatorResourceId, ObjectiveResourceId, "tenant1", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "current state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "was not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123";
        Service.UpdateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTimeoutExceptionToGatewayTimeout()
    {
        Service.UpdateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException("Internal timeout details"));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("timed out", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Internal timeout details", response.Message);
    }
}
