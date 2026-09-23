// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentRecommendCapacityCommandTests
    : CommandUnitTestsBase<GoalAssignmentRecommendCapacityCommand, IResilienceManagementService>
{
    private const string ResourceId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm";

    [Fact]
    public void Constructor_HasMetadata()
    {
        Assert.Equal("recommend-capacity", Command.GetCommand().Name);
        Assert.Contains("capacity recommendations", Command.GetCommand().Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_PassesExactArguments(bool selected)
    {
        Service.RecommendGoalAssignmentCapacityAsync("sg", "ga",
            Arg.Is<IReadOnlyList<string>?>(ids => selected ? ids != null && ids.SequenceEqual(new[] { ResourceId }) : ids == null),
            "tenant1", Arg.Any<CancellationToken>())
            .Returns(new GoalAssignmentOperationResult("11111111-1111-1111-1111-111111111111", "Accepted", false));
        var arguments = new List<string> { "--service-group", "sg", "--goal-assignment", "ga", "--tenant", "tenant1" };
        if (selected)
        {
            arguments.AddRange(["--resource-ids", ResourceId]);
        }
        var response = await ExecuteCommandAsync(arguments.ToArray());
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.OperationId);
        Assert.Equal("Accepted", result.Status);
        Assert.False(result.HasCompleted);
        Assert.Single(Service.ReceivedCalls());
        await Service.Received(1).RecommendGoalAssignmentCapacityAsync("sg", "ga",
            Arg.Is<IReadOnlyList<string>?>(ids => selected ? ids != null && ids.SequenceEqual(new[] { ResourceId }) : ids == null),
            "tenant1", TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--service-group sg")]
    [InlineData("--goal-assignment ga")]
    [InlineData("--service-group ../sg --goal-assignment ga")]
    [InlineData("--service-group sg --goal-assignment ga%2F")]
    [InlineData("--service-group sg --goal-assignment ga --resource-ids invalid")]
    [InlineData("--service-group sg --goal-assignment ga --resource-ids /subscriptions/bad/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm")]
    public async Task ExecuteAsync_InvalidInputDoesNotCallService(string arguments)
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await ExecuteCommandAsync(arguments)).Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(500)]
    [InlineData(0)]
    public async Task ExecuteAsync_SanitizesErrors(int status)
    {
        Exception exception = status == 0 ? new Exception("secret-provider-detail") : new RequestFailedException(status, "secret-provider-detail");
        Service.RecommendGoalAssignmentCapacityAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        var response = await ExecuteCommandAsync("--service-group sg --goal-assignment ga");
        Assert.Equal((HttpStatusCode)(status == 0 ? 500 : status), response.Status);
        Assert.DoesNotContain("secret-provider-detail", response.Message);
        Assert.Null(response.Results);
        Assert.Contains(Logger.ReceivedCalls(), call => call.GetMethodInfo().Name == "Log" && ReferenceEquals(call.GetArguments()[3], exception));
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDuplicateResourceIds()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg", "--goal-assignment", "ga",
            "--resource-ids", ResourceId, "--resource-ids", ResourceId.ToUpperInvariant());
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
