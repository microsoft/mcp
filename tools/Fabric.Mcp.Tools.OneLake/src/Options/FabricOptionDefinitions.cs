// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.OneLake.Options;

public static class FabricOptionDefinitions
{

    // Workspace options
    public const string WorkspaceName = "workspace";
    public static readonly Option<string> Workspace = new($"--{WorkspaceName}")
    {
        Description = "The name or ID of the Microsoft Fabric workspace.",
        Required = true
    };

    public const string WorkspaceIdName = "workspace-id";
    public static readonly Option<string> WorkspaceId = new($"--{WorkspaceIdName}")
    {
        Description = "The ID of the Microsoft Fabric workspace.",
        Required = true
    };

    // Item options
    public const string ItemName = "item";
    public static readonly Option<string> Item = new($"--{ItemName}")
    {
        Description = "The name or ID of the Fabric item.",
        Required = false
    };

    public const string ItemIdName = "item-id";
    public static readonly Option<string> ItemId = new($"--{ItemIdName}")
    {
        Description = "The ID of the Fabric item.",
        Required = true
    };

    public const string ItemTypeName = "item-type";
    public static readonly Option<string> ItemType = new($"--{ItemTypeName}")
    {
        Description = "The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).",
        Required = false
    };

    // Lakehouse options
    public const string LakehouseIdName = "lakehouse-id";
    public static readonly Option<string> LakehouseId = new($"--{LakehouseIdName}")
    {
        Description = "The ID of the Lakehouse.",
        Required = true
    };

    // File path options
    public const string FilePathName = "file-path";
    public static readonly Option<string> FilePath = new($"--{FilePathName}")
    {
        Description = "The path to the file in OneLake.",
        Required = true
    };

    public const string DirectoryPathName = "directory-path";
    public static readonly Option<string> DirectoryPath = new($"--{DirectoryPathName}")
    {
        Description = "The path to the directory in OneLake.",
        Required = false
    };

    public const string PathName = "path";
    public static readonly Option<string> Path = new($"--{PathName}")
    {
        Description = "The path to list in OneLake storage (optional, defaults to root).",
        Required = false
    };

    // Shortcut options
    public const string ShortcutNameName = "shortcut-name";
    public static readonly Option<string> ShortcutName = new($"--{ShortcutNameName}")
    {
        Description = "The name of the OneLake shortcut.",
        Required = true
    };

    public const string ShortcutPathName = "shortcut-path";
    public static readonly Option<string> ShortcutPath = new($"--{ShortcutPathName}")
    {
        Description = "The path where the shortcut is located.",
        Required = true
    };

    public const string TargetTypeName = "target-type";
    public static readonly Option<string> TargetType = new($"--{TargetTypeName}")
    {
        Description = "The type of shortcut target (OneLake, AdlsGen2, S3).",
        Required = true
    };

    public const string TargetUrlName = "target-url";
    public static readonly Option<string> TargetUrl = new($"--{TargetUrlName}")
    {
        Description = "The URL of the shortcut target.",
        Required = false
    };

    public const string TargetWorkspaceIdName = "target-workspace-id";
    public static readonly Option<string> TargetWorkspaceId = new($"--{TargetWorkspaceIdName}")
    {
        Description = "The workspace ID for OneLake shortcut target.",
        Required = false
    };

    public const string TargetItemIdName = "target-item-id";
    public static readonly Option<string> TargetItemId = new($"--{TargetItemIdName}")
    {
        Description = "The item ID for OneLake shortcut target.",
        Required = false
    };

    public const string TargetPathName = "target-path";
    public static readonly Option<string> TargetPath = new($"--{TargetPathName}")
    {
        Description = "The path for the shortcut target.",
        Required = false
    };

    // Data operation options
    public const string RecursiveName = "recursive";
    public static readonly Option<bool> Recursive = new($"--{RecursiveName}")
    {
        Description = "Whether to perform the operation recursively.",
        Required = false
    };

    public const string OverwriteName = "overwrite";
    public static readonly Option<bool> Overwrite = new($"--{OverwriteName}")
    {
        Description = "Whether to overwrite existing files.",
        Required = false
    };

    public const string ContentName = "content";
    public static readonly Option<string> Content = new($"--{ContentName}")
    {
        Description = "The content to write to the file.",
        Required = false
    };

    public const string LocalFilePathName = "local-file-path";
    public static readonly Option<string> LocalFilePath = new($"--{LocalFilePathName}")
    {
        Description = "The path to a local file to upload.",
        Required = false
    };

    // Display options
    public const string DisplayNameName = "display-name";
    public static readonly Option<string> DisplayName = new($"--{DisplayNameName}")
    {
        Description = "The display name for the item.",
        Required = true
    };

    public const string DescriptionName = "description";
    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "The description for the item.",
        Required = false
    };

    // Connection options
    public const string ConnectionIdName = "connection-id";
    public static readonly Option<string> ConnectionId = new($"--{ConnectionIdName}")
    {
        Description = "The connection ID for external data sources.",
        Required = false
    };

    // Pagination options
    public const string ContinuationTokenName = "continuation-token";
    public static readonly Option<string> ContinuationToken = new($"--{ContinuationTokenName}")
    {
        Description = "Token for retrieving the next page of results.",
        Required = false
    };

    // Endpoint options
    public const string EndpointTypeName = "endpoint-type";
    public static readonly Option<string> EndpointType = new($"--{EndpointTypeName}")
    {
        Description = "The endpoint type to use for listing items (fabric-api, blob, auto). Default is 'auto' which uses blob endpoint when appropriate.",
        Required = false
    };
}