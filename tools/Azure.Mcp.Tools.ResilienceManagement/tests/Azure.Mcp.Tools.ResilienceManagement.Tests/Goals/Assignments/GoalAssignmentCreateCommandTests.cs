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

public sealed class GoalAssignmentCreateCommandTests : CommandUnitTestsBase<GoalAssignmentCreateCommand, IGoalAssignmentCreateService>
{
    private const string ValidArgs = "--service-group sg1 --goal-assignment assignment1 --goal-template template1 --tenant tenant1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(ValidArgs, true)]
    [InlineData("--goal-assignment assignment1 --goal-template template1", false)]
    [InlineData("--service-group sg1 --goal-template template1", false)]
    [InlineData("--service-group sg1 --goal-assignment assignment1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateGoalAssignmentAsync(
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
    [InlineData("../sg", "assignment1", "template1")]
    [InlineData("sg1", "folder/assignment1", "template1")]
    [InlineData("sg1", "assignment1", "folder\\template1")]
    public async Task ExecuteAsync_RejectsNonPathSegmentNames(string serviceGroup, string goalAssignment, string goalTemplate)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--goal-assignment", goalAssignment,
            "--goal-template", goalTemplate);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
        await Service.DidNotReceiveWithAnyArgs().CreateGoalAssignmentAsync(
            default!,
            default!,
            default!,
            default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHydratedGoalAssignmentAndPassesExactArguments()
    {
        var expected = new GoalAssignmentInfo(
            "id1",
            "assignment1",
            new GoalAssignmentInfoProperties("Resiliency", "template-id", "Succeeded"));
        Service.CreateGoalAssignmentAsync("sg1", "assignment1", "template1", "tenant1", Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        Assert.Equal("assignment1", result.GoalAssignment.Name);
        Assert.Equal("template-id", result.GoalAssignment.Properties?.GoalTemplateId);
        Assert.Equal("Succeeded", result.GoalAssignment.Properties?.ProvisioningState);
        await Service.Received(1).CreateGoalAssignmentAsync("sg1", "assignment1", "template1", "tenant1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesTimeout()
    {
        Service.CreateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new TimeoutException());

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.Status);
        Assert.Contains("Check the assignment state", response.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "conflicts with the current resource state")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "service group or goal template was not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123";
        Service.CreateGoalAssignmentAsync(
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
}
