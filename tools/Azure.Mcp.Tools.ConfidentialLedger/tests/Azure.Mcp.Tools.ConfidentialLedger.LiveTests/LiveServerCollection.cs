using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
