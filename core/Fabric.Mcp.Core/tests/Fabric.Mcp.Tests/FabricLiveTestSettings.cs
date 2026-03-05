using Microsoft.Mcp.Tests.Client.Helpers;

namespace Fabric.Mcp.Tests
{
    public class FabricLiveTestSettings : LiveTestSettingsBase
    {
        public override string ServerExecutableName => "fabmcp";
    }
}
