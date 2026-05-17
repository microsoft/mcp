using Azure.Mcp.Tools.Monitor.Models;
using static Azure.Mcp.Tools.Monitor.Models.OnboardingConstants;

namespace Azure.Mcp.Tools.Monitor.Generators;

/// <summary>
/// Shared base for Application Insights 2.x → 3.x brownfield generators.
/// Contains logic that is identical across ASP.NET Core and Worker Service:
/// TelemetryClient breaking methods, initializer/processor migration, sampling.
/// Subclasses provide package names, migration docs, and service-options handling.
/// </summary>
public abstract class BrownfieldGeneratorBase : IGenerator
{
    // ── Subclass contracts ──────────────────────────────────────────────

    public abstract bool CanHandle(Analysis analysis);

    /// <summary>App-type filter for project lookup.</summary>
    protected abstract AppType TargetAppType { get; }

    /// <summary>Find the matching project. Override for multi-AppType matching (e.g. classic ASP.NET).</summary>
    protected virtual ProjectInfo FindProject(Analysis analysis)
        => analysis.Projects.First(p => p.AppType == TargetAppType);

    /// <summary>NuGet package name, e.g. "Microsoft.ApplicationInsights.AspNetCore".</summary>
    protected abstract string PackageName { get; }

    /// <summary>Target version specifier, e.g. "3.*".</summary>
    protected abstract string PackageVersion { get; }

    /// <summary>Learning resource URI for the code-change migration doc.</summary>
    protected abstract string MigrationCodeResource { get; }

    /// <summary>Learning resource URI for the no-code-change migration doc.</summary>
    protected abstract string MigrationNoCodeChangeResource { get; }

    /// <summary>Human-readable entry point method name (for action descriptions).</summary>
    protected abstract string EntryPointMethodName { get; }

    /// <summary>Package manager type for install command. Default "nuget" (dotnet add package). Override to "nuget-vs" for classic ASP.NET.</summary>
    protected virtual string PackageManagerType => Packages.PackageManagerNuGet;

    /// <summary>Default entry point file name. Override for classic ASP.NET (Global.asax.cs).</summary>
    protected virtual string DefaultEntryPoint => "Program.cs";

    /// <summary>Check for framework-specific removed properties / extension methods.</summary>
    protected abstract bool HasFrameworkSpecificCodeChanges(ServiceOptionsFindings opts);

