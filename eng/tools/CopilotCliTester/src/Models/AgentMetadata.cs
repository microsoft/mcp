// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

/// <summary>
/// Metadata collected during an agent session
/// </summary>
internal sealed class AgentMetadata
{
    public List<AgentSessionEvent> Events { get; } = [];
}
