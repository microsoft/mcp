---
title: Environment definition
description: Learn how to structure a environment item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric environment item definition
author: nis-goel
ms.author: nisgoel
ms.service: fabric
ms.date: 02/27/2025
---

# Environment definition

This article provides a breakdown of the definition structure for environment items.

## Definition parts

| Definition part path | type | Required | Description |
|--|--|--|--|
| `Libraries/CustomLibraries/<libraryname>.jar` | CustomLibraries (JAR) | false | A custom jar library in Base64 encoded format |
| `Libraries/CustomLibraries/<libraryname>.py` | CustomLibraries (PY) | false | A custom Python script file in Base64 encoded format |
| `Libraries/CustomLibraries/<libraryname>.whl` | CustomLibraries (WHL) | false | A custom wheel file in Base64 encoded format |
| `Libraries/CustomLibraries/<libraryname>.tar.gz` | CustomLibraries (TAR.GZ) | false | A custom R archive file in Base64 encoded format |
| `Libraries/PublicLibraries/environment.yml` | ExternalLibraries (YAML) | false | An environment YAML file with external libraries in Base64 encoded format |
| `Setting/Sparkcompute.yml` | SparkComputeSettings (YAML) | false | Spark compute settings YAML in Base64 encoded format |
| `.platform` | PlatformDetails (JSON) | false | Describes the metadata of the item |

Each definition part of an environment item is constructed as follows:

* **Path** - The file name, for example `Setting/Sparkcompute.yml`.
* **Payload type** - InlineBase64
* **Payload** See: [Example of payload content decoded from Base64](#description-for-spark-settings-contents).

### Description for Spark External Libraries Contents

Describes the fields used to construct the `environment.yml.`

| Name                  | Type                | Required        | Description                                                                                                             |
|-----------------------|---------------------|-----------------|----------------------|
| dependencies          | Dictionary              | true            | A list of conda packages that will be installed in the environment. The format is `<package_name>==<version_number>`. |
| pip                   | Dictionary              | false           | Specifies additional Python packages to be installed using pip. This can be a list of strings where each string is a pip package to be installed in format `<package_name>==<version_number>`. |

### Description for Spark Settings Contents

Describes the fields used to construct the `SparkCompute.yml`.

| Name                  | Type            | Required        | Description                                                                                                                |
|-----------------------|-----------------|-----------------|----------------------------------------------------------------------------------------------------------------------------|
| enable_native_execution_engine   | Boolean                 | true   | Enable native execution engine. True - Enabled, False - Disabled. |  
| instance_pool_id                 | String                  | true   | Environment pool. Must be a valid custom pool specified by the instance pool ID. When not specified (null) a starter pool is created. |
| driver_cores                     | Integer                  | true   | Spark driver cores. The allowed values are 4, 8, 16, 32 and 64. |
| driver_memory                    | String                  | true   | Spark driver memory. The allowed values are 28g, 56g, 112g, 224g, 400g. |
| executor_cores                   | Integer                  | true   | Spark executor cores. The allowed values are 4, 8, 16, 32, 64. |
| executor_memory                  | String                  | true   | Spark executor memory. The allowed values are 28g, 56g, 112g, 224g, 400g. |
| dynamic_executor_allocation      | Object                  | true   | Dynamic executor allocation. See [Description for dynamic_executor_allocation Contents](#description-for-dynamic_executor_allocation-contents). |
| spark_conf                       | Dictionary                  | false  | Spark configurations. |
| runtime_version                  | String                  | true   | Runtime version, find the supported [fabric runtimes](https://learn.microsoft.com/fabric/data-engineering/runtime). |

### Description for dynamic_executor_allocation Contents

Describes the fields used to construct the `dynamic_executor_allocation`.

| Name                  | Type                | Required        | Description                                                                                                                |
|-----------------------|---------------------|-----------------|----------------------|
| enabled               | Boolean             | true            | The status of the dynamic executor allocation. True - Enabled, False - Disabled.    |
| min_executors         | Integer             | true            | The minimum executor number for dynamic allocation. Minimum value is 1. The maximum value must be lower than the `maxExecutors`.  |
| max_executors         | Integer             | true            | The maximum executor number for dynamic allocation. Minimum value is 1. The maximum value must be lower than the `maxNodeCount` instance pool.  |

## Platform part

The platform part is a file that contains the environment metadata information.
* [Create Item](https://learn.microsoft.com/rest/api/fabric/core/items/create-item) with definition respects the platform file if provided
* [Get Item](https://learn.microsoft.com/rest/api/fabric/core/items/get-item) definition always returns the platform file.
* [Update Item](https://learn.microsoft.com/rest/api/fabric/core/items/update-item) definition accepts the platform file if provided, but only if you set a new URL parameter `updateMetadata=true`.


### Example of spark public libraries `environment.yml` content decoded from Base64

```yaml
dependencies:
  - matplotlib==0.10.1
  - scipy==0.0.1
  - pip:
      - fuzzywuzzy==0.18.0
      - numpy==0.1.28
```

### Example of spark settings `Sparkcompute.yml` content decoded from Base64

```yaml
enable_native_execution_engine: false
instance_pool_id: 655fc33c-2712-45a3-864a-b2a00429a8aa
driver_cores: 4
driver_memory: 28g
executor_cores: 4
executor_memory: 28g
dynamic_executor_allocation:
  enabled: true
  min_executors: 1
  max_executors: 2
spark_conf:
  spark.acls.enable: true
runtime_version: 1.3
```

## Definition example

```json
{
    "format": "null",
    "parts": [
        {
            "path": "Libraries/CustomLibraries/samplelibrary.jar",
            "payload": "eyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Libraries/CustomLibraries/samplepython.py",
            "payload": "FyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Libraries/CustomLibraries/samplewheel-0.18.0-py2.py3-none-any.whl",
            "payload": "LyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Libraries/CustomLibraries/sampleR.tar.gz",
            "payload": "ZyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Libraries/PublicLibraries/environment.yml",
            "payload": "IyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": "Setting/Sparkcompute.yml",
            "payload": "GyJuYmZvcm1hdCI6N..",
            "payloadType": "InlineBase64"
        },
        {
            "path": ".platform",
            "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc",
            "payloadType": "InlineBase64"
        }
    ]
}
```
