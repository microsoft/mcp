// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.UnitTests.Server.Helpers;

public class MockCommandFactory : ICommandFactory
{
    private readonly Dictionary<string, IBaseCommand> _allCommands;

    public const string Group1 = "Group1";
    public const string Group2 = "Group2";
    public const string Group3 = "Group3";

    public MockCommandFactory()
    {
        Command1 = new MockCommand("A");
        Command2 = new MockCommand("B");
        ReadonlyCommand = new MockCommand("C", isReadonly: true);
        HiddenCommand = new MockHiddenCommand("D");

        _allCommands = new Dictionary<string, IBaseCommand>
            {
                { Command1.Name, Command1 },
                { Command2.Name, Command2 },
                { ReadonlyCommand.Name, ReadonlyCommand },
                { HiddenCommand.Name, HiddenCommand },
            };

        MockGroupedCommands = new Dictionary<string, IBaseCommand>
            {
                { Group1, new MockCommand(Group1) },
                { Group2, new MockCommand(Group2) },
                { Group3, new MockHiddenCommand(Group3) },
            };
    }

    public MockCommand Command1 { get; }

    public MockCommand Command2 { get; }

    public MockCommand ReadonlyCommand { get; }

    public MockCommand HiddenCommand { get; }

    public Dictionary<string, IBaseCommand> MockGroupedCommands { get; }

    public void AddCommand(string name, IBaseCommand command)
    {
        _allCommands.Add(name, command);
    }

    public char Separator => '+';

    public CommandGroup RootGroup { get; } = new CommandGroup("Test", "Test Group");

    public IReadOnlyDictionary<string, IBaseCommand> AllCommands => _allCommands;

    public IReadOnlyDictionary<string, IBaseCommand> GroupCommands(string[] groupNames)
    {
        return MockGroupedCommands;
    }

    public string? GetServiceArea(string commandName)
    {
        return "";
    }
}
