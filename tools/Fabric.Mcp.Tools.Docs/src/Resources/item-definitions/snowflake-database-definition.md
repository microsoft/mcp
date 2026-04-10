# Snowflake Database definition

This article provides a breakdown of the structure for Snowflake Database definition items.

## Supported formats

Snowflake Database items support the `JSON` format.

## Definition parts

The definition of a Snowflake Database item is constructed from the SnowflakeDatabaseProperties part, which contains the following:

* **Path**: The path to the file that contains the JSON definition.
* **Payload**: See [Example of SnowflakeDatabaseProperties.json part decoded from Base64](#example-of-snowflakedatabasepropertiesjson-part-decoded-from-base64).
* **PayloadType**: InlineBase64

## Example of SnowflakeDatabaseProperties.json part decoded from Base64

The JSON file describing the Snowflake Database has the following properties:

| Property                | Type   | Required | Description                                                 |
| ----------------------- | ------ | -------- | ----------------------------------------------------------- |
| `connectionId`   | string | &#10060; | The connection ID for the Snowflake Database, it should be empty in create requests. |
| `snowflakeDatabaseName` | string | &#9989; | The Snowflake database name. If sent, it should be empty in create requests. |

```json

{
  "snowflakeDatabaseName": "ExampleDatabase",
  "connectionId": "456e7890-e89b-12d3-a456-426614174002"
}

```

## Definition example

```json
{
  "parts": [
    {
      "path": "SnowflakeDatabaseProperties.json",
      "payload": "eyJzbm93Zmxha2VEYXRhYmFzZU5hbWUiOiAiRXhhbXBsZURhdGFiYXNlIiwgImNvbm5lY3Rpb25JZCI6ICI0NTZlNzg5MC1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDIifQ==",
      "payloadType": "InlineBase64"
    }
  ]
}
```
