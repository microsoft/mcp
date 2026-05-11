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
    [JsonPropertyName("type")]
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
/// Request body for the Create Or Update Shortcuts API.
/// </summary>
public class ShortcutCreateOrUpdateRequest
{
    [JsonPropertyName("shortcuts")]
    public List<OneLakeShortcut>? Shortcuts { get; set; }

    [JsonPropertyName("createOrOverwrite")]
    public bool? CreateOrOverwrite { get; set; }
}

/// <summary>
/// Response from the Create Or Update Shortcuts API.
/// </summary>
public class ShortcutCreateOrUpdateResponse
{
    [JsonPropertyName("value")]
    public List<OneLakeShortcut>? Value { get; set; }
}
