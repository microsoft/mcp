// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Resources;

public class GoalResourceGetCommandTests : CommandUnitTestsBase<GoalResourceGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";
    private const string GoalAssignment = "assignment1";

    [Fact]
    public async Task ExecuteAsync_ListsGoalResources_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "resource1"), new("id2", "resource2") };
        Service.ListGoalResourcesAsync(ServiceGroup, GoalAssignment, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--goal-assignment", GoalAssignment);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalResourceGetCommandResult);
        Assert.NotNull(result.GoalResources);
        Assert.Equal(2, result.GoalResources!.Count);
        Assert.Null(result.GoalResource);
    }

    [Fact]
    public async Task ExecuteAsync_GetsGoalResource_WhenNameProvided()
    {
        var expected = new GoalResourceInfo("id1", "resource1");
        Service.GetGoalResourceAsync(ServiceGroup, GoalAssignment, "resource1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--goal-assignment", GoalAssignment, "--name", "resource1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalResourceGetCommandResult);
        Assert.NotNull(result.GoalResource);
        Assert.Equal("resource1", result.GoalResource!.Name);
        Assert.Null(result.GoalResources);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListGoalResourcesAsync(ServiceGroup, GoalAssignment, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--goal-assignment", GoalAssignment);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
