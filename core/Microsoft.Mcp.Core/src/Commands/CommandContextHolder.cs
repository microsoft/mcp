// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Holds a command context that can be invalidated across inherited execution contexts.
/// </summary>
/// <param name="context">The context owned by one command execution scope.</param>
/// <remarks>
/// <see cref="AsyncLocal{T}"/> values flow by reference into child execution contexts.
/// Those flows therefore share this holder, allowing scope disposal to clear their ambient
/// context as well as the originating flow's context. A holder is not reused for later scopes.
/// </remarks>
internal sealed class CommandContextHolder(CommandContext context)
{
    private CommandContext? _context = context;

    /// <summary>
    /// Gets the scope's context with visibility of clearing performed in another flow.
    /// </summary>
    /// <value>The active context, or <see langword="null"/> after <see cref="Clear"/>.</value>
    internal CommandContext? Context => Volatile.Read(ref _context);

    /// <summary>
    /// Atomically clears the context for every flow sharing this holder.
    /// </summary>
    /// <remarks>
    /// Called when the owning scope ends. Repeated calls are harmless, but clearing does not
    /// invalidate context references that consumers retained separately from the accessor.
    /// </remarks>
    internal void Clear() => Interlocked.Exchange(ref _context, null);
}
