using System;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers
{
    /// <summary>
    /// xUnit fixture that runs once per test class (or collection if used via [CollectionDefinition]).
    /// Provides optional access to a shared TestProxy via Proxy property if tests need it later.
    /// </summary>
    public class TestProxyFixture : IAsyncLifetime
    {
        public IRecordingPathResolver PathResolver { get; private set; } = new RecordingPathResolver();

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

        /// <summary>
        /// XUnit class fixtures are created via parameterless constructor, so this method allows configuring a custom path resolver after construction.
        /// This is necessary if we want to atomically resolve paths in a different way than the default RecordingPathResolver. Unfortunately due to limitations
        /// with xunit classfixture instantiation we cannot pass parameters to the constructor, EVEN IF they are nullable and have a default.
        /// </summary>
        /// <param name="pathResolver"></param>
        public void ConfigurePathResolver(IRecordingPathResolver pathResolver)
        {
            PathResolver = pathResolver;
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
