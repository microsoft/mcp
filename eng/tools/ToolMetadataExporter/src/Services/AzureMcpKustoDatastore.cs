// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using System.Runtime.CompilerServices;
using Kusto.Data.Common;
using Kusto.Data.Ingestion;
using Kusto.Ingest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Models.Kusto;

namespace ToolMetadataExporter.Services;

public class AzureMcpKustoDatastore : IAzureMcpDatastore
{
    internal const string ExistingToolsKqlFileName = "GetAvailableTools.kql";

    private readonly ICslQueryProvider _kustoClient;
    private readonly IKustoIngestClient _ingestClient;
    private readonly ILogger<AzureMcpKustoDatastore> _logger;
    private readonly DirectoryInfo _queriesDirectory;
    private readonly string _databaseName;
    private readonly string _tableName;

    public AzureMcpKustoDatastore(
        ICslQueryProvider kustoClient,
        IKustoIngestClient ingestClient,
        IOptions<AppConfiguration> configuration,
        ILogger<AzureMcpKustoDatastore> logger)
    {
        _kustoClient = kustoClient;
        _ingestClient = ingestClient;
        _logger = logger;

        _databaseName = configuration.Value.DatabaseName ?? throw new ArgumentNullException(nameof(AppConfiguration.DatabaseName));
        _tableName = configuration.Value.McpToolEventsTableName ?? throw new ArgumentNullException(nameof(AppConfiguration.McpToolEventsTableName));
        _queriesDirectory = configuration.Value.QueriesFolder == null
            ? throw new ArgumentNullException(nameof(configuration.Value.QueriesFolder))
            : new DirectoryInfo(configuration.Value.QueriesFolder);

        if (!_queriesDirectory.Exists)
        {
            throw new ArgumentException($"'{_queriesDirectory.FullName}' does not exist. Value: {configuration.Value.QueriesFolder}");
        }
    }

    public async Task<IList<AzureMcpTool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        var queryFile = _queriesDirectory.GetFiles(ExistingToolsKqlFileName).FirstOrDefault() ?? throw new InvalidOperationException($"Could not find {ExistingToolsKqlFileName} in {_queriesDirectory.FullName}");

        var results = new List<AzureMcpTool>();

        await foreach (var latestEvent in GetLatestToolEventsAsync(queryFile.FullName, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(latestEvent.ToolId))
            {
                throw new InvalidOperationException(
                    $"Cannot have an event with no id. Name: {latestEvent.ToolName}, Area: {latestEvent.ToolArea}");
            }

            string? toolName;
            string? toolArea;
            switch (latestEvent.EventType)
            {
                case McpToolEventType.Created:
                    toolName = latestEvent.ToolName;
                    toolArea = latestEvent.ToolArea;
                    break;
                case McpToolEventType.Updated:
                    toolName = latestEvent.ReplacedByToolName;
                    toolArea = latestEvent.ReplacedByToolArea;
                    break;
                default:
                    throw new InvalidOperationException($"Tool '{latestEvent.ToolId}' has unsupported event type: {latestEvent.EventType}");
            }

            if (string.IsNullOrEmpty(toolName) || string.IsNullOrEmpty(toolArea))
            {
                throw new InvalidOperationException($"Tool '{latestEvent.ToolId}' without tool name and/or a tool area.");
            }

            results.Add(new AzureMcpTool(latestEvent.ToolId, toolName, toolArea));
        }

        return results;
    }

    public async Task AddToolEventsAsync(List<McpToolEvent> toolEvents, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using MemoryStream stream = new();

        await JsonSerializer.SerializeAsync(stream, toolEvents, ModelsSerializationContext.Default.ListMcpToolEvent, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        cancellationToken.ThrowIfCancellationRequested();

        var ingestionProperties = new KustoIngestionProperties(_databaseName, _tableName)
        {
            Format = DataSourceFormat.singlejson,
            IngestionMapping = new IngestionMapping()
            {
                IngestionMappingKind = IngestionMappingKind.Json,
                IngestionMappings = McpToolEvent.GetColumnMappings()
            }
        };

        IKustoIngestionResult result = await _ingestClient.IngestFromStreamAsync(stream, ingestionProperties);

        if (result != null)
        {
            _logger.LogInformation("Ingestion results.");
            foreach (IngestionStatus? item in result.GetIngestionStatusCollection())
            {
                _logger.LogInformation("Id: {IngestionSourceId}\tTable: {Table}\tStatus: {Status}\tDetails: {Details}",
                    item.IngestionSourceId,
                    item.Table,
                    item.Status,
                    item.Details);
            }
        }
        else
        {
            _logger.LogWarning("Ingestion client did not produce any results.");
        }
    }

    internal async IAsyncEnumerable<McpToolEvent> GetLatestToolEventsAsync(string kqlFilePath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(kqlFilePath))
        {
            throw new FileNotFoundException($"KQL file not found: {kqlFilePath}");
        }

        var kql = await File.ReadAllTextAsync(kqlFilePath, cancellationToken);

        var clientRequestProperties = new ClientRequestProperties();
        IDataReader reader = await _kustoClient.ExecuteQueryAsync(_databaseName, kql, clientRequestProperties, cancellationToken);

        var eventTimeOrdinal = reader.GetOrdinal(nameof(McpToolEvent.EventTime));
        var eventTypeOrdinal = reader.GetOrdinal(nameof(McpToolEvent.EventType));
        var serverVersionOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ServerVersion));
        var toolIdOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ToolId));
        var toolNameOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ToolName));
        var toolAreaOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ToolArea));
        var replacedByToolNameOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ReplacedByToolName));
        var replacedByToolAreaOrdinal = reader.GetOrdinal(nameof(McpToolEvent.ReplacedByToolArea));

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            DateTime eventTime = reader.GetDateTime(eventTimeOrdinal);
            var eventTypeString = reader.GetString(eventTypeOrdinal);
            var serverVersion = reader.GetString(serverVersionOrdinal);
            var toolId = reader.GetString(toolIdOrdinal);
            var toolName = reader.GetString(toolNameOrdinal);
            var toolArea = reader.GetString(toolAreaOrdinal);
            var replacedByToolName = reader.IsDBNull(replacedByToolNameOrdinal)
                ? null
                : reader.GetString(replacedByToolNameOrdinal);
            var replacedByToolArea = reader.IsDBNull(replacedByToolAreaOrdinal)
                ? null
                : reader.GetString(replacedByToolAreaOrdinal);

            if (!Enum.TryParse<McpToolEventType>(eventTypeString, ignoreCase: true, out McpToolEventType eventType))
            {
                throw new InvalidOperationException($"Invalid EventType value: '{eventTypeString}'. EventTime: '{eventTime}', ToolName: '{toolName}', ToolArea: '{toolArea}'");
            }

            var tool = new McpToolEvent
            {
                EventTime = eventTime,
                EventType = eventType,
                ServerVersion = serverVersion,
                ToolId = toolId,
                ToolName = toolName,
                ToolArea = toolArea,
                ReplacedByToolName = replacedByToolName,
                ReplacedByToolArea = replacedByToolArea,
            };

            yield return tool;
        }
    }
}
