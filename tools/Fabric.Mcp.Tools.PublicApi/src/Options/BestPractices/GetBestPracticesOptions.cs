using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.PublicApi.Options.BestPractices
{
    public class GetBestPracticesOptions : BaseFabricOptions
    {
        [JsonPropertyName(FabricOptionDefinitions.TopicName)]
        public string? Topic { get; set; }
    }
}
