// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>Keeps entities in one health state. <see cref="NotHealthy"/> keeps Degraded, Unhealthy and Unknown ones.</summary>
public enum HealthModelHealthFilter
{
    Healthy,
    Degraded,
    Unhealthy,
    Unknown,
    NotHealthy,
}
