// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.CostManagement.Models;

public record CostQueryRow(
    decimal Cost,
    string? UsageDate,
    string? GroupValue);
