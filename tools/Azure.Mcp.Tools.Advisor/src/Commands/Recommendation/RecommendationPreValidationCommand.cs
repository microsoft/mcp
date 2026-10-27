// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Advisor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "e08c278c-9b5c-43d5-a63a-3a0df53cb2e2",
    Name = "prevalidate",
    Title = "Prevalidate Idle Resource Recommendations",
    Description = "Check whether affected resources for one selected Azure Advisor Cost/IdleResources recommendation type pass read-only pre-deletion validation. " +
        "Use for explicit safety, dependency, lock, recent-activity, or ready-to-delete checks, not ordinary recommendation listing or savings analysis. " +
        "Uses current New recommendations and the onboarded ResourceInsights definitions to recheck detection, minimum unused age (30 days by default), " +
        "management locks, resource-specific risks, and visible dependencies. Choose one or more subscriptions or a single Service Group. " +
        "Pass exact affected-resource IDs when validating selected rows. Returns per-resource safe, needsReview, or blocked verdicts, evidence, " +
        "manual checks, Retail USD savings, unmatched selections, and a truncation indicator. " +
        "Unknown or incomplete gating evidence requires review. Passing automated checks does not authorize deletion. " +
        "Never deletes resources, executes scripts, or changes recommendation state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationPreValidationCommand(
    ILogger<RecommendationPreValidationCommand> logger,
    IAdvisorService advisorService,
    IPreValidationService preValidationService,
    ISubscriptionResolver subscriptionResolver)
    : RecommendationScopeCommand<RecommendationPreValidationOptions, PreValidationResult>(subscriptionResolver)
{
    internal const int MaximumResources = 25;

    public override void ValidateOptions(RecommendationPreValidationOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (!Guid.TryParse(options.RecommendationTypeId, out _))
        {
            validationResult.Errors.Add("--recommendation-type-id must be a GUID.");
        }

        if (options.ServiceGroup is not null && options.ResourceGroup is not null)
        {
            validationResult.Errors.Add("--resource-group cannot be combined with --service-group.");
        }
        if (options.Top is < 1 or > MaximumResources)
        {
            validationResult.Errors.Add("--top must be between 1 and 25.");
        }
        if (options.MinimumUnusedDays is < 1 or > UnusedAgePolicy.MaximumMinimumUnusedDays)
        {
            validationResult.Errors.Add("--minimum-unused-days must be between 1 and 365.");
        }
        if (options.Resources?.Any(static resource => string.IsNullOrWhiteSpace(resource) ||
            resource.Trim().TrimEnd('/').Length == 0 || resource.Length > 2048) == true)
        {
            validationResult.Errors.Add("--resources must contain non-empty resource IDs or names of at most 2048 characters.");
        }
        if (NormalizeSelections(options.Resources).Length > (options.Top ?? MaximumResources))
        {
            validationResult.Errors.Add("All selected resources must fit within --top and the 25-resource limit.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, RecommendationPreValidationOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var selections = NormalizeSelections(options.Resources);
            var typeId = Guid.Parse(options.RecommendationTypeId).ToString("D");
            var filters = new RecommendationFilters(
                Category: "Cost", SubCategory: "IdleResources", Status: RecommendationStatus.New,
                RecommendationTypeId: typeId, Resource: options.Resource?.Trim(),
                ServiceGroup: options.ServiceGroup?.Trim(), Resources: selections);
            var top = options.Top ?? MaximumResources;
            var result = options.Subscriptions is { Length: > 0 }
                ? await advisorService.ListRecommendationsAsync(
                    RecommendationScopeValidator.GetSubscriptions(options), options.ResourceGroup, filters,
                    selections.Length == 0 ? top : 100, options.Tenant, cancellationToken)
                : await advisorService.ListRecommendationsAsync(
                    options.Subscription, options.ResourceGroup, filters,
                    selections.Length == 0 ? top : 100, options.Tenant, cancellationToken);

            var candidates = result.Results ?? [];
            foreach (var selection in selections.Where(static selection => !selection.StartsWith('/')))
            {
                var matches = candidates.Where(candidate => Matches(candidate, selection))
                    .Select(static candidate => candidate.Properties.ResourceMetadata?.ResourceId)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count();
                if (matches > 1 || result.AreResultsTruncated)
                {
                    throw new ArgumentException("Selected resource names could not be resolved uniquely. Use full ARM resource IDs.");
                }
            }

            var unmatched = selections.Where(selection => !candidates.Any(candidate => Matches(candidate, selection))).ToArray();
            var selected = candidates
                .Where(candidate => selections.Length == 0 || selections.Any(selection => Matches(candidate, selection)))
                .GroupBy(static candidate => candidate.Properties.ResourceMetadata?.ResourceId ?? candidate.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var validatedCandidates = selected.Take(top).SelectMany(static group => group).ToArray();
            var validation = await preValidationService.ValidateAsync(
                typeId, validatedCandidates, options.Tenant,
                result.AreResultsTruncated || selected.Length > top, unmatched,
                options.MinimumUnusedDays ?? UnusedAgePolicy.DefaultMinimumUnusedDays, cancellationToken);
            context.Response.Results = ResponseResult.Create(validation, AdvisorJsonContext.Default.PreValidationResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error prevalidating Advisor recommendations. RecommendationTypeId: {RecommendationTypeId}.",
                options.RecommendationTypeId);
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static string[] NormalizeSelections(string[]? resources) =>
        resources?.Where(static resource => !string.IsNullOrWhiteSpace(resource))
            .Select(static resource => resource.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];

    private static bool Matches(Models.Recommendation candidate, string selection) =>
        string.Equals(candidate.Properties.ResourceMetadata?.ResourceId?.TrimEnd('/'), selection, StringComparison.OrdinalIgnoreCase) ||
        (!selection.StartsWith('/') && string.Equals(candidate.Properties.ImpactedValue, selection, StringComparison.OrdinalIgnoreCase));

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { Status: 403 } => "Prevalidation could not read the selected Azure scope. Verify read permissions and tenant context.",
        RequestFailedException => "Azure could not complete prevalidation. Retry the read or narrow the selected scope.",
        ArgumentException => ex.Message,
        _ => "Prevalidation could not complete its read-only checks. Verify the selected scope, tenant, and read permissions before retrying.",
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        // The base error payload includes the raw backend exception even when GetErrorMessage is sanitized.
        context.Response.Results = null;
    }
}
