// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using DataFactory.MCP.Models.Pipeline;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands;

[CommandMetadata(
    Id = "12da19c8-48e2-46de-8a0e-055080744315",
    Name = "create_pipeline",
    Title = "Create Pipeline",
    Description = "Creates a new data pipeline in a specified Microsoft Fabric workspace. Requires a workspace ID, display name, and optional description. Use this to set up new data orchestration workflows.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PipelineCreateCommand(
    ILogger<PipelineCreateCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<PipelineCreateCommand.PipelineCreateOptions>()
{
    private readonly ILogger<PipelineCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.DisplayName.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.Description.AsOptional());
        command.Options.Add(DataFactoryOptionDefinitions.FolderId.AsOptional());
    }

    protected override PipelineCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.DisplayName = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.DisplayName.Name) ?? string.Empty;
        options.Description = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.Description.Name);
        options.FolderId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.FolderId.Name);
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
                Description = options.Description,
                FolderId = options.FolderId
            };

            var response = await _pipelineService.CreatePipelineAsync(options.WorkspaceId, request);

            _logger.LogInformation("Successfully created pipeline '{DisplayName}' in workspace {WorkspaceId}",
                options.DisplayName, options.WorkspaceId);

            var result = new PipelineCreateCommandResult
            {
                PipelineId = response.Id,
                DisplayName = response.DisplayName,
                Description = response.Description,
                WorkspaceId = response.WorkspaceId
            };

            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.PipelineCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pipeline '{DisplayName}' in workspace {WorkspaceId}.",
                options.DisplayName, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class PipelineCreateOptions : GlobalOptions
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FolderId { get; set; }
    }

    public sealed class PipelineCreateCommandResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string WorkspaceId { get; set; } = string.Empty;
    }
}
