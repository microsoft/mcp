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
    Id = "faf493a8-0c7d-4347-854a-a85120dd8199",
    Name = "create-pipeline",
    Title = "Create Pipeline",
    Description = "Creates a new pipeline in a Microsoft Fabric workspace. Requires workspace ID and display name. Optionally provide a description.",
    Destructive = false,
    Idempotent = false,
    ReadOnly = false,
    OpenWorld = false)]
public sealed class CreatePipelineCommand(
    ILogger<CreatePipelineCommand> logger,
    PipelineHandler handler) : AuthenticatedCommand<CreatePipelineOptions, CreatePipelineCommandResult>
{
    private readonly ILogger<CreatePipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PipelineHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreatePipelineOptions options, CancellationToken cancellationToken)
    {
        var result = await _handler.CreateAsync(options.WorkspaceId, options.DisplayName, options.Description);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created pipeline '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);

            var response = result.Value!.Pipeline;
            var pipeline = new global::DataFactory.MCP.Models.Pipeline.Pipeline
            {
                Id = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                Type = response.Type,
                WorkspaceId = response.WorkspaceId,
                FolderId = response.FolderId
            };

            var commandResult = new CreatePipelineCommandResult(pipeline);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.CreatePipelineCommandResult);
        }
        else
        {
            _logger.LogError("Error creating pipeline '{DisplayName}' in workspace {WorkspaceId}: {Error}",
                options.DisplayName, options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
