// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Assessment;

/// <summary>
/// Options for the assessment create command.
/// </summary>
/// <remarks>
/// TODO: Add more customer-provided assessment settings (for example sizing, pricing and discount options) from the Business
/// Case Service machine assessment contract.
/// </remarks>
public sealed class CreateAssessmentOptions
{
    /// <summary>
    /// Gets or sets the assessment name.
    /// </summary>
    [Option(Description = "Required. Name of the assessment to create: 1-64 characters, starting with a letter or digit, using only " +
        "letters, digits, '-' or '_'.")]
    public string? AssessmentName { get; set; }

    /// <summary>
    /// Gets or sets the target Azure region.
    /// </summary>
    [Option(Description = "Required. Target Azure region to assess for, as an Azure region name such as 'eastus' or 'westeurope'.")]
    public string? TargetRegion { get; set; }

    /// <summary>
    /// Gets or sets the currency for cost estimates.
    /// </summary>
    [Option(Description = "Required. Currency for cost estimates, as a three-letter ISO 4217 code such as 'USD' or 'EUR'.")]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the read SAS URL of the collector ZIP to assess.
    /// </summary>
    /// <remarks>
    /// TODO: Get this from the Discovery Service (a read SAS for the session's collector ZIP) instead of asking the user for it.
    /// </remarks>
    [Option(Description = "Required. Read-only SAS URL (https) of the offline collector ZIP to assess, as given by the user. Ask the user " +
        "for it if it is missing and do not invent it.")]
    public string? InventorySasUri { get; set; }
}
