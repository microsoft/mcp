namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

/// <summary>
/// Constants for Azure Monitor onboarding specifications
/// </summary>
public static class OnboardingConstants
{
    public const string SpecVersion = "0.1";

    // Agent Instructions
    public const string AgentPreExecuteInstruction =
        "Fetch and review all resources using get_learning_resource before presenting recommendations to the user";

    // Learning Resource Paths
    public static class LearningResources
    {
        // Shared / Cross-language
        public const string ConceptsGenAiObservability = "learn://concepts/shared/gen-ai-observability.md";

        // .NET
        public const string ConceptsOpenTelemetryPipelineDotNet = "learn://concepts/dotnet/opentelemetry-pipeline.md";
        public const string ConceptsAzureMonitorDistro = "learn://concepts/dotnet/azure-monitor-distro.md";
        public const string ApiUseAzureMonitor = "learn://api-reference/dotnet/UseAzureMonitor.md";
        public const string ApiAddOpenTelemetry = "learn://api-reference/dotnet/AddOpenTelemetry.md";
        public const string ApiOpenTelemetrySdkCreate = "learn://api-reference/dotnet/OpenTelemetrySdkCreate.md";
        public const string ApiSdkCreateTracerProviderBuilder = "learn://api-reference/dotnet/SdkCreateTracerProviderBuilder.md";
        public const string ApiConfigureOpenTelemetryProvider = "learn://api-reference/dotnet/ConfigureOpenTelemetryProvider.md";
        public const string ApiWithTracing = "learn://api-reference/dotnet/WithTracing.md";
        public const string ApiWithMetrics = "learn://api-reference/dotnet/WithMetrics.md";
        public const string ApiWithLogging = "learn://api-reference/dotnet/WithLogging.md";
        public const string ApiConfigureResource = "learn://api-reference/dotnet/ConfigureResource.md";
        public const string ApiSampling = "learn://api-reference/dotnet/Sampling.md";
        public const string ApiActivityProcessors = "learn://api-reference/dotnet/ActivityProcessors.md";
        public const string ApiLogProcessors = "learn://api-reference/dotnet/LogProcessors.md";
        public const string ApiAzureMonitorExporter = "learn://api-reference/dotnet/AzureMonitorExporter.md";
        public const string ApiUseAzureMonitorExporter = "learn://api-reference/dotnet/UseAzureMonitorExporter.md";
        public const string ApiApplicationInsightsWeb = "learn://api-reference/dotnet/ApplicationInsightsWeb.md";
        public const string ApiAddApplicationInsightsTelemetry = "learn://api-reference/dotnet/AddApplicationInsightsTelemetry.md";
        public const string ApiAddApplicationInsightsTelemetryWorkerService = "learn://api-reference/dotnet/AddApplicationInsightsTelemetryWorkerService.md";
        public const string ConceptsAspNetClassicAppInsights = "learn://concepts/dotnet/aspnet-classic-appinsights.md";
        public const string MigrationAppInsights2xTo3xCode = "learn://migration/dotnet/appinsights-2x-to-3x-code-migration.md";
        public const string MigrationAppInsights2xTo3xNoCodeChange = "learn://migration/dotnet/appinsights-2x-to-3x-no-code-change.md";
        public const string MigrationWorkerService2xTo3xCode = "learn://migration/dotnet/workerservice-2x-to-3x-code-migration.md";
        public const string MigrationWorkerService2xTo3xNoCodeChange = "learn://migration/dotnet/workerservice-2x-to-3x-no-code-change.md";
        public const string ExampleAspNetCoreSetup = "learn://examples/dotnet/aspnetcore-setup.md";
        public const string ExampleAspNetClassicSetup = "learn://examples/dotnet/aspnet-classic-setup.md";
        public const string ExampleWorkerServiceSetup = "learn://examples/dotnet/workerservice-setup.md";

        // Node.js
        public const string ConceptsOpenTelemetryPipelineNodeJs = "learn://concepts/nodejs/opentelemetry-pipeline.md";
        public const string ConceptsAzureMonitorNodeJs = "learn://concepts/nodejs/azure-monitor-overview.md";
        public const string ExampleExpressSetup = "learn://examples/nodejs/express-setup.md";
        public const string ExampleFastifySetup = "learn://examples/nodejs/fastify-setup.md";
        public const string ExampleNestJsSetup = "learn://examples/nodejs/nestjs-setup.md";
        public const string ExampleNextJsSetup = "learn://examples/nodejs/nextjs-setup.md";
        public const string ExampleLangchainJsSetup = "learn://examples/nodejs/langchain-js-setup.md";

