// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Assessment;

/// <summary>
/// Options for the assessment status command.
/// </summary>
public sealed class GetAssessmentStatusOptions
{
    /// <summary>
    /// Gets or sets the assessment name.
    /// </summary>
    [Option(Description = "Required. Name of the assessment (assessmentName) returned when the assessment was created.")]
    public string? AssessmentName { get; set; }

    /// <summary>
    /// Gets or sets the tracking operation ID.
    /// </summary>
    [Option(Description = "Required. ID of the assessment operation (operationId, a GUID) returned when the assessment was created.")]
    public string? OperationId { get; set; }
}
