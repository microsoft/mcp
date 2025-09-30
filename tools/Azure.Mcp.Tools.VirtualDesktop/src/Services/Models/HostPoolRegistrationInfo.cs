// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// Represents a RegistrationInfo definition.
/// </summary>
internal sealed class HostPoolRegistrationInfo
{
    /// <summary> Expiration time of registration token. </summary>
    [JsonPropertyName("expirationTime")]
    public DateTimeOffset? ExpireOn { get; set; }
    /// <summary> The registration token base64 encoded string. </summary>
    public string? Token { get; set; }
    /// <summary> The type of resetting the token. </summary>
    public string? RegistrationTokenOperation { get; set; }
}
