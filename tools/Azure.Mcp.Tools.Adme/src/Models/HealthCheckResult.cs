// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Adme.Models;

/// <summary>
/// Represents the outcome of ADME authentication and connectivity checks.
/// </summary>
public sealed record HealthCheckResult(
    bool AuthOk,
    string? AuthError,
    bool ConnectivityOk,
    string? ConnectivityError,
    int? ConnectivityStatusCode);
