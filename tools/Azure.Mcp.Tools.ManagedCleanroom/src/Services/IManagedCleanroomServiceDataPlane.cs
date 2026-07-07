// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedCleanroom.Services;

/// <summary>
/// Data-plane operations against the Cleanroom Analytics Frontend service.
/// Authentication uses a bearer token scoped to the frontend endpoint.
/// </summary>
public interface IManagedCleanroomServiceDataPlane
{
    Task<JsonElement> ListCollaborationsAsync(
        string endpoint,
        bool? activeOnly = null,
    bool allowUntrustedCert = false,
        string? tokenScope = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
