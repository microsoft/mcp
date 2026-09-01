// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

public enum DatabaseType
{
    SqlServer,

    [JsonStringEnumMemberName("MySQL")]
    MySql,

    [JsonStringEnumMemberName("PostgreSQL")]
    PostgreSql,

    [JsonStringEnumMemberName("CosmosDB")]
    CosmosDb,
}
