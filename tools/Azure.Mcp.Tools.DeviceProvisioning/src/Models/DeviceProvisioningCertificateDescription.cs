// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record DeviceProvisioningCertificateDescription(
    string Name,
    string Certificate,
    bool IsVerified,
    string Subject,
    string Thumbprint);
