using System.Text.Json.Serialization;
using AzureMcp.Ai.Models;

namespace AzureMcp.Ai.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(CompletionsCreateResult))]
public sealed partial class AiJsonContext : JsonSerializerContext;