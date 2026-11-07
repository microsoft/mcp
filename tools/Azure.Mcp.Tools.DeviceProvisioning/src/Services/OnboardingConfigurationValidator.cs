// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

internal static class OnboardingConfigurationValidator
{
    public static void Validate(
        DeviceProvisioningServiceDescription service,
        DpsEnrollmentSnapshot enrollment,
        string? intendedHub,
        OnboardingValidationBuilder result)
    {
        result.IdScope = service.IdScope;
        result.ServiceState = service.State;
        result.ProvisioningState = service.ProvisioningState;
        result.AttestationType = enrollment.AttestationType;
        result.EffectiveAllocationPolicy = string.IsNullOrWhiteSpace(enrollment.AllocationPolicy)
            ? service.AllocationPolicy
            : enrollment.AllocationPolicy;

        ValidateService(service, result);
        ValidateEnrollment(enrollment, result);
        ValidateAllocation(service, enrollment, intendedHub, result);
        AddReprovisionPolicy(enrollment, result);
        AddAttestationSummary(enrollment, result);
    }

    private static void ValidateService(
        DeviceProvisioningServiceDescription service,
        OnboardingValidationBuilder result)
    {
        if (string.Equals(service.State, "Active", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "dps.resource.state",
                "passed",
                "The DPS instance is active.");
        }
        else
        {
            result.AddFinding(
                "dps.resource.state",
                "blocker",
                $"The DPS instance state is '{service.State}'.",
                "Restore the DPS instance to an active state before onboarding devices.");
        }

        if (string.Equals(service.ProvisioningState, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "dps.resource.provisioningState",
                "passed",
                "The DPS ARM resource provisioning state is succeeded.");
        }
        else
        {
            result.AddFinding(
                "dps.resource.provisioningState",
                "warning",
                $"The DPS ARM resource provisioning state is '{service.ProvisioningState}'.",
                "Confirm that the ARM resource operation completes successfully.");
        }

        if (string.IsNullOrWhiteSpace(service.PublicNetworkAccess))
        {
            result.AddFinding(
                "dps.network.publicAccess",
                "passed",
                "The DPS public network access setting was not explicitly returned and uses the platform default of enabled.");
        }
        else if (string.Equals(service.PublicNetworkAccess, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "dps.network.publicAccess",
                "warning",
                "Public network access is disabled. Devices and operators require an approved private network path.",
                "Verify private endpoint, DNS, and client network configuration.");
        }
        else
        {
            result.AddFinding(
                "dps.network.publicAccess",
                "passed",
                "The DPS public network access setting does not prevent public endpoint use.");
        }
    }

    private static void ValidateEnrollment(
        DpsEnrollmentSnapshot enrollment,
        OnboardingValidationBuilder result)
    {
        if (string.Equals(enrollment.ProvisioningStatus, "enabled", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "enrollment.enabled",
                "passed",
                "The selected enrollment is enabled.");
        }
        else
        {
            result.AddFinding(
                "enrollment.enabled",
                "blocker",
                $"The selected enrollment provisioning status is '{enrollment.ProvisioningStatus}'.",
                "Enable the selected enrollment before onboarding devices.");
        }

        if (string.IsNullOrWhiteSpace(enrollment.AttestationType))
        {
            result.AddFinding(
                "enrollment.attestation.type",
                "unverified",
                "The enrollment did not return an attestation type.");
        }
        else
        {
            result.AddFinding(
                "enrollment.attestation.type",
                "informational",
                $"The selected enrollment uses {enrollment.AttestationType} attestation.");
        }

        result.AddFinding(
            "enrollment.initialTwin",
            "informational",
            enrollment.HasInitialTwin
                ? "The enrollment contains an initial twin configuration."
                : "The enrollment does not contain an initial twin configuration.");
    }

