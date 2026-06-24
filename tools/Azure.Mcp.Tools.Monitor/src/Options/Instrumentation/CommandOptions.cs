// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Instrumentation;

public sealed class GetLearningResourceOptions
{
    [Option(Description = "Learning resource path.")]
    public string? Path { get; set; }
}

public sealed class OrchestratorStartOptions
{
    [Option(Description = "Absolute path to the workspace folder.")]
    public required string WorkspacePath { get; set; }
}

public sealed class OrchestratorNextOptions
{
    [Option(Description = MonitorOptionDefinitions.SessionIdDescription)]
    public required string SessionId { get; set; }

    [Option(Description = "One sentence describing what you executed, e.g., 'Ran dotnet add package command' or 'Added UseAzureMonitor() to Program.cs'")]
    public required string CompletionNote { get; set; }
}

public sealed class SendBrownfieldAnalysisOptions
{
    [Option(Description = MonitorOptionDefinitions.SessionIdDescription)]
    public required string SessionId { get; set; }

    [Option(Description = """
        JSON object with brownfield analysis findings. Required properties:
        - serviceOptions: Service options findings from analyzing AddApplicationInsightsTelemetry() call. Null if not found.
        - initializers: Telemetry initializer findings from analyzing ITelemetryInitializer or IConfigureOptions<TelemetryConfiguration> implementations. Null if none found.
        - processors: Telemetry processor findings from analyzing ITelemetryProcessor implementations. Null if none found.
        - clientUsage: TelemetryClient usage findings from analyzing direct TelemetryClient usage. Null if not found.
        - sampling: Custom sampling configuration findings. Null if no custom sampling.
        - telemetryPipeline: Custom ITelemetryChannel or TelemetrySinks usage findings. Null if not found.
        - logging: Explicit logger provider and filter findings. Null if not found.
        For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null.
        """)]
    public required string FindingsJson { get; set; }
}

public sealed class SendEnhancementSelectOptions
{
    [Option(Description = MonitorOptionDefinitions.SessionIdDescription)]
    public required string SessionId { get; set; }

    [Option(Description = "One or more enhancement keys, comma-separated (e.g. 'redis', 'redis,processors', 'entityframework,otlp').")]
    public required string EnhancementKeys { get; set; }
}
