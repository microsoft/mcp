using System.Text.Json.Serialization;
using Azure.Mcp.Tools.MonitorInstrumentation.Detectors;
using Azure.Mcp.Tools.MonitorInstrumentation.Tools;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Models;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(GeneratorConfig))]
[JsonSerializable(typeof(InstrumentationData))]
[JsonSerializable(typeof(OrchestratorResponse))]
[JsonSerializable(typeof(BrownfieldFindings))]
[JsonSerializable(typeof(AnalysisTemplate))]
internal partial class OnboardingJsonContext : JsonSerializerContext;
