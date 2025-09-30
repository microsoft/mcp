// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.VirtualDesktop.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the SessionHostProperties data model.
/// A storage account resource.
/// </summary>
internal sealed class SessionHostProperties
{
    /// <summary> ObjectId of SessionHost. (internal use). </summary>
    public string? ObjectId { get; set; }
    /// <summary> Last heart beat from SessionHost. </summary>
    [JsonPropertyName("lastHeartBeat")]
    public DateTimeOffset? LastHeartBeatOn { get; set; }
    /// <summary> Number of sessions on SessionHost. </summary>
    public int? Sessions { get; set; }
    /// <summary> Version of agent on SessionHost. </summary>
    public string? AgentVersion { get; set; }
    /// <summary> Allow a new session. </summary>
    public bool? AllowNewSession { get; set; }
    /// <summary> Virtual Machine Id of SessionHost's underlying virtual machine. </summary>
    [JsonPropertyName("virtualMachineId")]
    public string? VmId { get; set; }
    /// <summary> Resource Id of SessionHost's underlying virtual machine. </summary>
    public string? ResourceId { get; set; }
    /// <summary> User assigned to SessionHost. </summary>
    public string? AssignedUser { get; set; }
    /// <summary> Friendly name of SessionHost. </summary>
    public string? FriendlyName { get; set; }
    /// <summary> Status for a SessionHost. </summary>
    public string? Status { get; set; }
    /// <summary> The timestamp of the status. </summary>
    public DateTimeOffset? StatusTimestamp { get; set; }
    /// <summary> The version of the OS on the session host. </summary>
    [JsonPropertyName("osVersion")]
    public string? OSVersion { get; set; }
    /// <summary> The version of the side by side stack on the session host. </summary>
    [JsonPropertyName("sxSStackVersion")]
    public string? SxsStackVersion { get; set; }
    /// <summary> Update state of a SessionHost. </summary>
    public string? UpdateState { get; set; }
    /// <summary> The timestamp of the last update. </summary>
    [JsonPropertyName("lastUpdateTime")]
    public DateTimeOffset? LastUpdatedOn { get; set; }
    /// <summary> The error message. </summary>
    public string? UpdateErrorMessage { get; set; }
    /// <summary> List of SessionHostHealthCheckReports. </summary>
    public IReadOnlyList<SessionHostHealthCheckReport>? SessionHostHealthCheckResults { get; set; }
}
