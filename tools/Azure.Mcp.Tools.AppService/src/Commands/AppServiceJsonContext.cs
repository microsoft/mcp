// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AppService.Commands.Database;
using Azure.Mcp.Tools.AppService.Commands.Webapps;
using Azure.Mcp.Tools.AppService.Models;

namespace Azure.Mcp.Tools.AppService.Commands;

[JsonSerializable(typeof(DatabaseAddCommand.DatabaseAddResult))]
[JsonSerializable(typeof(DatabaseConnectionInfo))]
[JsonSerializable(typeof(WebappDetails))]
[JsonSerializable(typeof(WebappsGetCommand.WebappsGetResult))]
public partial class AppServiceJsonContext : JsonSerializerContext;
