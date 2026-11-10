// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<FolderDeleteCommandResult> DeleteFolderAsync(
        string workspaceId, string folderId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(folderId, out var folder) || folder == Guid.Empty)
        {
            throw new ArgumentException("Folder ID must be a nonempty UUID.", nameof(folderId));
        }

        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/folders/{folder:D}";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        string? errorCode = null;
        var errorMetadataUnavailable = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Delete, url, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            if (responseStatus == HttpStatusCode.OK)
            {
                // Headers confirm deletion; late cancellation or irrelevant body data must not erase it.
                return new(workspace, folder);
            }

            failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (!response.IsSuccessStatusCode)
            {
                errorMetadataUnavailable = true;
                (errorCode, errorMetadataUnavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            }
            throw new FolderDeleteRequestException(failureStatus.Value, retryAfter, errorCode, errorMetadataUnavailable,
                responseStatusCode: responseStatus);
        }
        catch (Exception ex) when (ex is not FolderDeleteRequestException &&
            ex is HttpRequestException or IOException or OperationCanceledException or TimeoutException or AuthenticationFailedException)
        {
            var status = responseStatus == HttpStatusCode.OK ? HttpStatusCode.BadGateway : failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new FolderDeleteRequestException(status, retryAfter, errorCode, errorMetadataUnavailable,
                requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }
}
