// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Guidance;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Guidance;

public sealed class GuidanceGetCommandTests
    : CommandUnitTestsBase<GuidanceGetCommand, IResiliencyAgentService>
{
    private readonly IArtifactWriter _artifactWriter = Substitute.For<IArtifactWriter>();
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public GuidanceGetCommandTests()
    {
        Services.AddSingleton(_artifactWriter);
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }

    [Fact]
    public void Constructor_ProvidesDiscriminativeMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("get", command.Name);
        Assert.Contains("general, conceptual questions", command.Description, StringComparison.Ordinal);
        Assert.Contains("specific application", command.Description, StringComparison.Ordinal);
        Assert.Contains("resiliencyagent_architecture_assess", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsConceptualQuestionWithoutAttachments()
    {
        Service.SendAsync(
                "conv-guidance",
                null,
                "What does zone resilience mean?",
                Arg.Is<IReadOnlyList<AgentAttachment>>(attachments => attachments.Count == 0),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-guidance",
                "task-guidance",
                "completed",
                true,
                false,
                "Zone resilience helps a workload tolerate the loss of an availability zone.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            "What does zone resilience mean?",
            "--conversation-id",
            "conv-guidance");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _attachmentCache.DidNotReceive().Remove(Arg.Any<IReadOnlyList<string>>());
    }
}
