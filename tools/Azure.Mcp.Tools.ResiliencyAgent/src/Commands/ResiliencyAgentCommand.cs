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
    IAttachmentCache attachmentCache)
    : AuthenticatedCommand<TOptions, ResiliencyAgentCommandResult>
    where TOptions : class, IResiliencyAgentRequestOptions
{
    private static readonly TimeSpan s_pollBudget = TimeSpan.FromSeconds(5);
    private const int MaxClarificationRounds = 20;
    private const string AnswerField = "answer";

    private readonly ILogger _logger = logger;
    private readonly IResiliencyAgentService _service = service;
    private readonly IArtifactWriter _artifactWriter = artifactWriter;
    private readonly IAttachmentCache _attachmentCache = attachmentCache;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            string conversationId = string.IsNullOrWhiteSpace(options.ConversationId)
                ? Guid.NewGuid().ToString()
                : options.ConversationId!;

            context.Activity?.AddTag("conversationId", conversationId);

            string[] attachmentIds = options is IAttachmentIdsOption attachmentOptions
                ? attachmentOptions.AttachmentIds ?? []
                : [];
            IReadOnlyList<AgentAttachment> attachments = _attachmentCache.Resolve(attachmentIds);

            AgentTurn turn = await _service.SendAsync(
                conversationId,
                null,
                options.Request,
                attachments,
                cancellationToken);
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
                if (turn.IsAwaitingUser)
                {
                    if (++rounds > MaxClarificationRounds)
                    {
                        _logger.LogWarning(
                            "Clarification limit reached. ConversationId: {ConversationId}", conversationId);
                        break;
                    }

                    string? answer = await AskUserAsync(context, turn.Reply, cancellationToken);
                    if (answer is null)
                    {
                        return Complete(context, turn with { State = "canceled" });
                    }

                    turn = await _service.SendAsync(
                        conversationId,
                        turn.TaskId,
                        answer,
                        [],
                        cancellationToken);
                    progress = await ReportProgressAsync(context, turn, progress, cancellationToken);
                    continue;
                }

                progress = await ReportProgressAsync(context, turn, progress, cancellationToken);
                turn = await _service.PollAsync(
                    conversationId, turn.TaskId, s_pollBudget, cancellationToken);
            }

            return Complete(context, turn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}.", Name);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private CommandResponse Complete(CommandContext context, AgentTurn turn)
    {
        SetTerminalStatus(context.Response, turn);

        IReadOnlyList<WrittenArtifact> written = turn.Artifacts.Count > 0
            ? _artifactWriter.Write(turn.Artifacts, turn.ConversationId)
            : [];

        context.Response.Results = ResponseResult.Create(
            new ResiliencyAgentCommandResult(
                turn.ConversationId,
                turn.State,
                turn.Reply,
                turn.SuggestedNextSteps ?? []),
            ResiliencyAgentJsonContext.Default.ResiliencyAgentCommandResult);

        AddMcpArtifactContent(context.Response, written);

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
            default:
                response.Status = HttpStatusCode.InternalServerError;
                response.TelemetryFailureMessage = "Azure Resiliency Agent returned an unexpected terminal state.";
                break;
        }

        response.Message = !string.IsNullOrWhiteSpace(turn.Reply)
            ? turn.Reply
            : $"The Azure Resiliency Agent ended in the '{turn.State}' state.";
    }

    private void AddMcpArtifactContent(
        CommandResponse response,
        IReadOnlyList<WrittenArtifact> writtenArtifacts)
    {
        if (writtenArtifacts.Count == 0)
        {
            return;
        }

        WrittenArtifact[] orderedArtifacts =
        [
            .. writtenArtifacts
                .OrderBy(static artifact => IsDetailedRecommendationReport(artifact) ? 0 : 1),
        ];

        List<ContentBlock> contentBlocks =
        [
            new TextContentBlock
            {
                Text = BuildArtifactPresentation(orderedArtifacts),
            },
        ];

        foreach (WrittenArtifact artifact in orderedArtifacts)
        {
            try
            {
                string text = System.IO.File.ReadAllText(artifact.Path);
                string uri = new Uri(artifact.Path).AbsoluteUri;
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
                        Text = text,
                    },
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _logger.LogWarning(
                    ex,
                    "Could not create MCP resource blocks for artifact {ArtifactPath}.",
                    artifact.Path);
            }
        }

        if (contentBlocks.Count > 0)
        {
            response.McpContent = contentBlocks;
        }
    }

    private static string BuildArtifactPresentation(IReadOnlyList<WrittenArtifact> artifacts)
    {
        var builder = new StringBuilder(
            "The Azure Resiliency Agent returned the following authoritative file artifacts. " +
            "Explicitly name each attached file when responding to the user.\n");

        foreach (WrittenArtifact artifact in artifacts)
        {
            builder.Append("- `");
            builder.Append(artifact.Name);
            builder.Append('`');

            if (IsDetailedRecommendationReport(artifact))
            {
                builder.Append(
                    " — full detailed recommendation report. Tell the user that this attached report " +
                    "contains the complete findings, alternatives, cost considerations, and " +
                    "per-resource recommendations. A chat summary is abbreviated and must not replace " +
                    "notifying the user about this report. Materialize the exact report content as a " +
                    "normal workspace file under `azure-resiliency/" + artifact.Name + "`. An MCP " +
                    "attachment, embedded resource, temporary session file, or tool-call path does not " +
                    "satisfy this workspace-file requirement. Reuse the intended workspace file when " +
                    "it already contains the exact returned content. If it does not exist, create it " +
                    "with the host's native file-edit workflow. If that name exists with different " +
                    "content, do not overwrite it; use a collision-safe suffix. Preserve the report " +
                    "content exactly and tell the user the resulting workspace-relative path.");
            }
            else if (!string.IsNullOrWhiteSpace(artifact.Description))
            {
                builder.Append(" — ");
                builder.Append(artifact.Description);
            }

            builder.Append('\n');
        }

        return builder.ToString().TrimEnd();
    }

    private static bool IsDetailedRecommendationReport(WrittenArtifact artifact)
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
            || artifact.Description?.Contains(
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
public sealed record ResiliencyAgentCommandResult(
    string ConversationId,
    string State,
    string? Reply,
    IReadOnlyList<string> SuggestedNextSteps);
