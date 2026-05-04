using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for NestJS greenfield projects (no existing telemetry)
/// </summary>
public class NestJsGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        // Single NestJS project, greenfield
        var nestjsProjects = analysis.Projects
            .Where(p => p.AppType == AppType.NestJs)
            .ToList();

        return analysis.Language == Language.NodeJs
            && nestjsProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.NestJs);
        var packageJsonPath = project.ProjectFile;
        var projectDir = Path.GetDirectoryName(packageJsonPath) ?? "";

        // NestJS typically uses src/main.ts as entry point
        var mainFile = DetectMainFile(projectDir);
        var tracingFile = Path.Combine(projectDir, "src", "tracing.ts");

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "azure-monitor-opentelemetry",
                "NestJS greenfield application. Azure Monitor OpenTelemetry provides automatic instrumentation for HTTP requests, dependencies, and custom telemetry. A separate tracing file is recommended for proper initialization.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineNodeJs,
                    LearningResources.ConceptsAzureMonitorNodeJs,
                    LearningResources.ExampleNestJsSetup
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
                "create-tracing-file",
                "Create tracing.ts file for OpenTelemetry initialization",
                tracingFile,
                @"import { useAzureMonitor } from '@azure/monitor-opentelemetry';

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});",
                "// Create new file at src/tracing.ts",
                "@azure/monitor-opentelemetry",
                "add-monitor-package")
            .AddModifyCodeAction(
                "import-tracing",
                "Import tracing at the top of main.ts (must be first import)",
                mainFile,
                @"import './tracing';",
                "// Add as the very first import in main.ts",
                "@azure/monitor-opentelemetry",
                "create-tracing-file")
            .AddConfigAction(
                "add-connection-string",
                "Set Azure Monitor connection string in environment variables",
                Path.Combine(projectDir, ".env"),
                "APPLICATIONINSIGHTS_CONNECTION_STRING",
                "<your-connection-string>",
                "APPLICATIONINSIGHTS_CONNECTION_STRING");

        return builder.Build();
    }

    private string DetectMainFile(string projectDir)
    {
        // Check common NestJS entry points
        var possiblePaths = new[]
        {
            Path.Combine(projectDir, "src", "main.ts"),
            Path.Combine(projectDir, "src", "main.js"),
            Path.Combine(projectDir, "main.ts"),
            Path.Combine(projectDir, "main.js")
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Default to src/main.ts for NestJS
        return Path.Combine(projectDir, "src", "main.ts");
    }
}
