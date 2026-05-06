// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using global::DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Fabric.Mcp.Tools.DataFactory.Options.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;

[CommandMetadata(
    Id = "a1b2c3d4-2001-4000-8000-000000000001",
    Name = "list-dataflows",
    Title = "List Dataflows",
    Description = "Lists all dataflows in a specified Microsoft Fabric workspace.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class ListDataflowsCommand(
    ILogger<ListDataflowsCommand> logger,
    IFabricDataflowService dataflowService) : GlobalCommand<ListDataflowsOptions>()
{
    private readonly ILogger<ListDataflowsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricDataflowService _dataflowService = dataflowService ?? throw new ArgumentNullException(nameof(dataflowService));

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

        try
        {
            var response = await _dataflowService.ListDataflowsAsync(options.WorkspaceId);

            _logger.LogInformation("Successfully listed {Count} dataflows in workspace {WorkspaceId}",
                response.Value.Count, options.WorkspaceId);

            var commandResult = new ListDataflowsCommandResult(response.Value, response.Value.Count);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.ListDataflowsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing dataflows in workspace {WorkspaceId}", options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
