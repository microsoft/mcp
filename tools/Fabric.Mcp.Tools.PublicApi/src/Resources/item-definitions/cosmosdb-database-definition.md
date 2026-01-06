  
# Cosmos DB Database definition
  
This article provides a breakdown of the structure for Cosmos DB Database definition item.

## Definition parts
This table lists the Cosmos DB Database definition parts.
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `definition.json` | JSON | true | Describes the Cosmos DB Database container metadata |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

Definition part of a Cosmos DB Database item is constructed as follows:

* **Path**: The file name, for example: `definition.json`
* **Payload Type**: InlineBase64
* **Payload**: See [Example of payload content decoded from Base64](#definitionjson-example)



## Cosmos DB Database

Describes the Cosmos DB Database item.

| Name | Type | Required | Description |
|--|--|--|--|
| `$schema` | String | true | URI of the JSON schema. Example: https://github.com/microsoft/json-schemas/blob/main/fabric/item/CosmosDB/definition/CosmosDB/1.0.0/schema.json |
| `containers` | [ContainerMetadata[]](#containermetadata) | true | List of the Cosmos DB container metadata. |


### ContainerMetadata
| Name | Type | Required | Description |
|--|--|--|--|
| `options` | [ContainerOptions](#containeroptions) | true | A key-value pair of options to be applied for the request. |
| `resource` | [ContainerResource](#containerresource) | true | Cosmos DB database container resource object. |

### ContainerOptions
| Name | Type | Required | Description |
|--|--|--|--|
| `autoscaleSettings` | [AutoscaleSettings](#autoscalesettings) | true | Specifies the Autoscale settings. |

### AutoscaleSettings
| Name | Type | Required | Description |
|--|--|--|--|
| `maxThroughput` | Integer | true | Represents maximum throughput, the resource can scale up to. |


### ContainerResource
| Name | Type | Required | Description |
|--|--|--|--|
| `id` | String | true | Name of the Cosmos DB container. |
| `computedProperties` | [ComputedProperty[]](#computedproperty) | false | List of computed properties. |
| `conflictResolutionPolicy` | [ConflictResolutionPolicy](#conflictresolutionpolicy) | false | The conflict resolution policy for the container. |
| `defaultTtl` | Integer | false | Default time to live |
| `fullTextPolicy` | [FullTextPolicy](#fulltextpolicy) | false | The FullText policy for the container. |
| `geospatialConfig` | [GeospatialConfig](#geospatialconfig) | false | Represents geospatial configuration for a collection. |
| `indexingPolicy` | [IndexingPolicy](#indexingpolicy) | false | The configuration of the indexing policy. |
| `partitionKey` | [ContainerPartitionKey](#containerpartitionkey) | true | The configuration of the partition key to be used for partitioning data into multiple partitions. |
| `uniqueKeyPolicy` | [UniqueKeyPolicy](#uniquekeypolicy) | false | The unique key policy configuration for specifying uniqueness constraints on documents in the collection in the Azure Cosmos DB service. |
| `vectorEmbeddingPolicy` | [VectorEmbeddingPolicy](#vectorembeddingpolicy) | false | The vector embedding policy for the container. |

### ComputedProperty

| Name | Type | Required | Description |
|--|--|--|--|
| `name` | String | true | The name of a computed property, for example - "cp_lowerName". |
| `query` | String | true | The query that evaluates the value for computed property, for example - "SELECT VALUE LOWER(c.name) FROM c". |

### ConflictResolutionPolicy

| Name | Type | Required | Default Value | Description |
|--|--|--|--|--|
| `conflictResolutionPath` | String | true | | The conflict resolution path in the case of LastWriterWins mode. |
| `conflictResolutionProcedure` | String | true | | The procedure to resolve conflicts in the case of custom mode. |
| `mode` | [ConflictResolutionMode](#conflictresolutionmode) | false | LastWriterWins | Indicates the conflict resolution mode. |

### ConflictResolutionMode

| Name | Description |
|--|--|
| `LastWriterWins` | |
| `Custom` | |

### FullTextPolicy

| Name | Type | Required | Description |
|--|--|--|--|
| `defaultLanguage` | String | true | The default language for a full text paths. |
| `fullTextPaths` | [FullTextPath[]](#fulltextpath) | true | List of FullText Paths. |

### FullTextPath

| Name | Type | Required | Description |
|--|--|--|--|
| `language` | String | true | The language of the full text field in the document. |
| `path` | String | true | The path to the full text field in the document. |

### GeospatialConfig

| Name | Type | Required | Description |
|--|--|--|--|
| `type` | [GeospatialType](#geospatialtype) | true |  The geospatial type. |

### GeospatialType
| Name | Description |
|--|--|
| `Geography` | |
| `Geometry` | |


### IndexingPolicy

| Name | Type | Required | Default Value | Description |
|--|--|--|--|--|
| `automatic` | Boolean | true | | Indicates if the indexing policy is automatic. |
| `compositeIndexes` | [CompositePath[]](#compositepath) | true | | List of composite path list. |
| `excludedPaths` | [ExcludedPath[]](#excludedpath) | true | | List of paths to exclude from indexing. |
| `fullTextIndexes` | [FullTextIndexPath[]](#fulltextindexpath) | true | | List of paths to include in the full text indexing. |
| `includedPaths` | [IncludedPath[]](#includedpath) | true | | List of paths to include in the indexing. |
| `indexingMode` | [IndexingMode](#indexingmode) | true | consistent | Indicates the indexing mode. |
| `spatialIndexes` | [SpatialSpec[]](#spatialspec) | true | | List of spatial specifics. |
| `vectorIndexes` | [VectorIndex[]](#vectorindex) | true | | List of paths to include in the vector indexing. |

### CompositePath

| Name | Type | Required | Description |
|--|--|--|--|
| `order` | [CompositePathSortOrder](#compositepathsortorder) | true | Sort order for composite paths. |
| `path` | String | true | The path for which the indexing behavior applies to. Index paths typically start with root and end with wildcard (/path/*) |

### CompositePathSortOrder
| Name | Description |
|--|--|
| `ascending` | |
| `descending` | |

### ExcludedPath

| Name | Type | Required | Description |
|--|--|--|--|
| `path` | String | true |  The path for which the indexing behavior applies to. Index paths typically start with root and end with wildcard (/path/*) |

### FullTextIndexPath

| Name | Type | Required | Description |
|--|--|--|--|
| `path` | String | true | The path to the full text field in the document. |


### IncludedPath

| Name | Type | Required | Description |
|--|--|--|--|
| `indexes` | [Indexes[]](#indexes) | true |  Sort order for composite paths. |
| `path` | String | true | The path for which the indexing behavior applies to. Index paths typically start with root and end with wildcard (/path/*) |

### Indexes

| Name | Type | Required | Default Value | Description |
|--|--|--|--|--|
| `dataType` | String | false | String | The datatype for which the indexing behavior is applied to. |
| `dataType` | String | false | Hash | Indicates the type of index. |
| `precision` | Integer | true | | The precision of the index. -1 is maximum precision. |

### IndexingMode

| Name | Description |
|--|--|
| `consistent` | |
| `lazy` | |
| `none` | |

### SpatialSpec

| Name | Type | Required | Description |
|--|--|--|--|
| `path` | String | true | The path for which the indexing behavior applies to. Index paths typically start with root and end with wildcard (/path/*) |
| `types` | [SpatialType[]](#spatialtype) | true |  List of path's spatial type. |

### SpatialType

| Name | Description |
|--|--|
| `Point` | |
| `LineString` | |
| `Polygon` | |
| `MultiPolygon` | |

### VectorIndex
| Name | Type | Required | Default Value | Description |
|--|--|--|--|--|
| `indexingSearchListSize` | Integer<br>minimum: 25<br>maximum: 500 | false | 100 | This is the size of the candidate list of approximate neighbors stored while building the DiskANN index as part of the optimization processes. Large values may improve recall at the expense of latency. This is only applicable for the diskANN vector index type. |
| `path` | String | true | | The path to the vector field in the document. |
| `quantizationByteSize` | Integer<br>minimum: 4 | true | | The number of bytes used in product quantization of the vectors. A larger value may result in better recall for vector searches at the expense of latency. This is only applicable for the quantizedFlat and diskANN vector index types. |
| `types` | [VectorIndexType](#vectorindextype) | true | | The index type of the vector. Currently, flat, diskANN, and quantizedFlat are supported. |
| `vectorIndexShardKey` | String[] | true | | Array of shard keys for the vector index. This is only applicable for the quantizedFlat and diskANN vector index types. |

### VectorIndexType

| Name | Description |
|--|--|
| `flat` | |
| `diskANN` | |
| `quantizedFlat` | |

### ContainerPartitionKey

| Name | Type | Required | Default Value | Description |
|--|--|--|--|--|
| `kind` | PartitionKind | false | Hash | Indicates the kind of algorithm used for partitioning. For MultiHash, multiple partition keys (upto three maximum) are supported for container create. |
| `paths` | String[] | true | | List of paths using which data within the container can be partitioned. |
| `systemKey` | Boolean | true | | Indicates if the container is using a system generated partition key. |
| `version` | Integer<br>minimum: 1<br>maximum: 2 | true | | Indicates the version of the partition key definition. |

### UniqueKeyPolicy

| Name | Type | Required | Description |
|--|--|--|--|
| `uniqueKeys` | [UniqueKey[]](#uniquekey) | true | List of unique keys on that enforces uniqueness constraint on documents in the collection in the Azure Cosmos DB service. |

### UniqueKey
| Name | Type | Required | Description |
|--|--|--|--|
| `paths` | String[] | true | List of paths must be unique for each document in the Azure Cosmos DB service. |

### VectorEmbeddingPolicy

| Name | Type | Required | Description |
|--|--|--|--|
| `vectorEmbeddings` | [VectorEmbedding[]](#vectorembedding) | true | List of vector embeddings. |

### VectorEmbedding

| Name | Type | Required | Description |
|--|--|--|--|
| `dataType` | [VectorDataType](#vectordatatype) | true | Indicates the data type of vector. |
| `dimensions` | int32 | true | The number of dimensions in the vector. |
| `distanceFunction` | [DistanceFunction](#distancefunction) | true | The distance function to use for distance calculation in between vectors. |
| `path` | String | true | The path to the vector field in the document. |

### VectorDataType

| Name | Description |
|--|--|
| `float16` | |
| `float32` | |
| `uint8` | |
| `int8` | |

### DistanceFunction

| Name | Description |
|--|--|
| `euclidean` | |
| `cosine` | |
| `dotproduct` | |

### `definition.json` example

```json
{
  "$schema": "https://github.com/microsoft/json-schemas/blob/main/fabric/item/CosmosDB/definition/CosmosDB/1.0.0/schema.json",
  "containers": [
    {
      "options": {
        "autoscaleSettings": {
          "maxThroughput": 5000
        }
      },
      "resource": {
        "id": "SampleData",
        "computedProperties": [],
        "conflictResolutionPolicy": {
          "mode": "LastWriterWins",
          "conflictResolutionPath": "/_ts",
          "conflictResolutionProcedure": ""
        },
        "geospatialConfig": {
          "type": "Geography"
        },
        "indexingPolicy": {
          "automatic": true,
          "indexingMode": "Consistent",
          "includedPaths": [
            {
              "path": "/*",
              "indexes": []
            }
          ],
          "excludedPaths": [
            {
              "path": "/\"_etag\"/?"
            }
          ],
          "compositeIndexes": [],
          "spatialIndexes": [],
          "vectorIndexes": [],
          "fullTextIndexes": []
        },
        "partitionKey": {
          "paths": [
            "/category"
          ],
          "kind": "Hash",
          "version": 2
        },
        "uniqueKeyPolicy": {
          "uniqueKeys": []
        }
      }
    }
  ]
}
```

## Definition Example
Here's an example of a Base64-encoded Cosmos DB Database definition, where the content from [`definition.json` example](#definitionjson-example) is encoded in Base64 and placed in the `payload` field with the path set to `definition.json`:

```JSON
{
  "definition": {
    "parts": [
      {
        "path": "definition.json",
        "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9pdGVtL0Nvc21vc0RCL2RlZmluaXRpb24vQ29zbW9zREIvMi4wLjAvc2NoZW1hLmpzb24iLAogICJjb250YWluZXJzIjogWwogICAgewogICAgICAib3B0aW9ucyI6IHsKICAgICAgICAiYXV0b3NjYWxlU2V0dGluZ3MiOiB7CiAgICAgICAgICAibWF4VGhyb3VnaHB1dCI6IDUwMDAKICAgICAgICB9CiAgICAgIH0sCiAgICAgICJyZXNvdXJjZSI6IHsKICAgICAgICAiaWQiOiAiU2FtcGxlRGF0YSIsCiAgICAgICAgImNvbXB1dGVkUHJvcGVydGllcyI6IFtdLAogICAgICAgICJjb25mbGljdFJlc29sdXRpb25Qb2xpY3kiOiB7CiAgICAgICAgICAibW9kZSI6ICJMYXN0V3JpdGVyV2lucyIsCiAgICAgICAgICAiY29uZmxpY3RSZXNvbHV0aW9uUGF0aCI6ICIvX3RzIiwKICAgICAgICAgICJjb25mbGljdFJlc29sdXRpb25Qcm9jZWR1cmUiOiAiIgogICAgICAgIH0sCiAgICAgICAgImdlb3NwYXRpYWxDb25maWciOiB7CiAgICAgICAgICAidHlwZSI6ICJHZW9ncmFwaHkiCiAgICAgICAgfSwKICAgICAgICAiaW5kZXhpbmdQb2xpY3kiOiB7CiAgICAgICAgICAiYXV0b21hdGljIjogdHJ1ZSwKICAgICAgICAgICJpbmRleGluZ01vZGUiOiAiQ29uc2lzdGVudCIsCiAgICAgICAgICAiaW5jbHVkZWRQYXRocyI6IFsKICAgICAgICAgICAgewogICAgICAgICAgICAgICJwYXRoIjogIi8qIiwKICAgICAgICAgICAgICAiaW5kZXhlcyI6IFtdCiAgICAgICAgICAgIH0KICAgICAgICAgIF0sCiAgICAgICAgICAiZXhjbHVkZWRQYXRocyI6IFsKICAgICAgICAgICAgewogICAgICAgICAgICAgICJwYXRoIjogIi9cIl9ldGFnXCIvPyIKICAgICAgICAgICAgfQogICAgICAgICAgXSwKICAgICAgICAgICJjb21wb3NpdGVJbmRleGVzIjogW10sCiAgICAgICAgICAic3BhdGlhbEluZGV4ZXMiOiBbXSwKICAgICAgICAgICJ2ZWN0b3JJbmRleGVzIjogW10sCiAgICAgICAgICAiZnVsbFRleHRJbmRleGVzIjogW10KICAgICAgICB9LAogICAgICAgICJwYXJ0aXRpb25LZXkiOiB7CiAgICAgICAgICAicGF0aHMiOiBbCiAgICAgICAgICAgICIvY2F0ZWdvcnkiCiAgICAgICAgICBdLAogICAgICAgICAgImtpbmQiOiAiSGFzaCIsCiAgICAgICAgICAidmVyc2lvbiI6IDIKICAgICAgICB9LAogICAgICAgICJ1bmlxdWVLZXlQb2xpY3kiOiB7CiAgICAgICAgICAidW5pcXVlS2V5cyI6IFtdCiAgICAgICAgfQogICAgICB9CiAgICB9CiAgXQp9",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ZG90UGxhdGZvcm1CYXNlNjRTdHJpbmc=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```
