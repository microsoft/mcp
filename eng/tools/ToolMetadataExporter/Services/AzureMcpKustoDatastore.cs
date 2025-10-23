// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        var queryFile = _queriesDirectory.GetFiles("GetAvailableTools.kql").FirstOrDefault();

        if (queryFile == null)
        {
            throw new InvalidOperationException($"Could not find GetAvailableTools.kql in {_queriesDirectory.FullName}");
        }

        var results = new List<AzureMcpTool>();

        await foreach (var latestEvent in GetLatestToolEventsAsync(queryFile.FullName, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(latestEvent.ToolId))
            {
                throw new InvalidOperationException(
                    $"Cannot have an event with no id. Name: {latestEvent.ToolArea}, Area: {latestEvent.ToolArea}");
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

    public async Task AddToolEventsAsync(IList<McpToolEvent> toolEvents, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using MemoryStream stream = new MemoryStream();

        await JsonSerializer.SerializeAsync(stream, toolEvents, ModelsSerializationContext.Default.ListMcpToolEvent, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);

        cancellationToken.ThrowIfCancellationRequested();

        var ingestionProperties = new KustoIngestionProperties(_databaseName, _tableName)
        {
            Format = DataSourceFormat.json,
            IngestionMapping = new IngestionMapping()
            {
                IngestionMappingKind = IngestionMappingKind.Json,
                IngestionMappings = McpToolEvent.GetColumnMappings()
            }
        };

        await _ingestClient.IngestFromStreamAsync(stream, ingestionProperties);
    }

    internal async IAsyncEnumerable<McpToolEvent> GetLatestToolEventsAsync(string kqlFilePath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(kqlFilePath))
        {
            throw new FileNotFoundException($"KQL file not found: {kqlFilePath}");
        }

        var kql = await File.ReadAllTextAsync(kqlFilePath);

        var clientRequestProperties = new ClientRequestProperties();
        var reader = await _kustoClient.ExecuteQueryAsync(_databaseName, kql, clientRequestProperties, cancellationToken);

        var eventTimeOrdinal = reader.GetOrdinal("EventTime");
        var eventTypeOrdinal = reader.GetOrdinal("EventType");
        var serverVersionOrdinal = reader.GetOrdinal("ServerVersion");
        var toolIdOrdinal = reader.GetOrdinal("ToolId");
        var toolNameOrdinal = reader.GetOrdinal("ToolName");
        var toolAreaOrdinal = reader.GetOrdinal("ToolArea");
        var replacedByToolNameOrdinal = reader.GetOrdinal("ReplacedByToolName");
        var replacedByToolAreaOrdinal = reader.GetOrdinal("ReplacedByToolArea");

        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var eventTime = reader.GetDateTime(eventTimeOrdinal);
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

            if (!Enum.TryParse<McpToolEventType>(eventTypeString, ignoreCase: true, out var eventType))
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