    /// <summary>Generate migration actions for framework-specific removed properties.</summary>
    protected abstract string AddServiceOptionsActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency);

    /// <summary>Generate migration actions for framework-specific removed extension methods.</summary>
    protected abstract string AddRemovedMethodActions(
        OnboardingSpecBuilder builder, ServiceOptionsFindings opts,
        string entryPoint, string lastDependency);

    // ── Shared implementation ───────────────────────────────────────────

    public OnboardingSpec Generate(Analysis analysis)
    {
        var findings = analysis.BrownfieldFindings;
        var project = FindProject(analysis);
        var projectFile = project.ProjectFile;
        var entryPoint = project.EntryPoint ?? DefaultEntryPoint;

        var builder = new OnboardingSpecBuilder(analysis)
            .WithAgentPreExecuteInstruction(AgentPreExecuteInstruction);

        var needsCodeChange = HasCodeChanges(findings);

        if (!needsCodeChange)
        {
            return builder
                .WithDecision(
                    Intents.Migrate,
                    Approaches.ApplicationInsights3x,
                    "Existing Application Insights SDK detected with no removed properties or custom code. Package upgrade only.")
                .AddReviewEducationAction(
                    "review-migration",
                    "Review the no-code-change migration guide",
                    [MigrationNoCodeChangeResource])
                .AddPackageAction(
                    "upgrade-appinsights",
                    $"Upgrade {PackageName} to 3.x",
                    PackageManagerType,
                    PackageName,
                    PackageVersion,
                    projectFile,
                    "review-migration")
                .Build();
        }

        // Code-change path
        builder.WithDecision(
            Intents.Migrate,
            Approaches.ApplicationInsights3x,
            "Existing Application Insights SDK detected with properties/patterns that require code changes for migration.");

        var learnResources = BuildLearnResources(findings);

        builder.AddReviewEducationAction(
            "review-migration",
            "Review the migration guide and relevant API references",
            learnResources);

        builder.AddPackageAction(
            "upgrade-appinsights",
            $"Upgrade {PackageName} to 3.x",
            PackageManagerType,
            PackageName,
            PackageVersion,
            projectFile,
            "review-migration");

        var lastDependency = "upgrade-appinsights";

        if (findings?.ServiceOptions != null)
        {
            lastDependency = AddServiceOptionsActions(builder, findings.ServiceOptions, entryPoint, lastDependency);
            lastDependency = AddRemovedMethodActions(builder, findings.ServiceOptions, entryPoint, lastDependency);
        }

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 })
            lastDependency = AddInitializerActions(builder, findings.Initializers, lastDependency);

        if (findings?.Processors is { Found: true, Implementations.Count: > 0 })
            lastDependency = AddProcessorActions(builder, findings.Processors, lastDependency);

        if (HasBreakingClientUsage(findings?.ClientUsage))
            lastDependency = AddClientUsageActions(builder, findings!.ClientUsage!, lastDependency);

        if (findings?.Sampling is { HasCustomSampling: true })
        {
            // Only generate a sampling action if it's truly custom sampling,
            // not just EnableAdaptiveSampling (which is handled by remove-deprecated-options)
            var isJustAdaptiveSamplingFlag = findings.Sampling.Type?.Contains("adaptive", StringComparison.OrdinalIgnoreCase) == true
                && findings.ServiceOptions?.EnableAdaptiveSampling != null;

            if (!isJustAdaptiveSamplingFlag)
                lastDependency = AddSamplingActions(builder, findings.Sampling, lastDependency);
        }

        if (findings?.TelemetryPipeline is { Found: true })
            lastDependency = AddTelemetryPipelineActions(builder, findings.TelemetryPipeline, lastDependency);

        if (findings?.Logging is { Found: true })
            lastDependency = AddLoggingActions(builder, findings.Logging, lastDependency);

        // Connection string config — virtual so classic ASP.NET can override
        AddConnectionStringAction(builder, findings, lastDependency);

        return builder.Build();
    }

    /// <summary>Add the connection string config step. Override for non-JSON config (e.g. classic ASP.NET uses applicationinsights.config).</summary>
    protected virtual void AddConnectionStringAction(OnboardingSpecBuilder builder, BrownfieldFindings? findings, string lastDependency)
    {
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
    }

    // ── Shared helpers ──────────────────────────────────────────────────

    protected bool HasCodeChanges(BrownfieldFindings? findings)
    {
        if (findings == null)
            return false;

        var opts = findings.ServiceOptions;
        if (opts != null)
        {
            // Shared removed properties
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
            if (opts.DependencyCollectionOptions != null)
                return true;

            // Shared removed extension methods
            if (opts.AddTelemetryProcessor == true)
                return true;
            if (opts.ConfigureTelemetryModule == true)
                return true;
            if (opts.UsesInstrumentationKeyOverload == true)
                return true;

            // Framework-specific checks
            if (HasFrameworkSpecificCodeChanges(opts))
                return true;
        }

        if (findings.Initializers is { Found: true })
            return true;
        if (findings.Processors is { Found: true })
            return true;
        if (HasBreakingClientUsage(findings.ClientUsage))
            return true;
        if (findings.Sampling is { HasCustomSampling: true })
            return true;
        if (findings.TelemetryPipeline is { Found: true })
            return true;
        if (findings.Logging is { Found: true })
            return true;

        return false;
    }

    /// <summary>Build the list of learn resources based on findings. Override for framework-specific resource swaps.</summary>
    protected virtual List<string> BuildLearnResources(BrownfieldFindings? findings)
    {
        var resources = new List<string> { MigrationCodeResource };

        if (findings?.Sampling is { HasCustomSampling: true })
            resources.Add(LearningResources.ApiSampling);

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 }
            || findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            resources.Add(LearningResources.ApiActivityProcessors);
            resources.Add(LearningResources.ApiLogProcessors);
            resources.Add(LearningResources.ApiConfigureOpenTelemetryProvider);
        }

        if (HasBreakingClientUsage(findings?.ClientUsage))
            resources.Add(LearningResources.ApiTelemetryClient);

        // Surface AAD migration doc when an initializer or processor related to
        // credential / AAD / token authentication is detected (e.g. TelemetryConfigurationEnricher
        // that calls SetAzureTokenCredential).
        if (HasAadRelatedCustomizations(findings))
            resources.Add(LearningResources.MigrationAadAuthentication);

        if (findings?.Logging is { Found: true })
            resources.Add(LearningResources.MigrationILoggerMigration);

        return resources;
    }

    /// <summary>
    /// Returns true if any initializer or processor appears related to AAD / credential / token
    /// authentication based on its class name or stated purpose.
    /// </summary>
    protected static bool HasAadRelatedCustomizations(BrownfieldFindings? findings)
    {
        static bool IsAadRelated(string? text)
            => text != null && (
                text.Contains("aad", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("SetAzureTokenCredential", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Entra", StringComparison.OrdinalIgnoreCase));

        if (findings?.Initializers is { Found: true, Implementations.Count: > 0 })
        {
            if (findings.Initializers.Implementations.Any(i =>
                IsAadRelated(i.ClassName) || IsAadRelated(i.Purpose)))
                return true;
        }

        if (findings?.Processors is { Found: true, Implementations.Count: > 0 })
        {
            if (findings.Processors.Implementations.Any(p =>
                IsAadRelated(p.ClassName) || IsAadRelated(p.Purpose)))
                return true;
        }

        // Also check serviceOptions — Console apps may call SetAzureTokenCredential
        // directly on TelemetryConfiguration without a wrapper class
        if (IsAadRelated(findings?.ServiceOptions?.SetupPattern))
            return true;

        return false;
    }

    // ── TelemetryClient ─────────────────────────────────────────────────

    /// <summary>
    /// Methods on TelemetryClient that have removed overloads or are entirely removed in 3.x.
    /// </summary>
    protected static readonly HashSet<string> BreakingClientMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "TrackPageView",
        "TrackEvent",
        "TrackException",
        "TrackAvailability",
        "TrackDependency",
        "GetMetric",
    };

    protected static bool HasBreakingClientUsage(ClientUsageFindings? clientUsage)
    {
        if (clientUsage is not { DirectUsage: true, Usages.Count: > 0 })
            return false;

        return clientUsage.Usages
            .SelectMany(u => u.Methods)
            .Any(m => BreakingClientMethods.Contains(m));
    }

    protected static string AddClientUsageActions(
        OnboardingSpecBuilder builder,
        ClientUsageFindings clientUsage,
        string lastDependency)
    {
        var dep = lastDependency;

        foreach (var usage in clientUsage.Usages)
        {
            var breakingMethods = usage.Methods
                .Where(m => BreakingClientMethods.Contains(m))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (breakingMethods.Count == 0)
                continue;

            var fileName = Path.GetFileNameWithoutExtension(usage.File ?? "unknown").ToLowerInvariant();
            var actionId = $"fix-telemetryclient-{fileName}";

            var instructions = new List<string>();
            foreach (var method in breakingMethods)
            {
                instructions.Add(method switch
                {
                    "TrackPageView" =>
                        "TrackPageView is removed in 3.x — replace with TrackEvent(name, properties) or TrackRequest.",
                    "TrackEvent" =>
                        "TrackEvent 3-param overload (with IDictionary<string, double> metrics) is removed — " +
                        "remove the metrics dictionary parameter. Track metrics separately via TrackMetric().",
                    "TrackException" =>
                        "TrackException 3-param overload (with IDictionary<string, double> metrics) is removed — " +
                        "remove the metrics dictionary parameter. Track metrics separately via TrackMetric().",
                    "TrackAvailability" =>
                        "TrackAvailability 8-param overload (with trailing IDictionary<string, double> metrics) is removed — " +
                        "remove the metrics dictionary parameter. Track metrics separately via TrackMetric().",
                    "TrackDependency" =>
                        "The obsolete 5-param TrackDependency overload is removed — " +
                        "use the full overload with dependencyTypeName, target, data, startTime, duration, success.",
                    "GetMetric" =>
                        "GetMetric overloads with MetricConfiguration / MetricAggregationScope parameters are removed — " +
                        "use the simplified GetMetric(metricId) or GetMetric(metricId, dim1, ...).",
                    _ => $"{method} has breaking changes in 3.x — review TelemetryClient.md."
                });
            }

            builder.AddManualStepAction(
                actionId,
                $"Fix TelemetryClient breaking calls in {usage.File ?? "unknown"}",
                $"File: {usage.File ?? "unknown"}. " +
                $"The following TelemetryClient methods have breaking changes in 3.x: {string.Join(", ", breakingMethods)}. " +
                string.Join(" ", instructions),
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    // ── Initializers ────────────────────────────────────────────────────

    protected static string AddInitializerActions(
        OnboardingSpecBuilder builder,
        InitializerFindings initializers,
        string lastDependency)
    {
        var dep = lastDependency;

        foreach (var init in initializers.Implementations)
        {
            // AAD-related entries get a different migration action — they're not real
            // ITelemetryInitializer implementations but IConfigureOptions<TelemetryConfiguration>
            // or direct SetAzureTokenCredential calls reported for AAD doc surfacing.
            if (IsAadRelatedEntry(init))
            {
                var aadActionId = $"migrate-aad-{init.ClassName.ToLowerInvariant()}";

                var registration = initializers.Registrations
                    .FirstOrDefault(r => r.Contains(init.ClassName, StringComparison.OrdinalIgnoreCase));
                var removeRegistration = registration != null
                    ? $" Remove the DI registration: `{registration}`."
                    : "";

                builder.AddManualStepAction(
                    aadActionId,
                    $"Migrate AAD authentication ({init.ClassName})",
                    $"File: {init.File ?? "unknown"}. {init.Purpose ?? "AAD authentication configuration"}. " +
                    "In 3.x, for DI scenarios use options.Credential on ApplicationInsightsServiceOptions. " +
                    "For non-DI (Console) scenarios, SetAzureTokenCredential(TokenCredential) is available — " +
                    "note the parameter type changed from object (2.x) to TokenCredential (3.x). " +
                    "See the AAD Authentication Migration guide for details." +
                    removeRegistration,
                    dependsOn: dep);
                dep = aadActionId;
                continue;
            }

            var actionId = $"migrate-initializer-{init.ClassName.ToLowerInvariant()}";
            var purpose = !string.IsNullOrWhiteSpace(init.Purpose) ? $" ({init.Purpose})" : "";

            var reg = initializers.Registrations
                .FirstOrDefault(r => r.Contains(init.ClassName, StringComparison.OrdinalIgnoreCase));
            var removeReg = reg != null
                ? $" Remove the old DI registration: `{reg}` — ITelemetryInitializer no longer exists in 3.x."
                : " Also remove the old AddSingleton<ITelemetryInitializer, ...>() DI registration — ITelemetryInitializer no longer exists in 3.x.";

            builder.AddManualStepAction(
                actionId,
                $"Convert ITelemetryInitializer '{init.ClassName}' to OpenTelemetry processor",
                $"Convert {init.ClassName}{purpose} from ITelemetryInitializer to a BaseProcessor<Activity>.OnStart implementation. " +
                $"File: {init.File ?? "unknown"}. " +
                "The initializer's Initialize(ITelemetry) method should become OnStart(Activity). " +
                "If the initializer touched all telemetry (not just RequestTelemetry/DependencyTelemetry), also create a BaseProcessor<LogRecord>.OnEnd for the log side — see LogProcessors.md. " +
                "Register the new processor(s) via .AddProcessor<T>() in the OpenTelemetry pipeline setup." +
                removeReg,
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    /// <summary>
    /// Returns true if an initializer entry is actually an AAD/credential related
    /// configuration (IConfigureOptions or direct SetAzureTokenCredential call)
    /// rather than a real ITelemetryInitializer.
    /// </summary>
    private static bool IsAadRelatedEntry(InitializerInfo init)
    {
        static bool Contains(string? text, string term)
            => text?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

        return Contains(init.ClassName, "credential") ||
               Contains(init.ClassName, "aad") ||
               Contains(init.ClassName, "Entra") ||
               Contains(init.Purpose, "SetAzureTokenCredential") ||
               Contains(init.Purpose, "AAD") ||
               Contains(init.Purpose, "IConfigureOptions<TelemetryConfiguration>");
    }

    // ── Processors ──────────────────────────────────────────────────────

    protected static string AddProcessorActions(
        OnboardingSpecBuilder builder,
        ProcessorFindings processors,
        string lastDependency)
    {
        var dep = lastDependency;

        foreach (var proc in processors.Implementations)
        {
            var actionId = $"migrate-processor-{proc.ClassName.ToLowerInvariant()}";
            var purpose = !string.IsNullOrWhiteSpace(proc.Purpose) ? $" ({proc.Purpose})" : "";

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

    // ── Sampling ────────────────────────────────────────────────────────

    protected string AddSamplingActions(
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
            $"Use TracesPerSecond or SamplingRatio in the new {EntryPointMethodName} options, " +
            "or configure a custom OTel sampler via .SetSampler<T>().",
            dependsOn: lastDependency);
        return actionId;
    }

    // ── Shared service-options helpers ───────────────────────────────────

    /// <summary>Generate actions for shared removed properties (IKey, adaptive sampling, etc.).</summary>
    protected static string AddSharedServiceOptionsActions(
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string entryPointMethodName,
        string lastDependency)
    {
        var dep = lastDependency;

        if (opts.InstrumentationKey != null)
        {
            var actionId = "migrate-ikey";
            builder.AddManualStepAction(
                actionId,
                "Replace InstrumentationKey with ConnectionString",
                $"In {entryPoint}, inside the {entryPointMethodName} options block, " +
                $"remove the line `options.InstrumentationKey = \"{opts.InstrumentationKey}\";` and replace it with " +
                "`options.ConnectionString = \"InstrumentationKey=...;IngestionEndpoint=...\";` " +
                "(use your actual connection string). " +
                "Alternatively, remove the InstrumentationKey line entirely and set the " +
                "APPLICATIONINSIGHTS_CONNECTION_STRING environment variable instead.",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    /// <summary>Generate actions for shared removed extension methods.</summary>
    protected static string AddSharedRemovedMethodActions(
        OnboardingSpecBuilder builder,
        ServiceOptionsFindings opts,
        string entryPoint,
        string entryPointMethodName,
        string lastDependency)
    {
        var dep = lastDependency;

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

        if (opts.UsesInstrumentationKeyOverload == true)
        {
            var actionId = "remove-ikey-overload";
            builder.AddManualStepAction(
                actionId,
                "Replace instrumentation key string overload",
                $"In {entryPoint}, the call to {entryPointMethodName}(\"ikey\") with a string argument is removed in 3.x. " +
                $"Replace with the parameterless {entryPointMethodName}() and set ConnectionString via options or the " +
                "APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    // ── TelemetryChannel / TelemetrySinks ───────────────────────────────

    protected static string AddTelemetryPipelineActions(
        OnboardingSpecBuilder builder,
        TelemetryPipelineFindings pipeline,
        string lastDependency)
    {
        var dep = lastDependency;

        if (pipeline.HasCustomChannel)
        {
            var className = pipeline.ClassName ?? "custom ITelemetryChannel";
            var details = !string.IsNullOrWhiteSpace(pipeline.Details) ? $" Details: {pipeline.Details}" : "";
            var actionId = "remove-telemetry-channel";
            builder.AddManualStepAction(
                actionId,
                $"Remove custom TelemetryChannel usage ({className})",
                $"The TelemetryChannel property on TelemetryConfiguration is removed in 3.x.{details} " +
                $"File: {pipeline.File ?? "unknown"}. " +
                "If using InMemoryChannel or ServerTelemetryChannel, remove the assignment — " +
                "the 3.x SDK manages its own export pipeline internally via OpenTelemetry. " +
                "If using a fully custom ITelemetryChannel, convert to an OpenTelemetry exporter instead.",
                dependsOn: dep);
            dep = actionId;
        }

        if (pipeline.HasTelemetrySinks)
        {
            var actionId = "remove-telemetry-sinks";
            builder.AddManualStepAction(
                actionId,
                "Remove TelemetrySinks / DefaultTelemetrySink usage",
                $"TelemetrySinks and DefaultTelemetrySink properties on TelemetryConfiguration are removed in 3.x. " +
                $"File: {pipeline.File ?? "unknown"}. " +
                "The 3.x SDK uses OpenTelemetry exporters internally. " +
                "If you need to export to multiple destinations, configure additional OpenTelemetry exporters " +
                "via ConfigureOpenTelemetryBuilder on TelemetryConfiguration or via the DI pipeline.",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }

    // ── Logging ─────────────────────────────────────────────────────────

    protected static string AddLoggingActions(
        OnboardingSpecBuilder builder,
        LoggingFindings logging,
        string lastDependency)
    {
        var dep = lastDependency;

        if (logging.HasExplicitLoggerProvider)
        {
            var actionId = "remove-explicit-logger-provider";
            builder.AddManualStepAction(
                actionId,
                "Remove explicit AddApplicationInsights() logger provider registration",
                $"In {logging.File ?? "unknown"}, remove the call to AddApplicationInsights() on ILoggingBuilder " +
                "(e.g. loggingBuilder.AddApplicationInsights() or services.AddLogging(b => b.AddApplicationInsights(...))). " +
                "In 3.x, ILogger output is exported to Application Insights automatically — no explicit logger provider registration is needed. " +
                "Also remove any ApplicationInsightsLoggerOptions configuration (e.g. TrackExceptionsAsExceptionTelemetry, IncludeScopes) — " +
                "these options no longer exist.",
                dependsOn: dep);
            dep = actionId;
        }

        if (logging.LogFilters.Count > 0)
        {
            var filterLines = string.Join("; ", logging.LogFilters);
            var actionId = "remove-ai-log-filters";
            builder.AddManualStepAction(
                actionId,
                "Migrate AddFilter<ApplicationInsightsLoggerProvider> to OpenTelemetry",
                $"In {logging.File ?? "unknown"}, replace the following log filter registrations: {filterLines}. " +
                "ApplicationInsightsLoggerProvider no longer exists in 3.x — logging now flows through OpenTelemetryLoggerProvider. " +
                "Replace AddFilter<ApplicationInsightsLoggerProvider>(...) with either: " +
                "(1) AddFilter<OpenTelemetryLoggerProvider>(category, level) to target the OTel provider specifically " +
                "(requires 'using OpenTelemetry.Logs;'), or " +
                "(2) AddFilter(category, level) for a provider-agnostic filter, or " +
                "(3) configure log-level filtering in appsettings.json under the \"Logging:LogLevel\" section. " +
                "Also replace 'using Microsoft.Extensions.Logging.ApplicationInsights;' with 'using OpenTelemetry.Logs;' if using option (1). " +
                "For advanced log filtering (dropping specific log records), use a BaseProcessor<LogRecord> " +
                "registered via ConfigureOpenTelemetryLoggerProvider.",
                dependsOn: dep);
            dep = actionId;
        }

        return dep;
    }
}
