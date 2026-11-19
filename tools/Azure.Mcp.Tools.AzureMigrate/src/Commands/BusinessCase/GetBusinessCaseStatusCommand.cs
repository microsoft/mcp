// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;

/// <summary>
/// POC: checks a business case and moves it forward, statelessly. Which stage the business case is in follows from the parameters:
/// <list type="bullet">
/// <item>With 'ds-session-id' and 'ds-operation-id' (stage 2, HaSkill), it reads the HA skill operation on the Discovery Service.</item>
/// <item>Otherwise (stage 1, Assessment), it reads the assessment operation. Once it has succeeded, the same call runs the Business
/// Case Service on-premises TCO and the HA evaluate (both synchronous; identical requests reuse earlier results, so repeating them
/// is safe) and starts the HA skill in a session derived from the assessment operation ID. While the HA skill is mocked (it is not
/// deployed on the Discovery Service yet), it instead finishes with the business case per migration strategy read from the HA
/// evaluation result. A transient failure of any step leaves
/// the business case Running so that the next call repeats it.</item>
/// </list>
/// Each call does at most one pass with no waiting, so it finishes within the client's tool call limit.
/// </summary>
[CommandMetadata(
    Id = "83765d81-da5b-4d77-8c0d-526cc20f0066",
    Name = "status",
    Title = "Get Azure Migrate Business Case Status",
    Description = "Checks a business case started by the business case create tool and moves it to its next stage. Pass exactly the " +
        "values in 'nextStatusParameters' of the latest business case result in the conversation, unchanged; ask the user to create a " +
        "business case if there is none. Returns 'status' (Running, Succeeded or Failed), 'stage' and 'stageNumber' of 'totalStages', " +
        "'isCompleted', 'nextStatusParameters' while it is running, 'result' once it succeeded, and 'instructions'. Follow " +
        "'instructions'. Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GetBusinessCaseStatusCommand(
    IAssessmentOperationStatusClient assessmentStatusClient,
    IBusinessCaseEvaluationClient evaluationClient,
    IHaSessionClient haClient,
    IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetBusinessCaseStatusOptions, BusinessCaseCommandResult>
{
    internal const int DefaultRetryAfterSeconds = 30;

    private readonly IAssessmentOperationStatusClient _assessmentStatusClient = assessmentStatusClient;
    private readonly IBusinessCaseEvaluationClient _evaluationClient = evaluationClient;
    private readonly IHaSessionClient _haClient = haClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Returns the Discovery Service session ID of a business case: 'bc-' and 16 hex characters of the SHA-256 of the assessment
    /// operation ID, so repeated calls address the same session (the Discovery Service allows 1-20 characters).
    /// </summary>
    internal static string SessionIdFor(Guid asOperationId) =>
        "bc-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(asOperationId.ToString("D"))))[..16].ToLowerInvariant();

    /// <summary>Returns true for an HTTP status worth retrying on the next call: none (network error), 408, 429, 500, 502, 503 or 504.</summary>
    internal static bool IsTransient(int? httpStatus) => httpStatus is null or 408 or 429 or 500 or 502 or 503 or 504;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetBusinessCaseStatusOptions options, CancellationToken cancellationToken)
    {
        BusinessCaseCommandResult result = await RunAsync(options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.BusinessCaseCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    internal async Task<BusinessCaseCommandResult> RunAsync(GetBusinessCaseStatusOptions options, CancellationToken cancellationToken)
    {
        string? name = string.IsNullOrWhiteSpace(options.AssessmentName) ? null : options.AssessmentName.Trim();
        string? asOperationIdText = options.AsOperationId?.Trim();
        if (!Guid.TryParse(asOperationIdText, out Guid asOperationId))
        {
            return BusinessCaseStages.Invalid("'as-operation-id' is required and must be a GUID, from 'nextStatusParameters'.", name, asOperationIdText);
        }

        string? inventorySasUri = options.InventorySasUri?.Trim();
        if (!CreateAssessmentCommand.IsValidInventorySasUri(inventorySasUri))
        {
            return BusinessCaseStages.Invalid("'inventory-sas-uri' is required, from 'nextStatusParameters'.", name, asOperationIdText);
        }

        string? dsSessionId = string.IsNullOrWhiteSpace(options.DsSessionId) ? null : options.DsSessionId.Trim();
        string? dsOperationIdText = string.IsNullOrWhiteSpace(options.DsOperationId) ? null : options.DsOperationId.Trim();
        Guid dsOperationId = Guid.Empty;
        if ((dsSessionId is null) != (dsOperationIdText is null)
            || (dsSessionId is not null && (!DsBackdoorClient.IsValidSessionId(dsSessionId) || !Guid.TryParse(dsOperationIdText, out dsOperationId))))
        {
            return BusinessCaseStages.Invalid(
                "'ds-session-id' and 'ds-operation-id' must be passed together, unchanged from 'nextStatusParameters'.", name, asOperationIdText);
        }

        (BusinessCaseHaSettings? settings, string? settingsError) = BusinessCaseHaSettings.Build(options);
        if (settingsError is not null)
        {
            return BusinessCaseStages.Invalid(settingsError, name, asOperationIdText);
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return BusinessCaseStages.Invalid(authError, name, asOperationIdText);
        }

        Context ctx = new(name, asOperationId, inventorySasUri!, settings!);
        return dsSessionId is not null
            ? await CheckHaSkillAsync(ctx, token, dsSessionId, dsOperationId, cancellationToken)
            : await AdvanceAssessmentAsync(ctx, token, cancellationToken);
    }

    private async Task<BusinessCaseCommandResult> CheckHaSkillAsync(
        Context ctx, string token, string sessionId, Guid operationId, CancellationToken cancellationToken)
    {
        string operationIdText = operationId.ToString("D");
        if (MockHaSessionClient.IsMockSession(sessionId))
        {
            return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, (int)HttpStatusCode.OK, null, null,
                "Mocked HA result: the HA skill is not deployed yet, so no HA skill was run.",
                "Tell the user the business case evaluations have completed but the final HA step is mocked because the HA skill is not " +
                "deployed yet, so there is no business case result to show.");
        }

        HaOperationStatus status = await _haClient.GetOperationStatusAsync(token, sessionId, operationId, cancellationToken);
        if (status.HttpStatus is 401 or 403 or 404)
        {
            return Finished(ctx, BusinessCaseStages.Failed, sessionId, operationIdText, "haSkillStatus", status.HttpStatus, status.ErrorCode,
                status.ErrorMessage ?? status.Error, null,
                status.HttpStatus == 404
                    ? "Tell the user the business case run could not be found for them and offer to create a new business case."
                    : "Tell the user they are not allowed to read this business case and quote 'errorMessage'.");
        }

        if (status.HttpStatus is not (>= 200 and < 300) || string.IsNullOrWhiteSpace(status.Status))
        {
            return Running(ctx, BusinessCaseStages.HaSkill, sessionId, operationIdText, status.HttpStatus, status.RetryAfterSeconds,
                "If you have not already said so, tell the user once that the business case is in its final stage (2 of 2).");
        }

        if (string.Equals(status.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase))
        {
            return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, status.HttpStatus, null, null, status.Result,
                "Tell the user the business case is ready and summarise 'result' in plain language without inventing details.");
        }

        if (CreateAssessmentCommand.IsTerminal(status.Status))
        {
            return Finished(ctx, BusinessCaseStages.Failed, sessionId, operationIdText, "haSkill", status.HttpStatus, status.ErrorCode,
                status.ErrorMessage, null,
                "Tell the user the business case did not complete in its final stage and quote 'errorMessage'. Offer to create it again.");
        }

        return Running(ctx, BusinessCaseStages.HaSkill, sessionId, operationIdText, status.HttpStatus, status.RetryAfterSeconds,
            "If you have not already said so, tell the user once that the business case is in its final stage (2 of 2).");
    }

    private async Task<BusinessCaseCommandResult> AdvanceAssessmentAsync(Context ctx, string token, CancellationToken cancellationToken)
    {
        AssessmentOperation operation = await _assessmentStatusClient.GetOperationAsync(ctx.AsOperationId, null, token, cancellationToken);
        if (operation.HttpStatus is 401 or 403 or 404)
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessmentStatus", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage, null,
                operation.HttpStatus == 404
                    ? "Tell the user the business case assessment could not be found for them and offer to create a new business case."
                    : "Tell the user they are not allowed to read this business case and quote 'errorMessage'.");
        }

        if (operation.HttpStatus is not (>= 200 and < 300) || !CreateAssessmentCommand.IsTerminal(operation.Status))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, operation.HttpStatus, operation.RetryAfterSeconds, AssessmentRunningText);
        }

        if (!string.Equals(operation.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase))
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessment", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage, null, "Tell the user the business case assessment did not complete and quote 'errorMessage'.");
        }

        if (!CreateAssessmentCommand.IsValidInventorySasUri(operation.OutputUrl))
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessmentOutput", operation.HttpStatus, null,
                "The assessment did not return a valid output URL.", null,
                "Tell the user the business case assessment completed without an output link, so the business case cannot continue.");
        }

        EvaluationResult tco = await _evaluationClient.GetOnPremTcoAsync(token, operation.OutputUrl!, ctx.InventorySasUri, cancellationToken);
        if (CheckEvaluation(ctx, tco, "onPremTco") is { } tcoResult)
        {
            return tcoResult;
        }

        EvaluationResult ha = await _evaluationClient.EvaluateHaAsync(token, operation.OutputUrl!, tco.ManifestUri!, ctx.Settings, cancellationToken);
        if (CheckEvaluation(ctx, ha, "haEvaluate") is { } haResult)
        {
            return haResult;
        }

        if (_haClient is MockHaSessionClient)
        {
            return await FinishFromHaEvaluationAsync(ctx, ha.ManifestUri!, cancellationToken);
        }

        HaSessionResult start = await _haClient.StartInSessionAsync(
            token, new HaInput(ctx.InventorySasUri, operation.OutputUrl!, tco.ManifestUri, ha.ManifestUri), SessionIdFor(ctx.AsOperationId),
            cancellationToken);
        if (start.FailedStep is null && start.SessionId is not null && start.OperationId is not null)
        {
            return Running(ctx, BusinessCaseStages.HaSkill, start.SessionId, start.OperationId, start.HttpStatus, null,
                "Tell the user the business case evaluations have completed and its final stage (2 of 2) has started.");
        }

        // The session already runs (an earlier call started it and its result was lost) or the Discovery Service is busy: try again.
        if (IsTransient(start.HttpStatus) || (start.FailedStep == "invoke" && start.Error?.Contains("SessionState", StringComparison.Ordinal) == true))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, start.HttpStatus, null, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, start.SessionId, start.OperationId, start.FailedStep ?? "haSkillStart", start.HttpStatus,
            null, start.Error, null,
            start.HttpStatus == (int)HttpStatusCode.Conflict
                ? "Tell the user the business case could not start its final stage because another of their Discovery Service sessions " +
                  "is active. They can try again once it has finished."
                : "Tell the user the business case could not start its final stage and quote 'errorMessage'.");
    }

    private const string AssessmentRunningText =
        "If you have not already said so, tell the user once that the business case is being evaluated (stage 1 of 2); do not repeat it.";

    /// <summary>
    /// While the HA skill is not deployed on the Discovery Service (HA mocked), the business case finishes with the HA evaluation's
    /// business case per migration strategy instead of the HA skill result.
    /// </summary>
    private async Task<BusinessCaseCommandResult> FinishFromHaEvaluationAsync(Context ctx, string haManifestUri, CancellationToken cancellationToken)
    {
        HaSummaryResult summary = await _evaluationClient.GetHaSummaryAsync(haManifestUri, cancellationToken);
        if (summary.Summary is not null)
        {
            return Finished(ctx, BusinessCaseStages.Succeeded, null, null, null, summary.HttpStatus, null, null, summary.Summary,
                "Tell the user the business case is ready. Summarise 'result' per migration strategy in plain language: annual savings, " +
                "payback period in years, total Azure versus on-premises cost and the year-on-year savings, in 'currency'. Do not invent " +
                "details; for a strategy with 'unavailableReason', say it has no business case.", BusinessCaseStages.HaSkill);
        }

        if (IsTransient(summary.HttpStatus))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, summary.HttpStatus, null, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, null, null, "haResult", summary.HttpStatus, null, summary.Error, null,
            "Tell the user the business case evaluation finished but its result could not be read, and quote 'errorMessage'.");
    }

    private static BusinessCaseCommandResult? CheckEvaluation(Context ctx, EvaluationResult evaluation, string step)
    {
        bool succeeded = evaluation.HttpStatus is >= 200 and < 300
            && string.Equals(evaluation.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase);
        if (succeeded && CreateAssessmentCommand.IsValidInventorySasUri(evaluation.ManifestUri))
        {
            return null;
        }

        if (IsTransient(evaluation.HttpStatus) || (evaluation.HttpStatus is >= 200 and < 300 && !CreateAssessmentCommand.IsTerminal(evaluation.Status)))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, evaluation.HttpStatus, evaluation.RetryAfterSeconds, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, null, null, step, evaluation.HttpStatus, evaluation.ErrorCode,
            evaluation.ErrorMessage ?? (succeeded ? "The response did not include a valid manifest URL." : null), null,
            "Tell the user the business case evaluation failed and quote 'errorMessage'.");
    }

    private static BusinessCaseCommandResult Running(
        Context ctx, string stage, string? dsSessionId, string? dsOperationId, int? httpStatus, int? retryAfterSeconds, string instructions) =>
        new(true, null, BusinessCaseStages.Running, false, stage, BusinessCaseStages.StageNumber(stage), BusinessCaseStages.Total,
            ctx.AssessmentName, ctx.AsOperationId.ToString("D"), dsSessionId, dsOperationId, null, httpStatus, null, null, null,
            retryAfterSeconds ?? DefaultRetryAfterSeconds,
            BusinessCaseNextStatusParameters.Create(ctx.AssessmentName, ctx.AsOperationId, ctx.InventorySasUri, ctx.Settings, dsSessionId, dsOperationId),
            instructions + " " + BusinessCaseStages.CheckAgainInstructions);

    private static BusinessCaseCommandResult Finished(
        Context ctx, string status, string? dsSessionId, string? dsOperationId, string? failedStep, int? httpStatus, string? errorCode,
        string? errorMessage, string? result, string instructions, string? stage = null)
    {
        stage ??= dsSessionId is null ? BusinessCaseStages.Assessment : BusinessCaseStages.HaSkill;
        return new(true, null, status, true, stage, BusinessCaseStages.StageNumber(stage), BusinessCaseStages.Total, ctx.AssessmentName,
            ctx.AsOperationId.ToString("D"), dsSessionId, dsOperationId, failedStep, httpStatus, errorCode, errorMessage, result, null, null,
            instructions);
    }

    private sealed record Context(string? AssessmentName, Guid AsOperationId, string InventorySasUri, BusinessCaseHaSettings Settings);
}
