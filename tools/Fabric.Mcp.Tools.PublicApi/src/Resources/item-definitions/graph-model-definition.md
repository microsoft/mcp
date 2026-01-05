# Graph Model definition

This article provides a breakdown of the definition structure for Graph Model items.

## Definition parts

This table lists the Graph Model definition parts.

| Definition part path   | type                                          | Required | Description                       |
| ---------------------- | --------------------------------------------- | -------- | --------------------------------- |
| `dataSources`          | [DataSource](#datasource)[]                   | true     | The array of data sources         |
| `graphDefinition`      | [GraphDefinition](#graphdefinition)           | true     | The data mapping graph definition |
| `graphType`            | [GraphType](#graphtype)                       | true     | The graph structure               |
| `stylingConfiguration` | [StylingConfiguration](#stylingconfiguration) | true     | The graph styling configuration   |

### DataSource

Describes the structure of the data source.

| Name       | Type                                          | Description                 |
| ---------- | --------------------------------------------- | --------------------------- |
| name       | String                                        | The name of the data source |
| type       | "DeltaTable"                                  | The data source type        |
| properties | [DataSourceProperties](#datasourceproperties) | The data source properties  |

### DataSourceProperties

The properties required by the data source type.
For DeltaTable sources, this includes the table path.

| Name | Type   | Description                 |
| ---- | ------ | --------------------------- |
| path | String | The path to the data source |

### GraphDefinition

Describes the data mapping graph definition.

| Name       | Type                      | Description                                |
| ---------- | ------------------------- | ------------------------------------------ |
| nodeTables | [NodeTable](#nodetable)[] | The array of node data mapping definitions |
| edgeTables | [EdgeTable](#edgetable)[] | The array of edge data mapping definitions |

### NodeTable

Describes the structure of node data mapping definition.

| Name             | Type                                  | Description                                    |
| ---------------- | ------------------------------------- | ---------------------------------------------- |
| id               | String                                | ID of the node data mapping definition         |
| nodeTypeAlias    | String                                | Alias of the node as defined in graph          |
| dataSourceName   | String                                | The name of the data source                    |
| propertyMappings | [PropertyMapping](#propertymapping)[] | The array of property data mapping definitions |

### EdgeTable

Describes the structure of edge data mapping definition.

| Name                 | Type                                  | Description                                      |
| -------------------- | ------------------------------------- | ------------------------------------------------ |
| id                   | String                                | ID of the edge data mapping definition           |
| edgeTypeAlias        | String                                | Alias of the edge as defined in graph            |
| dataSourceName       | String                                | The name of the data source                      |
| sourceNodeKeyColumns | String[]                              | The array of columns that map to the source node |
| targetNodeKeyColumns | String[]                              | The array of columns that map to the target node |
| propertyMappings     | [PropertyMapping](#propertymapping)[] | The array of property data mapping definitions   |

### PropertyMapping

Describes the structure of property data mapping definition.

| Name         | Type                                                        | Description                   |
| ------------ | ----------------------------------------------------------- | ----------------------------- |
| propertyName | String                                                      | The name of the property      |
| sourceColumn | String                                                      | The name of the source column |
| filter       | [SingleFilter](#singlefilter) / [GroupFilter](#groupfilter) | The filter definition         |

### SingleFilter

Describes the structure of a single filter.

| Name       | Type   | Description                                                                          |
| ---------- | ------ | ------------------------------------------------------------------------------------ |
| operator   | String | The operator name of the filter                                                      |
| columnName | String | The column name for this filter                                                      |
| value      | Object | The comparison value. Supports string, number, dateTime, and arrays of these values. |

### GroupFilter

Describes the structure of a group filter.

| Name     | Type                                                            | Description                      |
| -------- | --------------------------------------------------------------- | -------------------------------- |
| operator | String                                                          | The operator name of the filter  |
| filters  | [SingleFilter](#singlefilter)[] / [GroupFilter](#groupfilter)[] | The filters of this group filter |
| and      | [SingleFilter](#singlefilter)[] / [GroupFilter](#groupfilter)[] | The filters for logical AND      |
| or       | [SingleFilter](#singlefilter)[] / [GroupFilter](#groupfilter)[] | The filters for logical OR       |

### GraphType

Describes the structure of a graph.

| Name      | Type                    | Description                  |
| --------- | ----------------------- | ---------------------------- |
| nodeTypes | [NodeType](#nodetype)[] | The array of node structures |
| edgeTypes | [EdgeType](#edgetype)[] | The array of edge structures |

### NodeType

Describes the structure of a node.

| Name                 | Type                    | Description                         |
| -------------------- | ----------------------- | ----------------------------------- |
| alias                | String                  | The alias                           |
| labels               | String[]                | The array of labels                 |
| primaryKeyProperties | String[]                | The array of primary key properties |
| properties           | [Property](#property)[] | The array of properties             |

### EdgeType

Describes the structure of an edge.

| Name                | Type                                    | Description                    |
| ------------------- | --------------------------------------- | ------------------------------ |
| alias               | String                                  | The alias                      |
| labels              | String[]                                | The array of labels            |
| sourceNodeType      | [NodeTypeReference](#nodetypereference) | The source node structure      |
| destinationNodeType | [NodeTypeReference](#nodetypereference) | The destination node structure |
| properties          | [Property](#property)[]                 | The array of properties        |

### Property

Describes the structure of a property.

| Name | Type   | Description              |
| ---- | ------ | ------------------------ |
| name | String | The name of the property |
| type | String | The type of the property |

### NodeTypeReference

Describes the structure of a NodeTypeReference.

| Name  | Type   | Description                      |
| ----- | ------ | -------------------------------- |
| alias | String | The alias of the referenced node |

### StylingConfiguration

Describes the structure of the styling configuration.

| Name        | Type                        | Description                                              |
| ----------- | --------------------------- | -------------------------------------------------------- |
| modelLayout | [ModelLayout](#modellayout) | The styling and layout configuration for the graph model |

### ModelLayout

Describes the model styles configuration.

| Name      | Type                                          | Description                    |
| --------- | --------------------------------------------- | ------------------------------ |
| positions | Dictionary<string, [Position](#position)>     | The positions of nodes         |
| styles    | Dictionary<string, [ModelStyle](#modelstyle)> | The styles of graph model      |
| pan       | [Position](#position)                         | The pan of model canvas        |
| zoomLevel | Integer                                       | The zoom level of model canvas |

### Position

Describes a 2D position.

| Name | Type    | Description            |
| ---- | ------- | ---------------------- |
| x    | Integer | The x coordinate value |
| y    | Integer | The y coordinate value |

### ModelStyle

Describes the style of a model element.

| Name | Type    | Description                 |
| ---- | ------- | --------------------------- |
| size | Integer | The size of a model element |

### Data Sources Example

```json
{
  "dataSources": [
    {
      "name": "Customer_Table",
      "type": "DeltaTable",
      "properties": {
        "path": "abfss://9e4b0e5d-3952-44df-9ac8-2503775e0425@onelake.dfs.fabric.microsoft.com/f66b6219-28a5-4738-8b37-0c486c661b15/Tables/Customers"
      }
    },
    {
      "name": "Employee_Table",
      "type": "DeltaTable",
      "properties": {
        "path": "abfss://9e4b0e5d-3952-44df-9ac8-2503775e0425@onelake.dfs.fabric.microsoft.com/f66b6219-28a5-4738-8b37-0c486c661b15/Tables/Employees"
      }
    },
    {
      "name": "Order_Table",
      "type": "DeltaTable",
      "properties": {
        "path": "abfss://9e4b0e5d-3952-44df-9ac8-2503775e0425@onelake.dfs.fabric.microsoft.com/f66b6219-28a5-4738-8b37-0c486c661b15/Tables/PurchaseOrders"
      }
    },
    {
      "name": "Product_Table",
      "type": "DeltaTable",
      "properties": {
        "path": "abfss://9e4b0e5d-3952-44df-9ac8-2503775e0425@onelake.dfs.fabric.microsoft.com/f66b6219-28a5-4738-8b37-0c486c661b15/Tables/Products"
      }
    }
  ]
}
```

### Graph Definition Example

```json
{
  "schemaVersion": "1.0.0",
  "nodeTables": [
    {
      "id": "Customer_5b6cb156-c778-4fce-8606-f0f712c04818",
      "nodeTypeAlias": "Customer_nodeType",
      "dataSourceName": "Customer_Table",
      "propertyMappings": [
        {
          "propertyName": "CustomerId",
          "sourceColumn": "Customer_Id"
        },
        {
          "propertyName": "FirstName",
          "sourceColumn": "First_name"
        },
        {
          "propertyName": "LastName",
          "sourceColumn": "Last_name"
        },
        {
          "propertyName": "Email",
          "sourceColumn": "Email"
        }
      ],
      "filter": {
        "and": [
          {
            "operator": "Contains",
            "columnName": "First_name",
            "value": "USA"
          },
          {
            "operator": "Contains",
            "columnName": "Last_name",
            "value": "A"
          }
        ]
      }
    },
    {
      "id": "Employee_f0f712c04818",
      "nodeTypeAlias": "Employee_nodeType",
      "dataSourceName": "Employee_Table",
      "propertyMappings": [
        {
          "propertyName": "EmployeeId",
          "sourceColumn": "Employee_Id"
        },
        {
          "propertyName": "FirstName",
          "sourceColumn": "First_Name"
        },
        {
          "propertyName": "LastName",
          "sourceColumn": "Last_Name"
        },
        {
          "propertyName": "Role",
          "sourceColumn": "Role"
        }
      ],
      "filter": {
        "operator": "AND",
        "filters": [
          {
            "operator": "Contains",
            "columnName": "First_name",
            "value": "USA"
          },
          {
            "operator": "Contains",
            "columnName": "Last_name",
            "value": "A"
          }
        ]
      }
    },
    {
      "id": "EmployeeCustomer_5b6cb156",
      "nodeTypeAlias": "EmployeeCustomer_nodeType",
      "dataSourceName": "Employee_Table",
      "propertyMappings": [
        {
          "propertyName": "EmployeeId",
          "sourceColumn": "Employee_Id"
        },
        {
          "propertyName": "FirstName",
          "sourceColumn": "First_Name"
        },
        {
          "propertyName": "LastName",
          "sourceColumn": "Last_Name"
        },
        {
          "propertyName": "Role",
          "sourceColumn": "Role"
        },
        {
          "propertyName": "Email",
          "sourceColumn": "Email"
        }
      ],
      "filter": {
        "operator": "Equal",
        "columnName": "Role",
        "value": "Manager"
      }
    },
    {
      "id": "Product_5b6cb156-c778-4fce-8606-f0f712c04818",
      "nodeTypeAlias": "Product_nodeType",
      "dataSourceName": "Product_Table",
      "propertyMappings": [
        {
          "propertyName": "CategoryId",
          "sourceColumn": "Category_Id"
        },
        {
          "propertyName": "ProductId",
          "sourceColumn": "Product_Id"
        },
        {
          "propertyName": "Name",
          "sourceColumn": "Name"
        },
        {
          "propertyName": "Price",
          "sourceColumn": "Price"
        },
        {
          "propertyName": "Cost",
          "sourceColumn": "Cost"
        }
      ],
      "filter": {
        "operator": "AND",
        "filters": [
          {
            "operator": "GreaterThan",
            "columnName": "Price",
            "value": 100
          },
          {
            "operator": "In",
            "columnName": "CategoryId",
            "value": ["Electronics", "Clothing", "Books"]
          }
        ]
      }
    }
  ],
  "edgeTables": [
    {
      "id": "CustomerPurchase_976cceac",
      "edgeTypeAlias": "CustomerPurchase_edgeType",
      "dataSourceName": "Order_Table",
      "sourceNodeKeyColumns": ["Customer_Id_FK"],
      "destinationNodeKeyColumns": ["Category_Id_FK", "Product_Id_FK"],
      "propertyMappings": [
        {
          "propertyName": "Quantity",
          "sourceColumn": "unit_price"
        },
        {
          "propertyName": "Date",
          "sourceColumn": "Date"
        }
      ],
      "filter": {
        "and": [
          {
            "operator": "Contains",
            "columnName": "Customer_Id_FK",
            "value": "USA"
          },
          {
            "operator": "Contains",
            "columnName": "Customer_Id_FK",
            "value": "A"
          }
        ]
      }
    },
    {
      "id": "EmployeePurchase_29be49f2",
      "edgeTypeAlias": "EmployeePurchase_edgeType",
      "dataSourceName": "Order_Table",
      "sourceNodeKeyColumns": ["Employee_Id_FK"],
      "destinationNodeKeyColumns": ["Category_Id_FK", "Product_Id_FK"],
      "propertyMappings": [
        {
          "propertyName": "Quantity",
          "sourceColumn": "unit_price"
        },
        {
          "propertyName": "Date",
          "sourceColumn": "Date"
        }
      ]
    },
    {
      "id": "EmployeeSold_2530ecef620c",
      "edgeTypeAlias": "Sold_edgeType",
      "dataSourceName": "Order_Table",
      "sourceNodeKeyColumns": ["Employee_Id_FK"],
      "destinationNodeKeyColumns": ["Category_Id_FK", "Product_Id_FK"],
      "propertyMappings": [
        {
          "propertyName": "CustomerId",
          "sourceColumn": "unit_price"
        },
        {
          "propertyName": "Date",
          "sourceColumn": "Date"
        },
        {
          "propertyName": "hasDiscount",
          "sourceColumn": "EmployeeDiscount"
        }
      ]
    }
  ]
}
```

### Graph type Example

```json
{
  "schemaVersion": "1.0.0",
  "nodeTypes": [
    {
      "alias": "Customer_nodeType",
      "labels": ["Customer"],
      "primaryKeyProperties": ["CustomerId"],
      "properties": [
        {
          "name": "CustomerId",
          "type": "STRING"
        },
        {
          "name": "FirstName",
          "type": "STRING"
        },
        {
          "name": "LastName",
          "type": "STRING"
        },
        {
          "name": "Email",
          "type": "STRING"
        }
      ]
    },
    {
      "alias": "Employee_nodeType",
      "labels": ["Employee"],
      "primaryKeyProperties": ["EmployeeId"],
      "properties": [
        {
          "name": "EmployeeId",
          "type": "STRING"
        },
        {
          "name": "FirstName",
          "type": "STRING"
        },
        {
          "name": "LastName",
          "type": "STRING"
        },
        {
          "name": "Role",
          "type": "STRING"
        },
        {
          "name": "Email",
          "type": "STRING"
        }
      ]
    },
    {
      "alias": "EmployeeCustomer_nodeType",
      "labels": ["Customer", "Employee"],
      "primaryKeyProperties": ["EmployeeId"],
      "properties": [
        {
          "name": "EmployeeId",
          "type": "STRING"
        },
        {
          "name": "FirstName",
          "type": "STRING"
        },
        {
          "name": "LastName",
          "type": "STRING"
        },
        {
          "name": "Role",
          "type": "STRING"
        },
        {
          "name": "Email",
          "type": "STRING"
        }
      ]
    },
    {
      "alias": "Product_nodeType",
      "labels": ["Product"],
      "primaryKeyProperties": ["CategoryId", "ProductId"],
      "properties": [
        {
          "name": "CategoryId",
          "type": "INT"
        },
        {
          "name": "ProductId",
          "type": "STRING"
        },
        {
          "name": "Name",
          "type": "STRING"
        },
        {
          "name": "Price",
          "type": "FLOAT"
        },
        {
          "name": "Cost",
          "type": "FLOAT"
        }
      ]
    }
  ],
  "edgeTypes": [
    {
      "alias": "CustomerPurchase_edgeType",
      "labels": ["PURCHASED"],
      "sourceNodeType": {
        "alias": "Customer_nodeType"
      },
      "destinationNodeType": {
        "alias": "Product_nodeType"
      },
      "properties": [
        {
          "name": "Quantity",
          "type": "INT"
        },
        {
          "name": "Date",
          "type": "DATETIME"
        }
      ]
    },
    {
      "alias": "EmployeePurchase_edgeType",
      "labels": ["PURCHASED"],
      "sourceNodeType": {
        "alias": "EmployeeCustomer_nodeType"
      },
      "destinationNodeType": {
        "alias": "Product_nodeType"
      },
      "properties": [
        {
          "name": "Quantity",
          "type": "INT"
        },
        {
          "name": "Date",
          "type": "DATETIME"
        }
      ]
    },
    {
      "alias": "Sold_edgeType",
      "labels": ["Sold"],
      "sourceNodeType": {
        "alias": "Employee_nodeType"
      },
      "destinationNodeType": {
        "alias": "Product_nodeType"
      },
      "properties": [
        {
          "name": "CustomerId",
          "type": "STRING"
        },
        {
          "name": "Date",
          "type": "DATETIME"
        },
        {
          "name": "hasDiscount",
          "type": "BOOLEAN"
        }
      ]
    }
  ]
}
```

### Styling Configuration Example

```json
{
  "schemaVersion": "1.0.0",
  "modelLayout": {
    "positions": {
      "Customer_nodeType": {
        "x": 1,
        "y": 1
      },
      "Employee_nodeType": {
        "x": 2,
        "y": 3
      },
      "EmployeeCustomer_nodeType": {
        "x": 3,
        "y": 4
      },
      "Product_nodeType": {
        "x": 6,
        "y": 7
      }
    },
    "styles": {
      "Customer_nodeType": {
        "size": 30
      },
      "Employee_nodeType": {
        "size": 30
      },
      "EmployeeCustomer_nodeType": {
        "size": 30
      },
      "Product_nodeType": {
        "size": 30
      },
      "CustomerPurchase_edgeType": {
        "size": 30
      },
      "EmployeePurchase_edgeType": {
        "size": 30
      },
      "Sold_edgeType": {
        "size": 30
      }
    },
    "pan": {
      "x": 0,
      "y": 0
    },
    "zoomLevel": 1
  }
}
```
