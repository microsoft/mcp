// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public sealed class OnboardingDeviceManifest
{
    public List<OnboardingDeviceEvidence?>? Devices { get; set; } = [];
}
