// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the result of the get project template command,
/// containing setup instructions and project structure overview.
/// </summary>
public sealed class ProjectTemplateResult
{
    public required string Language { get; init; }

    public required string InitInstructions { get; init; }

    public required IReadOnlyList<string> ProjectStructure { get; init; }
}
