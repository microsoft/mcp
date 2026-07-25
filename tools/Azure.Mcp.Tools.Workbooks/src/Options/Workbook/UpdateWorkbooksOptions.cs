// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public sealed class UpdateWorkbooksOptions
{
    [Option(Description = "The Azure Resource ID of the workbook to retrieve.")]
    public required string WorkbookId { get; set; }

    [Option(Description = WorkbooksOptionDescriptions.DisplayNameDescription)]
    public string? DisplayName { get; set; }

    [Option(Description = WorkbooksOptionDescriptions.SerializedContentDescription)]
    public string? SerializedContent { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
