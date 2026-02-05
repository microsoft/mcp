using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.VirtualDesktop.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
