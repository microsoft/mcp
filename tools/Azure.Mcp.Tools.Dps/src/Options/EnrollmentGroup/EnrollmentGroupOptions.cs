
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Dps.Options.EnrollmentGroup
{
    internal class EnrollmentGroupOptions : GlobalOptions
    {
        [JsonPropertyName(DpsOptionDefinitions.HostnameName)]
        public string DpsHostName { get; set; } = string.Empty;

        [JsonPropertyName(DpsOptionDefinitions.EnrollmentGroupIdName)]
        public string EnrollmentGroupId { get; set; } = string.Empty;
    }
}
