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
| `semanticModelReference` | SemanticModelReference | true | Reference to the semantic model backing this plan. |
| `sheets` | SheetReference[] | true | List of sheets in the plan. |

### SemanticModelReference

| Property | Type | Required | Description |
|---|---|---|---|
| `connection` | ConnectionReferenceOrVar | No* | Fabric connection reference. Required when using the portable format (with `semanticModel`). |
| `semanticModel` | ItemReferenceOrVar | No* | Fabric item reference to the semantic model. Required when using the portable format. |
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

Either an inline connection reference or a reference to a [variable library](variable-library-definition.md) variable of type `ConnectionReference`.

| Property | Type | Required | Description |
|---|---|---|---|
| `connectionId` | string (uuid) | true | The ID of the connection. |

### ItemReferenceOrVar

Either an inline item reference or a reference to a [variable library](variable-library-definition.md) variable of type `ItemReference`.

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
| `workloadModeConfig` | WorkloadModeConfig | true | Panel visibility configuration per workload mode. |
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
| `pageGroupingMeta` | PageGroupingMeta | false | Page grouping metadata. |

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
| `cubePartitionMeasures` | CubePartitionMeasure[] | true | List of measures associated with cube partitions. |
| `cubePartitionMeasureMappings` | CubePartitionMeasureMapping[] | true | Mappings between cube partitions and measures. |
| `cubePartitionMeasureDataInputColumnMappings` | CubePartitionMeasureDataInputColumnMapping[] | true | Mappings from cube partition measures to data input columns. |

### CubePartition

| Property | Type | Required | Description |
|---|---|---|---|
| `recordGuid` | string (uuid) | true | Unique identifier for the cube partition. |
| `name` | string | true | Display name of the partition. |
| `dimensions` | Dimension[] | true | List of dimension definitions. |
| `timeDimensions` | TimeDimension[] | true | List of time dimension definitions. |
| `measures` | Measure[] | true | List of measure definitions. |
| `rowCount` | integer | true | Number of rows in the partition. |
| `id` | integer | false | Internal numeric ID. |
| `status` | string | false | Status code (`ACTIVE`). |

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
| `queryGroups` | QueryGroup[] | false | Optional groupings of queries for organizational purposes. |

