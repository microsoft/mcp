// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Tenant;

public interface ITenantService
{
    Task<List<TenantResource>> GetTenants();
    Task<string?> GetTenantId(string tenantIdOrName);
    Task<string?> GetTenantIdByName(string tenantName);
    Task<string?> GetTenantNameById(string tenantId);
    bool IsTenantId(string tenantId);

    /// <summary>
    /// Gets an instance of <see cref="TokenCredential"/>.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="cancellation">A cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with a value of <see cref="TokenCredential"/>.
    /// </returns>
    /// <remarks>
    /// Implementors of this method must use <see cref="IAzureTokenCredentialProvider"/> to obtain the token credential.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation has been cancelled.</exception>
    Task<TokenCredential> GetTokenAsync(
        string? tenantId,
        CancellationToken cancellationToken);
}
