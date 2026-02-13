// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Commands;

public interface ICommandFactory
{

    RootCommand RootCommand { get; }
    CommandGroup RootGroup { get; }

    IReadOnlyDictionary<string, IBaseCommand> AllCommands { get; }

    IReadOnlyDictionary<string, IBaseCommand> GroupCommands(string[] groupNames);

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
