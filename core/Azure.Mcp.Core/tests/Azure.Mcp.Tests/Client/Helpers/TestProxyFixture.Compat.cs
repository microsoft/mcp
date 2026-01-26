using System;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers
{
    /// <summary>
    /// Backward compatibility wrapper for tests still using xUnit class fixtures.
    /// Bridges the old xUnit fixture pattern to the new TestProxyManager singleton.
    /// Tests should migrate away from using this in constructor parameters.
    /// </summary>
    public class TestProxyFixture : IAsyncLifetime
    {
        private readonly TestProxyManager _manager = TestProxyManager.GetInstance();

        public IRecordingPathResolver PathResolver => _manager.PathResolver;
        public TestProxy? Proxy => _manager.Proxy;

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public async Task StartProxyAsync(string assetsJsonPath)
        {
            await _manager.StartProxyAsync(assetsJsonPath);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public void ConfigurePathResolver(IRecordingPathResolver pathResolver)
        {
            _manager.ConfigurePathResolver(pathResolver);
        }

        public Uri? GetProxyUri()
        {
            return _manager.GetProxyUri();
        }
    }
}
