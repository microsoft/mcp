// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.ResourceHealth.Options;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResourceHealth.Commands;

public abstract class BaseResourceHealthCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseResourceHealthOptions, new()
{
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ResourceHealthRequestFailedException { StatusCode: HttpStatusCode.Conflict } requestFailedEx =>
            $"Azure Resource Health returned Conflict. The subscription may need the Microsoft.ResourceHealth provider registered, or the provider may still be registering. Details: {requestFailedEx.ErrorMessage ?? requestFailedEx.Message}",
        ResourceHealthRequestFailedException requestFailedEx =>
            $"Azure Resource Health request failed with status {(int)requestFailedEx.StatusCode} ({requestFailedEx.StatusCode}). Error code: {requestFailedEx.ErrorCode ?? requestFailedEx.StatusCode.ToString()}. Details: {requestFailedEx.ErrorMessage ?? requestFailedEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ResourceHealthRequestFailedException requestFailedEx => requestFailedEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };
}
