// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Starts a machine assessment from a collector ZIP on the Assessment Service:
/// PUT /businessCaseRestApi/machineAssessmentsFromZip/{assessmentName}, which returns 202 with the operation.
/// </summary>
public sealed class AssessmentCreateClient(ILogger<AssessmentCreateClient> logger) : IAssessmentCreateClient
{
    private readonly ILogger<AssessmentCreateClient> _logger = logger;

    /// <inheritdoc/>
    public Task<AssessmentOperation> CreateAssessmentAsync(AssessmentRequest request, string token, CancellationToken cancellationToken)
    {
        string body = DsBackdoorClient.WriteJson(w =>
        {
            w.WriteString("workspaceId", AssessmentApi.ResolveWorkspaceId(request.WorkspaceId));
            w.WriteString("inventorySasUri", request.InventorySasUri);
            w.WriteStartObject("settings");
            w.WriteString("azureLocation", request.TargetRegion);
            w.WriteString("currency", request.Currency);
            if (request.Settings is { } s)
            {
                WriteSettings(w, s);
            }

            w.WriteEndObject();
        });

        return AssessmentApi.SendAsync(
            _logger, HttpMethod.Put, AssessmentApi.CreateUrl(AssessmentApi.BaseUrl, request.AssessmentName), token, body, "create", cancellationToken);
    }

    /// <summary>Writes only the optional settings the user supplied; anything left out uses the assessment's default.</summary>
    internal static void WriteSettings(System.Text.Json.Utf8JsonWriter w, AssessmentSettings s)
    {
        void Num(string name, double? v) { if (v is not null) { w.WriteNumber(name, v.Value); } }
        void Str(string name, string? v) { if (v is not null) { w.WriteString(name, v); } }
        void Arr(string name, string[]? v)
        {
            if (v is null) { return; }
            w.WriteStartArray(name);
            foreach (string item in v) { w.WriteStringValue(item); }
            w.WriteEndArray();
        }

        Num("scalingFactor", s.ScalingFactor);
        Num("storageScalingFactor", s.StorageScalingFactor);
        Num("discountPercentage", s.DiscountPercentage);
        Str("environmentType", s.EnvironmentType);
        if (s.LicensingProgram is not null || s.BillingSubscriptionId is not null)
        {
            w.WriteStartObject("billingSettings");
            Str("licensingProgram", s.LicensingProgram);
            Str("subscriptionId", s.BillingSubscriptionId);
            w.WriteEndObject();
        }

        if (s.SavingsOption is not null || s.AzureOfferCode is not null)
        {
            w.WriteStartObject("savingsSettings");
            Str("savingsOptions", s.SavingsOption);
            Str("azureOfferCode", s.AzureOfferCode);
            w.WriteEndObject();
        }

        Str("azureSecurityOfferingType", s.SecurityOfferingType);
        Arr("azureDiskTypes", s.DiskTypes);
        Str("azurePricingTier", s.PricingTier);
        Str("azureStorageRedundancy", s.StorageRedundancy);
        Str("azureHybridUseBenefit", s.AzureHybridBenefit);
        Str("linuxAzureHybridUseBenefit", s.LinuxAzureHybridBenefit);
        Arr("azureVmFamilies", s.VmFamilies);
        if (s.UptimeDaysPerMonth is not null || s.UptimeHoursPerDay is not null)
        {
            // The API requires both values when vmUptime is supplied; the missing one keeps its default.
            w.WriteStartObject("vmUptime");
            w.WriteNumber("daysPerMonth", s.UptimeDaysPerMonth ?? 31);
            w.WriteNumber("hoursPerDay", s.UptimeHoursPerDay ?? 24);
            w.WriteEndObject();
        }

        Arr("azureVmSecurityOptions", s.VmSecurityOptions);
        if (s.ManagementServices is not null)
        {
            w.WriteStartArray("azureManagementServices");
            foreach (string service in s.ManagementServices)
            {
                w.WriteStartObject();
                w.WriteString("serviceName", service);
                w.WriteEndObject();
            }

            w.WriteEndArray();
        }
    }
}
