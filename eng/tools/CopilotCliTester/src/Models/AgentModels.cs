// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

/// <summary>
/// Configuration for running an agent session
/// </summary>
internal sealed class AgentRunConfig
{
    public required string Prompt { get; init; }
    public string? ToolName { get; init; }
    public string? Namespace { get; init; }
    public Func<AgentMetadata, bool>? ShouldEarlyTerminate { get; init; }
    public SystemPromptConfig? SystemPrompt { get; init; }
    public bool? Debug { get; init; }
    public string? Model { get; init; }
}

/// <summary>
/// Metadata collected during an agent session
/// </summary>
internal sealed class AgentMetadata
{
    public List<AgentSessionEvent> Events { get; } = [];
}

/// <summary>
/// System prompt configuration
/// </summary>
internal sealed class SystemPromptConfig
{
    public required SystemPromptMode Mode { get; init; }
    public required string Content { get; init; }
}

/// <summary>
/// System prompt mode
/// </summary>
internal enum SystemPromptMode
{
    Append,
    Replace
}

/// <summary>
/// A single event from the agent session
/// </summary>
internal sealed class AgentSessionEvent
{
    public required string Type { get; init; }
    public Dictionary<string, object?> Data { get; init; } = [];
}



