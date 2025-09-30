// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.VirtualDesktop.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the user session properties.
/// </summary>
internal sealed class UserSessionProperties
{
    /// <summary> ObjectId of user session. (internal use). </summary>
    public string? ObjectId { get; set; }
    /// <summary> The user principal name. </summary>
    public string? UserPrincipalName { get; set; }
    /// <summary> Application type of application. </summary>
    public string? ApplicationType { get; set; }
    /// <summary> State of user session. </summary>
    public string? SessionState { get; set; }
    /// <summary> The active directory user name. </summary>
    public string? ActiveDirectoryUserName { get; set; }
    /// <summary> The timestamp of the user session create. </summary>
    [JsonPropertyName("createTime")]
    public DateTimeOffset? CreateOn { get; set; }
}
