// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentRecommendCapacityCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentRecommendCapacityCommand, GoalAssignmentOperationResult>
{
    protected override string CommandName => "recommend-capacity";
    protected override bool Destructive => false;
    protected override bool Idempotent => false;
    protected override GoalAssignmentOperationResult ExpectedResult => new("sg1", "assignment1", OperationId, "Accepted", false);
    protected override Task<GoalAssignmentOperationResult> ConfigureServiceCall() =>
        Service.RecommendGoalAssignmentCapacityAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

    protected override void AssertPayloadArguments(object?[] arguments) =>
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<string>>(arguments[2]));

    [Theory]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    public async Task ExecuteAsync_EnforcesExactFiftyResourceBoundary(int count, bool valid)
    {
        string[] ids = Enumerable.Range(0, count).Select(index => ResourceId + index).ToArray();
        ConfigureServiceCall().Returns(ExpectedResult);

        var response = await ExecuteCommandAsync([.. Arguments, "--resource-ids", .. ids]);

        Assert.Equal(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (valid)
        {
            Assert.Equal(ids, Assert.IsAssignableFrom<IReadOnlyList<string>>(
                Assert.Single(Service.ReceivedCalls()).GetArguments()[2]));
        }
        else
        {
            Assert.Empty(Service.ReceivedCalls());
        }
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("/subscriptions/11111111-1111-1111-1111-111111111111")]
    public async Task ExecuteAsync_RejectsInvalidIdWithoutCallingService(string id)
    {
        var response = await ExecuteCommandAsync([.. Arguments, "--resource-ids", id]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsCaseInsensitiveDuplicateIdsWithoutCallingService()
    {
        var response = await ExecuteCommandAsync([.. Arguments, "--resource-ids", ResourceId, ResourceId.ToUpperInvariant()]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
