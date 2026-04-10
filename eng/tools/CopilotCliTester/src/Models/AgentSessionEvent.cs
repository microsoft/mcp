// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

/// <summary>
/// A single event from the agent session
/// </summary>
internal sealed class AgentSessionEvent
{
    public required string Type { get; init; }
    public Dictionary<string, object?> Data { get; init; } = [];
}