# DocumentDB MCP Server Commands Documentation

## Table of Contents

1. [Connection Tools](#1-connection-tools)
2. [Database Tools](#2-database-tools)
3. [Collection Tools](#3-collection-tools)
4. [Document Tools](#4-document-tools)
5. [Index Tools](#5-index-tools)
6. [Workflow Tools](#6-workflow-tools)

---

## 1. Connection Tools

### 1.1 connect_mongodb

**Name**: Connect to MongoDB  
**Description**: Connect to a MongoDB instance with a connection string

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `connection_string` | `string` | Yes | MongoDB connection string (e.g., `mongodb://localhost:27017`) |
| `test_connection` | `boolean \| string` | No | Test the connection after connecting (default: `true`) |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted connection result with status information"
}
```

---

### 1.2 disconnect_mongodb

**Name**: Disconnect from MongoDB  
**Description**:  Disconnect from the current MongoDB instance

**Parameters**:  None

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted disconnection result"
}
```

---

### 1.3 get_connection_status

**Name**: Get Connection Status  
**Description**: Get the current MongoDB connection status and details

**Parameters**:  None

**Returns**:

```json
{
  "type":  "text",
  "text":  "JSON formatted connection status information"
}
```

---

## 2. Database Tools

### 2.1 list_databases

**Name**: List Databases  
**Description**: List all databases in the DocumentDB instance

**Parameters**: None

**Returns**:

```json
{
  "type": "text",
  "text": "[\"admin\", \"local\", \"mydb\"]"
}
```

Return format:  JSON array of database names

---

### 2.2 db_stats

**Name**: Database Statistics  
**Description**: Get detailed statistics about a database's size and storage usage

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted database statistics (includes dataSize, storageSize, indexSize, etc.)"
}
```

---

### 2.3 get_db_info

**Name**:  Get Database Info  
**Description**:  Get database information including all collections and their document counts

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "database_name": "string",
    "collections": [
      {
        "name":  "string",
        "count":  "number",
        "error": "string (optional)"
      }
    ]
  }
}
```

---

### 2.4 drop_database

**Name**: Drop Database  
**Description**: Drop a database and all its collections

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database to drop |

**Returns**: 

```json
{
  "type": "text",
  "text": {
    "success": "boolean",
    "message": "string",
    "data": "object"
  }
}
```

---

## 3. Collection Tools

### 3.1 collection_stats

**Name**: Collection Statistics  
**Description**: Get detailed statistics about a collection's size and storage usage

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |

**Returns**: 

```json
{
  "type": "text",
  "text": "JSON formatted collection statistics"
}
```

---

### 3.2 rename_collection

**Name**: Rename Collection  
**Description**: Rename a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection to rename |
| `new_collection_name` | `string` | Yes | New name for the collection |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "message": "Collection renamed successfully"
  }
}
```

---

### 3.3 drop_collection

**Name**: Drop Collection  
**Description**: Drop a collection from a database

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection to drop |

**Returns**: 

```json
{
  "type": "text",
  "text": {
    "message": "Collection dropped successfully"
  }
}
```

---

### 3.4 sample_documents

**Name**: Sample Documents  
**Description**:  Retrieve sample documents from a specific collection.  Useful for understanding data schema and query generation

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `sample_size` | `number \| string` | No | Number of documents to sample (default: `10`) |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON array of sample documents"
}
```

---

## 4. Document Tools

### 4.1 find_documents

**Name**: Find Documents  
**Description**: Find documents in a collection.  Supports consolidated "options" object (limit, skip, sort, projection)

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection to query |
| `query` | `object \| string` | No | Query filter in MongoDB style (default: `{}`) |
| `options` | `object \| string` | No | Query options:  `limit` (default 100), `skip` (default 0), `sort`, `projection` |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "documents": "Array<object>",
    "total_count": "number",
    "returned_count": "number",
    "has_more": "boolean",
    "query": "object",
    "applied_options": {
      "limit": "number",
      "skip": "number",
      "sort": "object (optional)",
      "projection": "object (optional)"
    }
  }
}
```

---

### 4.2 count_documents

**Name**: Count Documents  
**Description**: Count documents in a collection matching a query

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection to query |
| `query` | `object \| string` | No | Query filter in MongoDB style (default:  `{}`) |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "count": "number",
    "query": "object"
  }
}
```

