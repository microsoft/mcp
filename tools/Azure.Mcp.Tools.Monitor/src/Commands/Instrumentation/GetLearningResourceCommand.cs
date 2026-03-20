// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands;

public sealed class GetLearningResourceCommand(ILogger<GetLearningResourceCommand> logger)
    : BaseCommand<GetLearningResourceOptions>
{
    private readonly ILogger<GetLearningResourceCommand> _logger = logger;

    public override string Id => "2c9f3785-4b97-4dd6-8489-af515638f0d5";

    public override string Name => "get-learning-resource";

    public override string Description =>
        "List all available learning resources for Azure Monitor instrumentation or get the content of a specific resource by path. Returns all resource paths by default, or retrieves the full content when a path is specified. Note: For instrumenting an application, use orchestrator-start instead.";

    public override string Title => "Get Azure Monitor Learning Resource";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorInstrumentationOptionDefinitions.Path);
    }

    protected override GetLearningResourceOptions BindOptions(ParseResult parseResult)
    {
        return new GetLearningResourceOptions
        {
            Path = parseResult.CommandResult.GetValueOrDefault<string>(MonitorInstrumentationOptionDefinitions.Path.Name)
        };
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                // List all learning resources
                var resources = GetLearningResourceTool.ListLearningResources();

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new GetLearningResourceCommandResult(Resources: resources ?? [], Content: null),
                    MonitorInstrumentationJsonContext.Default.GetLearningResourceCommandResult);
            }
            else
            {
                // Get specific learning resource content
                var content = GetLearningResourceTool.GetLearningResource(options.Path);

                context.Response.Status = HttpStatusCode.OK;
                context.Response.Results = ResponseResult.Create(
                    new GetLearningResourceCommandResult(Resources: null, Content: content),
                    MonitorInstrumentationJsonContext.Default.GetLearningResourceCommandResult);
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

    internal record GetLearningResourceCommandResult(List<string>? Resources, string? Content);

}
