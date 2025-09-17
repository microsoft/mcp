// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Models.Elicitation;

/// <summary>
/// Parameters for an elicitation/create request to gather user input.
/// </summary>
public sealed class ElicitationRequestParams
{
    /// <summary>
    /// Gets or sets the message to display to the user requesting information.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the JSON schema defining the structure of the expected response.
    /// </summary>
    [JsonPropertyName("requestedSchema")]
    public required JsonObject RequestedSchema { get; set; }
}
