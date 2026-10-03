// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Commands;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests;

public sealed class ArchitectureAssessCommandTests
    : CommandUnitTestsBase<ArchitectureAssessCommand, IResiliencyAgentService>
{
    private readonly IArtifactWriter _artifactWriter = Substitute.For<IArtifactWriter>();
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public ArchitectureAssessCommandTests()
    {
        Services.AddSingleton(_artifactWriter);
        Services.AddSingleton(_attachmentCache);
    }

    [Fact]
    public void Constructor_ProvidesDiscriminativeMetadata()
    {
        var command = Command.GetCommand();

        Assert.Equal("assess", command.Name);
        Assert.Contains("design-time zonal resilience assessment", command.Description, StringComparison.Ordinal);
        Assert.Contains("resiliencyagent_file_attach", command.Description, StringComparison.Ordinal);
        Assert.Contains("resiliencyagent_iac_generate", command.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsAttachmentsAndConsumesThemAfterAcceptedSend()
    {
        var attachment = new AgentAttachment(
            "attachment-1",
            "architecture.png",
            "image/png",
            [1, 2, 3],
            "hash",
            DateTimeOffset.UtcNow.AddHours(1));
        _attachmentCache.Resolve(Arg.Is<IReadOnlyList<string>>(
                ids => ids.SequenceEqual(new[] { "attachment-1" })))
            .Returns([attachment]);
        Service.SendAsync(
                "conv-architecture",
                null,
                "Assess this architecture",
                Arg.Is<IReadOnlyList<AgentAttachment>>(
                    files => files.Count == 1 && files[0] == attachment),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-architecture",
                "task-1",
                "completed",
                true,
                false,
                "Architecture assessment complete.",
                []));

        var response = await ExecuteCommandAsync(
            "--request",
            "Assess this architecture",
            "--conversation-id",
            "conv-architecture",
            "--attachment-ids",
            "attachment-1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _attachmentCache.Received(1).Remove(
            Arg.Is<IReadOnlyList<string>>(ids => ids.SequenceEqual(new[] { "attachment-1" })));
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotConsumeAttachmentsWhenSendFails()
    {
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>>())
            .Returns([new AgentAttachment(
                "attachment-1",
                "architecture.png",
                "image/png",
                [1],
                "hash",
                DateTimeOffset.UtcNow.AddHours(1))]);
        Service.SendAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<AgentTurn>>(_ => throw new HttpRequestException("send failed"));

        var response = await ExecuteCommandAsync(
            "--request",
            "Assess",
            "--attachment-ids",
            "attachment-1");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.Status);
        _attachmentCache.DidNotReceive().Remove(Arg.Any<IReadOnlyList<string>>());
    }
}

public sealed class BicepReviewCommandTests
    : CommandUnitTestsBase<BicepReviewCommand, IResiliencyAgentService>
{
    private readonly IArtifactWriter _artifactWriter = Substitute.For<IArtifactWriter>();
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public BicepReviewCommandTests()
    {
        Services.AddSingleton(_artifactWriter);
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }

    [Fact]
    public async Task ExecuteAsync_WritesOnlyActualA2AArtifacts()
    {
        var artifact = new AgentArtifact(
            "corrected",
            "Corrected template",
            "text/plain",
            "resource r ...",
            "bicep");
        Service.SendAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                "review this Bicep",
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-1",
                "task-1",
                "completed",
                true,
                false,
                "Review complete.",
                [artifact]));
        _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), "conv-1")
            .Returns([new WrittenArtifact(
                "corrected.bicep",
                @"C:\ws\corrected.bicep",
                "[corrected.bicep](file:///C:/ws/corrected.bicep)",
                "bicep",
                null)]);

        var response = await ExecuteCommandAsync("--request", "review this Bicep");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _artifactWriter.Received(1).Write(Arg.Any<IEnumerable<AgentArtifact>>(), "conv-1");
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotInferArtifactsFromReplyText()
    {
        const string reply = "main.bicep\n```bicep\nparam location string\n```";
        Service.SendAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<IReadOnlyList<AgentAttachment>>(),
                Arg.Any<CancellationToken>())
            .Returns(new AgentTurn(
                "conv-1",
                "task-1",
                "completed",
                true,
                false,
                reply,
                []));

        var response = await ExecuteCommandAsync("--request", "review");

        _artifactWriter.DidNotReceive().Write(
            Arg.Any<IEnumerable<AgentArtifact>>(),
            Arg.Any<string>());
        var result = ValidateAndDeserializeResponse(
            response,
            ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);
        Assert.Equal(reply, result.Reply);
        Assert.DoesNotContain(
            "\"files\"",
            JsonSerializer.Serialize(response.Results),
            StringComparison.OrdinalIgnoreCase);
    }
}
