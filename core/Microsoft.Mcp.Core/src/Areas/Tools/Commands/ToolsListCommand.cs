// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
public sealed class ToolsListCommand(ILogger<ToolsListCommand> logger) : BaseCommand()
{
    private const string CommandTitle = "List Available Tools";

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<ICommandFactory>();
            var tools = await Task.Run(() => CommandHelper.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => CreateCommand(factory.Separator, kvp.Key, kvp.Value))
                .ToList());

            context.Response.Results = ResponseResult.Create(tools, ModelsJsonContext.Default.ListCommandInfo);
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred processing tool.");
            HandleException(context, ex);

            return context.Response;
        }
    }

    private static CommandInfo CreateCommand(char separator, string tokenizedName, IBaseCommand command)
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
            Command = tokenizedName.Replace(separator, ' '),
            Options = optionInfos,
        };
    }
}
