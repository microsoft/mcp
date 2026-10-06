// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Commands;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests.Client;
using ModelContextProtocol.Protocol;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Iac;

public sealed class IacGenerateCommandTests
    : CommandUnitTestsBase<IacGenerateCommand, IResiliencyAgentService>
{
    private readonly IArtifactWriter _artifactWriter = Substitute.For<IArtifactWriter>();
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();

    public IacGenerateCommandTests()
    {
        Services.AddSingleton(_artifactWriter);
        Services.AddSingleton(_attachmentCache);
        _attachmentCache.Resolve(Arg.Any<IReadOnlyList<string>?>()).Returns([]);
    }
    private static AgentTurn Completed(string reply = "All resources are zone redundant.", params AgentArtifact[] artifacts) =>
        new("conv-1", "task-1", "completed", true, false, reply, artifacts);

    private static AgentTurn CompletedWithNextSteps(params string[] steps) =>
        new("conv-1", "task-1", "completed", true, false, "Done.", [], null, steps);

    private static AgentTurn Working(string? reasoningStep = null) =>
        new("conv-1", "task-1", "working", false, false, null, [], reasoningStep);

    private static AgentTurn AwaitingUser(string question = "Which subscription?") =>
        new("conv-1", "task-1", "input-required", false, true, question, []);

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("generate", command.Name);
        Assert.Contains("create or generate new zone-resilient", command.Description, StringComparison.Ordinal);
        Assert.Contains("does not review or correct", command.Description, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--request \"is my app zone redundant\"", true)]
    [InlineData("--request \"is my app zone redundant\" --conversation-id conv-1", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
                .Returns(Completed());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var artifact = new AgentArtifact("main.bicep", "template", "text/plain", "param a string", "bicep");
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed("Done.", artifact));
        _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>())
            .Returns([new WrittenArtifact(
                "main.bicep",
                @"C:\ws\azure-resiliency\main.bicep",
                "[main.bicep](file:///C:/ws/azure-resiliency/main.bicep)",
                "bicep",
                "template")]);

        var response = await ExecuteCommandAsync("--request", "generate a template");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);

        Assert.Equal("conv-1", result.ConversationId);
        Assert.Equal("completed", result.State);
        Assert.Equal("Done.", result.Reply);
        Assert.DoesNotContain(
            "\"files\"",
            JsonSerializer.Serialize(response.Results),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WritesArtifactsToDiskRatherThanReturningContent()
    {
        var artifact = new AgentArtifact("registry.bicep", null, "text/plain", "resource r ...", "bicep");
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed("Generated.", artifact));
        _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>())
            .Returns([new WrittenArtifact(
                "registry.bicep",
                @"C:\ws\azure-resiliency\registry.bicep",
                "[registry.bicep](file:///C:/ws/azure-resiliency/registry.bicep)",
                "bicep",
                null)]);

        var response = await ExecuteCommandAsync("--request", "generate a template");

        _artifactWriter.Received(1).Write(Arg.Any<IEnumerable<AgentArtifact>>(), "conv-1");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);
        Assert.Equal("Generated.", result.Reply);
        Assert.DoesNotContain(
            "\"files\"",
            JsonSerializer.Serialize(response.Results),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotWriteWhenThereAreNoArtifacts()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _artifactWriter.DidNotReceive().Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>());
        Assert.Null(response.McpContent);
    }

    [Fact]
    public async Task ExecuteAsync_PromotesDetailedReportAndAddsOnlyEmbeddedResources()
    {
        string directory = Path.Combine(Path.GetTempPath(), "resiliency-mcp-blocks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string reportPath = Path.Combine(directory, "DetailedRecommendationReport-2.md");
        string templatePath = Path.Combine(directory, "main.bicep");
        const string reportContent = "# Detailed recommendations\n\nResource is resilient.";
        const string templateContent = "resource plan 'Microsoft.Web/serverfarms@2024-11-01' = {}";
        File.WriteAllText(reportPath, reportContent);
        File.WriteAllText(templatePath, templateContent);

        try
        {
            var reportArtifact = new AgentArtifact(
                "DetailedRecommendationReport.md",
                "Zonal resiliency recommendation report",
                "text/markdown",
                reportContent,
                "md");
            var templateArtifact = new AgentArtifact(
                "main.bicep",
                "Corrected Bicep template",
                "text/plain",
                templateContent,
                "bicep");
            Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
                .Returns(Completed("Done.", templateArtifact, reportArtifact));
            _artifactWriter.Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>())
                .Returns(
                [
                    new WrittenArtifact(
                        "main.bicep",
                        templatePath,
                        $"[main.bicep]({new Uri(templatePath).AbsoluteUri})",
                        "bicep",
                        "Corrected Bicep template"),
                    new WrittenArtifact(
                        "DetailedRecommendationReport-2.md",
                        reportPath,
                        $"[DetailedRecommendationReport-2.md]({new Uri(reportPath).AbsoluteUri})",
                        "md",
                        "Zonal resiliency recommendation report"),
                ]);

            var response = await ExecuteCommandAsync("--request", "assess posture");

            Assert.NotNull(response.McpContent);
            Assert.Collection(
                response.McpContent,
                block =>
                {
                    var presentation = Assert.IsType<TextContentBlock>(block);
                    Assert.Contains("DetailedRecommendationReport-2.md", presentation.Text, StringComparison.Ordinal);
                    Assert.Contains("full detailed recommendation report", presentation.Text, StringComparison.Ordinal);
                    Assert.Contains("chat summary is abbreviated", presentation.Text, StringComparison.OrdinalIgnoreCase);
                    Assert.Contains("main.bicep", presentation.Text, StringComparison.Ordinal);
                },
                block =>
                {
                    var embedded = Assert.IsType<EmbeddedResourceBlock>(block);
                    var resource = Assert.IsType<TextResourceContents>(embedded.Resource);
                    Assert.Equal(new Uri(reportPath).AbsoluteUri, resource.Uri);
                    Assert.Equal("text/markdown", resource.MimeType);
                    Assert.Equal(reportContent, resource.Text);
                },
                block =>
                {
                    var embedded = Assert.IsType<EmbeddedResourceBlock>(block);
                    var resource = Assert.IsType<TextResourceContents>(embedded.Resource);
                    Assert.Equal(new Uri(templatePath).AbsoluteUri, resource.Uri);
                    Assert.Equal("text/plain", resource.MimeType);
                    Assert.Equal(templateContent, resource.Text);
                });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotParseBicepFencesFromGuidanceReplies()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed(
                "**main.bicep**\n```bicep\nresource example 'Type@version' = {}\n```"));

        var response = await ExecuteCommandAsync("--request", "show an example");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        _artifactWriter.DidNotReceive().Write(Arg.Any<IEnumerable<AgentArtifact>>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_ReusesSuppliedConversationId()
    {
        Service.SendAsync("conv-existing", Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "follow up", "--conversation-id", "conv-existing");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SendAsync(
            "conv-existing", null, "follow up", Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PollsUntilTerminal()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Working());
        Service.PollAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Working(), Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(2).PollAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SurvivesWhenNoProgressTokenIsAvailable()
    {
        // The specification forbids reporting progress without a token from the client, so the
        // conversation must still complete when there is nothing to report to.
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Working("Analyzing Request"));
        Service.PollAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenClientCannotElicit_ReportsAnError()
    {
        // The agent needs the user, but a unit-test context has no MCP server to ask through.
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(AwaitingUser());

        var response = await ExecuteCommandAsync("--request", "make my app resilient");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("elicitation", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTheAgentsOwnSuggestedNextSteps()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(CompletedWithNextSteps("Generate Bicep templates", "Add more resource types"));

        var response = await ExecuteCommandAsync("--request", "assess my app");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);

        Assert.Equal(["Generate Bicep templates", "Add more resource types"], result.SuggestedNextSteps);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNoNextStepsWhenTheAgentOfferedNone()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .Returns(Completed());

        var response = await ExecuteCommandAsync("--request", "assess my app");

        var result = ValidateAndDeserializeResponse(
            response, ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);

        Assert.Empty(result.SuggestedNextSteps);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.SendAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<IReadOnlyList<AgentAttachment>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--request", "assess my app");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
