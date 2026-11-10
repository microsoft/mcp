// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record DeviceProvisioningOnboardingValidationRequest(
    string ServiceName,
    string ResourceGroup,
    string Subscription,
    string? Tenant,
    string? EnrollmentId,
    string? EnrollmentGroupId,
    string? RegistrationId,
    string? IntendedHub,
    string? CertificateChain,
    string? TpmEndorsementKey,
    bool IncludeRegistrationState,
    List<OnboardingDeviceEvidence>? Devices = null);
