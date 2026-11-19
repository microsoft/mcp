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
    [Option(Description = "Optional. Read-only SAS URL (https) of the offline collector ZIP, only when the user gives one. Leave it out " +
        "to use the user's inventory instead (see 'session-id'); never invent it.")]
    public string? InventorySasUri { get; set; }

    /// <summary>Gets or sets the inventory session whose offline ZIP is exported when no SAS URL is given.</summary>
    [Option(Description = "Optional. ID of the user's inventory session ('sessionId' from the inventory tools). When 'inventory-sas-uri' " +
        "is left out, a fresh link to this inventory's offline zip is exported and used. Pass the latest 'sessionId' in the conversation.")]
    public string? SessionId { get; set; }

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

    /// <summary>Gets or sets the storage scaling factor.</summary>
    [Option(Description = "Optional. Storage scaling factor between 1 and 2. Default 1.")]
    public double? StorageScalingFactor { get; set; }

    /// <summary>Gets or sets the discount percentage.</summary>
    [Option(Description = "Optional. Discount percentage on Azure costs, 0 to 100. Default 0.")]
    public double? DiscountPercentage { get; set; }

    /// <summary>Gets or sets the environment type.</summary>
    [Option(Description = "Optional. Environment type: 'Production' or 'DevTest'. Default 'Production'.")]
    public string? EnvironmentType { get; set; }

    /// <summary>Gets or sets the licensing program.</summary>
    [Option(Description = "Optional. Licensing program: 'Retail', 'EA' or 'MCA'. Default 'Retail'. EA and MCA pricing need 'billing-subscription-id'.")]
    public string? LicensingProgram { get; set; }

    /// <summary>Gets or sets the billing subscription id.</summary>
    [Option(Description = "Optional. Subscription ID (GUID) used for EA or MCA negotiated pricing. Default none.")]
    public string? BillingSubscriptionId { get; set; }

    /// <summary>Gets or sets the Azure offer code.</summary>
    [Option(Description = "Optional. Azure offer code, for example 'MSAZR0003P' (pay as you go) or 'MSAZR0023P' (dev/test). Default 'MSAZR0003P'.")]
    public string? AzureOfferCode { get; set; }

    /// <summary>Gets or sets the security offering type.</summary>
    [Option(Description = "Optional. Microsoft Defender for Cloud cost: 'NO' or 'MDC'. Default 'NO'.")]
    public string? SecurityOfferingType { get; set; }

    /// <summary>Gets or sets the disk types.</summary>
    [Option(Description = "Optional. Disk types to consider, any of 'Standard', 'StandardSSD', 'Premium', 'StandardOrPremium', 'Ultra', " +
        "'PremiumV2'. Replaces the default rather than adding to it. Default ['StandardSSD'].")]
    public string[]? DiskTypes { get; set; }

    /// <summary>Gets or sets the pricing tier.</summary>
    [Option(Description = "Optional. Pricing tier: 'Standard' or 'Basic'. Default 'Standard'.")]
    public string? PricingTier { get; set; }

    /// <summary>Gets or sets the storage redundancy.</summary>
    [Option(Description = "Optional. Storage redundancy: 'LocallyRedundant', 'GeoRedundant', 'ZoneRedundant' or 'ReadAccessGeoRedundant'. " +
        "Default 'LocallyRedundant'.")]
    public string? StorageRedundancy { get; set; }

    /// <summary>Gets or sets the VM families.</summary>
    [Option(Description = "Optional. VM families to limit recommendations to, for example 'Dsv5_series' or 'Ev5_series'. Default: all families.")]
    public string[]? VmFamilies { get; set; }

    /// <summary>Gets or sets the uptime days per month.</summary>
    [Option(Description = "Optional. Days per month the VMs run, 1 to 31. Default 31.")]
    public int? UptimeDaysPerMonth { get; set; }

    /// <summary>Gets or sets the uptime hours per day.</summary>
    [Option(Description = "Optional. Hours per day the VMs run, 1 to 24. Default 24.")]
    public int? UptimeHoursPerDay { get; set; }

    /// <summary>Gets or sets the VM security options.</summary>
    [Option(Description = "Optional. VM security types to consider, any of 'Standard', 'TVM' (trusted launch) or 'CVM' (confidential). Default none.")]
    public string[]? VmSecurityOptions { get; set; }

    /// <summary>Gets or sets the management services.</summary>
    [Option(Description = "Optional. Azure management service names to include in costs. Default none.")]
    public string[]? ManagementServices { get; set; }
}
