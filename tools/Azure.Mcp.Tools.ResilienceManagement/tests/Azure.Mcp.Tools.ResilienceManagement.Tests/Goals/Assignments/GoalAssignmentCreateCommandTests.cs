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
    private const string ValidArgs =
        "--service-group sg1 --goal-assignment assignment1 " +
        "--require-zonal-resiliency false --require-regional-resiliency true " +
        "--regional-recovery-point-objective PT4H " +
        "--regional-recovery-time-objective PT24H --tenant tenant1";

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
    [InlineData("--goal-assignment assignment1 --require-zonal-resiliency true", false)]
    [InlineData("--service-group sg1 --require-zonal-resiliency true", false)]
    [InlineData("--service-group sg1 --goal-assignment assignment1", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.CreateGoalAssignmentAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<bool>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
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
            "--require-zonal-resiliency", "true",
            "--require-regional-resiliency", "false");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("single non-empty path segment", response.Message);
        await Service.DidNotReceiveWithAnyArgs().CreateGoalAssignmentAsync(
            default!,
            default!,
            default,
            default,
            default,
            default,
            default,
            TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task ExecuteAsync_RejectsAmbiguousPlanType(
        bool requireZonalResiliency,
        bool requireRegionalResiliency)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--goal-assignment", "assignment1",
            "--require-zonal-resiliency", requireZonalResiliency.ToString(),
            "--require-regional-resiliency", requireRegionalResiliency.ToString());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Exactly one", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsRegionalGoalWithoutBothObjectives()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--goal-assignment", "assignment1",
            "--require-zonal-resiliency", "false",
            "--require-regional-resiliency", "true",
            "--regional-recovery-point-objective", "PT4H");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("require regional-recovery-point-objective", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsHydratedGoalAssignmentAndPassesExactArguments()
    {
        var expected = new GoalAssignmentInfo(
            "id1",
            "assignment1",
            new GoalAssignmentInfoProperties(
                ProvisioningState: "Succeeded",
                RequireZonalResiliency: false,
                RequireRegionalResiliency: true,
                RegionalObjectives: new("PT4H", "PT24H")));
        Service.CreateGoalAssignmentAsync(
                "sg1",
                "assignment1",
                false,
                true,
                "PT4H",
                "PT24H",
                "tenant1",
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(ValidArgs);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentCreateCommandResult);
        Assert.Equal("assignment1", result.GoalAssignment.Name);
        Assert.Equal("Succeeded", result.GoalAssignment.Properties?.ProvisioningState);
        Assert.False(result.GoalAssignment.Properties?.RequireZonalResiliency);
        Assert.True(result.GoalAssignment.Properties?.RequireRegionalResiliency);
        Assert.Equal(
            "PT24H",
            result.GoalAssignment.Properties?.RegionalObjectives?.TargetRecoveryTimeObjective);
        await Service.Received(1).CreateGoalAssignmentAsync(
            "sg1",
            "assignment1",
            false,
            true,
            "PT4H",
            "PT24H",
            "tenant1",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesTimeout()
    {
        Service.CreateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
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
    [InlineData(HttpStatusCode.NotFound, "service group was not found")]
    [InlineData(HttpStatusCode.BadRequest, "request failed")]
    public async Task ExecuteAsync_SanitizesRequestFailedException(HttpStatusCode status, string expectedMessage)
    {
        const string providerDetails = "Sensitive provider details: request-id=123";
        Service.CreateGoalAssignmentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)status, providerDetails));

        var response = await ExecuteCommandAsync(ValidArgs);

        Assert.Equal(status, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(providerDetails, response.Message);
    }
}
