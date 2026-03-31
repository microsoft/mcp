// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Serializable descriptor for a command option. Contains everything needed to
/// create a <see cref="System.CommandLine.Option{T}"/> without instantiating a command handler.
/// </summary>
public sealed record OptionDescriptor
{
    /// <summary>
    /// The option name without the leading "--" prefix (e.g. "language", "subscription").
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable description shown in help text.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// CLR type name in lowercase (e.g. "string", "int32", "boolean").
    /// </summary>
    [JsonPropertyName("typeName")]
    public required string TypeName { get; init; }

    /// <summary>
    /// Whether this option is required for the command to execute.
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Required { get; init; }

    /// <summary>
    /// JSON-serialized default value, or null if no default.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Whether this option is hidden from help text.
    /// </summary>
    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Hidden { get; init; }
}
