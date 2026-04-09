// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.GalleryApplication;

public class GalleryApplicationVersionCreateOptions : BaseComputeOptions
{
    public string? Gallery { get; set; }

    public string? GalleryApplication { get; set; }

    public string? GalleryApplicationVersion { get; set; }

    public string? Location { get; set; }

    public string? Tags { get; set; }

    public string? SourceMediaLink { get; set; }

    public string? DefaultConfigurationLink { get; set; }

    public int? ReplicaCount { get; set; }

    public bool? ExcludeFromLatest { get; set; }

    public string? ManageActionInstall { get; set; }

    public string? ManageActionRemove { get; set; }

    public string? ManageActionUpdate { get; set; }

    public string? PackageFileName { get; set; }

    public string? ConfigFileName { get; set; }

    public string? ScriptBehaviorAfterReboot { get; set; }

    public string? EndOfLifeDate { get; set; }

    public string? TargetRegions { get; set; }
}
