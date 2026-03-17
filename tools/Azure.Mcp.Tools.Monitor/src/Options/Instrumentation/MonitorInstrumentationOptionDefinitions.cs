// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Options;

public static class MonitorInstrumentationOptionDefinitions
{
    public const string WorkspacePathName = "workspace-path";
    public static readonly Option<string> WorkspacePath = new($"--{WorkspacePathName}")
    {
        Description = "Absolute path to the workspace folder.",
        Required = true
    };

    public const string PathName = "path";
    public static readonly Option<string> Path = new($"--{PathName}")
    {
        Description = "Learning resource path.",
        Required = true
    };

    public const string SessionIdName = "session-id";
    public static readonly Option<string> SessionId = new($"--{SessionIdName}")
    {
        Description = "The workspace path returned as sessionId from orchestrator_start.",
        Required = true
    };

    public const string CompletionNoteName = "completion-note";
    public static readonly Option<string> CompletionNote = new($"--{CompletionNoteName}")
    {
        Description = "One sentence describing what you executed, e.g., 'Ran dotnet add package command' or 'Added UseAzureMonitor() to Program.cs'",
        Required = true
    };

    public const string FindingsJsonName = "findings-json";
    public static readonly Option<string> FindingsJson = new($"--{FindingsJsonName}")
    {
        Description = """
            JSON object with brownfield analysis findings. Required properties:
            - serviceOptions: Service options findings from analyzing AddApplicationInsightsTelemetry() call. Null if not found.
            - initializers: Telemetry initializer findings from analyzing ITelemetryInitializer implementations. Null if none found.
            - processors: Telemetry processor findings from analyzing ITelemetryProcessor implementations. Null if none found.
            - clientUsage: TelemetryClient usage findings from analyzing direct TelemetryClient usage. Null if not found.
            - sampling: Custom sampling configuration findings. Null if no custom sampling.
            - telemetryPipeline: Custom ITelemetryChannel or TelemetrySinks usage findings. Null if not found.
            For sections that do not exist in the codebase, pass an empty/default object (e.g. found: false, hasCustomSampling: false) rather than null.
            """,
        Required = true
    };
}
