using System.Reflection;
using Xunit;

namespace Azure.Mcp.Tests.Client.Helpers
{
    /// <summary>
    /// xUnit fixture that runs once per test class (or collection if used via [CollectionDefinition]).
    /// Provides optional access to a shared TestProxy via Proxy property if tests need it later.
    /// </summary>
    public sealed class TestProxyFixture : IAsyncLifetime
    {
        public static string DetermineRepositoryRoot()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(path))
            {
                // we look for both directory and file because depending on user git config the .git may be a file instead of a directory
                if (Directory.Exists(Path.Combine(path, ".git")) || File.Exists(Path.Combine(path, ".git")))
                    return path;
                var parent = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(parent) || parent == path)
                    break;
                path = parent;
            }
            return Environment.CurrentDirectory;
        }

        /// <summary>
        /// Proxy instance created lazily. RecordedCommandTestsBase will start it after determining TestMode from LiveTestSettings.
        /// </summary>
        public TestProxy? Proxy { get; private set; }

        public ValueTask InitializeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public async Task StartProxyAsync(string? assetsJsonPath = null)
        {
            var root = DetermineRepositoryRoot();
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
