// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Providers instances of <see cref="TokenCredential"/> appropriate for the current environment.
/// Implementations are expected to be of <see cref="ServiceLifetime.Singleton"/>, however, in
/// multi-user environments using on-behalf-of downstream authentication, the implementation
/// must return credentials within the context of the user in the current execution context.
/// </summary>
/// <remarks>
/// <para>
/// <b>Callers</b> can either directly depend on this interface or indirectly depend on it through
/// <see cref="ITenantService"/>.
/// </para>
/// <para>
/// <b>Implementors</b> of this interface are responsible for generating, caching, and retrieving tokens
/// that can be used for authentication or authorization purposes. The specific type of the
/// <see cref="TokenCredential"/> is opaque to the caller of this interface as it will vary based
/// on the environment and configured authentication.
/// </para>
/// </remarks>
public interface IAzureTokenCredentialProvider
{
    /// <summary>
    /// Gets an instance of <see cref="TokenCredential"/> applicable for the current environment
    /// and downstream authentication configuration.
    /// </summary>
    /// <param name="tenantId">An optional tenant ID to use for the token request. Use of this may
    /// cause <see cref="InvalidOperationException"/> to be thrown. See the exceptions section for
    /// details.
    /// </param>
    /// <param name="cancellation">A cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a value of <see cref="TokenCredential"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException"
    /// >Thrown when the operation has been cancelled.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a credential cannot be provided. This can happen for reasons that vary based on the
    /// underlying implementation and authentication configuration of the environment. The
    /// <see cref="Exception.Message"/> of the <see cref="InvalidOperationException"/> should include
    /// additional details. For example, in multi-user environments using on-behalf-of will
    /// throw if a non-<see langword="null"/> <paramref name="tenantId"/> is provided that does
    /// not match the tenant of the authenticated user in the current execution context.
    /// </exception>
    Task<TokenCredential> GetTokenCredentialAsync(
        string? tenantId,
        CancellationToken cancellation);
}
