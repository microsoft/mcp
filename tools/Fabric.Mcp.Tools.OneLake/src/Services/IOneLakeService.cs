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
    Task<IEnumerable<Workspace>> ListOneLakeWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<string> ListOneLakeWorkspacesXmlAsync(string? continuationToken = null, CancellationToken cancellationToken = default);
    
    // Item Operations
    Task<IEnumerable<OneLakeItem>> ListOneLakeItemsAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<string> ListOneLakeItemsXmlAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<string> ListOneLakeItemsDfsJsonAsync(string workspaceId, bool recursive = true, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task<OneLakeItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default);
    
    // OneLake Shortcuts Operations
    Task<IEnumerable<OneLakeShortcut>> ListShortcutsAsync(string workspaceId, string itemId, string path, CancellationToken cancellationToken = default);
    Task<OneLakeShortcut> GetShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default);
    Task<OneLakeShortcut> CreateShortcutAsync(string workspaceId, string itemId, string path, CreateShortcutRequest request, CancellationToken cancellationToken = default);
    Task DeleteShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default);
    
    // Data Operations (OneLake Data Plane)
    Task<OneLakeFileInfo> GetFileInfoAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
    Task<IEnumerable<OneLakeFileInfo>> ListBlobsAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OneLakeFileInfo>> ListBlobsIntelligentAsync(string workspaceId, string itemId, bool recursive = false, CancellationToken cancellationToken = default);
    Task<List<FileSystemItem>> ListPathAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default);
    Task<List<FileSystemItem>> ListPathIntelligentAsync(string workspaceId, string itemId, bool recursive = false, CancellationToken cancellationToken = default);
    
    // Raw Response Methods for Debug/Analysis
    Task<string> ListBlobsRawAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default);
    Task<string> ListPathRawAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default);
    Task<Stream> ReadFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
    Task WriteFileAsync(string workspaceId, string itemId, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
    Task CreateDirectoryAsync(string workspaceId, string itemId, string directoryPath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default);
    Task DeleteDirectoryAsync(string workspaceId, string itemId, string directoryPath, bool recursive = false, CancellationToken cancellationToken = default);
}