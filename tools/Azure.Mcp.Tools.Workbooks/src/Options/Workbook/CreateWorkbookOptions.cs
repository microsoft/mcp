// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public sealed class CreateWorkbookOptions : ISubscriptionOption
{
    [Option(Description = WorkbooksOptionDescriptions.DisplayNameDescription)]
    public required string DisplayName { get; set; }

    [Option(Description = WorkbooksOptionDescriptions.SerializedContentDescription)]
    public required string SerializedContent { get; set; }

    [Option(Description = "The linked resource ID for the workbook. By default, this is 'azure monitor'.")]
    public string? SourceId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
