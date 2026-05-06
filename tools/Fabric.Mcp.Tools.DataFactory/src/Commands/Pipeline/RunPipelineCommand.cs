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
    Id = "a1b2c3d4-1001-4000-8000-000000000005",
    Name = "run-pipeline",
    Title = "Run Pipeline",
    Description = "Triggers a run of a specified pipeline in a Microsoft Fabric workspace. Requires workspace ID and pipeline ID. Returns the run instance ID.",
    Destructive = false,
    Idempotent = false,
    ReadOnly = false,
    OpenWorld = false)]
public sealed class RunPipelineCommand(
    ILogger<RunPipelineCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<RunPipelineOptions>()
{
    private readonly ILogger<RunPipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.PipelineId.AsRequired());
    }

    protected override RunPipelineOptions BindOptions(ParseResult parseResult)
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
            var runId = await _pipelineService.RunPipelineAsync(options.WorkspaceId, options.PipelineId);

            _logger.LogInformation("Successfully triggered pipeline {PipelineId} in workspace {WorkspaceId}, RunId: {RunId}",
                options.PipelineId, options.WorkspaceId, runId);

            var result = new RunPipelineCommandResult(runId);
            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.RunPipelineCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline {PipelineId} in workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
