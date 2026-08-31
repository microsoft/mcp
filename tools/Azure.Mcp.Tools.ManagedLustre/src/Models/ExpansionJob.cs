// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public sealed record ExpansionJob
{
    public string? Name { get; init; }
    public string? Id { get; init; }
    public string? Type { get; init; }
    public string? Location { get; init; }
    public ExpansionJobProperties? Properties { get; init; }
}

public sealed record ExpansionJobProperties
{
    public string? ProvisioningState { get; init; }
    public float? NewStorageCapacityTiB { get; init; }
    public ExpansionJobStatus? Status { get; init; }
}

public sealed record ExpansionJobStatus
{
    public string? State { get; init; }
    public string? StatusCode { get; init; }
    public string? StatusMessage { get; init; }
    public float? PercentComplete { get; init; }
    public DateTime? StartTimeUTC { get; init; }
    public DateTime? CompletionTimeUTC { get; init; }
}
