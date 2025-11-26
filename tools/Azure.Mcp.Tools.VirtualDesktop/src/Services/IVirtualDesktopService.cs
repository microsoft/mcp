// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.VirtualDesktop.Models;

namespace Azure.Mcp.Tools.VirtualDesktop.Services;

using Azure.Mcp.Core.Options;

public interface IVirtualDesktopService
{
    Task<IReadOnlyList<HostPool>> ListHostpoolsAsync(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HostPool>> ListHostpoolsByResourceGroupAsync(
        string subscription,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionHost>> ListSessionHostsAsync(
        string subscription,
        string hostPoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionHost>> ListSessionHostsByResourceIdAsync(
        string subscription,
        string hostPoolResourceId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionHost>> ListSessionHostsByResourceGroupAsync(
        string subscription,
        string resourceGroup,
        string hostPoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSession>> ListUserSessionsAsync(
        string subscription,
        string hostPoolName,
        string sessionHostName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSession>> ListUserSessionsByResourceIdAsync(
        string subscription,
        string hostPoolResourceId,
        string sessionHostName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSession>> ListUserSessionsByResourceGroupAsync(
        string subscription,
        string resourceGroup,
        string hostPoolName,
        string sessionHostName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
