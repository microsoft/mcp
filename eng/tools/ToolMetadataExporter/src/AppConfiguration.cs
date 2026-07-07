// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolMetadataExporter;

public class AppConfiguration
{
    /// <summary>
    /// URL of the Kusto ingestion endpoint. Endpoint is used for ingressing data to Kusto cluster.
    /// </summary>
    public string? IngestionEndpoint { get; set; }

    /// <summary>
    /// URL for the Kusto query endpoint. Endpoint is used for querying data from Kusto cluster.
    /// </summary>
    public string? QueryEndpoint { get; set; }

    /// <summary>
    /// Name of the Database in the Kusto cluster.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Name of the existing table in Kusto database where MCP tool events are stored.
    /// </summary>
    public string? McpToolEventsTableName { get; set; }

    /// <summary>
    /// Folder path where Kusto query files are stored. By default, it is "Resources/queries".
    /// Used to load file named <see cref="ToolMetadataExporter.Services.AzureMcpKustoDatastore.ExistingToolsKqlFileName"/>
    /// which fetches current MCP tools.
    /// </summary>
    public string? QueriesFolder { get; set; } = "Resources/queries";

    /// <summary>
    /// Directory to read and write files.
    /// </summary>
    public string? WorkDirectory { get; set; }

    /// <summary>
    /// true if the application should run in dry-run mode. In dry-run mode, no events are published to Kusto
    /// Changes are written locally to <see cref="AppConfiguration.WorkDirectory"/>.
    /// false to publish events to Kusto.
    /// </summary>
    public bool IsDryRun { get; set; }

    /// <summary>
    /// Path to the Azure MCP executable (azmcp.exe) to analyze.
    /// </summary>
    public string? AzmcpExe { get; set; }

    /// <summary>
    /// true to use the current time as the value for <see cref="Models.Kusto.McpToolEvent.EventTime"/>.
    /// false to use the <see cref="AzmcpExe"/>'s build date time as the event time.
    /// </summary>
    public bool UseAnalysisTime { get; set; }
}
