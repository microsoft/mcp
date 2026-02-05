using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Grafana.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
