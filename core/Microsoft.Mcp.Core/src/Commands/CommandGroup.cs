// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

public class CommandGroup(string name, string description, string? title = null)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string? Title { get; } = title;
    public List<CommandGroup> SubGroup { get; } = [];
    public Dictionary<string, IBaseCommand> Commands { get; } = [];
    public Command Command { get; } = new Command(name, description);
    public ToolMetadata? ToolMetadata { get; set; }

    /// <summary>
    /// Adds a command to this group.
    /// This calls 'AddCommand(string path, IBaseCommand command)' with the command's name as the path.
    /// </summary>
    /// <param name="command">The command to add to this group.</param>
    public void AddCommand(IBaseCommand command) => AddCommand(command.Name, command);

    /// <summary>
    /// Adds a command to this group at the specified path, performing a recursive search for the correct subgroup if
    /// the path contains dots.
    /// <para>
    /// For example, if the path is "subgroup1.subgroup2.command", this method will first look for a subgroup named
    /// "subgroup1", then look for a subgroup named "subgroup2" within "subgroup1", and finally add the command to
    /// "subgroup2".
    /// </para>
    /// <para>
    /// Prefer using the overload that takes an IBaseCommand directly when possible, as it is simpler and less
    /// error-prone. Use this overload when you need to specify a path that is different from the command's name or
    /// when you want to add a command to a subgroup.
    /// </para>
    /// </summary>
    /// <param name="path">The command path.</param>
    /// <param name="command">The command to add to this group.</param>
    /// <exception cref="InvalidOperationException">If any subgroups specified by the path don't exist.</exception>
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
        Command.Subcommands.Add(subGroup.Command);
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
