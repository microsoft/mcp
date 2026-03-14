using System.Text.Json;
using Azure.Mcp.Tools.MonitorInstrumentation.Detectors;
using Azure.Mcp.Tools.MonitorInstrumentation.Generators;
using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using Azure.Mcp.Tools.MonitorInstrumentation.Pipeline;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.MonitorInstrumentation.UnitTests.Tools;

public sealed class OrchestratorToolTests
{
    [Fact]
    public void StartAndNext_WithSupportedFlow_ProgressesToComplete()
    {
        // Arrange
        var workspacePath = CreateWorkspaceDirectory();
        try
        {
            var analyzer = CreateAnalyzer(
                state: InstrumentationState.Greenfield,
                existingInstrumentation: null,
                generators: [CreateGenerator(CreateSpecWithTwoActions())]);

            var tool = new OrchestratorTool(analyzer);

            // Act
            var startResponse = ParseJson(tool.Start(workspacePath));
            var sessionId = startResponse.GetProperty("sessionId").GetString();

            var nextResponse = ParseJson(tool.Next(sessionId!, "Completed step 1"));
            var finalResponse = ParseJson(tool.Next(sessionId!, "Completed step 2"));

            // Assert
            Assert.Equal("in_progress", startResponse.GetProperty("status").GetString());
            Assert.Equal("Step 1 of 2", startResponse.GetProperty("progress").GetString());

            Assert.Equal("in_progress", nextResponse.GetProperty("status").GetString());
            Assert.Equal("Step 2 of 2", nextResponse.GetProperty("progress").GetString());
            Assert.Equal(1, nextResponse.GetProperty("completedActions").GetArrayLength());

            Assert.Equal("complete", finalResponse.GetProperty("status").GetString());
            Assert.Equal(2, finalResponse.GetProperty("completedActions").GetArrayLength());
        }
        finally
        {
            DeleteDirectoryIfExists(workspacePath);
        }
    }

    [Fact]
    public void Next_WithoutActiveSession_ReturnsError()
    {
        // Arrange
        var analyzer = CreateAnalyzer(
            state: InstrumentationState.Greenfield,
            existingInstrumentation: null,
            generators: [CreateGenerator(CreateSpecWithTwoActions())]);
        var tool = new OrchestratorTool(analyzer);

        // Act
        var response = ParseJson(tool.Next($"missing-{Guid.NewGuid():N}", "done"));

        // Assert
        Assert.Equal("error", response.GetProperty("status").GetString());
        Assert.Contains("No active session", response.GetProperty("message").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Start_WithBrownfieldUnsupportedAiSdk_ReturnsAnalysisNeeded()
    {
        // Arrange
        var workspacePath = CreateWorkspaceDirectory();
        try
        {
            var analyzer = CreateAnalyzer(
                state: InstrumentationState.Brownfield,
                existingInstrumentation: new ExistingInstrumentation { Type = InstrumentationType.ApplicationInsightsSdk },
                generators: []);

            var tool = new OrchestratorTool(analyzer);

            // Act
            var response = ParseJson(tool.Start(workspacePath));

            // Assert
            Assert.Equal("analysis_needed", response.GetProperty("status").GetString());
            Assert.Equal(workspacePath, response.GetProperty("sessionId").GetString());
            Assert.True(response.TryGetProperty("analysisTemplate", out var analysisTemplate));
            Assert.True(analysisTemplate.TryGetProperty("serviceOptions", out _));
        }
        finally
        {
            DeleteDirectoryIfExists(workspacePath);
        }
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

    private static OnboardingSpec CreateSpecWithTwoActions()
    {
        return new OnboardingSpecBuilder(new Analysis
        {
            Language = Language.DotNet,
            State = InstrumentationState.Greenfield,
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
            OnboardingConstants.Intents.Onboard,
            OnboardingConstants.Approaches.AzureMonitorDistro,
            "supported")
        .AddManualStepAction("step-1", "First step", "Do first")
        .AddManualStepAction("step-2", "Second step", "Do second", null, "step-1")
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

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
