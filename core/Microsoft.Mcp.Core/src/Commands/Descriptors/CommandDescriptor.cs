// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands.Descriptors;

/// <summary>
/// Serializable descriptor for a single command. Contains all metadata needed to
/// register the command with System.CommandLine without constructing the handler.
/// </summary>
public sealed record CommandDescriptor
{
    /// <summary>
    /// Unique identifier for the command (GUID string).
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The command verb (e.g. "list", "get", "create").
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
    public required string Title { get; init; }

    /// <summary>
    /// MCP tool metadata describing behavioral characteristics.
    /// </summary>
    [JsonPropertyName("metadata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolMetadata? Metadata { get; init; }

    /// <summary>
    /// Whether this command is hidden from help output.
    /// </summary>
    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Hidden { get; init; }

    /// <summary>
    /// Tool annotations describing behavioral characteristics (destructive, read-only, etc.).
    /// </summary>
    [JsonPropertyName("annotations")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ToolAnnotations? Annotations { get; init; }

    /// <summary>
    /// Options specific to this command (does not include inherited options from <see cref="InheritOptions"/>).
    /// </summary>
    [JsonPropertyName("options")]
    public OptionDescriptor[] Options { get; init; } = [];

    /// <summary>
    /// Assembly-qualified type name of the <see cref="ICommandHandler"/> implementation.
    /// Used for deferred resolution from DI at execution time.
    /// </summary>
    [JsonPropertyName("handlerType")]
    public required string HandlerType { get; init; }

    /// <summary>
    /// Determines which inherited options are automatically added by the framework.
    /// </summary>
    [JsonPropertyName("inheritOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public InheritOptions InheritOptions { get; init; } = InheritOptions.Basic;
}
