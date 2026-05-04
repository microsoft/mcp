using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Node.js Console greenfield projects (no existing telemetry)
/// </summary>
public class ConsoleNodeJsGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        var consoleProjects = analysis.Projects
            .Where(p => p.AppType == AppType.ConsoleNodeJs)
            .ToList();

        return analysis.Language == Language.NodeJs
            && consoleProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.ConsoleNodeJs);
        var packageJsonPath = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "index.js";
        var projectDir = Path.GetDirectoryName(packageJsonPath) ?? "";

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "applicationinsights",
                "Node.js application using console logging. The applicationinsights package is required for automatic console log collection, as @azure/monitor-opentelemetry only supports Bunyan and Winston.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineNodeJs,
                    LearningResources.ConceptsAzureMonitorNodeJs,
                    LearningResources.ExampleConsoleNodeJsSetup
                ])
            .AddPackageAction(
                "add-monitor-package",
                "Add Application Insights package",
                "npm",
                "applicationinsights",
                "latest",
                packageJsonPath,
                "review-education")
            .AddModifyCodeAction(
                "configure-opentelemetry",
                "Initialize Application Insights at application startup",
                entryPoint,
                @"const appInsights = require('applicationinsights');

// Initialize Application Insights with console log collection
appInsights.setup(process.env.APPLICATIONINSIGHTS_CONNECTION_STRING)
  .setAutoCollectConsole(true, true)
  .start();",
                "// At the very top of the file, before any other imports",
                "applicationinsights",
                "add-monitor-package")
            .AddConfigAction(
                "add-connection-string",
                "Set Azure Monitor connection string in environment variables",
                Path.Combine(projectDir, ".env"),
                "APPLICATIONINSIGHTS_CONNECTION_STRING",
                "<your-connection-string>",
                "APPLICATIONINSIGHTS_CONNECTION_STRING");

        return builder.Build();
    }
}
