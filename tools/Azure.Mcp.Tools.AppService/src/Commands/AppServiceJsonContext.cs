// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AzureMcp.AppService.Commands.Database;
using AzureMcp.AppService.Models;

namespace AzureMcp.AppService.Commands;

[JsonSerializable(typeof(DatabaseAddCommand.Result))]
[JsonSerializable(typeof(DatabaseConnectionInfo))]
public partial class AppServiceJsonContext : JsonSerializerContext
{
    public static JsonTypeInfo<DatabaseAddCommand.Result> DatabaseAddCommandResult => Default.Result;
}
