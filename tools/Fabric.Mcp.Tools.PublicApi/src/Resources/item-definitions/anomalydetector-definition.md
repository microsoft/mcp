---
title: Anomaly detector definition
description: Learn how to structure an anomaly detector item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric anomaly detector item definition
author: rnandurimsft
ms.author: rnanduri
ms.service: fabric
ms.date: 08/11/2025
---

# Anomaly detector definition

This article provides a breakdown of the definition structure for anomaly detector items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `Configurations.json` | [AnomalyDefinition](#anomalydefinition-contents) (JSON) | true | The anomaly detector configuration, including data source, model selection, parameters, and detection settings |

## AnomalyDefinition contents

Root shape of the definition payload.

| Name | Type | Required | Description |
|------|------|----------|-------------|
| univariateConfigurations | [ADUnivariateConfiguration](#adunivariateconfiguration-contents)[] | true | One or more univariate anomaly detection configurations |

### ADUnivariateConfiguration contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| configurationId | String (GUID) | true | Unique ID of the configuration |
| configurationName | String | true | Human-friendly name of the configuration |
| fabricDataSource | [FabricDataSourceDetails](#fabricdatasourcedetails-and-kqldbsource) | true | Details of the Fabric data source to read the time series from |
| modelOption | [AdModelOption](#admodeloption-contents) | true | Model selection and parameters |
| detectionSettings | [DetectionSettings](#detectionsettings-contents) | true | Detection behavior settings |

### FabricDataSourceDetails and KQLDbSource

FabricDataSourceDetails is a polymorphic type. Use the discriminator property `dataSourceType` to select the concrete shape. Currently supported in this example: `KqlDb`.

Common properties for all Fabric data sources:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| workspaceId | String (GUID) | true | Workspace ID that contains the source artifact |
| artifactId | String (GUID) | true | Artifact ID of the source (for KQL DB, the database artifact) |
| dataSourceType | String | true | Polymorphic discriminator. Use `KqlDb` for KQL database sources |

KQLDbSource additional properties:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| tableName | String | true | Table name with the time series data |
| timestampColumnName | String | true | Column containing timestamps |
| instanceIDColumnName | String | true | Column identifying the series instance (entity ID) |
| attributeColumnName | String | true | Column containing the observed value or attribute |

### AdModelOption contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| modelSelection | [ModelInfo](#modelinfo-contents) | true | Model algorithm and version |
| parameters | [AdModelParameter](#admodelparameter-contents-polymorphic)[] | false | Optional list of model parameters |

### ModelInfo contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| modelAlgorithm | String | true | Algorithm identifier (for example, `SR`, `PCA`, `HBOS`) |
| modelVersion | String | true | Model version identifier |
| modelName | String | true | Display name for the model |
| modelDescription | String | false | Optional description of the model |

## Supported modelAlgorithm values

Set `modelSelection.modelAlgorithm` to one of the following values. The value must match exactly (case-sensitive):

- DIFF_ROLLING_SCORE — Difference from rolling mean (squared error)
- SIGNAL_SCORE — Uses the raw signal as the score
- MEDIAN_DISTANCE_SCORE — Absolute distance from median
- MEDIAN_DISTANCE_SCORE_ADVANCED — Rolling median distance over a window
- MEAN_DISTANCE_SCORE — Absolute distance from mean
- DIFF_DIFF_STANDARD_SCORE — Standard deviation of second differences over a window
- SR_WINDOW — Spectral Residual (windowed variant)
- HBOS — Histogram-Based Outlier Score
- SR — Spectral Residual
- PCA — Principal Component Analysis based scoring
- SR_WINDOW_ADVANCED — Advanced Spectral Residual (windowed) variant
- KNN — k-Nearest Neighbors distance based scoring

### DetectionSettings contents

| Name | Type | Required | Description |
|------|------|----------|-------------|
| confidence | Integer (1-100) | true | Detection confidence threshold |
| autoPublish | Boolean | false | When true, anomaly events are published; default is false |

### AdModelParameter contents (polymorphic)

This type uses polymorphism. Use the discriminator property `$type` with one of the following values to specify the parameter shape:

- Integer
- Float
- String
- Boolean
- IntArr
- FloatArr
- StringArr
- BoolArr

Common property for all parameters:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| name | String | true | Parameter name |
| value | true | Parameter value |

Parameter shapes by `$type` value:

- `$type: "Integer"`  -> value: Integer
- `$type: "Float"`    -> value: Number
- `$type: "String"`   -> value: String
- `$type: "Boolean"`  -> value: Boolean
- `$type: "IntArr"`   -> value: Integer[]
- `$type: "FloatArr"` -> value: Number[]
- `$type: "StringArr"`-> value: String[]
- `$type: "BoolArr"`  -> value: Boolean[]

## AnomalyDefinition example

```json
{ 
  "$id": "https://developer.microsoft.com/json-schemas/fabric/item/anomalyDetector/definition/1.0.0/schema.json",
  "$schema": "https://json-schema.org/draft-07/schema#",
  "univariateConfigurations": [
    {
      "configurationId": "a1a1a1a1-b2b2-4c4c-8d8d-e3e3e3e3e3e3",
      "configurationName": "KQL temperature anomalies",
      "fabricDataSource": {
        "dataSourceType": "KqlDb",
        "workspaceId": "11111111-2222-3333-4444-555555555555",
        "artifactId": "66666666-7777-8888-9999-aaaaaaaaaaaa",
        "tableName": "device_temperature",
        "timestampColumnName": "ts",
        "instanceIDColumnName": "deviceId",
        "attributeColumnName": "temperature"
      },
      "modelOption": {
        "modelSelection": {
          "modelAlgorithm": "SR",
          "modelVersion": "1.0",
          "modelName": "Spectral Residual",
          "modelDescription": "Spectral Residual configuration for temperature series"
        },
        "parameters": [
          {
            "$type": "String",
            "name": "ImputeMode",
            "value": "auto"
          },
          {
            "$type": "String",
            "name": "Granularity",
            "value": "daily"
          },
          {
            "$type": "Float",
            "name": "MaxAnomalyRatio",
            "value": 0.9
          },
          {
            "$type": "Integer",
            "name": "Sensitivity",
            "value": 45
          },
          {
            "$type": "Integer",
            "name": "Regularization",
            "value": 1
          },
          {
            "$type": "Integer",
            "name": "Periodicity",
            "value": 24
          }
        ]
      },
      "detectionSettings": {
        "confidence": 95,
        "autoPublish": true
      }
    }
  ]
}
```

## Notes

- Use `dataSourceType` to select the concrete data source shape. For KQL DB, set `dataSourceType` to `KqlDb` and include the additional KQL fields.
- For model parameters, include the `$type` discriminator so parameters are bound to the correct shape.
- `confidence` must be an integer between 1 and 100 (inclusive).

## Model parameters by algorithm

This section lists commonly used parameter names observed in the implementation. All parameters are optional unless stated; defaults are determined by the service when omitted. Provide parameters in the `modelOption.parameters` array, using the proper `$type`.

### General cross-cutting parameters

The following parameters are supported across all model algorithms:

| Parameter | Type | Description | Example Value |
|-----------|------|-------------|---------------|
| ImputeMode | String | Data imputation strategy for missing values | `"auto"` |
| Granularity | String | Time series granularity | `"daily"` |
| MaxAnomalyRatio | Float | Maximum expected ratio of anomalies in the data | `0.9` |
| Sensitivity | Integer | Detection sensitivity level | `45` |
| Regularization | Integer | Regularization frequency for the model | `1` |
| Periodicity | Integer | Expected periodicity in the data | `24` (hourly data with daily pattern) |

Example usage in parameters array:

```json
"parameters": [
  {
    "$type": "String",
    "name": "ImputeMode",
    "value": "auto"
  },
  {
    "$type": "String",
    "name": "Granularity",
    "value": "daily"
  },
  {
    "$type": "Float",
    "name": "MaxAnomalyRatio",
    "value": 0.9
  },
  {
    "$type": "Integer",
    "name": "Sensitivity",
    "value": 45
  },
  {
    "$type": "Integer",
    "name": "Regularization",
    "value": 1
  },
  {
    "$type": "Integer",
    "name": "Periodicity",
    "value": 24
  }
]
```


Notes:

- Parameter names are case-sensitive and should match those shown above.
- Some models compute internal defaults when parameters aren’t provided.
