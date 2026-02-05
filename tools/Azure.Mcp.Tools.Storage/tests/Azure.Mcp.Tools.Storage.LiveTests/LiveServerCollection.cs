using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Storage.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
