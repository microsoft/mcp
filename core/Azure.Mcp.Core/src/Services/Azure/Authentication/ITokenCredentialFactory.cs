// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Factory for creating TokenCredential instances based on the current execution context.
/// Implementations can provide different strategies for single-user vs multi-user scenarios.
/// </summary>
public interface ITokenCredentialFactory
{
    /// <summary>
    /// Creates a TokenCredential for the current execution context.
    /// In multi-user scenarios, uses current HTTP request context.
    /// In single-user scenarios, uses default credential chain.
    /// </summary>
    /// <param name="tenant">Optional tenant ID for multi-tenant scenarios</param>
    /// <returns>TokenCredential for the current user and tenant</returns>
    Task<TokenCredential> CreateAsync(string? tenant = null);

    /// <summary>
    /// Gets the current user identity for cache isolation.
    /// Returns null for single-user scenarios.
    /// </summary>
    /// <returns>User identity string, or null for single-user mode</returns>
    string? GetCurrentUserId();
}
