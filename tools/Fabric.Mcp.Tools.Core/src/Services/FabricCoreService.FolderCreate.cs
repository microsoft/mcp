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
    public async Task<FolderCreateCommandResult> CreateFolderAsync(
        string workspaceId, string displayName, string? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        FolderCreateInput.Validate(workspaceId, displayName, parentFolderId, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }

        var workspace = Guid.Parse(workspaceId);
        Guid? parent = parentFolderId is null ? null : Guid.Parse(parentFolderId);
        var body = JsonSerializer.Serialize(new CreateFolderRequest(displayName, parent), CoreJsonContext.Default.CreateFolderRequest);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/folders";
        var requestStarted = false;
        var creationConfirmed = false;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Post, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            creationConfirmed = response.StatusCode == HttpStatusCode.Created;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (!creationConfirmed)
            {
                failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
                var (code, unavailable) = response.IsSuccessStatusCode
                    ? (null, false)
                    : await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new FolderCreateRequestException(failureStatus.Value, retryAfter, code, unavailable);
            }

            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned non-JSON folder metadata.");
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric folder metadata exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var folder = ReadCreatedFolder(bytes, workspace, parent);
            return new(folder, retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is not FolderCreateRequestException &&
            ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException or AuthenticationFailedException)
        {
            var status = failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                _ when creationConfirmed => HttpStatusCode.BadGateway,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new FolderCreateRequestException(status, retryAfter,
                errorMetadataUnavailable: failureStatus.HasValue, requestNotSent: !requestStarted, creationConfirmed: creationConfirmed);
        }
    }

    private static FabricFolder ReadCreatedFolder(ReadOnlyMemory<byte> bytes, Guid workspaceId, Guid? parentFolderId)
    {
        try
        {
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            FabricCoreHttpHelpers.ValidateMetadataJson(document.RootElement);
            var folder = document.RootElement.Deserialize(CoreJsonContext.Default.FabricFolder);
            if (folder is null || folder.Id == Guid.Empty || folder.WorkspaceId != workspaceId ||
                string.IsNullOrWhiteSpace(folder.DisplayName) || folder.ParentFolderId == Guid.Empty ||
                (folder.ParentFolderId is { } parent && parent != parentFolderId))
            {
                throw new JsonException("Fabric returned invalid created folder metadata.");
            }
            return folder;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Decode lazy JSON strings here, inside the confirmed-creation boundary.
            throw new JsonException("Fabric returned invalid created folder metadata.");
        }
    }
}
