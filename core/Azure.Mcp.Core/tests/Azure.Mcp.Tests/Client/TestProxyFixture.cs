using Xunit;

namespace Azure.Mcp.Tests.Client
{
    /// <summary>
    /// xUnit fixture that runs once per test class (or collection if used via [CollectionDefinition]).
    /// Provides optional access to a shared TestProxy via Proxy property if tests need it later.
    /// </summary>
    public sealed class TestProxyFixture : IAsyncLifetime
    {
        /// <summary>
        /// Optional shared proxy instance (not started automatically here to keep side-effects minimal). Tests should set their local copy of Proxy to
        /// This fixture.Proxy in their constructor if they want recorded or playback tests.
        /// </summary>
        public TestProxy? Proxy { get; private set; }

        // xUnit v3 IAsyncLifetime uses ValueTask
        public ValueTask InitializeAsync()
        {
            // todo: start the proxy here.
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
