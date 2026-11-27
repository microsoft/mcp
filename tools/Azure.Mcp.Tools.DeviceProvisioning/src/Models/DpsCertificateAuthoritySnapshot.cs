// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

internal record DpsCertificateAuthoritySnapshot(
    string Role,
    string Name,
    string ResolutionStatus,
    DeviceProvisioningCertificateDescription? Certificate);
