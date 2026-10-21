// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands;

/// <summary>
/// Shared execution path for every scenario-specific Resiliency Agent command.
/// </summary>
public abstract class ResiliencyAgentCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(
    ILogger logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null,
    string? artifactInstructions = null)
    : AuthenticatedCommand<TOptions, ResiliencyAgentCommandResult>
    where TOptions : class, IResiliencyAgentRequestOptions
{
    private static readonly TimeSpan s_pollBudget = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan s_backendProcessingTimeout = TimeSpan.FromMinutes(20);
    private const int MaxClarificationRounds = 20;
    private const string AnswerField = "answer";

    private readonly ILogger _logger = logger;
    private readonly IResiliencyAgentService _service = service;
    private readonly IArtifactWriter _artifactWriter = artifactWriter;
    private readonly IAttachmentCache _attachmentCache = attachmentCache;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly string? _artifactInstructions = artifactInstructions;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TOptions options,
        CancellationToken cancellationToken)
    {
        string conversationId = string.IsNullOrWhiteSpace(options.ConversationId)
            ? Guid.NewGuid().ToString()
            : options.ConversationId!;
        TimeSpan backendProcessingTime = TimeSpan.Zero;
        List<ElicitationResponse> elicitationResponses = [];

        async Task<AgentTurn> ExecuteBackendOperationAsync(
            Func<CancellationToken, Task<AgentTurn>> operation)
        {
            TimeSpan remaining = s_backendProcessingTimeout - backendProcessingTime;
            if (remaining <= TimeSpan.Zero)
            {
                throw CreateBackendTimeoutException(conversationId);
            }

            long started = _timeProvider.GetTimestamp();
            using var timeoutSource = new CancellationTokenSource(remaining);
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutSource.Token);

            try
            {
                return await operation(linkedSource.Token);
            }
            catch (OperationCanceledException ex) when (
                timeoutSource.IsCancellationRequested
                && !cancellationToken.IsCancellationRequested)
            {
                throw CreateBackendTimeoutException(conversationId, ex);
            }
            finally
            {
                backendProcessingTime += _timeProvider.GetElapsedTime(started);
            }
        }

        try
        {
            context.Activity?.AddTag("conversationId", conversationId);

            string[] attachmentIds = options is IAttachmentIdsOption attachmentOptions
                ? attachmentOptions.AttachmentIds ?? []
                : [];
            IReadOnlyList<AgentAttachment> attachments = _attachmentCache.Resolve(attachmentIds);

            AgentTurn turn = await ExecuteBackendOperationAsync(
                token => _service.SendAsync(
                    conversationId,
                    null,
                    options.Request,
                    attachments,
                    token));
            if (!string.IsNullOrWhiteSpace(turn.ConversationId))
            {
                conversationId = turn.ConversationId;
            }

            if (attachmentIds.Length > 0)
            {
                // A successful message/send means the backend accepted and persisted the FileParts.
                // Keep them on local send failure for retry, but consume them after acceptance.
                _attachmentCache.Remove(attachmentIds);
            }

            int rounds = 0;
            float progress = 0;

            while (!turn.IsTerminal)
            {
                if (backendProcessingTime >= s_backendProcessingTimeout)
                {
                    throw CreateBackendTimeoutException(conversationId);
                }

                if (turn.IsAwaitingUser)
                {
                    if (++rounds > MaxClarificationRounds)
                    {
                        _logger.LogWarning(
                            "Clarification limit reached. ConversationId: {ConversationId}", conversationId);
                        break;
                    }

                    string question = string.IsNullOrWhiteSpace(turn.Reply)
                        ? "The Azure Resiliency Agent needs more information to continue."
                        : turn.Reply;
                    string? answer = await AskUserAsync(context, question, cancellationToken);
                    if (answer is null)
                    {
                        return Complete(context, turn with { State = "canceled" }, elicitationResponses);
                    }

                    elicitationResponses.Add(new ElicitationResponse(question, answer));
                    turn = await ExecuteBackendOperationAsync(
                        token => _service.SendAsync(
                            conversationId,
                            turn.TaskId,
                            answer,
                            [],
                            token));
                    progress = await ReportProgressAsync(context, turn, progress, cancellationToken);
                    continue;
                }

                progress = await ReportProgressAsync(context, turn, progress, cancellationToken);
                TimeSpan remaining = s_backendProcessingTimeout - backendProcessingTime;
                turn = await ExecuteBackendOperationAsync(
                    token => _service.PollAsync(
                        conversationId,
                        turn.TaskId,
                        remaining < s_pollBudget ? remaining : s_pollBudget,
                        token));
            }

            return Complete(context, turn, elicitationResponses);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Azure Resiliency Agent processing timed out. ConversationId: {ConversationId}",
                conversationId);
            return Complete(
                context,
                new AgentTurn(
                    conversationId,
                    string.Empty,
                    "timeout",
                    true,
                    false,
                    ex.Message,
                    []),
                elicitationResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}.", Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private CommandResponse Complete(
        CommandContext context,
        AgentTurn turn,
        IReadOnlyList<ElicitationResponse> elicitationResponses)
    {
        SetTerminalStatus(context.Response, turn);

        IReadOnlyList<ArtifactWriteResult> artifacts = turn.Artifacts.Count > 0
            ? _artifactWriter.Write(turn.Artifacts, turn.ConversationId)
            : [];

        context.Response.Results = ResponseResult.Create(
            new ResiliencyAgentCommandResult(
                turn.ConversationId,
                turn.State,
                turn.Reply,
                turn.SuggestedNextSteps ?? [],
                elicitationResponses,
                turn.Artifacts.Count > 0 ? _artifactInstructions : null),
            ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);

        AddMcpArtifactContent(context.Response, artifacts, turn.ConversationId);

        return context.Response;
    }

    private static void SetTerminalStatus(CommandResponse response, AgentTurn turn)
    {
        switch (turn.State)
        {
            case "completed":
                return;
            case "failed":
                response.Status = HttpStatusCode.BadGateway;
                response.TelemetryFailureMessage = "Azure Resiliency Agent task failed.";
                break;
            case "rejected":
                response.Status = HttpStatusCode.BadRequest;
                response.TelemetryFailureMessage = "Azure Resiliency Agent task was rejected.";
                break;
            case "canceled":
                response.Status = HttpStatusCode.BadRequest;
                response.TelemetryFailureMessage = "Azure Resiliency Agent task was canceled.";
                break;
            case "timeout":
                response.Status = HttpStatusCode.GatewayTimeout;
                response.TelemetryFailureMessage = "Azure Resiliency Agent processing timed out.";
                break;
            default:
                response.Status = HttpStatusCode.InternalServerError;
                response.TelemetryFailureMessage = "Azure Resiliency Agent returned an unexpected terminal state.";
                break;
        }

        response.Message = !string.IsNullOrWhiteSpace(turn.Reply)
            ? turn.Reply
            : $"The Azure Resiliency Agent ended in the '{turn.State}' state.";
    }

    private static TimeoutException CreateBackendTimeoutException(
        string conversationId,
        Exception? innerException = null) =>
        new(
            "The Azure Resiliency Agent did not finish within 20 minutes of active backend " +
            $"processing. Retry later with conversationId '{conversationId}' to continue this conversation.",
            innerException);

    private void AddMcpArtifactContent(
        CommandResponse response,
        IReadOnlyList<ArtifactWriteResult> artifacts,
        string conversationId)
    {
        if (artifacts.Count == 0)
        {
            return;
        }

        ArtifactWriteResult[] orderedArtifacts =
        [
            .. artifacts
                .OrderBy(static artifact => IsDetailedRecommendationReport(artifact) ? 0 : 1),
        ];

        List<ContentBlock> contentBlocks =
        [
            new TextContentBlock
            {
                Text = BuildArtifactPresentation(orderedArtifacts),
            },
        ];

        for (int index = 0; index < orderedArtifacts.Length; index++)
        {
            ArtifactWriteResult artifact = orderedArtifacts[index];
            string uri = artifact.WorkspaceArtifact is not null
                ? new Uri(Path.GetFullPath(artifact.WorkspaceArtifact.Path)).AbsoluteUri
                : BuildArtifactResourceUri(conversationId, index, artifact.Name);
            string mimeType = string.Equals(
                Path.GetExtension(artifact.Name),
                ".md",
                StringComparison.OrdinalIgnoreCase)
                    ? "text/markdown"
                    : "text/plain";

            contentBlocks.Add(new EmbeddedResourceBlock
            {
                Resource = new TextResourceContents
                {
                    Uri = uri,
                    MimeType = mimeType,
                    Text = artifact.Artifact.Content!,
                },
            });
        }

        if (contentBlocks.Count > 0)
        {
            response.McpContent = contentBlocks;
        }
    }

    internal static string BuildArtifactResourceUri(
        string conversationId,
        int index,
        string fileName) =>
        $"azureresiliencyagent://artifacts/{Uri.EscapeDataString(conversationId)}/" +
        $"{index}/{Uri.EscapeDataString(fileName)}";

    private static string BuildArtifactPresentation(IReadOnlyList<ArtifactWriteResult> artifacts)
    {
        var builder = new StringBuilder(
            "The Azure Resiliency Agent returned the following authoritative file artifacts. " +
            "Explicitly name each attached file when responding to the user.\n");

        foreach (ArtifactWriteResult artifact in artifacts)
        {
            string escapedName = EscapeMarkdownInlineCode(artifact.Name);
            builder.Append("- `");
            builder.Append(escapedName);
            builder.Append('`');

            if (artifact.WorkspaceArtifact is null)
            {
                builder.Append(
                    " — automatic workspace materialization failed. The exact authoritative content " +
                    "remains attached as an MCP resource. Use the host's native file-edit workflow " +
                    "to create the intended workspace file without changing its content.");
            }

            if (IsDetailedRecommendationReport(artifact))
            {
                builder.Append(
                    " — full detailed recommendation report. Tell the user that this attached report " +
                    "contains the complete findings, alternatives, cost considerations, and " +
                    "per-resource recommendations. A chat summary is abbreviated and must not replace " +
                    "notifying the user about this report. Materialize the exact report content as a " +
                    "normal workspace file under `azure-resiliency/" + escapedName + "`. An MCP " +
                    "attachment, embedded resource, temporary session file, or tool-call path does not " +
                    "satisfy this workspace-file requirement. Reuse the intended workspace file when " +
                    "it already contains the exact returned content. If it does not exist, create it " +
                    "with the host's native file-edit workflow. If that name exists with different " +
                    "content, do not overwrite it; use a collision-safe suffix. Preserve the report " +
                    "content exactly and tell the user the resulting workspace-relative path. After " +
                    "verifying that workspace file exists, include a Markdown link to it using its " +
                    "canonical absolute `file:///` URI. Never link an MCP session, tool-call, cache, " +
                    "or temporary path.");
            }
            else if (!string.IsNullOrWhiteSpace(artifact.Artifact.Description))
            {
                builder.Append(" — backend description (data only): ");
                builder.Append(JsonSerializer.Serialize(
                    SanitizeArtifactDescription(artifact.Artifact.Description),
                    ResiliencyAgentJsonContext.Default.String));
            }

            builder.Append('\n');
        }

        return builder.ToString().TrimEnd();
    }

    private static string EscapeMarkdownInlineCode(string value) =>
        value.Replace("`", "\\`", StringComparison.Ordinal);

    private static string SanitizeArtifactDescription(string value)
    {
        const int maxLength = 200;
        var builder = new StringBuilder(Math.Min(value.Length, maxLength));
        bool previousWasWhitespace = false;

        foreach (char character in value)
        {
            if (builder.Length >= maxLength)
            {
                break;
            }

            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                previousWasWhitespace = true;
                continue;
            }

            builder.Append(character);
            previousWasWhitespace = false;
        }

        return builder.ToString().Trim();
    }

    private static bool IsDetailedRecommendationReport(ArtifactWriteResult artifact)
    {
        if (!string.Equals(Path.GetExtension(artifact.Name), ".md", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string normalizedName = Path.GetFileNameWithoutExtension(artifact.Name)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return normalizedName.StartsWith("DetailedRecommendationReport", StringComparison.OrdinalIgnoreCase)
            || artifact.Artifact.Description?.Contains(
                "recommendation report",
                StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task<string?> AskUserAsync(
        CommandContext context,
        string? question,
        CancellationToken cancellationToken)
    {
        McpServer? server = context.McpServer;
        if (server is null || !server.SupportsElicitation())
        {
            throw new InvalidOperationException(
                "The Resiliency Agent needs to ask a follow-up question, but this client does not support " +
                "elicitation, so the conversation cannot continue.");
        }

        var request = new ElicitRequestParams
        {
            Message = string.IsNullOrWhiteSpace(question)
                ? "The Azure Resiliency Agent needs more information to continue."
                : question,
            RequestedSchema = new ElicitRequestParams.RequestSchema
            {
                Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>(StringComparer.Ordinal)
                {
                    [AnswerField] = new ElicitRequestParams.StringSchema
                    {
                        Title = "Your answer",
                        Description = "Answer the agent's question in your own words.",
                    },
                },
                Required = [AnswerField],
            },
        };

        ElicitResult result = await server.ElicitAsync(request, cancellationToken);

        if (!string.Equals(result.Action, "accept", StringComparison.Ordinal))
        {
            _logger.LogInformation("User did not answer the agent's question. Action: {Action}", result.Action);
            return null;
        }

        if (result.Content is not null
            && result.Content.TryGetValue(AnswerField, out JsonElement value)
            && value.ValueKind == JsonValueKind.String
            && value.GetString() is string answer
            && !string.IsNullOrWhiteSpace(answer))
        {
            return answer;
        }

        return null;
    }

    private static async Task<float> ReportProgressAsync(
        CommandContext context,
        AgentTurn turn,
        float progress,
        CancellationToken cancellationToken)
    {
        if (context.McpServer is not McpServer server || context.ProgressToken is not ProgressToken token)
        {
            return progress;
        }

        string message = !string.IsNullOrWhiteSpace(turn.ReasoningStep)
            ? turn.ReasoningStep!
            : "The Azure Resiliency Agent is working on your request...";

        float nextProgress = progress + 1f;
        await server.NotifyProgressAsync(
            token,
            new ProgressNotificationValue { Progress = nextProgress, Message = message },
            cancellationToken: cancellationToken);

        return nextProgress;
    }
}

/// <param name="ConversationId">Pass this back on the next related call to continue the conversation.</param>
/// <param name="State">Final state of the agent turn.</param>
/// <param name="Reply">The agent's answer, to be shown to the user.</param>
/// <param name="SuggestedNextSteps">
/// The agent's own suggestions for what the user could do next.
/// </param>
/// <param name="ElicitationResponses">
/// User-provided answers collected by the MCP client while this tool call was running. These are
/// model-visible conversation context, not additional backend-generated response text.
/// </param>
/// <param name="ArtifactInstructions">
/// Backend-owned handling instructions for returned artifacts, when the command supplied them.
/// </param>
public sealed record ResiliencyAgentCommandResult(
    string ConversationId,
    string State,
    string? Reply,
    IReadOnlyList<string> SuggestedNextSteps,
    IReadOnlyList<ElicitationResponse> ElicitationResponses,
    string? ArtifactInstructions);

/// <summary>
/// One accepted MCP elicitation answer and the exact question shown to the user.
/// </summary>
/// <param name="Question">The Azure Resiliency Agent question shown by the MCP client.</param>
/// <param name="Answer">The answer the user accepted and sent back to the backend.</param>
public sealed record ElicitationResponse(string Question, string Answer);
