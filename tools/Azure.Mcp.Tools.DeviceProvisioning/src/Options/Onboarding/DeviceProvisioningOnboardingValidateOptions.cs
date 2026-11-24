// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.DeviceProvisioning.Options.Onboarding;

public sealed class DeviceProvisioningOnboardingValidateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Device Provisioning Service instance.")]
    public required string Service { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The existing individual enrollment registration ID. Specify exactly one of --enrollment-id or --enrollment-group-id.")]
    public string? EnrollmentId { get; set; }

    [Option(Description = "The existing enrollment group ID. Specify exactly one of --enrollment-id or --enrollment-group-id.")]
    public string? EnrollmentGroupId { get; set; }

    [Option(Description = "Optional device registration ID for current stored registration-state assessment.")]
    public string? RegistrationId { get; set; }

    [Option(Description = "Optional intended IoT Hub hostname to compare with enrollment targets, DPS linked Hubs, and the current stored assignment.")]
    public string? IntendedHub { get; set; }

    [Option(Description = "Optional PEM text containing only public X.509 certificates. Private-key material is rejected.")]
    public string? CertificateChain { get; set; }

    [Option(Description = "Optional public TPM endorsement key to compare with the selected enrollment.")]
    public string? TpmEndorsementKey { get; set; }

    [Option(Description = "Optional inline JSON object containing 1-10 devices to assess against the selected enrollment or group. Shape: {\"devices\":[{\"registrationId\":\"device1\",\"certificateChain\":\"<public PEM>\"}]}. Cannot be combined with --registration-id, --certificate-chain, or --tpm-endorsement-key.")]
    public string? DeviceManifest { get; set; }

    [Option(Description = "Whether to read the available stored registration state. Defaults to true when a registration ID can be determined.")]
    public bool? IncludeRegistrationState { get; set; }

    [Option(Description = "Whether to include full per-device findings in the response. Defaults to false; aggregate results and exceptional devices are still returned.")]
    public bool? IncludeDeviceDetails { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
