// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.VirtualDesktop.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services.Models;

/// <summary>
/// A class representing the user session data model.
/// </summary>
internal sealed class UserSessionData
{
    /// <summary> The resource ID for the resource. </summary>
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }
    /// <summary> The type of the resource. </summary>
    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }
    /// <summary> The name of the resource. </summary>
    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }
    /// <summary> The location of the resource. </summary>
    public string? Location { get; set; }
    /// <summary> The UserSessionData SKU. </summary>
    public ResourceSku? Sku { get; set; }
    /// <summary> Properties of the UserSessionData. </summary>
    public UserSessionProperties? Properties { get; set; }
    /// <summary> The kind of HostPool. </summary>
    public string? Kind { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static UserSessionData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, VirtualDesktopJsonContext.Default.UserSessionData);
    }
}
