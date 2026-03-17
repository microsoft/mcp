// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class AutoexportJob
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public AutoexportJobProperties? Properties { get; set; }
}

public class AutoexportJobProperties
{
    public string? ProvisioningState { get; set; }
    public string[]? AutoExportPrefixes { get; set; }
    public string? AdminStatus { get; set; }
    public AutoexportJobStatus? Status { get; set; }
}

public class AutoexportJobStatus
{
    public string? State { get; set; }
    public long? TotalFilesExported { get; set; }
    public long? TotalMiBExported { get; set; }
    public long? TotalFilesFailed { get; set; }
    public int? ExportIterationCount { get; set; }
    public long? CurrentIterationFilesDiscovered { get; set; }
    public long? CurrentIterationMiBDiscovered { get; set; }
    public long? CurrentIterationFilesExported { get; set; }
    public long? CurrentIterationMiBExported { get; set; }
    public long? CurrentIterationFilesFailed { get; set; }
    public DateTime? LastStartedTime { get; set; }
}
