// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Interface for all commands
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public interface IBaseCommand
{
    /// <summary>
    /// A unique identifier for the command. Identifier must be a constant value representing a GUID.
    /// See <see cref="Guid.NewGuid()"/> to generate a random GUID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the name of the command
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the command
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the title of the command
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets metadata about an MCP tool describing its behavioral characteristics.
    /// This metadata helps MCP clients understand how the tool operates and its potential effects.
    /// </summary>
    ToolMetadata Metadata { get; }

    /// <summary>
    /// Gets the JSON type information describing the command's result type, or <see langword="null"/>
    /// when the command does not advertise a structured result. Used to generate the MCP
    /// <c>outputSchema</c> and to serialize <c>structuredContent</c>.
    /// </summary>
    JsonTypeInfo? ResultTypeInfo => null;

    /// <summary>
    /// Gets the command definition
    /// </summary>
    Command GetCommand();

    /// <summary>
    /// Executes the command
    /// </summary>
    Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken);

    ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null);
}
