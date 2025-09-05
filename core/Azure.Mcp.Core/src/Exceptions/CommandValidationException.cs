// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Exceptions;

/// <summary>
/// Represents a structured validation failure during command handling.
/// Prefer throwing this instead of relying on parser error message text.
/// </summary>
public class CommandValidationException : Exception
{
    public CommandValidationException(
        string message,
        int statusCode = 400,
        string? code = null,
        IReadOnlyList<string>? missingOptions = null) : base(message)
    {
        StatusCode = statusCode;
        Code = code ?? "ValidationError";
        MissingOptions = missingOptions;
    }

    /// <summary>
    /// HTTP-like status code to return in the response. Defaults to 400.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Optional machine-readable error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Optional list of missing option tokens (e.g., --resource-group).
    /// </summary>
    public IReadOnlyList<string>? MissingOptions { get; }
}
