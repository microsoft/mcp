// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Pagination;

/// <summary>
/// Thrown when a cursor cannot be loaded or fails validation.
/// All external-facing cursor errors (not found, fingerprint mismatch,
/// unsupported version) are surfaced as this type. The <see cref="Reason"/>
/// property provides the internal diagnostic reason.
/// </summary>
public sealed class InvalidCursorException : Exception
{
    /// <summary>
    /// The internal reason for the cursor failure, useful for logging and diagnostics.
    /// </summary>
    public InvalidCursorReason Reason { get; }

    public InvalidCursorException(InvalidCursorReason reason, string message)
        : base(message)
    {
        Reason = reason;
    }

    public InvalidCursorException(InvalidCursorReason reason, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }
}
