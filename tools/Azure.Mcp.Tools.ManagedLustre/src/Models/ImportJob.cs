// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class ImportJob
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public ImportJobProperties? Properties { get; set; }
}

public class ImportJobProperties
{
    public string? ProvisioningState { get; set; }
    public string[]? ImportPrefixes { get; set; }
    public string? ConflictResolutionMode { get; set; }
    public long? MaximumErrors { get; set; }
    public string? AdminStatus { get; set; }
    public ImportJobStatus? Status { get; set; }
}

public class ImportJobStatus
{
    public string? State { get; set; }
    public long? TotalBlobsWalked { get; set; }
    public double? BlobsWalkedPerSecond { get; set; }
    public long? TotalBlobsImported { get; set; }
    public double? BlobsImportedPerSecond { get; set; }
    public long? TotalErrors { get; set; }
    public long? TotalConflicts { get; set; }
}
