// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

// Core OneLake entities
public class Workspace
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Workspace";

    [JsonPropertyName("capacityId")]
    public string? CapacityId { get; set; }

    [JsonPropertyName("defaultDatasetStorageFormat")]
    public string? DefaultDatasetStorageFormat { get; set; }

    [JsonPropertyName("properties")]
    public WorkspaceProperties? Properties { get; set; }

    [JsonPropertyName("metadata")]
    public WorkspaceMetadata? Metadata { get; set; }
}

public class WorkspaceProperties
{
    [JsonPropertyName("lastModified")]
    public DateTime? LastModified { get; set; }
}

public class WorkspaceMetadata
{
    [JsonPropertyName("regionalServiceEndpoint")]
    public string? RegionalServiceEndpoint { get; set; }

    [JsonPropertyName("workspaceObjectId")]
    public string? WorkspaceObjectId { get; set; }

    [JsonPropertyName("workspacePortalUrl")]
    public string? WorkspacePortalUrl { get; set; }
}

public class OneLakeItem
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

    [JsonPropertyName("metadata")]
    public OneLakeItemMetadata? Metadata { get; set; }
}

public class OneLakeItemMetadata
{
    [JsonPropertyName("artifactId")]
    public string? ArtifactId { get; set; }

    [JsonPropertyName("artifactPortalUrl")]
    public string? ArtifactPortalUrl { get; set; }

    [JsonPropertyName("resourceType")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("blobType")]
    public string? BlobType { get; set; }
}

public class Lakehouse : OneLakeItem
{
    [JsonPropertyName("sqlAnalyticsEndpoint")]
    public OneLakeEndpoint? SqlAnalyticsEndpoint { get; set; }

    [JsonPropertyName("oneLakeTablesPath")]
    public string? OneLakeTablesPath { get; set; }

    [JsonPropertyName("oneLakeFilesPath")]
    public string? OneLakeFilesPath { get; set; }

    [JsonPropertyName("defaultLakehouseSchema")]
    public string? DefaultLakehouseSchema { get; set; }

    [JsonPropertyName("defaultLakehouseWorkspace")]
    public string? DefaultLakehouseWorkspace { get; set; }
}

// OneLake shortcuts
public class OneLakeShortcut
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public ShortcutTarget Target { get; set; } = new();

    [JsonPropertyName("createdDate")]
    public DateTime? CreatedDate { get; set; }

    [JsonPropertyName("lastModifiedDate")]
    public DateTime? LastModifiedDate { get; set; }
}

public class ShortcutTarget
{
    [JsonPropertyName("oneLakeShortcut")]
    public OneLakeShortcutTarget? OneLakeShortcut { get; set; }

    [JsonPropertyName("adlsGen2")]
    public AdlsGen2ShortcutTarget? AdlsGen2 { get; set; }

    [JsonPropertyName("s3")]
    public S3ShortcutTarget? S3 { get; set; }
}

public class OneLakeShortcutTarget
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    [JsonPropertyName("itemId")]
    public string ItemId { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}

public class AdlsGen2ShortcutTarget
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }
}

public class S3ShortcutTarget
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }
}

// File and directory information
public class OneLakeFileInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("isDirectory")]
    public bool IsDirectory { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime? LastModified { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    [JsonPropertyName("etag")]
    public string? ETag { get; set; }
}

// Request/response models
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

public class UpdateItemRequest
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("definition")]
    public object? Definition { get; set; }
}

public class CreateShortcutRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public ShortcutTarget Target { get; set; } = new();
}

// Collection response models
public class WorkspacesResponse
{
    [JsonPropertyName("value")]
    public List<Workspace> Value { get; set; } = [];

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}

public class ItemsResponse
{
    [JsonPropertyName("value")]
    public List<OneLakeItem> Value { get; set; } = [];

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}

public class LakehousesResponse
{
    [JsonPropertyName("value")]
    public List<Lakehouse> Value { get; set; } = [];

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}

