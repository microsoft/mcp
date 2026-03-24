// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Factory for creating <see cref="TokenCredential"/> instances that optionally wrap
/// with logging functionality.
/// </summary>
public interface ILoggingTokenCredentialFactory
{
    /// <summary>
    /// Wraps the given credential with logging if logging is enabled.
    /// </summary>
    /// <param name="credential">The credential to potentially wrap.</param>
    /// <param name="tenantId">The tenant ID for logging context.</param>
    /// <returns>The original credential if logging is disabled, or a wrapped credential if enabled.</returns>
    TokenCredential WrapIfEnabled(TokenCredential credential, string? tenantId);
}
