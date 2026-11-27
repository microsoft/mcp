// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentDeleteCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentDeleteCommand, bool>
{
    protected override string CommandName => "delete";
    protected override bool ExpectedResult => true;
    protected override Task<bool> ConfigureServiceCall() =>
        Service.DeleteGoalAssignmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());

    protected override void AssertSerializedResult(JsonElement result)
    {
        Assert.Equal("sg1", result.GetProperty("serviceGroup").GetString());
        Assert.Equal("assignment1", result.GetProperty("goalAssignment").GetString());
        Assert.True(result.GetProperty("deleted").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_AbsentAssignmentExplicitlySerializesDeletedFalse()
    {
        ConfigureServiceCall().Returns(false);

        var response = await ExecuteCommandAsync(Arguments);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.Results));
        Assert.False(document.RootElement.GetProperty("deleted").GetBoolean());
    }
}
