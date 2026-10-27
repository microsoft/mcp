// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed class PreValidationService(
    IAzureService azureService,
    IMonitorMetricsService monitorMetricsService,
    ILogger<PreValidationService> logger,
    TimeProvider? timeProvider = null)
    : BaseAzureResourceService(azureService), IPreValidationService
{
    private const int MaximumArmPages = 100;
    private const int MaximumMetricsLookbackDays = 93;
    private const int DeletionLookbackDays = 14;
    private const string DeletionChangeQuery = """
        resourcechanges
        | extend changeTime = todatetime(properties.changeAttributes.timestamp)
        | where changeTime >= ago(14d)
        | extend key = tolower(tostring(properties.targetResourceId))
        | where key in~ (<RESOURCE_IDS>) and tostring(properties.changeType) =~ 'Delete'
        | summarize deletedOn = max(changeTime) by key
        | project key, deletedOn
        """;
    private readonly AdvisorResourceGraphQueryExecutor _queryExecutor = new(azureService);
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<PreValidationResult> ValidateAsync(
        string recommendationTypeId,
        IReadOnlyList<Recommendation> candidates,
        string? tenant,
        bool sourceResultsTruncated,
        IReadOnlyList<string> unmatchedResources,
        int minimumUnusedDays,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(unmatchedResources);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumUnusedDays, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumUnusedDays, UnusedAgePolicy.MaximumMinimumUnusedDays);
        if (!Guid.TryParse(recommendationTypeId, out var typeId))
        {
            throw new ArgumentException("The recommendation type ID must be a GUID.", nameof(recommendationTypeId));
        }

        var groups = candidates.GroupBy(static candidate => candidate.Properties.ResourceMetadata?.ResourceId,
            StringComparer.OrdinalIgnoreCase).ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(groups.Length, RecommendationPreValidationCommand.MaximumResources);
        var identifiers = new Dictionary<string, ResourceIdentifier>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in groups)
        {
            if (string.IsNullOrWhiteSpace(group.Key) || !ResourceIdentifier.TryParse(group.Key, out var identifier) || identifier is null ||
                !Guid.TryParse(identifier.SubscriptionId, out _) || string.IsNullOrWhiteSpace(identifier.ResourceGroupName))
            {
                throw new JsonException("Advisor returned an invalid or mismatched affected-resource identity. No safety verdict was produced.");
            }
            if (group.Any(candidate =>
                !string.Equals(candidate.Properties.Category, "Cost", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(candidate.Properties.RecommendationStatus, "New", StringComparison.OrdinalIgnoreCase) ||
                !Guid.TryParse(candidate.Properties.RecommendationTypeId, out var candidateTypeId) || candidateTypeId != typeId))
            {
                throw new JsonException("Prevalidation requires current New Cost recommendations of the selected type.");
            }
            identifiers.Add(group.Key, identifier);
        }

        var now = _timeProvider.GetUtcNow();
        var checksByResource = identifiers.Keys.ToDictionary(
            static id => id, static _ => new List<PreValidationCheckResult>(), StringComparer.OrdinalIgnoreCase);
        var insights = identifiers.ToDictionary(static pair => pair.Key,
            pair => ResourceInsightsCatalog.Load(pair.Value.ResourceType.ToString(), typeId.ToString("D")),
            StringComparer.OrdinalIgnoreCase);
        foreach (var id in identifiers.Keys.Where(id => insights[id] is null))
        {
            checksByResource[id].Add(new("supported-resource-type", "Matching resource validation checks",
                "scope", "warn", "unverified",
                "No compiled resource-type checks match this resource and recommendation type ID. No automated safety verdict is available."));
        }
        var supportedIdentifiers = identifiers.Where(pair => insights[pair.Key] is not null)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DateTimeOffset> deletedResources = new(StringComparer.OrdinalIgnoreCase);
        if (supportedIdentifiers.Count > 0)
        {
            var resolved = await _queryExecutor.ResolveSubscriptionsScopeAsync(
                supportedIdentifiers.Values.Select(static identifier => identifier.SubscriptionId!).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                resourceGroup: null, tenant, cancellationToken);
            var tenantId = resolved.TenantResource.Data.TenantId?.ToString()
                ?? throw new InvalidOperationException("The selected resources did not resolve to an Azure tenant.");
            foreach (var resourceTypeGroup in supportedIdentifiers.GroupBy(
                static pair => pair.Value.ResourceType.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                var insight = insights[resourceTypeGroup.First().Key]!;
                var targets = resourceTypeGroup.ToDictionary(
                    static pair => pair.Key, static pair => pair.Value, StringComparer.OrdinalIgnoreCase);
                foreach (var check in insight.Checks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (check.Kind == "unusedAge")
                    {
                        var observations = targets.Keys.ToDictionary(
                            static id => id, static _ => new List<UnusedAgeObservation>(), StringComparer.OrdinalIgnoreCase);
                        foreach (var signal in check.Signals ?? [])
                        {
                            Dictionary<string, UnusedAgeObservation> evidence;
                            try
                            {
                                evidence = signal.Source switch
                                {
                                    "armChangedTime" => await ReadArmActivityAsync(targets, insight.ResourceType, tenantId, cancellationToken),
                                    "arg" => await ReadArgActivityAsync(signal, targets.Keys, resolved.TenantResource, cancellationToken),
                                    "metrics" => await ReadMetricActivityAsync(
                                        signal, targets, tenantId, now, minimumUnusedDays, cancellationToken),
                                    _ => throw new InvalidOperationException("Unsupported compiled unused-age signal."),
                                };
                            }
                            catch (Exception ex) when (IsReadFailure(ex))
                            {
                                logger.LogWarning(ex, "Could not read prevalidation activity signal {SignalSource}.", signal.Source);
                                evidence = [];
                            }
                            foreach (var id in targets.Keys)
                            {
                                observations[id].Add(evidence.GetValueOrDefault(id));
                            }
                        }
                        foreach (var id in targets.Keys)
                        {
                            checksByResource[id].Add(UnusedAgePolicy.Evaluate(observations[id], now, minimumUnusedDays));
                        }
                    }
                    else if (check.Kind == "locks")
                    {
                        var outcomes = await ReadLockChecksAsync(check, targets, tenantId, cancellationToken);
                        foreach (var id in targets.Keys)
                        {
                            checksByResource[id].Add(outcomes[id]);
                        }
                    }
                    else if (check.Kind == "manual")
                    {
                        foreach (var id in targets.Keys)
                        {
                            checksByResource[id].Add(new(check.Id, check.Name, check.Goal, check.Severity,
                                "manual", check.Interpretation,
                                Guidance: BindManualGuidance(check.Guidance, targets[id], insight.RecommendationTypeId)));
                        }
                    }
                    else if (check.Kind == "arg")
                    {
                        try
                        {
                            var rows = await ReadArgRowsAsync(check.Query, targets.Keys, resolved.TenantResource, cancellationToken);
                            foreach (var id in targets.Keys)
                            {
                                checksByResource[id].Add(EvaluateArgCheck(check, rows.GetValueOrDefault(id) ?? []));
                            }
                        }
                        catch (Exception ex) when (IsReadFailure(ex))
                        {
                            logger.LogWarning(ex, "Could not verify prevalidation check {CheckId}.", check.Id);
                            foreach (var id in targets.Keys)
                            {
                                checksByResource[id].Add(Unverified(check));
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported compiled prevalidation check.");
                    }
                }
            }
            // Only failed scope/detection checks can indicate a missing resource. Locks and other risks are not deletion evidence.
            var deletionCandidates = supportedIdentifiers.Keys.Where(id =>
                checksByResource[id].Any(static check => check.Goal == "scope" && check.Severity == "block" && check.Outcome == "failed")).ToArray();
            if (deletionCandidates.Length > 0)
            {
                deletedResources = await ReadDeletedResourcesAsync(
                    deletionCandidates, checksByResource, resolved.TenantResource, now, cancellationToken);
            }
        }

        var resources = groups.Select(group =>
        {
            var id = group.Key!;
            var checks = checksByResource[id];
            var candidate = group.First();
            var isDeleted = deletedResources.TryGetValue(id, out var deletedOn);
            return new PreValidationResourceResult(id, candidate.Properties.ImpactedValue, identifiers[id].ResourceType.ToString(),
                group.Select(static recommendation => recommendation.Name ?? recommendation.Id)
                    .OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                GetVerdict(checks), checks, isDeleted ? [] : insights[id]?.ManualConfirmations ?? [],
                ReadRetailValue(candidate.Properties.Savings, "yearlyPotentialSavings"),
                ReadRetailValue(candidate.Properties.Savings, "cost"),
                isDeleted ? deletedOn : null,
                isDeleted
                    ? $"This resource was already deleted on {deletedOn.UtcDateTime.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)}. " +
                        "No action is needed. Advisor will remove this recommendation automatically within 72 hours once it refreshes. " +
                        "Postpone, dismiss, and manual completion are unavailable for deleted resources."
                    : null,
                isDeleted ? false : null);
        }).ToArray();
        return BuildResult(typeId.ToString("D"), resources, now, minimumUnusedDays,
            sourceResultsTruncated, unmatchedResources);
    }

    private static bool IsReadFailure(Exception exception) =>
        exception is RequestFailedException or HttpRequestException or JsonException or TimeoutException;

    private async Task<Dictionary<string, DateTimeOffset>> ReadDeletedResourcesAsync(
        IReadOnlyList<string> resourceIds,
        IReadOnlyDictionary<string, List<PreValidationCheckResult>> checksByResource,
        TenantResource tenant, DateTimeOffset now, CancellationToken cancellationToken)
    {
        Dictionary<string, List<JsonElement>>? rows = null;
        var deleted = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);
        try
        {
            rows = await ReadArgRowsAsync(DeletionChangeQuery, resourceIds, tenant, cancellationToken);
            foreach (var (id, values) in rows)
            {
                if (values.Count != 1 || ReadTimestamp(values[0], "deletedOn") is not { } deletedOn ||
                    deletedOn < now.AddDays(-DeletionLookbackDays) || deletedOn > now)
                {
                    throw new JsonException("Resource deletion history contained an invalid or out-of-window deletion timestamp.");
                }
                deleted.Add(id, deletedOn);
            }
        }
        catch (Exception ex) when (IsReadFailure(ex))
        {
            logger.LogWarning(ex, "Could not confirm deleted resources from Resource Graph change history; original blockers are unchanged.");
            rows = null;
            deleted.Clear();
        }

        foreach (var id in resourceIds)
        {
            checksByResource[id].Add(new("deletion-status", "Confirm deletion from Resource Graph change history",
                "scope", "info", rows is null ? "unverified" : "info",
                rows is null
                    ? "Deletion history could not be verified. The original blocking checks remain applicable; missing resources are not proof of deletion."
                    : deleted.ContainsKey(id)
                        ? "A Delete event in the last 14 days confirms that this resource was already deleted."
                        : "No Delete event was found in the last 14 days. The resource is not confirmed deleted; original blockers are unchanged.",
                rows?.GetValueOrDefault(id)));
        }
        return deleted;
    }

    private static async Task<Dictionary<string, List<JsonElement>>> ReadArgRowsAsync(
        string? template, IEnumerable<string> resourceIds, TenantResource tenant, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        var ids = resourceIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var query = BindResourceIds(template, ids);
        var result = await AdvisorResourceGraphQueryExecutor.ExecutePagedAsync(tenant, query, cancellationToken);
        if (result.IsTruncated)
        {
            throw new JsonException("Prevalidation Resource Graph evidence was truncated.");
        }

        var rows = new Dictionary<string, List<JsonElement>>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in result.Rows)
        {
            if (row.ValueKind != JsonValueKind.Object ||
                !row.TryGetProperty("key", out var key) || key.ValueKind != JsonValueKind.String ||
                !ids.TryGetValue(key.GetString()!, out var id))
            {
                throw new JsonException("Prevalidation Resource Graph evidence contained an unexpected resource key.");
            }
            if (!rows.TryGetValue(id, out var values))
            {
                rows[id] = values = [];
            }
            values.Add(row);
        }
        return rows;
    }

    internal static string BindResourceIds(string template, IEnumerable<string> resourceIds) =>
        template.Replace("<RESOURCE_IDS>",
            string.Join(", ", resourceIds.Select(static id => $"'{RecommendationQueryBuilder.EscapeKqlString(id.ToLowerInvariant())}'")),
            StringComparison.Ordinal);

    internal static PreValidationCheckResult EvaluateArgCheck(ResourceInsightCheck check, IReadOnlyList<JsonElement> rows)
    {
        bool? passed = check.Expect switch
        {
            "rows" => rows.Count > 0,
            "noRows" => rows.Count == 0,
            "info" => true,
            "field" => CompareFields(check, rows),
            _ => throw new InvalidOperationException("Unsupported compiled prevalidation expectation."),
        };
        var outcome = check.Expect == "info" ? "info" : passed switch
        {
            true => "passed",
            null => "unverified",
            false when check.Severity == "block" => "failed",
            false when check.Severity == "warn" => "warning",
            _ => "info",
        };
        return new(check.Id, check.Name, check.Goal, check.Severity, outcome,
            $"{rows.Count} matching evidence row(s). {check.Interpretation}", rows);
    }

    private static bool? CompareFields(ResourceInsightCheck check, IReadOnlyList<JsonElement> rows)
    {
        if (rows.Count == 0 || check.Field is null)
        {
            return null;
        }
        var matched = true;
        foreach (var row in rows)
        {
            if (!row.TryGetProperty(check.Field, out var field))
            {
                return null;
            }
            if (check.ExpectedKind is JsonValueKind.True or JsonValueKind.False)
            {
                if (field.ValueKind is JsonValueKind.True or JsonValueKind.False)
                {
                    matched &= field.ValueKind == check.ExpectedKind;
                }
                // ARG also serializes calculated booleans as numeric 0/1; other numbers and strings remain unverified.
                else if (field.ValueKind == JsonValueKind.Number && field.TryGetInt32(out var boolean) && boolean is 0 or 1)
                {
                    matched &= (boolean == 1) == (check.ExpectedKind == JsonValueKind.True);
                }
                else
                {
                    return null;
                }
            }
            else if (check.ExpectedKind == JsonValueKind.String && field.ValueKind == JsonValueKind.String)
            {
                matched &= string.Equals(field.GetString(), check.ExpectedValue, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return null;
            }
        }
        return matched;
    }

    private static async Task<Dictionary<string, UnusedAgeObservation>> ReadArgActivityAsync(
        UnusedAgeSignal signal, IEnumerable<string> resourceIds, TenantResource tenant, CancellationToken cancellationToken)
    {
        var rows = await ReadArgRowsAsync(signal.Query, resourceIds, tenant, cancellationToken);
        return rows.ToDictionary(static pair => pair.Key, pair =>
        {
            var timestamps = pair.Value.Select(row => ReadTimestamp(row, signal.Field)).ToArray();
            var latest = timestamps.Max();
            return latest is null && signal.WindowDays is > 0
                ? new UnusedAgeObservation(QuietWindowDays: signal.WindowDays)
                : new UnusedAgeObservation(latest, IsIncomplete: timestamps.Any(static timestamp => timestamp is null));
        }, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, UnusedAgeObservation>> ReadArmActivityAsync(
        IReadOnlyDictionary<string, ResourceIdentifier> identifiers, string resourceType, string tenant,
        CancellationToken cancellationToken)
    {
        var evidence = new Dictionary<string, UnusedAgeObservation>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in identifiers.GroupBy(static pair =>
            $"/subscriptions/{pair.Value.SubscriptionId}/resourceGroups/{pair.Value.ResourceGroupName}",
            StringComparer.OrdinalIgnoreCase))
        {
            var wanted = group.Select(static pair => pair.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var identifier = group.First().Value;
            var resourceGroupPath = $"/subscriptions/{Uri.EscapeDataString(identifier.SubscriptionId!)}/resourceGroups/{Uri.EscapeDataString(identifier.ResourceGroupName!)}";
            var filter = Uri.EscapeDataString($"resourceType eq '{resourceType}'");
            try
            {
                await foreach (var row in ReadArmRowsAsync(
                    $"{resourceGroupPath}/resources?api-version=2021-04-01&$expand=createdTime,changedTime&$filter={filter}",
                    tenant, cancellationToken))
                {
                    if (row.TryGetProperty("id", out var idValue) && idValue.ValueKind == JsonValueKind.String &&
                        wanted.TryGetValue(idValue.GetString()!, out var id))
                    {
                        var timestamps = new[] { ReadTimestamp(row, "changedTime"), ReadTimestamp(row, "createdTime") };
                        evidence[id] = new(timestamps.Max());
                        wanted.Remove(id);
                        if (wanted.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) when (IsReadFailure(ex))
            {
                logger.LogWarning(ex, "Could not complete ARM activity reads for prevalidation.");
            }
        }
        return evidence;
    }

    private async Task<Dictionary<string, PreValidationCheckResult>> ReadLockChecksAsync(
        ResourceInsightCheck check, IReadOnlyDictionary<string, ResourceIdentifier> identifiers, string tenant,
        CancellationToken cancellationToken)
    {
        var outcomes = identifiers.Keys.ToDictionary(static id => id, _ => Unverified(check), StringComparer.OrdinalIgnoreCase);
        foreach (var group in identifiers.GroupBy(static pair => pair.Value.SubscriptionId!, StringComparer.OrdinalIgnoreCase))
        {
            var found = new Dictionary<string, List<JsonElement>>(StringComparer.OrdinalIgnoreCase);
            var complete = false;
            try
            {
                await foreach (var row in ReadArmRowsAsync(
                    $"/subscriptions/{Uri.EscapeDataString(group.Key)}/providers/Microsoft.Authorization/locks?api-version=2016-09-01",
                    tenant, cancellationToken))
                {
                    if (!row.TryGetProperty("id", out var idValue) || idValue.ValueKind != JsonValueKind.String ||
                        !row.TryGetProperty("properties", out var properties) ||
                        properties.ValueKind != JsonValueKind.Object ||
                        !properties.TryGetProperty("level", out var level) || level.ValueKind != JsonValueKind.String ||
                        level.GetString() is not ("CanNotDelete" or "ReadOnly"))
                    {
                        throw new JsonException("Management lock evidence was incomplete.");
                    }
                    var lockId = idValue.GetString()!;
                    var marker = lockId.LastIndexOf("/providers/Microsoft.Authorization/locks/", StringComparison.OrdinalIgnoreCase);
                    if (marker <= 0 ||
                        !lockId.StartsWith($"/subscriptions/{group.Key}/", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new JsonException("Management lock evidence contained an invalid scope.");
                    }
                    var scope = lockId[..marker].TrimEnd('/');
                    foreach (var resource in group)
                    {
                        if (resource.Key.Equals(scope, StringComparison.OrdinalIgnoreCase) ||
                            resource.Key.StartsWith(scope + "/", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!found.TryGetValue(resource.Key, out var locks))
                            {
                                found[resource.Key] = locks = [];
                            }
                            locks.Add(row);
                        }
                    }
                }
                complete = true;
            }
            catch (Exception ex) when (IsReadFailure(ex))
            {
                logger.LogWarning(ex, "Could not complete management lock inventory for prevalidation.");
            }
            foreach (var resource in group)
            {
                var locks = found.GetValueOrDefault(resource.Key);
                outcomes[resource.Key] = locks is { Count: > 0 }
                    ? new(check.Id, check.Name, check.Goal, "block", "failed", "An effective management lock prevents deletion.", locks)
                    : complete
                        ? new(check.Id, check.Name, check.Goal, "block", "passed", "No effective resource, parent-resource, resource-group, or subscription management locks were found.")
                        : Unverified(check);
            }
        }
        return outcomes;
    }

    private async IAsyncEnumerable<JsonElement> ReadArmRowsAsync(
        string path, string tenant, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var endpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint;
        var original = new Uri(endpoint, path);
        AzureService.ValidateAzureServiceEndpoint(original.AbsoluteUri, "arm");
        var token = await GetArmAccessTokenAsync(tenant, cancellationToken);
        using var client = AzureService.GetClient();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        Uri? next = original;
        for (var page = 0; next is not null; page++)
        {
            if (page >= MaximumArmPages || !seen.Add(next.AbsoluteUri))
            {
                throw new JsonException("ARM prevalidation evidence was incomplete because pagination did not finish.");
            }
            // An allowed Azure host alone does not authorize forwarding a bearer token to another collection.
            if (next.Scheme != original.Scheme || next.Authority != original.Authority ||
                !next.AbsolutePath.Equals(original.AbsolutePath, StringComparison.OrdinalIgnoreCase) ||
                next.UserInfo.Length != 0 || next.Fragment.Length != 0)
            {
                throw new SecurityException("ARM prevalidation pagination left the original collection.");
            }
            AzureService.ValidateAzureServiceEndpoint(next.AbsoluteUri, "arm");
            using var request = new HttpRequestMessage(HttpMethod.Get, next);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("value", out var rows) || rows.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("ARM prevalidation evidence did not contain a valid value array.");
            }
            foreach (var row in rows.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("ARM prevalidation evidence contained an invalid resource.");
                }
                yield return row.Clone();
            }
            next = null;
            if (document.RootElement.TryGetProperty("nextLink", out var nextLink) && nextLink.ValueKind != JsonValueKind.Null)
            {
                if (nextLink.ValueKind != JsonValueKind.String)
                {
                    throw new JsonException("ARM prevalidation pagination returned an invalid next link.");
                }
                if (!string.IsNullOrWhiteSpace(nextLink.GetString()))
                {
                    if (!Uri.TryCreate(endpoint, nextLink.GetString(), out next))
                    {
                        throw new JsonException("ARM prevalidation pagination returned an invalid next link.");
                    }
                }
            }
        }
    }

    private async Task<Dictionary<string, UnusedAgeObservation>> ReadMetricActivityAsync(
        UnusedAgeSignal signal, IReadOnlyDictionary<string, ResourceIdentifier> identifiers, string tenant,
        DateTimeOffset now, int minimumUnusedDays, CancellationToken cancellationToken)
    {
        if (signal.MetricNames is not { Count: > 0 })
        {
            throw new InvalidOperationException("Compiled metric activity signals must specify metrics.");
        }
        // Monitor rounds returned timestamps to minutes. Align the requested window without relaxing daily coverage checks.
        var end = new DateTimeOffset(now.UtcTicks - now.UtcTicks % TimeSpan.TicksPerMinute, TimeSpan.Zero);
        var start = end.AddDays(-Math.Min(minimumUnusedDays + 1, MaximumMetricsLookbackDays));
        var results = new System.Collections.Concurrent.ConcurrentDictionary<string, UnusedAgeObservation>(StringComparer.OrdinalIgnoreCase);
        await Parallel.ForEachAsync(identifiers, new ParallelOptions
        {
            MaxDegreeOfParallelism = 4,
            CancellationToken = cancellationToken,
        }, async (resource, token) =>
        {
            try
            {
                var metrics = await monitorMetricsService.QueryMetricsAsync(
                    resource.Value.SubscriptionId!, resource.Value.ResourceGroupName,
                    resource.Value.ResourceType.ToString(), resource.Key, resource.Value.ResourceType.ToString(),
                    signal.MetricNames, start.ToString("O", CultureInfo.InvariantCulture), end.ToString("O", CultureInfo.InvariantCulture),
                    interval: "P1D", aggregation: signal.Aggregation, tenant: tenant, cancellationToken: token);
                results[resource.Key] = EvaluateMetricActivity(signal, metrics, start, end);
            }
            catch (Exception ex) when (IsReadFailure(ex))
            {
                logger.LogWarning(ex, "Could not read Azure Monitor activity evidence for prevalidation.");
                results[resource.Key] = new(IsIncomplete: true);
            }
        });
        return new(results, StringComparer.OrdinalIgnoreCase);
    }

    internal static UnusedAgeObservation EvaluateMetricActivity(
        UnusedAgeSignal signal, IReadOnlyList<MetricResult> metrics, DateTimeOffset start, DateTimeOffset end)
    {
        if (signal.MetricNames is not { Count: > 0 })
        {
            throw new InvalidOperationException("Compiled metric activity signals must specify metrics.");
        }
        var incomplete = false;
        DateTimeOffset? latest = null;
        var quietDays = (int)Math.Floor((end - start).TotalDays);
        foreach (var name in signal.MetricNames)
        {
            var matches = metrics.Where(metric => metric.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1 || matches[0].TimeSeries.Count == 0)
            {
                incomplete = true;
                continue;
            }
            foreach (var series in matches[0].TimeSeries)
            {
                var values = signal.Aggregation.ToLowerInvariant() switch
                {
                    "total" => series.TotalBuckets,
                    "maximum" => series.MaxBuckets,
                    "average" => series.AvgBuckets,
                    "minimum" => series.MinBuckets,
                    "count" => series.CountBuckets,
                    _ => throw new InvalidOperationException("Unsupported compiled metric aggregation."),
                };
                var seriesStart = new DateTimeOffset(DateTime.SpecifyKind(series.Start, DateTimeKind.Utc));
                var seriesEnd = new DateTimeOffset(DateTime.SpecifyKind(series.End, DateTimeKind.Utc));
                // Monitor's compact arrays omit null points. Require a complete daily grid before mapping indexes to timestamps.
                if (series.Interval != "P1D" || seriesEnd < seriesStart ||
                    seriesStart > start.AddDays(1) || seriesEnd < end.AddDays(-1) || seriesEnd > end ||
                    values is not { Length: > 0 } ||
                    values.Length != (int)Math.Floor((seriesEnd - seriesStart).TotalDays) + 1 ||
                    values.Any(static value => !double.IsFinite(value) || value < 0))
                {
                    incomplete = true;
                    continue;
                }
                for (var index = 0; index < values.Length; index++)
                {
                    if (values[index] > signal.ActivityThreshold)
                    {
                        var activity = seriesStart.AddDays(index + 1);
                        activity = activity > end ? end : activity;
                        latest = latest is null || activity > latest ? activity : latest;
                    }
                    quietDays = Math.Min(quietDays, (int)Math.Floor((end - seriesStart).TotalDays));
                }
            }
        }
        return new(latest, latest is null ? quietDays : null, incomplete);
    }

    internal static string GetVerdict(IReadOnlyCollection<PreValidationCheckResult> checks) =>
        checks.Any(static check => check.Severity == "block" && check.Outcome == "failed") ? "blocked" :
        checks.Count == 0 || checks.Any(static check =>
            (check.Severity is "block" or "warn") && (check.Outcome is "unverified" or "warning" or "failed")) ? "needsReview" : "safe";

    private static PreValidationCheckResult Unverified(ResourceInsightCheck check) =>
        new(check.Id, check.Name, check.Goal, check.Severity, "unverified",
            $"Could not verify this check; no pass or blocker was inferred. {check.Interpretation}");

    private static DateTimeOffset? ReadTimestamp(JsonElement row, string property) =>
        row.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String &&
        DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp)
            ? timestamp : null;

    private static decimal? ReadRetailValue(JsonElement? savings, string property) =>
        savings is { ValueKind: JsonValueKind.Object } value &&
        value.TryGetProperty("retail", out var retail) && retail.ValueKind == JsonValueKind.Object &&
        retail.TryGetProperty(property, out var amount) && amount.ValueKind == JsonValueKind.Number &&
        amount.TryGetDecimal(out var result) ? result : null;

    private static string? BindManualGuidance(string? guidance, ResourceIdentifier resource, string typeId) =>
        guidance?.Replace("<RESOURCE_ID>", resource.ToString(), StringComparison.Ordinal)
            .Replace("<RESOURCE_GROUP>", resource.ResourceGroupName, StringComparison.Ordinal)
            .Replace("<REC_TYPE_ID>", typeId, StringComparison.Ordinal);

    internal static PreValidationResult BuildResult(
        string typeId, IReadOnlyList<PreValidationResourceResult> resources, DateTimeOffset now,
        int minimumUnusedDays, bool truncated, IReadOnlyList<string> unmatchedResources)
    {
        var safe = resources.Where(static resource => resource.Verdict == "safe").ToArray();
        var review = resources.Where(static resource => resource.Verdict == "needsReview").ToArray();
        var blocked = resources.Where(static resource => resource.Verdict == "blocked").ToArray();
        var verdict = blocked.Length > 0 ? "blocked" :
            resources.Count == 0 || review.Length > 0 || truncated || unmatchedResources.Count > 0 ? "needsReview" : "safe";
        var summary = new[] { ("safe", safe), ("needsReview", review), ("blocked", blocked) }
            .Select(static bucket => new PreValidationBucketSummary(bucket.Item1, bucket.Item2.Length,
                SumKnown(bucket.Item2.Select(static resource => resource.YearlyPotentialSavings)),
                SumKnown(bucket.Item2.Select(static resource => resource.Cost)),
                bucket.Item2.Count(static resource => resource.YearlyPotentialSavings is null),
                bucket.Item2.Count(static resource => resource.Cost is null),
                bucket.Item2.Count(static resource => resource.ResourceDeletedOn is not null))).ToArray();
        return new(typeId, verdict, now, minimumUnusedDays, RecommendationQueryBuilder.RetailPricingBasis, "USD",
            truncated, unmatchedResources, safe, review, blocked, summary,
            resources.Count == 0
                ? "No matching current New Cost/IdleResources resources were validated. No deletion safety was established."
                : "Read-only, point-in-time automated checks only; no deletion or state update is authorized. " +
                    "Complete manual confirmations before any separately approved action. Dependency scans cover visible ARM properties only; " +
                    "RBAC-hidden, external, data-plane, and recently changed dependencies may be absent from Resource Graph. " +
                    "Missing pricing remains unavailable; amounts are Retail USD, not contractual savings.");
    }

    private static decimal? SumKnown(IEnumerable<decimal?> values)
    {
        var known = values.OfType<decimal>().ToArray();
        return known.Length == 0 ? null : known.Sum();
    }
}
