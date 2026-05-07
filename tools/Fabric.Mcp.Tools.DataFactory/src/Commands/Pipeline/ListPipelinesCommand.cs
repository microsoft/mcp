// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Fabric.Mcp.Tools.DataFactory.Options.Pipeline;
using global::DataFactory.MCP.Handlers.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;

[CommandMetadata(
    Id = "365d2efc-ce25-4a32-afb1-294daf6e462b",
    Name = "list-pipelines",
    Title = "List Pipelines",
    Description = "Lists all pipelines in a specified Microsoft Fabric workspace. Requires the workspace ID.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class ListPipelinesCommand(
    ILogger<ListPipelinesCommand> logger,
    PipelineHandler handler) : GlobalCommand<ListPipelinesOptions>()
{
    private readonly ILogger<ListPipelinesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PipelineHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
    }

    protected override ListPipelinesOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Successfully listed {Count} pipelines in workspace {WorkspaceId}",
                result.Value!.PipelineCount, options.WorkspaceId);

            var commandResult = new ListPipelinesCommandResult(result.Value.Pipelines.ToList(), result.Value.PipelineCount);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.ListPipelinesCommandResult);
        }
        else
        {
            _logger.LogError("Error listing pipelines in workspace {WorkspaceId}: {Error}", options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
