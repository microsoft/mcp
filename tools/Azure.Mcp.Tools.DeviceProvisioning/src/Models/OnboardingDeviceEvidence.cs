// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public sealed class OnboardingDeviceEvidence
{
    public string RegistrationId { get; set; } = string.Empty;

    public string? CertificateChain { get; set; }

    public string? TpmEndorsementKey { get; set; }
}
