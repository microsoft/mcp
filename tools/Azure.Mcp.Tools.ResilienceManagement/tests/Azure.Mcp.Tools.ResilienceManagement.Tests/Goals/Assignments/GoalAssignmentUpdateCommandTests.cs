// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentUpdateCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentUpdateCommand, GoalAssignmentInfo>
{
    protected override string CommandName => "update";
    protected override GoalAssignmentInfo ExpectedResult => new("assignment-id", "assignment1",
        new("Succeeded", false));
    protected override string[] AdditionalArguments => ["--service-level-resource-ids", ServiceLevelResourceId];
    protected override Task<GoalAssignmentInfo> ConfigureServiceCall() =>
        Service.UpdateGoalAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

    protected override void AssertPayloadArguments(object?[] arguments)
    {
        Assert.Equal([ServiceLevelResourceId], Assert.IsAssignableFrom<IReadOnlyList<string>>(arguments[2]));
    }

    protected override void AssertSerializedResult(JsonElement result)
    {
        var assignment = result.GetProperty("goalAssignment");
        Assert.Equal("assignment1", assignment.GetProperty("name").GetString());
        Assert.Equal("Succeeded", assignment.GetProperty("properties").GetProperty("provisioningState").GetString());
        Assert.False(assignment.GetProperty("properties").GetProperty("requireZonalResiliency").GetBoolean());
    }

    [Theory]
    [InlineData("--service-group sg1 --goal-assignment assignment1")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --service-level-resource-ids invalid")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --service-level-resource-ids")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --clear-service-level-resources false")]
    [InlineData("--service-group sg1 --goal-assignment assignment1 --require-zonal-resiliency false")]
    public async Task ExecuteAsync_RejectsNoChangesOrInvalidIdsWithoutCallingService(string arguments)
    {
        var response = await ExecuteCommandAsync(arguments);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_BindsReplacementTargetsInOrder()
    {
        ConfigureServiceCall().Returns(ExpectedResult);

        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "assignment1",
            "--service-level-resource-ids", ServiceLevelResourceId, ServiceLevelResourceId + "2");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateGoalAssignmentAsync("sg1", "assignment1",
            Arg.Is<IReadOnlyList<string>>(ids => ids.SequenceEqual(new[] { ServiceLevelResourceId, ServiceLevelResourceId + "2" })),
            null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ExplicitClearPassesEmptyTargetsAndCancellation()
    {
        ConfigureServiceCall().Returns(ExpectedResult);
        using var cancellation = new CancellationTokenSource();

        var response = await Command.ExecuteAsync(Context, new GoalAssignmentUpdateOptions
        {
            ServiceGroup = "sg1",
            GoalAssignment = "assignment1",
            Tenant = Tenant,
            ClearServiceLevelResources = true
        }, cancellation.Token);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateGoalAssignmentAsync("sg1", "assignment1",
            Arg.Is<IReadOnlyList<string>>(ids => ids.Count == 0),
            Tenant, cancellation.Token);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDuplicateTargetsWithoutCallingService()
    {
        var response = await ExecuteCommandAsync([.. Arguments, ServiceLevelResourceId.ToUpperInvariant()]);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public void Constructor_DoesNotExposeImmutableZonalRequirement() =>
        Assert.DoesNotContain(CommandDefinition.Options, option => option.Name == "--require-zonal-resiliency");

    [Fact]
    public async Task ExecuteAsync_RejectsReplacementAndClearWithoutCallingService()
    {
        var response = await ExecuteCommandAsync([.. Arguments, "--clear-service-level-resources", "true"]);

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
            "--service-level-resource-ids", id);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidateOptions_RejectsExplicitEmptyReplacement(bool clear)
    {
        var validation = new ValidationResult();

        Command.ValidateOptions(new GoalAssignmentUpdateOptions
        {
            ServiceGroup = "sg1",
            GoalAssignment = "assignment1",
            ServiceLevelResourceIds = [],
            ClearServiceLevelResources = clear
        }, validation);

        Assert.NotEmpty(validation.Errors);
        Assert.Empty(Service.ReceivedCalls());
    }
}
