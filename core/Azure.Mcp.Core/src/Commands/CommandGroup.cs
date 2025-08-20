// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Commands;

public class CommandGroup(string name, string description)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public List<CommandGroup> SubGroup { get; } = [];
    public Dictionary<string, IBaseCommand> Commands { get; } = [];
    public Command Command { get; } = new Command(name, description);

    public void AddCommand(string path, IBaseCommand command)
    {
        // Split on first dot to get group and remaining path
        var parts = path.Split(['.'], 2);

        if (parts.Length == 1)
        {
            // This is a direct command for this group
            Commands[path] = command;
        }
        else
        {
            // Find or create the subgroup
            var subGroup = SubGroup.FirstOrDefault(g => g.Name == parts[0]) ??
                throw new InvalidOperationException($"Subgroup {parts[0]} not found. Group must be registered before commands.");

            // Recursively add command to subgroup
            subGroup.AddCommand(parts[1], command);
        }
    }

    public void AddSubGroup(CommandGroup subGroup)
    {
        SubGroup.Add(subGroup);
        Command.Add(subGroup.Command);
    }

    public IBaseCommand GetCommand(string path)
    {
        // Split on first dot to get group and remaining path
        var parts = path.Split(['.'], 2);

        if (parts.Length == 1)
        {
            // This is a direct command for this group
            return Commands[parts[0]];
        }
        else
        {
            // Find the subgroup and recursively get the command
            var subGroup = SubGroup.FirstOrDefault(g => g.Name == parts[0]) ??
                throw new InvalidOperationException($"Subgroup {parts[0]} not found.");

            return subGroup.GetCommand(parts[1]);
        }
    }
}
