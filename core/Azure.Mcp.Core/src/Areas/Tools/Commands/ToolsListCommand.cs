// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
public sealed class ToolsListCommand(ILogger<ToolsListCommand> logger) : BaseCommand()
{
    private const string CommandTitle = "List Available Tools";
    private static readonly Option<bool> NamespacesOption = new("--namespaces")
    {
        Description = "If specified, returns a list of top-level service namespaces instead of individual commands.",
    };

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
        command.Options.Add(NamespacesOption);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<CommandFactory>();

            // If the --namespaces flag set, return distinct top-level namespaces (area group names beneath root 'azmcp')
            var namespacesOnly = parseResult.CommandResult.HasOptionResult(NamespacesOption);
            if (namespacesOnly)
            {
                var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "extension", "server", "tools" };
                var rootGroup = factory.RootGroup; // azmcp

                // Build a map of visible commands once to reuse for option resolution
                var visible = CommandFactory.GetVisibleCommands(factory.AllCommands);

                var namespaceCommands = rootGroup.SubGroup
                    .Where(g => !ignored.Contains(g.Name))
                    .GroupBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(grp =>
                    {
                        var first = grp.First();

                        // Collect all descendant command token keys for this namespace (prefix match "<ns>.")
                        // Commands in the map are tokenized starting with root (e.g., azmcp_storage_account_get)
                        var nsPrefix = string.Concat(rootGroup.Name, CommandFactory.Separator, first.Name, CommandFactory.Separator);
                        var subcommandInfos = visible
                            .Where(kvp => kvp.Key.StartsWith(nsPrefix, StringComparison.OrdinalIgnoreCase))
                            .Select(kvp => CreateCommand(kvp.Key, kvp.Value))
                            .OrderBy(ci => ci.Command, StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        return new CommandInfo
                        {
                            Name = first.Name,
                            Description = first.Description,
                            Command = $"azmcp {first.Name}",
                            Subcommands = subcommandInfos,
                            Options = null,
                            SubcommandsCount = subcommandInfos.Count
                        };
                    })
                    .OrderBy(ci => ci.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                context.Response.Results = ResponseResult.Create(namespaceCommands, ModelsJsonContext.Default.ListCommandInfo);
                context.Response.ResultsCount = namespaceCommands.Count;
                return context.Response;
            }

            var tools = await Task.Run(() => CommandFactory.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => CreateCommand(kvp.Key, kvp.Value))
                .ToList());

            context.Response.Results = ResponseResult.Create(tools, ModelsJsonContext.Default.ListCommandInfo);
            context.Response.ResultsCount = tools.Count;
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred processing listing tools.");
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
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = tokenizedName.Replace(CommandFactory.Separator, ' '),
            Options = optionInfos,
            // Leaf commands have no subcommands.
            SubcommandsCount = 0,
        };
    }
}
