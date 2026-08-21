// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public class ExpansionJob
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public ExpansionJobProperties? Properties { get; set; }
}

public class ExpansionJobProperties
{
    public string? ProvisioningState { get; set; }
    public float? NewStorageCapacityTiB { get; set; }
    public ExpansionJobStatus? Status { get; set; }
}

public class ExpansionJobStatus
{
    public string? State { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusMessage { get; set; }
    public float? PercentComplete { get; set; }
    public DateTime? StartTimeUTC { get; set; }
    public DateTime? CompletionTimeUTC { get; set; }
}
