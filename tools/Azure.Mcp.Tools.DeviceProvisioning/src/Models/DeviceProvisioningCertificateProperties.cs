// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

internal sealed class DeviceProvisioningCertificateProperties
{
    public string? Certificate { get; set; }

    public bool IsVerified { get; set; }

    public string? Subject { get; set; }

    public string? Thumbprint { get; set; }
}
