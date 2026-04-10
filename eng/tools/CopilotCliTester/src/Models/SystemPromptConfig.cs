// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

/// <summary>
/// System prompt configuration
/// </summary>
internal sealed class SystemPromptConfig
{
    public required SystemPromptMode Mode { get; init; }
    public required string Content { get; init; }
}