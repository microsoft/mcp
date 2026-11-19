// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;

/// <summary>
/// Options for the business case create command. Optional settings left out use the business case defaults.
/// </summary>
public sealed class CreateBusinessCaseOptions : BusinessCaseHaSettingsOptions
{
    /// <summary>Gets or sets the name of the assessment the business case creates.</summary>
    [Option(Description = "Required. Name of the business case assessment: 1-64 characters, starting with a letter or digit, using only " +
        "letters, digits, '-' or '_'.")]
    public string? AssessmentName { get; set; }

    /// <summary>Gets or sets the target Azure region.</summary>
    [Option(Description = "Required. Target Azure region, as an Azure region name such as 'eastus' or 'westeurope'.")]
    public string? TargetRegion { get; set; }

    /// <summary>Gets or sets the currency.</summary>
    [Option(Description = "Optional. Currency for costs. Only 'USD' is supported for now. Default 'USD'.")]
    public string? Currency { get; set; }

    /// <summary>Gets or sets the read SAS URL of the collector ZIP.</summary>
    [Option(Description = "Required. Read-only SAS URL (https) of the offline collector ZIP, as given by the user. Ask the user for it " +
        "if it is missing and do not invent it.")]
    public string? InventorySasUri { get; set; }

    /// <summary>Gets or sets the compute scaling factor.</summary>
    [Option(Description = "Optional. Compute scaling (comfort) factor between 1 and 2. Default 1.3.")]
    public double? ScalingFactor { get; set; }

    /// <summary>Gets or sets the savings option.</summary>
    [Option(Description = "Optional. Savings option: 'None', 'RI1Year', 'RI3Year', 'SavingsPlan1Year' or 'SavingsPlan3Year'. Default 'RI3Year'.")]
    public string? SavingsOption { get; set; }

    /// <summary>Gets or sets the Azure Hybrid Benefit for Windows.</summary>
    [Option(Description = "Optional. Azure Hybrid Benefit for Windows: 'Yes' or 'No'. Default 'Yes'.")]
    public string? AzureHybridBenefit { get; set; }

    /// <summary>Gets or sets the Azure Hybrid Benefit for Linux.</summary>
    [Option(Description = "Optional. Azure Hybrid Benefit for Linux: 'Yes' or 'No'. Default 'Yes'.")]
    public string? LinuxAzureHybridBenefit { get; set; }
}
