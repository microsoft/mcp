using System;
using System.Threading;

namespace Azure.Mcp.Tests.Client.Helpers
{
    /// <summary>
    /// Singleton manager for the test proxy instance. Ensures all tests share a single TestProxy,
    /// lazily started on demand with semaphore coordination for thread-safe initialization.
    /// </summary>
    public sealed class TestProxyManager
    {
        private static readonly Lazy<TestProxyManager> _instance = new(() => new TestProxyManager());
        private readonly SemaphoreSlim _startLock = new(1, 1);

        public IRecordingPathResolver PathResolver { get; private set; } = new RecordingPathResolver();

        /// <summary>
        /// Proxy instance created lazily on first access. Will be started when StartProxyAsync is called.
        /// </summary>
        public TestProxy? Proxy { get; private set; }

        private TestProxyManager()
        {
        }

        public static TestProxyManager GetInstance() => _instance.Value;

        public async Task StartProxyAsync(string assetsJsonPath)
        {
            if (Proxy != null)
            {
                return;
            }

            await _startLock.WaitAsync();
            try
            {
                if (Proxy != null)
                {
                    return;
                }

                var root = PathResolver.RepositoryRoot;
                var proxy = new TestProxy();
                await proxy.Start(root, assetsJsonPath);
                Proxy = proxy;
            }
            finally
            {
                _startLock.Release();
            }
        }

        public void Dispose()
        {
            if (Proxy is not null)
            {
                Proxy.Dispose();
                Proxy = null;
            }
        }

        /// <summary>
        /// Configures a custom path resolver for this manager instance.
        /// This should be called before proxy initialization.
        /// </summary>
        /// <param name="pathResolver">The path resolver to use</param>
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
