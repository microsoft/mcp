// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
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
        Update the customer-provided status of one Azure Advisor recommendation in a subscription.
        Mark an Advisor recommendation as Completed. Dismiss an Advisor recommendation because the risk is acceptable by using RiskIsAcceptable, or select another explicit dismissal reason.
        Postpone an Advisor recommendation until a requested future date and time. Reactivate a postponed or dismissed recommendation by setting it to New.
        Requires subscription context from --subscription, which accepts an Azure subscription ID or name, or from the configured default subscription. Requires --recommendation-id, which is the recommendation's stable ID.
        If no dismissal reason is supplied or the user's intent cannot be mapped to a supported reason, uses Other.
        State changes are rejected for Security category recommendations, and recommendations already marked as resolved by the Advisor platform.
        Returns a concise shared recommendation object containing the updated lifecycle state and identifying recommendation and resource details.
        """,
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
    : SubscriptionCommand<RecommendationUpdateOptions, RecommendationUpdateCommand.RecommendationUpdateResult>(subscriptionResolver)
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

        RecommendationStateUpdateValidator.AddValidationErrors(
            options.RecommendationStatus,
            options.PostponedUntilDateTime,
            validationResult.Errors);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RecommendationUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var recommendationDismissReason = RecommendationStateUpdateValidator.ResolveDismissReason(
                options.RecommendationStatus,
                options.RecommendationDismissReason);
            var recommendation = await _advisorService.UpdateRecommendationAsync(
                options.Subscription!,
                options.RecommendationId.Trim(),
                options.RecommendationStatus,
                options.PostponedUntilDateTime,
                recommendationDismissReason,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(recommendation),
                AdvisorJsonContext.Default.RecommendationUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Advisor recommendation. Subscription: {Subscription}, RecommendationId: {RecommendationId}, RecommendationStatus: {RecommendationStatus}.",
                options.Subscription,
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
        RequestFailedException { ErrorCode: "InvalidRequestPayload" } reqEx =>
            $"Advisor rejected the recommendation state update. {reqEx.Message}",
        RequestFailedException { ErrorCode: "InvalidSubscriptionId" } reqEx =>
            $"Advisor rejected the subscription. {reqEx.Message}",
        RequestFailedException { ErrorCode: "InvalidRecommendationId" } reqEx =>
            $"Advisor rejected the recommendation ID. {reqEx.Message}",
        RequestFailedException { ErrorCode: "RecommendationNotFound" } =>
            "Advisor recommendation not found. Verify --subscription and --recommendation-id",
        RequestFailedException { ErrorCode: "ConcurrentModification" } =>
            "The Advisor recommendation was modified concurrently. Retrieve the latest recommendation and retry the update",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Advisor recommendation not found. Verify --subscription and --recommendation-id",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Advisor recommendation. Verify you have permission to update recommendation state",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Advisor rejected the recommendation state update. {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "The Advisor recommendation was modified concurrently. Retrieve the latest recommendation and retry",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecommendationUpdateResult(Models.Recommendation Recommendation);
}
