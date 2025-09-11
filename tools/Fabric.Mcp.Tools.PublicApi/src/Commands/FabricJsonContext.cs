// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Models;

namespace Fabric.Mcp.Tools.PublicApi.Commands;


[JsonSerializable(typeof(FabricWorkloadPublicApi))]
[JsonSerializable(typeof(ListWorkloadsCommand.ItemListCommandResult))]
[JsonSerializable(typeof(GetExamplesCommand.ExampleFileResult))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(IEnumerable<string>))]
public partial class FabricJsonContext : JsonSerializerContext
{
}
