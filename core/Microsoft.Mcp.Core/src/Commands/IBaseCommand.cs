// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Interface for all commands
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
public interface IBaseCommand<TContext, TOptions, TResult>
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
    /// Gets the command definition
    /// </summary>
    Command GetCommand();

    /// <summary>
    /// Executes the command
    /// </summary>
    Task<TResult> ExecuteAsync(TContext context, TOptions options, CancellationToken cancellationToken);
}
