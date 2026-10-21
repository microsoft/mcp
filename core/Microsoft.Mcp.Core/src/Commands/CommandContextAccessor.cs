// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Implements command execution context access with an instance-owned <see cref="AsyncLocal{T}"/>.
/// </summary>
/// <remarks>
/// Use a singleton registration so loaders, services, and cached pipelines share the same accessor.
/// Independent service providers retain independent ambient stores, and concurrent invocations
/// use separate asynchronous-flow-local values. Nested active scopes are rejected.
/// <para>
/// Each value is a shared holder rather than a direct context reference. Clearing the holder on
/// disposal also removes the context from execution contexts inherited by outliving child work.
/// A child flow whose inherited holder has been cleared may start a new independent scope.
/// </para>
/// </remarks>
public sealed class CommandContextAccessor : ICommandContextAccessor
{
    private readonly AsyncLocal<CommandContextHolder?> _current = new();

    /// <inheritdoc/>
    public CommandContext? CurrentContext => _current.Value?.Context;

    /// <inheritdoc/>
    public IDisposable BeginScope(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (CurrentContext is not null)
        {
            throw new InvalidOperationException("A command execution scope is already active in this asynchronous flow.");
        }

        var holder = new CommandContextHolder(context);
        _current.Value = holder;
        return new CommandContextScope(this, holder);
    }

    /// <summary>
    /// Invalidates the current scope's shared holder and removes it from this asynchronous flow.
    /// </summary>
    /// <param name="holder">The holder installed by <see cref="BeginScope"/> for this scope.</param>
    /// <remarks>
    /// Called by <see cref="CommandContextScope.Dispose"/> in the flow owning the scope.
    /// The shared holder is cleared before the local value is removed so inherited child
    /// flows cannot continue observing the completed invocation. No previous scope is restored.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="holder"/> is not the holder currently installed in this flow.
    /// </exception>
    internal void EndScope(CommandContextHolder holder)
    {
        if (!ReferenceEquals(_current.Value, holder))
        {
            throw new InvalidOperationException("The command execution scope is not the current scope in this asynchronous flow.");
        }

        // Clear the shared holder so inherited ExecutionContexts cannot retain a completed invocation.
        holder.Clear();
        _current.Value = null;
    }
}
