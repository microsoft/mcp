// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MonitorInstrumentation.Options;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Commands;

public sealed class ListLearningResourcesCommand(ILogger<ListLearningResourcesCommand> logger)
    : BaseCommand<ListLearningResourcesOptions>
{
    private readonly ILogger<ListLearningResourcesCommand> _logger = logger;

    public override string Id => "6e9fa72c-89d9-4f72-a8dc-ef2cc6499f7f";

    public override string Name => "list_learning_resources";

    public override string Description =>
        "List all available learning resources for Azure Monitor instrumentation. Note: For instrumenting an application, use orchestrator_start instead.";

    public override string Title => "List Azure Monitor Learning Resources";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = true,
        Secret = false
    };

    protected override ListLearningResourcesOptions BindOptions(ParseResult parseResult) => new();

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        try
        {
            var result = ListLearningResourcesTool.ListLearningResources();

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, MonitorInstrumentationJsonContext.Default.String);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}", Name);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
