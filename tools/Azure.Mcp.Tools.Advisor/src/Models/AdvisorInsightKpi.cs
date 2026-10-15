// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A named KPI of an insight. <see cref="Value"/> is a JSON number for numeric KPIs and a string otherwise.
/// </summary>
public sealed record AdvisorInsightKpi(string Name, JsonElement Value, string? Unit);
