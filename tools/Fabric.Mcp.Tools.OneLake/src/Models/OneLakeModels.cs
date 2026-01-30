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

public sealed record BlobPutResult(
    [property: JsonPropertyName("workspaceId")] string WorkspaceId,
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("contentLength")] long ContentLength,
    [property: JsonPropertyName("contentType")] string ContentType,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified,
    [property: JsonPropertyName("requestId")] string? RequestId,
    [property: JsonPropertyName("version")] string? Version = null,
    [property: JsonPropertyName("requestServerEncrypted")] bool? RequestServerEncrypted = null,
    [property: JsonPropertyName("contentMd5")] string? ContentMd5 = null,
    [property: JsonPropertyName("contentCrc64")] string? ContentCrc64 = null,
    [property: JsonPropertyName("encryptionScope")] string? EncryptionScope = null,
    [property: JsonPropertyName("encryptionKeySha256")] string? EncryptionKeySha256 = null,
    [property: JsonPropertyName("versionId")] string? VersionId = null,
    [property: JsonPropertyName("clientRequestId")] string? ClientRequestId = null,
    [property: JsonPropertyName("rootActivityId")] string? RootActivityId = null
);

public sealed record BlobGetResult(
    [property: JsonPropertyName("workspaceId")] string WorkspaceId,
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("contentLength")] long? ContentLength,
    [property: JsonPropertyName("contentType")] string? ContentType,
    [property: JsonPropertyName("charset")] string? Charset,
    [property: JsonPropertyName("contentEncoding")] string? ContentEncoding,
    [property: JsonPropertyName("contentLanguage")] string? ContentLanguage,
    [property: JsonPropertyName("contentDisposition")] string? ContentDisposition,
    [property: JsonPropertyName("contentMd5")] string? ContentMd5,
    [property: JsonPropertyName("contentCrc64")] string? ContentCrc64,
    [property: JsonPropertyName("contentBase64")] string? ContentBase64,
    [property: JsonPropertyName("contentText")] string? ContentText,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified,
    [property: JsonPropertyName("requestServerEncrypted")] bool? RequestServerEncrypted,
    [property: JsonPropertyName("encryptionScope")] string? EncryptionScope,
    [property: JsonPropertyName("encryptionKeySha256")] string? EncryptionKeySha256,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("versionId")] string? VersionId,
    [property: JsonPropertyName("requestId")] string? RequestId,
    [property: JsonPropertyName("clientRequestId")] string? ClientRequestId,
    [property: JsonPropertyName("rootActivityId")] string? RootActivityId
)
{
    [JsonPropertyName("contentFilePath")]
    public string? ContentFilePath { get; init; }

    [JsonPropertyName("inlineContentTruncated")]
    public bool InlineContentTruncated { get; init; }
}

public sealed record BlobDeleteResult(
    [property: JsonPropertyName("workspaceId")] string WorkspaceId,
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("versionId")] string? VersionId,
    [property: JsonPropertyName("requestId")] string? RequestId,
    [property: JsonPropertyName("clientRequestId")] string? ClientRequestId,
    [property: JsonPropertyName("rootActivityId")] string? RootActivityId
);

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

// Endpoint and authentication models
public class OneLakeEndpoint
{
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    [JsonPropertyName("provisioningStatus")]
    public string? ProvisioningStatus { get; set; }
}

// File system models for hierarchical directory views
public class FileSystemItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "file" or "directory"

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    [JsonPropertyName("etag")]
    public string? ETag { get; set; }

    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }

    [JsonPropertyName("owner")]
    public string? Owner { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    [JsonPropertyName("isDirectory")]
    public bool IsDirectory => Type == "directory";

    [JsonPropertyName("children")]
    public List<FileSystemItem>? Children { get; set; }
}

// Configuration and constants
public static class OneLakeEndpoints
{
    public const string FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1";
    public const string StorageScope = "https://storage.azure.com/.default";

    // Environment-aware Fabric API base URL
    public static string GetFabricApiBaseUrl() => ProductionEndpoints.FabricApiBaseUrl;

    // Environment-aware Fabric authentication scope
    public static string GetFabricScope() => ProductionEndpoints.FabricScope;

    public static readonly string[] FabricScopes =
    [
        "https://api.fabric.microsoft.com/.default"
    ];

    // Production environment endpoints
    private static readonly OneLakeEnvironmentEndpoints ProductionEndpoints = new()
    {
        OneLakeDataPlaneBaseUrl = "https://api.onelake.fabric.microsoft.com",
        OneLakeDataPlaneDfsBaseUrl = "https://onelake.dfs.fabric.microsoft.com",
        OneLakeDataPlaneBlobBaseUrl = "https://onelake.blob.fabric.microsoft.com",
        OneLakeTableApiBaseUrl = "https://onelake.table.fabric.microsoft.com",
        FabricApiBaseUrl = "https://api.fabric.microsoft.com/v1",
        FabricScope = "https://api.fabric.microsoft.com/.default"
    };

    // Public properties that return production URLs
    public static string OneLakeDataPlaneBaseUrl => ProductionEndpoints.OneLakeDataPlaneBaseUrl;

    public static string OneLakeDataPlaneDfsBaseUrl => ProductionEndpoints.OneLakeDataPlaneDfsBaseUrl;

    public static string OneLakeDataPlaneBlobBaseUrl => ProductionEndpoints.OneLakeDataPlaneBlobBaseUrl;

    public static string OneLakeTableApiBaseUrl => ProductionEndpoints.OneLakeTableApiBaseUrl;

    // Method to get production endpoints
    public static OneLakeEnvironmentEndpoints GetEndpoints(string environment) => ProductionEndpoints;
}

public class OneLakeEnvironmentEndpoints
{
    public string OneLakeDataPlaneBaseUrl { get; set; } = string.Empty;
    public string OneLakeDataPlaneDfsBaseUrl { get; set; } = string.Empty;
    public string OneLakeDataPlaneBlobBaseUrl { get; set; } = string.Empty;
    public string OneLakeTableApiBaseUrl { get; set; } = string.Empty;
    public string FabricApiBaseUrl { get; set; } = string.Empty;
    public string FabricScope { get; set; } = string.Empty;
}
