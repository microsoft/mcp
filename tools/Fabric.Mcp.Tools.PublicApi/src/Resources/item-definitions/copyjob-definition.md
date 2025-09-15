---
title: Copy job definition
description: Learn how to create a Copy job item definition when using the Microsoft Fabric REST API.
ms.title: Microsot Fabric Copy job item definition
author: anuska-ray
ms.author: anuskaray
ms.service: fabric
ms.date: 02/03/2025
---
  
# Copy job definition
  
This article provides a breakdown of the definition structure for Copy job items.
  
## Definition parts
  
This table lists the definition parts.
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `copyjob-content.json` | [ContentDetails](#contentdetails) (JSON) | true | Describes properties and settings of the item like source and destination|
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

## ContentDetails

Describes content of payload
  
| Name                  | Type                | Description                         |
|-----------------------|---------------------|-------------------------------------|
| properties            | [CopyJobProperties](#description-for-copyjobproperties-contents)   | CopyJob properties.|
| activities            | [CopyJobActivity](#description-for-copyjobactivity-contents)[]  | List of CopyJob activities.|

### Description for CopyJobProperties contents

Describes the fields used to construct the properties

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| jobMode               | String (Enum)   | true            | Describes the job mode with two possible values: `Batch` and `CDC`(Incremental).|
| source                | [CopyJobConnection](#description-for-copyjobconnection-contents)| false| Describes the source. |
| destination           | [CopyJobConnection](#description-for-copyjobconnection-contents)| false| Describes the destination. |
| policy                | [CopyJobPolicy](#description-of-copyjobpolicy-contents)          | false           | Describes the policy for the copy job like timeout duration.|

### Description for CopyJobConnection contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Describes the type of data source or destination. |
| connectionSettings    | [CopyJobConnectionSettings](#description-for-copyjobconnectionsettings-contents)| true | Describes the connection settings for the endpoint.|

### Description for CopyJobConnectionSettings contents

Describes the fields for Connection Settings. Here, depending on the type, either typeProperties or externalReferences or both need to be mentioned.

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| type                  | String          | true            | Describes the type of connection. |
| typeProperties   | [CopyJobConnectionTypeProperties](#description-for-copyjobconnectiontypeproperties-contents)          | false            | Describes the properties for the connection.|
| externalReferences| [ExternalReference](#description-for-externalreferences-contents)         | false            | Describes the external reference for the connection.|

### Description for CopyJobConnectionTypeProperties contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| schema                | String          | false           | Specifies the schema. |
| database              | String          | false           | Name of the database. |
| workspaceId           | String (Guid)   | false           | Specifies the Id for the workspace in which the connected item exists.|
| artifactId            | String (Guid)   | false           | Specifies the Id for the connected item.|
| rootFolder            | String          | false           | Specifies the root folder.|
| endpoint              | String          | false           | Specifies the endpoint.|

### Description for ExternalReferences contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| connection            | String (Guid)   | true            | Specifies the Id of the connection.|

### Description of CopyJobPolicy contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| timeout               | String          | true            | Specifies the timeout duration. Follows the pattern: `((\\d+)\\.)?(\\d\\d):(60\|(\[0-5\]\[0-9\])):(60\|(\[0-5\]\[0-9\]))` .|

### Description for CopyJobActivity contents

Describes the fields used to construct the properties

| Name                  | Type                      | Required        | Description       |
|-----------------------|---------------------------|-----------------|-------------------|
| properties            | [CopyJobActivityProperties](#description-for-copyjobactivityproperties-contents) | true            | Describes the activity properties. |
| id                    | String (Guid)             | false           | The activity id.  |

### Description for CopyJobActivityProperties contents

Describes the fields used to construct the properties

| Name                  | Type   | Required    | Description       |
|-----------------------|--------|-------------|-------------------|
| source                | [CopyJobActivitySource](#description-for-copyjobactivitysource-contents) | true        | Describes the source settings. |
| destination           | [CopyJobActivityDestination](#description-for-copyjobactivitydestination-contents) | true        | Describes the destination settings. |
| translator            | [CopyJobActivityTranslator](#description-for-copyjobactivitytranslator-contents) | true        | Describes the translator settings. |
| typeConversionSettings| [CopyJobActivityMappingTypeConversionSettings](#description-for-copyjobactivitymappingtypeconversionsettings-contents) | true        | Describes the type conversion settings. |
| enableStaging         | Boolean| false       | Specifies whether staging is enabled or not. |

### Description for CopyJobActivitySource contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| exportSettings        | [CopyJobExportSettings](#description-for-copyjobexportsettings-contents)| false         | Describes the export settings.|
| datasetSettings       | [CopyJobDatasetSettings](#description-for-copyjobdatasetsettings-contents)| false        | Describes the dataset settings.|
| changeDataSettings    | [CopyJobChangeDataSettings](#description-for-copyjobchangedatasettings-contents)| false     | Describes the change data settings.|
| storeSettings         | [CopyJobStoreSettings](#description-for-copyjobstoresettings-contents)| false          | Describes the store settings. |
| formatSettings        | [CopyJobFormatSettings](#description-for-copyjobformatsettings-contents)| false         | Describes the format settings. |
| noTruncation          | Boolean                 | false | Specifies if truncation is allowed or not. |

### Description for CopyJobActivityDestination contents

| Name                  | Type            | Required        | Description       |
|-----------------------|-----------------|-----------------|-------------------|
| tableOption           | String          | false           | Specifies the option for table related action.|
| upsertSettings        | [SqlUpsertSettings](#description-for-copyjobsqlupsertsettings-contents)| false      | Describes the upsert settings.|
| partitionOption       | String          | false           | Specifies the partition mechanism for writing to destination.|
| partitionNameList     | String[]           | false           | Specifies the list of keys/columns to be used for partition mechanism. |
| writeBehaviour        | String          | false           | Specifies the write behvaiour like Append or Overwrite.|
| importSettings        | [CopyJobImportSettings](#description-for-copyjobimportsettings-contents)| false         | Describes the import settings.|
| datasetSettings       | [CopyJobDatasetSettings](#description-for-copyjobdatasetsettings-contents)| false        | Describes the dataset settings.|
| storeSettings         | [CopyJobStoreSettings](#description-for-copyjobstoresettings-contents)| false          | Describes the store settings. |
| formatSettings        | [CopyJobFormatSettings](#description-for-copyjobformatsettings-contents)| false         | Describes the format settings. |
| noTruncation          | Boolean                 | false | Specifies if truncation is allowed or not. |

### Description for CopyJobExportSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type        | String  | false         | Describes the type of export settings.|

### Description for CopyJobDatasetSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| Schema        | String  | false         | Describes the schema.|
| table        | String  | false         | Specifies the table name.|
| Location       | [CopyJobDatasetSettingsLocation](#description-for-copyjobdatasetsettingslocation-contents)  | false         | Describes the location.|
| Compression    | [CopyJobDatasetSettingsCompressionSettings](#description-for-copyjobdatasetsettingscompressionsettings-contents)| false      | Describes the compression settings.|
| columnDelimiter| String | false| Specifies the column delimiter.|
| rowDelimiter | String | false | Specifies the row delimiter.|
| escapeChar | String | false | Specifies the escape character. |
| firstRowAsHeader | Boolean | false | Specifies if the first row will be used as header or not.|
| quoteChar | String | false | Specifies the quote character.|
| encodingName | String | false | Specifies the encoding. |
| compressionCodec | String | false | Specifies the compression codec.|
| compressionLevel| String | false | Specifies the compression level. |
| objectApiName | String | false | Specifies the object api name. |
| reportId | String | false | Specifies the report Id in case of Salesforce type connection. |

### Description for CopyJobDatasetSettingsLocation contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type        | String  | false         | Describes the type of location.|
| fileName    | String  | false         | Specifies the file Name. |
| folderPath  | String  | false         | The Folder Path.|
| container   | String  | false         | Container name. |
| fileSystem  | String  | false         | Name of file system. |
| bucketName  | String  | false         | Bucket Name.|

### Description for CopyJobDatasetSettingsCompressionSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type        | String  | false         | Describes the compression type.|
| level       | String  | false         | Decribes the compression level. |

### Description for CopyJobChangeDataSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| ReadMethod| String                            | false    | Specifies the read method.|
| Columns   | [CopyJobChangeDataColumn](#description-for-copyjobchangedatacolumn-contents)[]   | false    | Specifies the list of change data columns. |

### Description for CopyJobChangeDataColumn contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| name         | String | false    | Specifies the name of the column.        |
| type         | String | false    | Specifies the type of the column.        |
| physicalType | String | false    | Specifies the physical type of the column. |
| length       | String | false    | Specifies the length of the column.      |
| scale        | Integer   | false    | Specifies the scale of the column.       |
| precision    | Integer   | false    | Specifies the precision of the column.   |

### Description for CopyJobStoreSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type        | String  | false         | Specifies the type of store setting.|
| recursive   | Boolean    | false         | Specifies whether it is recursive.|
| prefix      | String  | false         | Specifies the prefix string to be used. |
| WildcardFolderPath      | String  | false    | Specifies the wildcard folder path.|
|wildcardFileName        | String  | false    | Specifies the wildcard file name.|
|fileListPath            | String  | false    | Specifies the file list path.|
|modifiedDatetimeStart   | String  | false    | Specifies the start of the modified datetime range.|
|modifiedDatetimeEnd     | String  | false    | Specifies the end of the modified datetime range.|
|enablePartitionDiscovery| Boolean    | false    | Specifies whether partition discovery is enabled.|
|copyBehavior            | String  | false    | Specifies the copy behavior.|

### Description for CopyJobFormatSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type                | String  | false    | Specifies the type of format setting.    |
| fileExtension       | String  | false    | Specifies the file extension.            |
| quoteAllText        | Boolean   | false    | Specifies whether to quote all text.     |
| enableVertiParquet  | Boolean  | false    | Specifies whether VertiParquet is enabled. |

### Description for CopyJobSqlUpsertSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| useTempDB         | Boolean | false    | Specifies whether to use temp db for upsert interim table. |
| interimSchemaName | String | false    | Schema name for interim table.|
| keys              | String[] | false    | Key column names for unique row identification.|

### Description for CopyJobImportSettings contents

| Name        | Type    | Required      | Description       |
|-------------|---------|---------------|-------------------|
| type        | String  | false         | Describes the type of import settings.|

### Description for CopyJobActivityTranslator contents

| Name      | Type            | Required        | Description       |
|-----------|-----------------|-----------------|-------------------|
| type      | String          | false    | Specifies the type of the translator.    |
| mappings  | [CopyJobActivityMapping](#description-for-copyjobactivitymapping-contents)[] | false    | Specifies the list of activity mappings. |

### Description for CopyJobActivityMapping contents

| Name      | Type            | Required        | Description       |
|-----------|-----------------|-----------------|-------------------|
| source      | [CopyJobActivityMappingColumn](#description-for-copyjobactivitymappingcolumn-contents)| false    | Specifies the source column mapping.     |
| destination | [CopyJobActivityMappingColumn](#description-for-copyjobactivitymappingcolumn-contents)| false    | Specifies the destination column mapping.|

### Description for CopyJobActivityMappingColumn contents

| Name      | Type            | Required        | Description       |
|-----------|-----------------|-----------------|-------------------|
| name         | String | false    | Specifies the name of the column.        |
| type         | String | false    | Specifies the type of the column.        |
| physicalType | String | false    | Specifies the physical type of the column. |
| length       | String (Integer) | false    | Specifies the length of the column.      |
| scale        | Integer  | false    | Specifies the scale of the column.       |
| precision    | Integer   | false    | Specifies the precision of the column.   |

### Description for CopyJobActivityMappingTypeConversionSettings contents

Describes the fields used to construct the properties

| Name                  | Type   | Required    | Description       |
|-----------------------|--------|-------------|-------------------|
| typeConversion        | [CopyJobActivityMappingTypeConversion](#description-for-copyjobactivitymappingtypeconversion-contents) | true | Describes the type conversion properties. |

### Description for CopyJobActivityMappingTypeConversion contents

Describes the fields used to construct the properties

| Name                  | Type   | Required    | Description       |
|-----------------------|--------|-------------|-------------------|
| allowDataTruncation   | Boolean | false    | Specifies whether data truncation is allowed. |
| treatBooleanAsNumber  | Boolean | false    | Specifies whether to treat boolean as a number. |

### ContentDetails Example 1

```JSON
{ 
  "properties": { 
    "jobMode": "Batch" 
  }, 
  "activities": [] 
} 
```

### ContentDetails Example 2

```JSON
{
  "properties": { 
    "jobMode": "Batch", 
    "source": { 
      "type": "LakehouseTable", 
      "connectionSettings": { 
        "type": "Lakehouse", 
        "typeProperties": { 
          "workspaceId": "00000000-0000-0000-0000-000000000000", 
          "artifactId": "aaaaaaaa-6666-7777-8888-bbbbbbbbbbbb", 
          "rootFolder": "Tables" 
        } 
      } 
    }, 
    "destination": { 
      "type": "LakehouseTable", 
      "connectionSettings": { 
        "type": "Lakehouse", 
        "typeProperties": { 
          "workspaceId": "00000000-0000-0000-0000-000000000000", 
          "artifactId": "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb", 
          "rootFolder": "Tables" 
        } 
      } 
    }, 
    "policy": { 
      "timeout": "0.12:00:00" 
    } 
  }, 
  "activities": [ 
    { 
      "id": "eeeeeeee-4444-5555-6666-ffffffffffff", 
      "properties": { 
        "source": { 
          "datasetSettings": { 
            "table": "publicholidays", 
            "firstRowAsHeader": true 
          } 
        }, 
        "destination": { 
          "writeBehavior": "Append", 
          "datasetSettings": { 
            "table": "publicholidays", 
            "firstRowAsHeader": false 
          } 
        }, 
        "translator": { 
          "type": "TabularTranslator" 
        }, 
        "typeConversionSettings": { 
          "typeConversion": { 
            "allowDataTruncation": true, 
            "treatBooleanAsNumber": false 
          } 
        } 
      } 
    } 
  ] 
}
```
