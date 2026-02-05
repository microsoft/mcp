using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
