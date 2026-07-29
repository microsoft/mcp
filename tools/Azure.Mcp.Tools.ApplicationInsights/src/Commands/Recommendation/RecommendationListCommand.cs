// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.ApplicationInsights.Options;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;

[CommandMetadata(
    Id = "8d259f21-43b3-4962-bec8-de616b8b5f0d",
    Name = "list",
    Title = "List Application Insights Recommendations",
    Description = """
        List Application Insights Code Optimization Recommendations in a subscription. Optionally filter by resource group when --resource-group is provided.
        Returns the code optimization recommendations based on the profiler data.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger, IApplicationInsightsService applicationInsightsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationListOptions, RecommendationListCommand.RecommendationListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<RecommendationListCommand> _logger = logger;
    private readonly IApplicationInsightsService _applicationInsightsService = applicationInsightsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecommendationListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var insights = await _applicationInsightsService.GetProfilerInsightsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = insights?.Count() > 0 ?
                ResponseResult.Create(new(insights), ApplicationInsightsJsonContext.Default.RecommendationListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Application Insights components for recommendations.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record RecommendationListCommandResult(IEnumerable<JsonNode> Recommendations);
}
