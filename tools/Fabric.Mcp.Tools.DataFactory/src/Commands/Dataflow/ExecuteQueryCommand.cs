// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options.Dataflow;
using global::DataFactory.MCP.Handlers.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Dataflow;

[CommandMetadata(
    Id = "b4e2a1f9-6c3d-4a8e-9f12-d5c7b8e3a041",
    Name = "execute-query",
    Title = "Execute Dataflow Query",
    Description = "Executes an M (Power Query) expression against a dataflow in a Microsoft Fabric workspace.",
    Destructive = false,
    Idempotent = true,
    ReadOnly = true,
    OpenWorld = false)]
public sealed class ExecuteQueryCommand(
    ILogger<ExecuteQueryCommand> logger,
    DataflowQueryHandler handler) : AuthenticatedCommand<ExecuteQueryOptions, ExecuteQueryCommandResult>
{
    private readonly ILogger<ExecuteQueryCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly DataflowQueryHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ExecuteQueryOptions options, CancellationToken cancellationToken)
    {
        var result = await _handler.ExecuteQueryAsync(
            options.WorkspaceId,
            options.DataflowId,
            options.QueryName,
            options.Query);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully executed query '{QueryName}' on dataflow {DataflowId}",
                options.QueryName, options.DataflowId);

            var commandResult = new ExecuteQueryCommandResult(
                result.Value!.Success,
                result.Value.Data,
                result.Value.Summary);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.ExecuteQueryCommandResult);
        }
        else
        {
            _logger.LogError("Error executing query '{QueryName}' on dataflow {DataflowId}: {Error}",
                options.QueryName, options.DataflowId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
