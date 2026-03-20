using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Next.js greenfield projects (no existing telemetry)
/// </summary>
public class NextJsGreenfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        // Single Next.js project, greenfield
        var nextjsProjects = analysis.Projects
            .Where(p => p.AppType == AppType.NextJs)
            .ToList();

        return analysis.Language == Language.NodeJs
            && nextjsProjects.Count == 1
            && analysis.State == InstrumentationState.Greenfield;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var project = analysis.Projects.First(p => p.AppType == AppType.NextJs);
        var packageJsonPath = project.ProjectFile;
        var projectDir = Path.GetDirectoryName(packageJsonPath) ?? "";

        // Next.js uses instrumentation.js/ts for OpenTelemetry setup
        var instrumentationFile = Path.Combine(projectDir, "instrumentation.js");

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction)
            .WithDecision(
                Intents.Onboard,
                "azure-monitor-opentelemetry",
                "Next.js greenfield application. Azure Monitor OpenTelemetry provides automatic instrumentation for HTTP requests, dependencies, and custom telemetry via Next.js instrumentation hook.")
            .AddReviewEducationAction(
                "review-education",
                "Review educational materials before implementation",
                [
                    LearningResources.ConceptsOpenTelemetryPipelineNodeJs,
                    LearningResources.ConceptsAzureMonitorNodeJs,
                    LearningResources.ExampleNextJsSetup
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
                "create-instrumentation-file",
                "Create instrumentation.js file for Next.js OpenTelemetry setup",
                instrumentationFile,
                @"import { useAzureMonitor } from '@azure/monitor-opentelemetry';

export function register() {
    // Only initialize on server-side
    if (process.env.NEXT_RUNTIME === 'nodejs') {
        useAzureMonitor({
            azureMonitorExporterOptions: {
                connectionString: process.env.APPLICATIONINSIGHTS_CONNECTION_STRING
            }
        });
    }
}",
                "// Create new file at project root",
                "@azure/monitor-opentelemetry",
                "add-monitor-package")
            .AddModifyCodeAction(
                "enable-instrumentation",
                "Enable instrumentation hook and externalize server-only packages in next.config.js",
                Path.Combine(projectDir, "next.config.js"),
                @"experimental: {
    instrumentationHook: true,
    serverComponentsExternalPackages: [
        '@azure/monitor-opentelemetry',
        '@opentelemetry/sdk-node',
        '@opentelemetry/api',
        '@opentelemetry/instrumentation',
        '@opentelemetry/exporter-logs-otlp-grpc',
        '@opentelemetry/otlp-grpc-exporter-base',
        '@grpc/grpc-js',
        '@grpc/proto-loader',
    ],
},
webpack: (config, { isServer }) => {
    if (isServer) {
        config.externals = config.externals || [];
        config.externals.push({
            '@azure/monitor-opentelemetry': 'commonjs @azure/monitor-opentelemetry',
            '@opentelemetry/sdk-node': 'commonjs @opentelemetry/sdk-node',
            '@opentelemetry/instrumentation': 'commonjs @opentelemetry/instrumentation',
            '@opentelemetry/api': 'commonjs @opentelemetry/api',
            '@grpc/grpc-js': 'commonjs @grpc/grpc-js',
        });
    }
    return config;
},",
                "// Add to nextConfig object",
                "next",
                "create-instrumentation-file")
            .AddConfigAction(
                "add-connection-string",
                "Set Azure Monitor connection string in environment variables",
                Path.Combine(projectDir, ".env.local"),
                "APPLICATIONINSIGHTS_CONNECTION_STRING",
                "<your-connection-string>",
                "APPLICATIONINSIGHTS_CONNECTION_STRING");

        return builder.Build();
    }
}
