// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using NSubstitute;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;

public sealed class GoalAssignmentRefreshResourcesCommandTests : GoalAssignmentCommandTestsBase<GoalAssignmentRefreshResourcesCommand, GoalAssignmentOperationResult>
{
    protected override string CommandName => "refresh-resources";
    protected override bool Destructive => false;
    protected override bool Idempotent => false;
    protected override GoalAssignmentOperationResult ExpectedResult => new("sg1", "assignment1", OperationId, "Accepted", false);
    protected override Task<GoalAssignmentOperationResult> ConfigureServiceCall() =>
        Service.RefreshGoalAssignmentResourcesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
}
