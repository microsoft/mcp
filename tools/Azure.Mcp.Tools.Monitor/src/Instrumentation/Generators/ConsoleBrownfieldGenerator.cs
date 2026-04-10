using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Generator for Console app (and class library) brownfield projects migrating from
/// Application Insights 2.x to 3.x. Handles the non-DI pattern where TelemetryConfiguration
/// and TelemetryClient are created manually (not via AddApplicationInsightsTelemetry).
/// </summary>
public class ConsoleBrownfieldGenerator : BrownfieldGeneratorBase
{
    protected override AppType TargetAppType => AppType.Console;
    protected override string PackageName => Packages.ApplicationInsightsCore;
    protected override string PackageVersion => Packages.ApplicationInsightsCore3x;
    protected override string MigrationCodeResource => LearningResources.MigrationConsole2xTo3xCode;
    protected override string MigrationNoCodeChangeResource => LearningResources.MigrationConsole2xTo3xCode; // Always has code changes
    protected override string EntryPointMethodName => "TelemetryConfiguration.CreateDefault";

    protected override ProjectInfo FindProject(Analysis analysis)
        => analysis.Projects.First(p =>
            p.AppType is AppType.Console or AppType.Library);

    public override bool CanHandle(Analysis analysis)
    {
        var consoleOrLibraryCount = analysis.Projects
            .Count(p => p.AppType is AppType.Console or AppType.Library);

        // Don't match if this is a Console app using WorkerService package
        // (those are handled by WorkerServiceBrownfieldGenerator)
        var hasWorkerServicePackage = analysis.ExistingInstrumentation?.Evidence
            .Any(e => e.Indicator.Contains("Microsoft.ApplicationInsights.WorkerService", StringComparison.OrdinalIgnoreCase)) == true;

        return analysis.Language == Language.DotNet
            && consoleOrLibraryCount >= 1
            && !hasWorkerServicePackage
            && analysis.State == InstrumentationState.Brownfield
            && analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && analysis.ExistingInstrumentation?.IsTargetVersion != true
            && analysis.BrownfieldFindings is not null;
    }

