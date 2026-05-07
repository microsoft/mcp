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
    PipelineHandler handler) : GlobalCommand<CreatePipelineOptions>()
{
    private readonly ILogger<CreatePipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PipelineHandler _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.DisplayName.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.Description.AsOptional());
    }

    protected override CreatePipelineOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Successfully created pipeline '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);

            // Map CreatePipelineResponse to Pipeline for the result
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
