// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Core.Exceptions;

/// <summary>
/// Represents a structured validation failure during command handling.
/// Prefer throwing this instead of relying on parser error message text.
/// </summary>
public class CommandValidationException : Exception
{
    public CommandValidationException(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string? code = null,
        IReadOnlyList<string>? missingOptions = null) : base(message)
    {
        StatusCode = statusCode;
        Code = code ?? "ValidationError";
        MissingOptions = missingOptions;
    }

    /// <summary>
    /// HTTP status code to return in the response. Defaults to InternalServerError (500).
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Optional machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Optional list of missing option tokens (e.g., --resource-group).
    /// </summary>
    public IReadOnlyList<string>? MissingOptions { get; }
}
