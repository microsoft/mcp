// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "0d3bd2d4-9b6e-4d7f-9f3a-2a1c8e1d4a02",
    Name = "list",
    Title = "List Advisor Recommendation Types",
    Description = "List the catalog of Azure Advisor recommendation types, categories, and impact levels available in the tenant. " +
                  "Use this for new or empty environments that do not yet have actual recommendations generated, or when you need the " +
                  "canonical identifiers and display names of Advisor metadata dimensions. Optionally pass --filter to narrow to a single " +
                  "dimension ('recommendationType', 'category', 'impact', or 'resourceType').",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationTypeListCommand(
    ILogger<RecommendationTypeListCommand> logger,
    IAdvisorService advisorService)
    : SubscriptionCommand<RecommendationTypeListOptions>
{
    private readonly ILogger<RecommendationTypeListCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(RecommendationTypeListOptionDefinitions.Filter);
    }

    protected override RecommendationTypeListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Filter = parseResult.GetValueOrDefault<string>(RecommendationTypeListOptionDefinitions.Filter.Name);
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
            var recommendationTypes = await _advisorService.ListRecommendationTypesAsync(
                options.Tenant,
                options.Filter,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecommendationTypeListResult(recommendationTypes ?? []),
                AdvisorJsonContext.Default.RecommendationTypeListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendation types. Tenant: {Tenant}, Filter: {Filter}.",
                options.Tenant, options.Filter);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the Advisor metadata endpoint. Verify the signed-in identity has Reader access to the tenant. Details: {httpEx.Message}",
        HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
            "Advisor metadata endpoint returned 404. Verify the cloud environment supports Microsoft.Advisor metadata.",
        HttpRequestException httpEx => $"Failed calling the Advisor metadata endpoint ({(int?)httpEx.StatusCode}). Details: {httpEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        HttpRequestException httpEx when httpEx.StatusCode.HasValue => httpEx.StatusCode.Value,
        _ => base.GetStatusCode(ex)
    };

    internal record RecommendationTypeListResult(List<Models.RecommendationType> RecommendationTypes);
}
