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
}
