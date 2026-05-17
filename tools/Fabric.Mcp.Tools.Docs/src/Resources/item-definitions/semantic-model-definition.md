# SemanticModel definition

This article provides a breakdown of the definition structure for semantic model items.

## Supported formats

Semantic model definitions can use either `TMDL` or `TMSL` format, but not both at the same time.

By default the `TMDL` format is used.

## Definition parts

| Definition part path | type                                               | Required | Description                                                                                                                        |
| -------------------- | -------------------------------------------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `definition/`        | [definition/ parts](#definition-part) (TMDL)       | true <sup>[1](#required1)</sup>     | Analysis Services tabular definition using [TMDL](https://learn.microsoft.com/analysis-services/tmdl/tmdl-overview) format.                                   |
| `model.bim`          | [model.bim part](#modelbim-part) (TMSL)            | true <sup>[1](#required1)</sup>     | Analysis Services tabular definition using [TMSL](https://learn.microsoft.com/analysis-services/tmsl/tabular-model-scripting-language-tmsl-reference) format. |
| `diagramLayout.json` | [diagramLayout.json part](#diagramlayoutjson-part) | false       | Contains diagram metadata of the semantic model.                                                                                   |
| `definition.pbism`   | [definition.pbism part](#definitionpbism-part)     | true      | Contains core settings about the semantic model and file format version.                                                           |

<a name="required1">1</a> - The `definition/` part is required for `TMDL` format, while `model.bim` is required for `TMSL` format. These are mutually exclusive - a semantic model uses one format or the other, not both.

Learn more about semantic model definition files in [Power BI Project documentation](https://learn.microsoft.com/power-bi/developer/projects/projects-dataset).

## Payload example using `TMDL` format:

```text
SemanticModel/
├── definition/ 
│   ├── tables/
│   │   ├── product.tmdl
│   │   ├── sales.tmdl
│   │   ├── calendar.tmdl
│   ├── relationships.tmdl
│   ├── model.tmdl
│   ├── database.tmdl
├── Copilot/
│   ├── Instructions/
│   │   ├── instructions.md
│   │   ├── version.json
│   ├── VerifiedAnswers/
│   ├── schema.json
│   ├── examplePrompts.json
│   ├── settings.json
│   └── version.json
├── diagramLayout.json 
└── definition.pbism 
```

```json
{
    "parts": [
        {
            "path": "definition/database.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/model.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/tables/product.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "definition/tables/sales.tmdl",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        ...
        {
            "path": "definition.pbism",
            "payload": "<base64 encoded string>",
            "payloadType": "InlineBase64"
        },
        {
            "path": "diagramLayout.json",
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
├── tables/
│   ├── product.tmdl
│   ├── sales.tmdl
│   ├── calendar.tmdl
├── relationships.tmdl
├── model.tmdl
├── database.tmdl
```

Example of `tables/product.tmdl`

```tmdl
table Product

	measure '# Products' = COUNTROWS('Product')
		formatString: #,##0

	column Product
		dataType: string
		isDefaultLabel
		summarizeBy: none
		sourceColumn: Product

	column ProductKey
		dataType: int64
		isHidden
		isKey
		summarizeBy: none
		sourceColumn: ProductKey

	column Category
		dataType: string
		summarizeBy: none
		sourceColumn: Category		

	partition Product-171f48b3-e0ea-4ea3-b9a0-c8c673eb0648 = m
		mode: import
		source =
				let				    
				    ...
				in
				    #"FinalStep"
```

## model.bim part

Example of `model.bim` file:

```json
{
  "compatibilityLevel": 1702,
  "model": {
    "annotations": [],
    "culture": "en-US",
    "sourceQueryCulture": "en-US",
    "cultures": [],
    "dataAccessOptions": {
      "legacyRedirects": true,
      "returnErrorValuesAsNull": true
    },
    "defaultPowerBIDataSourceVersion": "powerBI_V3",
    "discourageImplicitMeasures": true,
    "expressions": [],
    "functions": [],
    "perspectives": [],
    "relationships": [],
    "roles": [],    
    "tables": [      
      {
        "name": "Product",
        "annotations": [],
        "columns": [
          {
            "name": "Product",
            "dataType": "string",
            "isDefaultLabel": true,            
            "sourceColumn": "Product",
            "summarizeBy": "none"
          },
          {
            "name": "ProductKey",        
            "dataType": "int64",
            "formatString": "0",
            "isAvailableInMdx": false,
            "isHidden": true,
            "isKey": true,            
            "sourceColumn": "ProductKey",
            "summarizeBy": "none"
          }
          ,
          {
            "name": "Category",
            "dataType": "string",            
            "sourceColumn": "Category",
            "summarizeBy": "none"
          }
        ],        
        "measures": [
          {
            "name": "# Products",
            "expression": "COUNTROWS('Product')",
            "formatString": "#,##0",
            "lineageTag": "1f8f1a2a-06b6-4989-8af7-212719cf3617"
          }
        ],
        "partitions": [
          {
            "name": "Product-171f48b3-e0ea-4ea3-b9a0-c8c673eb0648",
            "mode": "import",
            "source": {
              "expression": [
                "let",
                "    ...",
                "in",
                "    #\"FinalStep\""
              ],
              "type": "m"
            }
          }
        ]
      }
    ]
  }
}
```

## diagramLayout.json part

Example of `diagramLayout.json` file:

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

## definition.pbism part

Example of `definition.pbism` file:

```json
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/semanticModel/definitionProperties/1.0.0/schema.json",
  "version": "5.0",
  "settings": {
    "qnaEnabled": false
  }
}
```