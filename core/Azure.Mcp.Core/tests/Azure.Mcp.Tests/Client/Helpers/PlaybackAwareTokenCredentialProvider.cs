// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tests.Client.Helpers;

public sealed class PlaybackAwareTokenCredentialProvider : IAzureTokenCredentialProvider
{
    private readonly Func<TestMode> _testModeAccessor;
    private readonly ILoggerFactory _loggerFactory;
    private readonly TokenCredential _playbackCredential = new PlaybackTokenCredential();
    private readonly Lazy<IAzureTokenCredentialProvider> _liveProvider;

    public PlaybackAwareTokenCredentialProvider(Func<TestMode> testModeAccessor, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(testModeAccessor);
        _testModeAccessor = testModeAccessor;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _liveProvider = new Lazy<IAzureTokenCredentialProvider>(() => new SingleIdentityTokenCredentialProvider(_loggerFactory));
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
