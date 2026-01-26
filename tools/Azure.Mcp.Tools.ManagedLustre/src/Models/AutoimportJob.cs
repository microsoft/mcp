// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class AutoimportJob
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public AutoimportJobProperties? Properties { get; set; }
}

public class AutoimportJobProperties
{
    public string? ProvisioningState { get; set; }
    public string[]? AutoImportPrefixes { get; set; }
    public string? ConflictResolutionMode { get; set; }
    public bool? EnableDeletions { get; set; }
    public int? MaximumErrors { get; set; }
    public string? AdminStatus { get; set; }
    public AutoimportJobStatus? Status { get; set; }
}

public class AutoimportJobStatus
{
    public string? State { get; set; }
    public string? StatusCode { get; set; }
    public long? TotalBlobsWalked { get; set; }
    public double? RateOfBlobWalk { get; set; }
    public long? TotalBlobsImported { get; set; }
    public double? RateOfBlobImport { get; set; }
    public long? ImportedFiles { get; set; }
    public long? ImportedDirectories { get; set; }
    public long? ImportedSymlinks { get; set; }
    public long? PreexistingFiles { get; set; }
    public long? PreexistingDirectories { get; set; }
    public long? PreexistingSymlinks { get; set; }
    public long? TotalErrors { get; set; }
    public long? TotalConflicts { get; set; }
    public BlobSyncEvents? BlobSyncEvents { get; set; }
    public DateTime? LastStartedTimeUTC { get; set; }
}

public class BlobSyncEvents
{
    public long? ImportedFiles { get; set; }
    public long? ImportedDirectories { get; set; }
    public long? ImportedSymlinks { get; set; }
    public long? PreexistingFiles { get; set; }
    public long? PreexistingDirectories { get; set; }
    public long? PreexistingSymlinks { get; set; }
    public long? TotalBlobsImported { get; set; }
    public double? RateOfBlobImport { get; set; }
    public long? TotalErrors { get; set; }
    public long? TotalConflicts { get; set; }
    public long? Deletions { get; set; }
    public DateTime? LastTimeFullySynchronized { get; set; }
}
