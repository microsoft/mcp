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
    Id = "0d8e07d4-63c2-4a96-b364-01d5db6e660b",
    Name = "get-pipeline",
    Title = "Get Pipeline",
    Description = "Gets details of a specific pipeline in a Microsoft Fabric workspace. Requires workspace ID and pipeline ID.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class GetPipelineCommand(
    ILogger<GetPipelineCommand> logger,
    PipelineHandler handler) : GlobalCommand<GetPipelineOptions>()
{
    private readonly ILogger<GetPipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PipelineHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.PipelineId.AsRequired());
    }

    protected override GetPipelineOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceIdName) ?? string.Empty;
        options.PipelineId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.PipelineIdName) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        var result = await _handler.GetAsync(options.WorkspaceId, options.PipelineId);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully retrieved pipeline {PipelineId} from workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);

            var commandResult = new GetPipelineCommandResult(result.Value!.Pipeline);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.GetPipelineCommandResult);
        }
        else
        {
            _logger.LogError("Error getting pipeline {PipelineId} from workspace {WorkspaceId}: {Error}",
                options.PipelineId, options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
