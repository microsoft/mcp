using System.Reflection;
using Xunit.v3;

namespace Azure.Mcp.Tests.Helpers
{
    /// <summary>
    /// Xunit attribute to clear known environment variables before each test is run.
    /// Live tests should not use this attribute, as they may need environment variables to configure authentication and proxy.
    /// </summary>
    public class ClearEnvironmentVariablesBeforeTestAttribute : BeforeAfterTestAttribute
    {
        // These are all the known environment variables that our server may use.
        // Proper test initialization should clear all of these, then set only the ones needed for the test.
        private static readonly List<string> _variablesToClear = [
            "ALL_PROXY",
            "ALLOW_INSECURE_EXTERNAL_BINDING",
            "APPLICATIONINSIGHTS_CONNECTION_STRING",
            "ASPNETCORE_URLS",
            "AZURE_CLIENT_ID",
            "AZURE_CREDENTIALS",
            "AZURE_MCP_AUTHENTICATION_RECORD",
            "AZURE_MCP_BROWSER_AUTH_TIMEOUT_SECONDS",
            "AZURE_MCP_CLIENT_ID",
            "AZURE_MCP_COLLECT_TELEMETRY",
            "AZURE_MCP_ENABLE_OTLP_EXPORTER",
            "AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL",
            "AZURE_SUBSCRIPTION_ID",
            "AZURE_TOKEN_CREDENTIALS",
            "HTTP_PROXY",
            "HTTPS_PROXY",
            "NO_PROXY",
        ];

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            foreach (var envVar in _variablesToClear)
            {
                Environment.SetEnvironmentVariable(envVar, null);
            }
        }
    }
}
