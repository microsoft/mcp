// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Token credential provider that uses Azure Managed Identity for authentication.
/// </summary>
public sealed class ManagedIdentityCredentialProvider : ITokenCredentialProvider
{
    private readonly ManagedIdentityCredential _managedIdentityCredential;

    /// <summary>
    /// Initializes a new instance of the ManagedIdentityCredentialProvider class.
    /// </summary>
    /// <param name="clientId">Optional client ID for user-assigned managed identity.</param>
    public ManagedIdentityCredentialProvider(string? clientId = null)
    {
        if (!string.IsNullOrEmpty(clientId))
        {
            _managedIdentityCredential = new ManagedIdentityCredential(clientId);
        }
        else
        {
            _managedIdentityCredential = new ManagedIdentityCredential();
        }
    }

    /// <summary>
    /// Creates a TokenCredential using the managed identity.
    /// </summary>
    /// <param name="tenant">Optional tenant ID (ignored for managed identity).</param>
    /// <returns>The ManagedIdentityCredential instance.</returns>
    public Task<TokenCredential> CreateAsync(string? tenant = null)
    {
        return Task.FromResult<TokenCredential>(_managedIdentityCredential);
    }

    /// <summary>
    /// Gets the current user identity.
    /// </summary>
    /// <returns>Always returns null for managed identity (single-user mode).</returns>
    public string? GetCurrentUserId()
    {
        return null;
    }
}
