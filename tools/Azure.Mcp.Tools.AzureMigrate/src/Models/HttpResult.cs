// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// The raw outcome of an HTTP request. The response is returned rather than thrown on, so callers can
/// map individual status codes onto domain-specific guidance.
/// </summary>
/// <param name="StatusCode">The response status code.</param>
/// <param name="Body">The response body, or an empty string when there was none.</param>
public sealed record HttpResult(HttpStatusCode StatusCode, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the request succeeded.
    /// </summary>
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;
}
