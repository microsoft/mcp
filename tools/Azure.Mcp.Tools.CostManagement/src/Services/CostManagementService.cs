// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.CostManagement.Models;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.CostManagement.Services;

public sealed class CostManagementService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<CostManagementService> logger)
    : BaseAzureService(tenantService), ICostManagementService
{
    private const string CostColumn = "Cost";
    private const string CostUsdColumn = "CostUSD";
    private const string PreTaxCostColumn = "PreTaxCost";
    private const string CurrencyColumn = "Currency";
    private const string UsageDateColumn = "UsageDate";

    internal static readonly IReadOnlySet<string> KnownDimensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ServiceName", "ResourceGroupName", "ResourceLocation", "ResourceId",
        "MeterCategory", "MeterSubCategory", "ChargeType", "BillingPeriod"
    };

    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<CostManagementService> _logger = logger;

    public async Task<CostQueryResult> QueryAsync(
        string subscription,
        string? resourceGroup,
        QueryTimeframe timeframe,
        DateTime? from,
        DateTime? to,
        QueryGranularity granularity,
        string? groupBy,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(("subscription", subscription));

        if (!string.IsNullOrWhiteSpace(groupBy) && !KnownDimensions.Contains(groupBy))
        {
            _logger.LogWarning(
                "--group-by '{GroupBy}' is not in the well-known dimension set. Passing through to the Cost Management API; an unsupported value will surface as HTTP 400.",
                groupBy);
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        var scope = string.IsNullOrEmpty(resourceGroup)
            ? new ResourceIdentifier($"/subscriptions/{subscriptionId}")
            : new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}");

        var dataset = new QueryDataset();
        dataset.Aggregation["totalCost"] = new QueryAggregation(CostColumn, FunctionType.Sum);

        if (granularity == QueryGranularity.Daily)
        {
            dataset.Granularity = GranularityType.Daily;
        }

        if (!string.IsNullOrWhiteSpace(groupBy))
        {
            dataset.Grouping.Add(new QueryGrouping(QueryColumnType.Dimension, groupBy));
        }

        var queryDefinition = new QueryDefinition(ExportType.ActualCost, MapTimeframe(timeframe), dataset);
        if (timeframe == QueryTimeframe.Custom)
        {
            queryDefinition.TimePeriod = new QueryTimePeriod(from!.Value, to!.Value);
        }

        _logger.LogDebug(
            "Querying Cost Management. Scope: {Scope}, Timeframe: {Timeframe}, Granularity: {Granularity}, GroupBy: {GroupBy}",
            scope, timeframe, granularity, groupBy);

        var response = await armClient.UsageQueryAsync(scope, queryDefinition, cancellationToken);
        return MapResult(response.Value, timeframe, from, to, granularity, groupBy);
    }

    private static TimeframeType MapTimeframe(QueryTimeframe timeframe) => timeframe switch
    {
        QueryTimeframe.MonthToDate => TimeframeType.MonthToDate,
        QueryTimeframe.BillingMonthToDate => TimeframeType.BillingMonthToDate,
        QueryTimeframe.TheLastBillingMonth => TimeframeType.TheLastBillingMonth,
        QueryTimeframe.WeekToDate => TimeframeType.WeekToDate,
        QueryTimeframe.Custom => TimeframeType.Custom,
        _ => throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, "Unsupported timeframe.")
    };

    private static CostQueryResult MapResult(
        QueryResult queryResult,
        QueryTimeframe timeframe,
        DateTime? from,
        DateTime? to,
        QueryGranularity granularity,
        string? groupBy)
    {
        var columns = queryResult.Columns ?? [];
        var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < columns.Count; i++)
        {
            columnIndex[columns[i].Name] = i;
        }

        int costIndex = ResolveColumnIndex(columnIndex, CostColumn, PreTaxCostColumn, CostUsdColumn);
        int currencyIndex = columnIndex.TryGetValue(CurrencyColumn, out var ci) ? ci : -1;
        int usageDateIndex = columnIndex.TryGetValue(UsageDateColumn, out var ui) ? ui : -1;
        int groupByIndex = !string.IsNullOrWhiteSpace(groupBy) && columnIndex.TryGetValue(groupBy!, out var gi) ? gi : -1;

        var rows = queryResult.Rows ?? [];
        var mapped = new List<CostQueryRow>(rows.Count);
        decimal total = 0m;
        string? currency = null;

        foreach (var row in rows)
        {
            var cost = costIndex >= 0 && costIndex < row.Count ? ReadDecimal(row[costIndex], CostColumn) : 0m;
            total += cost;

            if (currencyIndex >= 0 && currencyIndex < row.Count)
            {
                var rowCurrency = ReadString(row[currencyIndex], CurrencyColumn);
                if (!string.IsNullOrEmpty(rowCurrency))
                {
                    currency = rowCurrency;
                }
            }

            string? usageDate = usageDateIndex >= 0 && usageDateIndex < row.Count
                ? FormatUsageDate(row[usageDateIndex])
                : null;

            string? groupValue = groupByIndex >= 0 && groupByIndex < row.Count
                ? ReadString(row[groupByIndex], groupBy!)
                : null;

            mapped.Add(new CostQueryRow(cost, usageDate, groupValue));
        }

        return new CostQueryResult(
            Currency: currency,
            TotalCost: total,
            Timeframe: timeframe.ToString(),
            FromDate: timeframe == QueryTimeframe.Custom ? from : null,
            ToDate: timeframe == QueryTimeframe.Custom ? to : null,
            Granularity: granularity.ToString(),
            GroupBy: groupBy,
            Rows: mapped);
    }

    internal static int ResolveColumnIndex(IReadOnlyDictionary<string, int> columnIndex, params string[] candidates)
    {
        foreach (var name in candidates)
        {
            if (columnIndex.TryGetValue(name, out var idx))
            {
                return idx;
            }
        }
        return -1;
    }

    internal static decimal ReadDecimal(BinaryData? data, string columnName)
    {
        if (data is null)
        {
            return 0m;
        }

        try
        {
            using var doc = JsonDocument.Parse(data);
            return doc.RootElement.ValueKind switch
            {
                JsonValueKind.Null => 0m,
                JsonValueKind.Number => doc.RootElement.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(
                    doc.RootElement.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed) => parsed,
                _ =>
                    throw new InvalidOperationException(BuildParseError(data, columnName, "decimal"))
            };
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(BuildParseError(data, columnName, "decimal"), ex);
        }
    }

    internal static string? ReadString(BinaryData? data, string columnName)
    {
        if (data is null)
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(data);
            return doc.RootElement.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => doc.RootElement.GetString(),
                JsonValueKind.Number => doc.RootElement.GetRawText(),
                _ => throw new InvalidOperationException(BuildParseError(data, columnName, "string"))
            };
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(BuildParseError(data, columnName, "string"), ex);
        }
    }

    private static string BuildParseError(BinaryData data, string columnName, string targetType)
    {
        var raw = data.ToString();
        var snippet = raw.Length > 120 ? raw[..120] + "..." : raw;
        return $"Cost Management API returned a value for column '{columnName}' that could not be parsed as {targetType}: {snippet}";
    }

    internal static string? FormatUsageDate(BinaryData? data)
    {
        var raw = ReadString(data, UsageDateColumn);
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        return raw.Length switch
        {
            8 => $"{raw[..4]}-{raw.Substring(4, 2)}-{raw.Substring(6, 2)}",
            6 => $"{raw[..4]}-{raw.Substring(4, 2)}-01",
            _ => raw
        };
    }
}
