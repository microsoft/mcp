// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Represents a structured validation failure during command handling.
/// Prefer throwing this instead of relying on parser error message text.
/// </summary>
public class CommandValidationException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
    string? code = null,
    IReadOnlyList<string>? missingOptions = null) : Exception(message)
{

    /// <summary>
    /// HTTP status code to return in the response. Defaults to InternalServerError (500).
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>
    /// Optional machine-readable error code.
    /// </summary>
    public string Code { get; } = code ?? "ValidationError";

    /// <summary>
    /// Optional list of missing option tokens (e.g., --resource-group).
    /// </summary>
    public IReadOnlyList<string>? MissingOptions { get; } = missingOptions;
}
