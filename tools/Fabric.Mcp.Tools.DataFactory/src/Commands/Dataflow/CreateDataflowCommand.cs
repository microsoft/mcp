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
    Id = "98c74b3b-1813-40db-9abe-30ad90beb236",
    Name = "create-dataflow",
    Title = "Create Dataflow",
    Description = "Creates a new dataflow in a specified Microsoft Fabric workspace.",
    Destructive = false,
    Idempotent = false,
    ReadOnly = false,
    OpenWorld = false)]
public sealed class CreateDataflowCommand(
    ILogger<CreateDataflowCommand> logger,
    DataflowHandler handler) : GlobalCommand<CreateDataflowOptions>()
{
    private readonly ILogger<CreateDataflowCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly DataflowHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.DisplayName.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.Description.AsOptional());
    }

    protected override CreateDataflowOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceIdName) ?? string.Empty;
        options.DisplayName = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.DisplayNameName) ?? string.Empty;
        options.Description = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.DescriptionName);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        var result = await _handler.CreateAsync(options.WorkspaceId, options.DisplayName, options.Description);
        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully created dataflow '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);

            // Map CreateDataflowResponse to Dataflow for the result
            var response = result.Value!.Dataflow;
            var dataflow = new global::DataFactory.MCP.Models.Dataflow.Dataflow
            {
                Id = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                Type = response.Type,
                WorkspaceId = response.WorkspaceId,
                FolderId = response.FolderId
            };

            var commandResult = new CreateDataflowCommandResult(dataflow);
            context.Response.Results = ResponseResult.Create(commandResult, DataFactoryJsonContext.Default.CreateDataflowCommandResult);
        }
        else
        {
            _logger.LogError("Error creating dataflow '{DisplayName}' in workspace {WorkspaceId}: {Error}",
                options.DisplayName, options.WorkspaceId, result.Error);
            HandleException(context, new Exception(result.Error));
        }

        return context.Response;
    }
}
