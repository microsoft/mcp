// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MonitorInstrumentation.Options;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Commands;

public sealed class GetLearningResourceCommand(ILogger<GetLearningResourceCommand> logger)
    : BaseCommand<GetLearningResourceOptions>
{
    private readonly ILogger<GetLearningResourceCommand> _logger = logger;

    public override string Id => "2c9f3785-4b97-4dd6-8489-af515638f0d5";

    public override string Name => "get_learning_resource";

    public override string Description =>
        "Get the content of a learning resource by path. Use list_learning_resources to see available paths. Note: For instrumenting an application, use orchestrator_start instead.";

    public override string Title => "Get Azure Monitor Learning Resource";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        command.Options.Add(MonitorInstrumentationOptionDefinitions.Path);
    }

    protected override GetLearningResourceOptions BindOptions(ParseResult parseResult)
    {
        return new GetLearningResourceOptions
        {
            Path = parseResult.CommandResult.GetValueOrDefault(MonitorInstrumentationOptionDefinitions.Path)
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
            var result = GetLearningResourceTool.GetLearningResource(options.Path!);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorInstrumentationJsonContext.Default.String);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Path: {Path}", Name, options.Path);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
