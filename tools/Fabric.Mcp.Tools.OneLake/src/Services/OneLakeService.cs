// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.OneLake.Models;

namespace Fabric.Mcp.Tools.OneLake.Services;

public class OneLakeService(HttpClient httpClient) : IOneLakeService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly DefaultAzureCredential _credential = new();

    // Workspace Operations
    public async Task<IEnumerable<Workspace>> ListWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"?continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<WorkspacesResponse>(response, OneLakeJsonContext.Default.WorkspacesResponse, cancellationToken);
        return result?.Value ?? [];
    }

    public async Task<IEnumerable<Workspace>> ListOneLakeWorkspacesAsync(string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/?comp=list";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        
        // OneLake API returns XML in Azure Storage format
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
                    Id = container.Element("Name")?.Value ?? "",
                    DisplayName = container.Element("Name")?.Value ?? ""
                };

                // Parse Properties
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

                // Parse Metadata
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
                        throw new InvalidOperationException($"Failed to parse OneLake workspace list response: {ex.Message}", ex);
            throw new InvalidOperationException($"Failed to parse OneLake response: {ex.Message}");
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
        
        // Return raw XML response
        using var reader = new StreamReader(response);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public async Task<Workspace> GetWorkspaceAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync<Workspace>(response, OneLakeJsonContext.Default.Workspace, cancellationToken) ?? new Workspace();
    }

    // Item Operations
    public async Task<IEnumerable<OneLakeItem>> ListItemsAsync(string workspaceId, string? itemType = null, bool recursive = true, string? rootFolderId = null, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(itemType)) queryParams.Add($"type={Uri.EscapeDataString(itemType)}");
        queryParams.Add($"recursive={recursive.ToString().ToLower()}");
        if (!string.IsNullOrEmpty(rootFolderId)) queryParams.Add($"rootFolderId={Uri.EscapeDataString(rootFolderId)}");
        if (!string.IsNullOrEmpty(continuationToken)) queryParams.Add($"continuationToken={Uri.EscapeDataString(continuationToken)}");

        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items";
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<ItemsResponse>(response, OneLakeJsonContext.Default.ItemsResponse, cancellationToken);
        return result?.Value ?? [];
    }

    public async Task<OneLakeItem> GetItemAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync<OneLakeItem>(response, OneLakeJsonContext.Default.OneLakeItem, cancellationToken) ?? new OneLakeItem();
    }

    public async Task<OneLakeItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items";
        var jsonContent = JsonSerializer.Serialize(request, OneLakeJsonContext.Default.CreateItemRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        
        try
        {
            return await JsonSerializer.DeserializeAsync<OneLakeItem>(response, OneLakeJsonContext.Default.OneLakeItem, cancellationToken) ?? new OneLakeItem();
        }
        catch (JsonException)
        {
            // Reset stream position to read the content for debugging
            response.Position = 0;
            var responseContent = await new StreamReader(response).ReadToEndAsync();
            Console.WriteLine($"Response content: {responseContent}");
            throw;
        }
    }

    public async Task<OneLakeItem> UpdateItemAsync(string workspaceId, string itemId, UpdateItemRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}";
        var jsonContent = JsonSerializer.Serialize(request, OneLakeJsonContext.Default.UpdateItemRequest);
        var response = await SendFabricApiRequestAsync(new HttpMethod("PATCH"), url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<OneLakeItem>(response, OneLakeJsonContext.Default.OneLakeItem, cancellationToken) ?? new OneLakeItem();
    }

    public async Task DeleteItemAsync(string workspaceId, string itemId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}";
        await SendFabricApiRequestAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);
    }

    // Lakehouse Operations
    public async Task<IEnumerable<Lakehouse>> ListLakehousesAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/lakehouses";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"?continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<LakehousesResponse>(response, OneLakeJsonContext.Default.LakehousesResponse, cancellationToken);
        return result?.Value ?? [];
    }

    public async Task<Lakehouse> GetLakehouseAsync(string workspaceId, string lakehouseId, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/lakehouses/{lakehouseId}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync<Lakehouse>(response, OneLakeJsonContext.Default.Lakehouse, cancellationToken) ?? new Lakehouse();
    }

    // OneLake Shortcuts Operations  
    public async Task<IEnumerable<OneLakeShortcut>> ListShortcutsAsync(string workspaceId, string itemId, string path, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts?path={Uri.EscapeDataString(path)}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<ShortcutsResponse>(response, OneLakeJsonContext.Default.ShortcutsResponse, cancellationToken);
        return result?.Value ?? [];
    }

    public async Task<OneLakeShortcut> GetShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts/{Uri.EscapeDataString(shortcutName)}?path={Uri.EscapeDataString(path)}";
        var response = await SendFabricApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await JsonSerializer.DeserializeAsync<OneLakeShortcut>(response, OneLakeJsonContext.Default.OneLakeShortcut, cancellationToken) ?? new OneLakeShortcut();
    }

    public async Task<OneLakeShortcut> CreateShortcutAsync(string workspaceId, string itemId, string path, CreateShortcutRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts?path={Uri.EscapeDataString(path)}";
        var jsonContent = JsonSerializer.Serialize(request, OneLakeJsonContext.Default.CreateShortcutRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<OneLakeShortcut>(response, OneLakeJsonContext.Default.OneLakeShortcut, cancellationToken) ?? new OneLakeShortcut();
    }

    public async Task DeleteShortcutAsync(string workspaceId, string itemId, string path, string shortcutName, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items/{itemId}/shortcuts/{Uri.EscapeDataString(shortcutName)}?path={Uri.EscapeDataString(path)}";
        await SendFabricApiRequestAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);
    }

    // Data Operations (OneLake Data Plane)
    public async Task<OneLakeFileInfo> GetFileInfoAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}/{itemId}/Files/{filePath.TrimStart('/')}";
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

    public async Task<IEnumerable<OneLakeFileInfo>> ListFilesAsync(string workspaceId, string itemId, string? path = null, bool recursive = false, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}/{itemId}/Files";
        if (!string.IsNullOrEmpty(path))
        {
            url += $"/{path.TrimStart('/')}";
        }
        url += $"?resource=filesystem&recursive={recursive.ToString().ToLower()}";

        var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // Parse directory listing response (simplified)
        var files = new List<OneLakeFileInfo>();
        // Implementation would parse the actual response format
        return files;
    }

    public async Task<IEnumerable<OneLakeItem>> ListOneLakeItemsAsync(string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}?delimiter=/&restype=container&comp=list";
        if (!string.IsNullOrEmpty(continuationToken))
        {
            url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
        }

        var response = await SendOneLakeApiRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        
        // OneLake API returns XML in Azure Storage format for item listing
        using var reader = new StreamReader(response);
        var xmlContent = await reader.ReadToEndAsync(cancellationToken);
        
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

    public async Task<Stream> ReadFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}/{itemId}/Files/{filePath.TrimStart('/')}";
        var response = await SendDataPlaneRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task WriteFileAsync(string workspaceId, string itemId, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}/{itemId}/Files/{filePath.TrimStart('/')}";
        
        // Create or overwrite file
        using var createRequest = new HttpRequestMessage(HttpMethod.Put, url);
        createRequest.Headers.Add("x-ms-resource", "file");
        if (overwrite)
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

    public async Task DeleteFileAsync(string workspaceId, string itemId, string filePath, CancellationToken cancellationToken = default)
    {
        var url = $"{OneLakeEndpoints.OneLakeDataPlaneBaseUrl}/{workspaceId}/{itemId}/Files/{filePath.TrimStart('/')}";
        await SendDataPlaneRequestAsync(HttpMethod.Delete, url, cancellationToken: cancellationToken);
    }

    // Private helper methods
    private async Task<Stream> SendFabricApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        var tokenContext = new TokenRequestContext(new[] { OneLakeEndpoints.GetFabricScope() });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);
        
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        
        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<Stream> SendOneLakeApiRequestAsync(HttpMethod method, string url, string? jsonContent = null, CancellationToken cancellationToken = default)
    {
        var tokenContext = new Azure.Core.TokenRequestContext(new[] { OneLakeEndpoints.StorageScope });
        var token = await _credential.GetTokenAsync(tokenContext, cancellationToken);
        
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Add("x-ms-version", "2023-11-03");
        
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
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return response;
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
}
