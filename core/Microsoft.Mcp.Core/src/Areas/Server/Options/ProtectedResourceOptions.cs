// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Configurable OAuth protected resource metadata values.
/// </summary>
public sealed class ProtectedResourceOptions
{
    /// <summary>
    /// Gets or sets the canonical protected resource identifier.
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Gets or sets authorization server issuer identifiers.
    /// </summary>
    public string[] AuthorizationServers { get; set; } = [];

    /// <summary>
    /// Gets or sets the delegated scopes advertised by the protected resource.
    /// </summary>
    public string[] ScopesSupported { get; set; } = [];
}
