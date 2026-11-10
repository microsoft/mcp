// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<WorkspaceRoleAssignmentGetCommandResult> GetWorkspaceRoleAssignmentAsync(
        string workspaceId, string workspaceRoleAssignmentId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(workspaceRoleAssignmentId, out var assignmentId) || assignmentId == Guid.Empty)
        {
            throw new ArgumentException("Workspace role assignment ID must be a nonempty UUID.", nameof(workspaceRoleAssignmentId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/roleAssignments/{assignmentId:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new WorkspaceRoleAssignmentGetRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
            }
            var body = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException(InvalidWorkspaceRoleAssignmentMessage), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var result = ReadWorkspaceRoleAssignmentSnapshot(body, assignmentId);
            cancellationToken.ThrowIfCancellationRequested();
            return result with
            {
                RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
                RetryAfterUtc = retryAfter?.Date
            };
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException)
        {
            var status = ex switch
            {
                JsonException => HttpStatusCode.BadGateway,
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            };
            throw new WorkspaceRoleAssignmentGetRequestException(status, retryAfter);
        }
    }

    private static WorkspaceRoleAssignmentGetCommandResult ReadWorkspaceRoleAssignmentSnapshot(
        ReadOnlyMemory<byte> body, Guid assignmentId)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            FabricCoreHttpHelpers.ValidateMetadataJson(document.RootElement);
            var assignment = ReadWorkspaceRoleAssignment(document.RootElement);
            if (assignment.Id != assignmentId)
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
            }
            var result = new WorkspaceRoleAssignmentGetCommandResult(assignment);
            // Reserve namespace, tool-response, and JSON-RPC envelopes. Without a collection
            // array, 30 nested principals without leaf details fit exactly 61 payload levels.
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = MaxRetainedRoleAssignmentDepth });
            JsonSerializer.Serialize(writer, result, CoreJsonContext.Default.WorkspaceRoleAssignmentGetCommandResult);
            return result;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // Lazy JSON decoding belongs here, not around unrelated stream/HTTP failures.
            throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
        }
    }
}
