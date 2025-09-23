// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Models;

namespace Azure.Mcp.Tools.AppService.Commands;

[JsonSerializable(typeof(DatabaseAddCommand.Result))]
[JsonSerializable(typeof(DatabaseConnectionInfo))]
public partial class AppServiceJsonContext : JsonSerializerContext;
