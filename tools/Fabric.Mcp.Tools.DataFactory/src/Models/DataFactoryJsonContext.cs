// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.DataFactory.Commands;

namespace Fabric.Mcp.Tools.DataFactory.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceListCommandResult))]
[JsonSerializable(typeof(WorkspaceListCommand.WorkspaceInfo))]
[JsonSerializable(typeof(PipelineListCommand.PipelineListCommandResult))]
[JsonSerializable(typeof(PipelineListCommand.PipelineInfo))]
[JsonSerializable(typeof(PipelineCreateCommand.PipelineCreateCommandResult))]
[JsonSerializable(typeof(PipelineGetCommand.PipelineGetCommandResult))]
[JsonSerializable(typeof(PipelineRunCommand.PipelineRunCommandResult))]
[JsonSerializable(typeof(List<WorkspaceListCommand.WorkspaceInfo>))]
[JsonSerializable(typeof(List<PipelineListCommand.PipelineInfo>))]
internal partial class DataFactoryJsonContext : JsonSerializerContext;
