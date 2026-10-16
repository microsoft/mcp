// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Carries the caller's requested tenant from RegistryToolLoader to AccessTokenHandler. The MCP
/// client API has no per-request hook, so an AsyncLocal is the available channel.
/// </summary>
public static class RegistryTenantContext
{
    private static readonly AsyncLocal<string?> s_currentTenantId = new();

    /// <summary>
    /// Tenant to mint registry server tokens for, or null to use the hosting identity's own tenant.
    /// </summary>
    public static string? CurrentTenantId
    {
        get => s_currentTenantId.Value;
        set => s_currentTenantId.Value = value;
    }
}
