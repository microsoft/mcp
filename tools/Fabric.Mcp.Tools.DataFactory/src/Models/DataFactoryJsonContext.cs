// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using global::DataFactory.MCP.Models.Pipeline;

namespace Fabric.Mcp.Tools.DataFactory.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ListPipelinesCommandResult))]
[JsonSerializable(typeof(CreatePipelineCommandResult))]
[JsonSerializable(typeof(GetPipelineCommandResult))]
[JsonSerializable(typeof(RunPipelineCommandResult))]
[JsonSerializable(typeof(ListDataflowsCommandResult))]
[JsonSerializable(typeof(CreateDataflowCommandResult))]
[JsonSerializable(typeof(ExecuteQueryCommandResult))]
public partial class DataFactoryJsonContext : JsonSerializerContext
{
}

public sealed record ListPipelinesCommandResult(List<Pipeline> Pipelines, int TotalCount);
public sealed record CreatePipelineCommandResult(Pipeline Pipeline);
public sealed record GetPipelineCommandResult(Pipeline Pipeline);
public sealed record RunPipelineCommandResult(string? RunId);
public sealed record ListDataflowsCommandResult(List<global::DataFactory.MCP.Models.Dataflow.Dataflow> Dataflows, int TotalCount);
public sealed record CreateDataflowCommandResult(global::DataFactory.MCP.Models.Dataflow.Dataflow Dataflow);
public sealed record ExecuteQueryCommandResult(bool Success, object? Data, global::DataFactory.MCP.Models.Dataflow.Query.QueryResultSummary? Summary);
