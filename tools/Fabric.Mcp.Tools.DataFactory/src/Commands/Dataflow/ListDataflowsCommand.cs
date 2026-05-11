// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Fabric.Mcp.Tools.DataFactory.Options.Dataflow;
using global::DataFactory.MCP.Handlers.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;

[CommandMetadata(
    Id = "07025647-7bd3-4e80-82eb-13bbd603d076",
    Name = "list-dataflows",
    Title = "List Dataflows",
    Description = "Lists all dataflows in a specified Microsoft Fabric workspace.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class ListDataflowsCommand(
    ILogger<ListDataflowsCommand> logger,
    DataflowHandler handler) : GlobalCommand<ListDataflowsOptions>()
{
    private readonly ILogger<ListDataflowsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly DataflowHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
    }

    protected override ListDataflowsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceIdName) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        var result = await _handler.ListAsync(options.WorkspaceId);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully listed {Count} dataflows in workspace {WorkspaceId}",
                result.Value!.DataflowCount, options.WorkspaceId);

            var commandResult = new ListDataflowsCommandResult(result.Value.Dataflows.ToList(), result.Value.DataflowCount);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.ListDataflowsCommandResult);
        }
        else
        {
            _logger.LogError("Error listing dataflows in workspace {WorkspaceId}: {Error}", options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
