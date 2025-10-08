// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Tools.Options;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Microsoft.Extensions.Logging;
using System.CommandLine.Parsing;

namespace Azure.Mcp.Core.Areas.Tools.Commands;

[HiddenCommand]
public sealed class ToolsListNamesCommand(ILogger<ToolsListNamesCommand> logger) : BaseCommand<ToolsListNamesOptions>
{
    private const string CommandTitle = "List Tool Names";

    public override string Name => "list-names";

    public override string Description =>
        """
        List all available tool names in the Azure MCP server. This command returns a simple list of tool names
        without descriptions or metadata, useful for quick discovery or automated tool enumeration.
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
        command.Options.Add(ToolsListOptionDefinitions.Namespace);
    }

    protected override ToolsListNamesOptions BindOptions(ParseResult parseResult)
    {
        var options = new ToolsListNamesOptions();
        options.Namespace = parseResult.GetValueOrDefault<string>(ToolsListOptionDefinitions.Namespace.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var factory = context.GetService<CommandFactory>();
            var options = BindOptions(parseResult);

            // Get all visible commands and extract their tokenized names (full command paths)
            var allToolNames = CommandFactory.GetVisibleCommands(factory.AllCommands)
                .Select(kvp => kvp.Key) // Use the tokenized key instead of just the command name
                .Where(name => !string.IsNullOrEmpty(name));

            // Apply namespace filtering if specified
            if (!string.IsNullOrEmpty(options.Namespace))
            {
                var namespacePrefix = $"azmcp_{options.Namespace}_";
                allToolNames = allToolNames.Where(name => name.StartsWith(namespacePrefix, StringComparison.OrdinalIgnoreCase));
            }

            var toolNames = await Task.Run(() => allToolNames
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList());

            var result = new ToolNamesResult(toolNames);
            context.Response.Results = ResponseResult.Create(result, ModelsJsonContext.Default.ToolNamesResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while processing tool names listing.");
            HandleException(context, ex);

            return context.Response;
        }
    }

    public record ToolNamesResult(List<string> Names);
}