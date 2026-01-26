  
# SQL database definition
  
This article provides a breakdown of the definition structure for SQL database items using `dacpac` format.
  
## Definition parts
  
This table lists the `dacpac` SQL database definition parts.
  
| Definition part path | type | Required | Description |
|--|--|--|--|
| `sqldb.dacpac` | Data-tier application package (dacpac) | true | The dacpac (database model) file |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |


## Data-tier application package (dacpac) part

The `dacpac` part is a file that contains the database model, which includes all the SQL database objects such as tables, views, and stored procedures. The `dacpac` file is used to deploy and manage the database schema in a declarative manner, dynamically calculating the changes needed to update the database to match the `dacpac` model.

A `dacpac` file can be created using SQL projects in Visual Studio Code, the SqlPackage command-line utility, or other database development tools that support the `dacpac` format. Learn more about SQL projects and creating `dacpac` files in the [SQL projects documentation](https://learn.microsoft.com/sql/tools/sql-database-projects/sql-database-projects) or the SQL database in Fabric article on [SqlPackage](https://learn.microsoft.com/fabric/database/sql/sqlpackage).


## Definition Example

```JSON
{
  "definition": {
    "parts": [
      {
        "path": "sqldb.dacpac",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```
