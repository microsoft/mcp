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
    Id = "ce83c7e5-da1a-4a97-bfdd-8366981ebf75",
    Name = "list_pipelines",
    Title = "List Pipelines",
    Description = "Lists all data pipelines in a specified Microsoft Fabric workspace. Returns pipeline names, IDs, and metadata. Use this to discover available pipelines before running or inspecting them.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PipelineListCommand(
    ILogger<PipelineListCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<PipelineListCommand.PipelineListOptions>()
{
    private readonly ILogger<PipelineListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.ContinuationToken.AsOptional());
    }

    protected override PipelineListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.WorkspaceId.Name) ?? string.Empty;
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(DataFactoryOptionDefinitions.ContinuationToken.Name);
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
            var response = await _pipelineService.ListPipelinesAsync(
                options.WorkspaceId,
                options.ContinuationToken);

            _logger.LogInformation("Retrieved {Count} pipelines from workspace {WorkspaceId}",
                response.Value.Count, options.WorkspaceId);

            var result = new PipelineListCommandResult
            {
                Pipelines = response.Value.Select(p => new PipelineInfo
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Type = p.Type,
                    WorkspaceId = p.WorkspaceId
                }).ToList(),
                ContinuationToken = response.ContinuationToken,
                TotalCount = response.Value.Count
            };

            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.PipelineListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing pipelines in workspace {WorkspaceId}.", options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class PipelineListOptions : GlobalOptions
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string? ContinuationToken { get; set; }
    }

    public sealed class PipelineListCommandResult
    {
        public List<PipelineInfo> Pipelines { get; set; } = [];
        public string? ContinuationToken { get; set; }
        public int TotalCount { get; set; }
    }

    public sealed class PipelineInfo
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
    }
}
