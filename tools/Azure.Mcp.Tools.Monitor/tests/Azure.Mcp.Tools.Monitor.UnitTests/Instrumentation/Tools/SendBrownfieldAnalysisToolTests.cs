using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Detectors;
using Azure.Mcp.Tools.Monitor.Generators;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Pipeline;
using Azure.Mcp.Tools.Monitor.Tools;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests.Tools;

public sealed class SendBrownfieldAnalysisToolTests
{
    [Fact]
    public void Submit_WithoutActiveSession_ReturnsError()
    {
        // Arrange
        var tool = new SendBrownfieldAnalysisTool([]);

        // Act
        var response = ParseJson(tool.Submit(
            $"missing-{Guid.NewGuid():N}",
            null,
            null,
            null,
            null,
            null,
            null,
            null));

        // Assert
        Assert.Equal("error", response.GetProperty("status").GetString());
        Assert.Contains("No active session", response.GetProperty("message").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Submit_WhenSessionIsNotAwaitingAnalysis_ReturnsError()
    {
        // Arrange
        var workspacePath = CreateWorkspaceDirectory();
        var orchestrator = new OrchestratorTool(CreateAnalyzer(
            state: InstrumentationState.Greenfield,
            existingInstrumentation: null,
            generators: [CreateGenerator(CreateSpecWithOneAction())]));
        var startResponse = ParseJson(orchestrator.Start(workspacePath));
        var sessionId = startResponse.GetProperty("sessionId").GetString();

        var tool = new SendBrownfieldAnalysisTool([CreateGenerator(CreateSpecWithOneAction())]);

        // Act
        var response = ParseJson(tool.Submit(
            sessionId!,
            null,
            null,
            null,
            null,
            null,
            null,
            null));

        // Assert
        Assert.Equal("error", response.GetProperty("status").GetString());
        Assert.Contains("not awaiting analysis", response.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Submit_WhenAwaitingAnalysisAndNoMatchingGenerator_ReturnsUnsupported()
    {
        // Arrange
        var workspacePath = CreateWorkspaceDirectory();
        var orchestrator = new OrchestratorTool(CreateAnalyzer(
            state: InstrumentationState.Brownfield,
            existingInstrumentation: new ExistingInstrumentation { Type = InstrumentationType.ApplicationInsightsSdk },
            generators: []));
        var startResponse = ParseJson(orchestrator.Start(workspacePath));
        var sessionId = startResponse.GetProperty("sessionId").GetString();

        var tool = new SendBrownfieldAnalysisTool([]);

        // Act
        var response = ParseJson(tool.Submit(
            sessionId!,
            new ServiceOptionsFindings { SetupPattern = "AddApplicationInsightsTelemetry" },
            null,
            null,
            null,
            null,
            null,
            null));

        // Assert
        Assert.Equal("unsupported", response.GetProperty("status").GetString());
    }

    [Fact]
    public void Submit_WhenAwaitingAnalysisAndGeneratorMatches_ReturnsInProgress()
    {
        // Arrange
        var workspacePath = CreateWorkspaceDirectory();
        var orchestrator = new OrchestratorTool(CreateAnalyzer(
            state: InstrumentationState.Brownfield,
            existingInstrumentation: new ExistingInstrumentation { Type = InstrumentationType.ApplicationInsightsSdk },
            generators: []));
        var startResponse = ParseJson(orchestrator.Start(workspacePath));
        var sessionId = startResponse.GetProperty("sessionId").GetString();

        var matchingGenerator = CreateGenerator(CreateSpecWithOneAction());
        var tool = new SendBrownfieldAnalysisTool([matchingGenerator]);

        // Act
        var response = ParseJson(tool.Submit(
            sessionId!,
            new ServiceOptionsFindings { SetupPattern = "AddApplicationInsightsTelemetry" },
            new InitializerFindings { Found = false },
            new ProcessorFindings { Found = false },
            new ClientUsageFindings { DirectUsage = false },
            new SamplingFindings { HasCustomSampling = false },
            new TelemetryPipelineFindings { Found = false },
            null));

        // Assert
        Assert.Equal("in_progress", response.GetProperty("status").GetString());
        Assert.Equal("Step 1 of 1", response.GetProperty("progress").GetString());
        Assert.True(response.TryGetProperty("currentAction", out _));
    }

    private static WorkspaceAnalyzer CreateAnalyzer(
        InstrumentationState state,
        ExistingInstrumentation? existingInstrumentation,
        IEnumerable<IGenerator> generators)
    {
        var languageDetector = Substitute.For<ILanguageDetector>();
        languageDetector.CanHandle(Arg.Any<string>()).Returns(true);
        languageDetector.Detect(Arg.Any<string>()).Returns(Language.DotNet);

        var appTypeDetector = Substitute.For<IAppTypeDetector>();
        appTypeDetector.SupportedLanguage.Returns(Language.DotNet);
        appTypeDetector.DetectProjects(Arg.Any<string>()).Returns(
        [
            new ProjectInfo
            {
                ProjectFile = "app.csproj",
                EntryPoint = "Program.cs",
                AppType = AppType.AspNetCore,
                HostingPattern = HostingPattern.MinimalApi
            }
        ]);

        var instrumentationDetector = Substitute.For<IInstrumentationDetector>();
        instrumentationDetector.SupportedLanguage.Returns(Language.DotNet);
        instrumentationDetector.Detect(Arg.Any<string>()).Returns(new InstrumentationResult(state, existingInstrumentation));

        return new WorkspaceAnalyzer(
            [languageDetector],
            [appTypeDetector],
            [instrumentationDetector],
            generators);
    }

    private static IGenerator CreateGenerator(OnboardingSpec spec)
    {
        var generator = Substitute.For<IGenerator>();
        generator.CanHandle(Arg.Any<Analysis>()).Returns(true);
        generator.Generate(Arg.Any<Analysis>()).Returns(spec);
        return generator;
    }

    private static OnboardingSpec CreateSpecWithOneAction()
    {
        return new OnboardingSpecBuilder(new Analysis
        {
            Language = Language.DotNet,
            State = InstrumentationState.Brownfield,
            ExistingInstrumentation = new ExistingInstrumentation { Type = InstrumentationType.ApplicationInsightsSdk },
            Projects =
            [
                new ProjectInfo
                {
                    ProjectFile = "app.csproj",
                    EntryPoint = "Program.cs",
                    AppType = AppType.AspNetCore,
                    HostingPattern = HostingPattern.MinimalApi
                }
            ]
        })
        .WithDecision(
            OnboardingConstants.Intents.Migrate,
            OnboardingConstants.Approaches.ApplicationInsights3x,
            "supported")
        .AddManualStepAction("migration-step", "Perform migration", "Do migration")
        .Build();
    }

    private static JsonElement ParseJson(string response)
    {
        using var document = JsonDocument.Parse(response);
        return document.RootElement.Clone();
    }

    private static string CreateWorkspaceDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"monitorinstrumentation-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
