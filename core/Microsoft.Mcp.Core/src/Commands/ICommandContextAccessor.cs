// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Provides the context of the command executing in the current asynchronous flow.
/// </summary>
/// <remarks>
/// Register one accessor per service provider with
/// <see cref="CommandContextServiceCollectionExtensions.AddCommandContextAccessor"/>.
/// Command loaders establish a scope only around actual in-process command execution;
/// discovery, learn responses, sampling callbacks, direct CLI execution, and external proxy
/// forwarding do not populate this accessor.
/// <para>
/// Services should read <see cref="CurrentContext"/> when they need invocation information,
/// rather than retaining a context in instance fields or cached clients. The accessor does
/// not depend on ASP.NET Core or MCP request-context accessors.
/// </para>
/// </remarks>
public interface ICommandContextAccessor
{
    /// <summary>
    /// Gets the active command context for the current asynchronous flow.
    /// </summary>
    /// <value>
    /// The same context instance passed to the executing command, or <see langword="null"/>
    /// outside an execution scope. An active context with an unresolved
    /// <see cref="CommandContext.ToolNamespaceName"/> is not equivalent to an absent context.
    /// </value>
    /// <remarks>
    /// The context flows across awaits and into child work that inherits the execution context.
    /// Scope disposal clears the ambient reference for inherited child work, even if that work
    /// outlives the command. Do not retain the context or its mutable response after execution.
    /// </remarks>
    CommandContext? CurrentContext { get; }

    /// <summary>
    /// Makes a command context available until the returned scope is disposed.
    /// </summary>
    /// <param name="context">
    /// The context passed to the selected command, with its original registered tool namespace
    /// populated by the loader when that namespace can be resolved.
    /// </param>
    /// <returns>
    /// A scope to dispose in the asynchronous flow that created it, after the awaited command
    /// execution finishes. Repeated disposal of a successfully disposed scope is harmless.
    /// </returns>
    /// <remarks>
    /// Call this method synchronously in the loader's executing flow, not inside an awaited
    /// initialization helper. Use a <c>using</c> block around the complete awaited command call
    /// so success, exceptions, and cancellation all clear the context. Only one scope can be
    /// active in a flow; services consuming the context should not open additional scopes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a command execution scope is already active in the current asynchronous flow.
    /// </exception>
    IDisposable BeginScope(CommandContext context);
}
