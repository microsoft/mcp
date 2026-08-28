# Plan definition

This article provides a breakdown of the definition structure for Plan items. A Plan item is a Microsoft Fabric Planning workload item that enables planning, budgeting, forecasting, and writeback scenarios against a semantic model.

## Supported formats

Plan items support the JSON format.

## Definition parts

A Plan definition contains top-level parts plus per-sheet and per-visual parts. Each part uses the `InlineBase64` payload type. This table lists all Plan definition parts.

### Top-level parts

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `definition.json` | Plan Definition (JSON) | true | Root definition declaring the semantic model reference and sheet list. |
| `planProperties.json` | Plan Properties (JSON) | true | Plan-level UI and behavior properties (theme, mode config, filter assignments). |
| `connectedPlanning/infobridge.json` | InfoBridge Configuration (JSON) | false | InfoBridge data source, query pipeline, and writeback destination configuration. Required when the plan uses Connected Planning. |
| `cube/cube.json` | Cube Configuration (JSON) | false | Cube partition definitions, measures, and column mappings. Required when the plan uses cube-based writeback. |
| `.platform` | PlatformDetails (JSON) | true | Fabric Git integration platform metadata (item type, display name, logical ID). |

### Per-sheet parts

Each sheet is stored under `sheets/{sheetId}/`. The `{sheetId}` is the `recordGuid` UUID of the sheet.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `sheets/{sheetId}/sheet.json` | Sheet (JSON) | true | Sheet-level canvas layout, filter pane position, commentary, and visual group map. |
| `sheets/{sheetId}/commentSettings.json` | Comment Settings (JSON) | false | Comment panel settings (allow comment, notification, indicator display). Required for PLANNING and POWERTABLE sheet types. |

### Per-visual parts (Planning visuals)

Each Planning visual is stored under `sheets/{sheetId}/visuals/{visualId}/`. The `{visualId}` is the UUID of the visual.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `sheets/{sheetId}/visuals/{visualId}/dataInput.json` | Data Input Columns (JSON) | true | Column definitions for a Planning visual (measures, forecasts, text/number inputs). |
| `sheets/{sheetId}/visuals/{visualId}/properties.json` | Visual Properties — Planning (JSON) | true | Pivot assignments, sorting, and filter configurations for a Planning visual. |
| `sheets/{sheetId}/visuals/{visualId}/writeback.json` | Writeback Configuration (JSON) | false | Writeback destination, column mapping, and auto-writeback settings. |
| `sheets/{sheetId}/visuals/{visualId}/insertRows.json` | Insert Rows (JSON) | false | Custom static and calculated rows inserted into the Planning visual. |
| `sheets/{sheetId}/visuals/{visualId}/scenarios.json` | Scenarios (JSON) | false | Scenario definitions with simulation data for what-if analysis. |
| `sheets/{sheetId}/visuals/{visualId}/modelTemplate.json` | Model Template (JSON) | false | Dynamic row template configurations for the Planning visual. |

### Per-visual parts (PowerTable visuals)

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `sheets/{sheetId}/visuals/{visualId}/columnConfigs.json` | PowerTable Column Configs (JSON) | true | Column definitions for a PowerTable visual (type, editability, validation, SCD metadata). |
| `sheets/{sheetId}/visuals/{visualId}/properties.json` | PowerTable Properties (JSON) | true | Pivot assignments, filters, visual styles, position, and visual state for a PowerTable visual. |
| `sheets/{sheetId}/visuals/{visualId}/source.json` | PowerTable Source (JSON) | true | Database connection and table reference for a PowerTable visual. |
| `sheets/{sheetId}/visuals/{visualId}/sourceSettings.json` | PowerTable Settings (JSON) | true | Row-level permissions (ROW_ADD, ROW_UPDATE, ROW_DELETE) and comment/SCD settings. |
| `sheets/{sheetId}/visuals/{visualId}/approvals.json` | PowerTable Approvals (JSON) | false | Approval workflow configuration including levels and routing filters. |
| `sheets/{sheetId}/visuals/{visualId}/automations.json` | PowerTable Automations (JSON) | false | Automation trigger and action flow definitions. |
| `sheets/{sheetId}/visuals/{visualId}/forms.json` | PowerTable Forms (JSON) | false | Data entry form layout definitions. |

### Per-visual parts (Intelligence visuals)

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `sheets/{sheetId}/visuals/{visualId}/properties.json` | Intelligence Properties (JSON) | true | Page-level settings, entity variables, canvas styles, commentary, and embedded visual configurations for an Intelligence sheet. |

## Definition example

```json
{
  "parts": [
    {
      "path": "definition.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "planProperties.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "cube/cube.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "connectedPlanning/infobridge.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/sheet.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/commentSettings.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/visuals/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/dataInput.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/visuals/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/properties.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": "sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/visuals/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/writeback.json",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    },
    {
      "path": ".platform",
      "payload": "<base64 encoded string>",
      "payloadType": "InlineBase64"
    }
  ]
}
```

## Plan Definition (`definition.json`)

Root definition for a Plan artifact containing the semantic model reference and sheet declarations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/definition/1.0.0/schema.json`. |
| `workloadItemName` | string | true | User-facing name of the workload item. |
| `semanticModelReference` | [SemanticModelReference](#semanticmodelreference) | true | Reference to the semantic model backing this plan. |
| `sheets` | [SheetReference](#sheetreference)[] | true | List of sheets in the plan. |

### SemanticModelReference

| Property | Type | Required | Description |
|---|---|---|---|
| `connection` | [ConnectionReferenceOrVar](#connectionreferenceorvar) | No* | Fabric connection reference. Required when using the portable format (with `semanticModel`). |
| `semanticModel` | [ItemReferenceOrVar](#itemreferenceorvar) | No* | Fabric item reference to the semantic model. Required when using the portable format. |
| `connectionId` | string (uuid) | No* | Connection ID. Required in the legacy format. |
| `semanticModelId` | string (uuid) | No* | Semantic model ID. Required in the legacy format. |
| `semanticModelName` | string | No* | Semantic model display name. Required in the legacy format. |
| `semanticModelWorkspaceId` | string (uuid) | No* | Workspace ID of the semantic model. Required in the legacy format. |
| `semanticModelWorkspaceName` | string | No* | Workspace name. Required in the legacy format. |
| `directLakeMode` | boolean | No* | Whether Direct Lake mode is enabled. Required in the legacy format. |
| `directQueryMode` | boolean | No* | Whether Direct Query mode is enabled. Required in the legacy format. |
| `sourceType` | string | No* | Source type: `POWERTABLE`, `INFOBRIDGE`, or `WORKLOAD`. Required in the legacy format. |

> *Either (`connection` + `semanticModel`) or the full legacy set is required.

### ConnectionReferenceOrVar

Either an inline connection reference or a reference to a [variable library](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/variable-library-definition) variable of type `ConnectionReference`.

| Property | Type | Required | Description |
|---|---|---|---|
| `connectionId` | string (uuid) | true | The ID of the connection. |

### ItemReferenceOrVar

Either an inline item reference or a reference to a [variable library](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/variable-library-definition) variable of type `ItemReference`.

| Property | Type | Required | Description |
|---|---|---|---|
| `workspaceId` | string (uuid) | true | The ID of the workspace. |
| `itemId` | string (uuid) | true | The ID of the item. |

### SheetReference

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Unique identifier for the sheet record. Also used as the `{sheetId}` in file paths. |
| `displayName` | string | true | User-facing name of the sheet. |
| `sheetType` | string | true | Sheet type: `PLANNING`, `REPORTING`, `POWERTABLE`, `INFOBRIDGE`, `SUPER_FILTER`, `BI_REPORTING`, `BI_ADHOC_ANALYSIS`, or `BI_DASHBOARD`. |
| `isHidden` | boolean | false | Whether the sheet is hidden. |
| `order` | number | false | Display order of the sheet. |
| `workloadItemEntityVisuals` | array | false | List of visual references on the sheet. |

### Plan Definition file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/definition/1.0.0/schema.json",
  "workloadItemName": "My Budget Plan",
  "semanticModelReference": {
    "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "semanticModelId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "semanticModelName": "Sales Dataset",
    "semanticModelWorkspaceId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "semanticModelWorkspaceName": "Finance Workspace",
    "directLakeMode": false,
    "directQueryMode": false,
    "sourceType": "WORKLOAD"
  },
  "sheets": [
    {
      "recordGuid": "019f243d-f53e-7841-b530-984dd0f34497",
      "displayName": "Budget Detail",
      "sheetType": "PLANNING",
      "isHidden": false,
      "workloadItemEntityVisuals": [
        {
          "visualType": "PLANNING",
          "visualId": "019f243d-f53e-7576-b29e-87494bb2e4e7",
          "isEmbedded": false
        }
      ]
    }
  ]
}
```

## Plan Properties (`planProperties.json`)