public class ShortcutsResponse
{
    [JsonPropertyName("value")]
    public List<OneLakeShortcut> Value { get; set; } = [];

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }
}

// Endpoint and authentication models
public class OneLakeEndpoint
{
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    [JsonPropertyName("provisioningStatus")]
    public string? ProvisioningStatus { get; set; }
}

// Configuration and constants
public static class OneLakeEndpoints
{
    public const string FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1";
    public const string StorageScope = "https://storage.azure.com/.default";
    
    public static readonly string[] FabricScopes =
    [
        "https://api.fabric.microsoft.com/.default"
    ];

    // Environment-specific endpoints
    private static readonly Dictionary<string, OneLakeEnvironmentEndpoints> EnvironmentEndpoints = new()
    {
        ["PROD"] = new OneLakeEnvironmentEndpoints
        {
            OneLakeDataPlaneBaseUrl = "https://api.onelake.fabric.microsoft.com",
            OneLakeDataPlaneDfsBaseUrl = "https://onelake.dfs.fabric.microsoft.com",
            OneLakeDataPlaneBlobBaseUrl = "https://onelake.blob.fabric.microsoft.com"
        },
        ["DAILY"] = new OneLakeEnvironmentEndpoints
        {
            OneLakeDataPlaneBaseUrl = "https://daily-api.onelake.fabric.microsoft.com",
            OneLakeDataPlaneDfsBaseUrl = "https://daily-onelake.dfs.fabric.microsoft.com",
            OneLakeDataPlaneBlobBaseUrl = "https://daily-onelake.blob.fabric.microsoft.com"
        },
        ["DXT"] = new OneLakeEnvironmentEndpoints
        {
            OneLakeDataPlaneBaseUrl = "https://dxt-api.onelake.fabric.microsoft.com",
            OneLakeDataPlaneDfsBaseUrl = "https://dxt-onelake.dfs.fabric.microsoft.com",
            OneLakeDataPlaneBlobBaseUrl = "https://dxt-onelake.blob.fabric.microsoft.com"
        },
        ["MSIT"] = new OneLakeEnvironmentEndpoints
        {
            OneLakeDataPlaneBaseUrl = "https://msit-api.onelake.fabric.microsoft.com",
            OneLakeDataPlaneDfsBaseUrl = "https://msit-onelake.dfs.fabric.microsoft.com",
            OneLakeDataPlaneBlobBaseUrl = "https://msit-onelake.blob.fabric.microsoft.com"
        }
    };

    // Get current environment from environment variable or default to PROD
    private static string CurrentEnvironment => 
        Environment.GetEnvironmentVariable("ONELAKE_ENVIRONMENT")?.ToUpperInvariant() ?? "PROD";

    // Public properties that return environment-specific URLs
    public static string OneLakeDataPlaneBaseUrl => 
        EnvironmentEndpoints[CurrentEnvironment].OneLakeDataPlaneBaseUrl;

    public static string OneLakeDataPlaneDfsBaseUrl => 
        EnvironmentEndpoints[CurrentEnvironment].OneLakeDataPlaneDfsBaseUrl;

    public static string OneLakeDataPlaneBlobBaseUrl => 
        EnvironmentEndpoints[CurrentEnvironment].OneLakeDataPlaneBlobBaseUrl;

    // Method to get endpoints for a specific environment
    public static OneLakeEnvironmentEndpoints GetEndpoints(string environment)
    {
        var env = environment.ToUpperInvariant();
        return EnvironmentEndpoints.TryGetValue(env, out var endpoints) 
            ? endpoints 
            : EnvironmentEndpoints["PROD"];
    }

    // Method to list available environments
    public static IEnumerable<string> GetAvailableEnvironments() => EnvironmentEndpoints.Keys;
}

public class OneLakeEnvironmentEndpoints
{
    public string OneLakeDataPlaneBaseUrl { get; set; } = string.Empty;
    public string OneLakeDataPlaneDfsBaseUrl { get; set; } = string.Empty;
    public string OneLakeDataPlaneBlobBaseUrl { get; set; } = string.Empty;
}