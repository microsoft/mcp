# Data Agent definition

This article provides a breakdown of the structure for Data Agent definition items. 

## Supported formats

Data Agent items support the JSON format. 

## Definition parts

This table lists the Data Agent definition parts. 

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `Files/Config/data_agent.json` | Data Agent Definition (JSON) | yes | The top-level configuration for a data agent.  Specifies the schema version.  |
| `Files/Config/draft/stage_config.json` | Data Agent Stage Configuration (JSON) | yes | The top-level configuration for a data agent per stage (draft and published each have their own). |
| `Files/Config/draft/{dataSourceType}-{dataSourceName}/datasource.json` | Data Source Configuration (JSON) | no | The basic configuration for a Data Agent data source. |
| `Files/Config/draft/{dataSourceType}-{dataSourceName}/fewshots.json` | Data Agent Few Shot Examples (JSON) | no | The few shot examples associated with a data source. |


If the Data Agent is published, the following parts will be added - 

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `Files/Config/publish_info.json` | Data Agent Publish Info (JSON) | no | Information about the Data Agent publish operation and state. 
| `Files/Config/published/stage_config.json` | Data Agent Stage Configuration (JSON) | no | The top-level configuration for a data agent per stage (draft and published each have their own). |
| `Files/Config/published/{dataSourceType}-{dataSourceName}/datasource.json` | Data Source Configuration (JSON) | no | The basic configuration for a Data Agent data source. |
| `Files/Config/published/{dataSourceType}-{dataSourceName}/fewshots.json` | Data Agent Few Shot Examples (JSON) | no | The few shot examples associated with a data source. |


## Definition example

