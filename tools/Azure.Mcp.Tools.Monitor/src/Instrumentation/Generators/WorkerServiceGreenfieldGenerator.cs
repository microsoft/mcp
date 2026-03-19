using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for .NET Worker Service greenfield projects (no existing telemetry).
/// Supports both Host.CreateDefaultBuilder and Host.CreateApplicationBuilder hosting patterns.
/// </summary>
public class WorkerServiceGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        // Single Worker Service project, greenfield
        var workerProjects = analysis.Projects
            .Where(p => p.AppType == AppType.Worker)
            .ToList();

        return analysis.Language == Language.DotNet
            && workerProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.Worker);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "Program.cs";
        var projectDir = Path.GetDirectoryName(projectFile) ?? "";

        // Select appropriate code marker based on detected hosting pattern
        var codeMarker = GetCodeMarkerForHostingPattern(project.HostingPattern);

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                Approaches.ApplicationInsights3x,
                "Worker Service greenfield application. AddApplicationInsightsTelemetryWorkerService() provides automatic instrumentation for dependencies, performance counters, and custom telemetry.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineDotNet,
                    LearningResources.ApiAddOpenTelemetry,
                    LearningResources.ExampleWorkerServiceSetup
                ])
            .AddPackageAction(
                "add-worker-service-package",
                "Add Application Insights Worker Service package",
                Packages.PackageManagerNuGet,
                Packages.WorkerService,
                Packages.WorkerServiceVersion,
                projectFile,
                "review-education")
            .AddModifyCodeAction(
                "configure-telemetry",
                "Add Application Insights telemetry to service configuration",
                entryPoint,
                CodePatterns.AddWorkerServiceSnippet,
                codeMarker,
                CodePatterns.WorkerServiceNamespace,
                "add-worker-service-package")
            .AddConfigAction(
                "add-connection-string",
                "Configure Application Insights connection string",
                Path.Combine(projectDir, Config.AppSettingsFileName),
                Config.AppInsightsConnectionStringPath,
                Config.ConnectionStringPlaceholder,
                Config.ConnectionStringEnvVar);

        return builder.Build();
    }

    /// <summary>
    /// Returns the appropriate code insertion marker based on the detected hosting pattern.
    /// </summary>
    private static string GetCodeMarkerForHostingPattern(HostingPattern pattern)
    {
        return pattern switch
        {
            HostingPattern.GenericHost => CodePatterns.HostCreateDefaultBuilderMarker,
            // For unknown patterns, default to GenericHost as Worker Services typically use that
            _ => CodePatterns.HostCreateDefaultBuilderMarker
        };
    }
}
