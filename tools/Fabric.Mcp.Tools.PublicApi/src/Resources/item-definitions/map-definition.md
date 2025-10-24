---
title: Map item definition
description: Learn how to create a Map item definition when using the Microsoft Fabric REST API.
author: ismaelna
ms.author: ismaelna
ms.title: Map item definition
ms.service: fabric
ms.date: 09/15/2025
---

# Map item definition

This article explains how to create and structure a Map item definition using the Microsoft Fabric REST API.

## Supported formats

Map items must be defined in **JSON** format.

## Definition structure

The definition consists of two required parts as shown in the following table.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `map.json` | MapDetails (JSON) | true | Contains the core map details that defines the Map item. |
| `.platform`| PlatformDetails (JSON) | true | Contains platform/environment metadata that describes the common details of the Map item. |

## Definition example

```json
{
  "definition": {
    "parts": [
      {
        "path": "map.json",
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9tYXAvZGVmaW5pdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsDQogICJiYXNlbWFwIjogew0KICAgICJvcHRpb25zIjogbnVsbCwNCiAgICAiY29udHJvbHMiOiBudWxsLA0KICAgICJiYWNrZ3JvdW5kQ29sb3IiOiBudWxsLA0KICAgICJ0aGVtZSI6IG51bGwNCiAgfSwNCiAgImRhdGFTb3VyY2VzIjogew0KICAgICJsYWtlaG91c2VzIjogW10sDQogICAgImtxbERhdGFCYXNlcyI6IFtdDQogIH0sDQogICJsYXllclNvdXJjZXMiOiBbXSwNCiAgImxheWVyU2V0dGluZ3MiOiBbXQ0KfQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9naXRJbnRlZ3JhdGlvbi9wbGF0Zm9ybVByb3BlcnRpZXMvMi4wLjAvc2NoZW1hLmpzb24iLAogICJtZXRhZGF0YSI6IHsKICAgICJ0eXBlIjogIk1hcCIsCiAgICAiZGlzcGxheU5hbWUiOiAiTWFwXzEyMzQ1NjciLAogICAgImRlc2NyaXB0aW9uIjogImRlc2NyIgogIH0sCiAgImNvbmZpZyI6IHsKICAgICJ2ZXJzaW9uIjogIjIuMCIsCiAgICAibG9naWNhbElkIjogIjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIKICB9Cn0=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```

## MapDetails

Key components in a map.json.

