---
title: Variable library definition
description: Learn how to create a Variable library item definition when using the Microsoft Fabric REST API.
ms.title: Variable library item definition
author: KirilKorablov
ms.author: kkorablov
ms.service: fabric
ms.date: 01/14/2025
---
  
# Variable library definition
  
This article provides a breakdown of the definition structure for variable library items.
  
## Definition parts
  
This table lists the variable library definition parts.
  
<!-- Replace the info in this example table with the definitions of the new item. List all the parts the definition has in the table -->
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `variables.json` | Variables (JSON) | &#9989; | Describes the variables in the item |
| `settings.json` | VariableLibrarySettings (JSON) | &#9989; | Define user controlled settings |
| `.platform` | PlatformDetails (JSON) | &#10060; | Describes common details of the item |
| `valueSets\valueSetName.json` | ValueSet (JSON) | &#10060; | Define the default template to be used when rendering the item |


## Variables
  
Describes the variables in a Variable Library item.
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Description                                     |
|-----------|-----------------|-------------------------------------------------|
| variables | Variable[]      | The variables collection.                    |
  
### Variable
  
Describes a variable.
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Required | Description                                     |
|-----------|-----------------|----------|--------------------------------------|
| name      | String          | &#9989; | The name of the variable.                           |
| type      | String          | &#9989; | The variable type.                              |
| value     | JSON          | &#9989; | The default value of the variable.                  |
| note      | String          | &#10060; | A note describing the variable.                 |

### Supported variable types

List of supported variable types.

| Name      | Type            | Example                                       |
|-----------|-----------------------------------------------|-----------------------------------------------|
| Boolean      | BooleanVariable          |  true, false      |
| DateTime      | DateTimeVariable          |  "2025-01-20T15:30:00Z"      |
| Number      | NumberVariable          |  1.1      |
| Integer     | IntegerVariable          |  1      |
| String     | StringVariable          |  "Some string"      |
| ItemReference     | ItemReferenceVariable          |  <br> { <br> &nbsp;&nbsp;"workspaceId": "aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb", <br> &nbsp;&nbsp;"itemId": "bbbbbbbb-1111-2222-3333-cccccccccccc" <br> }      |


### Variables.json example <!-- an example of an ExecutionDetails JSON -->

```JSON
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/variableLibrary/definition/variables/1.0.0/schema.json",
  "variables": [
    {
      "name": "variable1",
      "note": "Some optional note",
      "type": "String",
      "value": "Some string value"
    },
    {
      "name": "variableWithoutNote",
      "type": "boolean",
      "value": true
    }
  ]
}
```

## ValueSet
  
Describes a value set. File name must be similar to value set name.
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Description                                     |
|-----------|-----------------|-------------------------------------------------|
| name      | String          | Name of the value set.                          |
| description| String         | Description of the value set.                   |
| variableOverrides| VariableOverride| List of overridden variables.                |

### VariableOverride
  
Describes a value override.
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Description                                     |
|-----------|-----------------|-------------------------------------------------|
| name      | String          | Name of the overridden variable.                          |
| value| String         | The new variable value.                   |
  
### valueSets/valueSet.json example <!-- an example of an ExecutionDetails JSON -->

```JSON
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/variableLibrary/definition/valueSet/1.0.0/schema.json",
  "name": "valueSetName",
  "variableOverrides": [
    {
      "name": "variable1",
      "value": "Some new value"
    }
  ]
}
```

## VariableLibrarySettings
  
Describes user defined Variable Library settings.
  
<!-- Provide a detailed list of contracts for each of the API types, in a form of a table. The table should look like the table in the reference pages (docs) and contain the root object properties. Provide additional tables for every non-primitive type which requires specification. In this template there are three examples of ExecutionDetails non-primitive types: ExecutionStatus, Log and LogLevel. At the end of each H2 section provide an example of the type. -->
  
| Name      | Type            | Description                                     |
|-----------|-----------------|-------------------------------------------------|
| valueSetsOrder      | String[]          | Optional list of value set names for ordering.                          |
  
### settings.json example <!-- an example of an ExecutionDetails JSON -->

```JSON
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/variableLibrary/definition/settings/1.0.0/schema.json",
  "valueSetsOrder": [
    "valueSet1",
    "valueSet0",
    "someOtherValueSet"
  ]
}
```

### Considerations and limitations

- ValueSetsOrder list can be empty or partial.
- ValueSetsOrder list can't include invalid value set names.
- When updating a variable library item with a partial or empty valueSetsOrder list, missing value set names are added to the end of the list in alphabetical order.

## Definition example

```json
{
"parts": [
    {
        "path": "variables.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
    },
    {
        "path": "valueSet/valueSet1.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
    },
    {
        "path": "valueSet/valueSet2.json",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
    },
    {
        "path": "settings.json",
        "payload": "<base64 encoded string>",
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
