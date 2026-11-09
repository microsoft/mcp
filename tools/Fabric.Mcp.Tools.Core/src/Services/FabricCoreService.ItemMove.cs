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
    public async Task<ItemMoveCommandResult> MoveItemAsync(
        string workspaceId, string itemId, string? targetFolderId = null, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemMoveInput.Validate(workspaceId, itemId, targetFolderId, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }

        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        Guid? target = targetFolderId is null ? null : Guid.Parse(targetFolderId);
        var body = JsonSerializer.Serialize(new MoveItemRequest(target), CoreJsonContext.Default.MoveItemRequest);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/move";
        var requestStarted = false;
        HttpStatusCode? responseStatus = null;
        HttpStatusCode? failureStatus = null;
        RetryConditionHeaderValue? retryAfter = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var response = await SendFabricHttpRequestAsync(
                HttpMethod.Post, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead,
                onSending: () => requestStarted = true, cancellationToken: cancellationToken);
            responseStatus = response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (responseStatus != HttpStatusCode.OK)
            {
                failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
                var (code, unavailable) = response.IsSuccessStatusCode
                    ? (null, false)
                    : await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new ItemMoveRequestException(failureStatus.Value, retryAfter, code, unavailable,
                    responseStatusCode: responseStatus);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned non-JSON moved-item metadata.");
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric moved-item metadata exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var items = ReadMovedItems(bytes, workspace, target);
            return new(items, retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is not ItemMoveRequestException &&
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
            throw new ItemMoveRequestException(status, retryAfter,
                errorMetadataUnavailable: failureStatus.HasValue, requestNotSent: !requestStarted, responseStatusCode: responseStatus);
        }
    }

    private static List<FabricMovedItem> ReadMovedItems(ReadOnlyMemory<byte> bytes, Guid workspaceId, Guid? targetFolderId)
    {
        using var document = JsonDocument.Parse(bytes);
        try
        {
            var root = document.RootElement;
            FabricCoreHttpHelpers.ValidateMetadataJson(root);
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("value", out var value) ||
                value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException("Fabric returned an invalid moved-item collection.");
            }

            List<FabricMovedItem> items = [];
            foreach (var entry in value.EnumerateArray())
            {
                var snapshot = entry.Deserialize(CoreJsonContext.Default.FabricMovedItem);
                if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.Type) ||
                    snapshot.Id == Guid.Empty || snapshot.LogicalId == Guid.Empty ||
                    (snapshot.WorkspaceId is { } workspace && workspace != workspaceId) ||
                    (snapshot.FolderId is { } folder && (folder == Guid.Empty || folder != targetFolderId)) ||
                    (snapshot.Tags is { } tags && tags.Any(static tag => tag is null || tag.Id == Guid.Empty || tag.DisplayName is null)) ||
                    snapshot.SensitivityLabel?.Id == Guid.Empty)
                {
                    throw new JsonException("Fabric returned invalid or conflicting moved-item metadata.");
                }
                // MovedItems has no minimum length or unique-ID constraint. Child IDs need not match the requested item.
                items.Add(snapshot);
            }
            return items;
        }
        catch (InvalidOperationException)
        {
            // String and property-name decoding can fail lazily after JsonDocument.Parse.
            throw new JsonException("Fabric returned invalid moved-item JSON metadata.");
        }
    }
}