---

### 4.3 insert_document

**Name**: Insert Document  
**Description**: Insert a single document into a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `document` | `object` | Yes | Document to insert |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "inserted_id": "string",
    "acknowledged": "boolean",
    "inserted_count": 1
  }
}
```

---

### 4.4 insert_many

**Name**:  Insert Many Documents  
**Description**:  Insert multiple documents into a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `documents` | `Array<object> \| string` | Yes | List of documents to insert |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "inserted_ids": "Array<string>",
    "acknowledged": "boolean",
    "inserted_count": "number"
  }
}
```

---

### 4.5 update_document

**Name**: Update Single Document  
**Description**: Update a document in a collection

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `filter` | `object \| string` | Yes | Query filter to find the document |
| `update` | `object \| string` | Yes | Update operations ($set, $inc, etc.) |
| `upsert` | `boolean \| string` | No | Create document if it doesn't exist (default: `false`) |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "matched_count": "number",
    "modified_count": "number",
    "upserted_id": "string | null",
    "acknowledged": "boolean"
  }
}
```

---

### 4.6 update_many

**Name**: Update Many Documents  
**Description**: Update multiple documents in a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `filter` | `object \| string` | Yes | Query filter to find the documents |
| `update` | `object \| string` | Yes | Update operations ($set, $inc, etc.) |
| `upsert` | `boolean \| string` | No | Create document if it doesn't exist (default: `false`) |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "matched_count": "number",
    "modified_count": "number",
    "upserted_id": "string | null",
    "acknowledged": "boolean"
  }
}
```

---

### 4.7 delete_document

**Name**: Delete Document  
**Description**: Delete a document from a collection

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `filter` | `object \| string` | Yes | Query filter to find the document |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "deleted_count": "number",
    "acknowledged": "boolean"
  }
}
```

---

### 4.8 delete_many

**Name**: Delete Many Documents  
**Description**: Delete multiple documents from a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `filter` | `object \| string` | Yes | Query filter to find the documents |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "deleted_count": "number",
    "acknowledged": "boolean"
  }
}
```

---

### 4.9 aggregate

**Name**: Aggregate Pipeline  
**Description**: Run an aggregation pipeline on a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `pipeline` | `Array<object> \| string` | Yes | List of aggregation stages |
| `allow_disk_use` | `boolean \| string` | No | Allow pipeline stages to write to disk (default: `false`) |

**Returns**:

```json
{
  "type":  "text",
  "text":  {
    "results": "Array<object>",
    "total_count": "number"
  }
}
```

---

### 4.10 find_and_modify

**Name**: Find And Modify Document  
**Description**:  Find one document by filter and apply update; returns the document BEFORE modification (or null if it doesn't exist)

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection to query |
| `query` | `object \| string` | Yes | Query filter in MongoDB style |
| `update` | `object \| string` | Yes | Update operations ($set, $inc, etc.) |
| `upsert` | `boolean \| string` | No | Create document if it does not exist (default: `false`) |

**Returns**:

```json
{
  "type":  "text",
  "text":  {
    "matched":  "boolean",
    "upsertedId": "string | undefined",
    "original_document": "object | null",
    "query": "object",
    "update": "object",
    "upsert": "boolean"
  }
}
```

---

### 4.11 explain_find_query

**Name**: Explain Find Query  
**Description**:  Explain the execution plan with execution stats for a find query using consolidated options (sort, projection, limit, skip)

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `query` | `object \| string` | No | Query filter in MongoDB style (default: `{}`) |
| `options` | `object \| string` | No | Query options: `sort`, `projection`, `limit`, `skip` |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "options_applied": {
      "sort": "object (optional)",
      "projection": "object (optional)",
      "limit": "number (optional)",
      "skip": "number (optional)"
    },
    "explain":  "object (MongoDB explain output)"
  }
}
```

---

### 4.12 explain_count_query

**Name**: Explain Count Query  
**Description**: Explain the execution plan with execution stats for count query on a given collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `query` | `object \| string` | No | Query filter in MongoDB style (default: `{}`) |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted explain output"
}
```

