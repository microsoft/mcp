// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.DocumentDb.Models;

/// <summary>
/// Represents a unified response structure for all DocumentDb MCP commands.
/// </summary>
public class DocumentDbResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the operation.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the message (error or informational) from the operation.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the response data payload from the operation.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Creates a DocumentDbResponse from a dictionary returned by the service.
    /// </summary>
    /// <param name="result">The service result dictionary.</param>
    /// <returns>A DocumentDbResponse instance or null if conversion fails.</returns>
    public static DocumentDbResponse? FromDictionary(object? result)
    {
        if (result is not Dictionary<string, object?> dict)
        {
            return null;
        }

        var success = dict.TryGetValue("success", out var successObj) && (bool)successObj!;
        var statusCode = dict.TryGetValue("statusCode", out var statusCodeObj)
            ? (HttpStatusCode)statusCodeObj!
            : (success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);

        return new DocumentDbResponse
        {
            Success = success,
            StatusCode = statusCode,
            Message = dict.TryGetValue("message", out var messageObj) ? messageObj?.ToString() : null,
            Data = dict.TryGetValue("data", out var dataObj) ? dataObj : null
        };
    }
}
