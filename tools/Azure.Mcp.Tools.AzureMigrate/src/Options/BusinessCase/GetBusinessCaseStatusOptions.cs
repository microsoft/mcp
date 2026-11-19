// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;

/// <summary>
/// Options for the business case status command. Nothing is stored on the server: every value comes from 'nextStatusParameters' of
/// the previous create or status result.
/// </summary>
public sealed class GetBusinessCaseStatusOptions : BusinessCaseHaSettingsOptions
{
    /// <summary>Gets or sets the assessment name, for display only.</summary>
    [Option(Description = "Optional. Name of the business case assessment, from 'nextStatusParameters'.")]
    public string? AssessmentName { get; set; }

    /// <summary>Gets or sets the assessment operation id.</summary>
    [Option(Description = "Required. Assessment operation ID (GUID), from 'nextStatusParameters'.")]
    public string? AsOperationId { get; set; }

    /// <summary>Gets or sets the read SAS URL of the collector ZIP.</summary>
    [Option(Description = "Required. Read-only SAS URL of the collector ZIP, from 'nextStatusParameters'.")]
    public string? InventorySasUri { get; set; }

    /// <summary>Gets or sets the Discovery Service session id of the HA skill run.</summary>
    [Option(Description = "Optional. Session ID of the HA skill run, from 'nextStatusParameters' once present.")]
    public string? DsSessionId { get; set; }

    /// <summary>Gets or sets the Discovery Service operation id of the HA skill run.</summary>
    [Option(Description = "Optional. Operation ID (GUID) of the HA skill run, from 'nextStatusParameters' once present.")]
    public string? DsOperationId { get; set; }
}
