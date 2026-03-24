// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Default implementation of <see cref="ILoggingTokenCredentialFactory"/> that wraps
/// credentials with <see cref="LoggingTokenCredential"/> when the logging category
/// for <see cref="LoggingTokenCredential"/> is enabled at <see cref="LogLevel.Information"/>.
/// </summary>
public sealed class LoggingTokenCredentialFactory(ILogger<LoggingTokenCredential> logger) : ILoggingTokenCredentialFactory
{
    private readonly ILogger<LoggingTokenCredential> _logger = logger;

    /// <inheritdoc/>
    public TokenCredential WrapIfEnabled(TokenCredential credential, string? tenantId)
    {
        return _logger.IsEnabled(LogLevel.Information)
            ? new LoggingTokenCredential(credential, tenantId, _logger)
            : credential;
    }
}
