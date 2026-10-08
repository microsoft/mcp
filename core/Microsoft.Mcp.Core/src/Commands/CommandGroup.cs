// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Mcp.Core.Commands;

public class CommandGroup(string name, string description, string? title = null)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string? Title { get; } = title;
    public List<CommandGroup> SubGroup { get; } = [];
    /// <summary>
    /// Gets command registrations keyed by their paths within this group.
    /// </summary>
    /// <remarks>
    /// Use <see cref="AddCommand(IBaseCommand)"/> for new setup commands and
    /// <see cref="AddCommand(string, CommandRegistration)"/> to regroup existing registrations.
    /// The command factory supplies the setup namespace before publishing pending registrations.
    /// </remarks>
    public Dictionary<string, CommandRegistration> Commands { get; } = [];
    public Command Command { get; } = new Command(name, description);
    public ToolMetadata? ToolMetadata { get; set; }

    /// <summary>
    /// Adds a command to this group by resolving it from the provided service provider.
    /// </summary>
    /// <typeparam name="TCommand">The command type to add.</typeparam>
    /// <param name="serviceProvider">The service provider that resolves the command.</param>
    public void AddCommand<TCommand>(IServiceProvider serviceProvider) where TCommand : IBaseCommand
        => AddCommand(serviceProvider.GetRequiredService<TCommand>());

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
        => AddCommand(path, CommandRegistration.CreateUnregistered(command));

    /// <summary>
    /// Adds an existing registration at a group-relative path without changing its original namespace.
    /// </summary>
    /// <param name="path">The command path, using dots to address existing subgroups.</param>
    /// <param name="registration">The original registration to preserve during regrouping.</param>
    /// <remarks>
    /// Used by consolidated loading to change presentation and routing without assigning a
    /// synthetic namespace to the command. An unresolved original namespace remains unresolved.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A subgroup named in <paramref name="path"/> does not exist.</exception>
    public void AddCommand(string path, CommandRegistration registration)
    {
        // Split on first dot to get group and remaining path
        var parts = path.Split(['.'], 2);

        if (parts.Length == 1)
        {
            // This is a direct command for this group
            Commands[path] = registration;
        }
        else
        {
            // Find or create the subgroup
            var subGroup = SubGroup.FirstOrDefault(g => g.Name == parts[0]) ??
                throw new InvalidOperationException($"Subgroup {parts[0]} not found. Group must be registered before commands.");

            // Recursively add command to subgroup
            subGroup.AddCommand(parts[1], registration);
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
            return Commands[parts[0]].Command;
        }
        else
        {
            // Find the subgroup and recursively get the command
            var subGroup = SubGroup.FirstOrDefault(g => g.Name == parts[0]) ??
                throw new InvalidOperationException($"Subgroup {parts[0]} not found.");

            return subGroup.GetCommand(parts[1]);
        }
    }

    /// <summary>
    /// Checks if all tools in this group and its subgroups match the given predicate.
    /// </summary>
    /// <param name="predicate">A predicate to test each tool's metadata.</param>
    /// <returns>Whether all tools in the group match the predicate.</returns>
    public bool AllToolsInGroupMatch(Predicate<ToolMetadata> predicate)
    {
        foreach (var command in Commands)
        {
            if (!predicate(command.Value.Command.Metadata))
            {
                return false;
            }
        }

        foreach (var subGroup in SubGroup)
        {
            if (!subGroup.AllToolsInGroupMatch(predicate))
            {
                return false;
            }
        }

        return true;
    }
}
