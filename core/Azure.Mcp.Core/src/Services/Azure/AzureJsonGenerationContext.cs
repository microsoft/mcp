using System.Text.Json.Serialization;

namespace Azure.Mcp;

[JsonSerializable(typeof(AzureCredentials))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AzureJsonGenerationContext : JsonSerializerContext
{
}
