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
[JsonSerializable(typeof(GetBestPracticesCommand.GetBestPracticesCommandResult))]
[JsonSerializable(typeof(GetWorkloadDefinitionCommand.GetWorkloadDefinitionCommandResult))]
[JsonSerializable(typeof(GetPlatformApisCommand.GetPlatformApisCommandResult))]
[JsonSerializable(typeof(GetWorkloadApisCommand.GetWorkloadApisCommandResult))]
public partial class FabricJsonContext : JsonSerializerContext
{
}
