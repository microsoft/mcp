// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager.ResilienceManagement.Models;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentUpdateResourcesCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentUpdateResourcesCommand, GoalAssignmentOperationResult>
{
    protected override string CommandName => "update-resources";
    protected override string[] AdditionalArguments => ["--resources", Payload];
    protected override GoalAssignmentOperationResult ExpectedResult => new("sg1", "assignment1", OperationId, "Accepted", false);
    protected override Task<GoalAssignmentOperationResult> ConfigureServiceCall() =>
        Service.UpdateGoalAssignmentResourcesAsync(Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<UpdateGoalResourceContent>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

    protected override void AssertPayloadArguments(object?[] arguments)
    {
        var content = Assert.IsType<UpdateGoalResourceContent>(arguments[2]);
        using var document = JsonDocument.Parse(ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W")).ToString());
        var properties = document.RootElement.GetProperty("resources")[0].GetProperty("properties");
        Assert.Equal(ResourceId, properties.GetProperty("resourceArmId").GetString());
        Assert.Equal("Excluded", properties.GetProperty("zonalResiliency").GetProperty("goalParticipation").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMissingResourcesWithoutCallingService()
    {
        var response = await ExecuteCommandAsync("--service-group sg1 --goal-assignment assignment1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsOversizedPayloadWithoutCallingService()
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "assignment1",
            "--resources", Payload + new string(' ', 1_048_576));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [MemberData(nameof(GoalAssignmentValidationTests.InvalidPayloads), MemberType = typeof(GoalAssignmentValidationTests))]
    public async Task ExecuteAsync_RejectsInvalidPayloadWithoutCallingService(string payload)
    {
        var response = await ExecuteCommandAsync("--service-group", "sg1", "--goal-assignment", "assignment1", "--resources", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }
}
