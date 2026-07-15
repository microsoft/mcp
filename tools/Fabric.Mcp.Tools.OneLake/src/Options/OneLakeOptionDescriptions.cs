// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.OneLake.Options;

internal static class OneLakeOptionDescriptions
{
    internal const string WorkspaceId = "The ID of the Microsoft Fabric workspace.";
    internal const string Workspace = "The name or ID of the Microsoft Fabric workspace.";
    internal const string ItemId = "The ID of the Fabric item.";
    internal const string Item = "The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').";
    internal const string FilePath = "The path to the file in Onelake.";
    internal const string DirectoryPath = "The path to the directory in OneLake.";
    internal const string Recursive = "Whether to perform the operation recursively.";
    internal const string DownloadFilePath = "Local path to save the downloaded content when running locally.";
    internal const string Content = "The content to write to the file.";
    internal const string LocalFilePath = "The path to a local file to upload.";
    internal const string Overwrite = "Whether to overwrite existing files.";
    internal const string ContinuationToken = "Token for retrieving the next page of results.";
    internal const string Format = "Output format for OneLake API responses. Use 'json' for parsed objects, 'xml' for raw XML API response, or 'raw' for unprocessed API response. Supported values: 'json' (default), 'xml', 'raw'.";
    internal const string Namespace = "The table namespace (schema) to inspect within the OneLake table API.";
    internal const string Schema = "Alias for --namespace when specifying table schemas in the OneLake table API.";
    internal const string RoleName = "The name of the data access role.";
    internal const string ShortcutName = "The name of the shortcut.";
    internal const string ShortcutPath = "The path of the shortcut within the item.";
    internal const string ShortcutConflictPolicy = "Action when a shortcut with the same name and path already exists. Default: Abort.";
    internal const string TargetLocation = "The target storage URL (e.g. 'https://myaccount.dfs.core.windows.net/container').";
    internal const string TargetSubpath = "The subpath within the target storage location.";
    internal const string TargetConnectionId = "The connection ID (GUID) for authenticating to the target.";
}
