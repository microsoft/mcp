using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Node.js Bunyan greenfield projects (no existing telemetry)
/// </summary>
public class BunyanNodeJsGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        var bunyanProjects = analysis.Projects
            .Where(p => p.AppType == AppType.BunyanNodeJs)
            .ToList();

        return analysis.Language == Language.NodeJs
            && bunyanProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.BunyanNodeJs);
        var packageJsonPath = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "index.js";
        var projectDir = Path.GetDirectoryName(packageJsonPath) ?? "";

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "azure-monitor-opentelemetry",
                "Node.js application with Bunyan logging. Azure Monitor OpenTelemetry provides automatic instrumentation for HTTP requests and can be configured to collect Bunyan logs.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineNodeJs,
                    LearningResources.ConceptsAzureMonitorNodeJs,
                    LearningResources.ExampleBunyanSetup
                ])
            .AddPackageAction(
                "add-monitor-package",
                "Add Azure Monitor OpenTelemetry package",
                "npm",
                "@azure/monitor-opentelemetry",
                "latest",
                packageJsonPath,
                "review-education")
            .AddModifyCodeAction(
                "configure-opentelemetry",
                "Initialize Azure Monitor OpenTelemetry at application startup",
                entryPoint,
                @"const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor integration with Bunyan log collection
useAzureMonitor({
  azureMonitorExporterOptions: {
    connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
  },
  instrumentationOptions: {
    bunyan: { enabled: true }
  }
});",
                "// At the very top of the file, before any other imports",
                "@azure/monitor-opentelemetry",
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
