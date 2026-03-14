namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

public sealed record ServiceOptionsTemplate
{
    public required string EntryPointFile { get; init; }
    public required string SetupPattern { get; init; }
    public required string InstrumentationKey { get; init; }
    public required string ConnectionString { get; init; }
    public required string EnableAdaptiveSampling { get; init; }
    public required string DeveloperMode { get; init; }
    public required string EndpointAddress { get; init; }
    public required string EnableHeartbeat { get; init; }
    public required string EnableDebugLogger { get; init; }
    public required string RequestCollectionOptions { get; init; }
    public required string DependencyCollectionOptions { get; init; }
    public required string SamplingRatio { get; init; }
    public required string TracesPerSecond { get; init; }
    public required string EnableQuickPulseMetricStream { get; init; }
    public required string UseApplicationInsights { get; init; }
    public required string AddTelemetryProcessor { get; init; }
    public required string ConfigureTelemetryModule { get; init; }
}
