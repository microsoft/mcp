// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Insights.Options;
using Azure.Mcp.Tools.Insights.Services;
using Azure.Mcp.Tools.Insights.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using ModelContextProtocol;

namespace Azure.Mcp.Tools.Insights.Commands;

[CommandMetadata(
    Id = "8d6ac0a4-1b3e-4d2c-8d2a-3a8c1c52cf94",
    Name = "get",
    Title = "Get Azure Infrastructure Insights",
    Description = """
        Derives insights from an existing Azure environment by analyzing patterns in
        Azure Resource Graph data. The caller can specify a tenant or subscription scope and
        optionally provide a free-form query describing their intent, which tailors the
        insights to that scenario.

        This command can only be used if the client supports MCP sampling.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = true,
    LocalRequired = false)]
public sealed partial class InsightsGetCommand(
    ILogger<InsightsGetCommand> logger,
    IInsightsService insightsService,
    ISamplingService samplingService,
    ISubscriptionService subscriptionService)
    : GlobalCommand<InsightsGetOptions>()
{
    private readonly ILogger<InsightsGetCommand> _logger = logger;

    private readonly IInsightsService _insightsService = insightsService;

    private readonly ISamplingService _samplingService = samplingService;

    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    private const int SamplingMaxTokens = 20000;

    private const int MaxQueryLength = 1000;

    private static readonly TimeSpan SamplingTimeout = TimeSpan.FromSeconds(300);

    private const string SystemPrompt = """
        # Role and Objective
        You are an expert Azure Insight Agent. Analyze the user's existing infrastructure and produce insights that inform downstream infrastructure plan generation.

        # Process
        1. Read the property aggregations of the user's existing infrastructure from Azure Resource Graph.
        2. Derive insights from dominant patterns in the user's existing infrastructure.
        3. Review the insights for completeness and accuracy; improve any that fall short.

        # Insight Guidelines
        When selecting resource properties to base insights on:
        - Only consider properties that represent explicit user decisions affecting design.
        - Never include runtime, versions, implementation details, app settings, default values, operational settings, or boilerplate configurations.
        - Never include instance-specific properties of a resource.
        - Focus on meaningful property areas like region, resource pairing, security posture, cost, naming and tagging conventions, and policies.

        Each insight is a structured object with the fields below.

        | Field | Required | Description |
        |---|---|---|
        | `id` | yes | Stable identifier, format `insight-NNN` (e.g. `insight-001`). |
        | `pattern` | yes | The factual pattern from the data, with counts or percentages. |
        | `implication` | yes | Concrete planning action that reflects the user's existing convention. |

        # Output

        Return the final insights using the schema below.

        ```json
        [
          {
            "id": "insight-001",
            "pattern": "96.1% (558 of 580) of resources reporting properties.minimumTlsVersion are pinned to TLS1_2",
            "implication": "New TLS-capable resources in the plan should set minimumTlsVersion to TLS1_2 to match the tenant convention."
          },
          {
            "id": "insight-002",
            "pattern": "89.5% (1,154 of 1,289) of resources reporting properties.publicNetworkAccess have it Enabled",
            "implication": "The tenant default for publicNetworkAccess is Enabled."
          }
        ]
        ```
        """;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.Subscription);
        command.Options.Add(InsightsOptionDefinitions.Query.AsOptional());
        command.Options.Add(InsightsOptionDefinitions.NoCache.AsOptional());
        command.Options.Add(InsightsOptionDefinitions.Scope.AsOptional());
    }

    protected override InsightsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var subscription = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription);

        if (!string.IsNullOrEmpty(subscription))
        {
            options.Subscription = subscription.Trim('"', '\'');
        }

        options.Query = SanitizeQuery(parseResult.GetValueOrDefault<string>(InsightsOptionDefinitions.Query));
        options.NoCache = parseResult.GetValueOrDefault<bool>(InsightsOptionDefinitions.NoCache);
        options.Scope = parseResult.GetValueOrDefault<string>(InsightsOptionDefinitions.Scope);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        if (context.McpServer is null || context.McpServer.ClientCapabilities?.Sampling is null)
        {
            // Note: CLI invocation doesn't support MCP sampling
            context.Response.Status = System.Net.HttpStatusCode.BadRequest;
            context.Response.Message = "Insights require an MCP client that supports sampling.";
            return context.Response;
        }

        // Reject overly long queries (already sanitized in BindOptions)
        if (options.Query is { Length: > MaxQueryLength })
        {
            context.Response.Status = System.Net.HttpStatusCode.BadRequest;
            context.Response.Message =
                $"--{InsightsOptionDefinitions.QueryName} length ({options.Query.Length}) " +
                $"exceeds the maximum allowed limit of {MaxQueryLength} characters.";
            return context.Response;
        }

        // Must have a valid scope (defaults to the subscription scope if not specified)
        var scope = (options.Scope ?? InsightsOptionDefinitions.ScopeSubscription).Trim().ToLowerInvariant();
        if (scope != InsightsOptionDefinitions.ScopeSubscription && scope != InsightsOptionDefinitions.ScopeTenant)
        {
            context.Response.Status = System.Net.HttpStatusCode.BadRequest;
            context.Response.Message =
                $"Invalid --{InsightsOptionDefinitions.ScopeName} value '{options.Scope}'. " +
                $"Allowed values: '{InsightsOptionDefinitions.ScopeSubscription}', '{InsightsOptionDefinitions.ScopeTenant}'.";
            return context.Response;
        }

        // Error if tenant scope is specified with an explicit subscription
        var explicitSubscription = !string.IsNullOrEmpty(options.Subscription);
        if (scope == InsightsOptionDefinitions.ScopeTenant && explicitSubscription)
        {
            context.Response.Status = System.Net.HttpStatusCode.BadRequest;
            context.Response.Message =
                $"--{OptionDefinitions.Common.Subscription.Name.TrimStart('-')} is not allowed when " +
                $"--{InsightsOptionDefinitions.ScopeName} is '{InsightsOptionDefinitions.ScopeTenant}'.";
            return context.Response;
        }

        // Use default subscription if not provided
        if (scope == InsightsOptionDefinitions.ScopeSubscription && !explicitSubscription)
        {
            options.Subscription = _subscriptionService.GetDefaultSubscriptionId();
            if (string.IsNullOrEmpty(options.Subscription))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message =
                    "No default subscription found. Pass --subscription, set an Azure CLI default " +
                    "(az account set --subscription <id>), or set AZURE_SUBSCRIPTION_ID.";
                return context.Response;
            }
        }

        try
        {
            IProgress<string> progress = new Progress<string>(msg => _ = NotifyProgressAsync(context, msg, cancellationToken));

            var aggregation = scope == InsightsOptionDefinitions.ScopeTenant
                ? await _insightsService.AggregateTenantAsync(options.Tenant, options.RetryPolicy, cancellationToken, progress, options.NoCache)
                : await _insightsService.AggregateSubscriptionAsync(options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken, progress, options.NoCache);

            progress.Report("Summarizing infrastructure patterns...");

            var payloadJson = BuildPayload(aggregation, options.Query);

            using var samplingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            samplingCts.CancelAfter(SamplingTimeout);

            var sampled = await _samplingService.SampleTextAsync(
                context.McpServer,
                SystemPrompt,
                payloadJson,
                SamplingMaxTokens,
                samplingCts.Token);

            var insights = ParseInsights(sampled);

            context.Response.Results = ResponseResult.Create(new InsightsGetCommandResult(insights), InsightsJsonContext.Default.InsightsGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deriving insights. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Builds the JSON payload sent to the LLM
    /// </summary>
    internal static string BuildPayload(SubscriptionAggregation aggregation, string? userQuery)
    {
        ArgumentNullException.ThrowIfNull(aggregation);

        var resourceTypes = new JsonObject();
        foreach (var kvp in aggregation.ResourceTypes.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            resourceTypes[kvp.Key] = new JsonObject
            {
                ["totalCount"] = kvp.Value.TotalCount,
                ["propertyAggregations"] = kvp.Value.PropertyAggregations.DeepClone(),
            };
        }

        var resourceContext = new JsonObject
        {
            ["subscriptionCount"] = aggregation.SubscriptionCount,
            ["resourceGroupCount"] = aggregation.ResourceGroupCount,
            ["resourceTypes"] = resourceTypes,
        };

        if (string.IsNullOrWhiteSpace(userQuery))
        {
            var root = new JsonObject
            {
                ["resourceContext"] = resourceContext,
            };
            return root.ToJsonString();
        }

        var ordered = new JsonObject
        {
            ["userQuery"] = userQuery.Trim(),
            ["resourceContext"] = resourceContext,
        };
        return ordered.ToJsonString();
    }

    /// <summary>
    /// Parses the LLM response based on the insights schema.
    /// Tolerates an optional <c>{ "insights": [...] }</c> wrapper and strips 
    /// markdown code fence if present. Insights with missing fields are skipped.
    /// </summary>
    internal static IReadOnlyList<InsightEntry> ParseInsights(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Sampling returned an empty response.");
        }

        var json = StripCodeFence(text.Trim());

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        JsonElement arr;
        if (root.ValueKind == JsonValueKind.Array)
        {
            arr = root;
        }
        else if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("insights", out var wrapped)
            && wrapped.ValueKind == JsonValueKind.Array)
        {
            arr = wrapped;
        }
        else
        {
            throw new InvalidOperationException("Sampling response was not a JSON array of insights.");
        }

        var list = new List<InsightEntry>(arr.GetArrayLength());
        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var id = GetStringProperty(item, "id");
            var pattern = GetStringProperty(item, "pattern");
            var implication = GetStringProperty(item, "implication");

            if (string.IsNullOrWhiteSpace(id)
                || string.IsNullOrWhiteSpace(pattern)
                || string.IsNullOrWhiteSpace(implication))
            {
                continue;
            }

            // Drop any insight that surfaced secret-shaped content
            if (ContainsSensitiveContent(pattern) || ContainsSensitiveContent(implication))
            {
                continue;
            }

            list.Add(new InsightEntry(id, pattern, implication));
        }
        return list;
    }

    private static string? GetStringProperty(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }
        return null;
    }

    /// <summary>
    /// Sends a progress notification to the client.
    /// </summary>
    private static async Task NotifyProgressAsync(CommandContext context, string message, CancellationToken cancellationToken)
    {
        if (context.McpServer is null || context.ProgressToken is null)
        {
            return;
        }

        await context.McpServer.NotifyProgressAsync(
            context.ProgressToken.Value,
            new ProgressNotificationValue
            {
                Progress = 0f,
                Message = message,
            },
            cancellationToken: cancellationToken);
    }

    private static string StripCodeFence(string text)
    {
        if (!text.StartsWith("```", StringComparison.Ordinal))
        {
            return text;
        }

        var newline = text.IndexOf('\n');
        if (newline < 0)
        {
            return text;
        }

        var inner = text[(newline + 1)..];
        if (inner.EndsWith("```", StringComparison.Ordinal))
        {
            inner = inner[..^3];
        }
        return inner.Trim();
    }

    private static string? SanitizeQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var sanitized = QueryWhitespaceRegex().Replace(query, " ").Trim();
        return sanitized.Length == 0 ? null : sanitized;
    }

    [GeneratedRegex(@"[\p{C}\s]+")]
    private static partial Regex QueryWhitespaceRegex();

    /// <summary>
    /// Returns <c>true</c> if the text contains secret-shaped content (connection strings,
    /// access keys, JWTs) that must not be surfaced in insights.
    /// </summary>
    private static bool ContainsSensitiveContent(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return text.Contains("AccountKey=", StringComparison.OrdinalIgnoreCase)
            || text.Contains("SharedAccessKey=", StringComparison.OrdinalIgnoreCase)
            || text.Contains("InstrumentationKey=", StringComparison.OrdinalIgnoreCase)
            || SensitiveContentRegex().IsMatch(text);
    }

    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]+\.|Server=.*;Password=", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveContentRegex();

    internal record InsightsGetCommandResult(IReadOnlyList<InsightEntry> Insights);

    internal record InsightEntry(string Id, string Pattern, string Implication);
}
