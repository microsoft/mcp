// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.VirtualDesktop.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the health check report data model.
/// </summary>
internal sealed class SessionHostHealthCheckReport
{
    /// <summary> Represents the name of the health check operation performed. </summary>
    public string? HealthCheckName { get; set; }
    /// <summary> Represents the Health state of the health check we performed. </summary>
    public string? HealthCheckResult { get; set; }
    /// <summary> Additional detailed information on the failure. </summary>
    public HealthCheckFailureDetails? AdditionalFailureDetails { get; set; }
}
