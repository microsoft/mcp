// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

public sealed class GetLearningResourceOptions
{
    public string? Path { get; set; }
}

public sealed class OrchestratorStartOptions
{
    public string? WorkspacePath { get; set; }
}

public sealed class OrchestratorNextOptions
{
    public string? SessionId { get; set; }

    public string? CompletionNote { get; set; }
}

public sealed class SendBrownfieldAnalysisOptions
{
    public string? SessionId { get; set; }

    public string? FindingsJson { get; set; }
}

public sealed class SendEnhancementSelectOptions
{
    public string? SessionId { get; set; }

    public string? EnhancementKeys { get; set; }
}