Plan-level UI and behavior properties for a Planning workload item.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planProperties/1.0.0/schema.json`. |
| `properties` | [PlanProperties](#planproperties) | true | Top-level plan properties object. |

### PlanProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `workloadModeConfig` | [WorkloadModeConfig](#workloadmodeconfig) | true | Panel visibility configuration per workload mode. |
| `theme` | object | false | Theme type configuration (`type`: integer). |
| `workloadLevelFilterAssignments` | array | false | Workload-level filter assignments. |
| `dataStreamerConfig` | object | false | Data streamer configuration (`enabled`: boolean). |
| `workloadLevelVariables` | array | false | Workload-level variable definitions. |
| `reportPageTooltipEntities` | array | false | Report page tooltip entity references. |
| `syncVisualsState` | object | false | Synchronized visuals state map. |
| `workloadLevelQueryFilterAssignments` | array | false | Workload-level query filter assignments. |
| `drillThroughConfig` | object | false | Drill-through configuration. |
| `entityAdditionalProps` | object | false | Additional entity properties. |
| `favoriteCharts` | array | false | Favorite chart references. |
| `syncSlicerConfig` | array | false | Synchronized slicer configurations. |
| `pageGroupingMeta` | [PageGroupingMeta](#pagegroupingmeta) | false | Page grouping metadata. |

### WorkloadModeConfig

Configuration for panel visibility per workload mode.

| Property | Type | Required | Description |
|---|---|---|---|
| `EDIT` | [EditModeConfig](#editmodeconfig) | true | Panel visibility settings for EDIT mode. |
| `READ` | [ReadModeConfig](#readmodeconfig) | true | Panel visibility settings for READ mode. |

### EditModeConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `showFilterPane` | boolean | true | Whether the filter pane is visible. |
| `showDataPane` | boolean | true | Whether the data pane is visible. |
| `showFieldsPane` | boolean | true | Whether the fields pane is visible. |
| `showElementsPane` | boolean | true | Whether the elements pane is visible. |
| `showBookmarksPane` | boolean | true | Whether the bookmarks pane is visible. |
| `showPersonalizePane` | boolean | true | Whether the personalize pane is visible. |
| `showCommentsPane` | boolean | true | Whether the comments pane is visible. |
| `showVariablesPane` | boolean | true | Whether the variables pane is visible. |
| `showLumelAIPane` | boolean | false | Whether the Lumel AI pane is visible. |

### ReadModeConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `showFilterPane` | boolean | true | Whether the filter pane is visible. |
| `showDataPane` | boolean | true | Whether the data pane is visible. |
| `showBookmarksPane` | boolean | true | Whether the bookmarks pane is visible. |
| `showPersonalizePane` | boolean | true | Whether the personalize pane is visible. |
| `showVariablesPane` | boolean | true | Whether the variables pane is visible. |
| `showCommentsPane` | boolean | true | Whether the comments pane is visible. |
| `showLumelAIPane` | boolean | false | Whether the Lumel AI pane is visible. |

### PageGroupingMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `pageGroupMapping` | object | true | Map of page identifiers to page groups. Additional properties are allowed. |
| `pageGroups` | array | true | Page group definitions. Item shape is open in the schema. |

### Plan Properties file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planProperties/1.0.0/schema.json",
  "properties": {
    "workloadModeConfig": {
      "EDIT": {
        "showFilterPane": true,
        "showDataPane": true,
        "showFieldsPane": true,
        "showElementsPane": true,
        "showBookmarksPane": false,
        "showPersonalizePane": false,
        "showCommentsPane": false,
        "showVariablesPane": false
      },
      "READ": {
        "showFilterPane": true,
        "showDataPane": false,
        "showBookmarksPane": false,
        "showPersonalizePane": false,
        "showVariablesPane": false,
        "showCommentsPane": true
      }
    },
    "dataStreamerConfig": {
      "enabled": false
    },
    "workloadLevelFilterAssignments": [],
    "workloadLevelVariables": []
  }
}
```

## Cube Configuration (`cube/cube.json`)

