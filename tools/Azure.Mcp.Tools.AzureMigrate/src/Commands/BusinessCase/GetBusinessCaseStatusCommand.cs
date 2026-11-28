// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;
using Microsoft.AspNetCore.Http;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Services;

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
    Id = CommandId,
    Name = "status",
    Title = "Get Azure Catalyst Business Case Status",
    Description = "Tracks the state of an Azure Catalyst business case (including its assessment, ROI summary and executive " +
        "artifacts) started by the business case create tool, moves it to its next stage and returns its result once it finishes. " +
        "When: when the 'instructions' of the latest business case result say to check its status, or when the user asks about the " +
        "progress or result of their business case, assessment, ROI summary or executive artifacts. " +
        "How: pass exactly the values in 'nextStatusParameters' of the latest business case result in the conversation, unchanged " +
        "and without inventing any; ask the user to create a business case if there is none. Wait about 'retryAfterSeconds' between " +
                "calls. Call it directly yourself, one call at a time; never write or run a script or code to call it or to wait between calls. " +
        "Returns: 'status' (Running, Succeeded or Failed), 'stage' and 'stageNumber' of 'totalStages', 'isCompleted', " +
        "'nextStatusParameters' while it is running, 'result' once it succeeded (a read link to the ROI report), 'report' (the ROI " +
        "report Markdown, when available) and 'instructions'. " +
        "Next: follow 'instructions': while 'status' is Running they say how to keep tracking it with the new " +
        "'nextStatusParameters'; once it has succeeded, follow 'instructions' " +
        "to show the report. Do not mention tool or parameter names to the user. " + ReplyFormat.ToolRule,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GetBusinessCaseStatusCommand(
    IAssessmentService assessmentService,
    IBusinessCaseEvaluationService evaluationService,
    IHaSessionService haService,
    IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetBusinessCaseStatusOptions, BusinessCaseCommandResult>
{
    internal const string CommandId = "83765d81-da5b-4d77-8c0d-526cc20f0066";
    internal const int DefaultRetryAfterSeconds = 30;

    private readonly IAssessmentService _assessmentService = assessmentService;
    private readonly IBusinessCaseEvaluationService _evaluationService = evaluationService;
    private readonly IHaSessionService _haService = haService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// True when the report card reads ROI.md from its SAS URL in the browser (see <see cref="BusinessCaseReportWidget.FetchesReport"/>),
    /// so the server returns only links and never downloads the report.
    /// </summary>
    internal bool CardFetchesReport { get; init; } = BusinessCaseReportWidget.FetchesReport;

    /// <summary>
    /// Returns the Discovery Service session ID of a business case: 'bc-' and 16 hex characters of the SHA-256 of the assessment
    /// operation ID, so repeated calls address the same session (the Discovery Service allows 1-20 characters).
    /// </summary>
    internal static string SessionIdFor(Guid asOperationId) =>
        "bc-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(asOperationId.ToString("D"))))[..16].ToLowerInvariant();

    /// <summary>Returns true for an HTTP status worth retrying on the next call: none (network error), 408, 429, 500, 502, 503 or 504.</summary>
    /// <summary>How the agent opens the ROI report in its file preview (side panel) when the user asks.</summary>
    internal const string OpenReportInstructions =
        "Later, if the user asks to open, preview or view the ROI report in the side panel or as a file, create a file named ROI.md " +
        "whose content is 'report' exactly as given, byte for byte, and open it; do not rewrite, " +
        "summarise or reformat it.";

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
        if (!AssessmentValidation.IsValidInventorySasUri(inventorySasUri))
        {
            return BusinessCaseStages.Invalid("'inventory-sas-uri' is required, from 'nextStatusParameters'.", name, asOperationIdText);
        }

        string? dsSessionId = string.IsNullOrWhiteSpace(options.DsSessionId) ? null : options.DsSessionId.Trim();
        string? dsOperationIdText = string.IsNullOrWhiteSpace(options.DsOperationId) ? null : options.DsOperationId.Trim();
        Guid dsOperationId = Guid.Empty;
        if ((dsSessionId is null) != (dsOperationIdText is null)
            || (dsSessionId is not null && (!DiscoveryServiceApi.IsValidSessionId(dsSessionId) || !Guid.TryParse(dsOperationIdText, out dsOperationId))))
        {
            return BusinessCaseStages.Invalid(
                "'ds-session-id' and 'ds-operation-id' must be passed together, unchanged from 'nextStatusParameters'.", name, asOperationIdText);
        }

        (BusinessCaseHaSettings? settings, string? settingsError) = BusinessCaseHaSettings.Build(options);
        if (settingsError is not null)
        {
            return BusinessCaseStages.Invalid(settingsError, name, asOperationIdText);
        }

        (string? token, string? authError) = DiscoveryServiceApi.GetBearerToken(_httpContextAccessor);
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
        if (MockHaSessionService.IsMockSession(sessionId))
        {
            return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, (int)HttpStatusCode.OK, null, null,
                "Mocked HA result: the HA skill is not deployed yet, so no HA skill was run.",
                ReplyFormat.Reply("Business case evaluations completed (final step mocked).",
                    "that the final HA step is mocked because the HA skill is not deployed yet, so there is no ROI result to show.",
                    "create the business case again once the HA skill is deployed."));
        }

        HaOperationStatus status = await _haService.GetOperationStatusAsync(token, sessionId, operationId, cancellationToken);
        if (status.HttpStatus is 401 or 403 or 404)
        {
            return Finished(ctx, BusinessCaseStages.Failed, sessionId, operationIdText, "haSkillStatus", status.HttpStatus, status.ErrorCode,
                status.ErrorMessage ?? status.Error, null, NotFoundOrForbidden(status.HttpStatus));
        }

        if (status.HttpStatus is not (>= 200 and < 300) || string.IsNullOrWhiteSpace(status.Status))
        {
            return Running(ctx, BusinessCaseStages.HaSkill, sessionId, operationIdText, status.HttpStatus, status.RetryAfterSeconds,
                "If you have not already said so, tell the user once that the business case is in its final stage (2 of 2).");
        }

        if (string.Equals(status.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase))
        {
            InventoryExportResult report = await _haService.ExportAsync(token, sessionId, cancellationToken);
            if (report.Uri is not null)
            {
                InventoryExportResult artifacts = await _haService.ExportArtifactsAsync(token, sessionId, cancellationToken);
                string? artifactsUri = artifacts.Uri;
                string artifactsLink = artifactsUri is null ? "" :
                    " Also give a clickable Markdown link to 'artifacts' (a ZIP of all business case artifacts) exactly as given, for " +
                    "example [Download all artifacts (ZIP)](<artifacts>); never retype or shorten it.";
                if (CardFetchesReport)
                {
                    return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, report.HttpStatus, null, null, report.Uri,
                        ReplyFormat.Reply("Business case ready.",
                            "that the ROI report is shown in the card above, then a clickable Markdown link to 'result' (the ROI report, " +
                            "ROI.md) exactly as given, for example [Download the ROI report (ROI.md)](<result>) (link expires soon); never " +
                            "retype or shorten the link." + artifactsLink + " You do not have the report's contents, so do not quote, guess " +
                            "or summarise any figures, strategies or costs from it.",
                            ReplyFormat.AfterBusinessCase)) with { Artifacts = artifactsUri };
                }

                string? markdown = await _haService.DownloadReportAsync(report.Uri, cancellationToken);
                if (markdown is not null)
                {
                    return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, report.HttpStatus, null, null, report.Uri,
                        ReplyFormat.Reply(
                            "'Business case ready:' followed by the total annual savings and the payback period of the recommended " +
                            "migration strategy from 'report', quoting the figures exactly, for example 'Business case ready: $1.2M " +
                            "annual savings, payback in 1.8 years'.",
                            "ROI and NPV of the recommended strategy quoting 'report' exactly with currency symbols, then a clickable " +
                            "Markdown link to 'result' exactly as given, for example 'ROI 16.47%, NPV $92,605.44. [Download the ROI " +
                            "report](<result>) (link expires soon)'; never retype or shorten the link." + artifactsLink + " Do not paste 'report' or " +
                            "repeat its strategies, costs or issues" +
                            (BusinessCaseReportWidget.IsEnabled ? "; the full report is shown in the card above." : "."),
                            ReplyFormat.AfterBusinessCase) + " " + OpenReportInstructions) with { Report = markdown, Artifacts = artifactsUri };
                }

                return Finished(ctx, BusinessCaseStages.Succeeded, sessionId, operationIdText, null, report.HttpStatus, null, null, report.Uri,
                    ReplyFormat.Reply("Business case ready.",
                        "a clickable Markdown link to 'result' (the ROI report, ROI.md) exactly as given, for example " +
                        "[Download the ROI report (ROI.md)](<result>), never retyped, shortened or shown on its own; that the link " +
                        "expires and asking for the business case status again gives a new one." + artifactsLink,
                        ReplyFormat.AfterBusinessCase)) with { Artifacts = artifactsUri };
            }

            if (IsTransient(report.HttpStatus))
            {
                return Running(ctx, BusinessCaseStages.HaSkill, sessionId, operationIdText, report.HttpStatus, null,
                    "If you have not already said so, tell the user once that the business case report is being prepared.");
            }

            return Finished(ctx, BusinessCaseStages.Failed, sessionId, operationIdText, "export", report.HttpStatus, null, report.Error, null,
                ReplyFormat.Failure("Business case completed, but its ROI report could not be downloaded.",
                    "ask for the business case status again to retry the download."));
        }

        if (AssessmentValidation.IsTerminal(status.Status))
        {
            return Finished(ctx, BusinessCaseStages.Failed, sessionId, operationIdText, "haSkill", status.HttpStatus, status.ErrorCode,
                status.ErrorMessage, null,
                ReplyFormat.Failure("Business case did not complete (final stage 2 of 2).", RecreateNextStep));
        }

        return Running(ctx, BusinessCaseStages.HaSkill, sessionId, operationIdText, status.HttpStatus, status.RetryAfterSeconds,
            "If you have not already said so, tell the user once that the business case is in its final stage (2 of 2).");
    }

    private async Task<BusinessCaseCommandResult> AdvanceAssessmentAsync(Context ctx, string token, CancellationToken cancellationToken)
    {
        AssessmentOperation operation = await _assessmentService.GetOperationAsync(ctx.AsOperationId, null, token, cancellationToken);
        if (operation.HttpStatus is 401 or 403 or 404)
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessmentStatus", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage, null, NotFoundOrForbidden(operation.HttpStatus));
        }

        if (operation.HttpStatus is not (>= 200 and < 300) || !AssessmentValidation.IsTerminal(operation.Status))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, operation.HttpStatus, operation.RetryAfterSeconds, AssessmentRunningText);
        }

        if (!string.Equals(operation.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase))
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessment", operation.HttpStatus, operation.ErrorCode,
                operation.ErrorMessage, null, ReplyFormat.Failure("Business case assessment did not complete (stage 1 of 2).", RecreateNextStep));
        }

        if (!AssessmentValidation.IsValidInventorySasUri(operation.OutputUrl))
        {
            return Finished(ctx, BusinessCaseStages.Failed, null, null, "assessmentOutput", operation.HttpStatus, null,
                "The assessment did not return a valid output URL.", null,
                ReplyFormat.Failure("Business case stopped: its assessment returned no output.", RecreateNextStep));
        }

        EvaluationResult tco = await _evaluationService.GetOnPremTcoAsync(token, operation.OutputUrl!, ctx.InventorySasUri, cancellationToken);
        if (CheckEvaluation(ctx, tco, "onPremTco") is { } tcoResult)
        {
            return tcoResult;
        }

        EvaluationResult ha = await _evaluationService.EvaluateHaAsync(token, operation.OutputUrl!, tco.ManifestUri!, ctx.Settings, cancellationToken);
        if (CheckEvaluation(ctx, ha, "haEvaluate") is { } haResult)
        {
            return haResult;
        }

        if (_haService is MockHaSessionService)
        {
            return await FinishFromHaEvaluationAsync(ctx, ha.ManifestUri!, cancellationToken);
        }

        // The card and the agent's wait tool can both move the same business case forward: start its final stage only once per
        // assessment operation and give later callers the same session and operation.
        if (s_started.TryGetValue(ctx.AsOperationId, out StartedStage? started))
        {
            return Running(ctx, BusinessCaseStages.HaSkill, started.SessionId, started.OperationId, null, null, FinalStageRunningText);
        }

        SemaphoreSlim gate = s_startGates.GetOrAdd(ctx.AsOperationId, _ => new SemaphoreSlim(1, 1));
        if (!await gate.WaitAsync(StartGateTimeout, cancellationToken))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, null, null, AssessmentRunningText);
        }

        try
        {
            if (s_started.TryGetValue(ctx.AsOperationId, out started))
            {
                return Running(ctx, BusinessCaseStages.HaSkill, started.SessionId, started.OperationId, null, null, FinalStageRunningText);
            }

            return await StartFinalStageAsync(ctx, token, operation.OutputUrl!, tco.ManifestUri!, ha.ManifestUri!, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> s_startGates = new();
    private static readonly ConcurrentDictionary<Guid, StartedStage> s_started = new();
    private static readonly TimeSpan StartGateTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan StartedRetention = TimeSpan.FromHours(6);

    private sealed record StartedStage(string SessionId, string OperationId, DateTime StartedUtc);

    private const string FinalStageRunningText =
        "If you have not already said so, tell the user once that the business case is in its final stage (2 of 2).";

    private static void RememberStart(Guid asOperationId, string sessionId, string operationId)
    {
        DateTime now = DateTime.UtcNow;
        foreach (KeyValuePair<Guid, StartedStage> entry in s_started)
        {
            if (now - entry.Value.StartedUtc > StartedRetention && s_started.TryRemove(entry.Key, out _))
            {
                s_startGates.TryRemove(entry.Key, out _);
            }
        }

        s_started[asOperationId] = new(sessionId, operationId, now);
    }

    private async Task<BusinessCaseCommandResult> StartFinalStageAsync(
        Context ctx, string token, string assessmentOutputUrl, string tcoManifestUri, string haManifestUri, CancellationToken cancellationToken)
    {
        HaSessionResult start = await _haService.StartInSessionAsync(
            token, new HaInput(ctx.InventorySasUri, assessmentOutputUrl, tcoManifestUri, haManifestUri), SessionIdFor(ctx.AsOperationId),
            cancellationToken);
        if (start.FailedStep is null && start.SessionId is not null && start.OperationId is not null)
        {
            RememberStart(ctx.AsOperationId, start.SessionId, start.OperationId);
            return Running(ctx, BusinessCaseStages.HaSkill, start.SessionId, start.OperationId, start.HttpStatus, null,
                "Tell the user the business case evaluations have completed and its final stage (2 of 2) has started.");
        }

        // The caller's active session still runs an earlier business case: wait for it to finish, then run in it.
        if (start.FailedStep == HaSessionService.SessionBusyStep)
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, start.HttpStatus, null,
                "If you have not already said so, tell the user once that an earlier ROI summary of theirs is still running and this one " +
                "will start as soon as it finishes; do not repeat it.");
        }

        // The session already runs (an earlier call started it and its result was lost, or the reused active session started
        // another invocation in the meantime) or the Discovery Service is busy: try again.
        if (IsTransient(start.HttpStatus) || (start.FailedStep == "invokeSession" && start.Error?.Contains("SessionState", StringComparison.Ordinal) == true))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, start.HttpStatus, null, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, start.SessionId, start.OperationId, start.FailedStep ?? "haSkillStart", start.HttpStatus,
            null, start.Error, null,
            start.HttpStatus == (int)HttpStatusCode.Conflict
                ? ReplyFormat.Failure("Business case could not start its final stage: another of your sessions is active.",
                    "wait for that session to finish, then create the business case again.")
                : ReplyFormat.Failure("Business case could not start its final stage (2 of 2).", RecreateNextStep));
    }

    private const string RecreateNextStep =
        "fix the input or inventory issue named in the cause if there is one, then offer to create the business case again.";

    private static string NotFoundOrForbidden(int? httpStatus) => httpStatus == 404
        ? ReplyFormat.Failure("Business case not found.", "offer to create a new business case from their inventory.")
        : ReplyFormat.Failure("You are not allowed to read this business case.", "sign in with the account that created it, then ask again.");

    private const string AssessmentRunningText =
        "If you have not already said so, tell the user once that the business case is being evaluated (stage 1 of 2); do not repeat it.";

    /// <summary>
    /// While the HA skill is not deployed on the Discovery Service (HA mocked), the business case finishes with the HA evaluation's
    /// business case per migration strategy instead of the HA skill result.
    /// </summary>
    private async Task<BusinessCaseCommandResult> FinishFromHaEvaluationAsync(Context ctx, string haManifestUri, CancellationToken cancellationToken)
    {
        HaSummaryResult summary = await _evaluationService.GetHaSummaryAsync(haManifestUri, cancellationToken);
        if (summary.Summary is not null)
        {
            return Finished(ctx, BusinessCaseStages.Succeeded, null, null, null, summary.HttpStatus, null, null, summary.Summary,
                ReplyFormat.Reply(
                    "'Business case ready:' followed by the annual savings and payback period of the strategy with the highest " +
                    "savings from 'result', in 'currency', quoting the figures exactly.",
                    "the ROI and total Azure versus on-premises cost of that strategy from 'result', in 'currency'; do not list " +
                    "the other strategies.",
                    ReplyFormat.AfterBusinessCase), BusinessCaseStages.HaSkill);
        }

        if (IsTransient(summary.HttpStatus))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, summary.HttpStatus, null, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, null, null, "haResult", summary.HttpStatus, null, summary.Error, null,
            ReplyFormat.Failure("Business case evaluated, but its result could not be read.", RecreateNextStep));
    }

    private static BusinessCaseCommandResult? CheckEvaluation(Context ctx, EvaluationResult evaluation, string step)
    {
        bool succeeded = evaluation.HttpStatus is >= 200 and < 300
            && string.Equals(evaluation.Status, BusinessCaseStages.Succeeded, StringComparison.OrdinalIgnoreCase);
        if (succeeded && AssessmentValidation.IsValidInventorySasUri(evaluation.ManifestUri))
        {
            return null;
        }

        if (IsTransient(evaluation.HttpStatus) || (evaluation.HttpStatus is >= 200 and < 300 && !AssessmentValidation.IsTerminal(evaluation.Status)))
        {
            return Running(ctx, BusinessCaseStages.Assessment, null, null, evaluation.HttpStatus, evaluation.RetryAfterSeconds, AssessmentRunningText);
        }

        return Finished(ctx, BusinessCaseStages.Failed, null, null, step, evaluation.HttpStatus, evaluation.ErrorCode,
            evaluation.ErrorMessage ?? (succeeded ? "The response did not include a valid manifest URL." : null), null,
            ReplyFormat.Failure("Business case evaluation failed (stage 1 of 2).", RecreateNextStep));
    }

    private static BusinessCaseCommandResult Running(
        Context ctx, string stage, string? dsSessionId, string? dsOperationId, int? httpStatus, int? retryAfterSeconds, string instructions) =>
        new(true, null, BusinessCaseStages.Running, false, stage, BusinessCaseStages.StageNumber(stage), BusinessCaseStages.Total,
            ctx.AssessmentName, ctx.AsOperationId.ToString("D"), dsSessionId, dsOperationId, null, httpStatus, null, null, null,
            retryAfterSeconds ?? DefaultRetryAfterSeconds,
            BusinessCaseNextStatusParameters.Create(ctx.AssessmentName, ctx.AsOperationId, ctx.InventorySasUri, ctx.Settings, dsSessionId, dsOperationId),
            instructions + " " + BusinessCaseStages.TrackingInstructions);

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
