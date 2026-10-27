// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Microsoft.Mcp.Core.Commands;

public interface ICommandFactory
{
    /// <summary>
    /// The name of the <c>--learn</c> CLI option. Centralised here so callers can detect
    /// it in raw arg arrays without coupling to the concrete <see cref="CommandFactory"/> class.
    /// </summary>
    const string LearnOptionName = "--learn";

    RootCommand RootCommand { get; }
    CommandGroup RootGroup { get; }

    /// <summary>
    /// Gets registered routing names and their commands' original setup identities.
    /// </summary>
    /// <remarks>Consolidated routing names retain the original command registration.</remarks>
    IReadOnlyDictionary<string, CommandRegistration> AllCommands { get; }

    /// <summary>
    /// Gets registrations in the specified displayed command groups.
    /// </summary>
    /// <param name="groupNames">Group names to select, matched case-insensitively.</param>
    /// <returns>Routing names and registrations, preserving original tool namespaces.</returns>
    IReadOnlyDictionary<string, CommandRegistration> GroupCommands(string[] groupNames);

    /// <summary>
    /// Selects an executable command and its original tool namespace from one registered routing name.
    /// </summary>
    /// <param name="fullCommandName">The full registered command routing name.</param>
    /// <returns>The registration, or <see langword="null"/> when the command is not registered.</returns>
    CommandRegistration? FindCommandRegistration(string fullCommandName);

    /// <summary>
    /// Handles a <c>--learn</c> request by examining the raw CLI <paramref name="args"/> and
    /// returning a JSON <see cref="CommandResponse"/> that describes the available commands
    /// or the specific command's parameters.  Must be called BEFORE
    /// <see cref="ParseResult.InvokeAsync"/> to bypass required-option validation.
    /// </summary>
    /// <param name="args">The raw command-line arguments array received by the process.</param>
    /// <returns>A JSON string ready to write to stdout.</returns>
    string GetLearnResponse(string[] args);

    /// <summary>
    /// Finds the executable command given its full routing name (for example, storage_account_list).
    /// </summary>
    /// <param name="fullCommandName">Name of the command with prefixes.</param>
    /// <returns>The command, or <see langword="null"/> when the routing name is not registered.</returns>
    /// <remarks>
    /// Call <see cref="FindCommandRegistration"/> when execution also needs the original tool namespace.
    /// </remarks>
    IBaseCommand? FindCommandByName(string fullCommandName);

    /// <summary>
    /// Gets the service area given the full command name (i.e. 'storage_account_list' would return 'storage').
    /// </summary>
    /// <param name="fullCommandName">Name of the command.</param>
    /// <returns>The displayed setup-area name, or <see langword="null"/> when the routing name is unknown.</returns>
    /// <remarks>
    /// Consolidated factories return their synthetic area name here. Use
    /// <see cref="CommandRegistration.ToolNamespaceName"/> from the selected registration when the
    /// original namespace is required, especially for namespace-scoped endpoint validation.
    /// </remarks>
    string? GetServiceArea(string fullCommandName);
}
