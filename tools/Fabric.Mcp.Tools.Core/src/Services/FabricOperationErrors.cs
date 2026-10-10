// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Identity;
using Fabric.Mcp.Tools.Core.Models;
using Microsoft.Identity.Client;

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricOperationErrors
{
    internal static HttpStatusCode GetStatusCode(Exception exception) => exception switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        MsalServiceException service => (HttpStatusCode)service.StatusCode,
        MsalClientException => HttpStatusCode.Unauthorized,
        RequestFailedException failed => (HttpStatusCode)failed.Status,
        HttpRequestException http => http.StatusCode ?? HttpStatusCode.ServiceUnavailable,
        OperationCanceledException => HttpStatusCode.RequestTimeout,
        TimeoutException => HttpStatusCode.GatewayTimeout,
        InvalidOperationException => HttpStatusCode.UnprocessableEntity,
        _ => HttpStatusCode.InternalServerError
    };

    internal static string GetMessage(Exception exception) => exception switch
    {
        FabricOperationException { StatusCode: HttpStatusCode.BadGateway } or FabricThrottledException => exception.Message,
        _ => GetStatusCode(exception) switch
        {
            HttpStatusCode.BadRequest => "Provide a nonempty operation UUID",
            HttpStatusCode.Unauthorized => "Authentication failed. Use the identity authorized for the initiating Fabric API",
            HttpStatusCode.Forbidden => "Access denied. Reading an operation requires the same resource permissions and delegated scopes as the initiating Fabric API",
            HttpStatusCode.NotFound => "The Fabric operation or result was not found, or is not accessible. This does not prove successful completion without a result",
            HttpStatusCode.TooManyRequests => "Fabric throttled the operation read. Wait before retrying the read",
            HttpStatusCode.RequestTimeout => "The operation read was canceled. The Fabric operation was not canceled",
            HttpStatusCode.GatewayTimeout => "The operation read timed out. The Fabric operation was not canceled",
            _ => "Unable to read the Fabric operation. Check service availability and resume using the operation ID; do not resubmit the original mutation"
        }
    };

    internal static FabricOperationIssue ToIssue(Exception exception) =>
        new(exception is FabricOperationException operation ? operation.Code : "ReadFailed",
            GetMessage(exception), (int)GetStatusCode(exception));
}
