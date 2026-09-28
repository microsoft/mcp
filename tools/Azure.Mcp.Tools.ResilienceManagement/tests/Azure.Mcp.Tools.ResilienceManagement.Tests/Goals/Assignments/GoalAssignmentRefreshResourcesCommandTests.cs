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

public sealed class GoalAssignmentRefreshResourcesCommandTests
    : CommandUnitTestsBase<GoalAssignmentRefreshResourcesCommand, IResilienceManagementService>
{
    [Fact]
    public void Constructor_HasMetadata()
    {
        Assert.Equal("refresh-resources", Command.GetCommand().Name);
        Assert.Contains("rediscovers", Command.GetCommand().Description);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_PassesExactArguments()
    {
        Service.RefreshGoalAssignmentResourcesAsync("sg", "ga", "tenant1", Arg.Any<CancellationToken>())
            .Returns(new GoalAssignmentOperationResult("11111111-1111-1111-1111-111111111111", "Accepted", false));
        var response = await ExecuteCommandAsync("--service-group sg --goal-assignment ga --tenant tenant1");
        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentOperationResult);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.OperationId);
        Assert.Equal("Accepted", result.Status);
        Assert.False(result.HasCompleted);
        await Service.Received(1).RefreshGoalAssignmentResourcesAsync("sg", "ga", "tenant1", TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--service-group sg")]
    [InlineData("--goal-assignment ga")]
    [InlineData("--service-group sg/other --goal-assignment ga")]
    [InlineData("--service-group sg --goal-assignment ..")]
    [InlineData("--service-group sg --goal-assignment ga?query")]
    [InlineData("--service-group sg --goal-assignment ga\\other")]
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
        Service.RefreshGoalAssignmentResourcesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);
        var response = await ExecuteCommandAsync("--service-group sg --goal-assignment ga");
        Assert.Equal((HttpStatusCode)(status == 0 ? 500 : status), response.Status);
        Assert.DoesNotContain("secret-provider-detail", response.Message);
        Assert.Null(response.Results);
        Assert.Contains(Logger.ReceivedCalls(), call => call.GetMethodInfo().Name == "Log" && ReferenceEquals(call.GetArguments()[3], exception));
    }
}
