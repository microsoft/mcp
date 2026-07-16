// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CloudArchitect.Options;

namespace Azure.Mcp.Tools.CloudArchitect.Models;

/// <summary>
/// Response object for the cloud architect design command.
/// </summary>
public class CloudArchitectResponseObject
{
    public string DisplayText { get; set; } = string.Empty;

    public string DisplayThought { get; set; } = string.Empty;

    public string DisplayHint { get; set; } = string.Empty;

    public int QuestionNumber { get; set; }

    public int TotalQuestions { get; set; }

    public bool NextQuestionNeeded { get; set; }

    public ArchitectureDesignToolState State { get; set; } = new();
}

/// <summary>
/// Complete response for the cloud architect design command including both response object and design architecture text.
/// </summary>
public class DesignCommandResult
{
    public string DesignArchitecture { get; set; } = string.Empty;

    public CloudArchitectResponseObject ResponseObject { get; set; } = new();
}
