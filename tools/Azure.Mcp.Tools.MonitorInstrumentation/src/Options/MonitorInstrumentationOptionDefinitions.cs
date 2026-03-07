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
        Description = "The session ID returned from orchestrator_start.",
        Required = true
    };

    public const string CompletionNoteName = "completion-note";
    public static readonly Option<string> CompletionNote = new($"--{CompletionNoteName}")
    {
        Description = "One-sentence note about what action was completed.",
        Required = true
    };

    public const string FindingsJsonName = "findings-json";
    public static readonly Option<string> FindingsJson = new($"--{FindingsJsonName}")
    {
        Description = "Brownfield findings JSON payload matching the analysis template.",
        Required = true
    };
}
