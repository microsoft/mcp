// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.DataResiliency;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.DataResiliency;

public sealed class DataResiliencyGetCommandTests
    : CommandUnitTestsBase<DataResiliencyGetCommand, IResiliencyAgentService>
{
    public DataResiliencyGetCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        IAttachmentCache attachmentCache = Substitute.For<IAttachmentCache>();
        attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
        Services.AddSingleton(attachmentCache);
    }

    [Fact]
    public void Constructor_ProvidesAssessmentAndGuidanceMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.Contains("Evaluate Azure VM Backup eligibility", command.Description, StringComparison.Ordinal);
        Assert.Contains("Service Groups", command.Description, StringComparison.Ordinal);
        Assert.Contains("failed-job diagnosis", command.Description, StringComparison.Ordinal);
        Assert.Contains("restore", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("soft-deleted item", command.Description, StringComparison.Ordinal);
        Assert.Contains("does not change Azure", command.Description, StringComparison.Ordinal);
        Assert.Contains("authoritativeUserResponse verbatim", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsBroadAssessmentRequest()
    {
        const string request = "Assess Backup coverage for Service Group checkout-prod";
        Service.SendAsync(
                "conv-data",
                Arg.Is<string?>(taskId => taskId == null),
                request,
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-data",
                "task-data",
                "completed",
                true,
                false,
                "Coverage assessment complete.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            request,
            "--conversation-id",
            "conv-data");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}

public sealed class DataResiliencyManageCommandTests
    : CommandUnitTestsBase<DataResiliencyManageCommand, IResiliencyAgentService>
{
    public DataResiliencyManageCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        IAttachmentCache attachmentCache = Substitute.For<IAttachmentCache>();
        attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
        Services.AddSingleton(attachmentCache);
    }

    [Fact]
    public void Constructor_ProvidesGuidedManagementMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("manage", command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.Contains("eligibility assessment", command.Description, StringComparison.Ordinal);
        Assert.Contains("fully specified request remains eligible", command.Description, StringComparison.Ordinal);
        Assert.Contains("dependent stages", command.Description, StringComparison.Ordinal);
        Assert.Contains("never approve a write", command.Description, StringComparison.Ordinal);
        Assert.Contains("Azure VM Backup", command.Description, StringComparison.Ordinal);
        Assert.Contains("action requests belong", command.Description, StringComparison.Ordinal);
        Assert.Contains("authoritativeUserResponse verbatim", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsFullySpecifiedGuidedRequest()
    {
        const string request =
            "Enable Backup for VM vm1 using vault rsv1, policy daily1, and all disks";
        Service.SendAsync(
                "conv-data",
                Arg.Is<string?>(taskId => taskId == null),
                request,
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-data",
                "task-data",
                "input-required",
                false,
                true,
                "Confirm the displayed Backup changes?",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            request,
            "--conversation-id",
            "conv-data");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
    }
}

public sealed class DataResiliencyGenerateCommandTests
    : CommandUnitTestsBase<DataResiliencyGenerateCommand, IResiliencyAgentService>
{
    public DataResiliencyGenerateCommandTests()
    {
        Services.AddSingleton(Substitute.For<IArtifactWriter>());
        IAttachmentCache attachmentCache = Substitute.For<IAttachmentCache>();
        attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
        Services.AddSingleton(attachmentCache);
    }

    [Fact]
    public void Constructor_ProvidesAutomationOnlyMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("generate", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.Contains("Azure CLI", command.Description, StringComparison.Ordinal);
        Assert.Contains("PowerShell", command.Description, StringComparison.Ordinal);
        Assert.Contains("REST", command.Description, StringComparison.Ordinal);
        Assert.Contains("ARM JSON", command.Description, StringComparison.Ordinal);
        Assert.Contains("Bicep", command.Description, StringComparison.Ordinal);
        Assert.Contains("Terraform", command.Description, StringComparison.Ordinal);
        Assert.Contains("failed-job", command.Description, StringComparison.Ordinal);
        Assert.Contains("vault-deletion", command.Description, StringComparison.Ordinal);
        Assert.Contains("Do not independently rewrite", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsExplicitAutomationRequest()
    {
        const string request = "Generate Terraform to configure Azure VM Backup";
        Service.SendAsync(
                "conv-data",
                Arg.Is<string?>(taskId => taskId == null),
                request,
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-data",
                "task-data",
                "completed",
                true,
                false,
                "Terraform package generated.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            request,
            "--conversation-id",
            "conv-data");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }
}
