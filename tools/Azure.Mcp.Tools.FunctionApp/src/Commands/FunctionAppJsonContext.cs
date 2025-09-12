// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.FunctionApp.Commands.FunctionApp;
using Azure.Mcp.Tools.FunctionApp.Models;

namespace Azure.Mcp.Tools.FunctionApp.Commands;

[JsonSerializable(typeof(FunctionAppGetCommand.FunctionAppGetCommandResult))]
[JsonSerializable(typeof(FunctionAppInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FunctionAppJsonContext : JsonSerializerContext;
