// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;

/// <summary>
/// Business case settings for the HA evaluation, shared by the create and status commands so that the status command can pass them on.
/// Settings left out use the HA defaults.
/// </summary>
public class BusinessCaseHaSettingsOptions
{
    /// <summary>Gets or sets the network cost as a percentage of compute and storage cost.</summary>
    [Option(Description = "Optional. Network cost as a percentage of compute and storage cost, 0 to 100. Default 5.")]
    public double? NetworkCostPercentage { get; set; }

    /// <summary>Gets or sets the IaaS labor cost percentage.</summary>
    [Option(Description = "Optional. Percentage of current labor cost that remains for IaaS workloads on Azure, 0 to 100. Default 75.")]
    public double? IaasLaborCostPercentage { get; set; }

    /// <summary>Gets or sets the PaaS labor cost percentage.</summary>
    [Option(Description = "Optional. Percentage of current labor cost that remains for PaaS workloads on Azure, 0 to 100. Default 60.")]
    public double? PaasLaborCostPercentage { get; set; }

    /// <summary>Gets or sets the management labor cost percentage.</summary>
    [Option(Description = "Optional. Percentage of current management labor cost that remains on Azure, 0 to 100. Default 60.")]
    public double? ManagementLaborCostPercentage { get; set; }

    /// <summary>Gets or sets the yearly infrastructure growth rate.</summary>
    [Option(Description = "Optional. Yearly infrastructure growth rate in percent, 0 to 100. Default 5.")]
    public double? InfrastructureGrowthRate { get; set; }

    /// <summary>Gets or sets the cumulative migration completion percentage per year.</summary>
    [Option(Description = "Optional. Percentage of the estate migrated by the end of each year, exactly 4 values for Year0 to Year3, " +
        "each 0 to 100. Default [0, 20, 50, 100].")]
    public double[]? MigrationCompletionByYear { get; set; }
}
