// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Models.Elicitation;

/// <summary>
/// Response from an elicitation/create request containing the user's action and data.
/// </summary>
public sealed class ElicitationResponse
{
    /// <summary>
    /// Gets or sets the action taken by the user.
    /// </summary>
    [JsonPropertyName("action")]
    public required ElicitationAction Action { get; set; }

    /// <summary>
    /// Gets or sets the user-provided content when action is "accept".
    /// </summary>
    [JsonPropertyName("content")]
    public JsonObject? Content { get; set; }
}

/// <summary>
/// The action taken by the user in response to an elicitation request.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ElicitationAction>))]
public enum ElicitationAction
{
    /// <summary>
    /// User explicitly approved and submitted with data.
    /// </summary>
    Accept,

    /// <summary>
    /// User explicitly declined the request.
    /// </summary>
    Decline,

    /// <summary>
    /// User dismissed without making an explicit choice.
    /// </summary>
    Cancel
}
