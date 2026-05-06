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
    /// Gets the <see cref="JsonTypeInfo"/> describing the strongly-typed result payload this command writes
    /// to <see cref="CommandResponse.Results"/>. When non-<see langword="null"/>, the tool loader uses it to
    /// generate the MCP <c>outputSchema</c> and to populate <c>structuredContent</c> on successful tool calls.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/> for commands that don't write a structured payload to
    /// <see cref="CommandResponse.Results"/> — typically text-only / passthrough commands that only
    /// populate <see cref="CommandResponse.Message"/>. Commands with a structured result should derive from
    /// <see cref="BaseCommand{TOptions, TResult}"/>, which overrides this with the source-generated
    /// <see cref="JsonTypeInfo{T}"/> for the declared <c>TResult</c>.
    /// </remarks>
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
