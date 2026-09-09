// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options;

public sealed class WorkspaceLogSearchOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the Log Analytics workspace containing the table.")]
    public required string Workspace { get; set; }

    [Option(Description = "The ASCII name of the Basic or Auxiliary Log Analytics table to search.")]
    public required string Table { get; set; }

    [Option(Description = "A KQL pipeline fragment beginning with '|'. The table is bound by the server.")]
    public required string Query { get; set; }

    [Option(Description = "A positive ISO 8601 duration or a closed RFC 3339 start/end interval, up to 30 days.")]
    public required string Timespan { get; set; }

    [Option(Description = "The maximum number of rows to return, from 1 through 100.", DefaultValue = 20)]
    public int Limit { get; set; } = 20;

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
