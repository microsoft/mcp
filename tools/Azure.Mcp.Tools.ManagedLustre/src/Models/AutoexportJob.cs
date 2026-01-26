// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public sealed record AutoexportJob
{
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required string ProvisioningState { get; init; }
    public string? AdminStatus { get; init; }
    public string[]? AutoExportPrefixes { get; init; }
    public string? State { get; init; }
    public string? StatusCode { get; init; }
    public string? StatusMessage { get; init; }
    public long? TotalFilesExported { get; init; }
    public long? TotalMiBExported { get; init; }
    public long? TotalFilesFailed { get; init; }
    public int? ExportIterationCount { get; init; }
    public DateTimeOffset? LastSuccessfulIterationCompletionTimeUTC { get; init; }
    public long? CurrentIterationFilesDiscovered { get; init; }
    public long? CurrentIterationMiBDiscovered { get; init; }
    public long? CurrentIterationFilesExported { get; init; }
    public long? CurrentIterationMiBExported { get; init; }
    public long? CurrentIterationFilesFailed { get; init; }
    public DateTimeOffset? LastStartedTimeUTC { get; init; }
    public DateTimeOffset? LastCompletionTimeUTC { get; init; }
}
