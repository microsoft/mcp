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
