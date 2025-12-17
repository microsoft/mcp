// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class AutoimportJob
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? ProvisioningState { get; set; }
    public string? ConflictResolutionMode { get; set; }
    public string[]? AutoImportPrefixes { get; set; }
    public string? AdminStatus { get; set; }
    public bool? EnableDeletions { get; set; }
    public int? MaximumErrors { get; set; }
    public long? TotalBlobsImported { get; set; }
    public long? TotalConflicts { get; set; }
    public long? TotalErrors { get; set; }
}
