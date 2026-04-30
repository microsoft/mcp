// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Rendering;

namespace Azure.Mcp.Tools.Kusto.Options;

public class SampleOptions : BaseTableOptions
{
    public int? Limit { get; set; }
    public ChartType? ChartType { get; set; }
}
