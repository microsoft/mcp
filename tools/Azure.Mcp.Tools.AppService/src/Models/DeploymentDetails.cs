// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents details about a Web App.
/// </summary>
public sealed record DeploymentDetails(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("active")] bool? Active,
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("author")] string Author,
    [property: JsonPropertyName("deployer")] string Deployer,
    [property: JsonPropertyName("startTime")] DateTimeOffset? StartTime,
    [property: JsonPropertyName("endTime")] DateTimeOffset? EndTime);
