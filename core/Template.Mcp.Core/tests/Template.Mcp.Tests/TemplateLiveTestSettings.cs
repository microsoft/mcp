using Azure.Mcp.Tests.Client.Helpers;

namespace Template.Mcp.Tests
{
    public class TemplateLiveTestSettings : LiveTestSettingsBase
    {
        public override string ServerExecutableName => "mcptmp";
    }
}
