// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Owns cleanup of a single ambient command execution context.
/// </summary>
/// <param name="accessor">The accessor that installed the execution scope.</param>
/// <param name="holder">The holder installed for this invocation.</param>
/// <remarks>
/// Created by <see cref="CommandContextAccessor.BeginScope"/> and used around an awaited
/// command call. Dispose in the flow that created the scope; transferring ownership to
/// unrelated work or disposing concurrently is not supported.
/// </remarks>
internal sealed class CommandContextScope(
    CommandContextAccessor accessor,
    CommandContextHolder holder) : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Clears the invocation's ambient context and marks this scope as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated successful disposal is harmless and does not clear any later scope.
    /// If disposal fails because the holder is not current, the scope remains undisposed
    /// so its owning flow can still perform cleanup.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// This scope's holder is not current in the flow performing disposal.
    /// </exception>
    public void Dispose()
    {
        if (!_disposed)
        {
            accessor.EndScope(holder);
            _disposed = true;
        }
    }
}
