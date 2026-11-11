// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

public interface IDeviceProvisioningOnboardingService
{
    Task<OnboardingValidationResult> ValidateOnboarding(
        DeviceProvisioningOnboardingValidationRequest request,
        CancellationToken cancellationToken = default);
}
