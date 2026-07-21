// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.OneLake.Commands;

internal static class OneLakeCommandValidators
{
    internal static string GetErrorMessage(Exception ex, Func<Exception, string> baseGetErrorMessage) => ex switch
    {
        ArgumentException argEx => $"Invalid argument: {argEx.Message}",
        InvalidOperationException opEx => $"Operation failed: {opEx.Message}",
        HttpRequestException httpEx => $"HTTP request failed: {httpEx.Message}",
        _ => baseGetErrorMessage.Invoke(ex)
    };

    internal static HttpStatusCode GetStatusCode(Exception ex, Func<Exception, HttpStatusCode> baseGetStatusCode) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        InvalidOperationException => HttpStatusCode.InternalServerError,
        HttpRequestException httpEx when httpEx.Message.Contains("404") => HttpStatusCode.NotFound,
        HttpRequestException httpEx when httpEx.Message.Contains("403") => HttpStatusCode.Forbidden,
        HttpRequestException httpEx when httpEx.Message.Contains("401") => HttpStatusCode.Unauthorized,
        _ => baseGetStatusCode.Invoke(ex)
    };
}
