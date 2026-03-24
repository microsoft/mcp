// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Tests.Helpers;

namespace Microsoft.Mcp.Tests.Client.Helpers;

public sealed class PlaybackAwareTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly Func<TestMode> _testModeAccessor;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILoggingTokenCredentialFactory _credentialFactory;
    private readonly TokenCredential _playbackCredential = new PlaybackTokenCredential();
    private readonly Lazy<IAzureTokenCredentialProvider> _liveProvider;

    public PlaybackAwareTokenCredentialProvider(
        Func<TestMode> testModeAccessor,
        ILoggerFactory loggerFactory,
        ILoggingTokenCredentialFactory credentialFactory)
    {
        ArgumentNullException.ThrowIfNull(testModeAccessor);
        _testModeAccessor = testModeAccessor;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _credentialFactory = credentialFactory ?? throw new ArgumentNullException(nameof(credentialFactory));
        _liveProvider = new Lazy<IAzureTokenCredentialProvider>(() => new SingleIdentityTokenCredentialProvider(_loggerFactory, _credentialFactory));
    }

    public Task<TokenCredential> GetTokenCredentialAsync(string? tenantId, CancellationToken cancellation)
    {
        if (_testModeAccessor() == TestMode.Playback)
        {
            return Task.FromResult<TokenCredential>(_playbackCredential);
        }

        return _liveProvider.Value.GetTokenCredentialAsync(tenantId, cancellation);
    }
}
