// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Identity;
using Azure.Mcp.Core;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// A2A JSON-RPC client for the Azure Resiliency Agent. Speaks the same <c>message/send</c> and
/// <c>tasks/get</c> surface Azure Portal Copilot uses, so this tool area adds a second caller rather
/// than a second agent.
/// </summary>
public sealed class ResiliencyAgentService : IResiliencyAgentService, IDisposable
{
    /// <summary>
    /// Overrides the A2A endpoint. Point this at a deployed environment to work against it instead of
    /// a service running on this machine.
    /// </summary>
    private const string EndpointEnvironmentVariable = "AZURE_RESILIENCY_AGENT_A2A_URL";

    /// <summary>
    /// Overrides the delegated scope requested for the endpoint. Set it to the empty string to call
    /// without a token, which is what a locally running service in its development configuration
    /// expects.
    /// </summary>
    private const string ScopeEnvironmentVariable = "AZURE_RESILIENCY_AGENT_SCOPE";

    /// <summary>
    /// The deployed Resiliency Agent endpoint.
    /// </summary>
    private const string DefaultEndpoint = "https://agent.public.resiliencemanagement.azure.com/a2a";

    /// <summary>
    /// Delegated scope for the Resiliency Agent application. The developer client signs the user in and
    /// acquires this token; the endpoint then exchanges it on-behalf-of for ARM, so the user's own RBAC
    /// applies throughout. This is the same scope Azure Portal Copilot requests in its manifest.
    /// </summary>
    private const string DefaultScope = "5c43d95c-7a1a-4860-8361-d86bb81364d0/.default";

    private static readonly TimeSpan s_pollInterval = TimeSpan.FromSeconds(2);

    private readonly ILogger<ResiliencyAgentService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IDataBoundaryResolver _dataBoundaryResolver;
    private readonly bool _sendDataBoundary;
    private readonly string _endpoint;

