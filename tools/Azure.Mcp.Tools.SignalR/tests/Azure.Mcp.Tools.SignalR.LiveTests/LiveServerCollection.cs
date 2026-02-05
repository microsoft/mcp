using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
