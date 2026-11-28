// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Services;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Validation and normalization of the assessment inputs and statuses that the business case commands share.
/// </summary>
internal static partial class AssessmentValidation
{
    // Operation statuses of the assessment: Running, Succeeded, Failed or Canceled.
    private static readonly string[] s_terminalStatuses = ["Succeeded", "Failed", "Canceled"];

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9_-]{0,63}$")]
    private static partial Regex AssessmentNameRegex();

    [GeneratedRegex("^[a-z][a-z0-9]{1,39}$")]
    private static partial Regex RegionRegex();

    internal static bool IsValidAssessmentName(string? name) => !string.IsNullOrEmpty(name) && AssessmentNameRegex().IsMatch(name);

    /// <summary>Returns true for an absolute http(s) URL, the shape accepted for 'inventory-sas-uri' and SAS URLs in general.</summary>
    internal static bool IsValidInventorySasUri(string? uri) =>
        !string.IsNullOrWhiteSpace(uri)
        && Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed)
        && (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp);

    internal static bool IsTerminal(string? status) =>
        status is not null && s_terminalStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the region in lower case without spaces (for example 'East US' becomes 'eastus'), or null if invalid.</summary>
    internal static string? NormalizeRegion(string? region)
    {
        string? normalized = region?.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToLowerInvariant();
        return !string.IsNullOrEmpty(normalized) && RegionRegex().IsMatch(normalized) ? normalized : null;
    }

    /// <summary>
    /// Validates the optional settings and returns them with enum values in the assessment's spelling, or null when none were supplied.
    /// </summary>
    internal static (AssessmentSettings? Settings, string? Error) BuildSettings(CreateAssessmentOptions o)
    {
        List<string> errors = [];

        double? Range(string option, double? value, double min, double max)
        {
            if (value is not null && (double.IsNaN(value.Value) || value < min || value > max))
            {
                errors.Add($"'{option}' must be between {min} and {max}.");
            }

            return value;
        }

        int? IntRange(string option, int? value, int min, int max)
        {
            if (value is not null && (value < min || value > max))
            {
                errors.Add($"'{option}' must be between {min} and {max}.");
            }

            return value;
        }

        string? One(string option, string? value, string[] allowed)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string? match = allowed.FirstOrDefault(a => a.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                errors.Add($"'{option}' must be one of: {string.Join(", ", allowed)}.");
            }

            return match;
        }

        string[]? Many(string option, string[]? values, string[]? allowed)
        {
            string[]? items = values?.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (items is null || items.Length == 0)
            {
                return null;
            }

            return allowed is null ? items : items.Select(v => One(option, v, allowed)!).ToArray();
        }

        string? subscriptionId = string.IsNullOrWhiteSpace(o.BillingSubscriptionId) ? null : o.BillingSubscriptionId.Trim();
        if (subscriptionId is not null && !Guid.TryParse(subscriptionId, out _))
        {
            errors.Add("'billing-subscription-id' must be a subscription ID (GUID).");
        }

        AssessmentSettings settings = new()
        {
            ScalingFactor = Range("scaling-factor", o.ScalingFactor, 1, 2),
            StorageScalingFactor = Range("storage-scaling-factor", o.StorageScalingFactor, 1, 2),
            DiscountPercentage = Range("discount-percentage", o.DiscountPercentage, 0, 100),
            EnvironmentType = One("environment-type", o.EnvironmentType, ["Production", "DevTest"]),
            LicensingProgram = One("licensing-program", o.LicensingProgram, ["Retail", "EA", "MCA"]),
            BillingSubscriptionId = subscriptionId,
            SavingsOption = One("savings-option", o.SavingsOption, ["None", "RI1Year", "RI3Year", "SavingsPlan1Year", "SavingsPlan3Year"]),
            AzureOfferCode = One("azure-offer-code", o.AzureOfferCode,
                ["MSAZR0003P", "MSAZR0023P", "MSMCAZR0044P", "MSMCAZR0059P", "MSMCAZR0060P", "MSMCAZR0063P", "MSAZRUSGOV0003P", "EA"]),
            SecurityOfferingType = One("security-offering-type", o.SecurityOfferingType, ["NO", "MDC"]),
            DiskTypes = Many("disk-types", o.DiskTypes, ["Standard", "StandardSSD", "Premium", "StandardOrPremium", "Ultra", "PremiumV2"]),
            PricingTier = One("pricing-tier", o.PricingTier, ["Standard", "Basic"]),
            StorageRedundancy = One("storage-redundancy", o.StorageRedundancy,
                ["LocallyRedundant", "GeoRedundant", "ZoneRedundant", "ReadAccessGeoRedundant"]),
            AzureHybridBenefit = One("azure-hybrid-benefit", o.AzureHybridBenefit, ["Yes", "No"]),
            LinuxAzureHybridBenefit = One("linux-azure-hybrid-benefit", o.LinuxAzureHybridBenefit, ["Yes", "No"]),
            VmFamilies = Many("vm-families", o.VmFamilies, null),
            UptimeDaysPerMonth = IntRange("uptime-days-per-month", o.UptimeDaysPerMonth, 1, 31),
            UptimeHoursPerDay = IntRange("uptime-hours-per-day", o.UptimeHoursPerDay, 1, 24),
            VmSecurityOptions = Many("vm-security-options", o.VmSecurityOptions, ["Standard", "TVM", "CVM"]),
            ManagementServices = Many("management-services", o.ManagementServices, null),
        };

        if (errors.Count > 0)
        {
            return (null, string.Join(" ", errors.Distinct()));
        }

        return settings == new AssessmentSettings() ? (null, null) : (settings, null);
    }
}
