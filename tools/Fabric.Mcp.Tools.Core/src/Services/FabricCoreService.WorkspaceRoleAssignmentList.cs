// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    private const string InvalidWorkspaceRoleAssignmentPageMessage =
        "Fabric returned invalid workspace role-assignment page metadata. No partial page was returned.";

    /// <inheritdoc />
    public async Task<WorkspaceRoleAssignmentListCommandResult> ListWorkspaceRoleAssignmentsAsync(
        string workspaceId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!FabricContinuationToken.IsValidContinuationToken(continuationToken))
        {
            throw new ArgumentException(FabricContinuationToken.InvalidContinuationTokenMessage, nameof(continuationToken));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var collection = new Uri($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/roleAssignments");
        var url = continuationToken is null
            ? collection.AbsoluteUri
            : $"{collection.AbsoluteUri}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new WorkspaceRoleAssignmentListRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }
        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Fabric returned a non-JSON workspace role-assignment page.");
        }
        var body = await FabricResponseBodyReader.ReadAsync(
            response, FabricResponseBodyReader.DefaultMaxJsonBytes,
            static () => new JsonException("The Fabric workspace role-assignment page exceeds the 1 MiB (1,048,576 byte) limit. No partial page was returned."),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var result = ReadWorkspaceRoleAssignmentsPage(body, collection, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result with
        {
            RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
            RetryAfterUtc = retryAfter?.Date
        };
    }

    private static WorkspaceRoleAssignmentListCommandResult ReadWorkspaceRoleAssignmentsPage(
        ReadOnlyMemory<byte> body, Uri collection, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            var page = document.RootElement;
            FabricCoreHttpHelpers.ValidateMetadataJson(page);
            if (page.ValueKind != JsonValueKind.Object ||
                !page.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException(InvalidWorkspaceRoleAssignmentPageMessage);
            }
            var token = ReadJobPageContinuationToken(page, collection, StringComparison.Ordinal);
            List<FabricWorkspaceRoleAssignment> assignments = [];
            foreach (var entry in value.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var assignment = entry.Deserialize(CoreJsonContext.Default.FabricWorkspaceRoleAssignment);
                if (assignment is null || assignment.Id == Guid.Empty || string.IsNullOrWhiteSpace(assignment.Role))
                {
                    throw new JsonException(InvalidWorkspaceRoleAssignmentPageMessage);
                }
                ValidateRoleAssignmentPrincipal(assignment.Principal);
                assignments.Add(assignment);
            }
            var result = new WorkspaceRoleAssignmentListCommandResult(assignments, token);
            // Profile parents can recursively contain principals. Reserve three levels for
            // namespace, tool-response, and JSON-RPC envelopes without truncating metadata.
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = 61 });
            JsonSerializer.Serialize(writer, result, CoreJsonContext.Default.WorkspaceRoleAssignmentListCommandResult);
            return result;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // JSON string/property decoding is lazy. Keep this boundary separate from
            // response reading and HTTP failures, whose status and retry guidance remain authoritative.
            throw new JsonException(InvalidWorkspaceRoleAssignmentPageMessage);
        }
    }

    private static void ValidateRoleAssignmentPrincipal(FabricPrincipal? principal)
    {
        if (principal is null || principal.Id == Guid.Empty || string.IsNullOrWhiteSpace(principal.Type) ||
            principal.ServicePrincipalDetails?.AadAppId == Guid.Empty ||
            (principal.GroupDetails?.GroupType is { } groupType && string.IsNullOrWhiteSpace(groupType)))
        {
            throw new JsonException(InvalidWorkspaceRoleAssignmentPageMessage);
        }
        if (principal.ServicePrincipalProfileDetails?.ParentPrincipal is { } parent)
        {
            ValidateRoleAssignmentPrincipal(parent);
        }
    }
}
