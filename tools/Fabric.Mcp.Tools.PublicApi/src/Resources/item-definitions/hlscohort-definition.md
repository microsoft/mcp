---
title: HLSCohort definition
description: Learn how to create a HLSCohort item definition when using the Microsoft Fabric REST API.
ms.title: Microsoft Fabric HLSCohort item definition
author: IshanHasan
ms.author: ishanhasan
ms.service: fabric
ms.date: 04/08/2024
---

# HLSCohort definition

This article provides a breakdown of the definition structure for HLSCohort items.

>[!NOTE]
>HLSCohort is also known as Healthcare Cohort (preview).
  
## Definition parts
  
This table lists the HLSCohort definition parts. <!-- remove 'pattern1' if there's only one way of defining the item -->
  
<!-- Replace the info in this example table with the definitions of the new item. List all the parts the definition has in the table -->
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `healthcarecohort.metadata.json` | HLSCohortDetails (JSON) | true | Describes the metadata of the cohort. |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item. |

## HLSCohortDetails
  
Describes the metadata of the cohort. <!-- Provide a description of the type. -->
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Description                                     |
|-----------|-----------------|-------------------------------------------------|
| dependencies    | HLSCohortDependency[] | A list of the related items.                |
| queryRequest | CohortDetails          | A set of conditions which define the cohort. |

### HLSCohortDependency <!-- an H3 title -->

A related item to the HLSCohort item.

| Name      | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| itemType | String   | The type of the related item. |
| itemObjectId   | String   | The itemId of the related item.                                  |
| folderObjectId     | String | The workspaceId of the related item.                                     |  
  
### CohortDetails <!-- an H3 title -->

The details of the cohort and the format of the query request.

| Name      | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| schemaName | String   | The name of the schema. |
| schemaVersion   | String   | The version of the schema.                                   |
| entryPoint     | String | The entrypoint of the schema.                                     |
| criteriaExpressionNodes | CriteriaExpressionNode[] | The criteria expression nodes. |

### CriteriaExpressionNode <!-- an H3 title -->

A node in the criteria expression tree.

| Name      | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| nodeType | NodeType  | The type of the criteria expression node. |
| criteriaCondition | CriteriaCondition | The criteria condition. |
| children | CriteriaExpressionNode[] | The children of the criteria expression node. |

### NodeType (Enum) <!-- an H3 title -->

The type of the criteria expression node including logical operators and condition (leaf) nodes.

| Name    | Description            |
|---------|------------------------|
| And | Logical AND operator. |
| Or | Logical OR operator. |
| UnitAnd | Logical AND between conditions on the same row of data. |
| Not | Logical NOT operator. |
| Leaf | Leaf node. |

### CriteriaCondition <!-- an H3 title -->

A condition in the criteria expression.

| Name      | Type     | Description                                    |
|-----------|----------|------------------------------------------------|
| propertyPath | String | The property path of the criteria condition. |
| rawValueSchemaDataType | ValueType | The schema data type of the raw value. |
| rawValue | String | The raw value of the criteria condition. |
| criteriaOperator | CriteriaOperator | The criteria operator. |
| isValueList | Boolean | Whether the value is a list or not. |

### ValueType (Enum) <!-- an H3 title -->

The data type of the raw value of this criteria condition.

| Name    | Description            |
|---------|------------------------|
| String | String data type. |
| Double | Double data type. |
| Integer | Integer data type. |
| Float | Float data type. |
| Long | Long data type. |
| Date | Date data type. |
| DateTime | DateTime data type. |
| Concept | Concept data type. |

### CriteriaOperator (Enum) <!-- an H3 title -->

The operator to be used in the criteria condition.

| Name    | Description            |
|---------|------------------------|
| Equal | Equal operator. |
| GreaterThan | Greater than operator. |
| GreaterThanOrEqual | Greater than or equal to operator. |
| LessThan | Less than operator. |
| LessThanOrEqual | Less than or equal to operator. |
| ContainedIn | Contained in operator. |

### HLSCohortDetails example <!-- an example of an HLSCohortDetails JSON -->

```json
{
  "dependencies": [
    {
      "itemType": "HealthDataManager",
      "itemObjectId": "00000000-0000-0000-0000-000000000000",
      "folderObjectId": "00000000-0000-0000-0000-000000000000"
    }
  ],
  "queryRequest": {
    "schemaName": "OMOP",
    "schemaVersion": "0.0.0",
    "entryPoint": "Person",
    "criteriaExpressionNodes": [
      {
        "nodeType": "And",
        "children": [
          {
            "nodeType": "Leaf",
            "criteriaCondition": {
              "propertyPath": "Person.YearOfBirth",
              "rawValueSchemaDataType": "Integer",
              "rawValue": "2000",
              "criteriaOperator": "Equal",
              "isValueList": false
            }
          }
        ]
      }
    ]
  }
}
```
