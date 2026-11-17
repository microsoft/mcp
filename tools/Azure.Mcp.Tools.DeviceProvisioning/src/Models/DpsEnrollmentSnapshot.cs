// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

internal record DpsEnrollmentSnapshot(
    string TargetId,
    string ProvisioningStatus,
    string AttestationType,
    string AllocationPolicy,
    string IotHubHostName,
    List<string> IotHubs,
    string CustomAllocationWebhookUrl,
    bool? UpdateHubAssignment,
    bool? MigrateDeviceData,
    bool HasInitialTwin,
    string TpmEndorsementKey,
    List<string> X509ClientCertificateThumbprints,
    List<string> X509SigningCertificateThumbprints,
    List<DpsCertificateAuthoritySnapshot> X509CertificateAuthorities)
{
    public bool UsesX509CertificateAuthorityReferences =>
        X509CertificateAuthorities.Count > 0;
}
