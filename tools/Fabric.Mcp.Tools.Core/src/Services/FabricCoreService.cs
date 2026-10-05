// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public class FabricCoreService(HttpClient httpClient, TokenCredential? credential = null) : IFabricCoreService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? new DefaultAzureCredential();
    private const string UserAgentHeaderName = "User-Agent";
    private const string UserAgentHeaderValue = "Fabric Core MCP";
    private const string InvalidUpdateResponseMessage = "Fabric returned an invalid Update Item response.";

    /// <inheritdoc />
    public async Task<CapacityListResponse> ListCapacitiesAsync(
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        if (continuationToken is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(continuationToken);
        }
        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/capacities";
        if (continuationToken is not null)
        {
            url += $"?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}";
        }

        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get,
            url,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new CapacityListThrottledException(retryAfter);
            }

            throw new HttpRequestException(
                "Unable to list Fabric capacities.",
                null,
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var page = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.CapacityListResponse, cancellationToken);
        if (page?.Value is null || page.Value.Any(static capacity => !FabricCapacityMetadata.IsValid(capacity)))
        {
            throw new JsonException("Fabric returned an invalid capacity metadata page.");
        }

        return page;
    }

    public async Task<FabricItem> CreateItemAsync(string workspaceId, CreateItemRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceId}/items";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.CreateItemRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<FabricItem>(response, CoreJsonContext.Default.FabricItem, cancellationToken) ?? new FabricItem();
    }

    /// <inheritdoc />
    public async Task<FabricCapacityMetadata> GetCapacityAsync(string capacityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(capacityId, out var parsedCapacityId) || parsedCapacityId == Guid.Empty)
        {
            throw new ArgumentException("Capacity ID must be a nonempty UUID.", nameof(capacityId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/capacities/{parsedCapacityId:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new FabricCapacityGetThrottledException(retryAfter);
            }

            var statusCode = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            throw new HttpRequestException("Unable to retrieve Fabric capacity metadata.", null, statusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var capacity = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.FabricCapacityMetadata, cancellationToken);

        if (!FabricCapacityMetadata.IsValid(capacity) || capacity.Id != parsedCapacityId)
        {
            throw new JsonException("Fabric returned invalid capacity metadata.");
        }

        return capacity;
    }

    /// <inheritdoc />
    public async Task<FabricWorkspaceMetadata> GetWorkspaceAsync(
        string workspaceId,
        bool? preferWorkspaceSpecificEndpoints = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var parsedWorkspaceId) || parsedWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{parsedWorkspaceId:D}";
        if (preferWorkspaceSpecificEndpoints is { } prefer)
        {
            url += prefer ? "?preferWorkspaceSpecificEndpoints=true" : "?preferWorkspaceSpecificEndpoints=false";
        }

        using var response = await SendFabricHttpRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new FabricWorkspaceThrottledException(retryAfter);
            }

            var statusCode = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            throw new HttpRequestException("Unable to retrieve Fabric workspace metadata.", null, statusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var workspace = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.FabricWorkspaceMetadata, cancellationToken);

        if (workspace is null || workspace.Id != parsedWorkspaceId ||
            string.IsNullOrWhiteSpace(workspace.DisplayName) || string.IsNullOrWhiteSpace(workspace.Type) ||
            workspace.Tags?.Any(static tag => tag is null || tag.Id == Guid.Empty || string.IsNullOrWhiteSpace(tag.DisplayName)) == true)
        {
            throw new JsonException("Fabric returned invalid workspace metadata.");
        }

        return workspace;
    }

    /// <inheritdoc />
    public async Task<WorkspaceCreateResult> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = WorkspaceCreateInputValidator.GetErrors(
            request.DisplayName, request.Description, request.CapacityId, request.DomainId).ToArray();
        if (errors.Length > 0)
        {
            throw new ArgumentException(string.Join(" ", errors), nameof(request));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces";
        var jsonContent = JsonSerializer.Serialize(
            new CreateWorkspaceRequest
            {
                DisplayName = request.DisplayName,
                Description = request.Description,
                CapacityId = NormalizeWorkspaceAssignmentId(request.CapacityId),
                DomainId = NormalizeWorkspaceAssignmentId(request.DomainId)
            },
            CoreJsonContext.Default.CreateWorkspaceRequest);

        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Post, url, jsonContent,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new WorkspaceCreateRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode,
                FabricCoreHttpHelpers.GetRetryAfter(response));
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var workspace = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.CreatedWorkspaceMetadata, cancellationToken);
        if (workspace is null || workspace.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(workspace.DisplayName) || string.IsNullOrWhiteSpace(workspace.Type) ||
            workspace.CapacityId == Guid.Empty || workspace.DomainId == Guid.Empty ||
            workspace.Tags?.Any(static tag => tag is null || tag.Id == Guid.Empty || string.IsNullOrWhiteSpace(tag.DisplayName)) == true)
        {
            throw new JsonException("Fabric returned invalid created workspace metadata.");
        }

        return new(workspace, GetWorkspaceLocation(response));
    }

    public async Task<CatalogSearchResponse> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/catalog/search";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.CatalogSearchRequest);
        var response = await SendFabricApiRequestAsync(HttpMethod.Post, url, jsonContent, null, cancellationToken);
        return await JsonSerializer.DeserializeAsync<CatalogSearchResponse>(response, CoreJsonContext.Default.CatalogSearchResponse, cancellationToken) ?? new CatalogSearchResponse();
    }

    public async Task<WorkspaceListResponse> ListWorkspacesAsync(
        string? roles = null,
        string? continuationToken = null,
        bool? preferWorkspaceSpecificEndpoints = null,
        CancellationToken cancellationToken = default)
    {
        if (!WorkspaceListInputValidator.TryNormalizeRoles(roles, out var normalizedRoles))
        {
            throw new ArgumentException(WorkspaceListInputValidator.RolesError, nameof(roles));
        }

        if (!WorkspaceListInputValidator.IsValidContinuationToken(continuationToken))
        {
            throw new ArgumentException(WorkspaceListInputValidator.ContinuationTokenError, nameof(continuationToken));
        }

        cancellationToken.ThrowIfCancellationRequested();

        List<string> query = [];
        if (normalizedRoles is not null)
        {
            query.Add($"roles={Uri.EscapeDataString(normalizedRoles)}");
        }
        if (continuationToken is not null)
        {
            query.Add($"continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}");
        }
        if (preferWorkspaceSpecificEndpoints is { } preferEndpoints)
        {
            query.Add($"preferWorkspaceSpecificEndpoints={(preferEndpoints ? "true" : "false")}");
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces";
        if (query.Count > 0)
        {
            url += $"?{string.Join('&', query)}";
        }

        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get,
            url,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new WorkspaceListThrottledException(retryAfter);
            }

            throw new HttpRequestException(
                "Unable to list Fabric workspaces.",
                null,
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var page = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.WorkspaceListResponse, cancellationToken);
        if (page is null || page.Value is null || page.Value.Any(static workspace => !IsValidWorkspace(workspace)))
        {
            throw new JsonException("Fabric returned invalid workspace metadata.");
        }

        return page;
    }

    /// <inheritdoc />
    public async Task<ItemListResponse> ListItemsAsync(
        string workspaceId,
        string? type = null,
        bool recursive = true,
        string? rootFolderId = null,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var parsedWorkspaceId) || parsedWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }

        Guid? parsedFolderId = null;
        if (rootFolderId is not null)
        {
            if (!Guid.TryParse(rootFolderId, out var folderId) || folderId == Guid.Empty)
            {
                throw new ArgumentException("Root folder ID must be a nonempty UUID.", nameof(rootFolderId));
            }
            parsedFolderId = folderId;
        }

        if (type is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type);
        }
        if (continuationToken is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(continuationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        List<string> query = [];
        if (type is not null)
        {
            query.Add($"type={Uri.EscapeDataString(type)}");
        }
        query.Add($"recursive={(recursive ? "true" : "false")}");
        if (parsedFolderId is { } root)
        {
            query.Add($"rootFolderId={root:D}");
        }
        if (continuationToken is not null)
        {
            query.Add($"continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}");
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{parsedWorkspaceId:D}/items?{string.Join("&", query)}";
        using var response = await SendFabricHttpRequestAsync(HttpMethod.Get, url, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new FabricItemListThrottledException(retryAfter);
            }

            throw new HttpRequestException("Unable to list Fabric item metadata.", null,
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var page = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.ItemListResponse, cancellationToken);
        if (page?.Value is null || page.Value.Any(item =>
            item is null || item.Id == Guid.Empty || item.WorkspaceId != parsedWorkspaceId ||
            string.IsNullOrWhiteSpace(item.DisplayName) || string.IsNullOrWhiteSpace(item.Type)))
        {
            throw new JsonException("Fabric returned an invalid item metadata page.");
        }

        return page;
    }

    /// <inheritdoc />
    public async Task<WorkspaceUpdateResponse> UpdateWorkspaceAsync(
        string workspaceId,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = WorkspaceUpdateInputValidator.GetErrors(workspaceId, request.DisplayName, request.Description);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join('\n', errors));
        }

        var parsedId = Guid.Parse(workspaceId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{parsedId:D}";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.UpdateWorkspaceRequest);

        using var response = await SendFabricHttpRequestAsync(HttpMethod.Patch, url, jsonContent, cancellationToken: cancellationToken);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                FabricCoreHttpHelpers.GetRetryAfter(response) is { } retryAfter)
            {
                throw new WorkspaceUpdateThrottledException(retryAfter);
            }

            throw new HttpRequestException(
                "Unable to update Fabric workspace metadata.",
                null,
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var workspace = await JsonSerializer.DeserializeAsync(content, CoreJsonContext.Default.WorkspaceUpdateResponse, cancellationToken);
        if (workspace is null || workspace.Id != parsedId ||
            string.IsNullOrWhiteSpace(workspace.DisplayName) || string.IsNullOrWhiteSpace(workspace.Type))
        {
            throw new JsonException("Fabric returned invalid workspace metadata.");
        }

        return workspace;
    }

    private static bool IsValidWorkspace(FabricWorkspaceListEntry? workspace) =>
        workspace is not null &&
        workspace.Id != Guid.Empty &&
        !string.IsNullOrWhiteSpace(workspace.DisplayName) &&
        !string.IsNullOrWhiteSpace(workspace.Type) &&
        (workspace.Tags is null || workspace.Tags.All(static tag =>
            tag is not null && tag.Id != Guid.Empty && !string.IsNullOrWhiteSpace(tag.DisplayName)));

    /// <inheritdoc />
    public async Task<ItemUpdateMetadata> UpdateItemAsync(
        string workspaceId,
        string itemId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validation = new ValidationResult();
        ItemUpdateInputValidator.Validate(workspaceId, itemId, request.DisplayName, request.Description, validation);
        if (!validation.IsValid)
        {
            throw new ArgumentException(string.Join('\n', validation.Errors));
        }

        var workspaceGuid = Guid.Parse(workspaceId);
        var itemGuid = Guid.Parse(itemId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspaceGuid:D}/items/{itemGuid:D}";
        var jsonContent = JsonSerializer.Serialize(request, CoreJsonContext.Default.UpdateItemRequest);
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Patch, url, jsonContent,
            completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.IsSuccessStatusCode)
            {
                throw new InvalidDataException(InvalidUpdateResponseMessage);
            }

            throw new ItemUpdateRequestException(response.StatusCode, GetUpdateRetryAfterSeconds(response));
        }

        using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
        ItemUpdateMetadata? item;
        try
        {
            item = await JsonSerializer.DeserializeAsync(body, CoreJsonContext.Default.ItemUpdateMetadata, cancellationToken);
        }
        catch (JsonException)
        {
            throw new InvalidDataException(InvalidUpdateResponseMessage);
        }

        if (item is null
            || !Guid.TryParse(item.Id, out var responseItemId) || responseItemId != itemGuid
            || !Guid.TryParse(item.WorkspaceId, out var responseWorkspaceId) || responseWorkspaceId != workspaceGuid
            || string.IsNullOrWhiteSpace(item.DisplayName)
            || string.IsNullOrWhiteSpace(item.Type))
        {
            throw new InvalidDataException(InvalidUpdateResponseMessage);
        }

        return item;
    }

    private static int? GetUpdateRetryAfterSeconds(HttpResponseMessage response)
    {
        return FabricCoreHttpHelpers.GetRetryAfter(response)?.Delta is { TotalSeconds: >= 0 and <= int.MaxValue } delta
            ? (int)delta.TotalSeconds
            : null;
    }

    private async Task<Stream> SendFabricApiRequestAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendFabricHttpRequestAsync(method, url, jsonContent, headers, cancellationToken: cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendFabricHttpRequestAsync(
        HttpMethod method,
        string url,
        string? jsonContent = null,
        Dictionary<string, string>? headers = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
    {
        var tokenRequestContext = new TokenRequestContext(FabricEndpoints.FabricScopes);
        var accessToken = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        request.Headers.Add(UserAgentHeaderName, UserAgentHeaderValue);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (jsonContent != null)
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        return await _httpClient.SendAsync(request, completionOption, cancellationToken);
    }

    private static string? NormalizeWorkspaceAssignmentId(string? value) => value is null ? null : Guid.Parse(value).ToString("D");

    private static string? GetWorkspaceLocation(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Location", out var values))
        {
            return null;
        }

        var locations = values.ToArray();
        if (locations.Length != 1 || string.IsNullOrWhiteSpace(locations[0]) ||
            !Uri.TryCreate(locations[0], UriKind.RelativeOrAbsolute, out var location))
        {
            throw new JsonException("Fabric returned an invalid workspace Location header.");
        }

        return location.OriginalString;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Fabric API request failed with status {(int)response.StatusCode} ({response.StatusCode}): {content}");
        }
    }
}
