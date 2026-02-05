using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.StorageSync.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