    private static void ValidateAllocation(
        DeviceProvisioningServiceDescription service,
        DpsEnrollmentSnapshot enrollment,
        string? intendedHub,
        OnboardingValidationBuilder result)
    {
        var hasEnrollmentAllocationSettings =
            !string.IsNullOrWhiteSpace(enrollment.AllocationPolicy) ||
            enrollment.IotHubs.Count > 0;
        if (hasEnrollmentAllocationSettings &&
            !string.IsNullOrWhiteSpace(enrollment.IotHubHostName))
        {
            result.AddFinding(
                "allocation.conflictingConfiguration",
                "blocker",
                "The enrollment contains both legacy iotHubHostName and enrollment-level allocation settings.",
                "Use either iotHubHostName or allocationPolicy/iotHubs, not both.");
        }

        var linkedHubs = service.IotHubs
            .Select(hub => hub.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allocationEnabledHubs = service.IotHubs
            .Where(hub => hub.ApplyAllocationPolicy != false)
            .Select(hub => hub.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var referencedHubs = enrollment.IotHubs.Count > 0
            ? enrollment.IotHubs
            : string.IsNullOrWhiteSpace(enrollment.IotHubHostName)
                ? []
                : [enrollment.IotHubHostName];
        var unlinkedHubs = referencedHubs
            .Where(hub => !linkedHubs.Contains(hub))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var allocationDisabledHubs = referencedHubs
            .Where(hub =>
                linkedHubs.Contains(hub) &&
                !allocationEnabledHubs.Contains(hub))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (unlinkedHubs.Count > 0)
        {
            result.AddFinding(
                "allocation.linkedHubConsistency",
                "blocker",
                $"The enrollment references IoT Hub(s) that are not linked to this DPS instance: {string.Join(", ", unlinkedHubs)}.",
                "Link the referenced IoT Hub or update the enrollment to use linked Hubs.");
        }
        else if (allocationDisabledHubs.Count > 0)
        {
            result.AddFinding(
                "allocation.linkedHubConsistency",
                "blocker",
                $"The enrollment references IoT Hub(s) that are linked but disabled for DPS allocation: {string.Join(", ", allocationDisabledHubs)}.",
                "Enable allocation for the referenced IoT Hub or update the enrollment to use an allocation-enabled linked Hub.");
        }
        else
        {
            result.AddFinding(
                "allocation.linkedHubConsistency",
                "passed",
                referencedHubs.Count == 0
                    ? "The enrollment uses the DPS-level linked Hub configuration."
                    : "All IoT Hubs referenced by the enrollment are linked to the DPS instance.");
        }

        ValidateIntendedHub(
            intendedHub,
            linkedHubs,
            allocationEnabledHubs,
            referencedHubs,
            result);

        var effectivePolicy = string.IsNullOrWhiteSpace(enrollment.AllocationPolicy)
            ? service.AllocationPolicy
            : enrollment.AllocationPolicy;
        if (string.Equals(effectivePolicy, "static", StringComparison.OrdinalIgnoreCase))
        {
            var targetCount = referencedHubs.Count;
            if (targetCount != 1)
            {
                result.AddFinding(
                    "allocation.staticConfiguration",
                    "blocker",
                    $"Static allocation requires exactly one target IoT Hub, but {targetCount} were configured.",
                    "Configure exactly one linked IoT Hub for static allocation.");
            }
            else
            {
                result.AddFinding(
                    "allocation.staticConfiguration",
                    "passed",
                    $"Static allocation targets '{referencedHubs[0]}'.");
            }
        }

        if (string.Equals(effectivePolicy, "custom", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(
                    enrollment.CustomAllocationWebhookUrl,
                    UriKind.Absolute,
                    out var webhookUri) ||
                webhookUri.Scheme != Uri.UriSchemeHttps)
            {
                result.AddFinding(
                    "allocation.customDefinition",
                    "blocker",
                    "Custom allocation does not contain a valid HTTPS webhook URL.",
                    "Configure an absolute HTTPS custom-allocation webhook URL.");
            }
            else
            {
                result.AddFinding(
                    "allocation.customDefinition",
                    "passed",
                    "The custom-allocation webhook URL has a valid HTTPS configuration shape.");
                result.AddLimitation(
                    "The custom-allocation webhook was not invoked, so its availability and authorization were not verified.");
            }
        }
    }

    private static void ValidateIntendedHub(
        string? intendedHub,
        HashSet<string> linkedHubs,
        HashSet<string> allocationEnabledHubs,
        IReadOnlyCollection<string> referencedHubs,
        OnboardingValidationBuilder result)
    {
        if (string.IsNullOrWhiteSpace(intendedHub))
        {
            return;
        }

        if (!linkedHubs.Contains(intendedHub))
        {
            result.AddFinding(
                "allocation.intendedHubConsistency",
                "blocker",
                $"The intended IoT Hub '{intendedHub}' is not linked to this DPS instance.",
                "Use the exact hostname of a linked IoT Hub or update the DPS linked-Hub configuration.");
            return;
        }

        if (!allocationEnabledHubs.Contains(intendedHub))
        {
            result.AddFinding(
                "allocation.intendedHubConsistency",
                "blocker",
                $"The intended IoT Hub '{intendedHub}' is linked to DPS but disabled for allocation.",
                "Enable allocation for the intended IoT Hub or select an allocation-enabled linked Hub.");
            return;
        }

        if (referencedHubs.Count > 0 &&
            !referencedHubs.Contains(intendedHub, StringComparer.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "allocation.intendedHubConsistency",
                "blocker",
                $"The intended IoT Hub '{intendedHub}' is linked to DPS but is not targeted by the selected enrollment.",
                "Update the enrollment allocation targets or provide an intended Hub referenced by the enrollment.");
            return;
        }

        result.AddFinding(
            "allocation.intendedHubConsistency",
            "passed",
            $"The intended IoT Hub '{intendedHub}' is consistent with the selected enrollment and DPS linked-Hub configuration.");
    }

    private static void AddReprovisionPolicy(
        DpsEnrollmentSnapshot enrollment,
        OnboardingValidationBuilder result)
    {
        if (enrollment.UpdateHubAssignment is null &&
            enrollment.MigrateDeviceData is null)
        {
            result.AddFinding(
                "reprovision.policy",
                "informational",
                "The response did not include an explicit reprovisioning policy.");
            return;
        }

        result.AddFinding(
            "reprovision.policy",
            "informational",
            $"Reprovisioning is configured with updateHubAssignment={enrollment.UpdateHubAssignment?.ToString().ToLowerInvariant() ?? "unspecified"} and migrateDeviceData={enrollment.MigrateDeviceData?.ToString().ToLowerInvariant() ?? "unspecified"}.");
    }

    private static void AddAttestationSummary(
        DpsEnrollmentSnapshot enrollment,
        OnboardingValidationBuilder result)
    {
        if (string.Equals(enrollment.AttestationType, "symmetricKey", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "attestation.symmetricKey",
                "informational",
                "The enrollment uses symmetric-key attestation. Key material was not requested or returned.");
        }
        else if (string.Equals(enrollment.AttestationType, "tpm", StringComparison.OrdinalIgnoreCase))
        {
            result.AddFinding(
                "attestation.tpm",
                "informational",
                string.IsNullOrWhiteSpace(enrollment.TpmEndorsementKey)
                    ? "The enrollment uses TPM attestation, but no public endorsement key was returned."
                    : "The enrollment uses TPM attestation and contains a public endorsement key.");
        }
        else if (string.Equals(enrollment.AttestationType, "x509", StringComparison.OrdinalIgnoreCase))
        {
            var mode = enrollment.UsesX509CertificateAuthorityReferences
                ? "certificate authority references"
                : enrollment.X509SigningCertificateThumbprints.Count > 0
                    ? "signing certificates"
                    : enrollment.X509ClientCertificateThumbprints.Count > 0
                        ? "client certificates"
                        : "no public certificate metadata";
            result.AddFinding(
                "attestation.x509.configuration",
                "informational",
                $"The enrollment uses X.509 attestation configured with {mode}.");
        }
    }
}