---

### 4.13 explain_aggregate_query

**Name**: Explain Aggregate Query  
**Description**:  Explain the execution plan with execution stats for an aggregation query on a given collection

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `pipeline` | `Array<object> \| string` | Yes | List of aggregation stages |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted explain output"
}
```

---

## 5. Index Tools

### 5.1 create_index

**Name**: Create Index  
**Description**: Create an index on a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `keys` | `object \| string` | Yes | Dictionary defining the index (e.g., `{"field": 1}` for ascending) |
| `options` | `object \| string` | No | Index options (e.g., `{unique: true, name: 'idx'}`) (default: `{}`) |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "index_name": "string",
    "keys": "object",
    "options": "object"
  }
}
```

---

### 5.2 list_indexes

**Name**: List Indexes  
**Description**: List all indexes on a collection

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |

**Returns**:

```json
{
  "type":  "text",
  "text":  {
    "indexes": "Array<object>",
    "count": "number"
  }
}
```

---

### 5.3 drop_index

**Name**: Drop Index  
**Description**: Drop an index from a collection

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |
| `index_name` | `string` | Yes | Name of the index to drop |

**Returns**:

```json
{
  "type": "text",
  "text": {
    "success": "boolean",
    "message": "string",
    "data": "object"
  }
}
```

---

### 5.4 index_stats

**Name**: Index Statistics  
**Description**: Get statistics for indexes on a collection

**Parameters**: 

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `db_name` | `string` | Yes | Name of the database |
| `collection_name` | `string` | Yes | Name of the collection |

**Returns**: 

```json
{
  "type": "text",
  "text": "JSON array of index statistics"
}
```

---

### 5.5 current_ops

**Name**: Current Operations  
**Description**: Get information about current MongoDB operations

**Parameters**:

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `ops` | `object \| string \| null` | No | Optional filter to narrow down the operations returned |

**Returns**:

```json
{
  "type": "text",
  "text": "JSON formatted current operations information"
}
```

---


## Command Summary

| Category | Command | Description |
|----------|---------|-------------|
| **Connection** | `connect_mongodb` | Connect to MongoDB |
| **Connection** | `disconnect_mongodb` | Disconnect |
| **Connection** | `get_connection_status` | Get connection status |
| **Database** | `list_databases` | List databases |
| **Database** | `db_stats` | Database statistics |
| **Database** | `get_db_info` | Database info |
| **Database** | `drop_database` | Drop database |
| **Collection** | `collection_stats` | Collection statistics |
| **Collection** | `rename_collection` | Rename collection |
| **Collection** | `drop_collection` | Drop collection |
| **Collection** | `sample_documents` | Sample documents |
| **Document** | `find_documents` | Find documents |
| **Document** | `count_documents` | Count documents |
| **Document** | `insert_document` | Insert single document |
| **Document** | `insert_many` | Insert multiple documents |
| **Document** | `update_document` | Update single document |
| **Document** | `update_many` | Update multiple documents |
| **Document** | `delete_document` | Delete single document |
| **Document** | `delete_many` | Delete multiple documents |
| **Document** | `aggregate` | Aggregation pipeline |
| **Document** | `find_and_modify` | Find and modify |
| **Document** | `explain_find_query` | Explain find query |
| **Document** | `explain_count_query` | Explain count query |
| **Document** | `explain_aggregate_query` | Explain aggregate query |
| **Index** | `create_index` | Create index |
| **Index** | `list_indexes` | List indexes |
| **Index** | `drop_index` | Drop index |
| **Index** | `index_stats` | Index statistics |
| **Index** | `current_ops` | Current operations |

**Total**:  29 Commands