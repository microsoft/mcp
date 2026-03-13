using Azure.Mcp.Tools.MonitorInstrumentation.Models;
using static Azure.Mcp.Tools.MonitorInstrumentation.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Generators;

/// <summary>
/// Generator for ASP.NET Core brownfield projects migrating from Application Insights SDK 2.x to 3.x.
/// Routes brownfield findings to existing learn resources and produces targeted migration actions.
/// </summary>
public class AspNetCoreBrownfieldGenerator : IGenerator
{
    public bool CanHandle(Analysis analysis)
    {
        var aspNetCoreProjects = analysis.Projects
            .Where(p => p.AppType == AppType.AspNetCore)
            .ToList();

        // BrownfieldFindings must be populated (by SendBrownfieldAnalysisTool)
        // so we don't match during the initial WorkspaceAnalyzer scan.
        return analysis.Language == Language.DotNet
            && aspNetCoreProjects.Count == 1
            && analysis.State == InstrumentationState.Brownfield
            && analysis.ExistingInstrumentation?.Type == InstrumentationType.ApplicationInsightsSdk
            && analysis.BrownfieldFindings is not null;
    }

    public OnboardingSpec Generate(Analysis analysis)
    {
        var findings = analysis.BrownfieldFindings;
        var project = analysis.Projects.First(p => p.AppType == AppType.AspNetCore);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? "Program.cs";

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction);

        // Determine if any code changes are needed
        var needsCodeChange = HasCodeChanges(findings);

        if (!needsCodeChange)
        {
            // No-code-change path: just bump the package version
            return builder
                .WithDecision(
                    Intents.Migrate,
                    Approaches.ApplicationInsights3x,
                    "Existing Application Insights SDK detected with no removed properties or custom code. Package upgrade only.")
                .AddReviewEducationAction(
                    "review-migration",
                    "Review the no-code-change migration guide",
                    [LearningResources.MigrationAppInsights2xTo3xNoCodeChange])
                .AddPackageAction(
                    "upgrade-appinsights",
                    "Upgrade Microsoft.ApplicationInsights.AspNetCore to 3.x",
                    Packages.PackageManagerNuGet,
                    Packages.ApplicationInsightsAspNetCore,
                    Packages.ApplicationInsightsAspNetCore3x,
                    projectFile,
                    "review-migration")
                .Build();
        }

        // Code-change path: build targeted migration plan based on findings
        builder.WithDecision(
            Intents.Migrate,
            Approaches.ApplicationInsights3x,
            "Existing Application Insights SDK detected with properties/patterns that require code changes for migration.");

        // Collect education resources based on what was found
        var learnResources = new List<string> { LearningResources.MigrationAppInsights2xTo3xCode };

