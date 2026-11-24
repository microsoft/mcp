// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Assessment;

/// <summary>
/// Assessment settings validated by <c>AssessmentValidation.BuildSettings</c> for the business case. Optional settings left out are
/// not sent, so the assessment uses its defaults.
/// </summary>
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

    /// <summary>Gets or sets the optional scaling factor.</summary>
    [Option(Description = "Optional. Compute scaling (comfort) factor between 1 and 2. Default 1.")]
    public double? ScalingFactor { get; set; }

    /// <summary>Gets or sets the optional storage scaling factor.</summary>
    [Option(Description = "Optional. Storage scaling factor between 1 and 2. Default 1.")]
    public double? StorageScalingFactor { get; set; }

    /// <summary>Gets or sets the optional discount percentage.</summary>
    [Option(Description = "Optional. Discount percentage on Azure costs, 0 to 100. Default 0.")]
    public double? DiscountPercentage { get; set; }

    /// <summary>Gets or sets the optional environment type.</summary>
    [Option(Description = "Optional. Environment type: 'Production' or 'DevTest'. Default 'Production'.")]
    public string? EnvironmentType { get; set; }

    /// <summary>Gets or sets the optional licensing program.</summary>
    [Option(Description = "Optional. Licensing program: 'Retail', 'EA' or 'MCA'. Default 'Retail'. EA and MCA pricing need 'billing-subscription-id'.")]
    public string? LicensingProgram { get; set; }

    /// <summary>Gets or sets the optional billing subscription id.</summary>
    [Option(Description = "Optional. Subscription ID (GUID) used for EA or MCA negotiated pricing. Default none.")]
    public string? BillingSubscriptionId { get; set; }

    /// <summary>Gets or sets the optional savings option.</summary>
    [Option(Description = "Optional. Savings option: 'None', 'RI1Year', 'RI3Year', 'SavingsPlan1Year' or 'SavingsPlan3Year'. Default 'None'.")]
    public string? SavingsOption { get; set; }

    /// <summary>Gets or sets the optional azure offer code.</summary>
    [Option(Description = "Optional. Azure offer code, for example 'MSAZR0003P' (pay as you go) or 'MSAZR0023P' (dev/test). Default 'MSAZR0003P'.")]
    public string? AzureOfferCode { get; set; }

    /// <summary>Gets or sets the optional security offering type.</summary>
    [Option(Description = "Optional. Microsoft Defender for Cloud cost: 'NO' or 'MDC'. Default 'NO'.")]
    public string? SecurityOfferingType { get; set; }

    /// <summary>Gets or sets the optional disk types.</summary>
    [Option(Description = "Optional. Disk types to consider, any of 'Standard', 'StandardSSD', 'Premium', 'StandardOrPremium', 'Ultra', " +
        "'PremiumV2'. Replaces the default rather than adding to it. Default ['StandardSSD'].")]
    public string[]? DiskTypes { get; set; }

    /// <summary>Gets or sets the optional pricing tier.</summary>
    [Option(Description = "Optional. Pricing tier: 'Standard' or 'Basic'. Default 'Standard'.")]
    public string? PricingTier { get; set; }

    /// <summary>Gets or sets the optional storage redundancy.</summary>
    [Option(Description = "Optional. Storage redundancy: 'LocallyRedundant', 'GeoRedundant', 'ZoneRedundant' or 'ReadAccessGeoRedundant'. " +
        "Default 'LocallyRedundant'.")]
    public string? StorageRedundancy { get; set; }

    /// <summary>Gets or sets the optional azure hybrid benefit.</summary>
    [Option(Description = "Optional. Azure Hybrid Benefit for Windows: 'Yes' or 'No'. Default 'No'.")]
    public string? AzureHybridBenefit { get; set; }

    /// <summary>Gets or sets the optional linux azure hybrid benefit.</summary>
    [Option(Description = "Optional. Azure Hybrid Benefit for Linux: 'Yes' or 'No'. Default 'No'.")]
    public string? LinuxAzureHybridBenefit { get; set; }

    /// <summary>Gets or sets the optional vm families.</summary>
    [Option(Description = "Optional. VM families to limit recommendations to, for example 'Dsv5_series' or 'Ev5_series'. Default: all families.")]
    public string[]? VmFamilies { get; set; }

    /// <summary>Gets or sets the optional uptime days per month.</summary>
    [Option(Description = "Optional. Days per month the VMs run, 1 to 31. Default 31.")]
    public int? UptimeDaysPerMonth { get; set; }

    /// <summary>Gets or sets the optional uptime hours per day.</summary>
    [Option(Description = "Optional. Hours per day the VMs run, 1 to 24. Default 24.")]
    public int? UptimeHoursPerDay { get; set; }

    /// <summary>Gets or sets the optional vm security options.</summary>
    [Option(Description = "Optional. VM security types to consider, any of 'Standard', 'TVM' (trusted launch) or 'CVM' (confidential). Default none.")]
    public string[]? VmSecurityOptions { get; set; }

    /// <summary>Gets or sets the optional management services.</summary>
    [Option(Description = "Optional. Azure management service names to include in costs. Default none.")]
    public string[]? ManagementServices { get; set; }
}
