using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Redis.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
