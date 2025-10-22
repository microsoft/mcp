using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Mcp.Tests.Helpers
{

    /// <summary>
    /// Execution mode for tests integrating with the test proxy.
    /// </summary>
    public enum TestMode
    {
        Live,
        Record,
        Playback
    }

    public static class TestEnvironment
    {
        public static bool IsRunningInCi =>
            string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD"));

        public static TestMode GetEnvironmentTestMode()
        {
            var mode = Environment.GetEnvironmentVariable("AZURE_RECORD_MODE");
            if (string.IsNullOrWhiteSpace(mode))
            {
                return TestMode.Live;
            }
            return Enum.TryParse<TestMode>(mode, ignoreCase: true, out var parsed) ? parsed : TestMode.Live;
        }


    }
}
