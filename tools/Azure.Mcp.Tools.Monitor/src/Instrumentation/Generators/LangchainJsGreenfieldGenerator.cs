using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for LangChain.js greenfield projects (no existing telemetry)
/// </summary>
public class LangchainJsGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        // Single LangChain.js project, greenfield
        var langchainProjects = analysis.Projects
            .Where(p => p.AppType == AppType.LangchainJs)
            .ToList();

        return analysis.Language == Language.NodeJs
            && langchainProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.LangchainJs);
        var packageJsonPath = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "index.js";
        var projectDir = Path.GetDirectoryName(packageJsonPath) ?? "";

        // Determine if the project uses ES modules or CommonJS
        var isEsModule = IsEsModuleProject(packageJsonPath);
        var tracingFileName = isEsModule ? "tracing.mjs" : "tracing.js";
        var tracingFile = Path.Combine(projectDir, tracingFileName);

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "azure-monitor-opentelemetry",
                "LangChain.js greenfield application. Azure Monitor OpenTelemetry provides automatic instrumentation for HTTP requests, LLM calls, and custom telemetry. A separate tracing file ensures proper initialization before LangChain imports.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineNodeJs,
                    LearningResources.ConceptsAzureMonitorNodeJs,
                    LearningResources.ConceptsGenAiObservability,
                    LearningResources.ExampleLangchainJsSetup
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
                $"Create {tracingFileName} file for OpenTelemetry initialization",
                tracingFile,
                GenerateTracingFileContent(isEsModule),
                $"// Create new file at project root: {tracingFileName}",
                "@azure/monitor-opentelemetry",
                "add-monitor-package")
            .AddModifyCodeAction(
                "import-tracing",
                "Import tracing at the top of the entry point (must be first import)",
                entryPoint,
                GenerateTracingImport(isEsModule, tracingFileName),
                "// Add as the very first import in the entry file",
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

    private bool IsEsModuleProject(string packageJsonPath)
    {
        try
        {
            var content = File.ReadAllText(packageJsonPath);
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("type", out var typeElement))
            {
                return typeElement.GetString() == "module";
            }
        }
        catch
        {
            // Default to CommonJS if we can't parse
        }
        return false;
    }

    private string GenerateTracingFileContent(bool isEsModule)
    {
        if (isEsModule)
        {
            return @"import { useAzureMonitor } from '@azure/monitor-opentelemetry';

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});";
        }
        else
        {
            return @"const { useAzureMonitor } = require('@azure/monitor-opentelemetry');

// Enable Azure Monitor integration
// This must be called before any other imports to ensure proper instrumentation
useAzureMonitor({
    azureMonitorExporterOptions: {
        connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
    }
});";
        }
    }

    private string GenerateTracingImport(bool isEsModule, string tracingFileName)
    {
        if (isEsModule)
        {
            return $"import './{tracingFileName}';";
        }
        else
        {
            return $"require('./{tracingFileName}');";
        }
    }
}
