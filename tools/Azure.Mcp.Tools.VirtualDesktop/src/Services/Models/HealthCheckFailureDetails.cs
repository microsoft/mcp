// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.VirtualDesktop.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the HealthCheckFailureDetails data model.
/// A storage account resource.
/// </summary>
internal sealed class HealthCheckFailureDetails
{
    /// <summary> Failure message: hints on what is wrong and how to recover. </summary>
        public string? Message { get; set; }
        /// <summary> Error code corresponding for the failure. </summary>
        public int? ErrorCode { get; set; }
        /// <summary> The timestamp of the last update. </summary>
        [JsonPropertyName("lastHealthCheckDateTime")]
        public DateTimeOffset? LastHealthCheckOn { get; set; }
}
