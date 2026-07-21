// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.CloudArchitect.Options;

namespace Azure.Mcp.Tools.CloudArchitect.Models;

/// <summary>
/// Response object for the cloud architect design command.
/// </summary>
public sealed record CloudArchitectResponseObject(
    string DisplayText,
    string DisplayThought,
    string DisplayHint,
    int QuestionNumber,
    int TotalQuestions,
    bool NextQuestionNeeded,
    ArchitectureDesignToolState State);

/// <summary>
/// Complete response for the cloud architect design command including both response object and design architecture text.
/// </summary>
public sealed record CloudArchitectDesignResponse(string DesignArchitecture, CloudArchitectResponseObject ResponseObject);
