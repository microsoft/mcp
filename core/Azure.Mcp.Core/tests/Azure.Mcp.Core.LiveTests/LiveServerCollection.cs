using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Core.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
