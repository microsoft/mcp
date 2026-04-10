// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Fabric.Mcp.Tools.Docs.Commands.BestPractices;
using Fabric.Mcp.Tools.Docs.Commands.PublicApis;
using Fabric.Mcp.Tools.Docs.Models;

namespace Fabric.Mcp.Tools.Docs.Commands;


[JsonSerializable(typeof(FabricWorkloadPublicApi))]
[JsonSerializable(typeof(ListWorkloadsCommand.ItemListCommandResult))]
[JsonSerializable(typeof(GetExamplesCommand.ExampleFileResult))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(IEnumerable<string>))]
public partial class FabricJsonContext : JsonSerializerContext
{
}
