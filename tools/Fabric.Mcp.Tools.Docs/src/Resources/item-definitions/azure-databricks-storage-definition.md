# Azure Databricks Storage definition

This article provides a breakdown of the structure for Azure Databricks Storage definition items.

## Supported formats

AzureDatabricksStorageDefinition items support the `JSON` format.

## Definition parts

The following table describes the parts included in the Azure Databricks Storage definition.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `definition.json` | [definition](#definition) | true  | Describes the properties of the Azure Databricks Storage item |
| `.platform`       | PlatformDetails (JSON)                        | false | Describes common details of the item |

## Definition

There aren't any properties described in the Azure Databricks Storage definition, so an empty object is given.

```json
{}
```

## Definition example

```json
{
"parts": [
    {
        "path": "definition.json",
        "payload": "e30=",
        "payloadType": "InlineBase64"
    },
    {
        "path": ".platform",
        "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9naXRJbnRlZ3JhdGlvbi9wbGF0Zm9ybVByb3BlcnRpZXMvMi4wLjAvc2NoZW1hLmpzb24iLAogICJtZXRhZGF0YSI6IHsKICAgICJ0eXBlIjogIkF6dXJlRGF0YWJyaWNrc1N0b3JhZ2UiLAogICAgImRpc3BsYXlOYW1lIjogIkF6dXJlRGF0YWJyaWNrc1N0b3JhZ2VfMzQ3cG1fMTNNYXkiLAogICAgImRlc2NyaXB0aW9uIjogIkF1cm9yYSBDYXJ0IFN0b3JhZ2UgY3JlYXRlZCB2aWEgRmFicmljIHB1YmxpYyBBUEkuIgogIH0sCiAgImNvbmZpZyI6IHsKICAgICJ2ZXJzaW9uIjogIjIuMCIsCiAgICAibG9naWNhbElkIjogIjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIKICB9Cn0=",
        "payloadType": "InlineBase64"
    }
]
}
```
