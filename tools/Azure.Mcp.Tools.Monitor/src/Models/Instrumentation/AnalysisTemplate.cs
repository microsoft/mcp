namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

public sealed record AnalysisTemplate
{
    public required ServiceOptionsTemplate ServiceOptions { get; init; }
    public required InitializerTemplate Initializers { get; init; }
    public required ProcessorTemplate Processors { get; init; }
    public required ClientUsageTemplate ClientUsage { get; init; }
    public required SamplingTemplate Sampling { get; init; }

    public static AnalysisTemplate CreateDefault()
    {
        return new AnalysisTemplate
        {
            ServiceOptions = new ServiceOptionsTemplate
            {
                EntryPointFile = "(string) File containing AddApplicationInsightsTelemetry, e.g. Program.cs",
                SetupPattern = "(string) e.g. AddApplicationInsightsTelemetry",
                InstrumentationKey = "(string|null) options.InstrumentationKey value - REMOVED in 3.x",
                ConnectionString = "(string|null) options.ConnectionString value",
                EnableAdaptiveSampling = "(bool|null) - REMOVED in 3.x",
                DeveloperMode = "(bool|null) - REMOVED in 3.x",
                EndpointAddress = "(string|null) - REMOVED in 3.x, now part of ConnectionString",
                EnableHeartbeat = "(bool|null) - REMOVED in 3.x",
                EnableDebugLogger = "(bool|null) - REMOVED in 3.x",
                RequestCollectionOptions = "(string|null) - REMOVED in 3.x",
                DependencyCollectionOptions = "(string|null) - REMOVED in 3.x",
                SamplingRatio = "(number|null) - New in 3.x, already correct if set",
                TracesPerSecond = "(number|null) - New in 3.x, already correct if set",
                EnableQuickPulseMetricStream = "(bool|null) - unchanged",
                UseApplicationInsights = "(bool) true if UseApplicationInsights() call found - REMOVED in 3.x",
                AddTelemetryProcessor = "(bool) true if AddApplicationInsightsTelemetryProcessor<T>() found - REMOVED in 3.x",
                ConfigureTelemetryModule = "(bool) true if ConfigureTelemetryModule<T>() found - REMOVED in 3.x"
            },
            Initializers = new InitializerTemplate
            {
                Found = "(bool) true if any ITelemetryInitializer implementations exist",
                Implementations =
                [
                    new ImplementationTemplate
                    {
                        ClassName = "(string) class name",
                        File = "(string) file path",
                        Purpose = "(string) what this initializer does"
                    }
                ],
                Registrations = ["(string) the DI registration line, e.g. services.AddSingleton<ITelemetryInitializer, MyInit>()"]
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
                        Methods = ["(string) method names called, e.g. TrackEvent, TrackException"]
                    }
                ]
            },
            Sampling = new SamplingTemplate
            {
                HasCustomSampling = "(bool) true if any custom sampling config exists",
                Type = "(string|null) e.g. adaptive, fixed-rate",
                Details = "(string|null) description of what was found",
                File = "(string|null) file where sampling is configured"
            }
        };
    }
}
