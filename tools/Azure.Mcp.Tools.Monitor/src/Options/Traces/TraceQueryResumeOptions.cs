// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceQueryResumeOptions : ISubscriptionOption
{
    [Option(Description = "Operation id returned by 'monitor traces query run'.")]
    public required string OperationId { get; set; }

    [Option(Description = "Rows requested per results page. Default 100; clamped to 1-1000. Does not change the submitted query's total row limit.")]
    public int? PageSize { get; set; }

    [Option(Description = "Include span-level detail in the response. Default true.")]
    public bool? IncludeSpans { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the Azure Monitor Workspace enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
