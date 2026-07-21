// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Models;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Storage.Services;

public sealed class StorageIntelligenceService(
    IAttachedDiskService attachedDiskService,
    IAzureTokenCredentialProvider tokenCredentialProvider,
    IHttpClientFactory httpClientFactory) : IStorageIntelligenceService
{
    private const string EndpointEnvironmentVariable = "AZURE_MCP_STORAGE_INTELLIGENCE_ENDPOINT";
    private const string ScopeEnvironmentVariable = "AZURE_MCP_STORAGE_INTELLIGENCE_SCOPE";
    private const string TenantEnvironmentVariable = "AZURE_MCP_STORAGE_INTELLIGENCE_TENANT_ID";
    private readonly IAttachedDiskService _attachedDiskService = attachedDiskService;
    private readonly IAzureTokenCredentialProvider _tokenCredentialProvider = tokenCredentialProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<JsonElement> DiagnoseDiskAsync(
        string? resourceId,
        string? subscription = null,
        string? resourceGroup = null,
        string? vm = null,
        string[]? diskNames = null,
        string? startTime = null,
        string? endTime = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEnvironmentVariable(EndpointEnvironmentVariable);
        var scope = GetRequiredEnvironmentVariable(ScopeEnvironmentVariable);
        var tenant = Environment.GetEnvironmentVariable(TenantEnvironmentVariable);
        ValidateEndpoint(endpoint);
        if (!Uri.TryCreate(scope, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"{ScopeEnvironmentVariable} must be an absolute application scope URI.");
        }

        var effectiveResourceId = resourceId;
        string[]? selectedDiskResourceIds = null;
        if (string.IsNullOrWhiteSpace(effectiveResourceId))
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
            ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup);
            ArgumentException.ThrowIfNullOrWhiteSpace(vm);

            var selection = await _attachedDiskService.ResolveFriendlySelectorAsync(
                subscription,
                resourceGroup,
                vm,
                diskNames,
                cancellationToken);
            effectiveResourceId = selection.VmResourceId;
            selectedDiskResourceIds = selection.DiskResourceIds;
        }
        else if (diskNames is { Length: > 0 })
        {
            selectedDiskResourceIds = await _attachedDiskService.ResolveDiskNamesAsync(
                effectiveResourceId,
                diskNames,
                cancellationToken);
        }

        var credential = await _tokenCredentialProvider.GetTokenCredentialAsync(tenant, cancellationToken);
        var accessToken = await credential.GetTokenAsync(
            new TokenRequestContext([scope]),
            cancellationToken);

        var requestBody = new DiskAnalysisRequest
        {
            ResourceId = effectiveResourceId,
            SubResourceIds = selectedDiskResourceIds,
            IssueStartTime = startTime,
            IssueEndTime = endTime
        };
        var requestJson = JsonSerializer.Serialize(requestBody, StorageJsonContext.Default.DiskAnalysisRequest);

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new("Bearer", accessToken.Token);

        using var response = await _httpClientFactory.CreateClient().SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new RequestFailedException(
                (int)response.StatusCode,
                $"Storage Intelligence disk analysis failed with HTTP status {(int)response.StatusCode}.");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var responseDocument = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return responseDocument.RootElement.Clone();
    }

    private static string GetRequiredEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"The server operator must configure {name} before using disk diagnostics.");

    private static void ValidateEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"{EndpointEnvironmentVariable} must be an absolute HTTPS URL.");
        }
    }
}
