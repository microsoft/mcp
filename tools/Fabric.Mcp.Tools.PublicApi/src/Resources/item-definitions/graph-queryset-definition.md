---
title: Graph QuerySet definition
description: Learn how to create a Graph QuerySet item definition when using the Microsoft Fabric REST API.
ms.title: Graph QuerySet item definition
author: fluffycc
ms.author: cacao
ms.service: fabric
ms.date: 09/02/2025
---

# Graph QuerySet definition
  
This article provides a breakdown of the definition structure for Graph QuerySet items.
  
## Definition parts
  
This table lists the Graph QuerySet definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `graphInstanceObjectId` | Guid | true | The object id referring to the graph to query |
| `graphInstanceFolderObjectId` | Guid | true | The folder id containing the graph to query  |
| `queries` | [Query](#query)[] | true | query array |

### Query
  
Describes the structure of the query.

| Name         | Type                        | Description                                            |
|--------------|-----------------------------|--------------------------------------------------------|
| displayName  | string                      | The name of the query as shown in UX                   |
| id           | string / Guid               | Unique id of the query                                 |
| queryMode    | "GQLCode" / "VisualDiagram" | Whether the query is custom gql or a low-code diagram  |
| GqlQueryText | string                      | The custom gql query to run                            |
| nodes        | [Node](#node)[]             | The graph nodes to include in the query                |
| edges        | [Edge](#edge)[]             | The graph edges to include in the query                |

### Node
  
Describes the structure of the node.

| Name               | Type                      | Description                                       |
|--------------------|---------------------------|---------------------------------------------------|
| alias              | string / Guid             | Alias of the node as defined in graph             |
| posX               | number                    | The x-coordinate of position in query diagram     |
| posY               | number                    | The y-coordinate of position in query diagram     |
| format             | [NodeFormat](#nodeformat) | The formatting definition for this node           |
| filters            | [Filter](#filter)[]       | Any filters applied to this node in query         |
| displayPropertyKey | string                    | The name of the property to display on the result |
  
### Edge
  
Describes the structure of the edge.

| Name                 | Type                | Description                                       |
|----------------------|---------------------|---------------------------------------------------|
| alias                | string / Guid       | Alias of the edge as defined in graph             |
| sourceNodeAlias      | string / Guid       | Alias of the node this edge is coming out from    |
| destinationNodeAlias | string / Guid       | Alias of the node this edge is going to           |
| filters              | [Filter](#filter)[] | Any filters applied to this edge in query         |
| displayPropertyKey   | string              | The name of the property to display on the result |

### NodeFormat
  
Describes the structure of the nodeformart.

| Name  | Type                | Description        |
|-------|---------------------|--------------------|
| color | string              | Hex code of color  |
| shape | “square” / “circle” | Shapes of the node |

### Filter
  
Describes the structure of the filter.

| Name            | Type   | Description                              |
|-----------------|--------|------------------------------------------|
| propertyName    | string | Proptery name of the filter              |
| sourceNodeAlias | string | Alias of the sources node in the filter  |
| value           | string | The value to filter the property against |

### QuerySet Example

```json
{
  "graphInstanceObjectId": "07e35924-f80e-4fdb-8284-10a283bea60d",
  "graphInstanceFolderObjectId": "f4e45c12-b576-4342-a432-ea276ce5a25c",
  "queries": [
    {
      "displayName": "query",
      "id": "a0bf1bf1-0d9e-41a1-bafe-df720e335294",
      "queryMode": "GQLCode",
      "gqlQueryText": "MATCH (node_Customer:Customer)-[edge_bought:bought]->(node_Book:Book) RETURN node_Customer AS Customer, edge_bought AS bought, node_Book AS Book LIMIT 100",
      "nodes": [],
      "edges": []
    }
  ]
}
```

### Query Array Example

```json
{
  "queries": [
    {
      "displayName": "new query",
      "id": "some_GUID",
      "queryMode": "GQLCode",
      "gqlQueryText": "MATCH (node_Author:Author)-[edge_wrote:wrote]->(node_Book:Book) RETURN node_Author AS Author, edge_wrote AS wrote, node_Book AS Book LIMIT 100",
      "nodes": [],
      "edges": []
    }
  ]
}
```