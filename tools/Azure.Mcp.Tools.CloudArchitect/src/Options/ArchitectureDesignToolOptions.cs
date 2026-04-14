// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.CloudArchitect.Options;

/// <summary>
/// The set of parameters that the architecture design tool takes as input.
/// </summary>
public class ArchitectureDesignToolOptions : GlobalOptions
{
    public string Question { get; set; } = string.Empty;

    public int QuestionNumber { get; set; }

    public int TotalQuestions { get; set; }

    public string? Answer { get; set; }

    public bool NextQuestionNeeded { get; set; }

    public double? ConfidenceScore { get; set; }

    public string? ArchitectureComponent { get; set; }

    public ArchitectureTier? ArchitectureTier { get; set; }

    public ArchitectureDesignToolState State { get; set; } = new();
}
