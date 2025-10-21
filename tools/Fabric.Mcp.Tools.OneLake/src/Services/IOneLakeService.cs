// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;

namespace Fabric.Mcp.Tools.OneLake.Services;

/// <summary>
/// Service interface for OneLake operations in Microsoft Fabric.
/// </summary>
public interface IOneLakeService
{
    // Workspace Operations
    Task<IEnumerable<Workspace>> ListWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workspace>> ListOneLakeWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<string> ListOneLakeWorkspacesXmlAsync(string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<Workspace> GetWorkspaceAsync(string workspaceId, CancellationToken cancellationToken = default);
    
    // Item Operations
    Task<IEnumerable<OneLakeItem>> ListItemsAsync(string workspaceId, string? itemType = null, bool recursive = true, string? rootFolderId = null, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<OneLakeItem>> ListOneLakeItemsAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<OneLakeItem> GetItemAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default);
    Task<OneLakeItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default);
    Task<OneLakeItem> UpdateItemAsync(string workspaceId, string itemId, UpdateItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default);
    
    // Lakehouse Operations
    Task<IEnumerable<Lakehouse>> ListLakehousesAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<Lakehouse> GetLakehouseAsync(string workspaceId, string lakehouseId, CancellationToken cancellationToken = default);
    
    // OneLake Shortcuts Operations
    Task<IEnumerable<OneLakeShortcut>> ListShortcutsAsync(string workspaceId, string itemId, string path, CancellationToken cancellationToken = default);
    Task<OneLakeShortcut> GetShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default);
    Task<OneLakeShortcut> CreateShortcutAsync(string workspaceId, string itemId, string path, CreateShortcutRequest request, CancellationToken cancellationToken = default);
    Task DeleteShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default);
    
    // Data Operations (OneLake Data Plane)
    Task<OneLakeFileInfo> GetFileInfoAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<OneLakeFileInfo>> ListFilesAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default);
    Task<Stream> ReadFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
    Task WriteFileAsync(string workspaceId, string itemId, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
}