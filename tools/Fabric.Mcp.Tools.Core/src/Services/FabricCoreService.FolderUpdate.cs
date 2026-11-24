// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<FolderUpdateCommandResult> UpdateFolderAsync(
        string workspaceId, string folderId, string displayName, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        FolderUpdateInput.Validate(workspaceId, folderId, displayName, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }

        var workspace = Guid.Parse(workspaceId);
        var folder = Guid.Parse(folderId);
        var body = JsonSerializer.Serialize(new UpdateFolderRequest(displayName), CoreJsonContext.Default.UpdateFolderRequest);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/folders/{folder:D}";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Patch, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (responseStatus != HttpStatusCode.OK)
            {
                failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
                var (code, unavailable) = response.IsSuccessStatusCode
                    ? (null, false)
                    : await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new FolderUpdateRequestException(failureStatus.Value, retryAfter, code, unavailable,
                    responseStatusCode: responseStatus);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned non-JSON folder metadata.");
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric folder metadata exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = ReadFolder(bytes, workspace, folder);
            if (string.IsNullOrWhiteSpace(snapshot.DisplayName))
            {
                throw new JsonException("Fabric returned invalid updated folder metadata.");
            }
            return new(snapshot, retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is not FolderUpdateRequestException &&
            ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException or AuthenticationFailedException)
        {
            var status = failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                _ when responseStatus == HttpStatusCode.OK => HttpStatusCode.BadGateway,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new FolderUpdateRequestException(status, retryAfter,
                errorMetadataUnavailable: failureStatus.HasValue, requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }
}
