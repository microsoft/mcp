// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.Mcp.Tools.ApplicationInsights.Options;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Command;
using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;

public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger) : SubscriptionCommand<RecommendationListOptions>()
{
    private const string CommandTitle = "List Application Insights Recommendations";
    private readonly ILogger<RecommendationListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List Application Insights components (as recommendation targets) in a subscription. Optionally filter by resource group when --resource-group is provided.
        Returns component name, id, location, appId and instrumentation key to support telemetry optimization workflows.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup();
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IApplicationInsightsService>();
            var insights = await service.GetProfilerInsightsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = insights?.Count() > 0 ?
                ResponseResult.Create(new RecommendationListCommandResult(insights), ApplicationInsightsJsonContext.Default.RecommendationListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Application Insights components for recommendations.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record RecommendationListCommandResult(IEnumerable<JsonNode> Recommendations);
}
