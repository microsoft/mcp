// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Kusto.Data.Common;

namespace ToolMetadataExporter.Models.Kusto;

public class McpToolEvent
{
    private const string EventTimeColumn = "EventTime";
    private const string EventTypeColumn = "EventType";
    private const string ServerVersionColumn = "ServerVersion";
    private const string ToolIdColumn = "ToolId";
    private const string ToolNameColumn = "ToolName";
    private const string ToolAreaColumn = "ToolArea";
    private const string ReplacedByToolNameColumn = "ReplacedByToolName";
    private const string ReplacedByToolAreaColumn = "ReplacedByToolArea";

    [JsonPropertyName(EventTimeColumn)]
    public DateTimeOffset? EventTime { get; set; }

    [JsonPropertyName(EventTypeColumn)]
    public McpToolEventType? EventType { get; set; }

    [JsonPropertyName(ServerVersionColumn)]
    public string? ServerVersion { get; set; }

    [JsonPropertyName(ToolIdColumn)]
    public string? ToolId { get; set; }

    [JsonPropertyName(ToolNameColumn)]
    public string? ToolName { get; set; }

    [JsonPropertyName(ToolAreaColumn)]
    public string? ToolArea { get; set; }

    [JsonPropertyName(ReplacedByToolNameColumn)]
    public string? ReplacedByToolName { get; set; }

    [JsonPropertyName(ReplacedByToolAreaColumn)]
    public string? ReplacedByToolArea { get; set; }

    public static ColumnMapping[] GetColumnMappings()
    {
        return [
            new ColumnMapping { ColumnName = EventTimeColumn, ColumnType = "datetime" },
            new ColumnMapping { ColumnName = EventTypeColumn, ColumnType = "string"},
            new ColumnMapping { ColumnName = ReplacedByToolAreaColumn, ColumnType = "string"},
            new ColumnMapping { ColumnName = ReplacedByToolNameColumn, ColumnType = "string"},
            new ColumnMapping { ColumnName = ServerVersionColumn, ColumnType = "string" },
            new ColumnMapping { ColumnName = ToolAreaColumn , ColumnType = "string" },
            new ColumnMapping { ColumnName = ToolIdColumn, ColumnType = "string"},
            new ColumnMapping { ColumnName = ToolNameColumn, ColumnType = "string" },
        ];
    }
}