        if (findings?.Sampling != null && findings.Sampling.HasCustomSampling)
        {
            learnResources.Add(LearningResources.ApiSampling);
        }

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 }
            || findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            learnResources.Add(LearningResources.ApiActivityProcessors);
            learnResources.Add(LearningResources.ApiLogProcessors);
            learnResources.Add(LearningResources.ApiConfigureOpenTelemetryProvider);
        }

        builder.AddReviewEducationAction(
            "review-migration",
            "Review the migration guide and relevant API references",
            learnResources);

        // Package upgrade
        builder.AddPackageAction(
            "upgrade-appinsights",
            "Upgrade Microsoft.ApplicationInsights.AspNetCore to 3.x",
            Packages.PackageManagerNuGet,
            Packages.ApplicationInsightsAspNetCore,
            Packages.ApplicationInsightsAspNetCore3x,
            projectFile,
            "review-migration");

        var lastDependency = "upgrade-appinsights";

        // Route service options findings
        if (findings?.ServiceOptions != null)
        {
            lastDependency = AddServiceOptionsActions(builder, findings.ServiceOptions, entryPoint, lastDependency);
        }

        // Route removed extension method findings
        if (findings?.ServiceOptions != null)
        {
            lastDependency = AddRemovedMethodActions(builder, findings.ServiceOptions, entryPoint, lastDependency);
        }

        // Route telemetry initializer findings
        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 })
        {
            lastDependency = AddInitializerActions(builder, findings.Initializers, lastDependency);
        }

        // Route telemetry processor findings
        if (findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            lastDependency = AddProcessorActions(builder, findings.Processors, lastDependency);
        }

        // TelemetryClient still works in 3.x — no migration actions needed for direct usage

        // Route custom sampling findings
        if (findings?.Sampling is { HasCustomSampling: true })
        {
            lastDependency = AddSamplingActions(builder, findings.Sampling, lastDependency);
        }

        // Connection string config (use AppInsights path, not AzureMonitor distro path)
        // If InstrumentationKey was found, tell the user to replace it in config too
        var hasIkeyInCode = findings?.ServiceOptions?.InstrumentationKey != null;
        var configDescription = hasIkeyInCode
            ? "Replace InstrumentationKey with ConnectionString in appsettings.json (remove the old ApplicationInsights.InstrumentationKey entry)"
            : "Configure Azure Monitor connection string";

        builder.AddConfigAction(
            "add-connection-string",
            configDescription,
            Config.AppSettingsFileName,
            Config.AppInsightsConnectionStringPath,
            Config.ConnectionStringPlaceholder,
            Config.ConnectionStringEnvVar,
            lastDependency);

        return builder.Build();
    }

    private static bool HasCodeChanges(BrownfieldFindings? findings)
    {
        if (findings == null)
            return false;

        var opts = findings.ServiceOptions;
        if (opts != null)
        {
            // Check for removed properties
            if (opts.InstrumentationKey != null)
                return true;
            if (opts.EnableAdaptiveSampling != null)
                return true;
            if (opts.DeveloperMode != null)
                return true;
            if (opts.EndpointAddress != null)
                return true;
            if (opts.EnableHeartbeat != null)
                return true;
            if (opts.EnableDebugLogger != null)
                return true;
            if (opts.RequestCollectionOptions != null)
                return true;
            if (opts.DependencyCollectionOptions != null)
                return true;

            // Check for removed extension methods
            if (opts.UseApplicationInsights == true)
                return true;
            if (opts.AddTelemetryProcessor == true)
                return true;
            if (opts.ConfigureTelemetryModule == true)
                return true;
        }

        if (findings.Initializers is { Found: true })
            return true;
        if (findings.Processors is { Found: true })
            return true;
        // TelemetryClient still works in 3.x — no code changes needed
        if (findings.Sampling is { HasCustomSampling: true })
            return true;

        return false;
    }

    private static string AddServiceOptionsActions(
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string lastDependency)
    {
        var dep = lastDependency;

        // Instrumentation key → connection string migration
        if (opts.InstrumentationKey != null)
        {
            var actionId = "migrate-ikey";
            builder.AddManualStepAction(
                actionId,
                "Replace InstrumentationKey with ConnectionString",
                $"In {entryPoint}, inside the AddApplicationInsightsTelemetry options block, " +
                $"remove the line `options.InstrumentationKey = \"{opts.InstrumentationKey}\";` and replace it with " +
                "`options.ConnectionString = \"InstrumentationKey=...;IngestionEndpoint=...\";` " +
                "(use your actual connection string). " +
                "Alternatively, remove the InstrumentationKey line entirely and set the " +
                "APPLICATIONINSIGHTS_CONNECTION_STRING environment variable instead.",
                dependsOn: dep);
            dep = actionId;
        }

        // Removed properties — generate delete actions
        var removedProperties = new List<(string name, object? value)>
        {
            ("EnableAdaptiveSampling", opts.EnableAdaptiveSampling),
            ("DeveloperMode", opts.DeveloperMode),
            ("EndpointAddress", opts.EndpointAddress),
            ("EnableHeartbeat", opts.EnableHeartbeat),
            ("EnableDebugLogger", opts.EnableDebugLogger),
            ("RequestCollectionOptions", opts.RequestCollectionOptions),
            ("DependencyCollectionOptions", opts.DependencyCollectionOptions),
        };

        var removedFound = removedProperties.Where(p => p.value != null).Select(p => p.name).ToList();
        if (removedFound.Count > 0)
        {
            var actionId = "remove-deprecated-options";
            builder.AddManualStepAction(
                actionId,
                "Remove deprecated ApplicationInsightsServiceOptions properties",
                $"In {entryPoint}, remove these properties from the AddApplicationInsightsTelemetry options block — they are removed in 3.x: {string.Join(", ", removedFound)}",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    private static string AddRemovedMethodActions(
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string lastDependency)
    {
        var dep = lastDependency;

        if (opts.UseApplicationInsights == true)
        {
            var actionId = "remove-use-appinsights";
            builder.AddManualStepAction(
                actionId,
                "Remove UseApplicationInsights() call",
                $"In {entryPoint}, remove the call to UseApplicationInsights() on IWebHostBuilder — it is removed in 3.x.",
                dependsOn: dep);
            dep = actionId;
        }

        if (opts.AddTelemetryProcessor == true)
        {
            var actionId = "remove-add-processor-ext";
            builder.AddManualStepAction(
                actionId,
                "Remove AddApplicationInsightsTelemetryProcessor<T>() call",
                $"In {entryPoint}, remove the call to AddApplicationInsightsTelemetryProcessor<T>() — it is removed in 3.x. Convert to an OpenTelemetry processor instead.",
                dependsOn: dep);
            dep = actionId;
        }

        if (opts.ConfigureTelemetryModule == true)
        {
            var actionId = "remove-configure-module";
            builder.AddManualStepAction(
                actionId,
                "Remove ConfigureTelemetryModule<T>() call",
                $"In {entryPoint}, remove the call to ConfigureTelemetryModule<T>() — it is removed in 3.x.",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    private static string AddInitializerActions(
        OnboardingSpecBuilder builder,
        InitializerFindings initializers,
        string lastDependency)
    {
        var dep = lastDependency;

        foreach (var init in initializers.Implementations)
        {
            var actionId = $"migrate-initializer-{init.ClassName.ToLowerInvariant()}";
            var purpose = !string.IsNullOrWhiteSpace(init.Purpose) ? $" ({init.Purpose})" : "";

            // Find the matching DI registration for this initializer, if captured
            var registration = initializers.Registrations
                .FirstOrDefault(r => r.Contains(init.ClassName, StringComparison.OrdinalIgnoreCase));
            var removeRegistration = registration != null
                ? $" Remove the old DI registration: `{registration}` — ITelemetryInitializer no longer exists in 3.x."
                : " Also remove the old AddSingleton<ITelemetryInitializer, ...>() DI registration — ITelemetryInitializer no longer exists in 3.x.";

            builder.AddManualStepAction(
                actionId,
                $"Convert ITelemetryInitializer '{init.ClassName}' to OpenTelemetry processor",
                $"Convert {init.ClassName}{purpose} from ITelemetryInitializer to a BaseProcessor<Activity>.OnStart implementation. " +
                $"File: {init.File ?? "unknown"}. " +
                "The initializer's Initialize(ITelemetry) method should become OnStart(Activity). " +
                "If the initializer touched all telemetry (not just RequestTelemetry/DependencyTelemetry), also create a BaseProcessor<LogRecord>.OnEnd for the log side — see LogProcessors.md. " +
                "Register the new processor(s) via .AddProcessor<T>() in the OpenTelemetry pipeline setup." +
                removeRegistration,
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    private static string AddProcessorActions(
        OnboardingSpecBuilder builder,
        ProcessorFindings processors,
        string lastDependency)
    {
        var dep = lastDependency;

        foreach (var proc in processors.Implementations)
        {
            var actionId = $"migrate-processor-{proc.ClassName.ToLowerInvariant()}";
            var purpose = !string.IsNullOrWhiteSpace(proc.Purpose) ? $" ({proc.Purpose})" : "";

            // Find the matching registration for this processor, if captured
            var registration = processors.Registrations
                .FirstOrDefault(r => r.Contains(proc.ClassName, StringComparison.OrdinalIgnoreCase));
            var removeRegistration = registration != null
                ? $" Remove the old registration: `{registration}` — ITelemetryProcessor no longer exists in 3.x."
                : " Also remove any old ITelemetryProcessor DI registration — ITelemetryProcessor no longer exists in 3.x.";

            builder.AddManualStepAction(
                actionId,
                $"Convert ITelemetryProcessor '{proc.ClassName}' to OpenTelemetry processor",
                $"Convert {proc.ClassName}{purpose} from ITelemetryProcessor to a BaseProcessor<Activity>.OnEnd implementation. " +
                $"File: {proc.File ?? "unknown"}. " +
                "The processor's Process(ITelemetry) method should become OnEnd(Activity). " +
                "To drop telemetry, clear the Recorded flag: data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded. " +
                "If the processor also handled TraceTelemetry/EventTelemetry, use ILoggingBuilder.AddFilter for log filtering or create a BaseProcessor<LogRecord>.OnEnd for log enrichment — see LogProcessors.md. " +
                "Register the new processor(s) via .AddProcessor<T>() in the OpenTelemetry pipeline setup." +
                removeRegistration,
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    private static string AddSamplingActions(
        OnboardingSpecBuilder builder,
        SamplingFindings sampling,
        string lastDependency)
    {
        var details = !string.IsNullOrWhiteSpace(sampling.Details) ? $" Current config: {sampling.Details}" : "";
        var actionId = "migrate-sampling";
        builder.AddManualStepAction(
            actionId,
            "Migrate custom sampling configuration to OpenTelemetry",
            $"Replace the existing {sampling.Type ?? "custom"} sampling configuration with OpenTelemetry sampling.{details} " +
            $"File: {sampling.File ?? "unknown"}. " +
            "Use TracesPerSecond or SamplingRatio in the new AddApplicationInsightsTelemetry options, " +
            "or configure a custom OTel sampler via .SetSampler<T>().",
            dependsOn: lastDependency);
        return actionId;
    }
}
