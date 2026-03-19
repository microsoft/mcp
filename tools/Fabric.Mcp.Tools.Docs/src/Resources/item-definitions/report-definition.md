# Report definition

This article provides a breakdown of the definition structure for report items.

## Supported formats

Report definitions can use either `PBIR` or `PBIR-Legacy` format, but not both at the same time. The report format matches how it is stored in the service - if it’s stored as a `PBIR`, it will be returned in `PBIR` format.

By default the `PBIR` format is used.

## Definition parts

| Definition part path              | type                                                                         | Required                         | Description                                                                                                                                                                                               |
| --------------------------------- | ---------------------------------------------------------------------------- | -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `StaticResources/`                | StaticResources part                                                         | false                            | A folder that includes resource files specific to the report and loaded by the user, like custom themes, images, and custom visuals.                                                                      |
| `definition/`                     | [definition/ parts](#definition-part) (PBIR)                                 | true <sup>[1](#required1)</sup>  | Definition of the Power BI Report (e.g. pages, visuals, bookmarks) using PBIR JSON format as a folder.                                                                                                    |
| `report.json`                     | report.json part (PBIR-Legacy)                                               | true <sup>[1](#required1)</sup> | Definition of the Power BI Report (e.g. pages, visuals, bookmarks) using PBIR-Legacy single JSON file.                                                                                                    |
| `semanticModelDiagramLayout.json` | [semanticModelDiagramLayout.json part](#semanticmodeldiagramlayoutjson-part) | false                            | Contains data model diagrams describing the structure of the semantic model associated with the report.                                                                                                   |
| `definition.pbir`                 | [definition.pbir part](#definitionpbir-part)                                 | true                             | Overall definition of the report and core settings. Also holds the reference to the semantic model of the report, it's possible to rebind the report to a different semantic model by updating this file. |

<a name="required1">1</a> - The `definition/` part is required for `PBIR` format, while `report.json` is required for `PBIR-Legacy` format. These are mutually exclusive - a report uses one format or the other, not both.

Learn more about report definition files in [Power BI Project documentation](https://learn.microsoft.com/power-bi/developer/projects/projects-report).

## Payload example using `PBIR` format:

```text
Report/
├── StaticResources/
│   ├── RegisteredResources/
│   │   ├── logo.jpg
│   │   ├── CustomTheme4437032645752863.json
├── definition/ 
│   ├── bookmarks/
│   │   ├── Bookmark7c19b7211ada7de10c30.bookmark.json
│   │   ├── bookmarks.json
│   ├── pages/
│   │   ├── 61481e08c8c340011ce0/
│   │   │   ├── visuals/
│   │   │   │   ├── 3852e5607b224b8ebd1a/
│   │   │   │   │   ├── visual.json
│   │   │   │   │   ├── mobile.json
│   │   │   │   ├── 7df3763f63115a096029/
│   │   │   │   │   ├── visual.json
│   │   │   ├── page.json
│   │   ├── pages.json
│   ├── version.json
│   ├── report.json
├── semanticModelDiagramLayout.json 
└── definition.pbir 
```

```json
{
    "parts": [
        {
            "path": "definition/report.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/version.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/pages.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/page.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/3852e5607b224b8ebd1a/visual.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/3852e5607b224b8ebd1a/mobile.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/pages/61481e08c8c340011ce0/visuals/7df3763f63115a096029/visual.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/bookmarks/Bookmark7c19b7211ada7de10c30.bookmark.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/bookmarks/bookmarks.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/RegisteredResources/logo.jpg",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "StaticResources/RegisteredResources/CustomTheme4437032645752863.json",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition.pbir",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        }
    ]
}
```

## definition/ part

Example of `definition/` folder:

```text
definition/ 
├── bookmarks/
│   ├── Bookmark7c19b7211ada7de10c30.bookmark.json
│   ├── bookmarks.json
├── pages/
│   ├── 61481e08c8c340011ce0/
│   │   ├── visuals/
│   │   │   ├── 3852e5607b224b8ebd1a/
│   │   │   │   ├── visual.json
│   │   │   │   ├── mobile.json
│   │   │   ├── 7df3763f63115a096029/
│   │   │   │   ├── visual.json
│   │   ├── page.json
│   ├── pages.json
├── version.json
├── report.json
```

Example of `report.json`

```jsonc
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definition/report/3.1.0/schema.json",
  "themeCollection": {
    "baseTheme": {
      "name": "CY25SU12",
      "reportVersionAtImport": {
        "visual": "2.5.0",
        "report": "3.1.0",
        "page": "2.3.0"
      },
      "type": "SharedResources"
    },
    "customTheme": {
      "name": "AccessibleCityPark",
      "reportVersionAtImport": {
        "visual": "2.5.0",
        "report": "3.1.0",
        "page": "2.3.0"
      },
      "type": "SharedResources"
    }
  },
  "objects": {
    "outspacePane": [
      {
        "properties": {
          "expanded": {
            "expr": {
              "Literal": {
                "Value": "false"
              }
            }
          },
          "visible": {
            "expr": {
              "Literal": {
                "Value": "true"
              }
            }
          }
        }
      }
    ]
  },
  "publicCustomVisuals": [
    "Gantt1448688115699" // Unique name of the custom visual. This name is used in the `visual.visualType` property of the `visual.json` files.
  ],
  "resourcePackages": [
    {
      "name": "SharedResources",
      "type": "SharedResources",
      "items": [
        {
          "name": "CY25SU12",
          "path": "BaseThemes/CY25SU12.json",
          "type": "BaseTheme"
        },
        {
          "name": "AccessibleCityPark",
          "path": "BuiltInThemes/AccessibleCityPark.json",
          "type": "CustomTheme"
        }
      ]
    },
    {
      "name": "RegisteredResources",
      "type": "RegisteredResources",
      "items": [
        {
          "name": "fabric_48_color21993586118811193.svg",
          "path": "fabric_48_color21993586118811193.svg",
          "type": "Image"
        }
      ]
    }
  ],
  "settings": {
    "useStylableVisualContainerHeader": true,
    "defaultFilterActionIsDataFilter": true,
    "defaultDrillFilterOtherVisuals": true,
    "allowChangeFilterTypes": true,
    "allowInlineExploration": true,
    "useEnhancedTooltips": true
  },
  "slowDataSourceSettings": {
    "isCrossHighlightingDisabled": false,
    "isSlicerSelectionsButtonEnabled": false,
    "isFilterSelectionsButtonEnabled": false,
    "isFieldWellButtonEnabled": false,
    "isApplyAllButtonEnabled": false
  }
}
```

Example of `pages/61481e08c8c340011ce0/page.json`

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definition/page/2.0.0/schema.json",
  "name": "61481e08c8c340011ce0",
  "displayName": "Page 1",
  "displayOption": "FitToPage",
  "height": 720,
  "width": 1280
}
```

Example of `pages/61481e08c8c340011ce0/visuals/3852e5607b224b8ebd1a/visual.json`

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definition/visualContainer/2.0.0/schema.json",
  "name": "3852e5607b224b8ebd1a",
  "position": {
    "x": 479.17437557394328,
    "y": 210.50760538349616,
    "z": 1000,
    "height": 272.09459029940882,
    "width": 341.09465386815845,
    "tabOrder": 1000
  },
  "visual": {
    "visualType": "barChart",
    "query": {
      "queryState": {
        "Category": {
          "projections": [
            {
              "field": {
                "Column": {
                  "Expression": {
                    "SourceRef": {
                      "Entity": "Product"
                    }
                  },
                  "Property": "Brand"
                }
              },
              "queryRef": "Product.Brand",
              "active": true
            },
            {
              "field": {
                "Column": {
                  "Expression": {
                    "SourceRef": {
                      "Entity": "Product"
                    }
                  },
                  "Property": "Product"
                }
              },
              "queryRef": "Product.Product",
              "active": false
            }
          ]
        },
        "Series": {
          "projections": [
            {
              "field": {
                "Column": {
                  "Expression": {
                    "SourceRef": {
                      "Entity": "Customer"
                    }
                  },
                  "Property": "Gender"
                }
              },
              "queryRef": "Customer.Gender"
            }
          ]
        },
        "Y": {
          "projections": [
            {
              "field": {
                "Measure": {
                  "Expression": {
                    "SourceRef": {
                      "Entity": "Sales"
                    }
                  },
                  "Property": "Sales Amount"
                }
              },
              "queryRef": "Measure Table.Sales Amount"
            }
          ]
        }
      },
      "sortDefinition": {
        "sort": [
          {
            "field": {
              "Measure": {
                "Expression": {
                  "SourceRef": {
                    "Entity": "Sales"
                  }
                },
                "Property": "Sales Amount"
              }
            },
            "direction": "Descending"
          }
        ]
      }
    },
    "drillFilterOtherVisuals": true
  },
  "filterConfig": {
    "filters": [
      {
        "name": "Filter",
        "field": {
          "Column": {
            "Expression": {
              "SourceRef": {
                "Entity": "Calendar"
              }
            },
            "Property": "Year"
          }
        },
        "type": "Categorical",
        "howCreated": "User"
      }
    ]
  }
}
```


## definition.pbir part

Example of `definition.pbir` file targeting a local semantic model folder:

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definitionProperties/2.0.0/schema.json",
  "version": "4.0",
  "datasetReference": {
    "byPath": {
      "path": "../Sales.SemanticModel"
    }
  }
}
```

Example of `definition.pbir` file targeting a semantic model in a workspace:

```json
{  
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/report/definitionProperties/2.0.0/schema.json",
  "version": "4.0",
  "datasetReference": {
    "byConnection": {      
      "connectionString": "semanticmodelid=[SemanticModelId]"
    }
  }
}
```


## semanticModelDiagramLayout.json part

Example of `semanticModelDiagramLayout.json` file:

```json
{
  "version": "1.1.0",
  "diagrams": [
    {
      "ordinal": 0,
      "scrollPosition": {
        "x": 0,
        "y": 74.883720930232556
      },
      "nodes": [
        {
          "location": {
            "x": 942.5095849858792,
            "y": 14.090768666666882
          },
          "nodeIndex": "[Table Name]",
          "nodeLineageTag": "[Table Lineage Tag]",
          "size": {
            "height": 1000,
            "width": 254
          },
          "zIndex": 5
        },
        {
          "location": {
            "x": 537.83428438628755,
            "y": 836.33418866666739
          },
          "nodeIndex": "[Table Name]",
          "nodeLineageTag": "[Table Lineage Tag]",
          "size": {
            "height": 481,
            "width": 276
          },
          "zIndex": 2
        }
      ],
      "name": "All tables",
      "zoomValue": 74.782608695652172,
      "pinKeyFieldsToTop": false,
      "showExtraHeaderInfo": false,
      "hideKeyFieldsWhenCollapsed": false,
      "tablesLocked": false
    }
  ],
  "selectedDiagram": "All tables",
  "defaultDiagram": "All tables"
}
```