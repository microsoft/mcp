// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public class GoalAssignmentGetCommandTests : CommandUnitTestsBase<GoalAssignmentGetCommand, IResilienceManagementService>
{
    private const string ServiceGroup = "sg1";

    [Fact]
    public async Task ExecuteAsync_ListsGoalAssignments_WhenNameOmitted()
    {
        var expected = new List<ResourceSummary> { new("id1", "assignment1"), new("id2", "assignment2") };
        Service.ListGoalAssignmentsAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentGetCommandResult);
        Assert.NotNull(result.GoalAssignments);
        Assert.Equal(2, result.GoalAssignments!.Count);
        Assert.Null(result.GoalAssignment);
    }

    [Fact]
    public async Task ExecuteAsync_GetsGoalAssignment_WhenNameProvided()
    {
        var expected = new GoalAssignmentInfo("id1", "assignment1");
        Service.GetGoalAssignmentAsync(ServiceGroup, "assignment1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup, "--name", "assignment1");

        var result = ValidateAndDeserializeResponse(response, ResilienceManagementJsonContext.Default.GoalAssignmentGetCommandResult);
        Assert.NotNull(result.GoalAssignment);
        Assert.Equal("assignment1", result.GoalAssignment!.Name);
        Assert.Null(result.GoalAssignments);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        var expectedError = "Test error";
        Service.ListGoalAssignmentsAsync(ServiceGroup, Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var response = await ExecuteCommandAsync("--service-group", ServiceGroup);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
