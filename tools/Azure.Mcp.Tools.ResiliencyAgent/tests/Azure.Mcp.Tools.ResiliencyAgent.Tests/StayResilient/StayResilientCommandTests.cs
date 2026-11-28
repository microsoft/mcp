// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Drills;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.RecoveryPlans;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.StayResilient;

public abstract class StayResilientCommandTestsBase<TCommand>
    : CommandUnitTestsBase<TCommand, IResiliencyAgentService>
    where TCommand : class, IBaseCommand
{
    protected StayResilientCommandTestsBase()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        IAttachmentCache attachmentCache = Substitute.For<IAttachmentCache>();
        attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
        Services.AddSingleton(attachmentCache);
    }
}

public sealed class DrillsGetCommandTests : StayResilientCommandTestsBase<DrillsGetCommand>
{
    [Fact]
    public void Constructor_ProvidesDrillInspectionAndCheckMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.Contains("readiness blockers", command.Description, StringComparison.Ordinal);
        Assert.Contains("resync-readiness", command.Description, StringComparison.Ordinal);
        Assert.Contains("do not start or complete a drill run", command.Description, StringComparison.Ordinal);
        Assert.Contains("authoritativeUserResponse verbatim", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsDrillDiagnosis()
    {
        const string request = "Why is drill d1 on service group sg1 not ready?";
        Service.SendAsync(
                "conv-stay",
                Arg.Is<string?>(taskId => taskId == null),
                request,
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn("conv-stay", "task-1", "completed", true, false, "Diagnosis complete.", []));

        var response = await ExecuteCommandAsync("--request", request, "--conversation-id", "conv-stay");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}

public sealed class DrillsManageCommandTests : StayResilientCommandTestsBase<DrillsManageCommand>
{
    [Fact]
    public void Constructor_ProvidesConfirmedDrillManagementMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("manage", command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.Contains("A fully specified request remains eligible", command.Description, StringComparison.Ordinal);
        Assert.Contains("Standard Usage Plan", command.Description, StringComparison.Ordinal);
        Assert.Contains("does not execute or delete", command.Description, StringComparison.Ordinal);
        Assert.Contains("Never approve a change", command.Description, StringComparison.Ordinal);
    }
}

public sealed class RecoveryPlansGetCommandTests : StayResilientCommandTestsBase<RecoveryPlansGetCommand>
{
    [Fact]
    public void Constructor_ProvidesPlanInspectionAndValidationMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.Contains("recovery order", command.Description, StringComparison.Ordinal);
        Assert.Contains("cross-plan comparison", command.Description, StringComparison.Ordinal);
        Assert.Contains("Azure Portal", command.Description, StringComparison.Ordinal);
        Assert.Contains("authoritativeUserResponse verbatim", command.Description, StringComparison.Ordinal);
    }
}

public sealed class RecoveryPlansManageCommandTests : StayResilientCommandTestsBase<RecoveryPlansManageCommand>
{
    [Fact]
    public void Constructor_ProvidesConfirmedPlanManagementMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("manage", command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.Contains("reorder", command.Description, StringComparison.Ordinal);
        Assert.Contains("recovery", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A fully specified request remains eligible", command.Description, StringComparison.Ordinal);
        Assert.Contains("does not execute recovery operations or delete", command.Description, StringComparison.Ordinal);
        Assert.Contains("Never approve a change", command.Description, StringComparison.Ordinal);
    }
}
