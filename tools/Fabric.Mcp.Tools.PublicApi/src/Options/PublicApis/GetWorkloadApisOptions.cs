using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.PublicApi.Options.PublicApis
{
    public class GetWorkloadApisOptions : BaseFabricOptions
    {
        [JsonPropertyName(FabricOptionDefinitions.WorkloadTypeName)]
        public string? WorkloadType { get; set; }
    }
}
