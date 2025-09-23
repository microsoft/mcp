// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class AutoUpgradeProfile
{
    public string? UpgradeChannel { get; set; }
    public string? NodeOSUpgradeChannel { get; set; }
}

public sealed class ClusterSecurityProfile
{
    public AzureKeyVaultKms? AzureKeyVaultKms { get; set; }
    public IList<string>? CustomCATrustCertificates { get; set; }
    public DefenderProfile? Defender { get; set; }
    public ImageCleanerProfile? ImageCleaner { get; set; }
    public WorkloadIdentityProfile? WorkloadIdentity { get; set; }
}

public sealed class ImageCleanerProfile
{
    public bool? Enabled { get; set; }
    public int? IntervalHours { get; set; }
}

public sealed class WorkloadIdentityProfile
{
    public bool? Enabled { get; set; }
}

public sealed class AzureKeyVaultKms
{
    public bool? Enabled { get; set; }
    public string? KeyId { get; set; }
}

public sealed class DefenderProfile
{
    public string? LogAnalyticsWorkspaceResourceId { get; set; }
    public DefenderSecurityMonitoring? SecurityMonitoring { get; set; }
}

public sealed class DefenderSecurityMonitoring
{
    public bool? Enabled { get; set; }
}

public sealed class ClusterStorageProfile
{
    public CsiDriverProfile? BlobCSIDriver { get; set; }
    public CsiDriverProfile? DiskCSIDriver { get; set; }
    public CsiDriverProfile? FileCSIDriver { get; set; }
    public SnapshotControllerProfile? SnapshotController { get; set; }
}

public sealed class CsiDriverProfile
{
    public bool? Enabled { get; set; }
}

public sealed class SnapshotControllerProfile
{
    public bool? Enabled { get; set; }
}

public sealed class ClusterMetricsProfile
{
    public CostAnalysisProfile? CostAnalysis { get; set; }
}

public sealed class CostAnalysisProfile
{
    public bool? Enabled { get; set; }
}

public sealed class ClusterNodeProvisioningProfile
{
    public string? Mode { get; set; }
    public string? DefaultNodePools { get; set; }
}

public sealed class ClusterBootstrapProfile
{
    public string? ArtifactSource { get; set; }
}

public sealed class AiToolchainOperatorProfile
{
    public bool? Enabled { get; set; }
}

public sealed class WorkloadAutoScalerProfile
{
    public WorkloadAutoScalerKeda? Keda { get; set; }
    public WorkloadAutoScalerVerticalPodAutoscaler? VerticalPodAutoscaler { get; set; }
}

public sealed class WorkloadAutoScalerKeda
{
    public bool? Enabled { get; set; }
}

public sealed class WorkloadAutoScalerVerticalPodAutoscaler
{
    public bool? Enabled { get; set; }
}
