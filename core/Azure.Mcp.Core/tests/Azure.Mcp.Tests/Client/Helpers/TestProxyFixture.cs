using System;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers
{
    /// <summary>
    /// xUnit fixture that runs once per test class (or collection if used via [CollectionDefinition]).
    /// Provides optional access to a shared TestProxy via Proxy property if tests need it later.
    /// </summary>
    public class TestProxyFixture(IRecordingPathResolver? pathResolver = null) : IAsyncLifetime
    {
        public IRecordingPathResolver PathResolver { get; } = pathResolver ?? new RecordingPathResolver();

        /// <summary>
        /// Proxy instance created lazily. RecordedCommandTestsBase will start it after determining TestMode from LiveTestSettings.
        /// </summary>
        public TestProxy? Proxy { get; private set; }

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public async Task StartProxyAsync(string assetsJsonPath)
        {
            var root = PathResolver.RepositoryRoot;
            var proxy = new TestProxy();
            await proxy.Start(root, assetsJsonPath);
            Proxy = proxy;
        }

        public ValueTask DisposeAsync()
        {
            if (Proxy is not null)
            {
                Proxy.Dispose();
            }
            return ValueTask.CompletedTask;
        }

        public Uri? GetProxyUri()
        {
            if (Proxy?.BaseUri is string proxyUrl && Uri.TryCreate(proxyUrl, UriKind.Absolute, out var proxyUri))
            {
                return proxyUri;
            }

            return null;
        }
    }
}
