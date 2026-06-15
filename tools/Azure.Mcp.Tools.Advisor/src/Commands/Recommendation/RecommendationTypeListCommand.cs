// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
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
    Description = "List the catalog of Azure Advisor recommendation types — every recommendation Advisor can possibly generate, " +
                  "with its category, impact, the resource type it targets, and sub-category. Results are sorted by impact " +
                  "(High → Medium → Low) so the most important recommendations surface first. " +
                  "Use this in two scenarios: " +
                  "(1) greenfield/empty environments where no actual recommendations have been generated yet — the catalog still " +
                  "shows what Advisor would recommend once resources exist; " +
                  "(2) brownfield environments where a new resource type is being onboarded into an existing subscription — pass " +
                  "--resource-type to see exactly which recommendations will apply to it. " +
                  "Optionally narrow results with --resource-type (e.g. 'microsoft.compute/virtualmachines'), " +
                  "--impact (High|Medium|Low), and/or --category (Cost|HighAvailability|Security|Performance|OperationalExcellence).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationTypeListCommand(
    ILogger<RecommendationTypeListCommand> logger,
    IAdvisorService advisorService)
    : GlobalCommand<RecommendationTypeListOptions>
{
    private static readonly string[] AllowedImpacts = ["High", "Medium", "Low"];

    private readonly ILogger<RecommendationTypeListCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(RecommendationTypeListOptionDefinitions.ResourceType);
        command.Options.Add(RecommendationTypeListOptionDefinitions.Impact);
        command.Options.Add(RecommendationTypeListOptionDefinitions.Category);

        command.Validators.Add(commandResult =>
        {
            if (!commandResult.TryGetValue(RecommendationTypeListOptionDefinitions.Impact, out string? value))
            {
                return;
            }

            var normalized = value?.Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                return;
            }

            if (!AllowedImpacts.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                commandResult.AddError(
                    $"Invalid --impact value '{value}'. Allowed values: {string.Join(", ", AllowedImpacts)}.");
            }
        });
    }

    protected override RecommendationTypeListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceType = parseResult.GetValueOrDefault<string>(RecommendationTypeListOptionDefinitions.ResourceType.Name);
        options.Impact = parseResult.GetValueOrDefault<string>(RecommendationTypeListOptionDefinitions.Impact.Name);
        options.Category = parseResult.GetValueOrDefault<string>(RecommendationTypeListOptionDefinitions.Category.Name);
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
                options.ResourceType,
                options.Impact,
                options.Category,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecommendationTypeListResult(recommendationTypes ?? []),
                AdvisorJsonContext.Default.RecommendationTypeListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendation types. Tenant: {Tenant}, ResourceType: {ResourceType}, Impact: {Impact}, Category: {Category}.",
                options.Tenant, options.ResourceType, options.Impact, options.Category);
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
