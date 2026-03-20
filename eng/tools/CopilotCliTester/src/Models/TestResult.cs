// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace CopilotCliTester.Models;

/// <summary>
/// Result of running a single test through Copilot SDK.
/// </summary>
internal sealed record TestResult {
    public required string Tool {get; init; }
    public required string Prompt {get; init; }
    public required double Duration {get; init; }
    public string[]? ToolsCalled {get; init; } = [];
    public int Attempts {get; init; } = 1;
    public required TestStatus Status {get; init; }
    public string? Error {get; init; } = null;
}

internal enum TestStatus {
    FAIL,
    PASS,
    ERROR
}