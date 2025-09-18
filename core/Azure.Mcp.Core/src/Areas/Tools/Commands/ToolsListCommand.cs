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
                // Exclude internal or special namespaces. 'extension' is flattened as top-level leaf commands.
                var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "server", "tools" };
                var rootGroup = factory.RootGroup; // azmcp

                // Build lookup for namespace descriptions from the existing registered groups
                var namespaceDescriptionMap = rootGroup.SubGroup.ToDictionary(commandGroup => commandGroup.Name, g => g.Description, StringComparer.OrdinalIgnoreCase);

                // Single pass over all visible commands to bucket by namespace
                var namespaceBuckets = new Dictionary<string, List<CommandInfo>>(StringComparer.OrdinalIgnoreCase);
                var extensionLeafCommands = new List<CommandInfo>();

                foreach (var kvp in CommandFactory.GetVisibleCommands(factory.AllCommands))
                {
                    var key = kvp.Key; // Tokenized e.g. azmcp_storage_account_get
                    var firstSeparatorIndex = key.IndexOf(CommandFactory.Separator); // Expect at least root + namespace + verb

                    if (firstSeparatorIndex < 0) continue; // Malformed, skip

                    var secondSeparatorIndex = key.IndexOf(CommandFactory.Separator, firstSeparatorIndex + 1);

                    if (secondSeparatorIndex < 0) continue; // Not enough tokens

                    var namespaceToken = key.Substring(firstSeparatorIndex + 1, secondSeparatorIndex - firstSeparatorIndex - 1);

                    if (ignored.Contains(namespaceToken))
                    {
                        // Skip internal groups entirely
                        continue;
                    }

                    var cmdInfo = CreateCommand(key, kvp.Value);

                    if (namespaceToken.Equals("extension", StringComparison.OrdinalIgnoreCase))
                    {
                        // Flatten: treat each extension command as top-level leaf
                        extensionLeafCommands.Add(cmdInfo);

                        continue;
                    }

                    if (!namespaceBuckets.TryGetValue(namespaceToken, out var list))
                    {
                        list = new List<CommandInfo>();
                        namespaceBuckets[namespaceToken] = list;
                    }

                    list.Add(cmdInfo);
                }

                // Build namespace CommandInfo objects
                var namespaceCommands = namespaceBuckets
                    .Select(kvp =>
                    {
                        var namespaceName = kvp.Key;
                        var subcommands = kvp.Value
                            .OrderBy(ci => ci.Command, StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        namespaceDescriptionMap.TryGetValue(namespaceName, out var desc);
                        return new CommandInfo
                        {
                            Name = namespaceName,
                            Description = desc ?? string.Empty,
                            Command = $"azmcp {namespaceName}",
                            Subcommands = subcommands,
                            Options = null,
                            SubcommandsCount = subcommands.Count
                        };
                    })
                    .OrderBy(ci => ci.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Order extension leaf commands
                extensionLeafCommands = extensionLeafCommands
                    .OrderBy(ci => ci.Command, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Combine and sort: namespaces first, then extension leaves by Name
                namespaceCommands.AddRange(extensionLeafCommands);
                namespaceCommands = namespaceCommands
                    .OrderByDescending(ci => ci.SubcommandsCount > 0)
                    .ThenBy(ci => ci.Name, StringComparer.OrdinalIgnoreCase)
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
            Name = commandDetails.Name,
            Description = commandDetails.Description ?? string.Empty,
            Command = tokenizedName.Replace(CommandFactory.Separator, ' '),
            Options = optionInfos,
            // Leaf commands have no subcommands.
            SubcommandsCount = 0,
        };
    }
}
