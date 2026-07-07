// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options.Pipeline;
using global::DataFactory.MCP.Handlers.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;

[CommandMetadata(
    Id = "be699abf-2297-4328-be8c-e93f045236f2",
    Name = "run-pipeline",
    Title = "Run Pipeline",
    Description = "Triggers a run of a specified pipeline in a Microsoft Fabric workspace. Requires workspace ID and pipeline ID. Returns the run instance ID.",
    Destructive = false,
    Idempotent = false,
    ReadOnly = false,
    OpenWorld = false)]
public sealed class RunPipelineCommand(
    ILogger<RunPipelineCommand> logger,
    PipelineHandler handler) : AuthenticatedCommand<RunPipelineOptions, RunPipelineCommandResult>
{
    private readonly ILogger<RunPipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PipelineHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RunPipelineOptions options, CancellationToken cancellationToken)
    {
        var result = await _handler.RunAsync(options.WorkspaceId, options.PipelineId);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully triggered pipeline {PipelineId} in workspace {WorkspaceId}, RunId: {RunId}",
                options.PipelineId, options.WorkspaceId, result.Value!.JobInstanceId);

            var commandResult = new RunPipelineCommandResult(result.Value.JobInstanceId);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.RunPipelineCommandResult);
        }
        else
        {
            _logger.LogError("Error running pipeline {PipelineId} in workspace {WorkspaceId}: {Error}",
                options.PipelineId, options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
