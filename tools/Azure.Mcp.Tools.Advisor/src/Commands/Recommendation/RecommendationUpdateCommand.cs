// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "567ca9d4-f802-4dd8-bfa3-2034e3c208ec",
    Name = "update",
    Title = "Update Advisor Recommendation State",
    Description = """
        Mark one Azure Advisor recommendation as completed, dismiss it because the risk is acceptable, postpone it until a requested future date and time, or reactivate it by setting New.
        When a user asks to ignore a recommendation because the risk is acceptable, dismiss it with the RiskIsAcceptable reason.
        Snooze an Advisor recommendation in a subscription or Azure service group until a future date and time. Snooze is the same operation as postpone and sets the recommendation to Postponed.
        Update, change, or set the customer-provided recommendation status or state to New, Postponed, Dismissed, or Completed. Common user requests may say mark done, ignore, reopen, or reset to New.
        Requires the stable recommendation ID and either --subscription or --service-group, but not both; the configured default subscription is used when neither is supplied.
        Postponed requires a future ISO 8601 date and time with a timezone offset. For Dismissed, provide an explicit supported reason when known; if no reason is supplied or the user's intent cannot be mapped to one, Other is used.
        Use --tenant when the target subscription or service group is in a non-default tenant. State changes are rejected for Security category and platform-resolved recommendations.
        Returns the updated ARM recommendation resource. Use this state-changing tool instead of list or summary when the user wants to modify or snooze one recommendation.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecommendationUpdateCommand(
    ILogger<RecommendationUpdateCommand> logger,
    IAdvisorService advisorService,
    ISubscriptionResolver subscriptionResolver)
    : RecommendationScopeCommand<RecommendationUpdateOptions, RecommendationUpdateCommand.RecommendationUpdateResult>(
        subscriptionResolver)
{
    private readonly ILogger<RecommendationUpdateCommand> _logger = logger;
    private readonly IAdvisorService _advisorService = advisorService;

    public override void ValidateOptions(RecommendationUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.RecommendationId))
        {
            validationResult.Errors.Add("--recommendation-id is required and cannot be empty.");
        }

        RecommendationStateUpdateValidator.AddCommandValidationErrors(
            options.RecommendationStatus,
            options.PostponedUntilDateTime,
            options.RecommendationDismissReason,
            validationResult.Errors);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _ = RecommendationStateUpdateValidator.TryParsePostponedUntilDateTime(
                options.PostponedUntilDateTime,
                out var postponedUntilDateTime,
                out _);
            var recommendationDismissReason = RecommendationStateUpdateValidator.ResolveDismissReason(
                options.RecommendationStatus,
                options.RecommendationDismissReason);
            var recommendation = !string.IsNullOrEmpty(options.ServiceGroup)
                ? await _advisorService.UpdateServiceGroupRecommendationAsync(
                    options.ServiceGroup,
                    options.RecommendationId.Trim(),
                    options.RecommendationStatus,
                    postponedUntilDateTime,
                    recommendationDismissReason,
                    options.Tenant,
                    cancellationToken)
                : await _advisorService.UpdateRecommendationAsync(
                    options.Subscription!,
                    options.RecommendationId.Trim(),
                    options.RecommendationStatus,
                    postponedUntilDateTime,
                    recommendationDismissReason,
                    options.Tenant,
                    cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(recommendation),
                AdvisorJsonContext.Default.RecommendationUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Advisor recommendation. Subscription: {Subscription}, ServiceGroup: {ServiceGroup}, RecommendationId: {RecommendationId}, RecommendationStatus: {RecommendationStatus}.",
                options.Subscription,
                options.ServiceGroup,
                options.RecommendationId,
                options.RecommendationStatus);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException { ErrorCode: "SecurityRecommendationStateChangeBlocked" } =>
            "Advisor recommendation state cannot be updated because state changes are not allowed for Security category recommendations",
        RequestFailedException { ErrorCode: "UndefinedRecommendationStateChangeBlocked" } =>
            "Advisor recommendation state cannot be updated because state changes are not allowed for recommendations with an Undefined customer state",
        RequestFailedException { ErrorCode: "ResolvedRecommendationStateChangeBlocked" } =>
            "Advisor recommendation state cannot be updated because the recommendation has already been marked as resolved by the Advisor platform",
        RequestFailedException { ErrorCode: "InvalidRequestPayload" } =>
            "Advisor rejected the recommendation state update because the request payload was invalid",
        RequestFailedException { ErrorCode: "InvalidSubscriptionId" } =>
            "Advisor rejected the subscription. Verify --subscription",
        RequestFailedException { ErrorCode: "InvalidServiceGroupId" } =>
            "Advisor rejected the service group. Verify --service-group",
        RequestFailedException { ErrorCode: "InvalidRecommendationId" } =>
            "Advisor rejected the recommendation ID. Verify --recommendation-id",
        RequestFailedException { ErrorCode: "RecommendationNotFound" } =>
            "Advisor recommendation not found. Verify --subscription or --service-group and --recommendation-id",
        RequestFailedException { ErrorCode: "ConcurrentModification" } =>
            "The Advisor recommendation was modified concurrently. Retrieve the latest recommendation and retry the update",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation not found. Verify --subscription or --service-group and --recommendation-id",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Advisor recommendation. Verify you have permission to update recommendation state",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            "Advisor rejected the recommendation state update",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The Advisor recommendation was modified concurrently. Retrieve the latest recommendation and retry",
        RequestFailedException { ErrorCode: not null } reqEx =>
            $"Advisor recommendation update failed with error code '{reqEx.ErrorCode}'",
        RequestFailedException reqEx =>
            $"Advisor recommendation update failed with status code {reqEx.Status}",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecommendationUpdateResult(Models.Recommendation Recommendation);
}
