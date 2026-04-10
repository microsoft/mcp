// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents a single file in a project template, containing
/// the file path/name and its text content.
/// </summary>
public sealed class ProjectTemplateFile
{
    public required string FileName { get; init; }

    public required string Content { get; init; }
}
