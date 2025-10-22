---
title: User Data Function definition
description: Learn how to create a User Data Function item definition when using the Microsoft Fabric REST API.
ms.title: User Data Function item definition
author: yareyes
ms.author: yareyes
ms.service: fabric
ms.date: 09/09/2025
---
  
# User Data Function definition
  
This article provides a breakdown of the definition structure for User Data Function definition items.
  
## Definition parts
  
This table lists the User Data Function definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `resources\functions.json` | [FunctionsMetadata](#functionsmetadata) (JSON) | true | Describes additional metadata information for the functions, for a simple artifact leave an empty json object |
| `privateLibraries\<libraryname>.whl` | CustomLibraries (WHL) | false | A custom wheel file or files in Base64 encoded format |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |
| `definition.json` | [UserDataFunctionDefinition](#userdatafunctiondefinition) (JSON) | true | Describes the functions, connected data sources, and libraries for a User Data Function artifact  |
| `function_app.py` | [PythonScript](#function_apppy-example) (PY) | true | The python script containing all the functions, for a simple artifact leave an empty file |

## UserDataFunctionDefinition

| Name | Type | Description |
|------|------|-------------|
| $schema | String | Url for schema specification |
| runtime | [RunTime](#runtime-enum) | Runtime environment for execution |
| connectedDataSources | [ConnectedDataSourceObject](#connecteddatasourceobject)[] | List of data sources connected to the User Data Function |
| functions | [FunctionObject](#functionobject)[] | List of functions associated with the User Data Function |
| libraries | [Libraries](#libraries) | Library object containing all public and private libraries used by the functions |

### RunTime (enum)

| Name | Description |
|------|-------------|
| NOTASSIGNED | Unassigned runtime |
| PYTHON | Python runtime |

### ConnectedDataSourceObject

| Name | Type | Description |
|------|------|-------------|
| alias | String | Alias for the connected data source |
| artifactId | Guid | Artifact ID of the connected data source |
| artifactType | String | Type of the connected data source |
| workspaceId | Guid | Workspace ID of the connected data source |

### FunctionObject

| Name | Type | Description |
|------|------|-------------|
| name | String | Name of the function |
| description | String | Description of the function |
| isPublicEndpointEnabled | Boolean | Indicates whether public url for function is enabled or not |

### Libraries

| Name | Type | Description |
|------|------|-------------|
| public | [PublicLibraryObject](#publiclibraryobject)[] | List of public packages available for the User Data Function |
| private | [PrivateLibraryObject](#privatelibraryobject)[] | List of private packages available for the User Data Function |

### PublicLibraryObject

| Name | Type | Description |
|------|------|-------------|
| version | String | Version of the package |
| name | String | Name of the package |
| type | [PackageType](#packagetype-enum) | Type of the package |

### PrivateLibraryObject

| Name | Type | Description |
|------|------|-------------|
| name | String | Name of the package |
| type | [PackageType](#packagetype-enum) | Type of the package |

### PackageType (enum)

| Name | Description |
|------|-------------|
| PYPI | Python Package Index (PYPI) type |
| WHEEL | Wheel type |

### UserDataFunctionDefinition example

```JSON
{
  "$schema": "https://developer.microsoft.com/json-schemas/fabric/item/userDataFunction/definition/1.1.0/schema.json",
  "runtime": "PYTHON",
  "connectedDataSources": [
    {
      "alias": "lh1y",
      "artifactId": "99558215-3e1f-848f-46a0-89e8f9ecf441",
      "artifactType": "Lakehouse",
      "workspaceId": "00000000-0000-0000-0000-000000000000"
    },
    {
      "alias": "wh1",
      "artifactId": "fd86f08a-15dd-919f-4700-fac08a32106f",
      "artifactType": "Warehouse",
      "workspaceId": "00000000-0000-0000-0000-000000000000"
    }
  ],
  "functions": [
    {
      "name": "hello_fabric",
      "description": "",
      "isPublicEndpointEnabled": true
    }
    {
      "name": "query_data_from_warehouse",
      "description": "",
      "isPublicEndpointEnabled": true
    },
    {
      "name": "read_csv_from_lakehouse",
      "description": "",
      "isPublicEndpointEnabled": true
    }
  ],
  "libraries": {
    "public": [
      {
        "name": "fabric-user-data-functions",
        "type": "PYPI",
        "version": "1.0"
      },
      {
        "name": "pandas",
        "type": "PYPI",
        "version": "2.2.3"
      }
    ],
    "private": [
      {
        "name": "my-own-custom-wheel.whl",
        "type": "WHEEL"
      }
    ]
  }
}
```

## FunctionsMetadata

| Name | Type | Description |
|------|------|-------------|
| runtime | [RunTime](#runtime-enum) | Runtime environment for execution |
| functionsMetadata | [FunctionModel](#functionmodel)[] | List of function metadata details |

### FunctionModel

| Name | Type | Description |
|------|------|-------------|
| name | String | Name of the function |
| scriptFile | String | Name of the script where the function code is located |
| bindings | [BindingModel](#bindingmodel)[] | List of bindings for the function, check BindingType enum and the corresponding Binding Model  |
| fabricProperties | [FunctionFabricProperties](#functionfabricproperties) | Fabric properties for the function |

### BindingModel

| Name | Type | Description |
|------|------|-------------|
| name | String | Name of the binding |
| direction | [BindingDirection](#bindingdirection-enum) | Direction of the binding |
| type | [BindingType](#bindingtype-enum) | Type of the binding |

### FunctionFabricProperties

| Name | Type | Description |
|------|------|-------------|
| fabricFunctionParameters | [FabricFunctionParameter](#fabricfunctionparameter)[] | List of parameters for the function |
| fabricFunctionReturnType | String | Return type of the function |
| fabricMetadataSchemaVersion | String | Schema version of the metadata. Default 1.1.0 |

### BindingDirection (enum)

| Name | Description |
|------|-------------|
| In | Input binding |

### BindingType (enum)

| Name | Description |
|------|-------------|
| [HttpTrigger](#httpbindingmodel) | HTTP trigger binding |
| [FabricItem](#fabricitembindingmodel) | Fabric item binding |
| [UserDataFunctionContext](#userdatafunctioncontextbindingmodel) | UserDataFunctionContext binding |

### FabricFunctionParameter

| Name | Type | Description |
|------|------|-------------|
| dataType | String | Data type of the function parameter. Supported data types can be found [in this document](https://learn.microsoft.com/fabric/data-engineering/user-data-functions/python-programming-model#supported-input-types) |
| name | String | Name of the function parameter |

### HttpBindingModel

| Name | Type | Description |
|------|------|-------------|
| methods | String[] | List of API supported methods |
| route | String | The route for the HTTP trigger |
| authLevel | [BindingAuthLevel](#bindingauthlevel-enum) | The authentication level for the HTTP trigger |
| name | String | Name of the binding |
| direction | [BindingDirection](#bindingdirection-enum) | Direction of the binding |
| type | [BindingType](#bindingtype-enum) | Type of the binding |

### BindingAuthLevel (enum)

| Name | Description |
|------|-------------|
| Anonymous | Anonymous authentication |

### FabricItemBindingModel

| Name | Type | Description |
|------|------|-------------|
| itemType | [ArtifactType](#artifacttype-enum) | Type of the fabric item |
| subType | String | Subtype of the fabric item |
| alias | String | Alias of the fabric item |
| audienceType | [GenericConnectionType](#genericconnectiontype-enum) | Audience of the fabric item. Only required for generic connections |
| name | String | Name of the binding |
| direction | [BindingDirection](#bindingdirection-enum) | Direction of the binding |
| type | [BindingType](#bindingtype-enum) | Type of the binding |

### ArtifactType (enum)

| Name | Description |
|------|-------------|
| SqlDbNative | SQL Database |
| DataWarehouse| Data Warehouse |
| LakeWarehouse | Lakehouse Warehouse |
| Lakehouse | Lakehouse |
| MountedWarehouse | Mounted Warehouse |
| MountedRelationalDatabase | Mounted Relational Database |
| AzureSql | Azure SQL Database |
| Warehouse | Warehouse |
| Variables | Variables Library |

#### GenericConnectionType (enum)

| Name | Description |
|------|-------------|
| CosmosDB | CosmosDB |
| KeyVault | KeyVault |

### UserDataFunctionContextBindingModel

| Name | Type | Description |
|------|------|-------------|
| name | String | Name of the binding |
| direction | [BindingDirection](#bindingdirection-enum) | Direction of the binding |
| type | [BindingType](#bindingtype-enum) | Type of the binding |

### FunctionsMetadata example

```JSON
{
  "runtime": "PYTHON",
  "functionsMetadata": [
    {
      "name": "hello_fabric",
      "scriptFile": "function_app.py",
      "bindings": [
        {
          "methods": [
            "POST"
          ],
          "route": "",
          "authLevel": "Anonymous",
          "name": "req",
          "direction": "In",
          "type": "HttpTrigger"
        }
      ],
      "fabricProperties": {
        "fabricMetadataSchemaVersion": "1.1.0",
        "fabricFunctionParameters": [
          {
            "dataType": "str",
            "name": "name"
          }
        ],
        "fabricFunctionReturnType": "str"
      }
    },
    {
      "name": "read_csv_from_lakehouse",
      "scriptFile": "function_app.py",
      "bindings": [
        {
          "methods": [
            "POST"
          ],
          "route": "",
          "authLevel": "Anonymous",
          "name": "req",
          "direction": "In",
          "type": "HttpTrigger"
        },
        {
          "itemType": null,
          "subType": "FabricLakehouseClient",
          "alias": "lh1y",
          "name": "myLakehouse",
          "direction": "In",
          "type": "FabricItem"
        }
      ],
      "fabricProperties": {
        "fabricMetadataSchemaVersion": "1.1.0",
        "fabricFunctionParameters": [
          {
            "dataType": "str",
            "name": "csvFileName"
          }
        ],
        "fabricFunctionReturnType": "str"
      }
    },
    {
      "name": "query_data_from_warehouse",
      "scriptFile": "function_app.py",
      "bindings": [
        {
          "methods": [
            "POST"
          ],
          "route": "",
          "authLevel": "Anonymous",
          "name": "req",
          "direction": "In",
          "type": "HttpTrigger"
        },
        {
          "itemType": null,
          "subType": "FabricSqlConnection",
          "alias": "wh1",
          "name": "myWarehouse",
          "direction": "In",
          "type": "FabricItem"
        }
      ],
      "fabricProperties": {
        "fabricMetadataSchemaVersion": "1.1.0",
        "fabricFunctionParameters": [],
        "fabricFunctionReturnType": "list"
      }
    }
  ]
}
```

## PythonScript

This Python file contains implementations for all functions declared in definition.json and functions.json. If you added any functions under the functions section in definition.json, you must implement them here.

### Function_app.py example

```Python
import datetime
import fabric.functions as fn
import logging

udf = fn.UserDataFunctions()

@udf.function()
def hello_fabric(name: str) -> str:
    logging.info('Python UDF trigger function processed a request.')

    return f"Welcome to Fabric Functions, {name}, at {datetime.datetime.now()}!"

import pandas as pd 
# Select 'Manage connections' and add a connection to a Lakehouse which has a CSV file
# Replace the alias "<My Lakehouse alias>" with your connection alias.
@udf.connection(argName="myLakehouse", alias="<My Lakehouse alias>")
@udf.function()
def read_csv_from_lakehouse(myLakehouse: fn.FabricLakehouseClient, csvFileName: str) -> str:
    '''
    Description: Read CSV file from lakehouse and return data as formatted string.
    
    Args:
        myLakehouse (fn.FabricLakehouseClient): Fabric lakehouse connection.
        csvFileName (str): CSV file name in the lakehouse Files folder.
    
    Returns: 
        str: Confirmation message with formatted CSV data rows.
    '''
    # Connect to the Lakehouse
    connection = myLakehouse.connectToFiles()   

    # Download the CSV file from the Lakehouse
    csvFile = connection.get_file_client(csvFileName)
    downloadFile=csvFile.download_file()
    csvData = downloadFile.readall()
    
    # Read the CSV data into a pandas DataFrame
    from io import StringIO
    df = pd.read_csv(StringIO(csvData.decode('utf-8')))

    # Display the DataFrame    
    result="" 
    for index, row in df.iterrows():
        result=result + "["+ (",".join([str(item) for item in row]))+"]"
    
    # Close the connection
    csvFile.close()
    connection.close()

    return f"CSV file read successfully.{result}"


import datetime
#Select 'Manage connections' to connect to a Warehouse
#Replace the alias "<My Warehouse Alias>" with your connection alias.
@udf.connection(argName="myWarehouse", alias="<My Warehouse Alias>")
@udf.function()
def query_data_from_warehouse(myWarehouse: fn.FabricSqlConnection) -> list:
    '''
    Description: Query employee data from a Fabric warehouse and return as JSON objects.
    
    Args:
        myWarehouse (fn.FabricSqlConnection): Fabric warehouse connection.
    
    Returns:
        list: Employee records as dictionaries with EmpName and DepID fields.
        
    Example:
        Returns [{'EmpName': 'John Smith', 'DepID': 31}, {'EmpName': 'Kayla Jones', 'DepID': 33}]
    '''
    whSqlConnection = myWarehouse.connect()
    # Use connection to execute a query
    cursor = whSqlConnection.cursor()
    cursor.execute(f"SELECT * FROM (VALUES ('John Smith',  31) , ('Kayla Jones', 33)) AS Employee(EmpName, DepID);")
    
    rows = [x for x in cursor]
    columnNames = [x[0] for x in cursor.description]
    # Turn the rows into a json object
    values = []
    for row in rows:
        item = {}
        for prop, val in zip(columnNames, row):
            if isinstance(val, (datetime.date, datetime.datetime)):
                val = val.isoformat()
            item[prop] = val
        values.append(item)
    
    cursor.close()
    whSqlConnection.close()

    return values
```
