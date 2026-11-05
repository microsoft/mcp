// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemScheduleUpdateCommandResult> UpdateItemScheduleAsync(
        string workspaceId, string itemId, string jobType, string scheduleId, bool enabled, string configuration,
        string? executionData = null, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemScheduleInput.ValidateUpdateContext(workspaceId, itemId, jobType, scheduleId, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }
        var body = ItemScheduleInput.CreateBody(enabled, configuration, executionData, cancellationToken);
        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        var schedule = Guid.Parse(scheduleId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/{Uri.EscapeDataString(jobType)}/schedules/{schedule:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Patch, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.IsSuccessStatusCode)
            {
                throw new ItemScheduleUpdateRequestException(HttpStatusCode.BadGateway, retryAfter);
            }
            var (errorCode, unavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            throw new ItemScheduleUpdateRequestException(response.StatusCode, retryAfter, errorCode, unavailable);
        }

        // HTTP 200 confirms the update even if its snapshot cannot be used. This API
        // is synchronous; Location and operation headers never authorize a follow-up.
        try
        {
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned a non-JSON schedule.");
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric schedule exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = FabricItemScheduleJson.ReadMutationResponse(bytes);
            if (snapshot.Id != schedule)
            {
                throw new JsonException("Fabric returned a different schedule.");
            }
            return new(workspace, item, jobType, snapshot,
                retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException)
        {
            throw new ItemScheduleUpdateRequestException(
                ex is OperationCanceledException ? HttpStatusCode.RequestTimeout : HttpStatusCode.BadGateway,
                retryAfter, updateConfirmed: true);
        }
    }
}
