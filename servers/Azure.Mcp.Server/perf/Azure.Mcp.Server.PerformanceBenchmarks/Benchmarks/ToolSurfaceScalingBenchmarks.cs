// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using BenchmarkDotNet.Attributes;
using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Server.Perf.Benchmarks;

/// <summary>
/// Measures how tool-discovery cost scales as the exposed tool catalog grows, at current
/// and projected tool counts (issue #3118: "Measure tool and namespace discovery at
/// current and projected tool counts" / "Track scaling behavior as the tool catalog grows").
///
/// For each <see cref="ToolCount"/> the benchmark builds a synthetic tool catalog whose
/// per-tool shape (name + description + a small typed JSON input schema) approximates a real
/// Azure MCP tool, then times the JSON serialization of the whole catalog — the dominant,
/// transport-independent cost of answering a tools/list request. Comparing the mean time per
/// tool across counts yields the discovery scaling-efficiency metric: if serialization is
/// linear, mean/ToolCount stays roughly constant as the catalog grows.
///
/// Run with:
///   dotnet run -c Release --project servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks -- --filter *ToolSurfaceScaling*
/// </summary>
[MemoryDiagnoser]
public class ToolSurfaceScalingBenchmarks
{
    // 71 ≈ current default surface, 510 ≈ current all-tools surface; the rest bracket
    // projected growth so the scaling curve is visible on both sides of today's counts.
    [Params(71, 250, 510, 1000)]
    public int ToolCount { get; set; }

    private Tool[] _tools = [];

    [GlobalSetup]
    public void Setup()
    {
        _tools = new Tool[ToolCount];
        for (var i = 0; i < ToolCount; i++)
        {
            _tools[i] = CreateSyntheticTool(i);
        }
    }

    [Benchmark(Description = "SerializeToolCatalog – serialize the full tools/list payload")]
    public string SerializeToolCatalog() => JsonSerializer.Serialize(_tools);

    /// <summary>
    /// Builds a single synthetic tool whose name, description, and input schema are sized to
    /// resemble a typical Azure MCP tool so per-tool serialization cost is representative.
    /// </summary>
    private static Tool CreateSyntheticTool(int index)
    {
        var inputSchema = JsonDocument.Parse($$"""
            {
              "type": "object",
              "properties": {
                "subscription": { "type": "string", "description": "The Azure subscription id or name for tool {{index}}." },
                "resourceGroup": { "type": "string", "description": "The resource group name." },
                "name": { "type": "string", "description": "The target resource name." },
                "limit": { "type": "integer", "description": "Maximum number of results to return." }
              },
              "required": ["subscription"]
            }
            """).RootElement;

        return new Tool
        {
            Name = $"azmcp_service{index % 60}_resource_operation{index}",
            Description = $"Performs operation {index} against an Azure resource, returning a structured result. " +
                          "Use this tool when the user asks to inspect or manage the corresponding Azure resource.",
            InputSchema = inputSchema,
        };
    }
}
