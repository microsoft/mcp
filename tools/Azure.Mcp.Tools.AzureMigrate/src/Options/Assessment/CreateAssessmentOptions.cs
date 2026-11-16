// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Assessment;

/// <summary>
/// Options for the assessment create command.
/// </summary>
/// <remarks>
/// TODO: Add more customer-provided assessment settings (for example sizing, pricing and discount options) once the
/// Assessment Service backdoor PUT contract is finalized.
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
}