```json
{
 "parts": [
    {
      "path": "Files/Config/data_agent.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL2RhdGFBZ2VudC8yLjEuMC9zY2hlbWEuanNvbiIKfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "Files/Config/draft/lakehouse-RoutingLH/datasource.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL2RhdGFBZ2VudC8yLjEuMC9zY2hlbWEuanNvbiIKfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "Files/Config/draft/lakehouse-RoutingLH/fewshots.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL2Zld1Nob3RzLzEuMC4wL3NjaGVtYS5qc29uIiwKICAiZmV3U2hvdHMiOiBbCiAgICB7CiAgICAgICJpZCI6ICIxOGRhYWExZC0yN2VkLTRkNzktODMyOC05ZjE3YjY2YTg0NjUiLAogICAgICAicXVlc3Rpb24iOiAidGVzdCIsCiAgICAgICJxdWVyeSI6ICJzZWxlY3QgMTsiCiAgICB9CiAgXQp9",
      "payloadType": "InlineBase64"
    },
    {
      "path": "Files/Config/published/lakehouse-RoutingLH/datasource.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL2RhdGFBZ2VudC8yLjEuMC9zY2hlbWEuanNvbiIKfQ==",
      "payloadType": "InlineBase64"
    },
    {
      "path": "Files/Config/published/lakehouse-RoutingLH/fewshots.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL2Zld1Nob3RzLzEuMC4wL3NjaGVtYS5qc29uIiwKICAiZmV3U2hvdHMiOiBbCiAgICB7CiAgICAgICJpZCI6ICIxOGRhYWExZC0yN2VkLTRkNzktODMyOC05ZjE3YjY2YTg0NjUiLAogICAgICAicXVlc3Rpb24iOiAidGVzdCIsCiAgICAgICJxdWVyeSI6ICJzZWxlY3QgMTsiCiAgICB9CiAgXQp9",
      "payloadType": "InlineBase64"
    },
    {
      "path":  "Files/Config/draft/stage_config.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL3N0YWdlQ29uZmlndXJhdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsCiAgImFpSW5zdHJ1Y3Rpb25zIjogIkFEU0RTWkYiCn0=",
      "payloadType": "InlineBase64"
    },
	{
      "path":  "Files/Config/published/stage_config.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL3N0YWdlQ29uZmlndXJhdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsCiAgImFpSW5zdHJ1Y3Rpb25zIjogIkFEU0RTWkYiCn0=",
      "payloadType": "InlineBase64"
    },
    {
      "path":  "Files/Config/publish_info.json",
      "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL2RhdGFBZ2VudC9kZWZpbml0aW9uL3B1Ymxpc2hJbmZvLzEuMC4wL3NjaGVtYS5qc29uIiwKICAiZGVzY3JpcHRpb24iOiAiIgp9",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## Data Agent Definition

The top-level configuration for a data agent.  Specifies the schema version.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | The schema version of the overall data agent configuration (e.g., "2.1.0"). |

### Data Agent Definition file example

```json
{
  "$schema": "2.1.0"
}
```

## Data Source Configuration

The basic configuration for a Data Agent data source.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | The schema version of the datasource file (e.g., "1.0.0"). |
| `artifactId` | string | true | The artifact id of the data source.  |
| `workspaceId` | string | true | The workspace id of the data source.  |
| `dataSourceInstructions` | string | false | The additional instructions.  |
| `displayName` | string | true | The display name of the data source. |
| `type` | string (enum) | true | The type of the data source (ex: lakehouse-tables, datawarehouse, kusto). Values: unknown, lakehouse_tables, lakehouse, data_warehouse, kusto, semantic_model, graph, mirrored_database, mirrored_azure_databricks |
| `userDescription` | string | false | The data source description. |
| `metadata` | object | false | The metadata (property bag for passing parameters that are not in the schema yet). |
| `elements` | DataSourceElement[] | false | The children elements of the data source. |

### type (enum)
| Name | Description | 
|---|---|
| lakehouse-tables | Lakehouse data type (tables only) | 
| lakehouse | Lakehouse data source | 
| data_warehouse | Data Warehouse data source | 
| kusto | KQL data source | 
| semantic_model | Semantic Model Data Source | 
| graph | GQL data source | 
| mirrored_database | Mirrored DB |  
| mirrored_azure_databricks | Mirrored Azure Databricks |
| unknown | unknown data source | 

### DataSourceElement

The basic information about a data source element.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string(uuid) | true | The id of the data source element. |
| `is_selected` | boolean | true | Whether the element is selected. |
| `display_name` | string | true | The display name of the data source element. |
| `type` | string (enum) | true | The type of the element |
| `data_type` | string | false | The type of the data source element (ex: string, int, double). This property's value is dependent on the datatype conventions of the system. |
| `description` | string | false | The element description. |
| `children` | DataSourceElement[] | false | The children of the element. |
| `index_state` | string | false | Whether the directory is indexed. |

### type (enum)
| Name | Description | 
|---|---|
| lakehouse_tables | Lakehouse tables top level element | 
| lakehouse_tables.schema | Lakehouse tables schema | 
| lakehouse_files | Lakehouse files  |
| lakehouse_files.directory | Lakehouse files directory | 
| warehouse_tables | Data warehouse tables | 
| warehouse_tables.schema | Data warehouse tables schema |
| kusto | kusto datasource element | 
| semantic_model | semantic model element | 
| lakehouse_tables.table | Lakehouse table element |
| warehouse_tables.table | Warehouse table element |
| kusto.table | Kusto table element |
| semantic_model.table | Semantic model table element |
| lakehouse_tables.column | Lakehouse table - column element | 
| warehouse_tables.column | Warehouse table - column element |  
| kusto.column | Kusto table - column element |
| kusto.functions | Kusto - functions |
| semantic_model.column | Semantic model column element |
| semantic_model.measure | Semantic model measure |
| graph.nodeType | Graph - node type element |
| graph.edgeType | Graph - edge type element |
| mirrored_database.schema | Mirrored DB schema |
| mirrored_database.table | Mirrored DB table element |
| mirrored_database.column | Mirrored DB column element |


### Data Source Configuration file example

```json
{
  "$schema": "1.0.0",
  "artifactId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "workspaceId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "displayName": "Sales Data Warehouse",
  "type": "data_warehouse",
  "userDescription": "Primary data warehouse for sales analytics",
  "dataSourceInstructions": "Use this source for all sales-related queries",
  "elements":  [
    {
      "display_name": "dbo",
      "type": "warehouse_tables.schema",
      "is_selected": true,
      "children": [
        {
          "display_name": "SalesTable",
          "type": "warehouse_tables.table",
          "is_selected": true,
          "description": "Main sales transactions table",
          "children": [
            {
              "display_name": "TransactionID",
              "type": "warehouse_tables.column",
              "data_type": "int",
              "description": "Unique transaction identifier"
            }
          ]
        }
      ]
    }
  ]
}
```

## Data Agent Few Shot Examples

The few shot examples associated with a data source.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string  | true | The schema version of the fewshots file (e.g., "1.0.0"). |
| `fewShots` | FewShot[] | false | The list of few shots. |

### FewShot Example Item

Represents the properties of a single few shot example.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string (uuid) | true | The ID of the example, which is a GUID. |
| `question` | string | true | The question related to the few shot example. |
| `query` | string | true | The example query. |

### Data Agent Few Shot Examples file example

```json
{
  "$schema":  "1.0.0",
  "fewShots": [
    {
      "id":  "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "question": "What were the total sales for last quarter?",
      "query":  "SELECT SUM(Amount) FROM SalesTable WHERE Quarter = 'Q4'"
    },
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "question":  "Show me the top 10 products by revenue",
      "query": "SELECT TOP 10 ProductName, SUM(Revenue) as TotalRevenue FROM Products GROUP BY ProductName ORDER BY TotalRevenue DESC"
    }
  ]
}
```

## Data Agent Publish Info

The publishing information for a data agent configuration. 

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | The schema version of the publish information file (e.g., "1.0.0"). |
| `description` | string | true | The Data Agent description at publishing time. |

### Data Agent Publish Info file example

```json
{
  "$schema":  "1.0.0",
  "description": "Sales Data Agent - Production version 1.0 - Published on 2026-01-21"
}
```

## Data Agent Stage Configuration

The top-level configuration for a data agent per stage.  Sandbox and production each have their own configuration.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | The schema version of the configuration file (e.g., "1.0.0"). |
| `aiInstructions` | string | true | The AI instructions for the Data Agent. |
| `experimental` | object | false | The experimental properties of the configuration. |

### Data Agent Stage Configuration file example

```json
{
  "$schema": "1.0.0",
  "aiInstructions": "You are a helpful sales data assistant. Always provide accurate data from the sales warehouse. When answering questions about revenue, include currency formatting.",
}
```