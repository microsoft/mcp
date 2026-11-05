// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemScheduleDeleteCommandResult> DeleteItemScheduleAsync(
        string workspaceId, string itemId, string jobType, string scheduleId,
        CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemScheduleInput.ValidateScheduleContext(workspaceId, itemId, jobType, scheduleId, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }

        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        var schedule = Guid.Parse(scheduleId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/{Uri.EscapeDataString(jobType)}/schedules/{schedule:D}";
        var requestStarted = false;
        var deletionConfirmed = false;
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
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Headers confirm deletion. Neither cancellation nor irrelevant response data
                // should turn this into an uncertain mutation or authorize another request.
                deletionConfirmed = true;
                return new(workspace, item, jobType, schedule);
            }

            failureStatus = response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode;
            retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
            if (!response.IsSuccessStatusCode)
            {
                (errorCode, errorMetadataUnavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
            }
            throw new ItemScheduleDeleteRequestException(failureStatus.Value, retryAfter, errorCode, errorMetadataUnavailable);
        }
        catch (Exception ex) when (ex is not ItemScheduleDeleteRequestException &&
            ex is HttpRequestException or IOException or OperationCanceledException or TimeoutException)
        {
            var status = deletionConfirmed ? HttpStatusCode.BadGateway : failureStatus ?? (ex switch
            {
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            });
            throw new ItemScheduleDeleteRequestException(status, retryAfter, errorCode, errorMetadataUnavailable,
                requestNotSent: !requestStarted, deletionConfirmed: deletionConfirmed);
        }
    }
}
