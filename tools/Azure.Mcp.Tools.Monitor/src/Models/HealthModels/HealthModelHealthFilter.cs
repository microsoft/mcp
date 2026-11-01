// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>Keeps entities in one health state; <see cref="NotHealthy"/> is Degraded, Unhealthy or Unknown.</summary>
public enum HealthModelHealthFilter
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown,
    NotHealthy,
}
