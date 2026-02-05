using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