        // Node.js Database Integrations
        public const string ExamplePostgresSetup = "learn://examples/nodejs/postgres-setup.md";
        public const string ExampleMongoDBSetup = "learn://examples/nodejs/mongodb-setup.md";
        public const string ExampleRedisSetup = "learn://examples/nodejs/redis-setup.md";
        public const string ExampleMySQLSetup = "learn://examples/nodejs/mysql-setup.md";

        // Node.js Logging Integrations
        public const string ExampleWinstonSetup = "learn://examples/nodejs/winston-setup.md";
        public const string ExampleBunyanSetup = "learn://examples/nodejs/bunyan-setup.md";
        public const string ExampleConsoleNodeJsSetup = "learn://examples/nodejs/console-setup.md";

        // Python
        public const string ConceptsOpenTelemetryPipelinePython = "learn://concepts/python/opentelemetry-pipeline.md";
        public const string ConceptsAzureMonitorPython = "learn://concepts/python/azure-monitor-overview.md";
        public const string ExampleDjangoSetup = "learn://examples/python/django-setup.md";
        public const string ExampleFlaskSetup = "learn://examples/python/flask-setup.md";
        public const string ExampleFastApiSetup = "learn://examples/python/fastapi-setup.md";
        public const string ExampleConsolePythonSetup = "learn://examples/python/console-setup.md";
        public const string ExampleGenAiSetup = "learn://examples/python/genai-setup.md";
        public const string ExampleGenericPythonSetup = "learn://examples/python/generic-setup.md";
    }

    // Package Information
    public static class Packages
    {
        public const string AzureMonitorAspNetCore = "Azure.Monitor.OpenTelemetry.AspNetCore";
        public const string ApplicationInsightsAspNetCore = "Microsoft.ApplicationInsights.AspNetCore";
        public const string ApplicationInsightsAspNetCore3x = "3.*";
        public const string WorkerService = "Microsoft.ApplicationInsights.WorkerService";
        public const string WorkerServiceVersion = "3.0.0-rc1";
        public const string PackageManagerNuGet = "nuget";
        public const string LatestStableVersion = "latest-stable";
    }

    // Configuration
    public static class Config
    {
        public const string AzureMonitorConnectionStringPath = "AzureMonitor.ConnectionString";
        public const string AppInsightsConnectionStringPath = "ApplicationInsights:ConnectionString";
        public const string ConnectionStringEnvVar = "APPLICATIONINSIGHTS_CONNECTION_STRING";
        public const string ConnectionStringPlaceholder = "<your-connection-string>";
        public const string AppSettingsFileName = "appsettings.json";
    }

    // Code Patterns
    public static class CodePatterns
    {
        // ASP.NET Core
        public const string UseAzureMonitorSnippet = "builder.Services.AddOpenTelemetry().UseAzureMonitor();";
        public const string WebApplicationCreateBuilderMarker = "WebApplication.CreateBuilder";
        public const string AzureMonitorNamespace = "Azure.Monitor.OpenTelemetry.AspNetCore";

        // Worker Service
        public const string AddWorkerServiceSnippet = "services.AddApplicationInsightsTelemetryWorkerService();";
        public const string HostCreateDefaultBuilderMarker = "Host.CreateDefaultBuilder";
        public const string HostCreateApplicationBuilderMarker = "Host.CreateApplicationBuilder";
        public const string WorkerServiceNamespace = "Microsoft.Extensions.DependencyInjection";
    }

    // Decision Intents
    public static class Intents
    {
        public const string Onboard = "onboard";
        public const string Migrate = "migrate";
        public const string Error = "error";
        public const string ClarificationNeeded = "clarification-needed";
        public const string Unsupported = "unsupported";
    }

    // Target Approaches
    public static class Approaches
    {
        public const string AzureMonitorDistro = "azure-monitor-distro";
        public const string ApplicationInsights3x = "appinsights-3x";
        public const string Manual = "manual";
        public const string None = "none";
    }

    // Package Detection
    public static class PackageDetection
    {
        // Known Application Insights SDK packages
        public static readonly string[] AiSdkPackages =
        [
            "Microsoft.ApplicationInsights",
            "Microsoft.ApplicationInsights.AspNetCore",
            "Microsoft.ApplicationInsights.WorkerService",
            "Microsoft.ApplicationInsights.Web"
        ];

        // Known OpenTelemetry packages
        public static readonly string[] OtelPackages =
        [
            "OpenTelemetry",
            "OpenTelemetry.Api",
            "OpenTelemetry.Extensions.Hosting"
        ];

        // Azure Monitor Distro packages
        public static readonly string[] AzureMonitorDistroPackages =
        [
            "Azure.Monitor.OpenTelemetry.AspNetCore",
            "Azure.Monitor.OpenTelemetry.Exporter"
        ];
    }
}
