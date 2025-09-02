---
title: API for GraphQL definition
description: Learn how to structure an GraphQLApi item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric GraphQLApi item definition
author: vadeveka
ms.author: vadeveka
ms.service: fabric
ms.date: 1/22/2025
---

# API for GraphQL definition

This article provides a breakdown of the definition structure for GraphQLApi items.
  
## Definition parts
  
This table lists the GraphQLApi definition parts.
    
| Definition part path | type | Required | Description |
|--|--|--|--|
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |
| `graphql-definition.json` | GraphQLDefinition (JSON) | true | Describes the general and datasource configuration of the API for GraphQL |


## GraphQLDefinition

| Name | Type | Description |
|------|------|-------------|
| $schema | String | URL for schema specification |
| datasources | Source[] | List of data sources associated with API for GraphQL |

### Source

| Name | Type | Description |
|------|------|-------------|
| sourceItemId | Guid | ID of the source item |
| sourceWorkspaceId | Guid | ID of the workspace containing the source item |
| connectionId | Guid | ID of the connection used for the source item |
| sourceType | SourceType | Type of source item |
| objects | SourceObject[] | List of objects in the source item that are exposed using the GraphQL API |

### SourceType (enum)

| Name | Description |
|------|-------------|
| SqlAnalyticsEndpoint | SQL Analytics Endpoint |
| Warehouse | Warehouse |
| SqlDbNative | SQL Database |
| AzureSql | Azure SQL Database |

### SourceObject

| Name | Type | Description |
|------|------|-------------|
| graphqlType | String | GraphQL type name to use for object in source item |
| sourceObject | String | Object name in the source item that's exposed by the GraphQL API |
| sourceObjectType | SourceObjectType | Object type in the source item |
| actions | <ActionType, ActionValue> | Dictionary of action type to action value that indicates queries and mutations with their enablement status |
| fieldMappings | <String, String> | Dictionary of fields in source object to GraphQL fields |
| relationships | Relationship[] | List of relationships configured from the source object |
| IsStoredProcedureQueryNode | Boolean | GraphQL operation type for exposing stored procedure. True - Query, False - Mutation. |

### SourceObjectType (enum)

| Name | Description |
|------|-------------|
| Table | Database table |
| View | Database view |
| StoredProcedure | Database stored procedure |

### ActionType (enum)

| Name | Description |
|------|-------------|
| Query | Query on source object |
| Query_by_pk | Query by primary key on source object |
| Create | Create mutation on source object |
| Update | Update mutation on source object |
| Delete | Delete mutation on source object |
| Execute | Execute query or mutation on stored procedure |

### ActionValue (enum)

| Name | Description |
|------|-------------|
| Enabled | Enable query or mutation |
| Disabled | Disable query or mutation |

### Relationship

| Name | Type | Description |
|------|------|-------------|
| field | String | GraphQL field name to use for the relationship in GraphQL type |
| cardinality | RelationshipCardinality | Cardinality for the relationship |
| targetObject | String | Target object for the relationship |
| sourceFields | String[] | List of fields in the source object used for the relationship |
| targetFields | String[] | List of fields in target object used for the relationship |
| linkingObject | String | Linking object for many-to-many relationships between source and target objects |
| linkingSourceFields | String[] | List of fields in the linking object for the relationship from the source object |
| linkingTargetFields | String[] | List of fields in the linking object for the relationship to the target object |

### RelationshipCardinality (enum)

| Name | Description |
|------|-------------|
| ManyToOne | Many-to-One relationship |
| OneToMany | One-to-Many relationship |
| OneToOne | One-to-One relationship |
| ManyToMany | Many-to-Many relationship |


### GraphQLDefinition example

```JSON
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/graphqlApi/definition/1.0.0/schema.json",
  "datasources": [
    {
      "objects": [
        {
          "actions": {
            "Query": "Enabled"
          },
          "fieldMappings": {
            "LastName": "LastName",
            "Name": "Name"
          },
          "graphqlType": "Customers",
          "relationships": [],
          "sourceObject": "model.Customers",
          "sourceObjectType": "View"
        }
      ],
      "sourceItemId": "c54cd4ce-cbaa-4fc5-bcc8-121f2a3e0c4c",
      "sourceType": "Warehouse",
      "sourceWorkspaceId": "c2231263-b81e-4dd2-a69b-41eca8c93584"
    },
    {
      "connectionId": "daa5dc10-6eae-4b19-af5f-da8db84c51c1",
      "objects": [
        {
          "actions": {
            "Query": "Enabled"
          },
          "fieldMappings": {
            "id": "id",
            "publisher_id": "publisher_id",
            "title": "title"
          },
          "graphqlType": "publishers",
          "relationships": [],
          "sourceObject": "dbo.publishers_view",
          "sourceObjectType": "View"
        },
        {
          "actions": {
            "Query": "Enabled",
            "Query_by_pk": "Enabled",
            "Create": "Enabled",
            "Update": "Enabled",
            "Delete": "Disabled"
          },
          "fieldMappings": {
            "id": "id",
            "title": "title"
          },
          "graphqlType": "books",
          "relationships": [],
          "sourceObject": "dbo.books",
          "sourceObjectType": "Table"
        }
      ],
      "sourceItemId": "daa5dc10-6eae-4b19-af5f-da8db84c51c1",
      "sourceType": "AzureSql",
      "sourceWorkspaceId": "00000000-0000-0000-0000-000000000000"
    }
  ]
}
```