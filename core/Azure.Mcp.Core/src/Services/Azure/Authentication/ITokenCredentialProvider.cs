// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Provider to obtain TokenCredential instances based on <see cref="ServerConfiguration"/>.
/// </summary>
public interface ITokenCredentialProvider
{
    /// <summary>
    /// Special singleton instance that signals to use <see cref="CustomChainedCredential"/>.
    /// </summary>
    public static readonly ITokenCredentialProvider Default = new DefaultTokenCredentialProvider();

    /// <summary>
    /// Creates a TokenCredential based on <see cref="ServerConfiguration"/>.
    /// </summary>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <returns>TokenCredential for the current user and tenant</returns>
    Task<TokenCredential> CreateAsync(string? tenant = null);

    /// <summary>
    /// Gets the current user identity (e.g., for cache isolation).
    /// Returns null for single-user scenarios.
    /// </summary>
    /// <returns>User identity string, or null for single-user mode</returns>
    string? GetCurrentUserId();
}

/// <summary>
/// Internal marker implementation that represents "use <see cref="CustomChainedCredential"/>".
/// </summary>
internal sealed class DefaultTokenCredentialProvider : ITokenCredentialProvider
{
    public Task<TokenCredential> CreateAsync(string? tenant = null)
    {
        throw new InvalidOperationException(
            "DefaultTokenCredentialProvider is a marker instance for BaseAzureService to use CustomChainedCredential");
    }

    public string? GetCurrentUserId() => null; // Single-user mode
}