    protected override List<string> BuildLearnResources(BrownfieldFindings? findings)
    {
        var resources = new List<string> { MigrationCodeResource };

        // Always include non-DI extensibility doc
        resources.Add(LearningResources.ApiConfigureOpenTelemetryBuilder);

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 }
            || findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            resources.Add(LearningResources.ApiActivityProcessors);
            resources.Add(LearningResources.ApiLogProcessors);
        }

        if (HasBreakingClientUsage(findings?.ClientUsage))
            resources.Add(LearningResources.ApiTelemetryClient);

        if (HasAadRelatedCustomizations(findings))
            resources.Add(LearningResources.MigrationAadAuthentication);

        if (findings?.Logging is { Found: true })
            resources.Add(LearningResources.MigrationILoggerMigration);

        return resources;
    }

    protected override bool HasFrameworkSpecificCodeChanges(ServiceOptionsFindings opts)
    {
        // Console apps always have code changes — TelemetryConfiguration setup must be updated
        return true;
    }

    protected override string AddServiceOptionsActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        var dep = AddSharedServiceOptionsActions(builder, opts, entryPoint, EntryPointMethodName, lastDependency);

        // Replace TelemetryConfiguration.Active with CreateDefault()
        var actionId = "fix-telemetry-config-active";
        builder.AddManualStepAction(
            actionId,
            "Replace TelemetryConfiguration.Active with CreateDefault()",
            $"In {entryPoint} and any other files, replace TelemetryConfiguration.Active with " +
            "TelemetryConfiguration.CreateDefault(). Note: CreateDefault() returns a static singleton in 3.x. " +
            "Also replace any config.InstrumentationKey assignments with config.ConnectionString. " +
            "ConnectionString is required in 3.x — the SDK will throw if it is not set.",
            dependsOn: dep);
        dep = actionId;

        // Remove manual module initialization
        actionId = "remove-manual-modules";
        builder.AddManualStepAction(
            actionId,
            "Remove manual TelemetryModule initialization",
            $"In {entryPoint} and any telemetry setup files, remove manual initialization of " +
            "DependencyTrackingTelemetryModule, PerformanceCollectorModule, and any other TelemetryModule " +
            "instances (new ...Module() + .Initialize(config)). " +
            "In 3.x, dependency tracking, performance counters, and request tracking are automatic — " +
            "no manual module setup is needed. " +
            "Also remove built-in initializers added manually: " +
            "HttpDependenciesParsingTelemetryInitializer, OperationCorrelationTelemetryInitializer — " +
            "correlation and HTTP parsing are automatic in 3.x.",
            dependsOn: dep);
        dep = actionId;

        // Remove config.TelemetryInitializers.Add() calls
        actionId = "remove-config-initializers-add";
        builder.AddManualStepAction(
            actionId,
            "Migrate config.TelemetryInitializers.Add() calls",
            $"In {entryPoint} and any telemetry setup files, the TelemetryInitializers collection on " +
            "TelemetryConfiguration is removed in 3.x. " +
            "For custom initializers added via config.TelemetryInitializers.Add(new MyInit()), " +
            "convert them to BaseProcessor<Activity> and register via: " +
            "config.ConfigureOpenTelemetryBuilder(builder => builder.WithTracing(t => t.AddProcessor<MyProcessor>())). " +
            "Important: add 'using OpenTelemetry;' — the WithTracing/WithLogging/ConfigureResource extension methods " +
            "are in the root OpenTelemetry namespace (not OpenTelemetry.Trace). " +
            "See the ActivityProcessors and TelemetryConfigurationBuilder API references.",
            dependsOn: dep);
        dep = actionId;

        // Remove DependencyCollector package
        actionId = "remove-dependency-collector";
        builder.AddManualStepAction(
            actionId,
            "Remove Microsoft.ApplicationInsights.DependencyCollector package",
            "Remove the Microsoft.ApplicationInsights.DependencyCollector NuGet package — " +
            "dependency tracking is automatic in 3.x and this package is no longer needed. " +
            "Run: dotnet remove package Microsoft.ApplicationInsights.DependencyCollector",
            dependsOn: dep);
        dep = actionId;

        return dep;
    }

    protected override string AddRemovedMethodActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency)
    {
        // Console apps don't have UseApplicationInsights() or ConfigureTelemetryModule
        // but may have TelemetryProcessorChainBuilder usage
        var dep = lastDependency;

        if (opts.AddTelemetryProcessor == true)
        {
            var actionId = "remove-processor-chain-builder";
            builder.AddManualStepAction(
                actionId,
                "Remove TelemetryProcessorChainBuilder usage",
                $"In {entryPoint}, remove any TelemetryProcessorChainBuilder or " +
                "config.TelemetryProcessors usage — these are removed in 3.x. " +
                "Convert to OpenTelemetry processors registered via " +
                "config.ConfigureOpenTelemetryBuilder(builder => builder.WithTracing(t => t.AddProcessor<T>())).",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    protected override void AddConnectionStringAction(OnboardingSpecBuilder builder, BrownfieldFindings? findings, string lastDependency)
    {
        // Console apps set ConnectionString directly on TelemetryConfiguration, not via appsettings.json.
        // The fix-telemetry-config-active step already covers this.
        // Only add a reminder if they didn't have InstrumentationKey in code (so it wasn't already covered)
        if (findings?.ServiceOptions?.InstrumentationKey == null)
        {
            builder.AddManualStepAction(
                "add-connection-string",
                "Set ConnectionString on TelemetryConfiguration",
                "Ensure config.ConnectionString is set before creating TelemetryClient. " +
                "ConnectionString is required in 3.x. Use the format: " +
                "\"InstrumentationKey=...;IngestionEndpoint=...\" " +
                "or set the APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.",
                dependsOn: lastDependency);
        }
    }
}
