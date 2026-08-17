// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Advisor.Commands.Recommendation;

[CommandMetadata(
    Id = "567ca9d4-f802-4dd8-bfa3-2034e3c208ec",
    Name = "patch",
    Title = "Patch Advisor Recommendation State",
    Description = "Update the status of one Azure Advisor recommendation in a subscription. Mark an Advisor recommendation as completed. " +
        "Dismiss an Advisor recommendation because the risk is acceptable by using the RiskIsAcceptable dismissal reason, or choose another dismissal reason. " +
        "Postpone an Advisor recommendation until the requested future calendar date, such as December 31, 2026. " +
        "Reactivate a postponed or dismissed Advisor recommendation by setting it to New. " +
        "Requires the recommendation stable ID, also called recommendation ID, and returns the updated recommendation.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationPatchCommand(
    ILogger<RecommendationPatchCommand> logger,
    IAdvisorService advisorService,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RecommendationPatchOptions, RecommendationPatchCommand.RecommendationPatchResult>(subscriptionResolver)
{
    private readonly ILogger<RecommendationPatchCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    public override void ValidateOptions(RecommendationPatchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.RecommendationId))
        {
            validationResult.Errors.Add("--recommendation-id is required and cannot be empty.");
        }

        if (options.RecommendationStatus == RecommendationStatus.Postponed)
        {
            if (options.PostponedUntilDateTime is null)
            {
                validationResult.Errors.Add("--postponed-until-date-time is required when --recommendation-status is Postponed.");
            }
            else if (options.PostponedUntilDateTime <= DateTimeOffset.UtcNow)
            {
                validationResult.Errors.Add("--postponed-until-date-time must be in the future.");
            }
        }

        if (options.RecommendationStatus == RecommendationStatus.Dismissed &&
            options.RecommendationDismissReason is null)
        {
            validationResult.Errors.Add("--recommendation-dismiss-reason is required when --recommendation-status is Dismissed.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationPatchOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var recommendation = await _advisorService.PatchRecommendationAsync(
                options.Subscription!,
                options.RecommendationId.Trim(),
                options.RecommendationStatus,
                options.PostponedUntilDateTime,
                options.RecommendationDismissReason,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(recommendation),
                AdvisorJsonContext.Default.RecommendationPatchResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error patching Advisor recommendation. Subscription: {Subscription}, RecommendationId: {RecommendationId}, RecommendationStatus: {RecommendationStatus}.",
                options.Subscription,
                options.RecommendationId,
                options.RecommendationStatus);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation not found. Verify the subscription and recommendation ID.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Advisor recommendation. Verify you have permission to update recommendation state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Advisor rejected the recommendation state update. {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The Advisor recommendation was modified concurrently. Retrieve the latest recommendation and retry.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecommendationPatchResult(Models.Recommendation Recommendation);
}
