// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ApplicationInsights.Options;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;

public sealed class RecommendationListCommand(ILogger<RecommendationListCommand> logger) : SubscriptionCommand<RecommendationListOptions>()
{
    private const string CommandTitle = "List Application Insights Recommendations";
    private readonly ILogger<RecommendationListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List Application Insights Code Optimization Recommendations in a subscription. Optionally filter by resource group when --resource-group is provided.
        Returns the code optimization recommendations based on the profiler data.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, LocalRequired = false, OpenWorld = false, Secret = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // New explicit option registration pattern: add resource group as optional per-command
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override RecommendationListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
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
