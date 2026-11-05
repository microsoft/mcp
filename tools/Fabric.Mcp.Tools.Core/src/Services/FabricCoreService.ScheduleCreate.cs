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
    public async Task<ItemScheduleCreateCommandResult> CreateItemScheduleAsync(
        string workspaceId, string itemId, string jobType, bool enabled, string configuration,
        string? executionData = null, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(workspaceId, itemId, jobType, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }
        var body = ItemScheduleInput.CreateBody(enabled, configuration, executionData, cancellationToken);
        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/{Uri.EscapeDataString(jobType)}/schedules";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Post, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            if (response.IsSuccessStatusCode)
            {
                throw new ItemScheduleCreateRequestException(HttpStatusCode.BadGateway, retryAfter);
            }
            var (errorCode, unavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            throw new ItemScheduleCreateRequestException(response.StatusCode, retryAfter, errorCode, unavailable);
        }

        // A 201 confirms creation even if reading its snapshot fails. Location is optional;
        // do not follow it or interpret operation headers as permission to poll.
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
            var schedule = FabricItemScheduleJson.ReadMutationResponse(bytes);
            return new(workspace, item, jobType, schedule,
                retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException)
        {
            throw new ItemScheduleCreateRequestException(
                ex is OperationCanceledException ? HttpStatusCode.RequestTimeout : HttpStatusCode.BadGateway,
                retryAfter, creationConfirmed: true);
        }
    }
}
