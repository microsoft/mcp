// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Tenant;

/// <summary>
/// Provides operations for managing Azure tenants and tenant-scoped authentication.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the list of all available Azure tenants.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, with a list of <see cref="TenantResource"/>
    /// instances.
    /// </returns>
    Task<List<TenantResource>> GetTenants();

    /// <summary>
    /// Gets the tenant ID from either a tenant ID or tenant name.
    /// </summary>
    /// <param name="tenantIdOrName">The tenant ID or tenant name.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the tenant ID or <see langword="null"/>
    /// if not found.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a tenant with the specified name is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tenant has a <see langword="null"> TenantId.
    /// </exception>
    Task<string> GetTenantId(string tenantIdOrName);

    /// <summary>
    /// Gets the tenant ID by tenant name.
    /// </summary>
    /// <param name="tenantName">The tenant name.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the tenant ID or <see langword="null"/>
    /// if not found.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a tenant with the specified name is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tenant has a <see langword="null"> TenantId.
    /// </exception>
    Task<string> GetTenantIdByName(string tenantName);

    /// <summary>
    /// Gets the tenant name by tenant ID.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the tenant name or <see langword="null"/> if not found.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a tenant with the specified ID is not found.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tenant has a <see langword="null"> DisplayName.
    /// </exception>
    Task<string> GetTenantNameById(string tenantId);

    /// <summary>
    /// Determines whether the specified string is a valid tenant ID (GUID format).
    /// </summary>
    /// <param name="tenantId">The string to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the string is a valid tenant ID; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsTenantId(string tenantId);

    /// <summary>
    /// Gets an instance of <see cref="TokenCredential"/>.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID. Use <see langword="null"/> in most cases.</param>
    /// <param name="cancellation">A cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a value of <see cref="TokenCredential"/>.
    /// </returns>
    /// <remarks>
    /// Implementors of this method must use <see cref="IAzureTokenCredentialProvider"/> to obtain
    /// the token credential.
    /// </remarks>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation has been cancelled.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a credential cannot be provided.
    /// </exception>
    Task<TokenCredential> GetTokenCredentialAsync(
        string? tenantId,
        CancellationToken cancellationToken);
}
