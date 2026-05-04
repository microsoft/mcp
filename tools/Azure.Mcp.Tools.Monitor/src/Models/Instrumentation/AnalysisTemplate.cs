namespace Azure.Mcp.Tools.Monitor.Models;

public sealed record AnalysisTemplate
{
    public required ServiceOptionsTemplate ServiceOptions { get; init; }
    public required InitializerTemplate Initializers { get; init; }
    public required ProcessorTemplate Processors { get; init; }
    public required ClientUsageTemplate ClientUsage { get; init; }
    public required SamplingTemplate Sampling { get; init; }
    public required TelemetryPipelineTemplate TelemetryPipeline { get; init; }
    public required LoggingTemplate Logging { get; init; }

    public static AnalysisTemplate CreateDefault()
    {
        return new AnalysisTemplate
        {
            ServiceOptions = new ServiceOptionsTemplate
            {
                EntryPointFile = "(string) File containing AddApplicationInsightsTelemetry or AddApplicationInsightsTelemetryWorkerService, e.g. Program.cs",
                SetupPattern = "(string) e.g. AddApplicationInsightsTelemetry or AddApplicationInsightsTelemetryWorkerService",
                InstrumentationKey = "(string|null) options.InstrumentationKey value - REMOVED in 3.x",
                ConnectionString = "(string|null) options.ConnectionString value",
                EnableAdaptiveSampling = "(bool|null) - REMOVED in 3.x",
                DeveloperMode = "(bool|null) - REMOVED in 3.x",
                EndpointAddress = "(string|null) - REMOVED in 3.x, now part of ConnectionString",
                EnableHeartbeat = "(bool|null) - REMOVED in 3.x",
                EnableDebugLogger = "(bool|null) - REMOVED in 3.x",
                RequestCollectionOptions = "(string|null) - REMOVED in 3.x",
                DependencyCollectionOptions = "(string|null) - REMOVED in 3.x",
                EnableEventCounterCollectionModule = "(bool|null) - REMOVED in 3.x (Worker Service only)",
                EnableAppServicesHeartbeatTelemetryModule = "(bool|null) - REMOVED in 3.x (Worker Service only)",
                EnableAzureInstanceMetadataTelemetryModule = "(bool|null) - REMOVED in 3.x (Worker Service only)",
                EnableDiagnosticsTelemetryModule = "(bool|null) - REMOVED in 3.x (Worker Service only)",
                SamplingRatio = "(number|null) - New in 3.x, already correct if set",
                TracesPerSecond = "(number|null) - New in 3.x, already correct if set",
                EnableQuickPulseMetricStream = "(bool|null) - unchanged",
                UseApplicationInsights = "(bool) true if UseApplicationInsights() call found - REMOVED in 3.x",
                AddTelemetryProcessor = "(bool) true if AddApplicationInsightsTelemetryProcessor<T>() found - REMOVED in 3.x",
                ConfigureTelemetryModule = "(bool) true if ConfigureTelemetryModule<T>() found - REMOVED in 3.x",
                UsesInstrumentationKeyOverload = "(bool) true if string overload e.g. AddApplicationInsightsTelemetry(\"ikey\") or AddApplicationInsightsTelemetryWorkerService(\"ikey\") found - REMOVED in 3.x"
            },
            Initializers = new InitializerTemplate
            {
                Found = "(bool) true if any ITelemetryInitializer or IConfigureOptions<TelemetryConfiguration> implementations exist",
                Implementations =
                [
                    new ImplementationTemplate
                    {
                        ClassName = "(string) class name",
                        File = "(string) file path",
                        Purpose = "(string) what this initializer does \u2014 for IConfigureOptions<TelemetryConfiguration>, mention if it calls SetAzureTokenCredential for AAD auth"
                    }
                ],
                Registrations = ["(string) the DI registration line, e.g. services.AddSingleton<ITelemetryInitializer, MyInit>() or services.AddSingleton<IConfigureOptions<TelemetryConfiguration>, MyEnricher>()"]
            },
            Processors = new ProcessorTemplate
            {
                Found = "(bool) true if any ITelemetryProcessor implementations exist",
                Implementations =
                [
                    new ImplementationTemplate
                    {
                        ClassName = "(string) class name",
                        File = "(string) file path",
                        Purpose = "(string) what this processor does"
                    }
                ],
                Registrations = ["(string) the registration line"]
            },
            ClientUsage = new ClientUsageTemplate
            {
                DirectUsage = "(bool) true if TelemetryClient is used anywhere",
                Usages =
                [
                    new ClientUsageEntryTemplate
                    {
                        File = "(string) file path",
                        Pattern = "(string) e.g. Constructor injection of TelemetryClient",
                        Methods = ["(string) every Track*/GetMetric method name called, e.g. TrackEvent, TrackException, TrackPageView, TrackAvailability, TrackTrace, TrackMetric, TrackDependency, GetMetric"]
                    }
                ]
            },
            Sampling = new SamplingTemplate
            {
                HasCustomSampling = "(bool) true if any custom sampling config exists",
                Type = "(string|null) e.g. adaptive, fixed-rate",
                Details = "(string|null) description of what was found",
                File = "(string|null) file where sampling is configured"
            },
            TelemetryPipeline = new TelemetryPipelineTemplate
            {
                Found = "(bool) true if any custom ITelemetryChannel, TelemetryConfiguration.TelemetryChannel assignment, TelemetrySinks, or DefaultTelemetrySink usage exists",
                HasCustomChannel = "(bool) true if custom ITelemetryChannel implementation or TelemetryChannel assignment found",
                HasTelemetrySinks = "(bool) true if TelemetrySinks or DefaultTelemetrySink usage found",
                ClassName = "(string|null) class name if custom channel implementation found",
                File = "(string|null) file path",
                Details = "(string|null) description of what was found"
            },
            Logging = new LoggingTemplate
            {
                Found = "(bool) true if any explicit Application Insights logger provider configuration exists",
                HasExplicitLoggerProvider = "(bool) true if AddApplicationInsights() on ILoggingBuilder found (e.g. loggingBuilder.AddApplicationInsights() or services.AddLogging(b => b.AddApplicationInsights(...)))",
                LogFilters = ["(string) each AddFilter<ApplicationInsightsLoggerProvider>(...) line found"],
                File = "(string|null) file where the logging configuration is located"
            }
        };
    }
}
