// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Associates an executable command with the namespace of its original setup registration.
/// </summary>
/// <param name="Command">The command instance selected for execution.</param>
/// <param name="ToolNamespaceName">
/// The original setup-area name, or <see langword="null"/> when that identity is unresolved.
/// </param>
/// <remarks>
/// Command factories finalize registrations when their setup areas register command groups.
/// Regrouping an existing registration preserves this same immutable value, even when its
/// exposed routing name changes. Namespace identity is not inferred from tool names or arguments.
/// Setup code should declare new commands through <see cref="CommandGroup.AddCommand(IBaseCommand)"/>
/// so the factory supplies its owning area's name. A directly constructed registration already
/// represents the original identity: a null namespace stays unresolved during regrouping.
/// </remarks>
public sealed record CommandRegistration(IBaseCommand Command, string? ToolNamespaceName)
{
    /// <summary>
    /// Gets whether the command was added before its owning setup area was known.
    /// </summary>
    /// <remarks>
    /// Only commands added through the command-instance overloads need binding. A published
    /// registration with a null namespace is unresolved, not awaiting a synthetic group's namespace.
    /// </remarks>
    internal bool NeedsNamespaceBinding { get; private init; }

    /// <summary>
    /// Creates a pending registration for a setup's command-group declaration.
    /// </summary>
    /// <param name="command">The command that will be owned by the registering setup.</param>
    /// <returns>A registration to finalize when the factory knows the owning setup area.</returns>
    internal static CommandRegistration CreateUnregistered(IBaseCommand command)
        => new(command, null) { NeedsNamespaceBinding = true };

    /// <summary>
    /// Supplies a setup namespace only for a command that has not yet been registered.
    /// </summary>
    /// <param name="namespaceName">The name of the setup registering the command group.</param>
    /// <returns>
    /// The finalized registration, with a null namespace when the setup name is empty or whitespace;
    /// or this instance if its original registration is already preserved.
    /// </returns>
    internal CommandRegistration BindNamespace(string namespaceName)
        => NeedsNamespaceBinding
            ? new(Command, string.IsNullOrWhiteSpace(namespaceName) ? null : namespaceName)
            : this;
}
