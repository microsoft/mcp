# Map item definition

This article explains how to create and structure a Map item definition using the Microsoft Fabric REST API.

## Supported formats

Map items must be defined in **JSON** format.

## Definition structure

The definition consists of one required part and two optional parts as shown in the following table.

| Definition part path | Type | Required | Description |
|---|---|---|---|
| `map.json` | [MapDetails](#mapdetails) (JSON) | true | Contains the core map details that define the Map item. |
| `queries` | [Directory](#queries-directory-layersourcekql-file) | false | Contains a list of Kusto query files that are part of map layer sources. Each Kusto query file (.kql) belongs to an individual layer in the map item and must be named using the convention `layerSource-<layerSourceId>.kql`, where `<layerSourceId>` matches the corresponding layer source identifier in `map.json`. |
| `.platform`| [PlatformDetails](#platformdetails) (JSON) | false | Contains platform/environment metadata that describes the common details of the Map item. |

The service resolves layer queries by looking for files under the `queries` directory whose names follow this pattern. For example, a layer source with ID `00000000-0000-0000-0000-000000000000` in `map.json` is associated with the file `queries/layerSource-00000000-0000-0000-0000-000000000000.kql`.

## Definition example

```json
{
  "definition": {
    "parts": [
      {
        "path": "map.json",
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9tYXAvZGVmaW5pdGlvbi8yLjAuMC9zY2hlbWEuanNvbiIsDQogICJiYXNlbWFwIjoge30sDQogICJkYXRhU291cmNlcyI6IFtdLA0KICAiaWNvblNvdXJjZXMiOiBbXSwNCiAgImxheWVyU291cmNlcyI6IFtdLA0KICAibGF5ZXJTZXR0aW5ncyI6IFtdDQp9",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```

### Definition example with optional definition parts

```json
{
  "definition": {
    "parts": [
      {
        "path": "map.json",
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9tYXAvZGVmaW5pdGlvbi8yLjAuMC9zY2hlbWEuanNvbiIsDQogICJiYXNlbWFwIjogew0KICAgICJvcHRpb25zIjogew0KICAgICAgInN0eWxlIjogImJsYW5rIg0KICAgIH0NCiAgfSwNCiAgImRhdGFTb3VyY2VzIjogWw0KICAgIHsNCiAgICAgICJpdGVtVHlwZSI6ICJMYWtlaG91c2UiLA0KICAgICAgIndvcmtzcGFjZUlkIjogImM1MjQ5NmI2LTA2YTYtNGVhZS1iNjQ2LTMxNTU0MjU1MGE4NSIsDQogICAgICAiaXRlbUlkIjogIjBlY2JhYTgwLWE3M2UtNGIzMC1hM2Y0LWU0MWMyMDg2ZDU0ZiINCiAgICB9LA0KICAgIHsNCiAgICAgICJpdGVtVHlwZSI6ICJLcWxEYXRhYmFzZSIsDQogICAgICAid29ya3NwYWNlSWQiOiAiYzUyNDk2YjYtMDZhNi00ZWFlLWI2NDYtMzE1NTQyNTUwYTg1IiwNCiAgICAgICJpdGVtSWQiOiAiMTdkY2M3YzUtYzdkOC00ZTI3LWFmMWMtZjVkYzIyOTc2ZjE4Ig0KICAgIH0sDQogICAgew0KICAgICAgIml0ZW1UeXBlIjogIkNvbm5lY3Rpb24iLA0KICAgICAgImNvbm5lY3Rpb25JZCI6ICI3MTc0MmEzYS01YTljLTQ4ZTgtODU1ZC1hMzhlZjUxMTk3YzMiDQogICAgfQ0KICBdLA0KICAiaWNvblNvdXJjZXMiOiBbXSwNCiAgImxheWVyU291cmNlcyI6IFsNCiAgICB7DQogICAgICAiaWQiOiAiMGM4MmJjZjYtNjQ5ZC00NmYxLTkwOTctODQ5MDBmMjRkOGQ5IiwNCiAgICAgICJuYW1lIjogInRhYjEiLA0KICAgICAgInR5cGUiOiAia3VzdG8iLA0KICAgICAgIm9wdGlvbnMiOiB7DQogICAgICAgICJjbHVzdGVyIjogZmFsc2UNCiAgICAgIH0sDQogICAgICAiaXRlbUlkIjogIjE3ZGNjN2M1LWM3ZDgtNGUyNy1hZjFjLWY1ZGMyMjk3NmYxOCIsDQogICAgICAicmVmcmVzaEludGVydmFsTXMiOiAwDQogICAgfSwNCiAgICB7DQogICAgICAiaWQiOiAiN2JlYTg5NmItZTNmMy00NjYxLWEzN2EtYmFhZDMzNjUyODY0IiwNCiAgICAgICJuYW1lIjogInNjaGVkdWxlZF90aWxlc2V0IiwNCiAgICAgICJ0eXBlIjogInBtdGlsZXMiLA0KICAgICAgIml0ZW1JZCI6ICIwZWNiYWE4MC1hNzNlLTRiMzAtYTNmNC1lNDFjMjA4NmQ1NGYiLA0KICAgICAgInJlbGF0aXZlUGF0aCI6ICJGaWxlcy9zY2hlZHVsZWRfdGlsZXNldC5wbXRpbGVzIiwNCiAgICAgICJyZWZyZXNoSW50ZXJ2YWxNcyI6IDANCiAgICB9LA0KICAgIHsNCiAgICAgICJpZCI6ICIzMjY1MDhhNC04M2I5LTQ1YjQtYWYxNC1kZmVhOWRkZTc2YjIiLA0KICAgICAgIm5hbWUiOiAiVG9tVG9tIE1hcCIsDQogICAgICAidHlwZSI6ICJjb25uZWN0aW9uIiwNCiAgICAgICJpdGVtSWQiOiAiZDVhMGYxNWYtMmQ1Ni00ZjA4LWFlNDktZmFjZmFjNGZkNmY2IiwNCiAgICAgICJyZWZyZXNoSW50ZXJ2YWxNcyI6IDAsDQogICAgICAiY29ubmVjdGlvbklkIjogIjcxNzQyYTNhLTVhOWMtNDhlOC04NTVkLWEzOGVmNTExOTdjMyIsDQogICAgICAiY29ubmVjdGlvblJlc291cmNlSWQiOiAiYmFzaWMiDQogICAgfSwNCiAgICB7DQogICAgICAiaWQiOiAiNWRmMGIxYTgtMDZkNC00OWI1LThlYmMtMDAxODJjMDBkZDhhIiwNCiAgICAgICJuYW1lIjogInJlc3VsdEZyb21PbmVEYXkiLA0KICAgICAgInR5cGUiOiAia3VzdG8iLA0KICAgICAgIm9wdGlvbnMiOiB7DQogICAgICAgICJjbHVzdGVyIjogdHJ1ZSwNCiAgICAgICAgImNsdXN0ZXJQcm9wZXJ0aWVzIjogew0KICAgICAgICAgICJjbHVzdGVyX3ZhbHVlIjogWw0KICAgICAgICAgICAgIisiLA0KICAgICAgICAgICAgWw0KICAgICAgICAgICAgICAiZ2V0IiwNCiAgICAgICAgICAgICAgIkNvbHVtbjEiDQogICAgICAgICAgICBdDQogICAgICAgICAgXQ0KICAgICAgICB9LA0KICAgICAgICAiY2x1c3RlckZpZWxkIjogIkNvbHVtbjEiLA0KICAgICAgICAiY2x1c3RlclR5cGUiOiAiYXZnIg0KICAgICAgfSwNCiAgICAgICJpdGVtSWQiOiAiMTdkY2M3YzUtYzdkOC00ZTI3LWFmMWMtZjVkYzIyOTc2ZjE4IiwNCiAgICAgICJyZWZyZXNoSW50ZXJ2YWxNcyI6IDANCiAgICB9DQogIF0sDQogICJsYXllclNldHRpbmdzIjogWw0KICAgIHsNCiAgICAgICJpZCI6ICJhY2E1NDkxMS1lMWU2LTQ3YjgtODM3MC0xN2ZiZDdmOWY4Y2EiLA0KICAgICAgIm5hbWUiOiAidGFiMVF1ZXJ5UmVzdWx0IiwNCiAgICAgICJzb3VyY2VJZCI6ICI1ZGYwYjFhOC0wNmQ0LTQ5YjUtOGViYy0wMDE4MmMwMGRkOGEiLA0KICAgICAgIm9wdGlvbnMiOiB7DQogICAgICAgICJjb2xvciI6ICIjOTM3M0MwIiwNCiAgICAgICAgInR5cGUiOiAidmVjdG9yIiwNCiAgICAgICAgInZpc2libGUiOiB0cnVlLA0KICAgICAgICAiYnViYmxlT3B0aW9ucyI6IHsNCiAgICAgICAgICAiY2x1c3RlclJhZGl1cyI6IDMwDQogICAgICAgIH0NCiAgICAgIH0sDQogICAgICAibGF0aXR1ZGVDb2x1bW5OYW1lIjogIkNvbHVtbjciLA0KICAgICAgImxvbmdpdHVkZUNvbHVtbk5hbWUiOiAiQ29sdW1uOCINCiAgICB9LA0KICAgIHsNCiAgICAgICJpZCI6ICIyNjc0ZmEwNi04Y2NhLTQ4ZTEtODU3ZC1jMjQ1ZmRiYTIxYWIiLA0KICAgICAgIm5hbWUiOiAidGFiMSIsDQogICAgICAic291cmNlSWQiOiAiMGM4MmJjZjYtNjQ5ZC00NmYxLTkwOTctODQ5MDBmMjRkOGQ5IiwNCiAgICAgICJvcHRpb25zIjogew0KICAgICAgICAiY29sb3IiOiAiIzNBOTZERCIsDQogICAgICAgICJ0eXBlIjogInZlY3RvciIsDQogICAgICAgICJ2aXNpYmxlIjogdHJ1ZSwNCiAgICAgICAgImJ1YmJsZU9wdGlvbnMiOiB7DQogICAgICAgICAgImNvbG9yIjogIiMzQTk2REQiLA0KICAgICAgICAgICJzaXplVHlwZSI6ICJmaXhlZCINCiAgICAgICAgfSwNCiAgICAgICAgIm1hcmtlck9wdGlvbnMiOiB7DQogICAgICAgICAgImZpbGxDb2xvciI6ICIjM0E5NkREIiwNCiAgICAgICAgICAiaWNvbk9wdGlvbnMiOiB7DQogICAgICAgICAgICAiaW1hZ2UiOiAiMjY3NGZhMDYtOGNjYS00OGUxLTg1N2QtYzI0NWZkYmEyMWFiOlNxdWFyZSINCiAgICAgICAgICB9DQogICAgICAgIH0sDQogICAgICAgICJsaW5lT3B0aW9ucyI6IHsNCiAgICAgICAgICAic3Ryb2tlQ29sb3IiOiAiIzNBOTZERCINCiAgICAgICAgfSwNCiAgICAgICAgInBvbHlnb25PcHRpb25zIjogew0KICAgICAgICAgICJmaWxsQ29sb3IiOiAiIzNBOTZERCINCiAgICAgICAgfSwNCiAgICAgICAgInBvbHlnb25FeHRydXNpb25PcHRpb25zIjogew0KICAgICAgICAgICJmaWxsQ29sb3IiOiAiIzNBOTZERCINCiAgICAgICAgfSwNCiAgICAgICAgImRhdGFMYWJlbE9wdGlvbnMiOiB7DQogICAgICAgICAgImVuYWJsZWQiOiB0cnVlLA0KICAgICAgICAgICJwbGFjZW1lbnQiOiAicG9pbnQiLA0KICAgICAgICAgICJmb250V2VpZ2h0IjogIkJvbGQiLA0KICAgICAgICAgICJhbmNob3IiOiAicmlnaHQiDQogICAgICAgIH0sDQogICAgICAgICJkYXRhTGFiZWxLZXlzIjogWw0KICAgICAgICAgICJDb2x1bW4yIg0KICAgICAgICBdLA0KICAgICAgICAicG9pbnRMYXllclR5cGUiOiAiYnViYmxlIg0KICAgICAgfSwNCiAgICAgICJsYXRpdHVkZUNvbHVtbk5hbWUiOiAiQ29sdW1uNyIsDQogICAgICAibG9uZ2l0dWRlQ29sdW1uTmFtZSI6ICJDb2x1bW44IiwNCiAgICAgICJmaWx0ZXJzIjogWw0KICAgICAgICB7DQogICAgICAgICAgInR5cGUiOiAidGV4dCIsDQogICAgICAgICAgInZhbHVlIjogWw0KICAgICAgICAgICAgIkdyZWVubGFuZCINCiAgICAgICAgICBdLA0KICAgICAgICAgICJpZCI6ICIwMmE2NDZjYi1hMzA4LTQwYWUtOTNhZi1lODE1Mjc3MzI2YWUiLA0KICAgICAgICAgICJmaWVsZCI6ICJDb2x1bW40IiwNCiAgICAgICAgICAibG9ja2VkIjogZmFsc2UNCiAgICAgICAgfQ0KICAgICAgXQ0KICAgIH0sDQogICAgew0KICAgICAgImlkIjogIjcxZTJhNzNlLTU3MmQtNDRlMC1iNDE3LWFjY2E3ZDBmMGJlMSIsDQogICAgICAibmFtZSI6ICJhZGRyZXNzIChzY2hlZHVsZWRfdGlsZXNldCkiLA0KICAgICAgInNvdXJjZUlkIjogIjdiZWE4OTZiLWUzZjMtNDY2MS1hMzdhLWJhYWQzMzY1Mjg2NCIsDQogICAgICAic291cmNlTGF5ZXJJZCI6ICJhZGRyZXNzIiwNCiAgICAgICJvcHRpb25zIjogew0KICAgICAgICAiY29sb3IiOiAiI0NBNTAxMCIsDQogICAgICAgICJ0eXBlIjogInZlY3RvciIsDQogICAgICAgICJ2aXNpYmxlIjogdHJ1ZSwNCiAgICAgICAgInNvdXJjZUxheWVyIjogImFkZHJlc3MiDQogICAgICB9LA0KICAgICAgImZpbHRlcnMiOiBbDQogICAgICAgIHsNCiAgICAgICAgICAidHlwZSI6ICJ0ZXh0IiwNCiAgICAgICAgICAidmFsdWUiOiBbDQogICAgICAgICAgICAiTk8iDQogICAgICAgICAgXSwNCiAgICAgICAgICAiaWQiOiAiZWE3NTQwNzYtOTdkOC00NDBhLTk1MjItYWY2OWZjZWM4NjIwIiwNCiAgICAgICAgICAiZmllbGQiOiAiQUNUSVZFIiwNCiAgICAgICAgICAibG9ja2VkIjogZmFsc2UNCiAgICAgICAgfQ0KICAgICAgXQ0KICAgIH0sDQogICAgew0KICAgICAgImlkIjogIjA3YTc5MTY3LWYyOGItNDVhNS1hYjE3LTc1ZTMwN2NiMjQ5ZiIsDQogICAgICAibmFtZSI6ICJUb21Ub20gTWFwIiwNCiAgICAgICJzb3VyY2VJZCI6ICIzMjY1MDhhNC04M2I5LTQ1YjQtYWYxNC1kZmVhOWRkZTc2YjIiLA0KICAgICAgInNvdXJjZUxheWVySWQiOiAiY29ubmVjdGlvbiIsDQogICAgICAib3B0aW9ucyI6IHsNCiAgICAgICAgInR5cGUiOiAicmFzdGVyIiwNCiAgICAgICAgInZpc2libGUiOiB0cnVlLA0KICAgICAgICAic291cmNlTGF5ZXIiOiAiY29ubmVjdGlvbiIsDQogICAgICAgICJvcGFjaXR5IjogMQ0KICAgICAgfQ0KICAgIH0NCiAgXQ0KfQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": "queries/layerSource-0c82bcf6-649d-46f1-9097-84900f24d8d9.kql",
        "payload": "dGFiMQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": "queries/layerSource-5df0b1a8-06d4-49b5-8ebc-00182c00dd8a.kql",
        "payload": "dGFiMQp8IHdoZXJlIENvbHVtbjkgPiAwIGFuZCBDb2x1bW45IDwgMjAw",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9naXRJbnRlZ3JhdGlvbi9wbGF0Zm9ybVByb3BlcnRpZXMvMi4wLjAvc2NoZW1hLmpzb24iLAogICJtZXRhZGF0YSI6IHsKICAgICJ0eXBlIjogIk1hcCIsCiAgICAiZGlzcGxheU5hbWUiOiAibWFwX2FsbGNvbm5lY3Rpb25zIgogIH0sCiAgImNvbmZpZyI6IHsKICAgICJ2ZXJzaW9uIjogIjIuMCIsCiAgICAibG9naWNhbElkIjogIjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIKICB9Cn0=",
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
| `$schema` | String  | true | The schema version for the map definition. |
| `basemap` | [BaseMap](#basemap)  | false | Configuration for base map settings such as controls, background color, and theme. |
| `dataSources` | [DataSource[]](#datasource)  | false | Array of data sources for the map, which can include workspace items (Lakehouse, KQLDatabase, Ontology) or connections. |
| `iconSources` | [IconSource[]](#iconsource)  | false | Array of icon sources for the map. |
| `layerSources` | [LayerSource[]](#layersource)  | false | Array of layer sources for the map. |
| `layerSettings` | [LayerSetting[]](#layersetting)  | false | Array of layer settings for the map. |

The data source items cannot be deleted if the Map still exists.

### BaseMap

The BaseMap properties in a Microsoft Fabric Map item definition configure the visual and interactive aspects of the map. The options property allows customization of map behavior, such as center position, zoom level, pitch, bearing, style, and style overrides. The controls property specifies which interactive elements are visible to users. backgroundColor sets the map's background color, enhancing visual clarity or thematic alignment. The theme property defines the overall aesthetic style of the map, such as light or dark mode, ensuring consistency with user preferences or application design.

| Property | Type | Required | Description |
|---|---|---|---|
| `options` | [BaseMapOptions](#basemapoptions)  | false | Options for the map view and style configuration. |
| `controls` | [Controls](#controls)  | false | Map control settings. |
| `backgroundColor` | String  | false | Background color of the map. |
| `theme` | [Theme (Enum)](#theme-enum)  | false | Theme for the map. |

#### BaseMapOptions

The BaseMapOptions object configures the map view and rendering behavior.

| Property | Type | Required | Description |
|---|---|---|---|
| `center` | Double[]  | false | The position to align the center of the map view with (longitude, latitude). |
| `zoom` | Double  | false | The zoom level of the map view. |
| `pitch` | Double  | false | The pitch (tilt) of the map in degrees between 0 and 60, where 0 is looking straight down on the map. |
| `bearing` | Double  | false | The bearing of the map (rotation) in degrees. When the bearing is 0, 90, 180, or 270, the top of the map container will be north, east, south, or west, respectively. |
| `style` | String  | false | The name of the style to use when rendering the map. The default style is "road". |
| `showLabels` | Boolean  | false | Specifies if the map should display labels. |
| `language` | String  | false | The language of the map labels. If set to "auto", the browser's preferred language will be used. |
| `renderWorldCopies` | Boolean  | false | Specifies if multiple copies of the world should be rendered when zoomed out. |
| `styleOverrides` | [StyleOverrides](#styleoverrides)  | false | Override the default styles for map elements. |
| `view` | String  | false | The view to show the correct maps of geopolitically disputed regions for a certain country/region. If not specified, the default unified view will be used. |

#### StyleOverrides

The StyleOverrides object allows customization of map element visibility.

| Property | Type | Required | Description |
|---|---|---|---|
| `countryRegion` | [BorderedMapElementStyles](#borderedmapelementstyles)  | false | Country or region border visibility settings. |
| `adminDistrict` | [BorderedMapElementStyles](#borderedmapelementstyles)  | false | First administrative level (state/province) border visibility settings. |
| `adminDistrict2` | [BorderedMapElementStyles](#borderedmapelementstyles)  | false | Second administrative level (county) border visibility settings. |
| `roadDetails` | [MapElementStyles](#mapelementstyles)  | false | Street blocks visibility settings. |
| `buildingFootprint` | [MapElementStyles](#mapelementstyles)  | false | Building footprints visibility settings. |

##### BorderedMapElementStyles

Visibility settings for bordered map elements such as country/region and administrative district boundaries.

| Property | Type | Required | Description |
|---|---|---|---|
| `borderVisible` | Boolean  | false | Specifies the visibility of the border. |

##### MapElementStyles

Visibility settings for map elements such as road details and building footprints.

| Property | Type | Required | Description |
|---|---|---|---|
| `visible` | Boolean  | false | Specifies the visibility of the element. |

#### Controls

The Controls object in a Microsoft Fabric Map item definition specifies the interactive elements available on the map interface. These include options such as zoom, pitch, compass, scale, traffic, and style, allowing users to navigate and interact with the map more effectively. Each control enhances usability by providing intuitive ways to explore spatial data.

| Property | Type | Required | Description |
|---|---|---|---|
| `zoom` | Boolean  | false | Enable zoom control. |
| `pitch` | Boolean  | false | Enable pitch control. |
| `compass` | Boolean  | false | Enable compass control. |
| `scale` | Boolean  | false | Enable scale control. |
| `traffic` | Boolean  | false | Enable traffic control. |
| `style` | Boolean  | false | Enable style control. |

#### Theme (Enum)

The Theme enum defines the string values available to configure the overall base map style.

| Name | Description |
|------|-------------|
| default | Default theme. |
| classic | Classic theme. |
| innovate | Innovate theme. |
| storm | Storm theme. |
| temperature | Temperature theme. |
| colorBlindSafe | Color blind safe theme. |

### DataSource

The `dataSources` array in a Microsoft Fabric Map item definition specifies the data inputs used to render the map. Each data source can be either a workspace item (Lakehouse, KQLDatabase, Ontology, etc.) or a Connection. These sources feed into the map layers, enabling dynamic visualization and interaction based on structured datasets.

#### Workspace Item DataSource

A workspace item data source references a Fabric item such as a Lakehouse, KQLDatabase, or Ontology.

| Property | Type | Required | Description |
|---|---|---|---|
| `itemType` | String  | true | The type of the data source item (e.g., "Lakehouse", "KQLDatabase", "Ontology"). |
| `workspaceId` | Guid  | true | The workspace ID of the item. |
| `itemId` | Guid  | true | The item ID. |

#### Connection DataSource

A connection data source references an external connection.

| Property | Type | Required | Description |
|---|---|---|---|
| `itemType` | String  | true | Must be "Connection". |
| `connectionId` | Guid  | true | The connection ID. |

### IconSource

The IconSource object defines custom icons that can be used for marker layers.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the icon source. |
| `name` | String  | true | Name of the icon source. |
| `type` | String  | true | Type of the icon source. |
| `itemId` | Guid  | true | ID of the data source item for this icon source. |
| `relativePath` | String  | true | Relative path to the icon source. |

### LayerSource

The LayerSource object in a Microsoft Fabric Map item definition defines the source data for a specific layer, enabling the rendering of visual elements such as bubbles, lines, and polygons. This object works in conjunction with layer settings to control how each layer appears and behaves on the map.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the layer source. |
| `name` | String  | true | Name of the layer source. |
| `type` | String  | true | Type of the layer source (e.g., "geojson", "pmtiles", "kusto"). |
| `options` | [LayerSourceOptions](#layersourceoptions)  | false | Options for the layer source including clustering configuration. |
| `itemId` | Guid  | false | ID of the data source item for this layer. |
| `relativePath` | String  | false | Relative path to the data source. |
| `ontologyEntityId` | String  | false | ID of the ontology entity. |
| `refreshIntervalMs` | Integer  | false | Refresh interval in milliseconds. |
| `connectionId` | Guid  | false | ID of the connection for external data sources. |
| `connectionResourceId` | String  | false | Identifier of a resource provided by the connection (e.g., layer name, asset name). |

#### LayerSourceOptions

The LayerSourceOptions object configures clustering behavior for point data.

| Property | Type | Required | Description |
|---|---|---|---|
| `cluster` | Boolean  | false | Enable clustering of point features. |
| `clusterProperties` | [ClusterProperties](#clusterproperties)  | false | Defines custom properties calculated using expressions against all points within each cluster. |
| `clusterField` | String  | false | Field name to use for cluster aggregation. |
| `clusterType` | String  | false | Type of cluster aggregation to perform. |

##### ClusterProperties

The ClusterProperties object is a key-value map where each key is a custom property name and the value is an [AggregateExpression](#aggregateexpression) that defines how to calculate that property across all points within a cluster.

| Property | Type | Required | Description |
|---|---|---|---|
| `{propertyName}` | [AggregateExpression](#aggregateexpression)  | false | A named aggregate expression. The key is the resulting property name on each cluster point. |

##### AggregateExpression

An aggregate expression defines a calculation processed over a set of data points within a cluster. It is represented as an array with the following structure:

`[operator, initialValue, mapExpression]`

| Element | Type | Required | Description |
|---|---|---|---|
| `operator` | String or String[]  | true | An expression function applied against all values calculated by the `mapExpression` for each point in the cluster. Supported operators for numbers: `+`, `*`, `max`, `min`. Supported operators for Booleans: `all`, `any`. |
| `initialValue` | Any  | false | An initial value against which the first calculated value is aggregated. |
| `mapExpression` | String[]  | true | An expression applied against each point in the data set (e.g., `["get", "fieldName"]`). |

### LayerSetting

The LayerSetting object in a Microsoft Fabric Map item definition controls how each map layer is rendered and behaves. It includes configuration options for visual properties such as color, visibility, geometry type, and data-driven styling—whether the layer appears as bubbles, heatmaps, markers, lines, or polygons. These settings allow for precise customization of how data is visually represented on the map, enhancing clarity and user interaction.

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the layer setting. |
| `name` | String  | true | Name of the layer setting. |
| `sourceId` | Guid  | true | ID of the associated layer source. |
| `sourceLayerId` | String  | false | ID of the specific layer within the source. |
| `options` | [LayerSettingOptions](#layersettingoptions)  | false | Options for the layer rendering. |
| `latitudeColumnName` | String  | false | Name of the latitude column. |
| `longitudeColumnName` | String  | false | Name of the longitude column. |
| `geometryColumnName` | String  | false | Name of the geometry column. |
| `filters` | [Filter[]](#filter)  | false | Array of data filters to apply to the layer. |

#### LayerSettingOptions

The LayerSettingOptions object provides comprehensive styling configuration for map layers.

| Property | Type | Required | Description |
|---|---|---|---|
| `type` | String  | false | Layer type: "vector" or "raster". |
| `visible` | Boolean  | false | Specifies if the layer is visible. |
| `minZoom` | Integer  | false | Minimum zoom level to render the layer (inclusive). |
| `maxZoom` | Integer  | false | Maximum zoom level to render the layer (exclusive). |
| `color` | String  | false | Base color for the layer. |
| `sourceLayer` | String  | false | Required for vector tile sources to identify which layer to render. |
| `enablePopups` | Boolean  | false | Show popups when shapes are clicked. |
| `heatmapWeight` | String  | false | Name of the numeric property in the data used as the per-feature heatmap weight. This works together with `heatmapOptions.weight` (a numeric intensity multiplier) to determine the final weight applied to each feature. |
| `allowExtrusions` | Boolean  | false | Render polygons with height property as 3D extrusions. |
| `pointLayerType` | String  | false | Type of point layer: "bubble", "heatmap", or "marker". |
| `bubbleOptions` | [BubbleOptions](#bubbleoptions)  | false | Styling options for bubble/point geometries. |
| `heatmapOptions` | [HeatmapOptions](#heatmapoptions)  | false | Styling options for heatmap layers. |
| `markerOptions` | [MarkerOptions](#markeroptions)  | false | Styling options for marker layers. |
| `lineOptions` | [LineOptions](#lineoptions)  | false | Styling options for line geometries. |
| `polygonOptions` | [PolygonOptions](#polygonoptions)  | false | Styling options for polygon fill. |
| `polygonExtrusionOptions` | [PolygonExtrusionOptions](#polygonextrusionoptions)  | false | Styling options for extruded polygons (3D). |
| `dataLabelOptions` | [DataLabelOptions](#datalabeloptions)  | false | Configuration for data labels displayed on features. |
| `dataLabelKeys` | String[]  | false | Property names to display as data labels. |
| `tooltipKeys` | String[]  | false | Property names to display in tooltips. |
| `opacity` | Double  | false | Opacity value between 0 and 1. |

#### BubbleOptions

Styling options for bubble/point geometries.

| Property | Type | Required | Description |
|---|---|---|---|
| `color` | [ColorDefinition](#colordefinition)  | false | The color to fill the circle symbol with. Supports data-driven expressions. |
| `radius` | Double  | false | The radius of the circle symbols in pixels. Must be >= 0. |
| `strokeColor` | [ColorDefinition](#colordefinition)  | false | The color of the circles' outlines. |
| `strokeWidth` | Integer  | false | The width of the circles' outlines in pixels. |
| `opacity` | Double  | false | Opacity value between 0 and 1. |
| `sizeType` | String  | false | Bubble size type: "fixed" or "data-driven". |
| `fixedSize` | Integer  | false | Fixed size for bubbles when sizeType is "fixed" (1-50). |
| `sizeProperty` | String  | false | Property name for data-driven bubble sizes. |
| `clusterRadius` | Integer  | false | Radius of each cluster in pixels. |
| `enableSeriesGroup` | Boolean  | false | Enable grouping by series for color differentiation. |
| `seriesGroup` | String  | false | Property key to use for series grouping. |
| `paletteId` | String  | false | ID of the color palette to use for data-driven colors. |
| `customColors` | [ColorDefinition](#colordefinition)  | false | Custom colors for data-driven styling. |

#### HeatmapOptions

Styling options for heatmap layers.

| Property | Type | Required | Description |
|---|---|---|---|
| `intensity` | Integer  | false | Global heatmap intensity. Higher values increase the visual weight of each point. |
| `opacity` | Double  | false | Opacity value between 0 and 1. |
| `radius` | Integer  | false | Radius in pixels used to render data points. Must be >= 1. |
| `weight` | Double  | false | How much each individual data point contributes to the heatmap. Must be > 0. |

#### MarkerOptions

Styling options for marker layers.

| Property | Type | Required | Description |
|---|---|---|---|
| `enableSeriesGroup` | Boolean  | false | Enable grouping by series for color differentiation. |
| `seriesGroup` | String  | false | Property key to use for series grouping. |
| `fillColor` | [ColorDefinition](#colordefinition)  | false | The color to fill the marker with. |
| `strokeColor` | [ColorDefinition](#colordefinition)  | false | The color of the markers' outlines. |
| `strokeWidth` | Integer  | false | The width of the markers' outlines in pixels. |
| `size` | Integer  | false | The size of the markers in pixels. |
| `clusterSize` | Integer  | false | The size of the clustered markers in pixels. |
| `icon` | String  | false | Icon name to use for the marker. |
| `iconOptions` | [IconOptions](#iconoptions)  | false | Options used to customize the icons of the markers. |

##### IconOptions

Options used to customize the rendering behavior of marker icons.

| Property | Type | Required | Description |
|---|---|---|---|
| `image` | String  | false | The name of the image in the map's image sprite to use for drawing the icon. |
| `anchor` | String  | false | Specifies which part of the icon is placed closest to the anchor position. Supported values: "center", "left", "right", "top", "bottom", "top-left", "top-right", "bottom-left", "bottom-right". |
| `opacity` | Double  | false | A number between 0 and 1 that indicates the opacity at which the icon will be drawn. |
| `rotation` | Double  | false | The amount to rotate the icon clockwise in degrees. |
| `allowOverlap` | Boolean  | false | Specifies if the symbol icon can overlay other symbols on the map. |
| `rotationAlignment` | String  | false | Determines the rotation behavior of icons. Supported values: "auto", "map", "viewport". |
| `pitchAlignment` | String  | false | Specifies the orientation of the icon when the map is pitched. Supported values: "auto", "map", "viewport". |

#### LineOptions

Styling options for line geometries.

| Property | Type | Required | Description |
|---|---|---|---|
| `strokeColor` | [ColorDefinition](#colordefinition)  | false | The color of the line. Supports data-driven expressions. |
| `strokeWidth` | Integer  | false | The width of the line in pixels. Must be >= 0. |
| `strokeOpacity` | Double  | false | Opacity value between 0 and 1. |
| `enableSeriesGroup` | Boolean  | false | Enable grouping by series for color differentiation. |
| `seriesGroup` | String  | false | Property key to use for series grouping. |
| `paletteId` | String  | false | ID of the color palette to use for data-driven colors. |
| `customColors` | [ColorDefinition](#colordefinition)  | false | Custom colors for data-driven styling. |

#### PolygonOptions

Styling options for polygon fill.

| Property | Type | Required | Description |
|---|---|---|---|
| `fillColor` | [ColorDefinition](#colordefinition)  | false | The color to fill the polygons with. Supports data-driven expressions. |
| `fillOpacity` | Double  | false | Opacity value between 0 and 1. |
| `enableSeriesGroup` | Boolean  | false | Enable grouping by series for color differentiation. |
| `seriesGroup` | String  | false | Property key to use for series grouping. |
| `paletteId` | String  | false | ID of the color palette to use for data-driven colors. |
| `customColors` | [ColorDefinition](#colordefinition)  | false | Custom colors for data-driven styling. |

#### PolygonExtrusionOptions

Styling options for extruded polygons (3D).

| Property | Type | Required | Description |
|---|---|---|---|
| `fillColor` | [ColorDefinition](#colordefinition)  | false | The color to fill the polygons with. |
| `fillOpacity` | Double  | false | Opacity value between 0 and 1. |
| `height` | Double  | false | The height in meters to extrude this layer. Must be >= 0. |
| `base` | Double  | false | The base height in meters. Must be >= 0 and <= height. |

#### DataLabelOptions

Configuration for data labels displayed on features.

| Property | Type | Required | Description |
|---|---|---|---|
| `enabled` | Boolean  | false | Whether data labels are enabled. |
| `color` | [ColorDefinition](#colordefinition)  | false | Text color. |
| `size` | Integer  | false | Text size in pixels. |
| `textStrokeColor` | [ColorDefinition](#colordefinition)  | false | Text stroke color. |
| `textStrokeWidth` | Double  | false | Text stroke width in pixels. |
| `allowOverlap` | Boolean  | false | Allow text overlap with other symbols. |

#### ColorDefinition

The ColorDefinition type represents a color value used in data-driven styling. It can be one of the following:

| Type | Description |
|---|---|
| String | A CSS color string (hex, rgb, rgba, hsl, hsla, or named color). |
| String[] | A data-driven expression array for dynamic styling. |

#### Filter

Filters allow you to limit the data displayed on a layer. Four filter types are supported:

##### Text Filter

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the filter. |
| `type` | String  | true | Must be "text". |
| `field` | String  | true | Name of the feature property to filter on. |
| `locked` | Boolean  | true | When true, prevents viewers from modifying or removing this filter. |
| `value` | String[]  | true | Array of text values to filter by. |

##### Boolean Filter

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the filter. |
| `type` | String  | true | Must be "boolean". |
| `field` | String  | true | Name of the feature property to filter on. |
| `locked` | Boolean  | true | When true, prevents viewers from modifying or removing this filter. |
| `value` | Boolean  | true | The boolean value to filter by. |

##### Number Filter

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the filter. |
| `type` | String  | true | Must be "number". |
| `field` | String  | true | Name of the feature property to filter on. |
| `locked` | Boolean  | true | When true, prevents viewers from modifying or removing this filter. |
| `min` | Double  | true | Minimum value (inclusive). |
| `max` | Double  | true | Maximum value (inclusive). |

##### DateTime Filter

| Property | Type | Required | Description |
|---|---|---|---|
| `id` | Guid  | true | Unique identifier for the filter. |
| `type` | String  | true | Must be "datetime". |
| `field` | String  | true | Name of the feature property to filter on. |
| `locked` | Boolean  | true | When true, prevents viewers from modifying or removing this filter. |
| `start` | String  | true | Start of the time range (ISO 8601 format). |
| `end` | String  | true | End of the time range (ISO 8601 format). |

### MapDetails example
  
```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/map/definition/2.0.0/schema.json",
  "basemap": {
    "options": {
      "style": "grayscale_dark",
      "styleOverrides": {
        "adminDistrict2": {
          "borderVisible": false
        }
      }
    }
  },
  "dataSources": [
    {
      "itemType": "Lakehouse",
      "workspaceId": "c52496b6-06a6-4eae-b646-315542550a85",
      "itemId": "0ecbaa80-a73e-4b30-a3f4-e41c2086d54f"
    },
    {
      "itemType": "Connection",
      "connectionId": "4ca78e3c-5f71-4222-9fa3-68a1695b3a71"
    }
  ],
  "iconSources": [],
  "layerSources": [
    {
      "id": "7c7cb07b-5c94-44c9-998e-a0fdb97bdf82",
      "name": "address",
      "type": "pmtiles",
      "itemId": "0ecbaa80-a73e-4b30-a3f4-e41c2086d54f",
      "relativePath": "Files/address.pmtiles",
      "refreshIntervalMs": 0,
      "connectionId": "4ca78e3c-5f71-4222-9fa3-68a1695b3a71"
    },
    {
      "id": "e25c55c2-949e-42e3-9e41-b5955f4de5b8",
      "name": "counties",
      "type": "geojson",
      "itemId": "0ecbaa80-a73e-4b30-a3f4-e41c2086d54f",
      "relativePath": "Files/counties.geojson",
      "refreshIntervalMs": 0
    }
  ],
  "layerSettings": [
    {
      "id": "0d674027-dd0b-420d-99f9-732f6da38f58",
      "name": "address (address)",
      "sourceId": "7c7cb07b-5c94-44c9-998e-a0fdb97bdf82",
      "sourceLayerId": "address",
      "options": {
        "color": "#101FFF",
        "type": "vector",
        "visible": true,
        "sourceLayer": "address",
        "bubbleOptions": {
          "color": "#101FFF"
        },
        "markerOptions": {
          "fillColor": "#101FFF"
        },
        "lineOptions": {
          "strokeColor": "#101FFF"
        },
        "polygonOptions": {
          "fillColor": "#101FFF"
        },
        "polygonExtrusionOptions": {
          "fillColor": "#101FFF"
        },
        "tooltipKeys": [
          "MSTRID",
          "CATEGORY"
        ],
        "pointLayerType": "heatmap"
      }
    },
    {
      "id": "eed125e2-bad3-4b44-9b2c-001002a6d161",
      "name": "counties",
      "sourceId": "e25c55c2-949e-42e3-9e41-b5955f4de5b8",
      "options": {
        "color": "#6DE637",
        "type": "vector",
        "visible": true,
        "bubbleOptions": {
          "color": "#6DE637"
        },
        "markerOptions": {
          "fillColor": "#6DE637"
        },
        "lineOptions": {
          "strokeColor": "#6DE637"
        },
        "polygonOptions": {
          "fillColor": "#6DE637"
        },
        "polygonExtrusionOptions": {
          "fillColor": "#6DE637"
        }
      }
    }
  ]
}
```

## queries directory: LayerSourceKql file

The LayerSourceKql file name is `layerSource-<layerSourceId>.kql`, where `<layerSourceId>` matches the corresponding layer source identifier in `map.json`. Each .kql file contains a Kusto query that determines which data is fetched and rendered on the map for the layer source it is associated with.

### LayerSourceKql file example:

```kql
tab1
| where Column9 > 0 and Column9 < 200
```