// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using global::DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Fabric.Mcp.Tools.DataFactory.Options.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;

[CommandMetadata(
    Id = "a1b2c3d4-1001-4000-8000-000000000004",
    Name = "get-pipeline",
    Title = "Get Pipeline",
    Description = "Gets details of a specific pipeline in a Microsoft Fabric workspace. Requires workspace ID and pipeline ID.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class GetPipelineCommand(
    ILogger<GetPipelineCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<GetPipelineOptions>()
{
    private readonly ILogger<GetPipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

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
        try
        {
            var pipeline = await _pipelineService.GetPipelineAsync(options.WorkspaceId, options.PipelineId);

            _logger.LogInformation("Successfully retrieved pipeline {PipelineId} from workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);

            var result = new GetPipelineCommandResult(pipeline);
            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.GetPipelineCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline {PipelineId} from workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
