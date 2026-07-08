# Plan item definition

This article provides a breakdown of the structure for Plan items.

## Supported formats

Plan items support the JSON format.

## Definition parts

This table lists the Plan definition parts.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `definition.json` | Plan definition (JSON) | true | Root definition for a Plan artifact containing semantic model references and sheet declarations. |
| `.platform` | PlatformDetails (JSON) | false | Common metadata for the item, included when metadata updates are needed. |
| `Sheets/{SheetID}/ConnectedPlanning/InfoBridge/infobridge.json` | InfoBridge configuration (JSON) | false | InfoBridge data source, query, and writeback configuration for connected planning. |
| `Sheets/{SheetID}/Intelligence/Properties/properties.json` | Intelligence Properties (JSON) | false | Properties and visual configuration for an Intelligence sheet. |
| `Sheets/{SheetID}/Planning/DataInput/dataInput.json` | Data Input Columns (JSON) | false | Array of data input column definitions for a Planning visual. |
| `Sheets/{SheetID}/Planning/InsertRows/insertRows.json` | Insert Rows (JSON) | false | Custom inserted row definitions for a Planning visual. |
| `Sheets/{SheetID}/Planning/Properties/properties.json` | Visual Properties (Planning) (JSON) | false | Pivot assignments, sorting, and filter configurations for a Planning visual. |
| `Sheets/{SheetID}/Planning/Scenarios/scenarios.json` | Scenarios (JSON) | false | Scenario definitions for a Planning visual. |
| `Sheets/{SheetID}/Planning/Writeback/writeback.json` | Writeback Configuration (JSON) | false | Writeback destination, column mapping, and auto-writeback settings for a Planning visual. |
| `Sheets/{SheetID}/PowerTable/Approvals/approvals.json` | PowerTable Approvals (JSON) | false | Approval workflow configuration for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/Automations/automations.json` | PowerTable Automations (JSON) | false | Automation trigger and action flow definitions for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/ColumnConfigs/columnConfigs.json` | PowerTable Column Configs (JSON) | false | Column configuration definitions for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/Forms/forms.json` | PowerTable Forms (JSON) | false | Data entry form layout definitions for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/Properties/properties.json` | PowerTable Properties (JSON) | false | Visual properties including assignments, filters, and visual state for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/Settings/settings.json` | PowerTable Settings (JSON) | false | Permission and row-operation settings for a PowerTable visual. |
| `Sheets/{SheetID}/PowerTable/Source/source.json` | PowerTable Source (JSON) | false | Database source connection configuration for a PowerTable visual. |

## Definition example

```json
{
  "definition": {
    "parts": [
      {
        "path": ".platform",
        "payload": "ew0KICAibWV0YWRhdGEiOiB7DQogICAgInR5cGUiOiAiUGxhbiIsDQogICAgImRpc3BsYXlOYW1lIjogIk15IFBsYW4iDQogIH0NCn0=",
        "payloadType": "InlineBase64"
      },
      {
        "path": "definition.json",
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9wbGFuL2RlZmluaXRpb24vZGVmaW5pdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsDQogICJzZW1hbnRpY01vZGVsUmVmZXJlbmNlIjogew0KICAgICJjb25uZWN0aW9uIjoge30sDQogICAgInNlbWFudGljTW9kZWwiOiB7fQ0KICB9LA0KICAic2hlZXRzIjogW10NCn0=",
        "payloadType": "InlineBase64"
      },
      {
        "path": "Sheets/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/Planning/Properties/properties.json",
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9wbGFuL2RlZmluaXRpb24vcGxhbm5pbmcvcHJvcGVydGllcy8xLjAuMC9zY2hlbWEuanNvbiIsDQogICJ2aXN1YWxzIjoge30NCn0=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```

The preceding `definition.json` payload is a minimal skeleton for structure only. Replace empty `connection`, `semanticModel`, and `sheets` values with valid references before using it in requests.

---

## Plan definition

Root definition for a Plan artifact containing semantic model references and sheet declarations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/definition/1.0.0/schema.json`. |
| `semanticModelReference` | SemanticModelReference | true | Reference to the semantic model backing this plan. |
| `sheets` | SheetReference[] | true | List of sheets in the plan. |

### SemanticModelReference

| Property | Type | Required | Description |
|---|---|---|---|
| `connection` | [ConnectionReferenceOrVar](variable-library-definition.md#supported-variable-types) | true | Connection reference to the semantic model. |
| `semanticModel` | [ItemReferenceOrVar](variable-library-definition.md#supported-variable-types) | true | Item reference to the semantic model. |

### SheetReference

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string (uuid) | true | Unique identifier for the sheet. |
| `displayName` | string | true | User-facing name of the sheet. |
| `sheetType` | string (Planning, PowerTable, Intelligence) | true | The type of sheet. |

### Plan definition file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/definition/1.0.0/schema.json",
  "semanticModelReference": {
    "connection": { "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
    "semanticModel": { "itemId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" }
  },
  "sheets": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "displayName": "Q4 Planning",
      "sheetType": "Planning"
    },
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "displayName": "Budget Table",
      "sheetType": "PowerTable"
    }
  ]
}
```

## PlatformDetails

The platform part contains environment metadata for the item.

* [Create Item](../../../docs-ref-autogen/fabric/core/Items/Create-Item.yml) with definition respects the platform file when provided.
* [Get Item Definition](../../../docs-ref-autogen/fabric/core/Items/Get-Item-Definition.yml) always returns the platform file.
* [Update Item Definition](../../../docs-ref-autogen/fabric/core/Items/Update-Item-Definition.yml) definition accepts the platform file when provided, but only when `updateMetadata=true`.

---

## InfoBridge configuration

InfoBridge configuration defining data sources, queries, transformation steps, and writeback destinations for connected planning.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/connectedPlanning/infobridge/1.0.0/schema.json`. |
| `sources` | Source[] | true | List of InfoBridge data sources. Minimum 1 item. |
| `queryGroups` | QueryGroup[] | false | Optional groupings of queries for organizational purposes. |

### Source

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | The display name of the source. |
| `type` | string or integer | true | The source type. Supported string values: `PLANNING`, `PARQUET`, `APPEND`, `MERGE`, `CSV`, `JSON`, `XLSX`, `SQL_SOURCE`, `JOIN`, `ENCRYPTED_PARQUET`, `EDITABLE`. Numeric equivalents are also accepted. |
| `visualId` | integer or string (uuid) | false | The visual identifier this source is associated with. |
| `meta` | SourceMeta or string | false | Source metadata. |
| `queries` | Query[] | false | List of queries for this source. |
| `dependentQueries` | string[] | false | List of dependent query GUIDs for join sources. |

### SourceMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `includeMeasures` | string[] | false | Measures to include. |
| `includeScenarios` | string[] | false | Scenarios to include. |
| `queries` | JoinQueryReference[] | false | Join query references (for join sources). |
| `joinType` | string | false | The join type (for example, INNER, LEFT). |
| `sql` | string | false | Optional SQL text for SQL source. |

### JoinQueryReference

| Property | Type | Required | Description |
|---|---|---|---|
| `queryId` | string | false | The GUID of the referenced query. Required when `sourceId` is absent. |
| `sourceId` | integer or string | false | Internal numeric source/query identifier. Required when `queryId` is absent. |
| `sourceName` | string | true | The display name of the source query. |
| `joinColumnName` | string[] | true | Column names used for the join. Minimum 1 item. |
| `isBaseQuery` | boolean | false | Whether this is the base query in the join. |

### Query

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | The display name of the query. |
| `queryId` | string | true | The unique GUID for this query. |
| `type` | string or integer | false | The query type. |
| `visualId` | integer or string (uuid) | false | The visual identifier (numeric internal ID or UUID in external definitions). |
| `meta` | object | false | Query-level metadata. |
| `transformationSteps` | TransformationStep[] | false | Ordered list of transformation steps. |
| `writebackSettings` | WritebackSettings | false | Writeback settings for this query. |
| `writebackDestinations` | WritebackDestination[] | false | List of writeback destinations. |

### TransformationStep

| Property | Type | Required | Description |
|---|---|---|---|
| `stepIndex` | integer | true | The ordinal index of this step. |
| `meta` | TransformationStepMeta | true | Step metadata including type, name, and description. |
| `notes` | any | false | Optional notes for the step. |

### TransformationStepMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string or integer | true | The transformation type (for example, `PLANNING`, `XLSX`, or numeric code). |
| `name` | string | true | The display name of the step. |
| `value` | object | false | Step-specific configuration values. |
| `description` | string | false | A human-readable description of the step. |

### WritebackDestination (InfoBridge)

| Property | Type | Required | Description |
|---|---|---|---|
| `tableName` | string | true | Target writeback table name. |
| `connection` | ConnectionReferenceOrVar | false | Connection reference. Required when `connectionId`/`databaseId` are absent. |
| `database` | ItemReferenceOrVar | false | Item reference to the database. Required when `connectionId`/`databaseId` are absent. |
| `schema` | string | false | Database schema name. |
| `connectionId` | string | false | Connection identifier (legacy). Required when `connection`/`database` are absent. |
| `dmtsConnectionId` | string | false | Legacy writeback connection ID variable used by connected planning examples. |
| `databaseId` | string | false | Database ID (legacy). |
| `connectionIdVariable` | string | false | Variable name for the connection ID. |
| `settingsHash` | string | false | Hash of destination settings. |
| `settings` | object | false | Destination settings including host, database, schema. |

### QueryGroup

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | The group display name. |
| `queryIds` | string[] | true | List of query GUIDs belonging to this group. |
| `description` | string | false | Optional group description. |
| `parentGroupId` | integer | false | Optional parent group ID for hierarchy. |

### InfoBridge configuration file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/connectedPlanning/infobridge/1.0.0/schema.json",
  "sources": [
    {
      "name": "Sales Planning",
      "type": "PLANNING",
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "queries": [
        {
          "name": "Revenue Query",
          "queryId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
          "transformationSteps": [
            {
              "stepIndex": 0,
              "meta": {
                "type": "PLANNING",
                "name": "Load from Planning",
                "description": "Loads data from the planning visual"
              }
            }
          ],
          "writebackDestinations": [
            {
              "connection": { "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
              "database": { "itemId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
              "schema": "dbo",
              "tableName": "RevenueWriteback"
            }
          ]
        }
      ]
    }
  ],
  "queryGroups": [
    {
      "name": "Revenue Queries",
      "queryIds": ["xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"]
    }
  ]
}
```

---

## Intelligence Properties

Properties definition for an Intelligence sheet visual, including page-level settings, variables, canvas styles, commentary, and embedded visual configurations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/intelligence/properties/1.0.0/schema.json`. |
| `properties` | PageProperties | true | Top-level page properties wrapper. |
| `visuals` | Visual[] | true | List of visuals on the Intelligence sheet. |

### PageProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Schema version for the properties format. |
| `properties` | PageSettings | true | Page-level settings. |

### PageSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `pageLevelFilterAssignments` | array | false | Page-level filter assignments. |
| `entityLevelVariables` | EntityVariable[] | false | Entity-level calculated variables (actions, numbers, dropdowns, and others). |
| `filterPanePosition` | string (LEFT, RIGHT, TOP, BOTTOM) | false | Position of the filter pane. |
| `topPositionFilterExpandConfig` | FilterExpandConfig | false | Expand configuration when filter pane is at the top. |
| `commentary` | Commentary | false | Commentary configuration including notes and annotations. |
| `canvasStyle` | CanvasStyle | false | Canvas layout and styling configuration. |
| `assignmentColumnMap` | object | false | No description provided. |
| `visualGroupMap` | object | false | No description provided. |
| `sourceVisualsMeta` | object | false | No description provided. |
| `controlPanePosition` | string (left, right) | false | No description provided. |

### EntityVariable

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique variable identifier (for example, `CALC_VARIABLE_xxxxx`). |
| `label` | string | true | Display label for the variable. |
| `type` | string (action, number, text, dropdown, date, slider) | true | Variable type. |
| `scope` | string (entity, visual, page) | true | Variable scope. |
| `technicalName` | string | true | Technical/programmatic name for the variable. |
| `description` | string | false | No description provided. |
| `showInViewMode` | boolean | false | No description provided. |
| `assignInterface` | boolean | false | No description provided. |
| `interfaceType` | string (switch, dropdown, input-box, slider, radio, checkbox) | false | No description provided. |
| `value` | any | false | Current value of the variable. |
| `defaultValue` | any | false | Default value of the variable. |
| `enableActionScript` | boolean | false | No description provided. |
| `enableToggle` | boolean | false | No description provided. |
| `activeButton` | ActionButton | false | Action button configuration for the active state. |
| `inactiveButton` | ActionButton | false | Action button configuration for the inactive state. |

### ActionButton

| Property | Type | Required | Description |
|---|---|---|---|
| `label` | string | true | Button label. |
| `style` | ButtonStyle | true | Button style configuration. |
| `actions` | ActionScript[] | true | List of action scripts triggered by the button. |

### ActionScript

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique action identifier. |
| `target` | string | true | Target visual or element. |
| `script` | string | true | Action script expression (for example, `SHOW_VISUAL`, `HIDE_VISUAL`). |

### Visual

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique visual identifier. |
| `visualType` | integer | true | Numeric visual type code. |
| `isEmbedded` | boolean | false | No description provided. |
| `originEntityId` | any | false | Origin entity ID (integer or string). |
| `properties` | VisualProperties | true | Visual properties wrapper. |

### Intelligence Properties file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/intelligence/properties/1.0.0/schema.json",
  "properties": {
    "schema": "1.0.0",
    "properties": {
      "filterPanePosition": "LEFT",
      "entityLevelVariables": [
        {
          "id": "CALC_VARIABLE_001",
          "label": "Region Filter",
          "type": "dropdown",
          "scope": "page",
          "technicalName": "regionFilter",
          "showInViewMode": true
        }
      ]
    }
  },
  "visuals": [
    {
      "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "visualType": 1,
      "properties": {
        "schema": "1.0.0",
        "properties": {}
      }
    }
  ]
}
```

---

## Data Input Columns

Array of data input column definitions for a Planning visual, including forecasts, text inputs, number inputs, and native measures. The root of this file is an array of `DataInputColumn` objects.

### DataInputColumn

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/dataInput/1.0.0/schema.json`. |
| `measureGuid` | string | true | Unique identifier for the measure/column. |
| `visualId` | string (uuid) | true | ID of the visual this column belongs to. |
| `columnMeta` | ColumnMeta | true | Column metadata including type and data type. |
| `name` | string | true | Display name of the column. |
| `dataInputType` | integer | true | Numeric code for the data input type. |
| `description` | string or null | false | Optional description. |
| `disableWriteAccess` | boolean | false | Whether write access is disabled. |
| `forecastAllowedUserPermissions` | boolean | false | Whether user permissions are allowed for forecast. |

### ColumnMeta (DataInput)

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique column meta identifier. |
| `label` | string | true | Display label. |
| `measure_type` | MeasureType | true | Discriminated union of measure types. |
| `data_type` | string (Number, Text) | true | Data type of the column. |
| `description` | string or null | false | No description provided. |

### MeasureType

Exactly one of the following keys must be present:

| Property | Type | Required | Description |
|---|---|---|---|
| `Forecast` | ForecastMeasure | false | Forecast-type measure configuration. |
| `DataInput` | DataInputMeasure | false | Data input-type measure configuration. |
| `VisualColumn` | VisualColumnMeasure | false | Visual column wrapper for a DataInput measure. |
| `Native` | NativeMeasure | false | Native measure configuration. |

### ForecastMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `forecast_version` | integer | true | Forecast version number. |
| `forecast_period` | object | true | Forecast period with `start` and `end` datetime values. |
| `closed_period_till` | string (date-time) or null | false | Closed period cutoff date. |
| `forecast_value_display` | string (Actual, Forecast) | false | Which value to display. |
| `open_period_config` | object | false | Configuration for the open forecast period. |
| `closed_period_config` | object | false | Configuration for the closed forecast period. |
| `forecast_allowed_user_permissions` | boolean | false | No description provided. |
| `disable_write_access` | boolean | false | No description provided. |
| `edit_config` | EditConfig | false | Edit configuration. |

### DataInputMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique identifier. |
| `column_type` | object | true | Discriminated union of column types (`Number` or `ShortText`). |
| `title` | string | true | Display title. |
| `disable_write_access` | boolean | false | No description provided. |
| `on_change_formula` | string | false | Formula evaluated on value change. |
| `allow_input` | string (ReadAndEdit, ReadOnly) | false | Input permission mode. |

### NativeMeasure

| Property | Type | Required | Description |
|---|---|---|---|
| `measure_role` | string | true | Role of the native measure (for example, `ACMeasure`). |

### NumberColumnType

| Property | Type | Required | Description |
|---|---|---|---|
| `min_value` | number or null | false | Minimum allowed value. |
| `max_value` | number or null | false | Maximum allowed value. |
| `distribute_parent_value_to_children` | boolean | false | No description provided. |
| `default_value` | number or null | false | Default numeric value. |

### ShortTextColumnType

| Property | Type | Required | Description |
|---|---|---|---|
| `default_value` | string or null | false | Default text value. |
| `allow_entry_on_totals_or_subtotals` | boolean | false | No description provided. |
| `prevent_null` | boolean | false | No description provided. |
| `field_validation` | string (AnyValue) | false | Validation rule. |
| `minimum_length` | integer or null | false | Minimum text length. |
| `maximum_length` | integer or null | false | Maximum text length. |

### Data Input Columns file example

```json
[
  {
    "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/dataInput/1.0.0/schema.json",
    "measureGuid": "measure-001",
    "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "name": "Revenue Forecast",
    "dataInputType": 1,
    "columnMeta": {
      "id": "col-001",
      "label": "Revenue Forecast",
      "data_type": "Number",
      "measure_type": {
        "Forecast": {
          "forecast_version": 1,
          "forecast_period": {
            "start": "2024-01-01T00:00:00Z",
            "end": "2024-12-31T00:00:00Z"
          },
          "forecast_value_display": "Forecast"
        }
      }
    }
  }
]
```

---

## Insert Rows

Object containing custom (inserted) rows for a Planning visual, including static rows and calculated rows.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/insertRows/1.0.0/schema.json`. |
| `rows` | InsertRow[] | true | Array of inserted row definitions. |

### InsertRow

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique row identifier. |
| `visualId` | string (uuid) | true | ID of the visual this row belongs to. |
| `rowMeta` | RowMeta | true | Row metadata. |
| `name` | string | true | Display name of the row. |
| `status` | integer | true | Status code of the row. |
| `dimensionId` | string | true | Dimension this row belongs to. |
| `visualRowConfigId` | string or null | false | Optional visual row configuration reference. |

### RowMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Row meta identifier. |
| `row_type` | RowType | true | Discriminated union of row types (StaticRow or CalculatedRow). |
| `title` | string | true | Row title. |
| `scaling_factor` | string | false | Scaling factor for display (for example, `Auto`). |
| `include_in_total` | boolean | false | Whether to include this row in totals. |
| `parent_id` | string | false | Parent row ID for hierarchy. |
| `level` | integer | false | Hierarchy level (minimum 0). |
| `previous_row_id` | string | false | ID of the preceding row for ordering. |
| `disabled` | boolean | false | Whether the row is disabled. |
| `bind_for_cross_filter` | boolean or null | false | No description provided. |
| `description` | string or null | false | No description provided. |
| `column_aggregation` | string (Sum, Average, Min, Max, Count) | false | Aggregation method for this row. |

### RowType

Exactly one of the following keys must be present:

| Property | Type | Required | Description |
|---|---|---|---|
| `StaticRow` | StaticRowType | false | Static (manually entered) row configuration. |
| `CalculatedRow` | CalculatedRowType | false | Calculated row configuration. |

### StaticRowType

| Property | Type | Required | Description |
|---|---|---|---|
| `distribute_parent_value_to_child` | boolean | false | Whether to distribute parent value to child rows. |
| `default_value` | object | false | Default value configuration with a `Static` string property. |
| `row_edit_mode` | string (InEditModeOnly, Always) | false | When the row is editable. |

### CalculatedRowType

| Property | Type | Required | Description |
|---|---|---|---|
| `formula` | string | true | Calculation formula for this row. |
| `description` | string | false | No description provided. |
| `include_in_chart` | boolean | false | No description provided. |

### Insert Rows file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/insertRows/1.0.0/schema.json",
  "rows": [
    {
      "id": "row-001",
      "visualId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "name": "Total Revenue",
      "status": 1,
      "dimensionId": "dim-001",
      "rowMeta": {
        "id": "meta-001",
        "title": "Total Revenue",
        "row_type": {
          "CalculatedRow": {
            "formula": "SUM(Revenue)",
            "include_in_chart": true
          }
        },
        "include_in_total": false,
        "column_aggregation": "Sum"
      }
    }
  ]
}
```

---

## Visual Properties (Planning)

Properties configuration for a Planning visual, including pivot assignments, sorting, and filter configurations.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/properties/1.0.0/schema.json`. |
| `visuals` | object | true | Map of visual ID to VisualProperties. |

### VisualProperties (Planning)

| Property | Type | Required | Description |
|---|---|---|---|
| `schema` | string | true | Properties schema version. |
| `properties` | object | true | Visual properties containing `pivotAssignments`, `sortingConfig`, and `superFilterAssignments`. |

### PivotAssignment (Planning)

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique identifier for the assignment (`Table[Column]` format). |
| `sourceId` | string | true | Source identifier. |
| `bucketId` | string (rows, columns, ameasure) | true | Target bucket for the assignment. `ameasure` is the schema-defined token. |
| `columnName` | string | true | Column name. |
| `dataType` | string (String, Int64, Double, DateTime, Boolean) | true | Column data type. |
| `order` | integer | false | Display order (minimum 0). |
| `columnType` | string (Column, Hierarchy Level, Measure) | false | Column classification. |
| `formatString` | string or null | false | Display format string. |
| `contents` | string | false | No description provided. |
| `sourceType` | string (PowerBI) | false | Source system type. |
| `name` | string | false | Internal name. |
| `customName` | string | false | User-defined display name. |
| `aggregationType` | string (sum, average, min, max, count) | false | Aggregation type for measures. |
| `isFlatHierarchy` | boolean | false | No description provided. |
| `hierarchyName` | string | false | Associated hierarchy name. |
| `tableName` | string | false | Source table name. |
| `dataRoleType` | string (Grouping, Measure) | false | Data role classification. |
| `measureGuid` | string | false | Measure GUID for data input columns. |

### SuperFilterAssignment (Planning)

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique filter assignment ID. |
| `pivotAssignments` | PivotAssignment[] | true | Pivot assignments for this filter. |
| `isDefault` | boolean | false | Whether this is the default filter. |
| `isDefaultMeasure` | boolean | false | No description provided. |
| `isDaxMeasure` | boolean | false | No description provided. |
| `bucketId` | string | false | No description provided. |
| `configuration` | FilterConfiguration | false | Filter configuration. |
| `filter` | array | false | Active filter values. |
| `position` | string (LEFT, RIGHT, TOP, BOTTOM) | false | Filter pane position. |
| `filterLevel` | string (visual, page, report) | false | Filter scope level. |

### Visual Properties (Planning) file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/properties/1.0.0/schema.json",
  "visuals": {
    "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx": {
      "schema": "1.0.0",
      "properties": {
        "pivotAssignments": [
          {
            "id": "Sales[Region]",
            "sourceId": "src-001",
            "bucketId": "rows",
            "columnName": "Region",
            "dataType": "String",
            "order": 0,
            "columnType": "Column",
            "tableName": "Sales"
          }
        ],
        "sortingConfig": [],
        "superFilterAssignments": []
      }
    }
  }
}
```

---

## Scenarios

Object containing scenario definitions for a Planning visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/scenarios/1.0.0/schema.json`. |
| `scenarios` | Scenario[] | true | Array of scenario definitions. |

### Scenario

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the scenario. |
| `status` | string (ACTIVE, INACTIVE, DRAFT) | true | Current status of the scenario. |
| `meta` | ScenarioMeta | true | Scenario metadata. |
| `autoWritebackEnabled` | string (ACTIVE, INACTIVE) | false | Whether auto-writeback is enabled for this scenario. |
| `simulations` | array or null | false | List of simulations associated with this scenario. |
| `userPermission` | array | false | User permissions for this scenario. |

### ScenarioMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `measureIds` | string[] | true | List of measure IDs included in this scenario. |
| `scenarioGuid` | string | true | Unique identifier for the scenario. |
| `order` | integer | true | Display order of the scenario (minimum 0). |
| `dimensionHash` | string | false | Hash representing the dimension configuration. |

### Scenarios file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/scenarios/1.0.0/schema.json",
  "scenarios": [
    {
      "name": "Base Case",
      "status": "ACTIVE",
      "autoWritebackEnabled": "ACTIVE",
      "meta": {
        "measureIds": ["measure-001", "measure-002"],
        "scenarioGuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "order": 0,
        "dimensionHash": "abc123"
      }
    },
    {
      "name": "Optimistic",
      "status": "DRAFT",
      "meta": {
        "measureIds": ["measure-001"],
        "scenarioGuid": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "order": 1
      }
    }
  ]
}
```

---

## Writeback Configuration

Writeback configuration for a Planning visual, defining destination, column mapping, and auto-writeback settings.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/writeback/1.0.0/schema.json`. |
| `writebackType` | integer | true | Writeback type code. |
| `destination` | WritebackDestination | true | Target database destination for writeback. |
| `writebackFilter` | object | false | Filter applied during writeback. |
| `excludedMeasureGuids` | string[] | false | Measure GUIDs excluded from writeback. |
| `isAutoWritebackEnabled` | integer | false | Auto-writeback enabled status code. |
| `autoWbEnabledScenarioIds` | string[] | false | Scenarios with auto-writeback enabled. |
| `debounce` | DebounceConfig | false | Debounce configuration for auto-writeback. |
| `isSnapshotWbEnabled` | integer | false | Snapshot writeback enabled status code. |
| `wbTableColumnMapping` | object | false | Map of measure GUIDs to ColumnMapping objects. |
| `numberPrecision` | object | false | Decimal precision configuration with a `decimal` integer property. |
| `stringColumnLength` | object | false | String column length configuration. |
| `writebackAsHTML` | boolean | false | Whether to write back as HTML. |
| `wbColumns` | WritebackColumn[] | false | Column definitions for the writeback table. |
| `hasWritebackAccess` | boolean | false | No description provided. |

### WritebackDestination (Planning)

| Property | Type | Required | Description |
|---|---|---|---|
| `connection` | ConnectionReferenceOrVar | true | Connection reference to the destination database. |
| `database` | ItemReferenceOrVar | true | Item reference to the destination database. |
| `schema` | string | true | Database schema name. |
| `tableName` | string | true | Target table name. |

### DebounceConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `duration` | integer | false | Debounce duration in seconds (minimum 0). |
| `isDebounceEnabled` | integer | false | Debounce enabled status code. |

### ColumnMapping

| Property | Type | Required | Description |
|---|---|---|---|
| `visualName` | string | true | Display name in the visual. |
| `wbColumnName` | string | true | Column name in the writeback table. |

### WritebackColumn

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Column name in the writeback table. |
| `dataType` | string (TEXT, NUMERIC, Number, Text) | true | Data type of the column. |
| `id` | string | false | Column ID (for dimension columns). |
| `measureGuid` | string | false | Measure GUID (for measure columns). |
| `isColumnDimension` | boolean | false | Whether this is a column dimension. |
| `isTimeDimension` | boolean | false | Whether this is a time dimension. |
| `timeIntervalUnit` | string (YEAR, QUARTER, MONTH, WEEK, DAY) | false | Time interval unit for time dimensions. |
| `source` | string (Native, Visual Measure, Visual Column) | false | Source type of the column. |

### Writeback Configuration file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/planning/writeback/1.0.0/schema.json",
  "writebackType": 1,
  "destination": {
    "connection": { "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
    "database": { "itemId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
    "schema": "dbo",
    "tableName": "PlanningWriteback"
  },
  "isAutoWritebackEnabled": 1,
  "debounce": {
    "duration": 5,
    "isDebounceEnabled": 1
  },
  "wbColumns": [
    {
      "name": "Revenue",
      "dataType": "NUMERIC",
      "measureGuid": "measure-001",
      "isColumnDimension": false
    },
    {
      "name": "Period",
      "dataType": "TEXT",
      "id": "dim-period",
      "isTimeDimension": true,
      "timeIntervalUnit": "MONTH"
    }
  ]
}
```

---

## PowerTable Approvals

Approval workflow configuration for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/approvals/1.0.0/schema.json`. |
| `ruleType` | integer | true | Numeric code for approval rule type. |
| `persistFlag` | integer | false | Numeric code controlling persistence behavior. |
| `settings` | object | false | Approval-specific settings payload. |
| `approvalLevel` | integer | false | Current or default approval level. |
| `multiLevelEnabled` | integer (0, 1) | false | Whether multi-level approvals are enabled. |
| `approvalLevels` | ApprovalLevel[] | false | Configured approval levels. |
| `approvalFilter` | ApprovalFilter[] | false | Filters applied to approval routing. |

### ApprovalLevel

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Level name. |
| `description` | string | true | Level description. |
| `level` | integer | true | Level number (minimum 1). |

### ApprovalFilter

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Filter name. |
| `filter` | object or null | true | Filter condition object. |
| `order` | integer | true | Display order (minimum 0). |

### PowerTable Approvals file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/approvals/1.0.0/schema.json",
  "ruleType": 1,
  "multiLevelEnabled": 1,
  "approvalLevel": 1,
  "approvalLevels": [
    {
      "name": "Manager Approval",
      "description": "First-level review by direct manager",
      "level": 1
    },
    {
      "name": "Director Approval",
      "description": "Final approval by director",
      "level": 2
    }
  ],
  "approvalFilter": [
    {
      "name": "Region Filter",
      "filter": null,
      "order": 0
    }
  ]
}
```

---

## PowerTable Automations

Array of automation definitions for a PowerTable visual, defining triggers and action flows.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/automations/1.0.0/schema.json`. |
| `automations` | Automation[] | true | Array of automation definitions. |

### Automation

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Display name of the automation. |
| `triggerType` | integer | true | Numeric code for the trigger type. |
| `config` | AutomationConfig | true | Automation configuration. |

### AutomationConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Automation name. |
| `trigger` | Trigger | true | Trigger configuration. |
| `entryGroupId` | string | true | ID of the first action group to execute. |
| `groups` | object | true | Map of group IDs to ActionGroup objects. |

### Trigger

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | string | true | Unique trigger identifier. |
| `triggerType` | string | true | Trigger type code. |
| `description` | string | false | Trigger description. |
| `triggerConfig` | object | false | Trigger-specific configuration including optional `conditions` array. |

### ActionGroup

| Property | Type | Required | Description |
|---|---|---|---|
| `groupId` | string | true | Unique group identifier. |
| `groupType` | string | true | Group type code. |
| `entryActionId` | string | true | ID of the first action in this group. |
| `actions` | object | true | Map of action IDs to Action objects. |
| `previousGroupId` | string or null | false | ID of the preceding action group. |
| `nextGroupId` | string or null | false | ID of the following action group. |
| `position` | integer | false | Display position. |

### Action

| Property | Type | Required | Description |
|---|---|---|---|
| `actionId` | string | true | Unique action identifier. |
| `actionType` | string | true | Action type code. |
| `config` | object | true | Action-specific configuration. |
| `previousActionId` | string or null | false | ID of the preceding action. |
| `nextActionId` | string or null | false | ID of the following action. |

### CreateRecordConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `connectionId` | string | true | Connection identifier (may use variable substitution). |
| `table` | string | true | Target table for the record. |
| `description` | string | false | No description provided. |
| `fields` | object | false | Field name to value mapping for the new record. |

### PowerTable Automations file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/automations/1.0.0/schema.json",
  "automations": [
    {
      "name": "On Row Submit",
      "triggerType": 1,
      "config": {
        "name": "On Row Submit",
        "trigger": {
          "id": "trigger-001",
          "triggerType": "ROW_SUBMIT",
          "description": "Fires when a row is submitted"
        },
        "entryGroupId": "group-001",
        "groups": {
          "group-001": {
            "groupId": "group-001",
            "groupType": "SEQUENTIAL",
            "entryActionId": "action-001",
            "actions": {
              "action-001": {
                "actionId": "action-001",
                "actionType": "CREATE_RECORD",
                "config": {
                  "createRecord": {
                    "connectionId": "conn-001",
                    "table": "AuditLog",
                    "fields": {
                      "event": "ROW_SUBMITTED"
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  ]
}
```

---

## PowerTable Column Configs

Array of column configuration definitions for a PowerTable visual. The root of this file is an array of `ColumnConfig` objects.

### ColumnConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/columnConfigs/1.0.0/schema.json`. |
| `columnGuid` | string | true | Unique identifier for the column. |
| `columnName` | string | true | Database column name. |
| `columnType` | integer | true | Column type code (1=text, 2=single select, 3=multi select, 4=date, and so on). |
| `columnMeta` | ColumnMeta (PowerTable) | true | Column metadata. |
| `displayName` | string | true | User-facing column name. |
| `hideColumn` | integer (0, 1) | false | Whether the column is hidden. 0=visible, 1=hidden. |
| `mandatory` | integer (0, 1) | false | Whether the column is mandatory. 0=optional, 1=required. |
| `allowEdit` | integer (0, 1) | false | Whether the column allows editing. 0=readonly, 1=editable. |
| `visualColumnType` | integer | false | Visual representation type. |
| `description` | string | false | No description provided. |

### ColumnMeta (PowerTable)

| Property | Type | Required | Description |
|---|---|---|---|
| `defaultValueType` | string (NONE, STATIC, FORMULA, AUTO_INCREMENT) | false | How the default value is determined. |
| `defaultValue` | string or null | false | Default value for the column. |
| `isPrimaryKey` | boolean | false | Whether this column is a primary key. |
| `updateValueOnRowModify` | boolean | false | Whether to update this value when any column in the row is modified. |
| `resetToDefaultOnRowModify` | boolean | false | Whether to reset to default when the row is modified. |
| `maximumAllowedLength` | string | false | Maximum allowed text length. |
| `textFieldColumnType` | string (Any Value, Email, URL, Phone) | false | Text field validation type. |
| `options` | array | false | Static dropdown options. |
| `selectionMethod` | string (DISTINCT_VALUES, LOAD_FROM_DATABASE, STATIC) | false | How dropdown options are sourced. |
| `optionLinking` | OptionLinking | false | Linked option configuration for dependent dropdowns. |
| `isFilterBasedOnAnotherValue` | boolean | false | Whether options are filtered based on another column value. |
| `fcVisualName` | string or null | false | Foreign column visual name. |
| `fcVisualIdColumnName` | string or null | false | Foreign column visual ID column name. |
| `fcVisualLabelColumnName` | string or null | false | Foreign column visual label column name. |

### OptionLinking

| Property | Type | Required | Description |
|---|---|---|---|
| `lookupConfig` | LookupConfig[] | false | Lookup configuration for linked options. |
| `filters` | array | false | Filters applied to the linked options. |

### LookupConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `table` | string | true | Lookup table name. |
| `label` | string | true | Column used for display label. |
| `id` | string | true | Column used for the value. |
| `schema` | string | false | Database schema. |
| `order` | integer | false | Display order. |
| `linkingColumn` | string | false | Column in the main table this links to. |

### PowerTable Column Configs file example

```json
[
  {
    "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/columnConfigs/1.0.0/schema.json",
    "columnGuid": "col-001",
    "columnName": "Status",
    "columnType": 2,
    "displayName": "Status",
    "allowEdit": 1,
    "mandatory": 0,
    "columnMeta": {
      "defaultValueType": "STATIC",
      "defaultValue": "Pending",
      "selectionMethod": "STATIC",
      "options": [
        { "label": "Pending", "value": "Pending" },
        { "label": "Approved", "value": "Approved" }
      ]
    }
  }
]
```

---

## PowerTable Forms

Array of form definitions for a PowerTable visual, defining data entry layouts.  The root of this file is an array of `Form` objects.

### Form

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/forms/1.0.0/schema.json`. |
| `title` | string | true | Form title. |
| `layoutMeta` | LayoutMeta | true | Layout configuration. |
| `description` | string | false | Form description. |
| `config` | FormConfig | false | Form-level configuration. |

### LayoutMeta

| Property | Type | Required | Description |
|---|---|---|---|
| `children` | FormElement[] | true | Ordered list of form elements. |
| `type` | string (form) | true | Layout type. |
| `id` | string | false | Optional layout ID. |
| `layoutType` | string (default, tabs, sections) | false | Layout variant. |

### FormElement

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | string (field, section, tab) | true | Element type. |
| `name` | string | true | Field/column name. |
| `mandatory` | integer (0, 1) | false | Whether the field is mandatory. |
| `allowEdit` | integer (0, 1) | false | Whether the field is editable. |
| `props` | object | false | Additional element properties. |

### FormConfig

| Property | Type | Required | Description |
|---|---|---|---|
| `showTitle` | boolean | false | Whether to show the form title. |
| `showLogo` | boolean | false | Whether to show a logo. |
| `submissionMessage` | string | false | Message shown after form submission. |
| `fieldLabel` | string (stacked, inline) | false | Label layout style. |

### PowerTable Forms file example

```json
[
  {
    "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/forms/1.0.0/schema.json",
    "title": "New Budget Entry",
    "description": "Form for submitting new budget rows",
    "layoutMeta": {
      "type": "form",
      "layoutType": "default",
      "children": [
        {
          "type": "field",
          "name": "Region",
          "mandatory": 1,
          "allowEdit": 1
        },
        {
          "type": "field",
          "name": "Amount",
          "mandatory": 1,
          "allowEdit": 1
        }
      ]
    },
    "config": {
      "showTitle": true,
      "fieldLabel": "stacked",
      "submissionMessage": "Budget entry submitted successfully."
    }
  }
]
```

---

## PowerTable Properties

Properties definition for a PowerTable visual, including assignments, filters, styles, and visual state.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/properties/1.0.0/schema.json`. |
| `properties` | PowerTableProperties | true | PowerTable visual properties. |

### PowerTableProperties

| Property | Type | Required | Description |
|---|---|---|---|
| `pivotAssignments` | array | true | Column/row/measure assignments. |
| `sortingConfig` | array | true | Sorting configurations. |
| `superFilterAssignments` | SuperFilterAssignment[] | true | Filter configurations. |
| `visualState` | VisualState | true | Visual state including column metadata and filter state. |
| `visualInteractions` | object | false | Visual interaction settings. |
| `groupName` | string | false | Visual group name. |
| `visualType` | integer | false | Numeric visual type code. |
| `chartType` | string | false | Chart type identifier. |
| `dimension` | Dimension | false | Visual dimensions (width and height). |
| `position` | Position | false | Visual position (x and y). |
| `visualStyles` | object | false | Visual style overrides. |
| `lockAspectRatio` | boolean | false | Whether to lock the visual's aspect ratio. |

### VisualState

| Property | Type | Required | Description |
|---|---|---|---|
| `assignmentColumnMap` | object | false | Column map for assignments. |
| `superFilterAssignments` | array | false | Active super filter assignments. |
| `superFilter` | object | false | Active super filter state. |
| `manageMeasures` | array | false | Manage measures state. |
| `columnMeta` | object | false | Column metadata state. |
| `superFilterMetaConfig` | object | false | Super filter meta configuration. |

### Dimension

| Property | Type | Required | Description |
|---|---|---|---|
| `width` | number | true | Width in pixels or units. |
| `height` | number | true | Height in pixels or units. |

### Position

| Property | Type | Required | Description |
|---|---|---|---|
| `x` | number | true | Horizontal position. |
| `y` | number | true | Vertical position. |

### PowerTable Properties file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/properties/1.0.0/schema.json",
  "properties": {
    "pivotAssignments": [],
    "sortingConfig": [],
    "superFilterAssignments": [],
    "visualState": {
      "columnMeta": {},
      "superFilterAssignments": []
    },
    "dimension": {
      "width": 800,
      "height": 400
    },
    "position": {
      "x": 0,
      "y": 0
    }
  }
}
```

---

## PowerTable Settings

Settings and permission configurations for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/settings/1.0.0/schema.json`. |
| `settings` | Setting[] | true | Array of setting configurations. |

### Setting

| Property | Type | Required | Description |
|---|---|---|---|
| `name` | string | true | Setting name. One of: `ROW_ADD`, `ROW_UPDATE`, `ROW_DELETE`, `ROW_IDENTIFIER`, `COMMENT_SETTINGS`, `SCD`. |
| `accessType` | string (ALL_USERS, SPECIFIC_USERS) | false | Who has access to this setting. |
| `meta` | object | false | Setting metadata with an optional `enabled` boolean. |
| `rules` | AccessRule[] | false | Access rules for this setting. |
| `settings` | RowIdentifierSettings or CommentSettings or SCDSettings | false | Setting-specific configuration. |

### AccessRule

| Property | Type | Required | Description |
|---|---|---|---|
| `ruleId` | string | true | Rule identifier. |
| `ruleName` | string | true | Rule display name. |
| `filter` | object | false | Filter condition for this rule. |
| `filterUsers` | string[] | false | User list for user-scoped rules. |

### RowIdentifierSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `rowIdentifier` | string | true | Column name used as the row identifier. |

### CommentSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `notification` | boolean | false | Whether to send notifications on comments. |
| `rowLevelComments` | boolean | false | Whether row-level comments are enabled. |
| `toggleAddonColumns` | boolean | false | Whether to toggle add-on columns. |
| `displayComment` | boolean | false | Whether to display comments inline. |

### SCDSettings

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | integer (2, 3) | true | SCD type (type 2 or type 3). |
| `enabled` | boolean | true | Whether SCD is enabled. |
| `config` | object | false | SCD configuration with `businessKey` and `identifierKeys` (activeFlag, startDate, endDate, effectiveDate). |

### PowerTable Settings file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/settings/1.0.0/schema.json",
  "settings": [
    {
      "name": "ROW_ADD",
      "accessType": "ALL_USERS",
      "meta": { "enabled": true }
    },
    {
      "name": "ROW_DELETE",
      "accessType": "SPECIFIC_USERS",
      "rules": [
        {
          "ruleId": "rule-001",
          "ruleName": "Managers Only",
          "filterUsers": ["manager@contoso.com"]
        }
      ]
    },
    {
      "name": "ROW_IDENTIFIER",
      "settings": {
        "rowIdentifier": "RecordId"
      }
    },
    {
      "name": "COMMENT_SETTINGS",
      "settings": {
        "notification": true,
        "rowLevelComments": true,
        "displayComment": true
      }
    }
  ]
}
```

---

## PowerTable Source

Database source connection configuration for a PowerTable visual.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string | true | Schema URI. Must be `https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/source/1.0.0/schema.json`. |
| `connection` | ConnectionReferenceOrVar | true | Connection reference to the source database. |
| `database` | ItemReferenceOrVar | true | Item reference to the source database. |
| `schema` | string | true | Database schema name. |
| `tableName` | string | true | Source table name. |

### PowerTable Source file example

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/plan/definition/powerTable/source/1.0.0/schema.json",
  "connection": { "connectionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
  "database": { "itemId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" },
  "schema": "dbo",
  "tableName": "BudgetData"
}
```
