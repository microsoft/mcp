// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// Represents a OneLake shortcut.
/// </summary>
public class OneLakeShortcut
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("target")]
    public ShortcutTarget? Target { get; set; }
}

/// <summary>
/// Target configuration for a shortcut.
/// </summary>
public class ShortcutTarget
{
    /// <summary>
    /// Discriminator returned by GET responses. Must NOT be sent on create/update requests.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("oneLake")]
    public OneLakeShortcutTarget? OneLake { get; set; }

    [JsonPropertyName("adlsGen2")]
    public AdlsGen2ShortcutTarget? AdlsGen2 { get; set; }

    [JsonPropertyName("amazonS3")]
    public AmazonS3ShortcutTarget? AmazonS3 { get; set; }

    [JsonPropertyName("googleCloudStorage")]
    public GoogleCloudStorageShortcutTarget? GoogleCloudStorage { get; set; }

    [JsonPropertyName("dataverse")]
    public DataverseShortcutTarget? Dataverse { get; set; }

    [JsonPropertyName("s3Compatible")]
    public S3CompatibleShortcutTarget? S3Compatible { get; set; }

    [JsonPropertyName("externalDataShare")]
    public ExternalDataShareShortcutTarget? ExternalDataShare { get; set; }

    [JsonPropertyName("azureBlobStorage")]
    public AzureBlobStorageShortcutTarget? AzureBlobStorage { get; set; }

    [JsonPropertyName("oneDriveSharePoint")]
    public OneDriveSharePointShortcutTarget? OneDriveSharePoint { get; set; }
}

/// <summary>
/// OneLake shortcut target pointing to another OneLake location.
/// </summary>
public class OneLakeShortcutTarget
{
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("itemId")]
    public string? ItemId { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("connectionId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// ADLS Gen2 shortcut target.
/// </summary>
public class AdlsGen2ShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// Amazon S3 shortcut target.
/// </summary>
public class AmazonS3ShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// Google Cloud Storage shortcut target.
/// </summary>
public class GoogleCloudStorageShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// Dataverse shortcut target.
/// </summary>
public class DataverseShortcutTarget
{
    [JsonPropertyName("environmentDomain")]
    public string? EnvironmentDomain { get; set; }

    [JsonPropertyName("deltaLakeFolder")]
    public string? DeltaLakeFolder { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// S3-compatible shortcut target.
/// </summary>
public class S3CompatibleShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("bucket")]
    public string? Bucket { get; set; }
}

/// <summary>
/// External data share shortcut target.
/// </summary>
public class ExternalDataShareShortcutTarget
{
    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// Azure Blob Storage shortcut target.
/// </summary>
public class AzureBlobStorageShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }
}

/// <summary>
/// OneDrive / SharePoint Online shortcut target.
/// </summary>
public class OneDriveSharePointShortcutTarget
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("subpath")]
    public string? Subpath { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("updateFabricItemSensitivity")]
    public bool? UpdateFabricItemSensitivity { get; set; }
}

/// <summary>
/// Response from the List Shortcuts API.
/// </summary>
public class ShortcutListResponse
{
    [JsonPropertyName("value")]
    public List<OneLakeShortcut>? Value { get; set; }

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("continuationUri")]
    public string? ContinuationUri { get; set; }
}

/// <summary>
/// Request body for the bulk create shortcuts API (POST /shortcuts/bulkCreate).
/// </summary>
public class BulkCreateShortcutsRequest
{
    [JsonPropertyName("createShortcutRequests")]
    public List<CreateShortcutWithTransformRequest>? CreateShortcutRequests { get; set; }
}

/// <summary>
/// A single shortcut creation request (with optional transform).
/// </summary>
public class CreateShortcutWithTransformRequest
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("target")]
    public ShortcutTarget? Target { get; set; }

    [JsonPropertyName("transform")]
    public CsvToDeltaTransform? Transform { get; set; }
}

/// <summary>
/// CSV-to-Delta transform definition.
/// </summary>
public class CsvToDeltaTransform
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("includeSubfolders")]
    public bool? IncludeSubfolders { get; set; }

    [JsonPropertyName("properties")]
    public CsvToDeltaTransformProperties? Properties { get; set; }
}

/// <summary>
/// Properties for the CSV-to-Delta transform.
/// </summary>
public class CsvToDeltaTransformProperties
{
    [JsonPropertyName("delimiter")]
    public string? Delimiter { get; set; }

    [JsonPropertyName("skipFilesWithErrors")]
    public bool? SkipFilesWithErrors { get; set; }

    [JsonPropertyName("useFirstRowAsHeader")]
    public bool? UseFirstRowAsHeader { get; set; }
}

/// <summary>
/// Response from POST /shortcuts/bulkCreate.
/// </summary>
public class BulkCreateShortcutResponse
{
    [JsonPropertyName("value")]
    public List<CreateShortcutResponse>? Value { get; set; }
}

/// <summary>
/// Per-shortcut result in a bulk create response.
/// </summary>
public class CreateShortcutResponse
{
    [JsonPropertyName("request")]
    public ShortcutRequestInfo? Request { get; set; }

    [JsonPropertyName("result")]
    public OneLakeShortcut? Result { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("error")]
    public ShortcutCreateError? Error { get; set; }
}

/// <summary>
/// Original name/path echoed back in a bulk create response item.
/// </summary>
public class ShortcutRequestInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }
}

/// <summary>
/// Error details for a failed shortcut in a bulk create response.
/// </summary>
public class ShortcutCreateError
{
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
