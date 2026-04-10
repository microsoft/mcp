namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Combined brownfield findings sent by the LLM after analyzing the codebase.
/// Each section is nullable — the LLM sets a section to null if the concern doesn't exist.
/// </summary>
public record BrownfieldFindings
{
    public ServiceOptionsFindings? ServiceOptions { get; init; }
    public InitializerFindings? Initializers { get; init; }
    public ProcessorFindings? Processors { get; init; }
    public ClientUsageFindings? ClientUsage { get; init; }
    public SamplingFindings? Sampling { get; init; }
    public TelemetryPipelineFindings? TelemetryPipeline { get; init; }
    public LoggingFindings? Logging { get; init; }
}

/// <summary>
/// Findings from analyzing the AddApplicationInsightsTelemetry() call and its options.
/// </summary>
public record ServiceOptionsFindings
{
    /// <summary>The file containing the setup call (e.g., "Program.cs")</summary>
    public string? EntryPointFile { get; init; }

    /// <summary>The setup pattern found (e.g., "AddApplicationInsightsTelemetry")</summary>
    public string? SetupPattern { get; init; }

    // --- Properties removed in 3.x (need migration) ---
    public string? InstrumentationKey { get; init; }
    public bool? EnableAdaptiveSampling { get; init; }
    public bool? DeveloperMode { get; init; }
    public string? EndpointAddress { get; init; }
    public bool? EnableHeartbeat { get; init; }
    public bool? EnableDebugLogger { get; init; }
    public string? RequestCollectionOptions { get; init; }
    public string? DependencyCollectionOptions { get; init; }

    // --- Properties removed in 3.x (Worker Service only) ---
    public bool? EnableEventCounterCollectionModule { get; init; }
    public bool? EnableAppServicesHeartbeatTelemetryModule { get; init; }
    public bool? EnableAzureInstanceMetadataTelemetryModule { get; init; }
    public bool? EnableDiagnosticsTelemetryModule { get; init; }

    // --- Properties still valid in 3.x (no action needed) ---
    public string? ConnectionString { get; init; }
    public double? SamplingRatio { get; init; }
    public double? TracesPerSecond { get; init; }
    public bool? EnableQuickPulseMetricStream { get; init; }

    // --- Removed extension methods ---
    public bool? UseApplicationInsights { get; init; }
    public bool? AddTelemetryProcessor { get; init; }
    public bool? ConfigureTelemetryModule { get; init; }

    /// <summary>true if the string overload e.g. AddApplicationInsightsTelemetry("ikey") is used — REMOVED in 3.x</summary>
    public bool? UsesInstrumentationKeyOverload { get; init; }
}

/// <summary>
/// Findings from analyzing ITelemetryInitializer implementations.
/// </summary>
public record InitializerFindings
{
    public bool Found { get; init; }
    public List<InitializerInfo> Implementations { get; init; } = [];
    public List<string> Registrations { get; init; } = [];
}

public record InitializerInfo
{
    public string ClassName { get; init; } = null!;
    public string? File { get; init; }
    public string? Purpose { get; init; }
}

/// <summary>
/// Findings from analyzing ITelemetryProcessor implementations.
/// </summary>
public record ProcessorFindings
{
    public bool Found { get; init; }
    public List<ProcessorInfo> Implementations { get; init; } = [];
    public List<string> Registrations { get; init; } = [];
}

public record ProcessorInfo
{
    public string ClassName { get; init; } = null!;
    public string? File { get; init; }
    public string? Purpose { get; init; }
}

/// <summary>
/// Findings from analyzing direct TelemetryClient usage.
/// </summary>
public record ClientUsageFindings
{
    public bool DirectUsage { get; init; }
    public List<ClientUsageInfo> Usages { get; init; } = [];
}

public record ClientUsageInfo
{
    public string File { get; init; } = null!;
    public string? Pattern { get; init; }
    public List<string> Methods { get; init; } = [];
}

/// <summary>
/// Findings from analyzing custom sampling configuration.
/// </summary>
public record SamplingFindings
{
    public bool HasCustomSampling { get; init; }
    public string? Type { get; init; }
    public string? Details { get; init; }
    public string? File { get; init; }
}

/// <summary>
/// Findings from analyzing custom ITelemetryChannel or TelemetrySink usage.
/// Both TelemetryChannel and TelemetrySinks properties are removed from TelemetryConfiguration in 3.x.
/// </summary>
public record TelemetryPipelineFindings
{
    public bool Found { get; init; }
    /// <summary>true if custom ITelemetryChannel or TelemetryConfiguration.TelemetryChannel assignment found</summary>
    public bool HasCustomChannel { get; init; }
    /// <summary>true if TelemetrySinks or DefaultTelemetrySink usage found</summary>
    public bool HasTelemetrySinks { get; init; }
    public string? ClassName { get; init; }
    public string? File { get; init; }
    public string? Details { get; init; }
}

/// <summary>
/// Findings from analyzing explicit Application Insights logger provider configuration.
/// In 3.x, ILogger output is exported to Application Insights automatically - explicit
/// AddApplicationInsights() calls and AddFilter&lt;ApplicationInsightsLoggerProvider&gt; must be removed.
/// </summary>
public record LoggingFindings
{
    /// <summary>true if any explicit Application Insights logger provider configuration exists</summary>
    public bool Found { get; init; }

    /// <summary>true if AddApplicationInsights() on ILoggingBuilder is found</summary>
    public bool HasExplicitLoggerProvider { get; init; }

    /// <summary>AddFilter&lt;ApplicationInsightsLoggerProvider&gt;(...) lines found</summary>
    public List<string> LogFilters { get; init; } = [];

    /// <summary>File containing the logging configuration</summary>
    public string? File { get; init; }
}
