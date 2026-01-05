# Spark Job definition

This article provides a breakdown of the structure for Spark job definition items. For detailed information, see: [How to create and update a Spark Job Definition with Microsoft Fabric Rest API](https://learn.microsoft.com/fabric/data-engineering/spark-job-definition-api).

## Supported formats

SparkJobDefinition items support the `SparkJobDefinitionV1` and `SparkJobDefinitionV2` format.

## Format differences

- **SparkJobDefinitionV1** defines only the Spark job definition properties in the payload.
- **SparkJobDefinitionV2** uses the same payload content as V1 and extends it by allowing the main executable and dependent libraries to be uploaded inline as additional parts.

> The properties within the Spark job definition payload are consistent across both formats. The distinction between the formats lies in the method by which the files are supplied, rather than in the payload content itself.

## SparkJobDefinitionV1 

### Definition parts

The definition of a Spark job item with `SparkJobDefinitionV1` format is made out of a single part, and is constructed as follows:

* **Path** - The file name, for example: `SparkJobDefinitionV1.json`

* **Payload Type** - InlineBase64

* **Payload** - See [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64)

#### Example of payload content decoded from Base64

```json
{
    "executableFile": null,
    "defaultLakehouseArtifactId": "",
    "mainClass": "",
    "additionalLakehouseIds": [],
    "retryPolicy": null,
    "commandLineArguments": "",
    "additionalLibraryUris": [],
    "language": "",
    "environmentArtifactId": null
}
```

### Definition example

Here's an example of an item definition for a Spark job.

```json
{
    "format": "SparkJobDefinitionV1",
    "parts": [
        {
            "path": "SparkJobDefinitionV1.json",
            "payload": "eyJleGVjdXRhYmxlRmlsZSI6bnVsbCwiZGVmYXVsdExha2Vob3VzZUFydGlmYWN0SWQiOiIiLCJtYWluQ2xhc3MiOiIiLCJhZGRpdGlvbmFsTGFrZWhvdXNlSWRzIjpbXSwicmV0cnlPbGljYXR5IjpudWxsLCJjb21tYW5kTGluZUFyZ3VtZW50c2I6bnVsbCwiY29tbWFuZExpbmVBYnJndW1lbnRzIjpbXSwibGFuZ3VhZ2UiOiIiLCJlbm52ZW1lbnRBYnJndW1lbnRzIjpbXSwibGFuZ3VhZ2UiOiIiLCJlbm52ZW1lbnRBYnJndW1lbnRzIjpbXSwiZW52aXJvbm1lbnRBcnRpZmFjdElkIjpudWxsfQ==",
            "payloadType": "InlineBase64"
        },
        {
            "path": ".platform",
            "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
            "payloadType": "InlineBase64"
        }
    ]
}
```
## SparkJobDefinitionV1.json schema

The following table describes the properties supported in `SparkJobDefinitionV1.json`.

| Property | Type | Required | Description |
|--------|------|----------|-------------|
| `executableFile` | string \| null | No | Path to the main executable file (for example, `main.py`)|
| `language` | string | No | Language of the Spark job (for example, `Python`, `Scala`). |
| `mainClass` | string | No | Fully qualified main class name for JVM-based jobs. |
| `commandLineArguments` | string | No | Command-line arguments passed to the job. |
| `defaultLakehouseArtifactId` | string | No | Default lakehouse item ID associated with the job. |
| `additionalLakehouseIds` | array of strings | No | Additional lakehouse item IDs required by the job. |
| `additionalLibraryUris` | array of strings | No | List of library file names. |
| `retryPolicy` | object \| null | No | Retry policy configuration for the Spark job. |
| `environmentArtifactId` | string \| null | No | Environment item ID used for execution. |

### retryPolicy

The `retryPolicy` property controls retry behavior for a Spark job.

- **Type:** string (JSON-encoded object)
- **Required:** No

The value must be a JSON-encoded string representing a retry policy.

#### Retry policy object

| Property | Type | Required | Description |
|--------|------|----------|-------------|
| `policyType` | string | Yes | Retry policy type. The only supported value is `SimpleRetry`. |
| `policyProperties` | object | No | Properties that configure the retry behavior. |

#### Retry policy properties (`policyProperties`)

| Property | Type | Required | Description |
|--------|------|----------|-------------|
| `retryCount` | integer | Yes | Number of retry attempts. Must be ≥1, or `-1`. |
| `intervalBetweenRetriesInSeconds` | integer | No | Interval between retries, in seconds (0–86400). |

### **Example**

```json
{
  "retryPolicy": {
    "policyType": "SimpleRetry",
    "policyProperties": {
      "retryCount": 3,
      "intervalBetweenRetriesInSeconds": 300
    }
  }
}
```

## SparkJobDefinitionV2

> **Note**
> The `SparkJobDefinitionV2` format supports uploading **Python (`.py`) and R (`.R`)** files only.
> Uploading `.jar` files is not supported in either the `Main` or `Libs` parts.

### Definition parts

The definition of a Spark job item with `SparkJobDefinitionV2` format consists of a `SparkJobDefinitionV1.json` part and, optionally, a `Main` file part and `Libs` file parts. 

Note: For historical reasons, the file name contains `V1` even though the `SparkJobDefinitionV2` format is being used. 
The `SparkJobDefinitionV1.json` part is constructed as follows:

* **Path** - `SparkJobDefinitionV1.json`

* **Payload Type** - InlineBase64

* **Payload** - See [Example of payload content decoded from Base64](#example-of-payload-content-decoded-from-base64)

#### Example of payload content decoded from Base64

```json
{
    "executableFile": "main.py",
    "defaultLakehouseArtifactId": "",
    "mainClass": "",
    "additionalLakehouseIds": [],
    "retryPolicy": null,
    "commandLineArguments": "",
    "additionalLibraryUris": ["lib1.py", "lib2.py"],
    "language": "Python",
    "environmentArtifactId": null
}
```

The `Main` file part is optional. There can be **at most one** `Main` file part. It should be supplied if the client wishes to upload a main definition file inline in the create/update request. The `Main` file part is constructed as follows:

* **Path** - The file path, which must start with `Main/`, followed by the file name. For example: `Main/main.py`.

* **Payload Type** - InlineBase64

* **Payload** - The file contents, encoded in Base64 format.

The `Libs` file part optional. There can be **multiple** `Libs` file parts. It should be supplied if the client wishes to upload a reference file inline in the create/update request. The `Libs` file part is constructed as follows:

* **Path** - The file path, which must start with `Libs/`, followed by the file name. For example: `Libs/lib1.py`.

* **Payload Type** - InlineBase64

* **Payload** - The file contents, encoded in Base64 format.


### Definition example

Here's an example of a SparkJobDefinitionV2 definition for a Spark job definition with a main definition file and two reference files.

```json
{
    "format": "SparkJobDefinitionV2",
    "parts": [
        {
            "path": "SparkJobDefinitionV1.json",
            "payload": "ewogICAgImV4ZWN1dGFibGVGaWxlIjogIm1haW4ucHkiLAogICAgImRlZmF1bHRMYWtlaG91c2VBcnRpZmFjdElkIjogIiIsCiAgICAibWFpbkNsYXNzIjogIiIsCiAgICAiYWRkaXRpb25hbExha2Vob3VzZUlkcyI6IFtdLAogICAgInJldHJ5UG9saWN5IjogbnVsbCwKICAgICJjb21tYW5kTGluZUFyZ3VtZW50cyI6ICIiLAogICAgImFkZGl0aW9uYWxMaWJyYXJ5VXJpcyI6IFsibGliMS5weSIsICJsaWIyLnB5Il0sCiAgICAibGFuZ3VhZ2UiOiAiUHl0aG9uIiwKICAgICJlbnZpcm9ubWVudEFydGlmYWN0SWQiOiBudWxsCn0=",
            "payloadType": "InlineBase64"
        },
        {
            "path": ".platform",
            "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Main/main.py",
            "payload": "cHJpbnQoMSk=",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Libs/lib1.py",
            "payload": "cHJpbnQoMik=",
            "payloadType": "InlineBase64"
        }
        {
            "path": "Libs/lib2.py",
            "payload": "cHJpbnQoMyk=",
            "payloadType": "InlineBase64"
        }
    ]
}
```