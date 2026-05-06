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
    Id = "f68fdd4e-49e5-4a75-a794-a169fcf60d95",
    Name = "run_pipeline",
    Title = "Run Pipeline",
    Description = "Runs a data pipeline on demand in a Microsoft Fabric workspace. Returns a tracking URL for monitoring the pipeline execution status. Use this to trigger pipeline runs programmatically.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PipelineRunCommand(
    ILogger<PipelineRunCommand> logger,
    IFabricPipelineService pipelineService) : GlobalCommand<PipelineRunCommand.PipelineRunOptions>()
{
    private readonly ILogger<PipelineRunCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPipelineService _pipelineService = pipelineService ?? throw new ArgumentNullException(nameof(pipelineService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(DataFactoryOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(DataFactoryOptionDefinitions.PipelineId.AsRequired());
    }

    protected override PipelineRunOptions BindOptions(ParseResult parseResult)
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
            var locationUrl = await _pipelineService.RunPipelineAsync(options.WorkspaceId, options.PipelineId);

            _logger.LogInformation("Pipeline '{PipelineId}' run initiated in workspace {WorkspaceId}",
                options.PipelineId, options.WorkspaceId);

            var result = new PipelineRunCommandResult
            {
                PipelineId = options.PipelineId,
                WorkspaceId = options.WorkspaceId,
                LocationUrl = locationUrl,
                Message = "Pipeline run initiated successfully."
            };

            context.Response.Results = ResponseResult.Create(result, DataFactoryJsonContext.Default.PipelineRunCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running pipeline '{PipelineId}' in workspace {WorkspaceId}.",
                options.PipelineId, options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class PipelineRunOptions : GlobalOptions
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string PipelineId { get; set; } = string.Empty;
    }

    public sealed class PipelineRunCommandResult
    {
        public string PipelineId { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
        public string? LocationUrl { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
