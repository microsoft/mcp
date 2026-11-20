// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;

/// <summary>Options for the business case report command, which shows a finished business case's ROI report in a card.</summary>
public sealed class ShowBusinessCaseReportOptions
{
    /// <summary>Gets or sets the assessment name, for display only.</summary>
    [Option(Description = "Optional. Name of the business case, from 'assessmentName' of the succeeded status result.")]
    public string? AssessmentName { get; set; }

    /// <summary>Gets or sets the read SAS link to the ROI report.</summary>
    [Option(Description = "Required. Read link to the ROI report, exactly as 'result' of the succeeded business case status result.")]
    public string? ReportUri { get; set; }
}
