using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.AzureIsv.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
