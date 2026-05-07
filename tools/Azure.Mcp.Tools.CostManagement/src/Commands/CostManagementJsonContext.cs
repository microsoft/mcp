// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.CostManagement.Commands.Query;
using Azure.Mcp.Tools.CostManagement.Models;

namespace Azure.Mcp.Tools.CostManagement.Commands;

[JsonSerializable(typeof(QueryRunCommand.QueryRunCommandResult))]
[JsonSerializable(typeof(CostQueryResult))]
[JsonSerializable(typeof(CostQueryRow))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class CostManagementJsonContext : JsonSerializerContext;
