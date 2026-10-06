// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Net;
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
    internal const int ProvisionalMaxResponseSizeBytes = 10 * 1024 * 1024;
    private const int ResponseReadBufferSizeBytes = 80 * 1024;
    private const int InitialResponseCapacityBytes = 16 * 1024;
    private const string DefaultEndpoint = "https://storageintelligenceweb.production.portalrp.azure.com/api/Disk/analyze";
    private const string DefaultScope = "a5965d26-227b-4df0-a516-d86aba489cb6/.default";
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
        var endpoint = GetEnvironmentVariableOrDefault(EndpointEnvironmentVariable, DefaultEndpoint);
        var scope = GetEnvironmentVariableOrDefault(ScopeEnvironmentVariable, DefaultScope);
        var tenant = Environment.GetEnvironmentVariable(TenantEnvironmentVariable);
        ValidateEndpoint(endpoint);
        if (!IsValidApplicationScope(scope))
        {
            throw new InvalidOperationException(
                $"{ScopeEnvironmentVariable} must be an absolute scope URI or use the format <application-id>/.default.");
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

        return await ReadResponseAsync(response.Content, cancellationToken);
    }

    private static async Task<JsonElement> ReadResponseAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is > ProvisionalMaxResponseSizeBytes)
        {
            throw CreateInvalidResponseException(
                $"Storage Intelligence disk analysis returned a response larger than the provisional {ProvisionalMaxResponseSizeBytes} byte safety limit.");
        }

        var initialCapacity = content.Headers.ContentLength is > 0 and var contentLength
            ? (int)Math.Min(contentLength, InitialResponseCapacityBytes)
            : 0;
        using var bufferedResponse = new MemoryStream(initialCapacity);
        var readBuffer = ArrayPool<byte>.Shared.Rent(ResponseReadBufferSizeBytes);
        try
        {
            await using var responseStream = await content.ReadAsStreamAsync(cancellationToken);
            var totalBytesRead = 0;
            while (true)
            {
                var bytesRead = await responseStream.ReadAsync(
                    readBuffer.AsMemory(),
                    cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                if (totalBytesRead > ProvisionalMaxResponseSizeBytes - bytesRead)
                {
                    throw CreateInvalidResponseException(
                        $"Storage Intelligence disk analysis returned a response larger than the provisional {ProvisionalMaxResponseSizeBytes} byte safety limit.");
                }

                EnsureResponseCapacity(bufferedResponse, totalBytesRead + bytesRead);
                await bufferedResponse.WriteAsync(
                    readBuffer.AsMemory(0, bytesRead),
                    cancellationToken);
                totalBytesRead += bytesRead;
            }

            if (IsEmptyJsonContent(bufferedResponse.GetBuffer().AsSpan(0, totalBytesRead)))
            {
                throw CreateInvalidResponseException(
                    "Storage Intelligence disk analysis returned an empty response.");
            }

            try
            {
                using var responseDocument = JsonDocument.Parse(
                    bufferedResponse.GetBuffer().AsMemory(0, totalBytesRead));
                return responseDocument.RootElement.Clone();
            }
            catch (JsonException ex)
            {
                throw CreateInvalidResponseException(
                    "Storage Intelligence disk analysis returned invalid JSON.",
                    ex);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer, clearArray: true);
        }
    }

    private static void EnsureResponseCapacity(MemoryStream bufferedResponse, int requiredCapacity)
    {
        if (requiredCapacity <= bufferedResponse.Capacity)
        {
            return;
        }

        var doubledCapacity = bufferedResponse.Capacity == 0
            ? InitialResponseCapacityBytes
            : bufferedResponse.Capacity * 2L;
        bufferedResponse.Capacity = (int)Math.Min(
            ProvisionalMaxResponseSizeBytes,
            Math.Max(requiredCapacity, doubledCapacity));
    }

    private static bool IsEmptyJsonContent(ReadOnlySpan<byte> content)
    {
        foreach (var value in content)
        {
            if (value is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n'))
            {
                return false;
            }
        }

        return true;
    }

    private static RequestFailedException CreateInvalidResponseException(
        string message,
        Exception? innerException = null) =>
        new((int)HttpStatusCode.BadGateway, message, errorCode: null, innerException);

    private static string GetEnvironmentVariableOrDefault(string name, string defaultValue) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : defaultValue;

    private static void ValidateEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"{EndpointEnvironmentVariable} must be an absolute HTTPS URL.");
        }
    }

    private static bool IsValidApplicationScope(string scope)
    {
        if (Uri.TryCreate(scope, UriKind.Absolute, out _))
        {
            return true;
        }

        const string defaultScopeSuffix = "/.default";
        return scope.EndsWith(defaultScopeSuffix, StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(scope[..^defaultScopeSuffix.Length], out _);
    }
}
