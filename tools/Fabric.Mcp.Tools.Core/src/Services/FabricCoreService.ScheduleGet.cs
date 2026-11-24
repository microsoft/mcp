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
    public async Task<ItemScheduleGetCommandResult> GetItemScheduleAsync(
        string workspaceId, string itemId, string jobType, string scheduleId,
        CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemScheduleInput.ValidateScheduleContext(workspaceId, itemId, jobType, scheduleId, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        var schedule = Guid.Parse(scheduleId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/{Uri.EscapeDataString(jobType)}/schedules/{schedule:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.IsSuccessStatusCode)
            {
                throw new ItemScheduleGetRequestException(HttpStatusCode.BadGateway, retryAfter);
            }
            var (errorCode, unavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            throw new ItemScheduleGetRequestException(response.StatusCode, retryAfter, errorCode, unavailable);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned a non-JSON schedule.");
            }
            var body = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric schedule exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = FabricItemScheduleJson.ReadResponse(body);
            if (snapshot.Id != schedule)
            {
                throw new JsonException("Fabric returned a different schedule.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(workspace, item, jobType, snapshot,
                retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException)
        {
            // A 200 with unusable metadata is not a schedule snapshot or a mutation receipt.
            throw new ItemScheduleGetRequestException(
                ex is OperationCanceledException ? HttpStatusCode.RequestTimeout : HttpStatusCode.BadGateway, retryAfter);
        }
    }
}
