// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Result of listing all available function templates for a language,
/// grouped by binding type (triggers, input bindings, output bindings).
/// </summary>
public sealed class TemplateListResult
{
    public required string Language { get; init; }

    public IReadOnlyList<TemplateSummary> Triggers { get; init; } = [];

    public IReadOnlyList<TemplateSummary> InputBindings { get; init; } = [];

    public IReadOnlyList<TemplateSummary> OutputBindings { get; init; } = [];
}
