// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using global::DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Fabric.Mcp.Tools.DataFactory.Options.Pipeline;
using global::DataFactory.MCP.Models.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands.Pipeline;

[CommandMetadata(
    Id = "a1b2c3d4-1001-4000-8000-000000000003",
    Name = "create-pipeline",
    Title = "Create Pipeline",
    Description = "Creates a new pipeline in a Microsoft Fabric workspace. Requires workspace ID and display name. Optionally provide a description.",
    Destructive = false,
    Idempotent = false,
    ReadOnly = false,
    OpenWorld = false)]
public sealed class CreatePipelineCommand(
    ILogger<CreatePipelineCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<CreatePipelineOptions>()
{
    private readonly ILogger<CreatePipelineCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

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
        try
        {
            var request = new CreatePipelineRequest
            {
                DisplayName = options.DisplayName,
                Description = options.Description
            };

            var response = await _pipelineService.CreatePipelineAsync(options.WorkspaceId, request);

            _logger.LogInformation("Successfully created pipeline '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);

            // Map CreatePipelineResponse to Pipeline for the result
            var pipeline = new global::DataFactory.MCP.Models.Pipeline.Pipeline
            {
                Id = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                Type = response.Type,
                WorkspaceId = response.WorkspaceId,
                FolderId = response.FolderId
            };

            var result = new CreatePipelineCommandResult(pipeline);
            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.CreatePipelineCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pipeline '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
