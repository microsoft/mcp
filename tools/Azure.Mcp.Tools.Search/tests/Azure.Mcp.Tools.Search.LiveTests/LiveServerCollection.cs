using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Search.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
