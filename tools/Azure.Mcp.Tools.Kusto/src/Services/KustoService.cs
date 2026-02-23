// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Globalization;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Kusto.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Kusto.Services;


public sealed class KustoService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    IHttpClientFactory httpClientFactory,
    ILogger<KustoService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IKustoService
{
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<KustoService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string CacheGroup = "kusto";
    private const string KustoClustersCacheKey = "clusters";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan s_providerCacheDuration = TimeSpan.FromHours(2);

    // Provider cache key generator
    private static string GetProviderCacheKey(string clusterUri, string? tenant)
    {
        var tenantKey = string.IsNullOrEmpty(tenant) ? "default" : tenant;
        return $"{tenantKey}:{clusterUri}";
    }

    public async Task<ResourceQueryResults<string>> ListClustersAsync(
        string subscriptionId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        try
        {
            var clusters = await ExecuteResourceQueryAsync(
                "Microsoft.Kusto/clusters",
                resourceGroup: null, // all resource groups
                subscriptionId,
                retryPolicy,
                item => ConvertToClusterModel(item).ClusterName,
                cancellationToken: cancellationToken);

            return clusters;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Kusto clusters: {ex.Message}", ex);
        }
    }

    public async Task<KustoClusterModel> GetClusterAsync(
            string subscriptionId,
            string clusterName,
            string? tenant = null,
            RetryPolicyOptions? retryPolicy = null,
            CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscriptionId), subscriptionId));

        try
        {
            var cluster = await ExecuteSingleResourceQueryAsync(
                        "Microsoft.Kusto/clusters",
                        resourceGroup: null, // all resource groups
                        subscription: subscriptionId,
                        retryPolicy: retryPolicy,
                        converter: ConvertToClusterModel,
                        additionalFilter: $"name =~ '{EscapeKqlString(clusterName)}'",
                        cancellationToken: cancellationToken);

            if (cluster == null)
            {
                throw new KeyNotFoundException($"Kusto cluster '{clusterName}' not found for subscription '{subscriptionId}'.");
            }
            return cluster;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Kusto cluster '{ClusterName}' for subscription '{Subscription}'",
                clusterName, subscriptionId);
            throw;
        }
    }

    public async Task<List<string>> ListDatabasesAsync(
        string subscriptionId,
        string clusterName,
        string? tenant = null,
        AuthMethod? authMethod =
        AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListDatabasesAsync(clusterUri, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<List<string>> ListDatabasesAsync(
        string clusterUri,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(clusterUri), clusterUri));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken).ConfigureAwait(false);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            "NetDefaultDB",
            ".show databases | project DatabaseName",
            cancellationToken);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<List<string>> ListTablesAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await ListTablesAsync(clusterUri, databaseName, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<List<string>> ListTablesAsync(
        string clusterUri,
        string databaseName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(clusterUri), clusterUri), (nameof(databaseName), databaseName));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken);
        var kustoResult = await kustoClient.ExecuteControlCommandAsync(
            databaseName,
            ".show tables",
            cancellationToken);
        return KustoResultToStringList(kustoResult);
    }

    public async Task<string> GetTableSchemaAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await GetTableSchemaAsync(clusterUri, databaseName, tableName, tenant, authMethod, retryPolicy, cancellationToken);
    }

    public async Task<string> GetTableSchemaAsync(
        string clusterUri,
        string databaseName,
        string tableName,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(clusterUri), clusterUri),
            (nameof(databaseName), databaseName),
            (nameof(tableName), tableName));

        var kustoClient = await GetOrCreateKustoClientAsync(clusterUri, tenant, cancellationToken);
        var kustoResult = await kustoClient.ExecuteQueryCommandAsync(
            databaseName,
            $".show table {tableName} cslschema", cancellationToken);
        var result = KustoResultToStringList(kustoResult);
        if (result.Count > 0)
        {
            return string.Join(Environment.NewLine, result);
        }
        throw new Exception($"No schema found for table '{tableName}' in database '{databaseName}'.");
    }

    public async Task<List<JsonElement>> QueryItemsAsync(
            string subscriptionId,
            string clusterName,
            string databaseName,
            string query,
            string? tenant = null,
            AuthMethod? authMethod = AuthMethod.Credential,
            RetryPolicyOptions? retryPolicy = null,
            CancellationToken cancellationToken = default)
    {
        var result = await QueryItemsWithStatisticsAsync(
            subscriptionId,
            clusterName,
            databaseName,
            query,
            showStats: false,
            tenant,
            authMethod,
            retryPolicy,
            cancellationToken);

        return result.Items;
    }

    public async Task<(List<JsonElement> Items, JsonElement? Statistics)> QueryItemsWithStatisticsAsync(
        string subscriptionId,
        string clusterName,
        string databaseName,
        string query,
        bool showStats = false,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscriptionId), subscriptionId),
            (nameof(clusterName), clusterName),
            (nameof(databaseName), databaseName),
            (nameof(query), query));

        string clusterUri = await GetClusterUriAsync(subscriptionId, clusterName, tenant, retryPolicy);
        return await QueryItemsWithStatisticsAsync(
            clusterUri,
            databaseName,
            query,
            showStats,
            tenant,
            authMethod,
            retryPolicy,
            cancellationToken);
    }

    public async Task<List<JsonElement>> QueryItemsAsync(
        string clusterUri,
        string databaseName,
        string query,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var result = await QueryItemsWithStatisticsAsync(
            clusterUri,
            databaseName,
            query,
            showStats: false,
            tenant,
            authMethod,
            retryPolicy,
            cancellationToken);

        return result.Items;
    }

    public async Task<(List<JsonElement> Items, JsonElement? Statistics)> QueryItemsWithStatisticsAsync(
        string clusterUri,
        string databaseName,
        string query,
        bool showStats = false,
        string? tenant = null,
        AuthMethod? authMethod = AuthMethod.Credential,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(clusterUri), clusterUri),
            (nameof(databaseName), databaseName),
            (nameof(query), query));

        var cslQueryProvider = await GetOrCreateCslQueryProviderAsync(clusterUri, tenant, cancellationToken);
        var kustoResult = await cslQueryProvider.ExecuteQueryCommandAsync(databaseName, query, cancellationToken);
        if (kustoResult.RootElement.ValueKind == JsonValueKind.Null)
        {
            return ([], null);
        }

        var root = kustoResult.RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Kusto query response was not in expected v2 frame format.");
        }

        var hasPrimaryResult = false;
        var hasQueryCompletionInformation = false;
        var hasErrors = false;
        JsonElement primaryResultFrame = default;
        JsonElement queryCompletionInformationFrame = default;
        string? oneApiErrors = null;

        foreach (var frame in root.EnumerateArray())
        {
            if (frame.ValueKind != JsonValueKind.Object
                || !frame.TryGetProperty("FrameType", out var frameTypeElement)
                || frameTypeElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var frameType = frameTypeElement.GetString();
            if (string.Equals(frameType, "DataSetCompletion", StringComparison.OrdinalIgnoreCase))
            {
                if (frame.TryGetProperty("HasErrors", out var hasErrorsElement)
                    && TryGetBooleanValue(hasErrorsElement, out var frameHasErrors)
                    && frameHasErrors)
                {
                    hasErrors = true;
                    if (frame.TryGetProperty("OneApiErrors", out var oneApiErrorsElement))
                    {
                        oneApiErrors = oneApiErrorsElement.GetRawText();
                    }
                }

                continue;
            }

            if (!string.Equals(frameType, "DataTable", StringComparison.OrdinalIgnoreCase)
                || !frame.TryGetProperty("TableKind", out var tableKindElement)
                || tableKindElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var tableKind = tableKindElement.GetString();
            if (!hasPrimaryResult && string.Equals(tableKind, "PrimaryResult", StringComparison.OrdinalIgnoreCase))
            {
                hasPrimaryResult = true;
                primaryResultFrame = frame;
                continue;
            }

            if (showStats && !hasQueryCompletionInformation
                && string.Equals(tableKind, "QueryCompletionInformation", StringComparison.OrdinalIgnoreCase))
            {
                hasQueryCompletionInformation = true;
                queryCompletionInformationFrame = frame;
            }
        }

        if (hasErrors)
        {
            throw new InvalidOperationException($"Kusto query completed with errors: {oneApiErrors ?? "No OneApiErrors details were returned."}");
        }

        if (!hasPrimaryResult)
        {
            throw new InvalidOperationException("Kusto query response did not contain a PrimaryResult frame.");
        }

        var items = ConvertPrimaryResultToItems(primaryResultFrame);
        JsonElement? statistics = null;
        if (showStats && hasQueryCompletionInformation)
        {
            try
            {
                statistics = ParseStatistics(queryCompletionInformationFrame);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Kusto query statistics.");
            }
        }

        return (items, statistics);
    }

    private static List<JsonElement> ConvertPrimaryResultToItems(JsonElement primaryResultFrame)
    {
        var result = new List<JsonElement>();
        if (!primaryResultFrame.TryGetProperty("Columns", out var columnsElement)
            || columnsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        var columnsDict = columnsElement.EnumerateArray()
            .Where(column => column.TryGetProperty("ColumnName", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
            .ToDictionary(
                column => column.GetProperty("ColumnName").GetString()!,
                column =>
                {
                    if (!column.TryGetProperty("ColumnType", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
                    {
                        return string.Empty;
                    }

                    return typeElement.GetString() ?? string.Empty;
                });

        var columnsBuffer = new ArrayBufferWriter<byte>();
        using (var columnsWriter = new Utf8JsonWriter(columnsBuffer))
        {
            columnsWriter.WriteStartObject();
            foreach (var column in columnsDict)
            {
                columnsWriter.WriteString(column.Key, column.Value);
            }

            columnsWriter.WriteEndObject();
        }

        using (var jsonDoc = JsonDocument.Parse(columnsBuffer.WrittenMemory))
        {
            result.Add(jsonDoc.RootElement.Clone());
        }

        if (!primaryResultFrame.TryGetProperty("Rows", out var rowsElement)
            || rowsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var row in rowsElement.EnumerateArray())
        {
            result.Add(row.Clone());
        }

        return result;
    }

    private static JsonElement? ParseStatistics(JsonElement queryCompletionInformationFrame)
    {
        if (!queryCompletionInformationFrame.TryGetProperty("Columns", out var columnsElement)
            || columnsElement.ValueKind != JsonValueKind.Array
            || !queryCompletionInformationFrame.TryGetProperty("Rows", out var rowsElement)
            || rowsElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var severityNameColumnIndex = GetColumnIndex(columnsElement, "SeverityName");
        var statusDescriptionColumnIndex = GetColumnIndex(columnsElement, "StatusDescription");
        if (severityNameColumnIndex < 0 || statusDescriptionColumnIndex < 0)
        {
            return null;
        }

        foreach (var row in rowsElement.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var severityName = GetRowStringValue(row, severityNameColumnIndex);
            if (!string.Equals(severityName, "Stats", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var statusDescription = GetRowStringValue(row, statusDescriptionColumnIndex);
            if (string.IsNullOrWhiteSpace(statusDescription))
            {
                return null;
            }

            using var statsDocument = JsonDocument.Parse(statusDescription);
            return ConvertStatisticsToResult(statsDocument.RootElement);
        }

        return null;
    }

    private static JsonElement? ConvertStatisticsToResult(JsonElement source)
    {
        var result = new JsonObject();
        if (TryGetDoubleProperty(source, "ExecutionTime", out var executionTime))
        {
            result["execution_time_sec"] = executionTime;
        }

        if (source.TryGetProperty("resource_usage", out var resourceUsage)
            && resourceUsage.ValueKind == JsonValueKind.Object)
        {
            AddCpuStats(result, resourceUsage);
            AddMemoryStats(result, resourceUsage);
            AddCacheStats(result, resourceUsage);
            AddNetworkStats(result, resourceUsage);
        }

        AddInputDatasetStats(result, source);
        AddResultDatasetStats(result, source);
        AddCrossClusterBreakdown(result, source);

        if (result.Count == 0)
        {
            return null;
        }

        using var document = JsonDocument.Parse(result.ToJsonString());
        return document.RootElement.Clone();
    }

    private static void AddCpuStats(JsonObject result, JsonElement resourceUsage)
    {
        if (!resourceUsage.TryGetProperty("cpu", out var cpu) || cpu.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var cpuResult = new JsonObject();
        if (TryGetStringProperty(cpu, "total cpu", out var totalCpu))
        {
            cpuResult["total"] = totalCpu;
        }

        if (cpu.TryGetProperty("breakdown", out var breakdown) && breakdown.ValueKind == JsonValueKind.Object)
        {
            if (TryGetStringProperty(breakdown, "query execution", out var queryExecution))
            {
                cpuResult["query_execution"] = queryExecution;
            }

            if (TryGetStringProperty(breakdown, "query planning", out var queryPlanning))
            {
                cpuResult["query_planning"] = queryPlanning;
            }
        }

        if (cpuResult.Count > 0)
        {
            result["cpu"] = cpuResult;
        }
    }

    private static void AddMemoryStats(JsonObject result, JsonElement resourceUsage)
    {
        if (!resourceUsage.TryGetProperty("memory", out var memory)
            || memory.ValueKind != JsonValueKind.Object
            || !TryGetDoubleProperty(memory, "peak_per_node", out var peakPerNodeBytes))
        {
            return;
        }

        result["memory_peak_per_node_mb"] = BytesToMegabytes(peakPerNodeBytes);
    }

    private static void AddCacheStats(JsonObject result, JsonElement resourceUsage)
    {
        if (!resourceUsage.TryGetProperty("cache", out var cache)
            || cache.ValueKind != JsonValueKind.Object
            || !cache.TryGetProperty("shards", out var shards)
            || shards.ValueKind != JsonValueKind.Object
            || !shards.TryGetProperty("hot", out var hot)
            || hot.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var cacheResult = new JsonObject();
        if (TryGetDoubleProperty(hot, "hitbytes", out var hitBytes))
        {
            cacheResult["hot_hit_mb"] = BytesToMegabytes(hitBytes);
        }

        if (TryGetDoubleProperty(hot, "missbytes", out var missBytes))
        {
            cacheResult["hot_miss_mb"] = BytesToMegabytes(missBytes);
        }

        if (cacheResult.Count > 0)
        {
            result["cache"] = cacheResult;
        }
    }

    private static void AddInputDatasetStats(JsonObject result, JsonElement source)
    {
        if (!source.TryGetProperty("input_dataset_statistics", out var inputDatasetStatistics)
            || inputDatasetStatistics.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (inputDatasetStatistics.TryGetProperty("extents", out var extents)
            && extents.ValueKind == JsonValueKind.Object)
        {
            var extentsResult = new JsonObject();
            if (TryGetLongProperty(extents, "scanned", out var scannedExtents))
            {
                extentsResult["scanned"] = scannedExtents;
            }

            if (TryGetLongProperty(extents, "total", out var totalExtents))
            {
                extentsResult["total"] = totalExtents;
            }

            if (extentsResult.Count > 0)
            {
                result["extents"] = extentsResult;
            }
        }

        if (inputDatasetStatistics.TryGetProperty("rows", out var rows)
            && rows.ValueKind == JsonValueKind.Object)
        {
            var rowsResult = new JsonObject();
            if (TryGetLongProperty(rows, "scanned", out var scannedRows))
            {
                rowsResult["scanned"] = scannedRows;
            }

            if (TryGetLongProperty(rows, "total", out var totalRows))
            {
                rowsResult["total"] = totalRows;
            }

            if (rowsResult.Count > 0)
            {
                result["rows"] = rowsResult;
            }
        }
    }

    private static void AddResultDatasetStats(JsonObject result, JsonElement source)
    {
        if (!source.TryGetProperty("dataset_statistics", out var datasetStatistics)
            || datasetStatistics.ValueKind != JsonValueKind.Array
            || datasetStatistics.GetArrayLength() == 0)
        {
            return;
        }

        var firstDataset = datasetStatistics[0];
        if (firstDataset.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var datasetResult = new JsonObject();
        if (TryGetLongProperty(firstDataset, "table_row_count", out var rowCount))
        {
            datasetResult["row_count"] = rowCount;
        }

        if (TryGetDoubleProperty(firstDataset, "table_size", out var tableSizeBytes))
        {
            datasetResult["size_kb"] = BytesToKilobytes(tableSizeBytes);
        }

        if (datasetResult.Count > 0)
        {
            result["result"] = datasetResult;
        }
    }

    private static void AddNetworkStats(JsonObject result, JsonElement resourceUsage)
    {
        if (!resourceUsage.TryGetProperty("network", out var network)
            || network.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var networkResult = new JsonObject();
        if (TryGetDoubleProperty(network, "cross_cluster_total_bytes", out var crossClusterBytes))
        {
            networkResult["cross_cluster_mb"] = BytesToMegabytes(crossClusterBytes);
        }

        if (TryGetDoubleProperty(network, "inter_cluster_total_bytes", out var interClusterBytes))
        {
            networkResult["inter_cluster_mb"] = BytesToMegabytes(interClusterBytes);
        }

        if (networkResult.Count > 0)
        {
            result["network"] = networkResult;
        }
    }

    private static void AddCrossClusterBreakdown(JsonObject result, JsonElement source)
    {
        if (!source.TryGetProperty("cross_cluster_resource_usage", out var crossClusterResourceUsage)
            || crossClusterResourceUsage.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var crossClusterResult = new JsonObject();
        foreach (var clusterUsage in crossClusterResourceUsage.EnumerateObject())
        {
            if (clusterUsage.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var clusterResult = new JsonObject();
            if (clusterUsage.Value.TryGetProperty("cpu", out var cpu)
                && cpu.ValueKind == JsonValueKind.Object
                && TryGetStringProperty(cpu, "total cpu", out var totalCpu))
            {
                clusterResult["cpu_total"] = totalCpu;
            }

            if (clusterUsage.Value.TryGetProperty("memory", out var memory)
                && memory.ValueKind == JsonValueKind.Object
                && TryGetDoubleProperty(memory, "peak_per_node", out var peakPerNodeBytes))
            {
                clusterResult["memory_peak_mb"] = BytesToMegabytes(peakPerNodeBytes);
            }

            if (clusterUsage.Value.TryGetProperty("cache", out var cache)
                && cache.ValueKind == JsonValueKind.Object
                && cache.TryGetProperty("shards", out var shards)
                && shards.ValueKind == JsonValueKind.Object
                && shards.TryGetProperty("hot", out var hot)
                && hot.ValueKind == JsonValueKind.Object)
            {
                if (TryGetDoubleProperty(hot, "hitbytes", out var hitBytes))
                {
                    clusterResult["cache_hit_mb"] = BytesToMegabytes(hitBytes);
                }

                if (TryGetDoubleProperty(hot, "missbytes", out var missBytes))
                {
                    clusterResult["cache_miss_mb"] = BytesToMegabytes(missBytes);
                }
            }

            if (clusterResult.Count == 0)
            {
                continue;
            }

            var clusterName = NormalizeClusterName(clusterUsage.Name);
            if (!string.IsNullOrWhiteSpace(clusterName))
            {
                crossClusterResult[clusterName] = clusterResult;
            }
        }

        if (crossClusterResult.Count > 0)
        {
            result["cross_cluster_breakdown"] = crossClusterResult;
        }
    }

    private static int GetColumnIndex(JsonElement columnsElement, string targetColumnName)
    {
        var index = 0;
        foreach (var column in columnsElement.EnumerateArray())
        {
            if (column.ValueKind == JsonValueKind.Object
                && column.TryGetProperty("ColumnName", out var columnNameElement)
                && columnNameElement.ValueKind == JsonValueKind.String
                && string.Equals(columnNameElement.GetString(), targetColumnName, StringComparison.Ordinal))
            {
                return index;
            }

            index++;
        }

        return -1;
    }

    private static string? GetRowStringValue(JsonElement row, int index)
    {
        if (index < 0)
        {
            return null;
        }

        var currentIndex = 0;
        foreach (var value in row.EnumerateArray())
        {
            if (currentIndex == index)
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Null => null,
                    _ => value.GetRawText()
                };
            }

            currentIndex++;
        }

        return null;
    }

    private static bool TryGetStringProperty(JsonElement element, string propertyName, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();
        return !string.IsNullOrEmpty(value);
    }

    private static bool TryGetDoubleProperty(JsonElement element, string propertyName, out double value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property) && TryGetDoubleValue(property, out value);
    }

    private static bool TryGetLongProperty(JsonElement element, string propertyName, out long value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property) && TryGetLongValue(property, out value);
    }

    private static bool TryGetBooleanValue(JsonElement element, out bool value)
    {
        value = false;
        return element.ValueKind switch
        {
            JsonValueKind.True => SetBooleanResult(true, out value),
            JsonValueKind.False => SetBooleanResult(false, out value),
            JsonValueKind.String => bool.TryParse(element.GetString(), out value),
            JsonValueKind.Number => TryConvertNumberToBoolean(element, out value),
            _ => false
        };
    }

    private static bool TryGetDoubleValue(JsonElement element, out double value)
    {
        value = 0;
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out value),
            JsonValueKind.String => double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value),
            _ => false
        };
    }

    private static bool TryGetLongValue(JsonElement element, out long value)
    {
        value = 0;
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetInt64(out value),
            JsonValueKind.String => long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value),
            _ => false
        };
    }

    private static bool SetBooleanResult(bool result, out bool value)
    {
        value = result;
        return true;
    }

    private static bool TryConvertNumberToBoolean(JsonElement element, out bool value)
    {
        value = false;
        if (!element.TryGetInt32(out var intValue))
        {
            return false;
        }

        value = intValue != 0;
        return true;
    }

    private static double BytesToMegabytes(double bytes) => Math.Round(bytes / 1048576d, 2);

    private static double BytesToKilobytes(double bytes) => Math.Round(bytes / 1024d, 2);

    private static string NormalizeClusterName(string clusterIdentifier)
    {
        if (Uri.TryCreate(clusterIdentifier, UriKind.Absolute, out var clusterUri))
        {
            return clusterUri.Host;
        }

        var normalizedClusterIdentifier = clusterIdentifier.Trim().TrimEnd('/');
        normalizedClusterIdentifier = normalizedClusterIdentifier.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);
        normalizedClusterIdentifier = normalizedClusterIdentifier.Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
        return normalizedClusterIdentifier;
    }

    private static bool TryGetFirstResultTable(JsonElement root, out JsonElement table)
    {
        table = default;
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("Tables", out var tablesElement)
            && tablesElement.ValueKind == JsonValueKind.Array
            && tablesElement.GetArrayLength() > 0)
        {
            table = tablesElement[0];
            return true;
        }

        if (root.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var hasFallbackDataTable = false;
        JsonElement fallbackDataTable = default;
        foreach (var frame in root.EnumerateArray())
        {
            if (frame.ValueKind != JsonValueKind.Object
                || !frame.TryGetProperty("FrameType", out var frameType)
                || frameType.ValueKind != JsonValueKind.String
                || !string.Equals(frameType.GetString(), "DataTable", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!hasFallbackDataTable)
            {
                fallbackDataTable = frame;
                hasFallbackDataTable = true;
            }

            if (frame.TryGetProperty("TableKind", out var tableKind)
                && tableKind.ValueKind == JsonValueKind.String
                && string.Equals(tableKind.GetString(), "PrimaryResult", StringComparison.OrdinalIgnoreCase))
            {
                table = frame;
                return true;
            }
        }

        if (hasFallbackDataTable)
        {
            table = fallbackDataTable;
            return true;
        }

        return false;
    }

    private List<string> KustoResultToStringList(KustoResult kustoResult)
    {
        var result = new List<string>();
        if (kustoResult.RootElement.ValueKind == JsonValueKind.Null)
        {
            return result;
        }
        var root = kustoResult.RootElement;
        if (!TryGetFirstResultTable(root, out var table))
        {
            return result;
        }

        if (!table.TryGetProperty("Columns", out var columnsElement) || columnsElement.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        var columns = columnsElement.EnumerateArray()
            .Select(column => ($"{column.GetProperty("ColumnName").GetString()}:{column.GetProperty("ColumnType").GetString()}"));
        var columnsAsString = string.Join(",", columns);
        result.Add(columnsAsString);
        if (!table.TryGetProperty("Rows", out var items) || items.ValueKind != JsonValueKind.Array)
        {
            return result;
        }
        foreach (var item in items.EnumerateArray())
        {
            var jsonAsString = item.ToString();
            result.Add(jsonAsString);
        }
        return result;
    }

    private async Task<KustoClient> GetOrCreateKustoClientAsync(string clusterUri, string? tenant, CancellationToken cancellationToken = default)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri, tenant) + "_command";
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration, cancellationToken);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant, cancellationToken);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientFactory);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration, cancellationToken);
        }

        return kustoClient;
    }

    private async Task<KustoClient> GetOrCreateCslQueryProviderAsync(string clusterUri, string? tenant, CancellationToken cancellationToken = default)
    {
        var providerCacheKey = GetProviderCacheKey(clusterUri, tenant) + "_query";
        var kustoClient = await _cacheService.GetAsync<KustoClient>(CacheGroup, providerCacheKey, s_providerCacheDuration, cancellationToken);
        if (kustoClient == null)
        {
            var tokenCredential = await GetCredential(tenant, cancellationToken);
            kustoClient = new KustoClient(clusterUri, tokenCredential, UserAgent, _httpClientFactory);
            await _cacheService.SetAsync(CacheGroup, providerCacheKey, kustoClient, s_providerCacheDuration, cancellationToken);
        }

        return kustoClient;
    }

    private async Task<string> GetClusterUriAsync(
        string subscriptionId,
        string clusterName,
        string? tenant,
        RetryPolicyOptions? retryPolicy)
    {
        var cluster = await GetClusterAsync(subscriptionId, clusterName, tenant, retryPolicy);
        var value = cluster?.ClusterUri;

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"Could not retrieve ClusterUri for cluster '{clusterName}'");
        }

        return value!;
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a cluster name string.
    /// </summary>
    /// <param name="item">The JsonElement containing cluster data</param>
    /// <returns>The cluster model</returns>
    private static KustoClusterModel ConvertToClusterModel(JsonElement item)
    {
        Models.KustoClusterData? kustoCluster = Models.KustoClusterData.FromJson(item);
        if (kustoCluster == null)
            throw new InvalidOperationException("Failed to parse Kusto cluster data");

        if (string.IsNullOrEmpty(kustoCluster.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(kustoCluster.ResourceId);

        return new KustoClusterModel(
            ClusterName: kustoCluster.ResourceName ?? "Unknown",
            Location: kustoCluster.Location,
            ResourceGroupName: id.ResourceGroupName,
            SubscriptionId: id.SubscriptionId,
            Sku: kustoCluster.Sku?.Name,
            Zones: kustoCluster.Zones == null ? string.Empty : string.Join(",", kustoCluster.Zones.ToList()),
            Identity: kustoCluster.Identity?.ManagedServiceIdentityType,
            ETag: kustoCluster.ETag?.ToString(),
            State: kustoCluster.Properties?.State,
            ProvisioningState: kustoCluster.Properties?.ProvisioningState,
            ClusterUri: kustoCluster.Properties?.ClusterUri?.ToString(),
            DataIngestionUri: kustoCluster.Properties?.DataIngestionUri?.ToString(),
            StateReason: kustoCluster.Properties?.StateReason,
            IsStreamingIngestEnabled: kustoCluster.Properties?.IsStreamingIngestEnabled ?? false,
            EngineType: kustoCluster.Properties?.EngineType?.ToString(),
            IsAutoStopEnabled: kustoCluster.Properties?.IsAutoStopEnabled ?? false
        );
    }
}
