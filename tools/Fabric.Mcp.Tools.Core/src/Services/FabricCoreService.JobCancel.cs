// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemJobInstanceCancelCommandResult> CancelItemJobInstanceAsync(
        string workspaceId, string itemId, string jobInstanceId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            throw new ArgumentException("Item ID must be a nonempty UUID.", nameof(itemId));
        }
        if (!Guid.TryParse(jobInstanceId, out var job) || job == Guid.Empty)
        {
            throw new ArgumentException("Job instance ID must be a nonempty UUID.", nameof(jobInstanceId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Post,
            $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/instances/{job:D}/cancel",
            completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            // The caller already supplied the target. Ignore Location and the body entirely,
            // and never let late request cancellation erase confirmed acceptance.
            return new(workspace, item, job,
                retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date,
                retryAfter is null ? "MissingOrInvalidRetryAfter" : null);
        }
        if (response.IsSuccessStatusCode)
        {
            throw new ItemJobInstanceCancelRequestException(HttpStatusCode.BadGateway, retryAfter);
        }

        var (errorCode, unavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
        throw new ItemJobInstanceCancelRequestException(response.StatusCode, retryAfter, errorCode, unavailable);
    }
}
