// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentCreateCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentCreateCommand, GoalAssignmentInfo>
{
    protected override string CommandName => "create";
    protected override GoalAssignmentInfo ExpectedResult => new("assignment-id", "assignment1",
        new("Succeeded", true));
    protected override string[] AdditionalArguments => ["--require-zonal-resiliency", "true", "--service-level-resource-ids", ServiceLevelResourceId];
    protected override Task<GoalAssignmentInfo> ConfigureServiceCall() =>
        Service.CreateGoalAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(),
            Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

    protected override void AssertPayloadArguments(object?[] arguments)
    {
        Assert.Equal(true, arguments[2]);
        Assert.Equal([ServiceLevelResourceId], Assert.IsAssignableFrom<IReadOnlyList<string>>(arguments[3]));
    }

    protected override void AssertSerializedResult(JsonElement result)
    {
        var assignment = result.GetProperty("goalAssignment");
        Assert.Equal("assignment1", assignment.GetProperty("name").GetString());
        Assert.Equal("assignment-id", assignment.GetProperty("id").GetString());
        Assert.True(assignment.GetProperty("properties").GetProperty("requireZonalResiliency").GetBoolean());
    }

    [Theory]
    [InlineData("--service-group sg1 --goal-assignment assignment1")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --require-zonal-resiliency invalid")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --require-zonal-resiliency false")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --require-zonal-resiliency true --service-level-resource-ids invalid")]
    public async Task ExecuteAsync_RejectsMissingBooleanOrInvalidInputWithoutServiceCall(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task FailedOperationWithSuccessfulHttpStatus_IsNotSuccess()
    {
        Service.CreateGoalAssignmentAsync("sg1", "goal1", true,
            Arg.Any<IReadOnlyList<string>?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(200, "provider-detail", "RHUBUserErrorServiceGroupMembersLimitExceeded", null));

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "goal1", "--require-zonal-resiliency", "true");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("500-member", response.Message);
        Assert.DoesNotContain("provider-detail", response.Message);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedTargetsRemainNullAndTrueIsPreserved()
    {
        ConfigureServiceCall().Returns(ExpectedResult);

        var response = await ExecuteCommandAsync("--service-group sg1 --goal-assignment assignment1 --require-zonal-resiliency true");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateGoalAssignmentAsync("sg1", "assignment1", true, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDuplicateTargetsWithoutCallingService()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "assignment1",
            "--require-zonal-resiliency", "true", "--service-level-resource-ids", ServiceLevelResourceId, ServiceLevelResourceId.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(ResourceId)]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg2/providers/Microsoft.Monitor/slis/sli1")]
    [InlineData("/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/other/sli1")]
    public async Task ExecuteAsync_RejectsWrongGroupOrTypeWithoutCallingService(string id)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "assignment1",
            "--require-zonal-resiliency", "true", "--service-level-resource-ids", id);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
