// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Database;

public enum DatabaseConnectionType
{
    [JsonStringEnumMemberName("SqlServer")]
    SqlServer,

    [JsonStringEnumMemberName("MySQL")]
    MySQL,

    [JsonStringEnumMemberName("PostgreSQL")]
    PostgreSQL,

    [JsonStringEnumMemberName("CosmosDB")]
    CosmosDB
}
