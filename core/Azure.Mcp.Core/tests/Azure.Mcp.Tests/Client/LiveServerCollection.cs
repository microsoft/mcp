using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tests.Client
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
