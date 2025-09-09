// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

public interface ICommandFactory
{
    public char Separator { get; }

    public CommandGroup RootGroup { get; }

    public IReadOnlyDictionary<string, IBaseCommand> AllCommands { get; }

    public IReadOnlyDictionary<string, IBaseCommand> GroupCommands(string[] groupNames);

    public string? GetServiceArea(string commandName);
}
