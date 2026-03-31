// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Serializable descriptor for a command group (tree node). Represents a hierarchical
/// grouping of commands that maps to a System.CommandLine <see cref="System.CommandLine.Command"/>.
/// </summary>
public sealed record CommandGroupDescriptor
{
    /// <summary>
    /// The group name used as the CLI verb (e.g. "storage", "keyvault", "server").
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable description shown in help text.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// User-friendly title for display purposes.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    /// <summary>
    /// Category for grouping in help output (e.g. "Azure Services", "CLI").
    /// Only meaningful on root-level area descriptors.
    /// </summary>
    [JsonPropertyName("category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Category { get; init; }

    /// <summary>
    /// Commands directly belonging to this group.
    /// </summary>
    [JsonPropertyName("commands")]
    public CommandDescriptor[] Commands { get; init; } = [];

    /// <summary>
    /// Nested command groups (e.g. "storage" → "account", "container").
    /// </summary>
    [JsonPropertyName("subGroups")]
    public CommandGroupDescriptor[] SubGroups { get; init; } = [];
}
