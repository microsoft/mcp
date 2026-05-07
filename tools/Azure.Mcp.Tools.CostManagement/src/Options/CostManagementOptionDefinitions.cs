// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.CostManagement.Models;

namespace Azure.Mcp.Tools.CostManagement.Options;

public static class CostManagementOptionDefinitions
{
    public const string TimeframeName = "timeframe";
    public const string FromName = "from";
    public const string ToName = "to";
    public const string GranularityName = "granularity";
    public const string GroupByName = "group-by";

    public static readonly Option<QueryTimeframe?> Timeframe = new($"--{TimeframeName}")
    {
        Description =
            "Predefined time range for the query. Defaults to MonthToDate. " +
            "Use 'Custom' together with --from and --to for a specific window. " +
            "Allowed values: MonthToDate, BillingMonthToDate, TheLastBillingMonth, WeekToDate, Custom. " +
            "Use TheLastBillingMonth for the previous full billing period.",
        Required = false
    };

    public static readonly Option<DateTime?> From = new($"--{FromName}")
    {
        Description = "Start date (UTC, ISO-8601, e.g. 2026-04-01) for Custom timeframe. Required when --timeframe Custom.",
        Required = false
    };

    public static readonly Option<DateTime?> To = new($"--{ToName}")
    {
        Description = "End date (UTC, ISO-8601, e.g. 2026-04-30) for Custom timeframe. Required when --timeframe Custom.",
        Required = false
    };

    public static readonly Option<QueryGranularity?> Granularity = new($"--{GranularityName}")
    {
        Description =
            "Row granularity. 'None' (default) returns a single aggregated total. " +
            "'Daily' returns one row per day. Allowed values: None, Daily.",
        Required = false
    };

    public static readonly Option<string> GroupBy = new($"--{GroupByName}")
    {
        Description =
            "Optional dimension to group costs by. Common values: ServiceName, ResourceGroupName, " +
            "ResourceLocation, ResourceId, MeterCategory, MeterSubCategory, ChargeType, BillingPeriod. " +
            "Other API-supported dimensions (including custom and tag-based dimensions) are also accepted; " +
            "unrecognized values are passed through and surface as HTTP 400 if the API rejects them. " +
            "Only one dimension may be specified in this command.",
        Required = false
    };
}
