// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DataFactory.MCP.Abstractions.Interfaces;
using Fabric.Mcp.Tools.DataFactory.Models;
using Fabric.Mcp.Tools.DataFactory.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.DataFactory.Commands;

[CommandMetadata(
    Id = "c761e25e-0bb7-4a70-a920-dcb31daa0edf",
    Name = "get_pipeline",
    Title = "Get Pipeline",
    Description = "Gets the metadata of a specific data pipeline by its ID from a Microsoft Fabric workspace. Returns pipeline details including name, description, and type.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PipelineGetCommand(
    ILogger<PipelineGetCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<PipelineGetCommand.PipelineGetOptions>()
{
    private readonly ILogger<PipelineGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.PipelineId.AsRequired());
    }

    protected override PipelineGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.PipelineId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.PipelineId.Name) ?? string.Empty;
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

            _logger.LogInformation("Retrieved pipeline '{PipelineId}' from workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);

            var result = new PipelineGetCommandResult
            {
                Id = pipeline.Id,
                DisplayName = pipeline.DisplayName,
                Description = pipeline.Description,
                Type = pipeline.Type,
                WorkspaceId = pipeline.WorkspaceId
            };

            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.PipelineGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pipeline '{PipelineId}' from workspace {WorkspaceId}.",
                options.PipelineId, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class PipelineGetOptions : GlobalOptions
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
    }

    public sealed class PipelineGetCommandResult
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
    }
}
