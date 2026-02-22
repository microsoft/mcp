// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// Represents a Fabric item (Lakehouse, Notebook, etc.)
/// </summary>
public class FabricItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    [JsonPropertyName("definition")]
    public object? Definition { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime? CreatedDate { get; set; }

    [JsonPropertyName("lastModifiedDate")]
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Request model for creating a Fabric item
/// </summary>
public class CreateItemRequest
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("definition")]
    public object? Definition { get; set; }
}

/// <summary>
/// Configuration and constants for Fabric API endpoints
/// </summary>
public static class FabricEndpoints
{
    private static readonly Dictionary<string, FabricEnvironmentEndpoints> EnvironmentEndpoints = new()
    {
        ["PROD"] = new FabricEnvironmentEndpoints
        {
            FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1",
            FabricScope = "https://api.fabric.microsoft.com/.default"
        },
        ["DAILY"] = new FabricEnvironmentEndpoints
        {
            FabricApiBaseUrl = "https://dailyapi.fabric.microsoft.com/v1",
            FabricScope = "https://api.fabric.microsoft.com/.default"
        },
        ["DXT"] = new FabricEnvironmentEndpoints
        {
            FabricApiBaseUrl = "https://dxt-api.fabric.microsoft.com/v1",
            FabricScope = "https://api.fabric.microsoft.com/.default"
        },
        ["MSIT"] = new FabricEnvironmentEndpoints
        {
            FabricApiBaseUrl = "https://msit-api.fabric.microsoft.com/v1",
            FabricScope = "https://api.fabric.microsoft.com/.default"
        }
    };

    private static string CurrentEnvironment =>
        Environment.GetEnvironmentVariable("FABRIC_ENVIRONMENT")?.ToUpperInvariant() ?? "PROD";

    public static string GetFabricApiBaseUrl() => GetEndpoints(CurrentEnvironment).FabricApiBaseUrl;

    public static string GetFabricScope() => GetEndpoints(CurrentEnvironment).FabricScope;

    public static readonly string[] FabricScopes = ["https://api.fabric.microsoft.com/.default"];

    public static FabricEnvironmentEndpoints GetEndpoints(string environment)
    {
        var env = environment.ToUpperInvariant();
        return EnvironmentEndpoints.TryGetValue(env, out var endpoints)
            ? endpoints
            : EnvironmentEndpoints["PROD"];
    }

    public static IEnumerable<string> GetAvailableEnvironments() => EnvironmentEndpoints.Keys;
}

public class FabricEnvironmentEndpoints
{
    public string FabricApiBaseUrl { get; set; } = string.Empty;
    public string FabricScope { get; set; } = string.Empty;
}