    [ActivatorUtilitiesConstructor]
    public ResiliencyAgentService(
        IAzureTokenCredentialProvider tokenCredentialProvider,
        IDataBoundaryResolver dataBoundaryResolver,
        ILogger<ResiliencyAgentService> logger)
    {
        _logger = logger;
        _dataBoundaryResolver = dataBoundaryResolver;
        _endpoint = ReadSetting(EndpointEnvironmentVariable, DefaultEndpoint);

        // The token is attached per request by the shared handler rather than cached here, so a
        // refreshed or re-scoped credential is picked up without restarting the server. An explicitly
        // empty scope disables it, which is how a locally running service - where authentication is
        // skipped in its development configuration - is reached.
        string scope = ResolveScope();

        if (scope.Length > 0)
        {
            _sendDataBoundary = true;
            _logger.LogDebug(
                "Calling the Resiliency Agent at {Endpoint} with a delegated token for scope {Scope}.",
                _endpoint,
                scope);

            var handler = new AccessTokenHandler(tokenCredentialProvider, [scope])
            {
                InnerHandler = new HttpClientHandler(),
            };

            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2) };
        }
        else
        {
            _sendDataBoundary = false;
            _logger.LogDebug(
                "Calling the Resiliency Agent at {Endpoint} without a delegated token, because {Variable} is set to an empty value.",
                _endpoint,
                ScopeEnvironmentVariable);

            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        }
    }

    public ResiliencyAgentService(
        HttpClient httpClient,
        IDataBoundaryResolver dataBoundaryResolver,
        ILogger<ResiliencyAgentService> logger,
        string endpoint,
        bool sendDataBoundary)
    {
        _httpClient = httpClient;
        _dataBoundaryResolver = dataBoundaryResolver;
        _logger = logger;
        _endpoint = endpoint;
        _sendDataBoundary = sendDataBoundary;
    }

    /// <summary>
    /// Resolves the delegated scope, distinguishing "not set" from "deliberately empty".
    /// </summary>
    private static string ResolveScope()
    {
        string? configured = Environment.GetEnvironmentVariable(ScopeEnvironmentVariable);

        // A set-but-empty value is a deliberate opt out of authentication; an unset value means take
        // the default. Treating both as "no token" would silently drop authentication whenever the
        // variable was simply absent.
        return configured is null ? DefaultScope : configured.Trim();
    }

    private static string ReadSetting(string variable, string fallback) =>
        Environment.GetEnvironmentVariable(variable) is string value && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;

    public async Task<AgentTurn> SendAsync(
        string conversationId,
        string? taskId,
        string text,
        IReadOnlyList<AgentAttachment> attachments,
        CancellationToken cancellationToken)
    {
        var parts = new JsonArray(
            new JsonObject
            {
                ["kind"] = "text",
                ["text"] = text,
            });

        foreach (AgentAttachment attachment in attachments)
        {
            parts.Add((JsonNode)new JsonObject
            {
                ["kind"] = "file",
                ["file"] = new JsonObject
                {
                    ["name"] = attachment.Name,
                    ["mimeType"] = attachment.MimeType,
                    ["bytes"] = Convert.ToBase64String(attachment.Content),
                },
            });
        }

        var message = new JsonObject
        {
            ["kind"] = "message",
            ["messageId"] = Guid.NewGuid().ToString(),
            ["role"] = "user",
            ["contextId"] = conversationId,
            ["parts"] = parts,
        };

        if (!string.IsNullOrWhiteSpace(taskId))
        {
            message["taskId"] = taskId;
        }

        using JsonDocument response = await SendRpcAsync(
            "message/send",
            new JsonObject { ["message"] = message },
            cancellationToken);

        AgentTurn turn = ReadTurn(GetResult(response), conversationId, taskId);

        _logger.LogInformation(
            "Resiliency turn sent. ConversationId: {ConversationId}, State: {State}",
            turn.ConversationId, turn.State);

        return turn;
    }

    public async Task<AgentTurn> PollAsync(
        string conversationId,
        string taskId,
        TimeSpan budget,
        CancellationToken cancellationToken)
    {
        DateTimeOffset deadline = DateTimeOffset.UtcNow.Add(budget);

        while (true)
        {
            using JsonDocument response = await SendRpcAsync(
                "tasks/get",
                new JsonObject { ["id"] = taskId },
                cancellationToken);

            AgentTurn turn = ReadTurn(GetResult(response), conversationId, taskId);

            if (turn.IsTerminal || turn.IsAwaitingUser || DateTimeOffset.UtcNow >= deadline)
            {
                return turn;
            }

            await Task.Delay(s_pollInterval, cancellationToken);
        }
    }

    private static AgentTurn ReadTurn(JsonElement result, string conversationId, string? taskId)
    {
        string state = result.TryGetProperty("status", out JsonElement status)
            ? ReadString(status, "state") ?? "unknown"
            : "unknown";

        string? reply = null;
        if (result.TryGetProperty("status", out JsonElement statusElement)
            && statusElement.TryGetProperty("message", out JsonElement statusMessage))
        {
            reply = ReadTextParts(statusMessage);
        }

        List<AgentArtifact> artifacts = [];
        if (result.TryGetProperty("artifacts", out JsonElement artifactArray)
            && artifactArray.ValueKind == JsonValueKind.Array)
        {
            // The agent deliberately emits the same generated files twice - once as downloadable file
            // parts and once inside a text envelope for inline rendering - so identical content is
            // collapsed here rather than handed to the caller twice.
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (JsonElement artifact in artifactArray.EnumerateArray())
            {
                foreach (AgentArtifact decoded in ReadArtifact(artifact))
                {
                    string key = decoded.Name + "\u0000" + (decoded.Content ?? string.Empty);
                    if (seen.Add(key))
                    {
                        artifacts.Add(decoded);
                    }
                }
            }
        }

        return new AgentTurn(
            ReadString(result, "contextId") ?? conversationId,
            ReadString(result, "id") ?? taskId ?? string.Empty,
            state,
            IsTerminal(state),
            IsAwaitingUser(state),
            reply,
            artifacts,
            ReadLatestReasoningStep(result),
            ReadSuggestedNextSteps(result));
    }

    /// <summary>
    /// Reads the agent's own suggestions for what the user could do next.
    /// </summary>
    /// <remarks>
    /// The backend emits these on the agent message only when it has given a definitive answer, and
    /// they are already phrased from the user's perspective - "Generate Bicep templates", "Add more
    /// resource types". Passing them on lets the caller offer the agent's actual next steps instead of
    /// inventing its own, which is how the same suggestions are surfaced in Azure Portal Copilot.
    /// </remarks>
    private static IReadOnlyList<string>? ReadSuggestedNextSteps(JsonElement result)
    {
        if (!result.TryGetProperty("status", out JsonElement status)
            || !status.TryGetProperty("message", out JsonElement message)
            || !message.TryGetProperty("metadata", out JsonElement metadata)
            || !metadata.TryGetProperty("suggestedPrompts", out JsonElement prompts)
            || prompts.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        List<string> steps = [];
        foreach (JsonElement prompt in prompts.EnumerateArray())
        {
            if (prompt.ValueKind == JsonValueKind.String
                && prompt.GetString() is string text
                && !string.IsNullOrWhiteSpace(text))
            {
                steps.Add(text);
            }
        }

        return steps.Count > 0 ? steps : null;
    }

    /// <summary>
    /// Reads the most recent reasoning step the agent has published for this task.
    /// </summary>
    /// <remarks>
    /// While a task is running the backend appends <c>{ id, text, header }</c> entries to
    /// <c>status.message.metadata.reasoningTrace</c>, and stops once the task is terminal. This is the
    /// same data the Azure Portal Copilot experience renders as progress, so surfacing it here keeps
    /// the two surfaces consistent and tells the user what the agent is actually doing.
    /// </remarks>
    private static string? ReadLatestReasoningStep(JsonElement result)
    {
        if (!result.TryGetProperty("status", out JsonElement status)
            || !status.TryGetProperty("message", out JsonElement message)
            || !message.TryGetProperty("metadata", out JsonElement metadata)
            || !metadata.TryGetProperty("reasoningTrace", out JsonElement trace)
            || trace.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        string? latest = null;
        foreach (JsonElement step in trace.EnumerateArray())
        {
            // The header is a short title such as "Assessing availability zone configuration"; the
            // text is the longer form. Prefer the title - progress is rendered on a single line.
            string? header = ReadString(step, "header") ?? ReadString(step, "Header");
            if (!string.IsNullOrWhiteSpace(header))
            {
                latest = header;
            }
        }

        return latest;
    }

    /// <summary>
    /// Single source of truth for which states end the loop. <c>rejected</c> is terminal, and
    /// <c>auth-required</c> needs the user rather than more waiting - a client that kept polling on
    /// either would never stop.
    /// </summary>
    private static bool IsTerminal(string state) =>
        state is "completed" or "failed" or "canceled" or "rejected";

    private static bool IsAwaitingUser(string state) =>
        state is "input-required" or "auth-required";

    /// <summary>
    /// Decodes one A2A artifact into readable parts. The agent emits reports as base64 file parts and
    /// generated templates as a text envelope of several files, so reading <c>text</c> naively returns
    /// nothing useful. Both shapes are unpacked here.
    /// </summary>
    internal static IEnumerable<AgentArtifact> ReadArtifact(JsonElement artifact)
    {
        string name = ReadString(artifact, "name") ?? "artifact";
        string? description = ReadString(artifact, "description");

        // The agent tags each artifact with the family it belongs to - arm, bicep, terraform - which is
        // more reliable than the media type and is what the caller needs in order to describe the files.
        string? format = artifact.TryGetProperty("metadata", out JsonElement artifactMetadata)
            ? ReadString(artifactMetadata, "type")
            : null;

        if (!artifact.TryGetProperty("parts", out JsonElement parts) || parts.ValueKind != JsonValueKind.Array)
        {
            return [new AgentArtifact(name, description, null, null, format)];
        }

        List<AgentArtifact> decoded = [];
        foreach (JsonElement part in parts.EnumerateArray())
        {
            switch (ReadString(part, "kind"))
            {
                case "file" when part.TryGetProperty("file", out JsonElement file):
                    decoded.Add(new AgentArtifact(
                        ReadString(file, "name") ?? name,
                        description,
                        ReadString(file, "mimeType") ?? "text/plain",
                        DecodeFile(file),
                        format));
                    break;

                case "text":
                    {
                        string text = ReadString(part, "text") ?? string.Empty;
                        List<AgentArtifact>? envelope = UnpackFileEnvelope(text, description, format);
                        if (envelope is not null)
                        {
                            decoded.AddRange(envelope);
                        }
                        else
                        {
                            decoded.Add(new AgentArtifact(name, description, "text/markdown", text, format));
                        }

                        break;
                    }

                case "data" when part.TryGetProperty("data", out JsonElement data):
                    decoded.Add(new AgentArtifact(
                        name,
                        description,
                        "text/markdown",
                        TranscodeDataToMarkdown(data),
                        format));
                    break;
            }
        }

        return decoded.Count > 0 ? decoded : [new AgentArtifact(name, description, null, null, format)];
    }

    /// <summary>
    /// Renders a structured data part as readable Markdown without discarding unfamiliar JSON.
    /// Uniform object arrays become tables; other structures remain inspectable as headings, bullets,
    /// scalar values, or raw nested JSON.
    /// </summary>
    private static string TranscodeDataToMarkdown(JsonElement data)
    {
        if (TryRenderZonalPosture(data, out string zonalPosture))
        {
            return zonalPosture;
        }

        if (data.ValueKind != JsonValueKind.Object)
        {
            var single = new StringBuilder();
            WriteElement(single, data, string.Empty);
            return single.ToString().TrimEnd();
        }

        StringBuilder builder = new();
        foreach (JsonProperty property in data.EnumerateObject())
        {
            builder.AppendLine($"## {property.Name}");
            builder.AppendLine();
            WriteElement(builder, property.Value, string.Empty);
            builder.AppendLine();
        }

        string markdown = builder.ToString().TrimEnd();
        return markdown.Length == 0 ? "_No data._" : markdown;
    }

    private static bool TryRenderZonalPosture(JsonElement data, out string markdown)
    {
        markdown = string.Empty;

        if (data.ValueKind != JsonValueKind.Object
            || !string.Equals(
                ReadString(data, "resiliencyAgentArtifactType"),
                "GetZonalPosture",
                StringComparison.OrdinalIgnoreCase)
            || !data.TryGetProperty("summary", out JsonElement summary)
            || summary.ValueKind != JsonValueKind.Object
            || !TryReadNumber(summary, "totalResources", out string totalResources)
            || !TryReadNumber(summary, "resilientResources", out string resilientResources)
            || !TryReadNumber(summary, "nonResilientResources", out string nonResilientResources)
            || !data.TryGetProperty("resources", out JsonElement resources)
            || resources.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        List<ZonalPostureResource> rows = [];
        foreach (JsonProperty resourceType in resources.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(resourceType.Name)
                || resourceType.Value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement resource in resourceType.Value.EnumerateArray())
            {
                if (resource.ValueKind != JsonValueKind.Object
                    || ReadString(resource, "resourceId") is not string resourceId
                    || string.IsNullOrWhiteSpace(resourceId)
                    || ReadString(resource, "status") is not string status
                    || string.IsNullOrWhiteSpace(status)
                    || !TryNormalizeArmResourceId(resourceId, out string normalizedId, out string label))
                {
                    return false;
                }

                rows.Add(new ZonalPostureResource(label, resourceType.Name, status, normalizedId));
            }
        }

        StringBuilder builder = new();
        builder.AppendLine("# Zonal Posture Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Total Resources | Resilient Resources | Non-Resilient Resources |");
        builder.AppendLine("|---|---|---|");
        builder.AppendLine($"| {totalResources} | {resilientResources} | {nonResilientResources} |");
        builder.AppendLine();
        builder.AppendLine("## Resources");
        builder.AppendLine();
        builder.AppendLine("| Resource | Resource Type | Status |");
        builder.AppendLine("|---|---|---|");

        foreach (ZonalPostureResource row in rows)
        {
            string portalUrl = BuildPortalResourceUrl(row.ResourceId);
            string resourceLink = $"[{EscapeMarkdownLinkLabel(row.Label)}]({portalUrl})";
            builder.AppendLine(
                $"| {resourceLink} | {EscapeMarkdownTableCell(row.ResourceType)} | " +
                $"{EscapeMarkdownTableCell(row.Status)} |");
        }

        markdown = builder.ToString().TrimEnd();
        return true;
    }

    private static bool TryReadNumber(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out JsonElement property)
            || property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        value = property.GetRawText();
        return true;
    }

    private static bool TryNormalizeArmResourceId(
        string resourceId,
        out string normalizedId,
        out string label)
    {
        normalizedId = string.Empty;
        label = string.Empty;

        string[] segments = resourceId
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
        {
            return false;
        }

        normalizedId = "/" + string.Join("/", segments);
        label = segments[^1];
        return !string.IsNullOrWhiteSpace(label);
    }

    private static string BuildPortalResourceUrl(string normalizedResourceId)
    {
        IEnumerable<string> escapedSegments = normalizedResourceId
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        return "https://portal.azure.com/#@/resource/" + string.Join("/", escapedSegments);
    }

    private static string EscapeMarkdownTableCell(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("|", "\\|")
            .Replace("`", "\\`")
            .Replace("*", "\\*")
            .Replace("_", "\\_")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("\r", string.Empty)
            .Replace("\n", " ");

    private static string EscapeMarkdownLinkLabel(string value) =>
        EscapeMarkdownTableCell(value)
            .Replace("(", "\\(")
            .Replace(")", "\\)");

    private sealed record ZonalPostureResource(
        string Label,
        string ResourceType,
        string Status,
        string ResourceId);

    private static void WriteElement(StringBuilder builder, JsonElement element, string prefix)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        builder.AppendLine($"{prefix}**{property.Name}**");
                        builder.AppendLine();
                        WriteElement(builder, property.Value, prefix);
                    }
                    else
                    {
                        builder.AppendLine($"{prefix}- **{property.Name}**: {Scalar(property.Value)}");
                    }
                }

                break;

            case JsonValueKind.Array:
                WriteArray(builder, element, prefix);
                break;

            default:
                builder.AppendLine($"{prefix}{Scalar(element)}");
                break;
        }
    }

    private static void WriteArray(StringBuilder builder, JsonElement array, string prefix)
    {
        List<JsonElement> rows = [.. array.EnumerateArray()];
        if (rows.Count == 0)
        {
            builder.AppendLine($"{prefix}_None._");
            return;
        }

        if (rows.TrueForAll(row => row.ValueKind == JsonValueKind.Object))
        {
            List<string> columns = [.. rows
                .SelectMany(row => row.EnumerateObject().Select(property => property.Name))
                .Distinct()];

            builder.AppendLine("| " + string.Join(" | ", columns) + " |");
            builder.AppendLine("|" + string.Concat(columns.Select(_ => "---|")));

            foreach (JsonElement row in rows)
            {
                IEnumerable<string> cells = columns.Select(column =>
                    row.TryGetProperty(column, out JsonElement value) ? Scalar(value) : string.Empty);
                builder.AppendLine("| " + string.Join(" | ", cells) + " |");
            }

            builder.AppendLine();
            return;
        }

        foreach (JsonElement row in rows)
        {
            builder.AppendLine($"{prefix}- {Scalar(row)}");
        }
    }

    private static string Scalar(JsonElement element)
    {
        string raw = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            JsonValueKind.Object or JsonValueKind.Array => element.GetRawText(),
            _ => element.ToString(),
        };

        string normalized = raw.Replace("|", "\\|").Replace("\r", string.Empty).Replace("\n", " ");
        return Uri.TryCreate(raw, UriKind.Absolute, out Uri? uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                ? $"[{normalized}]({raw})"
                : normalized;
    }

    private static string? DecodeFile(JsonElement file)
    {
        if (ReadString(file, "bytes") is string base64 && base64.Length > 0)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            }
            catch (FormatException)
            {
                // Not base64 after all - surface it as-is rather than losing the content.
                return base64;
            }
        }

        return ReadString(file, "uri");
    }

    /// <summary>
    /// Some generators emit <c>{"files":[{"file_name":...,"content":...}]}</c> as a single text part.
    /// Unpack it so each file arrives under its real name instead of one opaque blob.
    /// </summary>
    private static List<AgentArtifact>? UnpackFileEnvelope(string text, string? description, string? format)
    {
        if (!text.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            if (!document.RootElement.TryGetProperty("files", out JsonElement files)
                || files.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            List<AgentArtifact> unpacked = [];
            foreach (JsonElement file in files.EnumerateArray())
            {
                unpacked.Add(new AgentArtifact(
                    ReadString(file, "file_name") ?? "file",
                    description,
                    "text/plain",
                    ReadString(file, "content"),
                    format));
            }

            return unpacked.Count > 0 ? unpacked : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<JsonDocument> SendRpcAsync(
        string method,
        JsonObject parameters,
        CancellationToken cancellationToken)
    {
        var envelope = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = Guid.NewGuid().ToString(),
            ["method"] = method,
            ["params"] = parameters,
        };

        using var content = new StringContent(envelope.ToJsonString(), Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = content,
        };

        if (_sendDataBoundary)
        {
            string boundary = await _dataBoundaryResolver.ResolveAsync(cancellationToken);
            request.Headers.TryAddWithoutValidation("DataBoundary", boundary);
        }

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (AuthenticationFailedException ex)
        {
            // Raised by the credential rather than by the endpoint. Worth separating, because the
            // remedy is different: the caller has to sign in or select a different credential, and no
            // request reached the service at all.
            _logger.LogError(ex, "Could not acquire a delegated token for the Resiliency Agent.");

            throw new InvalidOperationException(
                "Could not acquire a token for the Azure Resiliency Agent. Sign in with a supported " +
                "Azure credential, or set AZURE_TOKEN_CREDENTIALS to select one explicitly. " +
                $"Details: {ex.Message}",
                ex);
        }

        using (httpResponse)
        {
            string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    // A token was obtained but the service would not accept it. The usual cause is the
                    // credential that won the chain not being one the service allows, so name it.
                    _logger.LogError(
                        "The Resiliency Agent rejected the request with {StatusCode}. Endpoint: {Endpoint}. Body: {Body}",
                        (int)httpResponse.StatusCode,
                        _endpoint,
                        Truncate(body));

                    throw new HttpRequestException(
                        $"The Azure Resiliency Agent rejected the request ({(int)httpResponse.StatusCode}). " +
                        "The token was acquired but the service did not accept the calling client. " +
                        "Set AZURE_TOKEN_CREDENTIALS to pin a supported credential and try again. " +
                        $"Body: {Truncate(body)}",
                        null,
                        httpResponse.StatusCode);
                }

                throw new HttpRequestException(
                    $"The Resiliency Agent returned {(int)httpResponse.StatusCode} for '{method}'. Body: {Truncate(body)}",
                    null,
                    httpResponse.StatusCode);
            }

            return JsonDocument.Parse(body);
        }
    }

    private static JsonElement GetResult(JsonDocument document)
    {
        if (document.RootElement.TryGetProperty("error", out JsonElement error))
        {
            throw new InvalidOperationException(
                $"The Resiliency Agent returned a JSON-RPC error: {ReadString(error, "message") ?? "unknown error"}");
        }

        if (!document.RootElement.TryGetProperty("result", out JsonElement result))
        {
            throw new InvalidOperationException("The Resiliency Agent response contained no 'result'.");
        }

        return result;
    }

    private static string? ReadString(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out JsonElement value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? ReadTextParts(JsonElement message)
    {
        if (!message.TryGetProperty("parts", out JsonElement parts) || parts.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (JsonElement part in parts.EnumerateArray())
        {
            if (ReadString(part, "text") is string text && !string.IsNullOrWhiteSpace(text))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(text);
            }
        }

        return builder.Length > 0 ? builder.ToString() : null;
    }

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500] + "...";

    public void Dispose() => _httpClient.Dispose();
}
