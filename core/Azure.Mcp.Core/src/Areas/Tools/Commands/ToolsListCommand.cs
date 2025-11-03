// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Azure.Mcp.Core.Areas.Tools.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
public sealed class ToolsListCommand(ILogger<ToolsListCommand> logger) : BaseCommand<ToolsListOptions>
{
    private const string CommandTitle = "List Available Tools";

    public override string Id => "63de05a7-047d-4f8a-86ea-cebd64527e2b";

    public override string Name => "list";

    public override string Description =>
        """
        List all available commands and their tools in a hierarchical structure. This command returns detailed information
        about each command, including its name, description, full command path, available subcommands, and all supported
        arguments. Use this to explore the CLI's functionality or to build interactive command interfaces.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ToolsListOptionDefinitions.Namespaces);
    }

    protected override ToolsListOptions BindOptions(ParseResult parseResult)
    {
        return new ToolsListOptions
        {
            Namespaces = parseResult.GetValueOrDefault(ToolsListOptionDefinitions.Namespaces)
        };
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<CommandFactory>();
            var options = BindOptions(parseResult);

            // If the --namespaces flag is set, return distinct top‑level namespaces (child groups beneath root 'azmcp').
            if (options.Namespaces)
            {
                var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "server", "tools" };
                var surfaced = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "extension" };
                var rootGroup = factory.RootGroup; // azmcp

                var namespaceCommands = rootGroup.SubGroup
                    .Where(g => !ignored.Contains(g.Name) && !surfaced.Contains(g.Name))
                    .Select(g => new CommandInfo
                    {
                        Name = g.Name,
                        Description = g.Description ?? string.Empty,
                        Command = $"{g.Name}",
                        // We deliberately omit populating Subcommands for the lightweight namespace view.
                    })
                    .OrderBy(ci => ci.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Add the commands to be surfaced directly to the list.
                // For commands in the surfaced list, each command is exposed as a separate tool in the namespace mode.
                foreach (var name in surfaced)
                {
                    var subgroup = rootGroup.SubGroup.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (subgroup is not null)
                    {
                        List<CommandInfo> foundCommands = [];
                        searchCommandInCommandGroup("", subgroup, foundCommands);
                        namespaceCommands.AddRange(foundCommands);
                    }
                }

                context.Response.Results = ResponseResult.Create(namespaceCommands, ModelsJsonContext.Default.ListCommandInfo);
                return context.Response;
            }

            var tools = await Task.Run(() => CommandFactory.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => CreateCommand(kvp.Key, kvp.Value))
                .ToList());

            context.Response.Results = ResponseResult.Create(tools, ModelsJsonContext.Default.ListCommandInfo);
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while processing tool listing.");
            HandleException(context, ex);

            return context.Response;
        }
    }

    private static CommandInfo CreateCommand(string tokenizedName, IBaseCommand command)
    {
        var commandDetails = command.GetCommand();

        var optionInfos = commandDetails.Options?
            .Select(arg => new OptionInfo(
                name: arg.Name,
                description: arg.Description!,
                required: arg.Required))
            .ToList();

        return new CommandInfo
        {
            Id = command.Id,
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = tokenizedName.Replace(CommandFactory.Separator, ' '),
            Options = optionInfos,
            Metadata = command.Metadata
        };
    }

    private void searchCommandInCommandGroup(string commandPrefix, CommandGroup searchedGroup, List<CommandInfo> foundCommands)
    {
        var commands = CommandFactory.GetVisibleCommands(searchedGroup.Commands).Select(kvp =>
        {
            var command = kvp.Value.GetCommand();
            return new CommandInfo
            {
                Name = $"{commandPrefix.Replace(" ", "_")}{searchedGroup.Name}_{command.Name}",
                Description = command.Description ?? string.Empty,
                Command = $"{(!string.IsNullOrEmpty(commandPrefix) ? commandPrefix : "")}{searchedGroup.Name} {command.Name}"
                // Omit Options and Subcommands for surfaced commands as well.
            };
        });
        foundCommands.AddRange(commands);
        foreach (CommandGroup nextLevelSubGroup in searchedGroup.SubGroup)
        {
            searchCommandInCommandGroup($"{commandPrefix}{searchedGroup.Name} ", nextLevelSubGroup, foundCommands);
        }
    }
}