| Property | Type | Required | Description |
|---|---|---|---|
| `$schema` | string  | true | The schema version for the map definition. |
| `basemap` | [BaseMap](#basemap)  | true | Configuration for base map settings such as controls, background color, and theme.. |
| `dataSources` | [DataSources](#datasources)  | true | Data sources for the map which can include arrays of lakehouses or KQL databases. |
| `layerSources` | [LayerSources](#layersources)  | true | Array of layer sources for the map. |
| `layerSettings` | [LayerSettings](#layersettings)  | true | Array of layer settings for the map. |

The lakehouse cannot be deleted if Map still exists. 

### BaseMap

The BaseMap properties in a Microsoft Fabric Map item definition configure the visual and interactive aspects of the map. The options property allows customization of map behavior, such as enabling or disabling zoom, pitch, compass, scale, and traffic controls. The controls property specifies which of these interactive elements are visible to users. backgroundColor sets the map’s background color, enhancing visual clarity or thematic alignment. The theme property defines the overall aesthetic style of the map, such as light or dark mode, ensuring consistency with user preferences or application design.

| Property | Type | Required | Description |
|---|---|---|---|
| `options` | string  | true | Options for the map. |
| `controls` | [Controls](#controls)  | true | Map control settings. |
| `backgroundColor` | string  | true | Background color of the map. |
| `theme` | [Theme (Enum)](#theme-enum)  | true | Theme for the map. |

#### Controls

The Controls object in a Microsoft Fabric Map item definition specifies the interactive elements available on the map interface. These include options such as zoom, pitch, compass, scale, and traffic, allowing users to navigate and interact with the map more effectively. Each control enhances usability by providing intuitive ways to explore spatial data

| Property | Type | Required | Description |
|---|---|---|---|
| `zoom` | boolean  | false | Enable zoom control. |
| `pitch` | boolean  | false | Enable pitch control. |
| `compass` | boolean  | false | Enable compass control. |
| `scale` | boolean  | false | Enable scale control. |
| `traffic` | boolean  | false | Enable traffic control. |

#### Theme (Enum)

The Theme enum defines the string values available to configure the overall base map style.

| Name | Description |
|------|-------------|
| default | Default theme. |
| classic | Classic theme. |
| innovate | Innovate theme. |
| storm | Storm theme. |
| temperature | Temparature theme. |
| colorBlindSafe | Color blind safe theme. |

### DataSources

The DataSources object in a Microsoft Fabric Map item definition specifies the data inputs used to render the map. It can include arrays of lakehouse or KQL databases, which serve as the primary sources of geospatial and analytical data. These sources feed into the map layers, enabling dynamic visualization and interaction based on structured datasets.

| Property | Type | Required | Description |
|---|---|---|---|
| `lakehouses` | [DataSource[]](#datasource)  | true | Array of lakehouse data sources. |
| `kqlDataBases` | [DataSource[]](#datasource)  | true | Array of KQL database data sources. |

#### DataSource

DataSource typically refers to a single data input—a specific lakehouse, KQL database that provides geospatial or analytical data for the map. It contains configuration details for one source.

| Property | Type | Required | Description |
|---|---|---|---|
| `workspaceId` | Guid  | false | The workspace ID of the data source. |
| `artifactId` | Guid  | false | The artifact ID of the data source. |

### LayerSources

The LayerSources object in a Microsoft Fabric Map item definition specifies an array of LayerSource objects to be displayed on the map. Each LayerSource object defines the source data for a specific layer, enabling the rendering of visual elements such as bubbles, lines, and polygons. This object works in conjunction with layer settings to control how each layer appears and behaves on the map.

| Property | Type | Required | Description |
|---|---|---|---|
| `layerSources` | [LayerSource[]](#layersource)  | true | Array of layer sources for the map. |

#### LayerSource

The LayerSource object in a Microsoft Fabric Map item definition defines the source data for a specific layer, enabling the rendering of visual elements such as bubbles, lines, and polygons. This object works in conjunction with layer settings to control how each layer appears and behaves on the map.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | false | Unique identifier for the layer source. |
| `name` | string  | false | Name of the layer source. |
| `type` | string  | false | Type of the layer source. |
| `options` | string  | false | Options for the layer source. |
| `parentArtifactId` | Guid  | false | ID of the parent artifact. |
| `relativePath` | string  | false | Relative path to the data source. |
| `querysetTabId` | string  | false | ID of the queryset and tab in format 'querysetId_tabId'. |
| `latitudeColumnName` | string  | false | Name of the latitude column. |
| `longitudeColumnName` | string  | false | Name of the longitude column. |
| `geometryColumnName` | string  | false | Name of the geometry column. |
| `refreshIntervalMs` | integer  | false | Refresh interval in milliseconds. |

### LayerSettings

The LayerSettings object in a Microsoft Fabric Map item definition specifies an array of LayerSetting objects.

| Property | Type | Required | Description |
|---|---|---|---|
| `layerSettings` | [LayerSetting[]](#layersetting)  | true | Array of layer settings for the map. |

#### LayerSetting

The LayerSetting object in a Microsoft Fabric Map item definition controls how each map layer is rendered and behaves. It includes configuration options for visual properties such as color, visibility, and geometry type—whether the layer appears as bubbles, lines, or polygons. These settings allow for precise customization of how data is visually represented on the map, enhancing clarity and user interaction.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | false | Unique identifier for the layer setting. |
| `name` | string  | false | Name of the layer setting. |
| `sourceId` | Guid  | false | ID of the associated layer source. |
| `sourceLayerId` | string  | false | ID of the specific layer within the source. |
| `options` | string  | false | Options for the layer rendering. |

### MapDetails example
  
```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/map/definition/1.0.0/schema.json",
  "basemap": {
    "options": "{\"renderWorldCopies\":true}",
    "controls": {
      "zoom": true,
      "pitch": true,
      "compass": true,
      "scale": true,
      "traffic": true
    },
    "backgroundColor": null,
    "theme": null
  },
  "dataSources": {
    "lakehouses": [
      {
        "workspaceId": "00000000-0000-0000-0000-000000000000",
        "artifactId": "d66a4449-bd7a-8e29-4654-ebd526fcea0e"
      }
    ],
    "kqlDataBases": [
      {
        "workspaceId": "00000000-0000-0000-0000-000000000000",
        "artifactId": "c86d4691-3946-ab72-49a0-ec4e1f042f3f"
      }
    ]
  },
  "layerSources": [
    {
      "id": "69d8c45f-de55-45ad-a03b-812752a0c07e",
      "name": "Files/land_places.pmtiles",
      "type": "pmtiles",
      "options": null,
      "parentArtifactId": "d66a4449-bd7a-8e29-4654-ebd526fcea0e",
      "relativePath": "Files/land_places.pmtiles",
      "querysetTabId": null,
      "latitudeColumnName": null,
      "longitudeColumnName": null,
      "geometryColumnName": null,
      "refreshIntervalMs": 0
    },
    {
      "id": "106ea450-aecc-4e62-beaf-d4f342baec63",
      "name": "Files/ne_50m_populated_places.geojson",
      "type": "geojson",
      "options": null,
      "parentArtifactId": "d66a4449-bd7a-8e29-4654-ebd526fcea0e",
      "relativePath": "Files/ne_50m_populated_places.geojson",
      "querysetTabId": null,
      "latitudeColumnName": null,
      "longitudeColumnName": null,
      "geometryColumnName": null,
      "refreshIntervalMs": 0
    },
    {
      "id": "a56fa15f-cb42-40a7-b871-64aabff56b01",
      "name": "Files/sample.geojson",
      "type": "geojson",
      "options": null,
      "parentArtifactId": "d66a4449-bd7a-8e29-4654-ebd526fcea0e",
      "relativePath": "Files/sample.geojson",
      "querysetTabId": null,
      "latitudeColumnName": null,
      "longitudeColumnName": null,
      "geometryColumnName": null,
      "refreshIntervalMs": 0
    },
    {
      "id": "681a8dc1-7ce7-4da8-a315-781bececfcbc",
      "name": "Sample_Record",
      "type": "kusto",
      "options": null,
      "parentArtifactId": "c86d4691-3946-ab72-49a0-ec4e1f042f3f",
      "relativePath": null,
      "querysetTabId": "cf1e02ff-1c9e-480f-afa7-18c22c452bdd_8aacc176-8b95-4d66-a3df-99636a65fe95",
      "latitudeColumnName": "sample_latitude",
      "longitudeColumnName": "sample_longitude",
      "geometryColumnName": "sample_geometry",
      "refreshIntervalMs": 0
    }
  ],
  "layerSettings": [
    {
      "id": "f41e4422-e36f-499d-9738-1e39f6096f3f",
      "name": "Files/land_places.pmtiles (ne_110m_land)",
      "sourceId": "69d8c45f-de55-45ad-a03b-812752a0c07e",
      "sourceLayerId": "ne_110m_land",
      "options": "{\"color\":\"#000000\",\"visible\":true,\"bubbleOptions\":{\"color\":\"#000000\"},\"lineOptions\":{\"strokeColor\":\"#000000\"},\"polygonOptions\":{\"fillColor\":\"#000000\"},\"polygonExtrusionOptions\":{\"fillColor\":\"#000000\"}}"
    },
    {
      "id": "09a210a8-3d0f-4e1b-a54d-94a0c771bfff",
      "name": "Files/ne_50m_populated_places.geojson",
      "sourceId": "106ea450-aecc-4e62-beaf-d4f342baec63",
      "sourceLayerId": null,
      "options": "{\"color\":\"#12239E\",\"visible\":true}"
    },
    {
      "id": "b0550a13-9493-4d87-8929-09ba31f9417b",
      "name": "Files/sample.geojson",
      "sourceId": "a56fa15f-cb42-40a7-b871-64aabff56b01",
      "sourceLayerId": null,
      "options": "{\"color\":\"#744EC2\",\"visible\":true,\"bubbleOptions\":{\"color\":\"#744EC2\"},\"lineOptions\":{\"strokeColor\":\"#744EC2\"},\"polygonOptions\":{\"fillColor\":\"#744EC2\"},\"polygonExtrusionOptions\":{\"fillColor\":\"#744EC2\"}}"
    },
    {
      "id": "70ecfc25-62c5-4e81-89b3-85aecce23886",
      "name": "Sample_Record",
      "sourceId": "681a8dc1-7ce7-4da8-a315-781bececfcbc",
      "sourceLayerId": null,
      "options": "{\"color\":\"#D90079\",\"visible\":true}"
    }
  ]
}
```
