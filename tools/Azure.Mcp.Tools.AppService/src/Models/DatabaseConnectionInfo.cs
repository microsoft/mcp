// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents a database connection configured for an Azure App Service.
/// </summary>
public class DatabaseConnectionInfo
{
    /// <summary>
    /// The type of database (e.g., "SqlServer", "MySQL", "PostgreSQL", "CosmosDB").
    /// </summary>
    [JsonPropertyName("databaseType")]
    public string DatabaseType { get; set; } = string.Empty;

    /// <summary>
    /// The server name or endpoint for the database.
    /// </summary>
    [JsonPropertyName("databaseServer")]
    public string DatabaseServer { get; set; } = string.Empty;

    /// <summary>
    /// The name of the database.
    /// </summary>
    [JsonPropertyName("databaseName")]
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// The connection string for the database.
    /// </summary>
    [JsonPropertyName("connectionString")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The name used to identify this connection string in App Service configuration.
    /// </summary>
    [JsonPropertyName("connectionStringName")]
    public string ConnectionStringName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the database connection has been successfully configured.
    /// </summary>
    [JsonPropertyName("isConfigured")]
    public bool IsConfigured { get; set; }

    /// <summary>
    /// The timestamp when the connection was configured.
    /// </summary>
    [JsonPropertyName("configuredAt")]
    public DateTime ConfiguredAt { get; set; }
}
