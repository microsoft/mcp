using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Speech.LiveTests
{
    [CollectionDefinition("LiveServer")]
    public sealed class LiveServerCollection : ICollectionFixture<LiveServerFixture>
    {
    }
}
