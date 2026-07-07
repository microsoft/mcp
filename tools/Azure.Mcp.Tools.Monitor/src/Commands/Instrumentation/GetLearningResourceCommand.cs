// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Options.Instrumentation;
using Azure.Mcp.Tools.Monitor.Tools.Instrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Instrumentation;

[CommandMetadata(
    Id = "2c9f3785-4b97-4dd6-8489-af515638f0d5",
    Name = "get-learning-resource",
    Title = "Get Azure Monitor Learning Resource",
    Description = "List all available learning resources for Azure Monitor instrumentation or get the content of a specific resource by path. Returns all resource paths by default, or retrieves the full content when a path is specified. Note: For instrumenting an application, use orchestrator-start instead.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class GetLearningResourceCommand(ILogger<GetLearningResourceCommand> logger)
    : BaseCommand<GetLearningResourceOptions, GetLearningResourceCommand.GetLearningResourceCommandResult>
{
    private readonly ILogger<GetLearningResourceCommand> _logger = logger;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, GetLearningResourceOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                // List all learning resources
                var resources = GetLearningResourceTool.ListLearningResources();

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(new(Resources: resources ?? [], Content: null),
                    MonitorJsonContext.Default.GetLearningResourceCommandResult);
            }
            else
            {
                // Get specific learning resource content
                var content = GetLearningResourceTool.GetLearningResource(options.Path);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(new(Resources: null, Content: content),
                    MonitorJsonContext.Default.GetLearningResourceCommandResult);
            }

            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Path: {Path}", Name, options.Path);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }

    public sealed record GetLearningResourceCommandResult(List<string>? Resources, string? Content);
}
