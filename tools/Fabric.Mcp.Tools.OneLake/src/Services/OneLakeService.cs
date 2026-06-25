// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.OneLake.Models;

namespace Fabric.Mcp.Tools.OneLake.Services;

public class OneLakeService(HttpClient httpClient, TokenCredential? credential = null) : IOneLakeService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? new DefaultAzureCredential();
    private const string UserAgentHeaderName = "User-Agent";
    private const string UserAgentHeaderValue = "OneLake MCP";
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _itemIdentifierCache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<string> ResolveItemIdentifierAsync(string workspaceId, string itemIdentifier, CancellationToken cancellationToken = default)
    {
        var normalizedWorkspaceId = NormalizeWorkspaceIdentifier(workspaceId);
        var normalizedItemInput = NormalizeItemIdentifier(itemIdentifier);

        if (Guid.TryParse(normalizedItemInput, out _))
        {
            return normalizedItemInput;
        }

        if (normalizedItemInput.Contains('.'))
        {
            return normalizedItemInput.TrimEnd('/');
        }

        var workspaceCache = GetWorkspaceItemCache(normalizedWorkspaceId);
        if (workspaceCache.TryGetValue(normalizedItemInput, out var cachedIdentifier))
        {
            return cachedIdentifier;
        }

        var items = await ListOneLakeItemsAsync(normalizedWorkspaceId, cancellationToken: cancellationToken);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                continue;
            }

            var artifactId = NormalizeItemIdentifier(item.Id);
            workspaceCache.TryAdd(artifactId, artifactId);

            if (!string.IsNullOrWhiteSpace(item.DisplayName))
            {
                workspaceCache.TryAdd(item.DisplayName.Trim(), artifactId);
            }

            var artifactGuid = item.Metadata?.ArtifactId;
            if (!string.IsNullOrWhiteSpace(artifactGuid))
            {
                workspaceCache.TryAdd(artifactGuid.Trim(), artifactId);
            }
        }

        if (workspaceCache.TryGetValue(normalizedItemInput, out cachedIdentifier))
        {
            return cachedIdentifier;
        }

        throw new InvalidOperationException($"Unable to resolve item '{itemIdentifier}' in workspace '{workspaceId}'. Provide the full item identifier including its suffix, for example 'ItemName.Lakehouse'.");
    }

    private ConcurrentDictionary<string, string> GetWorkspaceItemCache(string workspaceId)
    {
        return _itemIdentifierCache.GetOrAdd(workspaceId, static _ => new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeWorkspaceIdentifier(string workspaceId)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceId));
        }

        return workspaceId.Trim();
    }

    private static string NormalizeItemIdentifier(string itemIdentifier)
    {
        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        return itemIdentifier.Trim().TrimEnd('/');
    }

    private async Task<(string WorkspaceId, string ItemId)> GetNormalizedIdentifiersAsync(string workspaceId, string itemId, CancellationToken cancellationToken)
    {
        var normalizedWorkspaceId = NormalizeWorkspaceIdentifier(workspaceId);
        var normalizedItemId = await ResolveItemIdentifierAsync(normalizedWorkspaceId, itemId, cancellationToken);
        return (normalizedWorkspaceId, normalizedItemId);
    }

    // Workspace Operations
    public async Task<IEnumerable<Workspace>> ListOneLakeWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);

        using var reader = new StreamReader(response);
        var xmlContent = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var containers = doc.Root?.Element("Containers")?.Elements("Container") ?? [];

            var workspaces = containers.Select(container =>
            {
                var workspace = new Workspace
                {
                    Id = container.Element("Name")?.Value ?? string.Empty,
                    DisplayName = container.Element("Name")?.Value ?? string.Empty
                };

                var propertiesElement = container.Element("Properties");
                if (propertiesElement != null)
                {
                    workspace.Properties = new WorkspaceProperties();
                    var lastModifiedString = propertiesElement.Element("Last-Modified")?.Value;
                    if (!string.IsNullOrEmpty(lastModifiedString) && DateTime.TryParse(lastModifiedString, out var lastModified))
                    {
                        workspace.Properties.LastModified = lastModified;
                    }
                }

                var metadataElement = container.Element("Metadata");
                if (metadataElement != null)
                {
                    workspace.Metadata = new WorkspaceMetadata
                    {
                        RegionalServiceEndpoint = metadataElement.Element("RegionalServiceEndpoint")?.Value,
                        WorkspaceObjectId = metadataElement.Element("WorkspaceObjectId")?.Value,
                        WorkspacePortalUrl = metadataElement.Element("WorkspacePortalUrl")?.Value
                    };
                }

                return workspace;
            }).ToList();

            return workspaces;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse OneLake workspace list response.", ex);
        }
    }

    public async Task<string> ListOneLakeWorkspacesXmlAsync(string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);

        using var reader = new StreamReader(response);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    // Item Operations
    public async Task<OneLakeItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items";
        var jsonContent = JsonSerializer.Serialize(request, OneLakeJsonContext.Default.CreateItemRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<OneLakeItem>(response, OneLakeJsonContext.Default.OneLakeItem, cancellationToken) ?? new OneLakeItem();
    }

    // Private helper method for internal use
    private async Task<Workspace> GetWorkspaceAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync<Workspace>(response, OneLakeJsonContext.Default.Workspace, cancellationToken) ?? new Workspace();
    }

    // Data Operations (OneLake Data Plane)
    public async Task<OneLakeFileInfo> GetFileInfoAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(filePath, nameof(filePath));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/Files/{filePath.TrimStart('/')}";
        var response = await SendDataPlaneRequestAsync(HttpMethod.Head, url, cancellationToken: cancellationToken);

        return new OneLakeFileInfo
        {
            Name = Path.GetFileName(filePath),
            Path = filePath,
            IsDirectory = false,
            Size = GetContentLength(response.Headers),
            LastModified = GetLastModified(response.Headers),
            ContentType = response.Content.Headers.ContentType?.ToString(),
            ETag = response.Headers.ETag?.ToString()
        };
    }

    public async Task<IEnumerable<OneLakeFileInfo>> ListBlobsAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (path is not null)
            ValidatePathForTraversal(path, nameof(path));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);

        // If no path is specified, intelligently discover and search top-level folders
        if (string.IsNullOrEmpty(path))
        {
            return await ListBlobsIntelligentAsync(normalizedWorkspaceId, normalizedItemId, recursive, cancellationToken);
        }

        // Blob API: path is part of the URL path per Blob Storage List Blobs spec
        var directory = ResolveDirectoryPath(path);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{directory}";
        url += "?restype=container&comp=list";
        if (recursive)
        {
            url += "&recursive=true";
        }

        var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        // Parse XML response to extract file information
        var files = ParseBlobListResponse(content);
        return files.OrderBy(f => f.IsDirectory ? 0 : 1).ThenBy(f => f.Name);
    }

    private static bool IsTopLevelFolder(string path)
    {
        // Check if the path represents a top-level folder in OneLake
        // Common top-level folders include Tables, Files, and potentially others
        var folder = path.Split('/')[0]; // Get the first segment
        return folder.Equals("Tables", StringComparison.OrdinalIgnoreCase) ||
               folder.Equals("Files", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<OneLakeFileInfo>> ListBlobsIntelligentAsync(string workspaceId, string itemId, bool recursive, CancellationToken cancellationToken)
    {
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // Intelligent discovery: Try to list contents from both Files and Tables folders
        var allFiles = new List<OneLakeFileInfo>();
        var topLevelFolders = new[] { "Files", "Tables" };

        foreach (var folder in topLevelFolders)
        {
            try
            {
                var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{folder}";
                url += $"?restype=container&comp=list";
                if (recursive)
                {
                    url += "&recursive=true";
                }

                var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                // Parse XML response to extract file information
                var files = ParseBlobListResponse(content);
                allFiles.AddRange(files);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                // Folder doesn't exist, skip it
                continue;
            }
            catch (Exception)
            {
                // Other errors, skip this folder but continue with others
                continue;
            }
        }

        return allFiles.OrderBy(f => f.IsDirectory ? 0 : 1).ThenBy(f => f.Name);
    }

    public async Task<TableConfigurationResult> GetTableConfigurationAsync(string workspaceIdentifier, string itemIdentifier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workspaceIdentifier))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        var (normalizedWorkspaceId, normalizedItemIdentifier, _, warehouseQueryValue) = await GetWarehousePrefixAsync(workspaceIdentifier, itemIdentifier, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeTableApiBaseUrl}/iceberg/v1/config?warehouse={warehouseQueryValue}";

        using var responseStream = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        using var reader = new StreamReader(responseStream);
        var rawResponse = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Received empty table configuration response.");
        }

        using var document = JsonDocument.Parse(rawResponse);
        var configuration = document.RootElement.Clone();

        return new TableConfigurationResult(normalizedWorkspaceId, normalizedItemIdentifier, configuration, rawResponse);
    }

    public async Task<TableNamespaceListResult> ListTableNamespacesAsync(string workspaceIdentifier, string itemIdentifier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workspaceIdentifier))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        var (normalizedWorkspaceId, normalizedItemIdentifier, warehousePrefix, _) = await GetWarehousePrefixAsync(workspaceIdentifier, itemIdentifier, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeTableApiBaseUrl}/iceberg/v1/{warehousePrefix}/namespaces";

        using var responseStream = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        using var reader = new StreamReader(responseStream);
        var rawResponse = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Received empty table namespace response.");
        }

        using var document = JsonDocument.Parse(rawResponse);
        var namespaces = document.RootElement.Clone();

        return new TableNamespaceListResult(normalizedWorkspaceId, normalizedItemIdentifier, namespaces, rawResponse);
    }

    public async Task<TableNamespaceGetResult> GetTableNamespaceAsync(string workspaceIdentifier, string itemIdentifier, string namespaceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workspaceIdentifier))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            throw new ArgumentException("Namespace name is required.", nameof(namespaceName));
        }

        var trimmedNamespace = namespaceName.Trim();
        if (string.IsNullOrEmpty(trimmedNamespace))
        {
            throw new ArgumentException("Namespace name cannot be empty.", nameof(namespaceName));
        }

        var (normalizedWorkspaceId, normalizedItemIdentifier, warehousePrefix, _) = await GetWarehousePrefixAsync(workspaceIdentifier, itemIdentifier, cancellationToken);
        var encodedNamespace = Uri.EscapeDataString(trimmedNamespace);
        var url = $"{OneLakeEndpoints.OneLakeTableApiBaseUrl}/iceberg/v1/{warehousePrefix}/namespaces/{encodedNamespace}";

        using var responseStream = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        using var reader = new StreamReader(responseStream);
        var rawResponse = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Received empty table namespace response.");
        }

        using var document = JsonDocument.Parse(rawResponse);
        var definition = document.RootElement.Clone();

        return new TableNamespaceGetResult(normalizedWorkspaceId, normalizedItemIdentifier, trimmedNamespace, definition, rawResponse);
    }

    public async Task<TableListResult> ListTablesAsync(string workspaceIdentifier, string itemIdentifier, string namespaceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workspaceIdentifier))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            throw new ArgumentException("Namespace name is required.", nameof(namespaceName));
        }

        var trimmedNamespace = namespaceName.Trim();
        if (string.IsNullOrEmpty(trimmedNamespace))
        {
            throw new ArgumentException("Namespace name cannot be empty.", nameof(namespaceName));
        }

        var (normalizedWorkspaceId, normalizedItemIdentifier, warehousePrefix, _) = await GetWarehousePrefixAsync(workspaceIdentifier, itemIdentifier, cancellationToken);
        var encodedNamespace = Uri.EscapeDataString(trimmedNamespace);
        var url = $"{OneLakeEndpoints.OneLakeTableApiBaseUrl}/iceberg/v1/{warehousePrefix}/namespaces/{encodedNamespace}/tables";

        using var responseStream = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        using var reader = new StreamReader(responseStream);
        var rawResponse = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Received empty table list response.");
        }

        using var document = JsonDocument.Parse(rawResponse);
        var tables = document.RootElement.Clone();

        return new TableListResult(normalizedWorkspaceId, normalizedItemIdentifier, trimmedNamespace, tables, rawResponse);
    }

    public async Task<TableGetResult> GetTableAsync(string workspaceIdentifier, string itemIdentifier, string namespaceName, string tableName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(workspaceIdentifier))
        {
            throw new ArgumentException("Workspace identifier is required.", nameof(workspaceIdentifier));
        }

        if (string.IsNullOrWhiteSpace(itemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            throw new ArgumentException("Namespace name is required.", nameof(namespaceName));
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        var trimmedNamespace = namespaceName.Trim();
        var trimmedTableName = tableName.Trim();

        if (string.IsNullOrEmpty(trimmedNamespace))
        {
            throw new ArgumentException("Namespace name cannot be empty.", nameof(namespaceName));
        }

        if (string.IsNullOrEmpty(trimmedTableName))
        {
            throw new ArgumentException("Table name cannot be empty.", nameof(tableName));
        }

        var (normalizedWorkspaceId, normalizedItemIdentifier, warehousePrefix, _) = await GetWarehousePrefixAsync(workspaceIdentifier, itemIdentifier, cancellationToken);
        var encodedNamespace = Uri.EscapeDataString(trimmedNamespace);
        var encodedTable = Uri.EscapeDataString(trimmedTableName);
        var url = $"{OneLakeEndpoints.OneLakeTableApiBaseUrl}/iceberg/v1/{warehousePrefix}/namespaces/{encodedNamespace}/tables/{encodedTable}";

        using var responseStream = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        using var reader = new StreamReader(responseStream);
        var rawResponse = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Received empty table response.");
        }

        using var document = JsonDocument.Parse(rawResponse);
        var tableDefinition = document.RootElement.Clone();

        return new TableGetResult(normalizedWorkspaceId, normalizedItemIdentifier, trimmedNamespace, trimmedTableName, tableDefinition, rawResponse);
    }

    private List<OneLakeFileInfo> ParseBlobListResponse(string xmlContent)
    {
        var files = new List<OneLakeFileInfo>();
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            // Parse blob elements (files and directories)
            var blobs = doc.Descendants(ns + "Blob");
            foreach (var blob in blobs)
            {
                var nameElement = blob.Element(ns + "Name");
                var propertiesElement = blob.Element(ns + "Properties");

                if (nameElement?.Value == null || propertiesElement == null)
                    continue;

                var fileName = nameElement.Value;
                var lastModified = propertiesElement.Element(ns + "Last-Modified")?.Value;
                var contentLength = propertiesElement.Element(ns + "Content-Length")?.Value;
                var contentType = propertiesElement.Element(ns + "Content-Type")?.Value;
                var resourceType = propertiesElement.Element(ns + "ResourceType")?.Value;

                var size = long.TryParse(contentLength, out var parsedSize) ? parsedSize : 0;

                // Use ResourceType to determine if this is a directory
                var isDirectory = string.Equals(resourceType, "directory", StringComparison.OrdinalIgnoreCase);

                files.Add(new OneLakeFileInfo
                {
                    Name = Path.GetFileName(fileName),
                    Path = fileName,
                    Size = size,
                    LastModified = DateTime.TryParse(lastModified, out var modifiedDate) ? modifiedDate : null,
                    ContentType = isDirectory ? "application/x-directory" : (contentType ?? "application/octet-stream"),
                    IsDirectory = isDirectory
                });
            }

            // Parse blob prefixes (directories) - Note: OneLake typically doesn't return these
            var blobPrefixes = doc.Descendants(ns + "BlobPrefix");
            foreach (var prefix in blobPrefixes)
            {
                var nameElement = prefix.Element(ns + "Name");
                if (nameElement?.Value != null)
                {
                    var dirName = nameElement.Value.TrimEnd('/');
                    files.Add(new OneLakeFileInfo
                    {
                        Name = Path.GetFileName(dirName),
                        Path = dirName,
                        Size = 0,
                        LastModified = null,
                        ContentType = "application/x-directory",
                        IsDirectory = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse OneLake file listing response: {ex.Message}", ex);
        }

        return files;
    }

    public async Task<List<FileSystemItem>> ListPathIntelligentAsync(string workspaceId, string itemId, bool recursive, CancellationToken cancellationToken)
    {
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // Intelligent discovery: Try to list contents from both Files and Tables folders using DFS API
        var allItems = new List<FileSystemItem>();
        var topLevelFolders = new[] { "Files", "Tables" };

        foreach (var folder in topLevelFolders)
        {
            try
            {
                var url = BuildAdlsListPathUrl(normalizedWorkspaceId, normalizedItemId, directory: folder, recursive: recursive);

                var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var items = ParsePathListResponse(content);
                allItems.AddRange(items);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                // Folder doesn't exist, skip it
                continue;
            }
            catch (Exception)
            {
                // Other errors, skip this folder but continue with others
                continue;
            }
        }

        return allItems.OrderBy(f => f.Type == "directory" ? 0 : 1).ThenBy(f => f.Name).ToList();
    }

    private List<FileSystemItem> ParsePathListResponse(string jsonContent)
    {
        var fileSystemItems = new List<FileSystemItem>();

        try
        {
            // Parse JSON response from ADLS Gen2 API
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (root.TryGetProperty("paths", out var pathsElement))
            {
                foreach (var pathItem in pathsElement.EnumerateArray())
                {
                    var name = pathItem.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : "";
                    var isDirectory = false;

                    if (pathItem.TryGetProperty("isDirectory", out var isDirElement))
                    {
                        // Handle both boolean and string representations
                        if (isDirElement.ValueKind == JsonValueKind.True)
                        {
                            isDirectory = true;
                        }
                        else if (isDirElement.ValueKind == JsonValueKind.False)
                        {
                            isDirectory = false;
                        }
                        else if (isDirElement.ValueKind == JsonValueKind.String)
                        {
                            isDirectory = bool.TryParse(isDirElement.GetString(), out var boolValue) && boolValue;
                        }
                    }

                    var contentLength = 0L;
                    if (pathItem.TryGetProperty("contentLength", out var lengthElement))
                    {
                        if (lengthElement.ValueKind == JsonValueKind.Number)
                        {
                            contentLength = lengthElement.GetInt64();
                        }
                        else if (lengthElement.ValueKind == JsonValueKind.String)
                        {
                            long.TryParse(lengthElement.GetString(), out contentLength);
                        }
                    }
                    var lastModified = pathItem.TryGetProperty("lastModified", out var modElement)
                        ? DateTime.TryParse(modElement.GetString(), out var modDate) ? modDate : (DateTime?)null
                        : null;
                    var etag = pathItem.TryGetProperty("etag", out var etagElement) ? etagElement.GetString() : null;
                    var permissions = pathItem.TryGetProperty("permissions", out var permsElement) ? permsElement.GetString() : null;
                    var owner = pathItem.TryGetProperty("owner", out var ownerElement) ? ownerElement.GetString() : null;
                    var group = pathItem.TryGetProperty("group", out var groupElement) ? groupElement.GetString() : null;

                    if (!string.IsNullOrEmpty(name))
                    {
                        var item = new FileSystemItem
                        {
                            Name = Path.GetFileName(name),
                            Path = name,
                            Type = isDirectory ? "directory" : "file",
                            Size = isDirectory ? null : contentLength,
                            LastModified = lastModified,
                            ContentType = isDirectory ? "application/x-directory" : "application/octet-stream",
                            ETag = etag,
                            Permissions = permissions,
                            Owner = owner,
                            Group = group,
                            Children = null
                        };

                        fileSystemItems.Add(item);
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse OneLake path listing response: {ex.Message}", ex);
        }

        return fileSystemItems;
    }

    public async Task<List<FileSystemItem>> ListPathAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (path is not null)
            ValidatePathForTraversal(path, nameof(path));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // If no path is specified, intelligently discover and search top-level folders
        if (string.IsNullOrEmpty(path))
        {
            return await ListPathIntelligentAsync(normalizedWorkspaceId, normalizedItemId, recursive, cancellationToken);
        }

        // Build ADLS Gen2 compliant URL with directory as a query parameter
        var directory = ResolveDirectoryPath(path);
        var url = BuildAdlsListPathUrl(normalizedWorkspaceId, normalizedItemId, directory: directory, recursive: recursive);

        var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var fileSystemItems = ParsePathListResponse(content);

        return fileSystemItems.OrderBy(f => f.Type == "directory" ? 0 : 1).ThenBy(f => f.Name).ToList();
    }

    private List<FileSystemItem> BuildHierarchicalStructure(List<FileSystemItem> flatItems, string basePath)
    {
        var root = new List<FileSystemItem>();
        var pathPrefix = basePath.TrimEnd('/') + "/";

        // Group items by their immediate parent directory
        var grouped = flatItems
            .Where(item => item.Path.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase) || item.Path == basePath.TrimEnd('/'))
            .GroupBy(item =>
            {
                var relativePath = item.Path.Substring(pathPrefix.Length);
                var firstSlash = relativePath.IndexOf('/');
                return firstSlash == -1 ? "" : relativePath.Substring(0, firstSlash);
            });

        foreach (var group in grouped)
        {
            if (string.IsNullOrEmpty(group.Key))
            {
                // Direct children of the base path
                root.AddRange(group);
            }
            else
            {
                // Create directory entry with children
                var dirPath = $"{pathPrefix}{group.Key}";
                var directoryItem = group.FirstOrDefault(item => item.Path == dirPath && item.Type == "directory");

                if (directoryItem == null)
                {
                    directoryItem = new FileSystemItem
                    {
                        Name = group.Key,
                        Path = dirPath,
                        Type = "directory",
                        Size = null,
                        LastModified = null,
                        ContentType = "application/x-directory"
                    };
                }

                directoryItem.Children = group.Where(item => item.Path != dirPath).ToList();
                root.Add(directoryItem);
            }
        }

        return root.OrderBy(f => f.Type == "directory" ? 0 : 1).ThenBy(f => f.Name).ToList();
    }

    public async Task<IEnumerable<OneLakeItem>> ListOneLakeItemsAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var xmlContent = await ExecuteWithWorkspaceFallbackAsync(
            workspaceId,
            async identifier =>
            {
                var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{identifier}?delimiter=/&restype=container&comp=list";
                if (!string.IsNullOrEmpty(continuationToken))
                {
                    url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
                }

                var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);

                using var reader = new StreamReader(response);
                return await reader.ReadToEndAsync(cancellationToken);
            },
            cancellationToken);

        try
        {
            var doc = XDocument.Parse(xmlContent);

            // For container listing with comp=list, Azure Storage returns different XML structure
            // Try various possible XML structures based on Azure Storage Blob Service API
            var items = new List<OneLakeItem>();

            // Option 1: Try <EnumerationResults><Blobs><BlobPrefix> (OneLake uses BlobPrefix)
            var blobPrefixes = doc.Root?.Element("Blobs")?.Elements("BlobPrefix");
            if (blobPrefixes != null && blobPrefixes.Any())
            {
                items.AddRange(ParseBlobPrefixElements(blobPrefixes, workspaceId));
            }

            // Option 2: Try <EnumerationResults><Blobs><Blob> (fallback for regular blobs)
            if (!items.Any())
            {
                var blobs = doc.Root?.Element("Blobs")?.Elements("Blob");
                if (blobs != null && blobs.Any())
                {
                    items.AddRange(ParseBlobElements(blobs, workspaceId));
                }
            }

            // Option 2: Try <EnumerationResults><Containers><Container> (like workspace listing)
            if (!items.Any())
            {
                var containers = doc.Root?.Element("Containers")?.Elements("Container");
                if (containers != null && containers.Any())
                {
                    items.AddRange(ParseContainerElements(containers, workspaceId));
                }
            }

            // Option 3: Try direct children of root element
            if (!items.Any())
            {
                var directElements = doc.Root?.Elements().Where(e => e.Name != "NextMarker" && e.Name != "MaxResults");
                if (directElements != null && directElements.Any())
                {
                    items.AddRange(ParseGenericElements(directElements, workspaceId));
                }
            }

            return items;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse OneLake items list response: {ex.Message}", ex);
        }
    }

    public async Task<string> ListBlobsRawAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (path is not null)
            ValidatePathForTraversal(path, nameof(path));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // If no path is specified, intelligently discover and search top-level folders
        if (string.IsNullOrEmpty(path))
        {
            // For intelligent discovery, combine responses from multiple folders
            var allResponses = new List<string>();
            var topLevelFolders = new[] { "Files", "Tables" };

            foreach (var folder in topLevelFolders)
            {
                try
                {
                    var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{folder}";
                    url += "?restype=container&comp=list";
                    if (recursive)
                    {
                        url += "&recursive=true";
                    }

                    var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    allResponses.Add($"<!-- Response for folder: {folder} -->\n{content}");
                }
                catch (Exception ex)
                {
                    allResponses.Add($"<!-- Error accessing folder {folder}: {ex.Message} -->");
                }
            }

            return string.Join("\n\n", allResponses);
        }

        // Blob API: path is part of the URL path per Blob Storage List Blobs spec
        var directory = ResolveDirectoryPath(path);
        var singleUrl = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{directory}";
        singleUrl += "?restype=container&comp=list";
        if (recursive)
        {
            singleUrl += "&recursive=true";
        }

        var singleResponse = await SendDataPlaneRequestAsync(HttpMethod.Get, singleUrl, cancellationToken: cancellationToken);
        return await singleResponse.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> ListPathRawAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default)
    {
        if (path is not null)
            ValidatePathForTraversal(path, nameof(path));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // If no path is specified, intelligently discover and search top-level folders
        if (string.IsNullOrEmpty(path))
        {
            // For intelligent discovery, combine responses from multiple folders
            var allResponses = new List<string>();
            var topLevelFolders = new[] { "Files", "Tables" };

            foreach (var folder in topLevelFolders)
            {
                try
                {
                    var url = BuildAdlsListPathUrl(normalizedWorkspaceId, normalizedItemId, directory: folder, recursive: recursive);

                    var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    allResponses.Add($"/* Response for folder: {folder} */\n{content}");
                }
                catch (Exception ex)
                {
                    allResponses.Add($"/* Error accessing folder {folder}: {ex.Message} */");
                }
            }

            return string.Join("\n\n", allResponses);
        }

        // Build ADLS Gen2 compliant URL with directory as a query parameter
        var directory = ResolveDirectoryPath(path);
        var singleUrl = BuildAdlsListPathUrl(normalizedWorkspaceId, normalizedItemId, directory: directory, recursive: recursive);

        var singleResponse = await SendDataPlaneRequestAsync(HttpMethod.Get, singleUrl, cancellationToken: cancellationToken);
        return await singleResponse.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> ListOneLakeItemsXmlAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithWorkspaceFallbackAsync(
            workspaceId,
            async identifier =>
            {
                var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{identifier}?delimiter=/&restype=container&comp=list";
                if (!string.IsNullOrEmpty(continuationToken))
                {
                    url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
                }

                var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);

                using var reader = new StreamReader(response);
                return await reader.ReadToEndAsync(cancellationToken);
            },
            cancellationToken);
    }

    public async Task<string> ListOneLakeItemsDfsJsonAsync(string workspaceId, bool recursive = true, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithWorkspaceFallbackAsync(
            workspaceId,
            async identifier =>
            {
                var url = $"{OneLakeEndpoints.OneLakeDataPlaneDfsBaseUrl}/{identifier}?resource=filesystem&recursive={recursive.ToString().ToLower()}";
                if (!string.IsNullOrEmpty(continuationToken))
                {
                    url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
                }

                var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);

                using var reader = new StreamReader(response);
                return await reader.ReadToEndAsync(cancellationToken);
            },
            cancellationToken);
    }

    private async Task<T> ExecuteWithWorkspaceFallbackAsync<T>(
        string workspaceId,
        Func<string, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(workspaceId, out _))
        {
            return await operation(workspaceId);
        }

        try
        {
            return await operation(workspaceId);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
        {
            var workspace = await GetWorkspaceAsync(workspaceId, cancellationToken);
            var workspaceName = workspace.DisplayName;

            if (string.IsNullOrWhiteSpace(workspaceName) || string.Equals(workspaceName, workspaceId, StringComparison.OrdinalIgnoreCase))
            {
                throw;
            }

            return await operation(workspaceName);
        }
    }

    private static List<OneLakeItem> ParseBlobPrefixElements(IEnumerable<XElement> blobPrefixes, string workspaceId)
    {
        return blobPrefixes.Select(blobPrefix =>
        {
            var nameElement = blobPrefix.Element("Name");
            var name = nameElement?.Value ?? "";

            // Remove trailing slash and extract type from name
            var cleanName = name.TrimEnd('/');
            var (itemName, itemType) = ExtractItemNameAndType(cleanName);

            var item = new OneLakeItem
            {
                Id = cleanName,
                DisplayName = itemName,
                WorkspaceId = workspaceId,
                Type = itemType
            };

            // Parse Properties
            var propertiesElement = blobPrefix.Element("Properties");
            if (propertiesElement != null)
            {
                var creationTimeString = propertiesElement.Element("Creation-Time")?.Value;
                if (!string.IsNullOrEmpty(creationTimeString) && DateTime.TryParse(creationTimeString, out var creationTime))
                {
                    item.CreatedDate = creationTime;
                }

                var lastModifiedString = propertiesElement.Element("Last-Modified")?.Value;
                if (!string.IsNullOrEmpty(lastModifiedString) && DateTime.TryParse(lastModifiedString, out var lastModified))
                {
                    item.LastModifiedDate = lastModified;
                }
            }

            // Parse Metadata
            var metadataElement = blobPrefix.Element("Metadata");
            if (metadataElement != null)
            {
                item.Metadata = new OneLakeItemMetadata
                {
                    ArtifactId = metadataElement.Element("ArtifactId")?.Value,
                    ArtifactPortalUrl = metadataElement.Element("ArtifactPortalUrl")?.Value
                };
            }

            // Also capture ResourceType and BlobType from Properties if available
            if (propertiesElement != null && item.Metadata != null)
            {
                item.Metadata.ResourceType = propertiesElement.Element("ResourceType")?.Value;
                item.Metadata.BlobType = propertiesElement.Element("BlobType")?.Value;
            }
            else if (propertiesElement != null)
            {
                item.Metadata = new OneLakeItemMetadata
                {
                    ResourceType = propertiesElement.Element("ResourceType")?.Value,
                    BlobType = propertiesElement.Element("BlobType")?.Value
                };
            }

            return item;
        }).ToList();
    }

    private static (string name, string type) ExtractItemNameAndType(string fullName)
    {
        // Handle Fabric item names like "sales.Lakehouse", "Notebook 1.SynapseNotebook"
        var lastDot = fullName.LastIndexOf('.');
        if (lastDot > 0 && lastDot < fullName.Length - 1)
        {
            var name = fullName.Substring(0, lastDot);
            var type = fullName.Substring(lastDot + 1);

            // Map Fabric types to display names
            return (name, MapFabricTypeToDisplayName(type));
        }

        return (fullName, "Item");
    }

    private static string MapFabricTypeToDisplayName(string fabricType)
    {
        return fabricType switch
        {
            "Lakehouse" => "Lakehouse",
            "SynapseNotebook" => "Notebook",
            "Report" => "Report",
            "Dataset" => "Dataset",
            "Dataflow" => "Dataflow",
            "DataPipeline" => "DataPipeline",
            "Warehouse" => "Warehouse",
            "KQLQueryset" => "KQLQueryset",
            "SQLEndpoint" => "SQLEndpoint",
            _ => fabricType
        };
    }

    private static List<OneLakeItem> ParseBlobElements(IEnumerable<XElement> blobs, string workspaceId)
    {
        return blobs.Select(blob =>
        {
            var item = new OneLakeItem
            {
                Id = blob.Element("Name")?.Value ?? "",
                DisplayName = blob.Element("Name")?.Value ?? "",
                WorkspaceId = workspaceId
            };

            // Parse Properties
            var propertiesElement = blob.Element("Properties");
            if (propertiesElement != null)
            {
                var creationTimeString = propertiesElement.Element("Creation-Time")?.Value;
                if (!string.IsNullOrEmpty(creationTimeString) && DateTime.TryParse(creationTimeString, out var creationTime))
                {
                    item.CreatedDate = creationTime;
                }

                var lastModifiedString = propertiesElement.Element("Last-Modified")?.Value;
                if (!string.IsNullOrEmpty(lastModifiedString) && DateTime.TryParse(lastModifiedString, out var lastModified))
                {
                    item.LastModifiedDate = lastModified;
                }

                var contentType = propertiesElement.Element("Content-Type")?.Value;
                if (!string.IsNullOrEmpty(contentType))
                {
                    item.Type = InferItemTypeFromContentType(contentType);
                }
            }

            return item;
        }).ToList();
    }

    private static List<OneLakeItem> ParseContainerElements(IEnumerable<XElement> containers, string workspaceId)
    {
        return containers.Select(container =>
        {
            var item = new OneLakeItem
            {
                Id = container.Element("Name")?.Value ?? "",
                DisplayName = container.Element("Name")?.Value ?? "",
                WorkspaceId = workspaceId,
                Type = "Container"
            };

            // Parse Properties  
            var propertiesElement = container.Element("Properties");
            if (propertiesElement != null)
            {
                var lastModifiedString = propertiesElement.Element("Last-Modified")?.Value;
                if (!string.IsNullOrEmpty(lastModifiedString) && DateTime.TryParse(lastModifiedString, out var lastModified))
                {
                    item.LastModifiedDate = lastModified;
                }
            }

            return item;
        }).ToList();
    }

    private static List<OneLakeItem> ParseGenericElements(IEnumerable<XElement> elements, string workspaceId)
    {
        return elements.Select(element =>
        {
            var item = new OneLakeItem
            {
                Id = element.Value ?? element.Name.LocalName,
                DisplayName = element.Value ?? element.Name.LocalName,
                WorkspaceId = workspaceId,
                Type = element.Name.LocalName
            };

            return item;
        }).ToList();
    }

    public Task<BlobGetResult> ReadFileAsync(string workspaceId, string itemId, string filePath, BlobDownloadOptions? downloadOptions = null, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(filePath, nameof(filePath));
        return DownloadBlobAsync(
            OneLakeEndpoints.OneLakeDataPlaneDfsBaseUrl,
            workspaceId,
            itemId,
            filePath,
            downloadOptions,
            queryString: null,
            cancellationToken);
    }

    public async Task WriteFileAsync(string workspaceId, string itemId, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(filePath, nameof(filePath));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        // Use DFS endpoint for file operations (similar to directory creation)
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneDfsBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{filePath.TrimStart('/')}";

        // Create or overwrite file using DFS API
        using var createRequest = new HttpRequestMessage(HttpMethod.Put, url + "?resource=file");
        createRequest.Headers.Add("x-ms-resource", "file");
        createRequest.Content = new ByteArrayContent(Array.Empty<byte>());
        createRequest.Content.Headers.ContentLength = 0;
        if (!overwrite)
        {
            createRequest.Headers.Add("If-None-Match", "*");
        }

        await SendDataPlaneRequestAsync(createRequest, cancellationToken: cancellationToken);

        // Upload content
        url += "?action=append&position=0";
        using var uploadRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = new StreamContent(content)
        };
        uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        await SendDataPlaneRequestAsync(uploadRequest, cancellationToken: cancellationToken);

        // Flush/commit
        url = url.Replace("action=append&position=0", $"action=flush&position={content.Length}");
        using var flushRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url);
        await SendDataPlaneRequestAsync(flushRequest, cancellationToken: cancellationToken);
    }

    public async Task<BlobPutResult> PutBlobAsync(string workspaceId, string itemId, string blobPath, Stream content, long contentLength, string? contentType = null, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ValidatePathForTraversal(blobPath, nameof(blobPath));

        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBlobBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{blobPath.TrimStart('/')}";

        if (content.CanSeek)
        {
            content.Seek(0, SeekOrigin.Begin);
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StreamContent(content)
        };

        request.Headers.Add("x-ms-blob-type", "BlockBlob");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        request.Content.Headers.ContentLength = contentLength;

        if (!overwrite)
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", "*");
        }

        using var response = await SendDataPlaneRequestAsync(request, cancellationToken: cancellationToken);

        var eTag = response.Headers.ETag?.Tag;
        var lastModified = response.Content?.Headers?.LastModified;
        if (lastModified is null && response.Headers.TryGetValues("Last-Modified", out var lastModifiedValues))
        {
            if (DateTimeOffset.TryParse(lastModifiedValues.FirstOrDefault(), out var parsed))
            {
                lastModified = parsed;
            }
        }
        string? requestId = GetHeaderValue(response.Headers, "x-ms-request-id");
        string? version = GetHeaderValue(response.Headers, "x-ms-version");
        string? versionId = GetHeaderValue(response.Headers, "x-ms-version-id");
        string? contentCrc64 = GetHeaderValue(response.Headers, "x-ms-content-crc64");
        string? encryptionScope = GetHeaderValue(response.Headers, "x-ms-encryption-scope");
        string? encryptionKeySha256 = GetHeaderValue(response.Headers, "x-ms-encryption-key-sha256");
        string? clientRequestId = GetHeaderValue(response.Headers, "x-ms-client-request-id");
        string? rootActivityId = GetHeaderValue(response.Headers, "x-ms-root-activity-id");

        bool? requestServerEncrypted = null;
        var requestServerEncryptedValue = GetHeaderValue(response.Headers, "x-ms-request-server-encrypted");
        if (!string.IsNullOrWhiteSpace(requestServerEncryptedValue) && bool.TryParse(requestServerEncryptedValue, out var encrypted))
        {
            requestServerEncrypted = encrypted;
        }

        string? contentMd5 = null;
        if (response.Content is { } httpContent)
        {
            if (httpContent.Headers.ContentMD5 is { Length: > 0 } contentMd5Bytes)
            {
                contentMd5 = Convert.ToBase64String(contentMd5Bytes);
            }
            else if (httpContent.Headers.TryGetValues("Content-MD5", out var contentMd5Values))
            {
                contentMd5 = contentMd5Values.FirstOrDefault();
            }
        }

        return new BlobPutResult(
            normalizedWorkspaceId,
            normalizedItemId,
            blobPath,
            contentLength,
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType!,
            eTag,
            lastModified,
            requestId,
            version,
            requestServerEncrypted,
            contentMd5,
            contentCrc64,
            encryptionScope,
            encryptionKeySha256,
            versionId,
            clientRequestId,
            rootActivityId);
    }

    public Task<BlobGetResult> GetBlobAsync(string workspaceId, string itemId, string blobPath, BlobDownloadOptions? downloadOptions = null, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(blobPath, nameof(blobPath));
        return DownloadBlobAsync(
            OneLakeEndpoints.OneLakeDataPlaneBlobBaseUrl,
            workspaceId,
            itemId,
            blobPath,
            downloadOptions,
            queryString: null,
            cancellationToken);
    }

    private async Task<BlobGetResult> DownloadBlobAsync(
        string baseUrl,
        string workspaceId,
        string itemId,
        string path,
        BlobDownloadOptions? downloadOptions,
        string? queryString,
        CancellationToken cancellationToken)
    {
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var effectiveOptions = downloadOptions ?? new BlobDownloadOptions();

        var trimmedPath = path.TrimStart('/');
        var urlBuilder = new StringBuilder($"{baseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{trimmedPath}");

        if (!string.IsNullOrWhiteSpace(queryString))
        {
            if (queryString.StartsWith("?", StringComparison.Ordinal))
            {
                urlBuilder.Append(queryString);
            }
            else
            {
                urlBuilder.Append('?');
                urlBuilder.Append(queryString);
            }
        }

        var url = urlBuilder.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await SendDataPlaneRequestAsync(request, cancellationToken: cancellationToken);

        var contentHeaders = response.Content?.Headers;
        var contentTypeHeader = contentHeaders?.ContentType;
        var contentType = contentTypeHeader?.ToString();
        var charset = contentTypeHeader?.CharSet;
        var contentEncoding = contentHeaders?.ContentEncoding?.FirstOrDefault();
        var contentLanguage = contentHeaders?.ContentLanguage?.FirstOrDefault();
        var contentDisposition = contentHeaders?.ContentDisposition?.ToString();
        var contentLength = contentHeaders?.ContentLength;

        string? contentMd5 = null;
        if (contentHeaders?.ContentMD5 is { Length: > 0 } md5Bytes)
        {
            contentMd5 = Convert.ToBase64String(md5Bytes);
        }
        else if (contentHeaders?.TryGetValues("Content-MD5", out var contentMd5Values) == true)
        {
            contentMd5 = contentMd5Values.FirstOrDefault();
        }

        var contentCrc64 = GetHeaderValue(response.Headers, "x-ms-content-crc64");

        var inlineRequested = effectiveOptions.IncludeInlineContent;
        var inlineLimit = effectiveOptions.InlineContentLimit;
        var destinationStream = effectiveOptions.DestinationStream;
        var contentFilePath = effectiveOptions.LocalFilePath;

        var shouldReadInline = inlineRequested;
        var inlineContentTruncated = false;
        if (inlineRequested && inlineLimit.HasValue && contentLength.HasValue && contentLength.Value > inlineLimit.Value)
        {
            shouldReadInline = false;
            inlineContentTruncated = true;
        }

        string? contentBase64 = null;
        string? contentText = null;

        if (response.Content != null && (shouldReadInline || destinationStream is not null))
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            if (shouldReadInline && destinationStream is not null)
            {
                using var bufferedStream = new MemoryStream();
                await responseStream.CopyToAsync(bufferedStream, cancellationToken);
                bufferedStream.Seek(0, SeekOrigin.Begin);
                await bufferedStream.CopyToAsync(destinationStream, cancellationToken);
                if (string.IsNullOrWhiteSpace(contentFilePath) && destinationStream is FileStream fileDownloadStream)
                {
                    contentFilePath = fileDownloadStream.Name;
                }
                var bytes = bufferedStream.ToArray();
                contentBase64 = Convert.ToBase64String(bytes);

                var hasBinaryEncoding = !string.IsNullOrWhiteSpace(contentEncoding) && !string.Equals(contentEncoding, "identity", StringComparison.OrdinalIgnoreCase);
                if (!hasBinaryEncoding && IsTextContent(contentType))
                {
                    var encoding = GetTextEncoding(charset);
                    contentText = encoding.GetString(bytes);
                }
            }
            else if (shouldReadInline)
            {
                using var bufferedStream = new MemoryStream();
                await responseStream.CopyToAsync(bufferedStream, cancellationToken);
                var bytes = bufferedStream.ToArray();
                contentBase64 = Convert.ToBase64String(bytes);

                var hasBinaryEncoding = !string.IsNullOrWhiteSpace(contentEncoding) && !string.Equals(contentEncoding, "identity", StringComparison.OrdinalIgnoreCase);
                if (!hasBinaryEncoding && IsTextContent(contentType))
                {
                    var encoding = GetTextEncoding(charset);
                    contentText = encoding.GetString(bytes);
                }
            }
            else if (destinationStream is not null)
            {
                await responseStream.CopyToAsync(destinationStream, cancellationToken);
                if (string.IsNullOrWhiteSpace(contentFilePath) && destinationStream is FileStream fileDownloadStream)
                {
                    contentFilePath = fileDownloadStream.Name;
                }
            }
        }

        var eTag = response.Headers.ETag?.Tag;
        var lastModified = contentHeaders?.LastModified;
        if (lastModified is null && response.Headers.TryGetValues("Last-Modified", out var lastModifiedValues))
        {
            if (DateTimeOffset.TryParse(lastModifiedValues.FirstOrDefault(), out var parsed))
            {
                lastModified = parsed;
            }
        }

        bool? requestServerEncrypted = null;
        var requestServerEncryptedValue = GetHeaderValue(response.Headers, "x-ms-request-server-encrypted");
        if (!string.IsNullOrWhiteSpace(requestServerEncryptedValue) && bool.TryParse(requestServerEncryptedValue, out var encrypted))
        {
            requestServerEncrypted = encrypted;
        }

        var encryptionScope = GetHeaderValue(response.Headers, "x-ms-encryption-scope");
        var encryptionKeySha256 = GetHeaderValue(response.Headers, "x-ms-encryption-key-sha256");
        var version = GetHeaderValue(response.Headers, "x-ms-version");
        var versionId = GetHeaderValue(response.Headers, "x-ms-version-id");
        var requestId = GetHeaderValue(response.Headers, "x-ms-request-id");
        var clientRequestId = GetHeaderValue(response.Headers, "x-ms-client-request-id");
        var rootActivityId = GetHeaderValue(response.Headers, "x-ms-root-activity-id");

        return new BlobGetResult(
            normalizedWorkspaceId,
            normalizedItemId,
            path,
            contentLength,
            string.IsNullOrWhiteSpace(contentType) ? contentTypeHeader?.MediaType : contentType,
            charset,
            contentEncoding,
            contentLanguage,
            contentDisposition,
            contentMd5,
            contentCrc64,
            contentBase64,
            contentText,
            eTag,
            lastModified,
            requestServerEncrypted,
            encryptionScope,
            encryptionKeySha256,
            version,
            versionId,
            requestId,
            clientRequestId,
            rootActivityId)
        {
            ContentFilePath = contentFilePath,
            InlineContentTruncated = inlineContentTruncated
        };
    }

    public async Task<BlobDeleteResult> DeleteBlobAsync(string workspaceId, string itemId, string blobPath, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(blobPath, nameof(blobPath));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBlobBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{blobPath.TrimStart('/')}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        using var response = await SendDataPlaneRequestAsync(request, cancellationToken: cancellationToken);

        var version = GetHeaderValue(response.Headers, "x-ms-version");
        var versionId = GetHeaderValue(response.Headers, "x-ms-version-id");
        var requestId = GetHeaderValue(response.Headers, "x-ms-request-id");
        var clientRequestId = GetHeaderValue(response.Headers, "x-ms-client-request-id");
        var rootActivityId = GetHeaderValue(response.Headers, "x-ms-root-activity-id");

        return new BlobDeleteResult(
            normalizedWorkspaceId,
            normalizedItemId,
            blobPath,
            version,
            versionId,
            requestId,
            clientRequestId,
            rootActivityId);
    }

    public async Task DeleteFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(filePath, nameof(filePath));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{filePath.TrimStart('/')}";
        await SendDataPlaneRequestAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);
    }

    public async Task DeleteDirectoryAsync(string workspaceId, string itemId, string directoryPath, bool recursive = false, CancellationToken cancellationToken = default)
    {
        ValidatePathForTraversal(directoryPath, nameof(directoryPath));
        // Use blob endpoint for directory deletion, similar to file deletion
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{directoryPath.TrimStart('/')}";

        if (recursive)
        {
            url += "?recursive=true";
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        // Add the required header to indicate we're deleting a directory
        request.Headers.Add("x-ms-delete-type", "directory");

        await SendDataPlaneRequestAsync(request, cancellationToken: cancellationToken);
    }

    public async Task CreateDirectoryAsync(string workspaceId, string itemId, string directoryPath, CancellationToken cancellationToken = default)
    {
        // In OneLake/Azure Data Lake Storage, directories are created implicitly when files are created
        // However, we can create an empty directory using a PUT request with the appropriate headers
        ValidatePathForTraversal(directoryPath, nameof(directoryPath));
        var (normalizedWorkspaceId, normalizedItemId) = await GetNormalizedIdentifiersAsync(workspaceId, itemId, cancellationToken);
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneDfsBaseUrl}/{normalizedWorkspaceId}/{normalizedItemId}/{directoryPath.TrimStart('/')}?resource=directory";

        using var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Add("x-ms-resource", "directory");

        await SendDataPlaneRequestAsync(request, cancellationToken: cancellationToken);
    }

    // Private helper methods

    /// <summary>
    /// Builds an ADLS Gen2 Path List API-compliant URL.
    /// Per https://learn.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/path/list
    /// the directory is a query parameter, not part of the URL path.
    /// URL format: GET {dfsBase}/{filesystem}?resource=filesystem&amp;directory={dir}&amp;recursive={recursive}
    /// In OneLake, filesystem = workspaceId; directory = itemId/path.
    /// </summary>
    private static string BuildAdlsListPathUrl(
        string workspaceId,
        string itemId,
        string? directory,
        bool recursive,
        int? maxResults = null,
        string? continuation = null,
        bool? upn = null)
    {
        // filesystem = workspaceId
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneDfsBaseUrl}/{workspaceId}?resource=filesystem";

        // Build the directory path: itemId + optional subdirectory
        var directoryPath = itemId;
        if (!string.IsNullOrEmpty(directory))
        {
            directoryPath = $"{itemId}/{directory.TrimStart('/')}";
        }

        url += $"&directory={Uri.EscapeDataString(directoryPath)}";
        url += $"&recursive={recursive.ToString().ToLowerInvariant()}";

        if (maxResults.HasValue)
        {
            url += $"&maxResults={maxResults.Value}";
        }

        if (!string.IsNullOrEmpty(continuation))
        {
            url += $"&continuation={Uri.EscapeDataString(continuation)}";
        }

        if (upn.HasValue)
        {
            url += $"&upn={upn.Value.ToString().ToLowerInvariant()}";
        }

        return url;
    }

    /// <summary>
    /// Resolves a user-provided path to the ADLS directory value relative to the item root.
    /// Handles top-level folders (Files, Tables) and defaults to Files/ prefix for backward compatibility.
    /// </summary>
    private static string ResolveDirectoryPath(string path)
    {
        var trimmedPath = path.TrimStart('/');

        if (trimmedPath.StartsWith("Files/", StringComparison.OrdinalIgnoreCase) ||
            trimmedPath.Equals("Files", StringComparison.OrdinalIgnoreCase) ||
            IsTopLevelFolder(trimmedPath))
        {
            return trimmedPath;
        }

        // Assume it's a sub-path within Files for backward compatibility
        return $"Files/{trimmedPath}";
    }

    private static void ValidatePathForTraversal(string path, string paramName)
    {
        // Decode percent-encoding so that %2e%2e or %2E%2E variants are caught
        // before checking for traversal segments.
        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(path);
        }
        catch (UriFormatException)
        {
            // Malformed percent-encoding — treat the raw string as-is.
            decoded = path;
        }

        // Normalise both forward- and back-slash separators and reject any
        // segment that resolves to ".", "..", or "~" (home-directory shorthand).
        var segments = decoded.Split('/', '\\');
        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed is "." or ".." or "~")
            {
                throw new ArgumentException("Path cannot contain directory traversal sequences.", paramName);
            }
        }
    }

    private async Task<Stream> SendFabricApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        ApplyUserAgent(request);

        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        // Handle long running operations (202 Accepted)
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            var locationUrl = response.Headers.Location?.ToString();
            if (!string.IsNullOrEmpty(locationUrl))
            {
                return await PollFabricLroAsync(locationUrl, cancellationToken) ?? Stream.Null;
            }
            return Stream.Null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Error details: {errorBody}",
                null,
                response.StatusCode);
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    /// <summary>
    /// Polls a Fabric long running operation URL until it succeeds or fails,
    /// then fetches and returns the result stream.
    /// </summary>
    private async Task<Stream?> PollFabricLroAsync(string operationUrl, CancellationToken cancellationToken)
    {
        const int MaxAttempts = 120; // ~10 minutes at 5s default
        var retryDelay = TimeSpan.FromSeconds(5);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            await Task.Delay(retryDelay, cancellationToken);

            var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
            var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

            using var pollRequest = new HttpRequestMessage(HttpMethod.Get, operationUrl);
            pollRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            ApplyUserAgent(pollRequest);

            var pollResponse = await _httpClient.SendAsync(pollRequest, cancellationToken);
            if (!pollResponse.IsSuccessStatusCode)
            {
                var errorBody = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"LRO polling failed: {(int)pollResponse.StatusCode} ({pollResponse.ReasonPhrase}). {errorBody}",
                    null,
                    pollResponse.StatusCode);
            }

            // Update retry delay from Retry-After header
            if (pollResponse.Headers.TryGetValues("Retry-After", out var retryAfterValues)
                && int.TryParse(retryAfterValues.FirstOrDefault(), out var retryAfterSecs))
            {
                retryDelay = TimeSpan.FromSeconds(Math.Max(1, retryAfterSecs));
            }

            var stateStream = await pollResponse.Content.ReadAsStreamAsync(cancellationToken);
            var state = await JsonSerializer.DeserializeAsync(stateStream, OneLakeJsonContext.Default.OperationState, cancellationToken);

            switch (state?.Status)
            {
                case "Succeeded":
                    // Location header on the completed poll response points to the result URL
                    var resultUrl = pollResponse.Headers.Location?.ToString();
                    if (!string.IsNullOrEmpty(resultUrl))
                    {
                        return await GetFabricLroResultAsync(resultUrl, cancellationToken);
                    }
                    return Stream.Null;

                case "Failed":
                    var errorMessage = state.Error?.Message ?? "Long running operation failed.";
                    var errorCode = state.Error?.ErrorCode ?? "Unknown";
                    throw new HttpRequestException(
                        $"Long running operation failed ({errorCode}): {errorMessage}",
                        null,
                        System.Net.HttpStatusCode.InternalServerError);
            }
            // NotStarted / Running / Undefined: keep polling
        }

        throw new HttpRequestException(
            "Long running operation timed out after maximum polling attempts.",
            null,
            System.Net.HttpStatusCode.RequestTimeout);
    }

    /// <summary>
    /// GETs the result URL of a completed Fabric long running operation.
    /// </summary>
    private async Task<Stream?> GetFabricLroResultAsync(string resultUrl, CancellationToken cancellationToken)
    {
        var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, resultUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        ApplyUserAgent(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Stream.Null;
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<Stream> SendOneLakeApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, CancellationToken cancellationToken = default)
    {
        var tokenContext = new Azure.Core.TokenRequestContext(new[] { OneLakeEndpoints.StorageScope });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Add("x-ms-version", "2023-11-03");
        ApplyUserAgent(request);

        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendDataPlaneRequestAsync(HttpMethod method, string url, string? tenant = null, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, url);
        return await SendDataPlaneRequestAsync(request, tenant, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendDataPlaneRequestAsync(HttpRequestMessage request, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.StorageScope });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Add("x-ms-version", "2023-11-03");
        request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R"));
        ApplyUserAgent(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }

    private static void ApplyUserAgent(HttpRequestMessage request)
    {
        if (request.Headers.Contains(UserAgentHeaderName))
        {
            request.Headers.Remove(UserAgentHeaderName);
        }

        request.Headers.TryAddWithoutValidation(UserAgentHeaderName, UserAgentHeaderValue);
    }

    private static string? GetHeaderValue(HttpHeaders? headers, string name)
    {
        if (headers is null)
        {
            return null;
        }

        return headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }

    private static bool IsTextContent(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("yaml", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("csv", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("html", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("javascript", StringComparison.OrdinalIgnoreCase);
    }

    private static Encoding GetTextEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset))
        {
            return Encoding.UTF8;
        }

        try
        {
            return Encoding.GetEncoding(charset);
        }
        catch (Exception)
        {
            return Encoding.UTF8;
        }
    }

    private static long? GetContentLength(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("Content-Length", out var values))
        {
            return long.TryParse(values.FirstOrDefault(), out var length) ? length : null;
        }
        return null;
    }

    private static DateTime? GetLastModified(HttpResponseHeaders headers)
    {
        if (headers.TryGetValues("Last-Modified", out var values))
        {
            return DateTime.TryParse(values.FirstOrDefault(), out var lastModified) ? lastModified : null;
        }
        return null;
    }

    private static string InferItemTypeFromContentType(string contentType)
    {
        return contentType switch
        {
            "application/vnd.ms-fabric.lakehouse" => "Lakehouse",
            "application/vnd.ms-fabric.notebook" => "Notebook",
            "application/vnd.ms-fabric.report" => "Report",
            "application/vnd.ms-fabric.dataset" => "Dataset",
            "application/vnd.ms-fabric.dataflow" => "Dataflow",
            "application/vnd.ms-fabric.pipeline" => "DataPipeline",
            "application/vnd.ms-fabric.warehouse" => "Warehouse",
            "application/vnd.ms-fabric.kqlqueryset" => "KQLQueryset",
            "application/vnd.ms-fabric.sqldatabase" => "SQLEndpoint",
            _ => "Item"
        };
    }

    public void Dispose()
    {
        // DefaultAzureCredential doesn't need disposal
    }

    // Data Access Security Operations
    public async Task<DataAccessRoleListResponse> ListDataAccessRolesAsync(string workspaceId, string itemId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/dataAccessRoles";
        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            url += $"?continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, OneLakeJsonContext.Default.DataAccessRoleListResponse, cancellationToken) ?? new DataAccessRoleListResponse();
    }

    public async Task<DataAccessRole> GetDataAccessRoleAsync(string workspaceId, string itemId, string roleName, CancellationToken cancellationToken = default)
    {
        var encodedRoleName = Uri.EscapeDataString(roleName);
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/dataAccessRoles/{encodedRoleName}?preview=true";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, OneLakeJsonContext.Default.DataAccessRole, cancellationToken) ?? new DataAccessRole();
    }

    public async Task<DataAccessRole> CreateOrUpdateDataAccessRoleAsync(string workspaceId, string itemId, string roleDefinitionJson, CancellationToken cancellationToken = default)
    {
        DataAccessRole roleDefinition;
        try
        {
            roleDefinition = JsonSerializer.Deserialize(roleDefinitionJson, OneLakeJsonContext.Default.DataAccessRole)
                ?? throw new ArgumentException("Invalid role definition JSON: deserialized to null.", nameof(roleDefinitionJson));
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid role definition JSON: {ex.Message}", nameof(roleDefinitionJson), ex);
        }

        if (string.IsNullOrWhiteSpace(roleDefinition.Name))
        {
            throw new ArgumentException("Role definition must include a non-empty 'name' property.", nameof(roleDefinitionJson));
        }

        // Default tenantId on Entra members when omitted — saves callers from having to
        // look up their own tenant ID for the most common single-tenant scenario.
        if (roleDefinition.Members?.MicrosoftEntraMembers is { Count: > 0 } entraMembers
            && entraMembers.Any(m => string.IsNullOrWhiteSpace(m.TenantId)))
        {
            var tenantId = await GetTenantIdFromTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                foreach (var member in entraMembers)
                {
                    member.TenantId ??= tenantId;
                }
            }
        }

        // Resolve any email/UPN-based principals to Entra object IDs via Graph API.
        if (roleDefinition.Members?.MicrosoftEntraMembers is { Count: > 0 } membersToResolve)
        {
            await ResolvePrincipalsAsync(membersToResolve, cancellationToken);
        }

        // Use the single-role POST endpoint (preview API) with Overwrite conflict policy.
        // This creates the role if it doesn't exist, or replaces it if it does — without
        // touching other roles on the item (unlike the bulk PUT approach).
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/dataAccessRoles?preview=true&dataAccessRoleConflictPolicy=Overwrite";
        var requestBody = JsonSerializer.Serialize(roleDefinition, OneLakeJsonContext.Default.DataAccessRole);
        var responseStream = await SendFabricApiRequestAsync(HttpMethod.Post, url, requestBody, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(responseStream, OneLakeJsonContext.Default.DataAccessRole, cancellationToken) ?? roleDefinition;
    }

    public async Task DeleteDataAccessRoleAsync(string workspaceId, string itemId, string roleName, CancellationToken cancellationToken = default)
    {
        var encodedRoleName = Uri.EscapeDataString(roleName);
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/dataAccessRoles/{encodedRoleName}?preview=true";
        await SendFabricApiDeleteRequestAsync(url, cancellationToken);
    }

    // Shortcut Operations
    public async Task<ShortcutListResponse> ListShortcutsAsync(string workspaceId, string itemId, string? parentPath = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts";
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(parentPath))
        {
            queryParams.Add($"parentPath={Uri.EscapeDataString(parentPath)}");
        }

        if (!string.IsNullOrWhiteSpace(continuationToken))
        {
            queryParams.Add($"continuationToken={Uri.EscapeDataString(continuationToken)}");
        }

        if (queryParams.Count > 0)
        {
            url += $"?{string.Join("&", queryParams)}";
        }

        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, OneLakeJsonContext.Default.ShortcutListResponse, cancellationToken) ?? new ShortcutListResponse();
    }

    public async Task<OneLakeShortcut> GetShortcutAsync(string workspaceId, string itemId, string shortcutPath, string shortcutName, CancellationToken cancellationToken = default)
    {
        var encodedPath = Uri.EscapeDataString(shortcutPath);
        var encodedName = Uri.EscapeDataString(shortcutName);
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts/{encodedPath}/{encodedName}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, OneLakeJsonContext.Default.OneLakeShortcut, cancellationToken) ?? new OneLakeShortcut();
    }

    public async Task<BulkCreateShortcutResponse> CreateOrUpdateShortcutsAsync(string workspaceId, string itemId, string shortcutsJson, string? shortcutConflictPolicy = null, CancellationToken cancellationToken = default)
    {
        BulkCreateShortcutsRequest request;
        try
        {
            request = JsonSerializer.Deserialize(shortcutsJson, OneLakeJsonContext.Default.BulkCreateShortcutsRequest)
                ?? throw new ArgumentException("Invalid shortcuts JSON: deserialized to null.", nameof(shortcutsJson));
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid shortcuts JSON: {ex.Message}", nameof(shortcutsJson), ex);
        }

        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts/bulkCreate";
        if (!string.IsNullOrWhiteSpace(shortcutConflictPolicy))
            url += $"?shortcutConflictPolicy={Uri.EscapeDataString(shortcutConflictPolicy)}";

        var body = JsonSerializer.Serialize(request, OneLakeJsonContext.Default.BulkCreateShortcutsRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, body, cancellationToken: cancellationToken);
        using var reader = new StreamReader(response);
        var responseBody = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
            return new BulkCreateShortcutResponse();
        return JsonSerializer.Deserialize(responseBody, OneLakeJsonContext.Default.BulkCreateShortcutResponse)
            ?? new BulkCreateShortcutResponse();
    }

    public async Task<OneLakeShortcut> CreateShortcutAsync(string workspaceId, string itemId, OneLakeShortcut shortcut, string? shortcutConflictPolicy = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts";
        if (!string.IsNullOrWhiteSpace(shortcutConflictPolicy))
            url += $"?shortcutConflictPolicy={Uri.EscapeDataString(shortcutConflictPolicy)}";

        var body = JsonSerializer.Serialize(shortcut, OneLakeJsonContext.Default.OneLakeShortcut);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, body, cancellationToken: cancellationToken);
        using var reader = new StreamReader(response);
        var responseBody = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseBody))
            return shortcut;
        return JsonSerializer.Deserialize(responseBody, OneLakeJsonContext.Default.OneLakeShortcut) ?? shortcut;
    }

    public async Task DeleteShortcutAsync(string workspaceId, string itemId, string shortcutPath, string shortcutName, CancellationToken cancellationToken = default)
    {
        var encodedPath = Uri.EscapeDataString(shortcutPath);
        var encodedName = Uri.EscapeDataString(shortcutName);
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts/{encodedPath}/{encodedName}";
        await SendFabricApiDeleteRequestAsync(url, cancellationToken);
    }

    public async Task ResetShortcutCacheAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/onelake/resetShortcutCache";
        await SendFabricApiRequestAsync(HttpMethod.Post, url, cancellationToken: cancellationToken);
    }

    // Settings Operations
    public async Task<OneLakeSettings> GetSettingsAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/onelake/settings";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync(response, OneLakeJsonContext.Default.OneLakeSettings, cancellationToken) ?? new OneLakeSettings();
    }

    public async Task ModifyDiagnosticsAsync(string workspaceId, OneLakeDiagnosticSettings settings, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/onelake/settings/modifyDiagnostics";
        var jsonContent = JsonSerializer.Serialize(settings, OneLakeJsonContext.Default.OneLakeDiagnosticSettings);
        await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, cancellationToken: cancellationToken);
    }

    public async Task ModifyImmutabilityPolicyAsync(string workspaceId, ImmutabilityPolicy policy, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/onelake/settings/modifyImmutabilityPolicy";
        var jsonContent = JsonSerializer.Serialize(policy, OneLakeJsonContext.Default.ImmutabilityPolicy);
        await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, cancellationToken: cancellationToken);
    }

    private async Task SendFabricApiDeleteRequestAsync(string url, CancellationToken cancellationToken)
    {
        var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        ApplyUserAgent(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string?> GetTenantIdFromTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
            var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

            // JWT is three base64url segments separated by dots; the payload is segment[1].
            var parts = token.Token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var payload = parts[1];
            // Pad base64url to standard base64
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            using var doc = JsonDocument.Parse(bytes);
            if (doc.RootElement.TryGetProperty("tid", out var tidElement))
            {
                return tidElement.GetString();
            }
        }
        catch
        {
            // Best-effort: if token parsing fails, caller should provide tenantId explicitly.
        }

        return null;
    }

    /// <summary>
    /// Resolves non-GUID objectId values (email/UPN) to Entra object IDs via Microsoft Graph.
    /// Resolution order: /users/{x} → /groups?$filter=mail eq '{x}'.
    /// </summary>
    private async Task ResolvePrincipalsAsync(List<MicrosoftEntraMember> members, CancellationToken cancellationToken)
    {
        var membersToResolve = members.Where(m => !string.IsNullOrWhiteSpace(m.ObjectId) && !Guid.TryParse(m.ObjectId, out _)).ToList();
        if (membersToResolve.Count == 0)
        {
            return;
        }

        const string graphScope = "https://graph.microsoft.com/.default";
        var tokenContext = new TokenRequestContext(new[] { graphScope });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);

        var errors = new List<string>();

        foreach (var member in membersToResolve)
        {
            var principal = member.ObjectId!.Trim();
            var resolved = await TryResolveUserAsync(principal, token.Token, cancellationToken)
                        ?? await TryResolveGroupByMailAsync(principal, token.Token, cancellationToken);

            if (resolved == null)
            {
                errors.Add($"No Entra principal matched '{principal}'. Ensure the email/UPN is correct and you have User.Read.All and GroupMember.Read.All permissions.");
                continue;
            }

            member.ObjectId = resolved.Value.ObjectId;
            member.ObjectType ??= resolved.Value.ObjectType;
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException("Failed to resolve one or more principals:\n" + string.Join("\n", errors));
        }
    }

    private async Task<(string ObjectId, string ObjectType)?> TryResolveUserAsync(string principalValue, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(principalValue)}?$select=id";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var id = doc.RootElement.GetProperty("id").GetString();
            return id != null ? (id, "User") : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<(string ObjectId, string ObjectType)?> TryResolveGroupByMailAsync(string mail, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var filter = Uri.EscapeDataString($"mail eq '{mail}'");
            var url = $"https://graph.microsoft.com/v1.0/groups?$filter={filter}&$select=id,displayName&$top=1";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var values = doc.RootElement.GetProperty("value");
            if (values.GetArrayLength() == 0)
            {
                return null;
            }

            var id = values[0].GetProperty("id").GetString();
            return id != null ? (id, "Group") : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static string ExtractWarehouseQueryValue(string warehousePrefix)
    {
        const string WarehousePrefixRoot = "warehouse/";
        if (warehousePrefix.StartsWith(WarehousePrefixRoot, StringComparison.OrdinalIgnoreCase))
        {
            return warehousePrefix[WarehousePrefixRoot.Length..];
        }

        return warehousePrefix;
    }

    private async Task<(string WorkspaceId, string ItemIdentifier, string WarehousePrefix, string WarehouseQueryValue)> GetWarehousePrefixAsync(string workspaceIdentifier, string itemIdentifier, CancellationToken cancellationToken)
    {
        var normalizedWorkspaceId = NormalizeWorkspaceIdentifier(workspaceIdentifier);
        var normalizedItemIdentifier = itemIdentifier?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedItemIdentifier))
        {
            throw new ArgumentException("Item identifier is required.", nameof(itemIdentifier));
        }

        if (Guid.TryParse(normalizedWorkspaceId, out _))
        {
            normalizedItemIdentifier = await ResolveItemIdentifierAsync(normalizedWorkspaceId, normalizedItemIdentifier, cancellationToken);
        }

        var workspaceSegment = Uri.EscapeDataString(normalizedWorkspaceId.TrimEnd('/'));
        var itemSegment = Uri.EscapeDataString(normalizedItemIdentifier.TrimStart('/'));
        var warehousePrefix = $"{workspaceSegment}/{itemSegment}";
        var warehouseQueryValue = warehousePrefix;

        return (normalizedWorkspaceId, normalizedItemIdentifier, warehousePrefix, warehouseQueryValue);
    }
}