Schema for cube partition payloads including partition definitions, cube partition measures, and mapping tables.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | false | JSON Schema URI reference. |
| `cubePartitions` | [CubePartition](#cubepartition)[] | true | List of cube partition definitions. |
| `cubePartitionMeasures` | [CubePartitionMeasure](#cubepartitionmeasure)[] | true | List of measures associated with cube partitions. |
| `cubePartitionMeasureMappings` | [CubePartitionMeasureMapping](#cubepartitionmeasuremapping)[] | true | Mappings between cube partitions and measures. |
| `cubePartitionMeasureDataInputColumnMappings` | [CubePartitionMeasureDataInputColumnMapping](#cubepartitionmeasuredatainputcolumnmapping)[] | true | Mappings from cube partition measures to data input columns. |

### CubePartition

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Unique identifier for the cube partition. |
| `name` | string | true | Display name of the partition. |
| `dimensions` | [Dimension](#dimension)[] | true | List of dimension definitions. |
| `timeDimensions` | [TimeDimension](#timedimension)[] | true | List of time dimension definitions. |
| `measures` | [Measure](#measure)[] | true | List of measure definitions. |
| `rowCount` | integer | true | Number of rows in the partition. |
| `id` | integer | false | Internal numeric ID. |
| `status` | string | false | Status code (`ACTIVE`). |

### Dimension

Dimension definition for a cube partition.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique dimension identifier (format: `[TABLE[Hierarchy]]~\|\|\|~TABLE[COLUMN]`). |
| `label` | string | true | Display name of the dimension. |
| `type` | string | true | Type classification. See [Cube column type](#cube-column-type). |
| `dataType` | string | true | Data type: `String`, `Int64`, `Decimal`, etc. |
| `distinctValueCount` | integer | false | Number of distinct values in this dimension. |

### Cube column type

Column and hierarchy categories used by cube dimensions and measures. These values correlate to the `EColumnType` enum.

| Name | Value | Description |
|---|---|---|
| `Measure` | `Measure` | Measure field. |
| `Column` | `Column` | Regular column. |
| `Hierarchy` | `Hierarchy` | Hierarchy container. |
| `Hierarchy Level` | `Hierarchy Level` | Hierarchy level. |
| `Row` | `Row` | Row entity. |
| `Date` | `Date` | Date dimension member. |
| `CalculatedTableColumn` | `CalculatedTableColumn` | Calculated table column. |

### TimeDimension

Time dimension definition for a cube partition.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique time dimension identifier (format: `LocalDateTable[Period]` or similar). |
| `label` | string | true | Display name of the time dimension. |
| `type` | string | true | Type classification. See [Cube column type](#cube-column-type). |
| `dataType` | string | true | Data type: `Int64`, `String`, `Date`, etc. |

### Measure

Measure definition for a cube partition.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique measure identifier (format: `TABLE[MeasureName]`). |
| `label` | string | true | Display name of the measure. |
| `type` | string | true | Type classification. See [Cube column type](#cube-column-type). |
| `dataType` | string | true | Data type: `Number`, `String`, `Decimal`, etc. |
| `isNative` | boolean | false | Whether this is a native semantic model measure. |
| `aggregationType` | string | false | Aggregation method: `Sum`, `Average`, `Min`, `Max`, `Count`, `DistinctCount`, etc. |

### CubePartitionMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Measure record identifier. |
| `name` | string | true | Measure name. |
| `config` | [CubePartitionMeasureConfig](#cubepartitionmeasureconfig) | true | Measure configuration. |
| `type` | string | true | Measure type. See [Cube partition measure type](#cube-partition-measure-type). |
| `id` | integer | false | Internal numeric identifier. |
| `status` | string | false | Status code, such as `ACTIVE`. |

### Cube partition measure type

Values used by `cubePartitionMeasures[].type`.

| Name | String value | Integer value | Description |
|---|---|---:|---|
| `FORECAST` | `FORECAST` | 10 | Forecast measure. |
| `NUMBER` | `NUMBER` | 20 | Numeric measure. |
| `FORMULA` | `FORMULA` | 30 | Formula-based measure. |

### CubePartitionMeasureConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `label` | string | false | Display label. |
| `measure_type` | object | false | Measure type dictionary. |
| `measureType` | object | false | Camel-case measure type dictionary. |
| `data_type` | string | false | Data type. |
| `dataType` | string | false | Camel-case data type. |
| `description` | string | false | Measure description. |
| `aggregationType` | string | false | Aggregation type. |
| `closedPeriodConfig` | object | false | Closed-period configuration. |
| `openPeriodConfig` | array | false | Open-period forecast configuration. |
| `openPeriodRange` | [PeriodRange](#periodrange) | false | Open-period range. |
| `formulaQuery` | string | false | Formula query. |
| `multiDimensionSettings` | object | false | Multi-dimensional settings. |
| `nativeMeasureMappings` | [Measure](#measure)[] | false | Native measure mappings. |

### CubePartitionMeasureMapping

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Mapping record identifier. |
| `cubePartitionId` | integer | No* | Partition ID. Required with `cubePartitionMeasureId` when reference GUIDs are not used. |
| `cubePartitionMeasureId` | integer | No* | Partition measure ID. |
| `cubePartition_RefId` | string (uuid) | No* | Partition reference ID. Required with `cubePartitionMeasure_RefId` when numeric IDs are not used. |
| `cubePartitionMeasure_RefId` | string (uuid) | No* | Partition measure reference ID. |
| `id` | integer | false | Internal numeric identifier. |
| `status` | string | false | Status code. |

### CubePartitionMeasureDataInputColumnMapping

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Mapping record identifier. |
| `dataInputColumnId` | integer | false | Data input column identifier. |
| `cubePartitionMeasureId` | integer | false | Cube partition measure identifier. |
| `visualId` | integer | false | Visual identifier. |
| `cubePartitionId` | integer or null | false | Cube partition identifier. |
| `filterHash` | string or null | false | Filter context hash. |
| `visual_RefId` | string (uuid) | false | Visual reference identifier. |
| `cubePartitionMeasure_RefId` | string (uuid) | false | Cube partition measure reference identifier. |
| `cubePartition_RefId` | string (uuid) or null | false | Cube partition reference identifier. |
| `dataInputColumnMeasureGuid` | string | false | Data input column measure GUID. |

### Cube Configuration file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/cube/1.0.0/schema.json",
  "cubePartitions": [
    {
      "recordGuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Breakdown 1",
      "dimensions": [
        {
          "id": "[TABLE[Hierarchy]]~|||~TABLE[REGION]",
          "label": "REGION",
          "type": "Hierarchy Level",
          "dataType": "String",
          "distinctValueCount": 3
        }
      ],
      "timeDimensions": [
        {
          "id": "LocalDateTable[Year]",
          "label": "Year",
          "type": "Hierarchy Level",
          "dataType": "Int64"
        }
      ],
      "measures": [
        {
          "id": "TABLE[AC]",
          "label": "AC",
          "type": "Measure",
          "dataType": "Number",
          "isNative": true,
          "aggregationType": "Sum"
        }
      ],
      "rowCount": 0
    }
  ],
  "cubePartitionMeasures": [],
  "cubePartitionMeasureMappings": [],
  "cubePartitionMeasureDataInputColumnMappings": []
}
```

## InfoBridge Configuration (`connectedPlanning/infobridge.json`)

InfoBridge configuration defining data sources, queries, transformation steps, and writeback destinations for connected planning.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/connectedPlanning/infobridge/1.0.0/schema.json`. |
| `sources` | [Source](#source)[] | true | List of InfoBridge data sources (minimum 1). |
| `queryGroups` | [QueryGroup](#querygroup)[] | false | Optional groupings of queries for organizational purposes. |

### Source

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the source. |
| `type` | string or integer | true | Source type. See [Source type](#source-type). |
| `visualId` | integer or string (uuid) | false | Visual identifier this source is associated with. |
| `meta` | [SourceMeta](#sourcemeta) or string | false | Source metadata. |
| `queries` | [Query](#query)[] | false | List of queries for this source. |
| `dependentQueries` | string[] | false | List of dependent query GUIDs for join sources. |

### Source type

Source type codes used by `root.sources[].type`.

| Name | Value | Description |
|---|---:|---|
| `PLANNING` | 10 | Planning source. |
| `APPEND` | 20 | Append source. |
| `MERGE` | 30 | Merge source. |
| `CSV` | 40 | CSV file source. |
| `JSON` | 50 | JSON file source. |
| `XLSX` | 60 | Excel workbook source. |
| `PARQUET` | 70 | Parquet source. |
| `JOIN` | 80 | Join source. |
| `ENCRYPTED_PARQUET` | 170 | Encrypted parquet source. |
| `EDITABLE` | 220 | Editable source. |
| `SQL_SOURCE` | 230 | SQL-backed source. |

### Join type

Join type codes used by `root.sources[].queries[].type` when the query represents a join.

| Value | Label |
|---:|---|
| 10 | `Inner` |
| 20 | `Left Outer` |
| 30 | `Right Outer` |
| 40 | `Full Outer` |

### QueryGroup

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Group display name. |
| `description` | string | false | Optional group description. |
| `parentGroupId` | integer | false | Optional parent group identifier. |
| `queryIds` | string[] | true | Query GUIDs belonging to the group. |

### SourceMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `includeMeasures` | (string or object)[] | false | Measures to include. |
| `includeScenarios` | string[] | false | Scenarios to include. |
| `queries` | (object or string)[] | false | Join query references. See [JoinQueryReference](#joinqueryreference). |
| `joinType` | string | false | Join type, such as `INNER` or `LEFT`. |
| `sql` | string | false | SQL text for a SQL source. |

### JoinQueryReference

| Property | Type | Required | Description |
|---|---|---|---|
| `queryId` | string | No* | Referenced query GUID. Required when `sourceId` is absent. |
| `sourceId` | integer or string | No* | Internal source/query identifier. Required when `queryId` is absent. |
| `sourceName` | string | true | Display name of the source query. |
| `joinColumnName` | string[] | true | Column names used for the join. |
| `isBaseQuery` | boolean | false | Whether this is the base query. |

### Query

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the query. |
| `queryId` | string | true | Unique GUID for this query. |
| `type` | string or integer | false | Query type code. See [Join type](#join-type). |
| `visualId` | integer or string (uuid) | false | Visual identifier. |
| `meta` | object | false | Query-level metadata. |
| `transformationSteps` | [TransformationStep](#transformationstep)[] | false | Ordered list of transformation steps. |
| `writebackSettings` | [WritebackSettings](#writebacksettings) | false | Writeback settings for this query. |
| `writebackDestinations` | [WritebackDestination](#writebackdestination)[] | false | List of writeback destinations. |

### TransformationStep

| Property | Type | Required | Description |
|---|---|---|---|
| `stepIndex` | integer | true | Ordinal index of the step. |
| `meta` | [TransformationStepMeta](#transformationstepmeta) | true | Type, name, and step metadata. |
| `notes` | any | false | Optional notes. The schema does not constrain the value. |

### TransformationStepMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string or integer | true | Transformation type. |
| `name` | string | true | Display name of the step. |
| `value` | object | false | Step-specific configuration. |
| `description` | string | false | Human-readable description. |

### WritebackSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `writebackMeta` | object | false | Query writeback metadata. |
| `writebackMeta.destinationPreferenceCleared` | boolean | false | Whether the destination preference was cleared. |
| `writebackMeta.numberPrecision` | object | false | Decimal precision settings. |
| `writebackMeta.stringColumnLength` | object | false | String column length settings. |

### WritebackDestination

| Property | Type | Required | Description |
|---|---|---|---|
| `connectionId` | string | No* | Connection ID. Required with `databaseId` in the legacy form. |
| `databaseId` | string | No* | Database ID. Required with `connectionId` or `dmtsConnectionId`. |
| `tableName` | string | true | Target table name. |
| `schema` | string | false | Database schema name. |
| `connection` | [ConnectionReferenceOrVar](#connectionreferenceorvar) | No* | Connection reference. Required with `database`. |
| `database` | [ItemReferenceOrVar](#itemreferenceorvar) | No* | Database item reference. Required with `connection`. |
| `dmtsConnectionId` | string | No* | Legacy/writeback connection identifier. |

### InfoBridge Configuration file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/connectedPlanning/infobridge/1.0.0/schema.json",
  "sources": [
    {
      "name": "2A-US",
      "type": 10,
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "queries": [
        {
          "name": "Main Query",
          "queryId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "transformationSteps": [],
          "writebackDestinations": [
            {
              "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
              "databaseId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
              "tableName": "wb_2a_us",
              "schema": "dbo"
            }
          ]
        }
      ]
    }
  ]
}
```

## Sheet (`sheets/{sheetId}/sheet.json`)

Sheet-level UI and canvas properties for a Planning workload item.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/sheets/sheet/1.0.0/schema.json`. |
| `properties` | [SheetProperties](#sheetproperties) | true | Sheet-level canvas and behavior properties. |

### SheetProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `pageLevelFilterAssignments` | array | true | Page-level filter assignments. |
| `entityLevelVariables` | array | true | Entity-level variable definitions. |
| `filterPanePosition` | string | true | Filter pane position (`LEFT`, `RIGHT`, `TOP`, `BOTTOM`). |
| `topPositionFilterExpandConfig` | [TopPositionFilterExpandConfig](#toppositionfilterexpandconfig) | true | Expand configuration for top-positioned filter pane. |
| `commentary` | [Commentary](#commentary) | true | Notes and annotation settings. |
| `canvasStyle` | [CanvasStyle](#canvasstyle) | true | Canvas dimensions, background, wallpaper, border, and shadow styles. |
| `assignmentColumnMap` | object | true | Map of column assignments. |
| `visualGroupMap` | object | true | Map of visual groups. |
| `sourceVisualsMeta` | object | true | Metadata for source visuals. |
| `controlPanePosition` | string | true | Position of the control pane. |

### TopPositionFilterExpandConfig

Expand configuration for filter pane positioned at the top.

| Property | Type | Required | Description |
|---|---|---|---|
| `isFilterPaneCollapsed` | boolean | false | Whether the filter pane is initially collapsed. |
| `expandedHeight` | integer | false | Expanded height of the filter pane in pixels. |

### Commentary

Notes and annotation settings for a sheet.

| Property | Type | Required | Description |
|---|---|---|---|
| `notes` | object | false | Notes configuration including notesMap, settings, noteOrder, enableMarkerMode, markerData. |
| `notes.notesMap` | object | false | Map of note IDs to note content. |
| `notes.settings` | object | false | Note settings (`enable`, `hideAllNotes` - both boolean). |
| `notes.noteOrder` | string[] | false | Ordered list of note IDs. |
| `notes.enableMarkerMode` | boolean | false | Whether marker mode is enabled for notes. |
| `notes.markerData` | array | false | Marker data for note annotations. |
| `annotation` | object | false | Annotation configuration. |
| `annotation.settings` | object | false | Annotation settings (`hideAllAnnotations` - boolean). |

### CanvasStyle

Canvas dimension, background, wallpaper, border, and shadow style configuration.

| Property | Type | Required | Description |
|---|---|---|---|
| `dimension` | object | false | Canvas dimension configuration. |
| `dimension.type` | string | false | Dimension type: `DEFAULT_16_9`, `CUSTOM`, etc. |
| `dimension.width` | integer | false | Canvas width in pixels or percentage units. |
| `dimension.height` | integer | false | Canvas height in pixels or percentage units. |
| `dimension.elementScalingUnit` | string | false | Scaling unit: `pixels`, `percentage`, etc. |
| `background` | object | false | Background color or image configuration. |
| `border` | object | false | Border style configuration. |
| `shadow` | object | false | Shadow effect configuration. |

### Sheet file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/sheets/sheet/1.0.0/schema.json",
  "properties": {
    "pageLevelFilterAssignments": [],
    "entityLevelVariables": [],
    "filterPanePosition": "RIGHT",
    "topPositionFilterExpandConfig": {
      "isFilterPaneCollapsed": true,
      "expandedHeight": 250
    },
    "commentary": {
      "notes": {
        "notesMap": {},
        "settings": {
          "enable": true,
          "hideAllNotes": false
        },
        "noteOrder": [],
        "enableMarkerMode": false,
        "markerData": []
      },
      "annotation": {
        "settings": {
          "hideAllAnnotations": false
        }
      }
    },
    "canvasStyle": {
      "dimension": {
        "type": "DEFAULT_16_9",
        "width": 1600,
        "height": 900,
        "elementScalingUnit": "percentage"
      }
    },
    "assignmentColumnMap": {},
    "visualGroupMap": {},
    "sourceVisualsMeta": {},
    "controlPanePosition": "right"
  }
}
```

## Comment Settings (`sheets/{sheetId}/commentSettings.json`)

Comment panel settings for a Planning or PowerTable sheet.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/sheets/commentSettings/1.0.0/schema.json`. |
| `recordGuid` | string (uuid) | true | Unique identifier for this comment settings record. |
| `allowComment` | boolean | true | Whether commenting is enabled. |
| `enableCommentsColumn` | boolean | true | Whether the comments column is shown. |
| `notification` | boolean | true | Whether comment notifications are enabled. |
| `keepCommentPanelOpen` | boolean | false | Whether the comment panel stays open. |
| `showStarredComments` | boolean | false | Whether starred comments are highlighted. |
| `enableStatusColumn` | boolean | false | Whether the status column is shown. |
| `rollUpComments` | boolean | false | Whether comments roll up to parent rows. |
| `commentIndicatorDisplay` | [CommentIndicatorDisplay](#commentindicatordisplay) | false | Visual indicator configuration (type, pixel size, position). |

### CommentIndicatorDisplay

Visual indicator configuration for comment display.

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string | false | Indicator type: `arrow`, `icon`, `badge`, etc. |
| `pixel` | integer | false | Indicator size in pixels. |
| `position` | string | false | Indicator position: `left`, `right`, `top`, `bottom`. |

### Comment Settings file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/sheets/commentSettings/1.0.0/schema.json",
  "recordGuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "keepCommentPanelOpen": false,
  "showStarredComments": false,
  "allowComment": true,
  "enableCommentsColumn": false,
  "enableStatusColumn": false,
  "rollUpComments": false,
  "commentIndicatorDisplay": {
    "type": "arrow",
    "pixel": 10,
    "position": "right"
  },
  "notification": true
}
```

## Data Input Columns (`sheets/{sheetId}/visuals/{visualId}/dataInput.json`)

Data input column definitions for a Planning visual, including forecasts, text inputs, number inputs, and native measures. The file is an object containing a `columns` array.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/dataInput/1.0.0/schema.json`. |
| `columns` | [DataInputColumn](#datainputcolumn)[] | true | List of data input column definitions. |

### DataInputColumn

| Property | Type | Required | Description |
|---|---|---|---|
| `measureGuid` | string | true | Unique identifier for the measure/column. |
| `visualId` | string (uuid) | true | ID of the visual this column belongs to. |
| `columnMeta` | [ColumnMeta](#columnmeta) | true | Column metadata including label, measure type, and data type. |
| `name` | string | true | Display name of the column. |
| `dataInputType` | integer | true | Data input column type code. See [DataInputColumnType](#datainputcolumntype). |
| `description` | string or null | false | Optional description. |
| `disableWriteAccess` | boolean | false | Whether write access is disabled. |
| `forecastAllowedUserPermissions` | boolean | false | Whether user permissions are allowed for forecast. |

### ColumnMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique column meta identifier. |
| `label` | string | true | Display label. |
| `measure_type` | [MeasureType](#measuretype) | true | Measure type definition (`Forecast`, `Native`, `DataInput`, `VisualColumn`, or `Formula`). |
| `data_type` | string | true | Data type: `Number` or `Text`. |

### DataInputColumnType

Numeric values for `DataInputColumn.dataInputType`.

| Name | Value | Description |
|---|---:|---|
| `TEXT` | 1 | Text input. |
| `SINGLE_SELECT` | 2 | Single-select input. |
| `MULTI_SELECT` | 3 | Multi-select input. |
| `DATE_TIME` | 4 | Date and time input. |
| `COMMENTS` | 5 | Comments input. |
| `NUMBER` | 6 | Number input. |
| `PERSON` | 7 | Person input. |
| `TIME` | 8 | Time input. |
| `LAST_UPDATED_BY` | 9 | Last-updated-by metadata. |
| `LAST_UPDATED_AT` | 10 | Last-updated-at metadata. |
| `CHECKBOX` | 11 | Checkbox input. |
| `DATETIME` | 12 | Date-time input. |
| `DECIMAL` | 13 | Decimal input. |
| `IMAGE` | 14 | Image input. |
| `URL` | 15 | URL input. |
| `EMAIL` | 16 | Email input. |
| `FORMULA` | 17 | Formula column. |
| `BUTTON_COLUMN` | 18 | Button column. |
| `CURRENCY` | 19 | Currency input. |
| `PHONE_NUMBER` | 20 | Phone-number input. |
| `RATING` | 21 | Rating input. |
| `ATTACHMENT` | 22 | Attachment input. |
| `PERCENT_SLIDER` | 23 | Percentage slider input. |
| `PERCENT` | 24 | Percentage input. |
| `SIMULATION` | 25 | Simulation input. |
| `LONG_TEXT` | 101 | Long-text input. |

### MeasureType

`MeasureType` is a discriminated union. Exactly one of the following keys is present.

| Property | Type | Description |
|---|---|---|
| `Forecast` | [ForecastMeasure](#forecastmeasure) | Forecast measure configuration (forecast version and period). |
| `Native` | [NativeMeasure](#nativemeasure) | Native measure sourced directly from the semantic model. |
| `DataInput` | [DataInputMeasure](#datainputmeasure) | Editable data-input measure configuration. |
| `VisualColumn` | [VisualColumnMeasure](#visualcolumnmeasure) | Measure sourced from a visual column. |
| `Formula` | [FormulaMeasure](#formulameasure) | Calculated measure defined by a formula. |

### ForecastMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `forecast_version` | integer | true | Forecast version. |
| `forecast_period` | [PeriodRange](#periodrange) | true | Forecast period with start and end timestamps. |
| `closed_period_source` | string or null | false | Source for closed periods. |
| `closed_period_till` | string or null | false | End of the closed period. |
| `auto_close_forecast_settings` | object or null | false | Auto-close settings. Additional properties are allowed. |
| `forecast_value_display` | string | false | Display mode: `Actual` or `Forecast`. |
| `forecast_value_retain_blanks` | boolean | false | Whether forecast blanks are retained. |
| `open_period_config` | object | false | Open-period configuration. |
| `closed_period_config` | object | false | Closed-period configuration. |
| `forecast_allowed_user_permissions` | boolean | false | Whether forecast permissions are allowed. |
| `disable_write_access` | boolean | false | Whether write access is disabled. |
| `edit_config` | [EditConfig](#editconfig) | false | Forecast edit configuration. |

### PeriodRange

| Property | Type | Required | Description |
|---|---|---|---|
| `start` | number | true | Start of the period. |
| `end` | number | true | End of the period. |

### NativeMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `measure_role` | string | true | Role of the native measure. |

### DataInputMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Data-input measure identifier. |
| `column_type` | object | true | Column type configuration, including `Number` and other open-ended types. |
| `title` | string | true | Display title. |
| `disable_write_access` | boolean | false | Whether write access is disabled. |
| `on_change_formula` | string | false | Formula evaluated on change. |
| `allow_input` | string, object, or null | false | Input permission configuration. |

### VisualColumnMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `DataInput` | [DataInputMeasure](#datainputmeasure) | false | Data-input measure used as a visual column. |

### FormulaMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `formula` | string | true | Formula expression for the measure. |

### EditConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `aggregate_total` | string or null | false | Aggregate-total setting. |
| `allow_input` | string or null | false | Input permission setting. |
| `on_change_formula` | string or null | false | Formula evaluated on change. |
| `number_column_metadata` | object | false | Number column metadata. |

### Data Input file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/dataInput/1.0.0/schema.json",
  "columns": [
    {
      "measureGuid": "923004462102427642",
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "AC",
      "dataInputType": 6,
      "columnMeta": {
        "id": "923004462102427642",
        "label": "AC",
        "measure_type": {
          "Native": {
            "measure_role": "ACMeasure"
          }
        },
        "data_type": "Number"
      }
    },
    {
      "measureGuid": "10665923179103051",
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Forecast",
      "dataInputType": 6,
      "columnMeta": {
        "id": "10665923179103051",
        "label": "Forecast",
        "measure_type": {
          "DataInput": {
            "id": "CALC_mo3fwb7p73947027",
            "column_type": {
              "Number": {
                "min_value": null,
                "max_value": null,
                "distribute_parent_value_to_children": true,
                "default_value": null
              }
            },
            "title": "Forecast",
            "disable_write_access": false,
            "on_change_formula": "",
            "allow_input": "ReadAndEdit"
          }
        },
        "data_type": "Number"
      }
    }
  ]
}
```

## Visual Properties — Planning (`sheets/{sheetId}/visuals/{visualId}/properties.json`)

Properties configuration for a Planning visual, including pivot assignments, sorting, and filter configurations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/properties/1.0.0/schema.json`. |
| `visuals` | object | true | Map of visual ID to [VisualProperties (Planning)](#visualproperties-planning). |

### VisualProperties (Planning)

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Properties schema version. |
| `properties` | object | true | Visual-specific property bag. |
| `properties.pivotAssignments` | [PivotAssignment](#pivotassignment)[] | true | Column/row/measure assignments for the pivot table. |
| `properties.sortingConfig` | array | false | Sorting configurations. |
| `properties.superFilterAssignments` | [SuperFilterAssignment](#superfilterassignment)[] | false | Filter configurations for the visual. |

### PivotAssignment

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique identifier in `Table[Column]` format. |
| `sourceId` | string | true | Source reference ID. |
| `bucketId` | string | true | Target bucket: `rows`, `columns`, or `ameasure`. |
| `columnName` | string | true | Column name. |
| `dataType` | string | true | Data type of the column. |

### SuperFilterAssignment

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Filter assignment identifier. |
| `pivotAssignments` | [PivotAssignment](#pivotassignment)[] | true | Pivot assignments used by the filter. |
| `isDefault` | boolean | false | Whether this is the default filter. |
| `isDefaultMeasure` | boolean | false | Whether the filter applies to the default measure. |
| `isDaxMeasure` | boolean | false | Whether the filter targets a DAX measure. |
| `bucketId` | string | false | Target bucket. |
| `configuration` | [FilterConfiguration](#filterconfiguration) | false | Filter configuration. |
| `filter` | object | false | Filter state. |
| `position` | integer | false | Filter position. |
| `filterLevel` | string | false | Filter scope level. |

### FilterConfiguration

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | false | Filter name. |
| `hide` | boolean | false | Whether the filter is hidden. |
| `isExpanded` | boolean | false | Whether the filter is expanded. |
| `locked` | boolean | false | Whether the filter is locked. |
| `filterMode` | string | false | Filter mode. |
| `visualType` | string | false | Filter visual type. |
| `scale` | object | false | Filter scale settings. |
| `enableNumericToFacet` | boolean | false | Whether numeric values can be faceted. |
| `filterOperator` | string | false | Filter operator. |
| `singleSelect` | boolean | false | Whether only one value may be selected. |
| `selectAll` | boolean | false | Whether all values are selected. |
| `slider` | object | false | Slider settings. |
| `regex` | object | false | Regular-expression settings. |
| `measureSearch` | object | false | Measure search settings. |
| `alphaNumericValues` | object | false | Alphanumeric filter values. |
| `sort` | object | false | Sort settings. |
| `topN` | object | false | Top-N settings. |
| `sheetDataItemAggregationType` | string | false | Aggregation type for the sheet data item. |

### Visual Properties (Planning) file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/properties/1.0.0/schema.json",
  "visuals": {
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx": {
      "schema": "0.0.1",
      "properties": {
        "pivotAssignments": [
          {
            "id": "[TABLE[Region Hierarchy]]~|||~TABLE[REGION]",
            "sourceId": "[TABLE[Region Hierarchy]]~|||~TABLE[REGION]",
            "bucketId": "rows",
            "columnName": "REGION",
            "dataType": "String",
            "order": 0,
            "columnType": "Hierarchy Level",
            "sourceType": "PowerBI"
          }
        ],
        "sortingConfig": [],
        "superFilterAssignments": []
      }
    }
  }
}
```

## Model Template (`sheets/{sheetId}/visuals/{visualId}/modelTemplate.json`)

Configuration for dynamic row templates and their rows.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/Planning/ModelTemplate/1.0.0/schema.json`. |
| `properties` | [ModelTemplateProperties](#modeltemplateproperties) | true | Dynamic row template properties. |

### ModelTemplateProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `dynamicRowTemplates` | [DynamicRowTemplate](#dynamicrowtemplate)[] | true | Dynamic row templates. |

### DynamicRowTemplate

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique template identifier. |
| `title` | string | true | Display title of the template. |
| `rows` | [ModelTemplateRow](#modeltemplaterow)[] | true | Rows in the template. |

### ModelTemplateRow

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Row identifier. |
| `row_type` | [ModelTemplateRowType](#modeltemplaterowtype) | true | Static row type configuration. |
| `title` | string | true | Row title. |
| `include_in_total` | boolean | true | Whether the row is included in totals. |
| `parent_id` | string | true | Parent row identifier. |
| `level` | integer | true | Hierarchy level. |
| `previous_row_id` | string | true | Previous row identifier. |
| `disabled` | boolean | true | Whether the row is disabled. |
| `scaling_factor` | string or null | false | Display scaling factor. |
| `bind_for_cross_filter` | string, object, or null | false | Cross-filter binding setting. |
| `description` | string or null | false | Row description. |
| `column_aggregation` | string or null | false | Aggregation: `Sum`, `Average`, `Min`, `Max`, or `Count`. |

### ModelTemplateRowType

| Property | Type | Required | Description |
|---|---|---|---|
| `StaticRow` | [ModelTemplateStaticRow](#modeltemplatestaticrow) | true | Static row configuration. |

### ModelTemplateStaticRow

| Property | Type | Required | Description |
|---|---|---|---|
| `distribute_parent_value_to_child` | boolean | true | Whether the parent value is distributed to children. |
| `default_value` | [ModelTemplateDefaultValue](#modeltemplatedefaultvalue) | true | Default row value. |
| `row_edit_mode` | string or null | true | Row editing mode. |

### ModelTemplateDefaultValue

| Property | Type | Required | Description |
|---|---|---|---|
| `Static` | string | true | Static default value. |

## Writeback Configuration (`sheets/{sheetId}/visuals/{visualId}/writeback.json`)

Writeback configuration for a Planning visual, defining destination, column mapping, and auto-writeback settings.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/writeback/1.0.0/schema.json`. |
| `writebackType` | integer | true | Writeback type code. See [Writeback table type](#writeback-table-type). |
| `destinations` | [WritebackDestination](#writebackdestination)[] | true | List of writeback destinations. |
| `writebackFilter` | object | false | Filter type: `none`, `filter`, or `calculatedRows`. |
| `excludedMeasureGuids` | string[] | false | Measure GUIDs excluded from writeback. |
| `isAutoWritebackEnabled` | integer | false | Auto-writeback enabled status code. |
| `autoWbEnabledScenarioIds` | string[] | false | Scenario IDs with auto-writeback enabled. |
| `debounce` | [DebounceConfig](#debounceconfig) | false | Debounce duration and enabled status. |
| `isSnapshotWbEnabled` | integer | false | Snapshot writeback enabled status code. |
| `wbTableColumnMapping` | object | false | Map of measure GUIDs to writeback column names. |
| `numberPrecision` | object | false | Decimal precision configuration. |
| `stringColumnLength` | object | false | String column length configuration. |
| `writebackAsHTML` | boolean | false | Whether to write back formatted HTML. |
| `skippedDimensionIds` | string[] | false | Dimension IDs skipped during writeback. |

### Writeback table type

| Name | Value | Description |
|---|---:|---|
| `PIVOT_LONG` | 2 | Long table layout (`Long`). |
| `PIVOT_WIDE` | 3 | Wide table layout (`Wide`). |
| `DELTA_ONLY` | 4 | Long table with changes (`Long with Changes`). |
| `DELTA_WIDE` | 5 | Wide table with changes (`Wide with Changes`). |

### DebounceConfig

Debounce configuration for writeback operations.

| Property | Type | Required | Description |
|---|---|---|---|
| `duration` | integer | false | Debounce duration in seconds. |
| `isDebounceEnabled` | integer | false | Debounce enabled status code. |

### Writeback Configuration file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/writeback/1.0.0/schema.json",
  "writebackType": 3,
  "writebackFilter": { "type": "none" },
  "excludedMeasureGuids": [],
  "isAutoWritebackEnabled": 20,
  "autoWbEnabledScenarioIds": [],
  "debounce": { "duration": 5, "isDebounceEnabled": 10 },
  "isSnapshotWbEnabled": 20,
  "wbTableColumnMapping": {},
  "numberPrecision": { "decimal": 2 },
  "stringColumnLength": { "type": 1, "length": "512" },
  "writebackAsHTML": false,
  "skippedDimensionIds": [],
  "destinations": [
    {
      "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "databaseId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "tableName": "wb_1a_budget_detail",
      "schema": "dbo"
    }
  ]
}
```

## Insert Rows (`sheets/{sheetId}/visuals/{visualId}/insertRows.json`)

Custom (inserted) rows for a Planning visual, including static rows and calculated rows. The file is an object containing a `rows` array.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/insertRows/1.0.0/schema.json`. |
| `rows` | [InsertRow](#insertrow)[] | true | List of inserted row definitions. |

### InsertRow

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique row identifier. |
| `visualId` | string (uuid) | true | ID of the visual this row belongs to. |
| `rowMeta` | [RowMeta](#rowmeta) | true | Row type and configuration (static, calculated, or data-bound). |
| `name` | string | true | Display name of the row. |
| `dimensionId` | string | true | Dimension this row belongs to. |
| `visualRowConfigId` | string or null | false | Optional visual row configuration reference. |
| `rowPath` | string | false | Hierarchical path for the row. |
| `derivedFromRowId` | string | false | Source row ID when row is derived. |

### RowMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique row meta identifier. |
| `row_type` | [RowType](#rowtype) | true | Discriminated union of row types (`StaticRow` or `CalculatedRow`). Exactly one key is present. |
| `title` | string | true | Display title of the row. |
| `scaling_factor` | string | false | Scaling factor for display (for example, `Auto`). |
| `include_in_total` | boolean | false | Whether the row is included in totals. |
| `parent_id` | string | false | Parent row ID for hierarchy. |
| `level` | integer | false | Hierarchy level of the row. |
| `previous_row_id` | string | false | ID of the preceding row for ordering. |
| `disabled` | boolean | false | Whether the row is disabled. |
| `bind_for_cross_filter` | boolean or null | false | Whether the row is bound for cross-filtering. |
| `description` | string or null | false | Optional description. |
| `column_aggregation` | string | false | Aggregation applied to the row: `Sum`, `Average`, `Min`, `Max`, or `Count`. |

### RowType

Exactly one row type key is present.

| Property | Type | Required | Description |
|---|---|---|---|
| `StaticRow` | [StaticRowType](#staticrowtype) | No* | Manually entered row configuration. |
| `CalculatedRow` | [CalculatedRowType](#calculatedrowtype) | No* | Formula-driven row configuration. |
| `ForecastRow` | object | No* | Forecast row configuration. The schema permits additional properties without defining fields. |
| `PercentContributionRow` | object or string | No* | Percent-contribution row configuration. |

### StaticRowType

| Property | Type | Required | Description |
|---|---|---|---|
| `distribute_parent_value_to_child` | boolean | false | Whether the parent value is distributed to children. |
| `default_value` | object | false | Default value configuration. |
| `row_edit_mode` | string or null | false | Row editing mode. |

### CalculatedRowType

| Property | Type | Required | Description |
|---|---|---|---|
| `formula` | string or object | false | Calculation formula. |
| `description` | string | false | Row description. |
| `include_in_chart` | boolean | false | Whether the row is included in charts. |
| `deferred` | boolean, string, or null | false | Deferred calculation setting. |
| `bind_for_cross_filter` | boolean | false | Whether the row participates in cross-filtering. |

### Insert Rows file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/insertRows/1.0.0/schema.json",
  "rows": [
    {
      "id": "1601791389093017127",
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "All",
      "dimensionId": "2463189626347903050",
      "rowMeta": {
        "id": "1601791389093017127",
        "row_type": {
          "CalculatedRow": {
            "formula": "R_76501857723870373699+R_64544306971566108700",
            "description": "",
            "include_in_chart": false,
            "bind_for_cross_filter": false
          }
        },
        "title": "All",
        "include_in_total": true,
        "level": 0,
        "disabled": false,
        "column_aggregation": "Sum"
      }
    }
  ]
}
```

## Scenarios (`sheets/{sheetId}/visuals/{visualId}/scenarios.json`)

Scenario definitions for a Planning visual. The file is an object containing a `scenarios` array.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/scenarios/1.0.0/schema.json`. |
| `scenarios` | [Scenario](#scenario)[] | true | List of scenario definitions. |

### Scenario

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the scenario. |
| `status` | string | true | Current status: `ACTIVE` or `LOCK`. |
| `meta` | [ScenarioMeta](#scenariometa) | true | Scenario metadata including measure IDs, GUID, and order. |
| `autoWritebackEnabled` | string | false | Whether auto-writeback is enabled: `ACTIVE` or `INACTIVE`. |
| `simulations` | array or null | false | List of simulations associated with this scenario. |

### ScenarioMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `measureIds` | string[] | true | List of measure IDs associated with the scenario. |
| `scenarioGuid` | string | true | Unique GUID for the scenario. |
| `order` | integer | true | Display order of the scenario. |
| `dimensionHash` | string | false | Hash representing the dimension configuration. |

### Scenarios file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/scenarios/1.0.0/schema.json",
  "scenarios": [
    {
      "name": "Scenario 1",
      "status": "ACTIVE",
      "meta": {
        "measureIds": ["1520862549008044604"],
        "scenarioGuid": "88480882635127202",
        "order": 1,
        "dimensionHash": "8018ac06b2278797"
      },
      "simulations": [
        {
          "measure_simulations": {
            "default_filter_context_hash": {}
          }
        }
      ]
    }
  ]
}
```

## PowerTable Column Configs (`sheets/{sheetId}/visuals/{visualId}/columnConfigs.json`)

Array of column configuration definitions for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `columnGuid` | string | true | Unique identifier for the column. |
| `columnName` | string | true | Database column name. |
| `columnType` | integer | true | Column type code. See [ColumnType](#columntype) |
| `columnMeta` | [PowerTableColumnMeta](#powertablecolumnmeta) | true | Column metadata including validation, defaults, and database metadata. |
| `displayName` | string | true | User-facing column name. |
| `hideColumn` | integer (0 or 1) | false | Whether the column is hidden. |
| `mandatory` | integer (0 or 1) | false | Whether the column is required. |
| `allowEdit` | integer (0 or 1) | false | Whether the column is editable. |
| `visualColumnType` | integer | false | Visual representation type code. See [VisualColumnType](#visualcolumntype) |
| `description` | string | false | Column description. |

> Each item in the array represents one column configuration. The `$schema` field may be present on each item.

### ColumnType
| Name           | Value |
|----------------|-------|
| TEXT           | 1     |
| SINGLE_SELECT  | 2     |
| MULTI_SELECT   | 3     |
| DATE           | 4     |
| NUMBER         | 6     |
| PERSON         | 7     |
| CHECKBOX       | 11    |
| DATETIME       | 12    |
| DECIMAL        | 13    |
| IMAGE          | 14    |
| URL            | 15    |
| EMAIL          | 16    |
| FORMULA        | 17    |
| BUTTON_COLUMN  | 18    |
| CURRENCY       | 19    |
| PHONE_NUMBER   | 20    |
| RATING         | 21    |
| ATTACHMENT     | 22    |
| PERCENT_SLIDER | 23    |
| PERCENT        | 24    |

### VisualColumnType
| Name              | Value |
|-------------------|-------|
| BASE              | 1     |
| SINGLE_SELECT     | 3     |
| MULTI_SELECT      | 4     |
| REFERENCE_COLUMN  | 5     |
| FORMULA           | 6     |
| ALTER_TABLE       | 7     |
| RELATION_COLUMN   | 8     |
| BUTTON_COLUMN     | 9     |
| ATTACHMENT        | 10    |
| ROLLUP            | 11    |

### PowerTableColumnMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `defaultValueType` | string | false | Default value mode: `NONE`, `STATIC`, `FORMULA`, or `AUTO_INCREMENT`. |
| `defaultValue` | string or null | false | Default column value. |
| `isPrimaryKey` | boolean | false | Whether the column is a primary key. |
| `updateValueOnRowModify` | boolean | false | Whether the value updates when a row changes. |
| `resetToDefaultOnRowModify` | boolean | false | Whether the value resets on row modification. |
| `maximumAllowedLength` | string | false | Maximum allowed text length. |
| `textFieldColumnType` | string | false | Text subtype: `Any Value`, `Email`, `URL`, or `Phone`. |
| `options` | object[] | false | Static dropdown options. |
| `selectionMethod` | string | false | Option source: `DISTINCT_VALUES`, `LOAD_FROM_DATABASE`, or `STATIC`. |
| `optionLinking` | [OptionLinking](#optionlinking) | false | Lookup and filter linking configuration. |
| `isFilterBasedOnAnotherValue` | boolean | false | Whether filtering depends on another value. |
| `allowNegative` | boolean | false | Whether negative values are allowed. |
| `dbMeta` | any | false | Database metadata; shape is open in the schema. |
| `largeNumberAbbrevation` | string or boolean | false | Large-number abbreviation setting. |
| `max` | any | false | Maximum value; shape is open in the schema. |
| `min` | any | false | Minimum value; shape is open in the schema. |
| `negativeValueFormat` | string | false | Negative value format. |
| `prefix` | string | false | Value prefix. |
| `suffix` | string | false | Value suffix. |
| `thousandSeparator` | string or boolean | false | Thousand separator setting. |

### OptionLinking

| Property | Type | Required | Description |
|---|---|---|---|
| `lookupConfig` | [LookupConfig](#lookupconfig)[] | false | Lookup table configurations. |
| `filters` | object[] | false | Linking filters. Item shape is open in the schema. |

### LookupConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | false | Lookup schema. |
| `table` | string | true | Lookup table name. |
| `label` | string | true | Display label column. |
| `id` | string | true | Value column. |
| `order` | integer | false | Lookup ordering. |
| `linkingColumn` | string | false | Main-table linking column. |

### PowerTable Column Configs file example

```json
[
  {
    "columnGuid": "Id",
    "columnName": "Id",
    "columnType": 6,
    "columnMeta": {
      "isIdentity": true,
      "isPrimaryKey": true,
      "defaultValueType": "NONE",
      "defaultValue": "",
      "dbMeta": {
        "type": "bigint",
        "isNullable": false,
        "isPrimaryKey": false,
        "isIdentity": true,
        "maxLength": 8
      }
    },
    "visualColumnType": 1,
    "allowEdit": 0,
    "mandatory": 1,
    "hideColumn": 0,
    "displayName": ""
  }
]
```

## PowerTable Properties (`sheets/{sheetId}/visuals/{visualId}/properties.json`)

Properties definition for a PowerTable visual, including assignments, filters, styles, and visual state.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/properties/1.0.0/schema.json`. |
| `properties` | [PowerTableProperties](#powertableproperties) | true | PowerTable visual properties. |

### PowerTableProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `pivotAssignments` | array | true | Column/row assignments. |
| `sortingConfig` | array | true | Sorting configurations. |
| `superFilterAssignments` | [PowerTableSuperFilterAssignment](#powertablesuperfilterassignment)[] | true | Filter configurations including filter state and pivot assignments. |
| `visualState` | object | true | Visual display state. |
| `visualInteractions` | object | false | Cross-visual interaction settings. |
| `dimension` | [Dimension](#dimension-visual) | false | Visual dimension (width and height). |
| `position` | [Position](#position) | false | Visual position on canvas (x and y). |
| `visualStyles` | object | false | Style overrides. |
| `groupName` | string | false | Group name for the visual. |
| `visualType` | integer | false | Visual type code. |
| `chartType` | string | false | Chart type identifier. |
| `mobileProperties` | object | false | Mobile layout properties. |

### PowerTableSuperFilterAssignment

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Filter assignment identifier. |
| `filter` | object | true | Filter state. |
| `position` | integer | true | Filter position. |
| `configuration` | object | true | Filter configuration. |
| `filterLevel` | string | true | Filter scope level. |
| `pivotAssignments` | [PowerTablePivotAssignment](#powertablepivotassignment)[] | true | Pivot assignments for the filter. |

### PowerTablePivotAssignment

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Assignment identifier. |
| `sourceId` | string | true | Source identifier. |
| `name` | string | true | Display name. |
| `columnName` | string | true | Column name. |
| `tableName` | string | true | Table name. |
| `order` | integer | true | Assignment order. |
| `bucketId` | string | false | Target bucket. |
| `dataType` | string | true | Column data type. |
| `columnType` | string | true | Column type. |
| `sourceType` | string | true | Source type. |

### Dimension (Visual)

Visual dimension configuration.

| Property | Type | Required | Description |
|---|---|---|---|
| `width` | integer | false | Width of the visual in pixels. |
| `height` | integer | false | Height of the visual in pixels. |

### Position

Position configuration for a visual on canvas.

| Property | Type | Required | Description |
|---|---|---|---|
| `x` | integer | false | X coordinate position. |
| `y` | integer | false | Y coordinate position. |

## PowerTable Source (`sheets/{sheetId}/visuals/{visualId}/source.json`)

Database source configuration for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/source/1.0.0/schema.json`. |
| `connection` | [ConnectionReferenceOrVar](#connectionreferenceorvar) | true | Fabric connection reference. |
| `database` | [ItemReferenceOrVar](#itemreferenceorvar) | true | Fabric item reference to the database. |
| `schema` | string | true | Database schema name (for example, `dbo`). |
| `tableName` | string | true | Database table name. |

### PowerTable Source file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/source/1.0.0/schema.json",
  "connection": {
    "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  },
  "database": {
    "workspaceId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "itemId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  },
  "schema": "dbo",
  "tableName": "detail_planning_using_powertable_new"
}
```

## PowerTable Settings (`sheets/{sheetId}/visuals/{visualId}/sourceSettings.json`)

Settings and permission configurations for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/settings/1.0.0/schema.json`. |
| `settings` | [Setting](#setting)[] | true | List of setting configurations. |

### Setting

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Setting name: `ROW_ADD`, `ROW_UPDATE`, `ROW_DELETE`, `ROW_IDENTIFIER`, `COMMENT_SETTINGS`, or `SCD`. |
| `accessType` | string | false | Access scope: `ALL_USERS` or `SPECIFIC_USERS`. |
| `meta` | object | false | Setting metadata (for example, `enabled` boolean). |
| `rules` | [AccessRule](#accessrule)[] | false | Access rules with user/filter targeting. |
| `settings` | object | false | Setting-specific configuration payload. |

### AccessRule

| Property | Type | Required | Description |
|---|---|---|---|
| `ruleId` | string | true | Rule identifier. |
| `ruleName` | string | true | Rule name. |
| `filter` | object | false | Rule filter. |
| `filterUsers` | string[] | false | Users targeted by the rule. |

### PowerTable Settings file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/settings/1.0.0/schema.json",
  "settings": [
    {
      "name": "SCD",
      "settings": { "type": 2, "enabled": false }
    },
    {
      "name": "COMMENT_SETTINGS",
      "settings": {
        "notification": true,
        "rowLevelComments": false,
        "toggleAddonColumns": false,
        "displayComment": true
      }
    },
    {
      "name": "ROW_ADD",
      "settings": { "enabled": true },
      "rules": [
        { "ruleId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "ruleName": "all_users", "filter": {} }
      ]
    },
    {
      "name": "ROW_UPDATE",
      "settings": { "enabled": true },
      "rules": [
        { "ruleId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", "ruleName": "all_users", "filter": {} }
      ]
    },
    {
      "name": "ROW_DELETE",
      "settings": { "enabled": false }
    }
  ]
}
```

## PowerTable Approvals (`sheets/{sheetId}/visuals/{visualId}/approvals.json`)

Approval workflow configuration for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/approvals/1.0.0/schema.json`. |
| `ruleType` | integer | true | Approval rule type: `DEFAULT` = 10, `MANUAL` = 20, or `LOOKUP` = 30. See [PowerTableApprovalType](#powertableapprovaltype). |
| `persistFlag` | integer | false | Persistence behavior: `SAVE_AFTER_REVIEW` = 10 or `SAVE_BEFORE_REVIEW` = 20. See [ApprovalPersistFlag](#approvalpersistflag). |
| `settings` | object | false | Approval-specific settings payload. |
| `approvalLevel` | integer | false | Current or default approval level. |
| `multiLevelEnabled` | integer (0 or 1) | false | Whether multi-level approvals are enabled. |
| `approvalLevels` | [ApprovalLevel](#approvallevel)[] | false | Configured approval levels (name, description, level). |
| `approvalFilter` | [ApprovalFilter](#approvalfilter)[] | false | Filters applied to approval routing. |

### PowerTableApprovalType

Numeric values for `approvals.json.ruleType`.

| Name | Value | Description |
|---|---:|---|
| `DEFAULT` | 10 | Default approval behavior. |
| `MANUAL` | 20 | Manual approval behavior. |
| `LOOKUP` | 30 | Lookup-based approval behavior. |

### ApprovalPersistFlag

Numeric values for `approvals.json.persistFlag`.

| Name | Value | Description |
|---|---:|---|
| `SAVE_AFTER_REVIEW` | 10 | Save after review. |
| `SAVE_BEFORE_REVIEW` | 20 | Save before review. |

### ApprovalLevel

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Approval level name. |
| `description` | string | true | Approval level description. |
| `level` | integer | true | Approval level number. |

### ApprovalFilter

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Filter name. |
| `filter` | object | true | Routing filter. |
| `order` | integer | true | Filter order. |

## PowerTable Automations (`sheets/{sheetId}/visuals/{visualId}/automations.json`)

Array of automation definitions for a PowerTable visual, defining triggers and action flows.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/automations/1.0.0/schema.json`. |
| `automations` | [Automation](#automation)[] | true | List of automation definitions. |

### Automation

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the automation. |
| `triggerType` | integer | true | Automation trigger code. See [AutomationTriggerType](#automationtriggertype). |
| `config` | [AutomationConfig](#automationconfig) | true | Automation configuration including trigger, entry group, and action groups. |

### AutomationTriggerType

Numeric values for `automations.json` automation triggers.

| Name | Value | Description |
|---|---:|---|
| `ROW_CREATED` | 1 | A row was created. |
| `ROW_UPDATED` | 2 | A row was updated. |
| `ROW_DELETED` | 3 | A row was deleted. |
| `RECORD_MATCHES_CONDITION` | 4 | A record matched a condition. |
| `SCHEDULED` | 5 | A scheduled trigger. |
| `FORM_SUBMITTED` | 6 | A form was submitted. |
| `BUTTON_CLICKED` | 7 | An automation button was clicked. |

### AutomationConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Automation name. |
| `trigger` | [Trigger](#trigger) | true | Automation trigger. |
| `entryGroupId` | string | true | ID of the first action group. |
| `groups` | object | true | Map of group IDs to [ActionGroup](#actiongroup) objects. |
| `version` | any | false | Automation version; shape is open in the schema. |

### Trigger

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Trigger identifier. |
| `triggerType` | string or integer | true | Trigger type code. Use the values in [AutomationTriggerType](#automationtriggertype); the schema permits a string representation in this nested trigger object. |
| `description` | string | false | Trigger description. |
| `triggerConfig` | object | false | Trigger conditions and other settings. |

### ActionGroup

| Property | Type | Required | Description |
|---|---|---|---|
| `groupId` | string | true | Group identifier. |
| `groupType` | string | true | Group type code. |
| `previousGroupId` | string or null | false | Previous group identifier. |
| `nextGroupId` | string or null | false | Next group identifier. |
| `entryActionId` | string | true | First action identifier. |
| `actions` | object | true | Map of action IDs to [Action](#action) objects. |
| `position` | integer | false | Group position. |

### Action

| Property | Type | Required | Description |
|---|---|---|---|
| `actionId` | string | true | Action identifier. |
| `actionType` | string | true | Action type code. |
| `previousActionId` | string or null | false | Previous action identifier. |
| `nextActionId` | string or null | false | Next action identifier. |
| `config` | object | true | Action-specific configuration. |

## PowerTable Forms (`sheets/{sheetId}/visuals/{visualId}/forms.json`)

Array of form definitions for a PowerTable visual, defining data entry layouts.

| Property | Type | Required | Description |
|---|---|---|---|
| `title` | string | true | Form title. |
| `layoutMeta` | [LayoutMeta](#layoutmeta) | true | Layout definition including children elements and type. |
| `description` | string | false | Optional form description. |
| `config` | [FormConfig](#formconfig) | false | Form behavior configuration. |

### LayoutMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `children` | [FormElement](#formelement)[] | true | Ordered list of form elements. |
| `type` | string | true | Layout type: `form`. |
| `id` | string | false | Optional layout ID. |
| `layoutType` | string | false | Layout style: `default`, `tabs`, or `sections`. |

### FormElement

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string | true | Element type: `field`, `section`, or `tab`. |
| `name` | string | true | Field or column name. |
| `mandatory` | integer | false | Whether the element is mandatory (`0` or `1`). |
| `allowEdit` | integer | false | Whether the element is editable (`0` or `1`). |
| `props` | object | false | Element-specific properties. |

### FormConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `showTitle` | boolean | false | Whether the form title is shown. |
| `showLogo` | boolean | false | Whether the form logo is shown. |
| `submissionMessage` | string | false | Message shown after submission. |
| `fieldLabel` | string | false | Field label layout: `stacked` or `inline`. |

## Intelligence Properties (`sheets/{sheetId}/visuals/{visualId}/properties.json`)

Properties definition for an Intelligence sheet visual, including page-level settings, variables, canvas styles, commentary, and embedded visual configurations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/intelligence/properties/1.0.0/schema.json`. |
| `properties` | [PageProperties](#pageproperties) | true | Page-level properties wrapper (schema version and settings). |
| `visuals` | [Visual](#visual)[] | true | List of visuals on the Intelligence sheet. |

### PageProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Schema version for the properties format. |
| `properties` | [PageSettings](#pagesettings) | true | Page-level settings including filters, variables, canvas styles, and commentary. |

### PageSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `pageLevelFilterAssignments` | array | false | Page-level filter assignments. |
| `entityLevelVariables` | [EntityVariable](#entityvariable)[] | false | Entity-level calculated variables (actions, numbers, dropdowns). |
| `filterPanePosition` | string | false | Filter pane position: `LEFT`, `RIGHT`, `TOP`, or `BOTTOM`. |
| `topPositionFilterExpandConfig` | [FilterExpandConfig](#filterexpandconfig) | false | Filter pane expand configuration. |
| `commentary` | [Commentary](#commentary) | false | Notes and annotation settings. |
| `canvasStyle` | [CanvasStyle](#canvasstyle) | false | Canvas dimension, background, wallpaper, border, and shadow styles. |
| `assignmentColumnMap` | object | false | Map of column assignments. |
| `visualGroupMap` | object | false | Map of visual groups. |
| `sourceVisualsMeta` | object | false | Metadata for source visuals. |
| `controlPanePosition` | string | false | Position of the control pane: `left` or `right`. |

### EntityVariable

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Variable identifier. |
| `label` | string | true | User-facing label. |
| `type` | string | true | Variable type. |
| `scope` | string | true | Variable scope. |
| `technicalName` | string | true | Technical variable name. |
| `description` | string | false | Variable description. |
| `showInViewMode` | boolean | false | Whether the variable is shown in view mode. |
| `assignInterface` | string | false | Assignment interface. |
| `allowDecimalValues` | boolean | false | Whether decimal values are allowed. |
| `interfaceType` | string | false | Interface type. |
| `value` | any | false | Current value. |
| `defaultValue` | any | false | Default value. |
| `options` | array | false | Available options. |
| `min` | number | false | Minimum value. |
| `max` | number | false | Maximum value. |
| `interval` | number | false | Slider interval. |

### FilterExpandConfig

Filter pane expand configuration.

| Property | Type | Required | Description |
|---|---|---|---|
| `isFilterPaneCollapsed` | boolean | false | Whether the filter pane is initially collapsed. |
| `expandedHeight` | integer | false | Expanded height of the filter pane in pixels. |

### Visual

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique visual identifier. |
| `visualType` | integer | true | Numeric visual type code. |
| `properties` | [VisualProperties](#visualproperties-intelligence) | true | Visual properties wrapper containing schema version, visual config, and etag. |
| `isEmbedded` | boolean | false | Whether the visual is embedded. |
| `originEntityId` | integer or string | false | Origin entity ID. |

### VisualProperties (Intelligence)

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Visual properties schema version. |
| `properties` | [VisualConfig](#visualconfig) | true | Visual configuration including pivot assignments, filters, styles, dimensions, and internal state. |
| `etag` | string | false | ETag for concurrency control. |

### VisualConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `pivotAssignments` | [PivotAssignment](#pivotassignment)[] | false | Column/row/measure assignments for the visual. |
| `sortingConfig` | array | false | Sorting configurations. |
| `superFilterAssignments` | array | false | Visual-level filter assignments. |
| `chartType` | string | false | Chart type identifier (for example, `COLUMN_VERTICAL`, `LINE`, `PIE`). |
| `groupName` | string | false | Group name for the visual. |
| `visualType` | integer | false | Visual type code. |
| `dimension` | [Dimension](#dimension-visual) | false | Visual dimension (width and height). |
| `position` | [Position](#position) | false | Visual position on canvas (x and y). |
| `visualStyles` | object | false | Style overrides (background, border, corner radius, padding, shadow, tooltip). |
| `visualState` | object | false | Internal visual state, including hidden properties stored as stringified JSON. |
| `visualInteractions` | object | false | Cross-visual interaction settings. |
| `mobileProperties` | object | false | Mobile layout properties. |

### Intelligence Properties file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/intelligence/properties/1.0.0/schema.json",
  "properties": {
    "schema": "0.0.1",
    "properties": {
      "pageLevelFilterAssignments": [],
      "entityLevelVariables": [],
      "filterPanePosition": "RIGHT",
      "canvasStyle": {
        "dimension": {
          "type": "DEFAULT_16_9",
          "width": 1600,
          "height": 900,
          "elementScalingUnit": "percentage"
        }
      },
      "controlPanePosition": "right"
    }
  },
  "visuals": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "visualType": 1,
      "isEmbedded": false,
      "properties": {
        "schema": "0.0.1",
        "properties": {
          "chartType": "COLUMN_VERTICAL",
          "pivotAssignments": [
            {
              "id": "[TABLE[Region Hierarchy]]~|||~TABLE[REGION]",
              "sourceId": "[TABLE[Region Hierarchy]]~|||~TABLE[REGION]",
              "bucketId": "rows",
              "columnName": "REGION",
              "dataType": "String"
            }
          ],
          "sortingConfig": [],
          "superFilterAssignments": [],
          "dimension": { "width": 400, "height": 300 },
          "position": { "x": 0, "y": 0 }
        }
      }
    }
  ]
}
```


