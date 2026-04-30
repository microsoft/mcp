// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    IReadOnlyDictionary<string, IBaseCommand> AllCommands { get; }

    IReadOnlyDictionary<string, IBaseCommand> GroupCommands(string[] groupNames);

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
    /// Finds the BaseCommand given its full command name (i.e. storage_account_list).
    /// </summary>
    /// <param name="fullCommandName">Name of the command with prefixes.</param>
    /// <returns></returns>
    IBaseCommand? FindCommandByName(string fullCommandName);

    /// <summary>
    /// Gets the service area given the full command name (i.e. 'storage_account_list' would return 'storage').
    /// </summary>
    /// <param name="fullCommandName">Name of the command.</param>
    string? GetServiceArea(string fullCommandName);
}
