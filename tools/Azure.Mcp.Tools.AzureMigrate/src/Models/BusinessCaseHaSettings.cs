// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Options.BusinessCase;

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Business case settings sent to the HA evaluate, all percentages from 0 to 100. <see cref="MigrationCompletionByYear"/> holds the
/// share of the estate migrated by the end of Year0 to Year3.
/// </summary>
public sealed record BusinessCaseHaSettings(
    double NetworkCostPercentage,
    double IaasLaborCostPercentage,
    double PaasLaborCostPercentage,
    double ManagementLaborCostPercentage,
    double InfrastructureGrowthRate,
    IReadOnlyList<double> MigrationCompletionByYear)
{
    /// <summary>HA defaults, used for any setting left out.</summary>
    internal static readonly BusinessCaseHaSettings Default = new(5, 75, 60, 60, 5, [0, 20, 50, 100]);

    internal const int MigrationYears = 4;

    /// <summary>Applies the HA defaults to settings left out and validates the rest.</summary>
    internal static (BusinessCaseHaSettings? Settings, string? Error) Build(BusinessCaseHaSettingsOptions o)
    {
        List<string> errors = [];

        double Percentage(string option, double? value, double defaultValue)
        {
            if (value is null)
            {
                return defaultValue;
            }

            if (double.IsNaN(value.Value) || value < 0 || value > 100)
            {
                errors.Add($"'{option}' must be between 0 and 100.");
            }

            return value.Value;
        }

        IReadOnlyList<double> ramp = Default.MigrationCompletionByYear;
        if (o.MigrationCompletionByYear is { Length: > 0 } years)
        {
            if (years.Length != MigrationYears || years.Any(y => double.IsNaN(y) || y < 0 || y > 100))
            {
                errors.Add($"'migration-completion-by-year' must have {MigrationYears} values (Year0 to Year3), each between 0 and 100.");
            }

            ramp = years;
        }

        BusinessCaseHaSettings settings = new(
            Percentage("network-cost-percentage", o.NetworkCostPercentage, Default.NetworkCostPercentage),
            Percentage("iaas-labor-cost-percentage", o.IaasLaborCostPercentage, Default.IaasLaborCostPercentage),
            Percentage("paas-labor-cost-percentage", o.PaasLaborCostPercentage, Default.PaasLaborCostPercentage),
            Percentage("management-labor-cost-percentage", o.ManagementLaborCostPercentage, Default.ManagementLaborCostPercentage),
            Percentage("infrastructure-growth-rate", o.InfrastructureGrowthRate, Default.InfrastructureGrowthRate),
            ramp);

        return errors.Count > 0 ? (null, string.Join(" ", errors)) : (settings, null);
    }
}
