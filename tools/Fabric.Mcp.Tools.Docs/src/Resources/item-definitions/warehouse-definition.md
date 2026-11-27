# Warehouse definition

This article provides a breakdown of the definition structure for Warehouse items.

## Supported formats

Warehouse items support `dacpac` and `sqlproj` formats. If no format is specified, operations will default to `sqlproj`.

## Data-tier application package (dacpac) part

The `dacpac` part is a file that contains the database model, which includes all the SQL objects such as tables, views, and stored procedures. The `dacpac` file is used to deploy and manage the database schema in a declarative manner, dynamically calculating the changes needed to update the database to match the `dacpac` model.

A `dacpac` file can be created using SQL projects in Visual Studio Code, the SqlPackage command-line utility, or other database development tools that support the `dacpac` format. Learn more about SQL projects and creating `dacpac` files in the [SQL projects documentation](https://learn.microsoft.com/sql/tools/sql-database-projects/sql-database-projects).

### Definition parts for `dacpac` format

This table lists the `dacpac` Warehouse definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `sqldw.dacpac` | Data-tier application package (dacpac) | true | The dacpac (database model) file |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

### Definition payload example using `dacpac` format

```JSON
{
  "definition": {
    "format": "dacpac",
    "parts": [
      {
        "path": "sqldw.dacpac",
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

## SQL database project (sqlproj) part

A SQL database project is a local representation of SQL objects that comprise the schema for a single database, such as tables, stored procedures, or functions. A SQL database project is typically tested locally by building it into a `dacpac` file to check for errors before deploying it to a target Warehouse. The structure of a SQL database project is dependent on the database objects, where Warehouse in Fabric defaults to organizing the project files by schema and object type. For example, a table named `Table1` in the `dbo` schema would be represented by a file path of `dbo/Tables/Table1.sql`. Learn more about SQL projects in the [SQL projects documentation](https://learn.microsoft.com/sql/tools/sql-database-projects/sql-database-projects).

### Definition parts for `sqlproj` format

This table lists the `sqlproj` Warehouse definition parts.

| Definition part path | type | Required | Description |
|--|--|--|--|
| `sqldw.sqlproj` | SQL database project | true | The `.sqlproj` is the project file for a Microsoft.Build.Sql project. It will be built to a `dacpac` file and deployed to the target Warehouse. Only one SQL project file (`.sqlproj`) is accepted per definition. |
| `**/*.sql` | Structured Query Language (SQL) file | false | One or more SQL object definition files. All files not explicitly excluded by the `.sqlproj` file are included. |
| `.sharedqueries/*.sql` | Structured Query Language (SQL) file | false | One or more SQL query files. These queries are deployed to the database as shared queries and are available in the editor. The `.sharedqueries` folder is excluded from the SQL project file and these queries are not included in the build process. |
| `.platform` | PlatformDetails (JSON) | false | Describes common details of the item |

### Definition payload example using `sqlproj` format

```JSON
{
  "definition": {
    "parts": [
      {
        "path": "sqldw.sqlproj",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": "dbo/Tables/Table1.sql",
        "payload": "<base64 encoded string>",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".sharedqueries/query.sql",
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