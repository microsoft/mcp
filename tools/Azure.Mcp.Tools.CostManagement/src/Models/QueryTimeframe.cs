// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.CostManagement.Models;

/// <summary>
/// Predefined time ranges accepted by the Cost Management Query API at subscription
/// and resource group scope. <c>TheLastMonth</c> is omitted because it is only supported
/// at billing-account scope; use <see cref="TheLastBillingMonth"/> for the previous full
/// billing period instead.
/// </summary>
public enum QueryTimeframe
{
    MonthToDate,
    BillingMonthToDate,
    TheCurrentMonth,
    TheLastBillingMonth,
    WeekToDate,
    Custom
}