### Source

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the source. |
| `type` | string or integer | true | Source type. String values: `PLANNING`, `PARQUET`, `APPEND`, `MERGE`, `CSV`, `JSON`, `XLSX`, `SQL_SOURCE`, `JOIN`, `ENCRYPTED_PARQUET`, `EDITABLE`. |
| `visualId` | integer or string (uuid) | false | Visual identifier this source is associated with. |
| `meta` | SourceMeta or string | false | Source metadata. |
| `queries` | [Query](#query)[] | false | List of queries for this source. |
| `dependentQueries` | string[] | false | List of dependent query GUIDs for join sources. |

### Query

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the query. |
| `queryId` | string | true | Unique GUID for this query. |
| `type` | string or integer | false | Query type code. |
| `visualId` | integer or string (uuid) | false | Visual identifier. |
| `meta` | object | false | Query-level metadata. |
| `transformationSteps` | TransformationStep[] | false | Ordered list of transformation steps. |
| `writebackSettings` | WritebackSettings | false | Writeback settings for this query. |
| `writebackDestinations` | WritebackDestination[] | false | List of writeback destinations. |

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
| `topPositionFilterExpandConfig` | TopPositionFilterExpandConfig | true | Expand configuration for top-positioned filter pane. |
| `commentary` | Commentary | true | Notes and annotation settings. |
| `canvasStyle` | CanvasStyle | true | Canvas dimensions, background, wallpaper, border, and shadow styles. |
| `assignmentColumnMap` | object | true | Map of column assignments. |
| `visualGroupMap` | object | true | Map of visual groups. |
| `sourceVisualsMeta` | object | true | Metadata for source visuals. |
| `controlPanePosition` | string | true | Position of the control pane. |

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
| `commentIndicatorDisplay` | CommentIndicatorDisplay | false | Visual indicator configuration (type, pixel size, position). |

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
| `dataInputType` | integer | true | Numeric code for the data input type (1=text, 6=number, etc.). |
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

### MeasureType

`MeasureType` is a discriminated union. Exactly one of the following keys is present.

| Property | Type | Description |
|---|---|---|
| `Forecast` | ForecastMeasure | Forecast measure configuration (forecast version and period). |
| `Native` | NativeMeasure | Native measure sourced directly from the semantic model. |
| `DataInput` | DataInputMeasure | Editable data-input measure configuration. |
| `VisualColumn` | VisualColumnMeasure | Measure sourced from a visual column. |
| `Formula` | FormulaMeasure | Calculated measure defined by a formula. |

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
| `visuals` | object | true | Map of visual ID to VisualProperties. |

### VisualProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Properties schema version. |
| `properties` | object | true | Visual-specific property bag. |
| `properties.pivotAssignments` | [PivotAssignment](#pivotassignment)[] | true | Column/row/measure assignments for the pivot table. |
| `properties.sortingConfig` | array | false | Sorting configurations. |
| `properties.superFilterAssignments` | SuperFilterAssignment[] | false | Filter configurations for the visual. |

### PivotAssignment

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique identifier in `Table[Column]` format. |
| `sourceId` | string | true | Source reference ID. |
| `bucketId` | string | true | Target bucket: `rows`, `columns`, or `ameasure`. |
| `columnName` | string | true | Column name. |
| `dataType` | string | true | Data type of the column. |

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

## Writeback Configuration (`sheets/{sheetId}/visuals/{visualId}/writeback.json`)

Writeback configuration for a Planning visual, defining destination, column mapping, and auto-writeback settings.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/writeback/1.0.0/schema.json`. |
| `writebackType` | integer | true | Writeback type code (2=standard, 3=cube). |
| `destinations` | WritebackDestination[] | true | List of writeback destinations. |
| `writebackFilter` | object | false | Filter type: `none`, `filter`, or `calculatedRows`. |
| `excludedMeasureGuids` | string[] | false | Measure GUIDs excluded from writeback. |
| `isAutoWritebackEnabled` | integer | false | Auto-writeback enabled status code. |
| `autoWbEnabledScenarioIds` | string[] | false | Scenario IDs with auto-writeback enabled. |
| `debounce` | DebounceConfig | false | Debounce duration and enabled status. |
| `isSnapshotWbEnabled` | integer | false | Snapshot writeback enabled status code. |
| `wbTableColumnMapping` | object | false | Map of measure GUIDs to writeback column names. |
| `numberPrecision` | object | false | Decimal precision configuration. |
| `stringColumnLength` | object | false | String column length configuration. |
| `writebackAsHTML` | boolean | false | Whether to write back formatted HTML. |
| `skippedDimensionIds` | string[] | false | Dimension IDs skipped during writeback. |

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
| `row_type` | RowType | true | Discriminated union of row types (`StaticRow` or `CalculatedRow`). Exactly one key is present. |
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

`StaticRow` configures a manually entered row (`distribute_parent_value_to_child`, `default_value`, `row_edit_mode`). `CalculatedRow` configures a formula-driven row (`formula`, `include_in_chart`, `deferred`, `bind_for_cross_filter`).

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
| `columnType` | integer | true | Column type code (1=numeric, 2=single select, 3=multi select, 4=date, 6=identity, etc.). |
| `columnMeta` | ColumnMeta | true | Column metadata including validation, defaults, and database metadata. |
| `displayName` | string | true | User-facing column name. |
| `hideColumn` | integer (0 or 1) | false | Whether the column is hidden. |
| `mandatory` | integer (0 or 1) | false | Whether the column is required. |
| `allowEdit` | integer (0 or 1) | false | Whether the column is editable. |
| `visualColumnType` | integer | false | Visual representation type code. |
| `description` | string | false | Column description. |

> Each item in the array represents one column configuration. The `$schema` field may be present on each item.

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
| `properties` | PowerTableProperties | true | PowerTable visual properties. |

### PowerTableProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `pivotAssignments` | array | true | Column/row assignments. |
| `sortingConfig` | array | true | Sorting configurations. |
| `superFilterAssignments` | SuperFilterAssignment[] | true | Filter configurations including filter state and pivot assignments. |
| `visualState` | object | true | Visual display state. |
| `visualInteractions` | object | false | Cross-visual interaction settings. |
| `dimension` | Dimension | false | Visual dimension (width and height). |
| `position` | Position | false | Visual position on canvas (x and y). |
| `visualStyles` | object | false | Style overrides. |
| `groupName` | string | false | Group name for the visual. |
| `visualType` | integer | false | Visual type code. |
| `chartType` | string | false | Chart type identifier. |
| `mobileProperties` | object | false | Mobile layout properties. |

## PowerTable Source (`sheets/{sheetId}/visuals/{visualId}/source.json`)

Database source configuration for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string (uri) | true | Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/source/1.0.0/schema.json`. |
| `connection` | ConnectionReferenceOrVar | true | Fabric connection reference. |
| `database` | ItemReferenceOrVar | true | Fabric item reference to the database. |
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
| `settings` | Setting[] | true | List of setting configurations. |

### Setting

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Setting name: `ROW_ADD`, `ROW_UPDATE`, `ROW_DELETE`, `ROW_IDENTIFIER`, `COMMENT_SETTINGS`, or `SCD`. |
| `accessType` | string | false | Access scope: `ALL_USERS` or `SPECIFIC_USERS`. |
| `meta` | object | false | Setting metadata (for example, `enabled` boolean). |
| `rules` | AccessRule[] | false | Access rules with user/filter targeting. |
| `settings` | RowIdentifierSettings or CommentSettings or SCDSettings | false | Setting-specific configuration payload. |

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
| `ruleType` | integer | true | Numeric code for approval rule type. |
| `persistFlag` | integer | false | Numeric code controlling persistence behavior. |
| `settings` | object | false | Approval-specific settings payload. |
| `approvalLevel` | integer | false | Current or default approval level. |
| `multiLevelEnabled` | integer (0 or 1) | false | Whether multi-level approvals are enabled. |
| `approvalLevels` | ApprovalLevel[] | false | Configured approval levels (name, description, level). |
| `approvalFilter` | ApprovalFilter[] | false | Filters applied to approval routing. |

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
| `triggerType` | integer | true | Numeric code for the trigger type. |
| `config` | AutomationConfig | true | Automation configuration including trigger, entry group, and action groups. |

## PowerTable Forms (`sheets/{sheetId}/visuals/{visualId}/forms.json`)

Array of form definitions for a PowerTable visual, defining data entry layouts.

| Property | Type | Required | Description |
|---|---|---|---|
| `title` | string | true | Form title. |
| `layoutMeta` | [LayoutMeta](#layoutmeta) | true | Layout definition including children elements and type. |
| `description` | string | false | Optional form description. |
| `config` | FormConfig | false | Form behavior configuration. |

### LayoutMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `children` | FormElement[] | true | Ordered list of form elements. |
| `type` | string | true | Layout type: `form`. |
| `id` | string | false | Optional layout ID. |
| `layoutType` | string | false | Layout style: `default`, `tabs`, or `sections`. |

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
| `entityLevelVariables` | EntityVariable[] | false | Entity-level calculated variables (actions, numbers, dropdowns). |
| `filterPanePosition` | string | false | Filter pane position: `LEFT`, `RIGHT`, `TOP`, or `BOTTOM`. |
| `topPositionFilterExpandConfig` | FilterExpandConfig | false | Filter pane expand configuration. |
| `commentary` | Commentary | false | Notes and annotation settings. |
| `canvasStyle` | CanvasStyle | false | Canvas dimension, background, wallpaper, border, and shadow styles. |
| `assignmentColumnMap` | object | false | Map of column assignments. |
| `visualGroupMap` | object | false | Map of visual groups. |
| `sourceVisualsMeta` | object | false | Metadata for source visuals. |
| `controlPanePosition` | string | false | Position of the control pane: `left` or `right`. |

### Visual

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique visual identifier. |
| `visualType` | integer | true | Numeric visual type code. |
| `properties` | [VisualProperties](#visualproperties) | true | Visual properties wrapper containing schema version, visual config, and etag. |
| `isEmbedded` | boolean | false | Whether the visual is embedded. |
| `originEntityId` | integer or string | false | Origin entity ID. |

### VisualProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Visual properties schema version. |
| `properties` | VisualConfig | true | Visual configuration including pivot assignments, filters, styles, dimensions, and internal state. |
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
| `dimension` | object | false | Visual dimension (width and height). |
| `position` | object | false | Visual position on canvas (x and y). |
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
