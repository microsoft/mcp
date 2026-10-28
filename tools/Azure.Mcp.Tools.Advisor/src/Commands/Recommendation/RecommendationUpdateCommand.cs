// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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
        Supply either one stable --recommendation-id or an explicit --recommendation-ids batch, not both. A batch uses one destructive tool call and one server consent request, rather than a separate tool call per resource.
        All batch IDs share the same scope, tenant, status, and postponement date or dismissal reason. Use either --subscription or --service-group, not both; the configured default subscription is used when neither is supplied.
        Postponed requires a future ISO 8601 date and time with a timezone offset. For Dismissed, provide an explicit supported reason when known; if no reason is supplied or the user's intent cannot be mapped to one, Other is used.
        Use --tenant when the target subscription or service group is in a non-default tenant. State changes are rejected for Security category and platform-resolved recommendations.
        A single-ID request returns the updated ARM recommendation resource as before. A batch returns per-ID Succeeded, Failed, Unverified, or NotAttempted outcomes; updates are independent, not atomic, and are not automatically retried.
        Use this state-changing tool instead of list or summary when the user wants to modify or snooze selected recommendations.
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

        if (options.RecommendationId is not null && options.RecommendationIds is not null)
        {
            validationResult.Errors.Add("Specify either --recommendation-id or --recommendation-ids, not both.");
        }
        else if (options.RecommendationIds is { } ids)
        {
            if (ids.Length == 0 || ids.Any(string.IsNullOrWhiteSpace))
            {
                validationResult.Errors.Add("--recommendation-ids must contain at least one ID and no empty IDs.");
            }
            else if (ids.Select(id => id.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != ids.Length)
            {
                validationResult.Errors.Add("--recommendation-ids must not contain duplicate IDs.");
            }
        }
        else if (string.IsNullOrWhiteSpace(options.RecommendationId))
        {
            validationResult.Errors.Add("--recommendation-id or --recommendation-ids is required and cannot be empty.");
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
        AdvisorTelemetryTags.AddScopeTags(context.Activity, options);

        try
        {
            _ = RecommendationStateUpdateValidator.TryParsePostponedUntilDateTime(
                options.PostponedUntilDateTime,
                out var postponedUntilDateTime,
                out _);
            var recommendationDismissReason = RecommendationStateUpdateValidator.ResolveDismissReason(
                options.RecommendationStatus,
                options.RecommendationDismissReason);
            if (options.RecommendationIds is { } ids)
            {
                return await ExecuteBatchAsync(
                    context, options, ids, postponedUntilDateTime, recommendationDismissReason, cancellationToken);
            }

            var recommendation = await UpdateRecommendationAsync(
                options, options.RecommendationId!.Trim(), postponedUntilDateTime, recommendationDismissReason, cancellationToken);

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

    private Task<Models.Recommendation> UpdateRecommendationAsync(
        RecommendationUpdateOptions options,
        string recommendationId,
        DateTimeOffset? postponedUntilDateTime,
        RecommendationDismissReason? recommendationDismissReason,
        CancellationToken cancellationToken) =>
        !string.IsNullOrEmpty(options.ServiceGroup)
            ? _advisorService.UpdateServiceGroupRecommendationAsync(
                options.ServiceGroup, recommendationId, options.RecommendationStatus,
                postponedUntilDateTime, recommendationDismissReason, options.Tenant, cancellationToken)
            : _advisorService.UpdateRecommendationAsync(
                options.Subscription!, recommendationId, options.RecommendationStatus,
                postponedUntilDateTime, recommendationDismissReason, options.Tenant, cancellationToken);

    private async Task<CommandResponse> ExecuteBatchAsync(
        CommandContext context,
        RecommendationUpdateOptions options,
        string[] ids,
        DateTimeOffset? postponedUntilDateTime,
        RecommendationDismissReason? recommendationDismissReason,
        CancellationToken cancellationToken)
    {
        List<RecommendationUpdateItemResult> results = [];
        foreach (var id in ids)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Advisor recommendation batch was canceled before all updates were attempted.");
                break;
            }

            var recommendationId = id.Trim();
            try
            {
                var recommendation = await UpdateRecommendationAsync(
                    options, recommendationId, postponedUntilDateTime, recommendationDismissReason, cancellationToken);
                results.Add(new(recommendationId, RecommendationUpdateOutcome.Succeeded, recommendation, (int)HttpStatusCode.OK));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Advisor recommendation update was canceled. RecommendationId: {RecommendationId}.", recommendationId);
                HandleException(context, ex);
                // A canceled write may have reached Azure; preserve its uncertain outcome and do not retry it.
                results.Add(new(recommendationId, RecommendationUpdateOutcome.Unverified,
                    StatusCode: (int)HttpStatusCode.RequestTimeout,
                    Error: "The update was canceled; recommendation state is unverified. Read its current state before retrying."));
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Advisor batch recommendation. RecommendationId: {RecommendationId}.", recommendationId);
                HandleException(context, ex);
                var isUnverified = ex is HttpRequestException or IOException or JsonException or TimeoutException ||
                    ex is RequestFailedException { Status: <= 0 or 408 or >= 500 };
                var statusCode = (int)GetStatusCode(ex);
                results.Add(new(recommendationId,
                    isUnverified ? RecommendationUpdateOutcome.Unverified : RecommendationUpdateOutcome.Failed,
                    StatusCode: statusCode is >= 400 and <= 599 ? statusCode : (int)HttpStatusCode.BadGateway,
                    Error: isUnverified
                        ? "The update outcome is unverified. Read its current state before retrying."
                        : ex is RequestFailedException ? GetErrorMessage(ex) : "Advisor recommendation update failed. Read its current state before retrying."));
            }
        }

        while (results.Count < ids.Length)
        {
            results.Add(new(ids[results.Count].Trim(), RecommendationUpdateOutcome.NotAttempted,
                Error: "The batch was canceled before this update was attempted."));
        }

        var succeeded = results.Count(result => result.Outcome == RecommendationUpdateOutcome.Succeeded);
        var failed = results.Count(result => result.Outcome == RecommendationUpdateOutcome.Failed);
        var unverified = results.Count(result => result.Outcome == RecommendationUpdateOutcome.Unverified);
        var notAttempted = results.Count - succeeded - failed - unverified;
        context.Response.Status = succeeded == results.Count
            ? HttpStatusCode.OK
            : succeeded > 0
                ? HttpStatusCode.MultiStatus
                : (HttpStatusCode)(results[0].StatusCode ?? (int)HttpStatusCode.RequestTimeout);
        context.Response.Message = $"Advisor batch update: {succeeded} succeeded; {failed} failed; {unverified} unverified; {notAttempted} not attempted.";
        context.Response.Results = ResponseResult.Create(
            new RecommendationUpdateResult(null) { Results = results }, AdvisorJsonContext.Default.RecommendationUpdateResult);
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

    public sealed record RecommendationUpdateResult(Models.Recommendation? Recommendation)
    {
        public IReadOnlyList<RecommendationUpdateItemResult>? Results { get; init; }
    }
}
