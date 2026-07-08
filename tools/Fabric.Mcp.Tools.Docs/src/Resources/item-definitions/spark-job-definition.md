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
| `executableFile` | string \| null | No | Path to the main executable file (for example, `main.py`). Can be a simple filename (requires a corresponding `Main/` file part in V2) or an external `abfss://` path. See [executableFile update behavior](#executablefile-update-behavior) for important details on how this property is handled during updates. |
| `language` | string | No | Language of the Spark job (for example, `Python`, `Scala`). |
| `mainClass` | string | No | Fully qualified main class name for JVM-based jobs. |
| `commandLineArguments` | string | No | Command-line arguments passed to the job. |
| `defaultLakehouseArtifactId` | string | No | Default lakehouse item ID associated with the job. |
| `additionalLakehouseIds` | array of strings | No | Additional lakehouse item IDs required by the job. |
| `additionalLibraryUris` | array of strings | No | List of library file URIs. Can be simple filenames (uploaded inline with `Libs/` parts in V2) or external `abfss://` paths. See [additionalLibraryUris update behavior](#additionallibraryuris-update-behavior) for important details on how this property is handled during updates. |
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

### V1 vs V2 update behavior

The update behavior differs significantly between the two formats:

- **SparkJobDefinitionV1** updates **property references only**. It does not upload or delete files in OneLake. Setting `executableFile` or `additionalLibraryUris` only changes the stored metadata reference. No file-level validations (such as `Main/` or `Libs/` part checks) apply.

- **SparkJobDefinitionV2** updates property references **and manages OneLake files**. It supports inline file upload via `Main/` and `Libs/` parts and automatically deletes replaced or removed files from OneLake. The transition tables and file-related error codes below describe V2 behavior unless otherwise noted.

### executableFile update behavior

The `executableFile` property follows **full-replacement** semantics on update. When updating a Spark Job Definition using [Update Spark Job Definition Definition](/rest/api/fabric/sparkjobdefinition/items/update-spark-job-definition-definition), the incoming value fully replaces the existing one.

The `executableFile` value can be either:
- A **simple filename** (for example, `main.py`) — requires a corresponding `Main/` file part to be included in the request (V2 format only).
- An **external `abfss://` path** — references an executable file stored elsewhere. No `Main/` file part is needed.

#### Key behaviors

| Existing `executableFile` | Incoming `executableFile` | `Main/` file part included? | Result |
|---|---|---|---|
| `"main.py"` (local) | `null` or omitted | No | Succeeds — **deletes the existing file from OneLake** and clears the reference. |
| `"main.py"` (local) | `"new.py"` (new filename) | Yes (`Main/new.py`) | Succeeds — uploads new file, **deletes old file** from OneLake. |
| `"main.py"` (local) | `"main.py"` (same filename) | Yes (`Main/main.py`) | Succeeds — overwrites the file in OneLake. |
| `"main.py"` (local) | `"new.py"` (new filename) | No | **400 BadRequest** — filename-only value requires a corresponding `Main/` part. |
| `null` (no executable) | `null` or omitted | No | Succeeds — no change. |
| `null` (no executable) | `"main.py"` (filename) | Yes (`Main/main.py`) | Succeeds — uploads the file and sets the reference. |
| `null` (no executable) | `"main.py"` (filename) | No | **400 BadRequest** — filename-only value requires a corresponding `Main/` part. |
| `abfss://...` (external path) | `null` or omitted | No | Succeeds — clears the reference. No file is deleted (file is external). |
| `abfss://...` (external path) | `abfss://other/...` (new external path) | No | Succeeds — updates the reference to the new path. |
| `abfss://...` (external path) | `abfss://...` (same or new path) | Yes (`Main/` matching filename) | Succeeds — uploads the file to OneLake and updates the reference. The `Main/` part is validated by resolving it to the artifact's `Main/` folder path and comparing against the full `abfss://` value of `executableFile`. |
| Any | `Main/` part with `.jar` extension | — | **400 BadRequest** — `.jar` files are not supported for inline upload. |
| Any | `Main/` part filename does not match `executableFile` | — | **400 BadRequest** — the filename in the `Main/` part must match the `executableFile` value. |
| Any | More than one `Main/` part | — | **400 BadRequest** — at most one `Main/` file part is allowed. |

#### Important notes

##### SparkJobDefinitionV2

> [!WARNING]
> **Omitting `executableFile` deletes the existing file.** Both omitting the `executableFile` property from the JSON payload and explicitly setting it to `null` produce the same result — if a local file exists in OneLake, it is **permanently deleted**. Always include `executableFile` in the payload if you want to preserve the existing file.

- **Setting `executableFile` to `null` deletes the local file.** Setting `executableFile` to `null` or omitting it on update deletes the existing local executable file from OneLake. Callers should be aware of this behavior to avoid accidental data loss.
- **Filename vs. external path determines whether a `Main/` part is required.** If `executableFile` is set to a simple filename (for example, `main.py`), a corresponding `Main/main.py` part must be included. If it is set to a full `abfss://` path, no `Main/` part is needed.
- **Inline `Main/`/`Libs/` parts cannot be `.jar` files.** `.jar` files are not supported for inline upload. Use an external `abfss://` path for `.jar` files instead.
- File upload and deletion is managed automatically. When a new `Main/` part is provided, the old file is deleted from OneLake.
- `Main/` part filename must match the `executableFile` value.
- The transition table above describes V2 behavior.

##### SparkJobDefinitionV1

- V1 does **not** upload or delete files in OneLake. Setting `executableFile` to `null` only clears the metadata reference — no file is deleted.
- No `Main/` part validation applies (V1 requests contain a single JSON blob only).
- All file-related rows in the transition table (upload, delete) are V2-only. V1 only updates the stored property value.

### additionalLibraryUris update behavior

The `additionalLibraryUris` property follows **full-replacement** semantics on update. When updating a Spark Job Definition using [Update Spark Job Definition Definition](/rest/api/fabric/sparkjobdefinition/items/update-spark-job-definition-definition), the caller must provide the complete desired state of library references.

#### Key behaviors

| Existing `additionalLibraryUris` | Incoming `additionalLibraryUris` | Result |
|---|---|---|
| `null` (never set) | Omitted or `null` | Succeeds — no library state change. |
| `null` (never set) | `["lib1.py"]` | Succeeds — libraries are added. |
| `null` (never set) | `[]` | Succeeds — explicit empty list is set. |
| Non-empty list (for example, `["lib1.py"]`) | `["lib1.py", "lib2.py"]` | Succeeds — libraries are replaced with the new list. |
| Non-empty list or empty list `[]` | `[]` | Succeeds — all library references are removed and any corresponding inline-uploaded files are deleted from OneLake. External `abfss://` references are simply removed from the list. |
| Non-empty list or empty list `[]` | Omitted or `null` | **400 BadRequest** — once the library list is explicitly established, it cannot be set back to `null`. |

#### Important notes

##### SparkJobDefinitionV2

- **Once set, the library list cannot become `null` again.** After `additionalLibraryUris` has been explicitly set (even to an empty array `[]`), omitting it or setting it to `null` in subsequent updates returns a `400 BadRequest` error. This prevents accidental loss of library references. Note that this differs from `executableFile`, which allows `null` on update.
- **To remove all libraries**, specify an empty array `[]`. This removes all library references and deletes any inline-uploaded files from OneLake. External `abfss://` references are removed from the list without affecting external storage.
- **To retain existing libraries**, include the full list of library URIs in the update request. Libraries not included in the new list are removed, and any inline-uploaded files for those libraries are deleted from OneLake.
- **To add new libraries**, include both the existing and new library URIs in the list. New library files can be uploaded inline as `Libs/` parts.
- File upload (via `Libs/` parts) and deletion (files removed from the list are deleted from OneLake) are managed automatically.
- The transition table above describes V2 behavior.

##### SparkJobDefinitionV1

- V1 does **not** upload or delete library files in OneLake. It only updates the stored property value (the list of URI references).
- V1 allows setting `additionalLibraryUris` to `null` at any time — the null-guard validation does not apply.
- No `Libs/` part validation applies (V1 requests contain a single JSON blob only).
- To update library references, provide the desired list of URIs. No file operations occur.

### Update definition errors

The following conditions return a **400 BadRequest** response when updating a Spark Job Definition item definition.

#### Library reference errors (SparkJobDefinitionV2 only)

| Condition | Resolution |
|---|---|
| Library list set to null after being established | Once the library list has been explicitly set (even to `[]`), it cannot become null. Provide the complete list of URIs, or an empty array `[]` to remove all libraries. |
| Library list contains invalid entries | One or more entries in the list are null, empty, or whitespace-only. Ensure all URIs are non-empty strings. |
| Library list contains duplicates | Two or more entries in the list are identical. Remove duplicate URIs from the list. |
| No matching Libs file part provided for filename-only URI | A URI in `additionalLibraryUris` is a bare filename (not an `abfss://` path) but no corresponding `Libs/` part was included. Include a `Libs/` part for each filename-only URI, or use an external `abfss://` path. |
| Libs file part does not match library URI | The filename in a `Libs/` part does not match any entry in `additionalLibraryUris`. Ensure each `Libs/` part path corresponds to a URI in the list. |

#### Executable file errors (SparkJobDefinitionV2 only)

| Condition | Resolution |
|---|---|
| Main file part does not match executable file | The filename in the `Main/` part differs from the `executableFile` value. Ensure the `Main/` part path matches `executableFile`. |
| No main file part provided for filename-only value | `executableFile` is set to a simple filename but no corresponding `Main/` part was included. Include a `Main/` part, or use an external `abfss://` path instead. |
| Too many main file parts | More than one `Main/` part was provided. Include at most one `Main/` part per request. |
| Unsupported file extension for inline upload | A `.jar` file was provided as a `Main/` or `Libs/` inline part. Use an external `abfss://` path for `.jar` files. |

#### General errors (both formats)

| Condition | Resolution |
|---|---|
| Executable file language mismatch | The file extension of `executableFile` does not match the `language` property (for example, a `.py` file with `language` set to `Scala`). Align the file extension with the declared language. |
| Invalid filename characters | The filename contains unsupported characters. Use only letters (a-z, A-Z), numbers (0-9), hyphens (`-`), underscores (`_`), periods (`.`), and tildes (`~`). |
| Unsupported definition format | The `format` value is not recognized. Use `SparkJobDefinitionV1` or `SparkJobDefinitionV2`. |
| Invalid definition parts count (V1 only) | The request does not contain exactly one definition part. V1 expects a single `SparkJobDefinitionV1.json` part. |
| Unrecognized payload property (V1 only) | The JSON payload contains a property that is not part of the schema. Remove unknown properties. |
| Invalid payload property values (V1 only) | The JSON payload contains malformed or invalid property values. Fix the values to match the expected types. |

## SparkJobDefinitionV2

> **Note**
> The `SparkJobDefinitionV2` format does not support uploading `.jar` files in either the `Main` or `Libs` parts. Use an external `abfss://` path for `.jar` files.

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
        },
        {
            "path": "Libs/lib2.py",
            "payload": "cHJpbnQoMyk=",
            "payloadType": "InlineBase64"
        }
    ]
}
```