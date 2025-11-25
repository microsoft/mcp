---
title: SQL database definition
description: Learn how to create a SQL database item definition when using the Microsoft Fabric REST API.
ms.title: SQL database item definition
author: yadongyaly
ms.author: yaly
ms.reviewer: drskwier
ms.service: fabric
ms.date: 10/07/2025
---
  
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
        "payload": "ew0KICAiJHNjaGVtYSI6ICJodHRwczovL2RldmVsb3Blci5taWNyb3NvZnQuY29tL2pzb24tc2NoZW1hcy9mYWJyaWMvaXRlbS9tYXAvZGVmaW5pdGlvbi8xLjAuMC9zY2hlbWEuanNvbiIsDQogICJiYXNlbWFwIjogew0KICAgICJvcHRpb25zIjogbnVsbCwNCiAgICAiY29udHJvbHMiOiBudWxsLA0KICAgICJiYWNrZ3JvdW5kQ29sb3IiOiBudWxsLA0KICAgICJ0aGVtZSI6IG51bGwNCiAgfSwNCiAgImRhdGFTb3VyY2VzIjogew0KICAgICJsYWtlaG91c2VzIjogW10sDQogICAgImtxbERhdGFCYXNlcyI6IFtdDQogIH0sDQogICJsYXllclNvdXJjZXMiOiBbXSwNCiAgImxheWVyU2V0dGluZ3MiOiBbXQ0KfQ==",
        "payloadType": "InlineBase64"
      },
      {
        "path": ".platform",
        "payload": "ewogICIkc2NoZW1hIjogImh0dHBzOi8vZGV2ZWxvcGVyLm1pY3Jvc29mdC5jb20vanNvbi1zY2hlbWFzL2ZhYnJpYy9naXRJbnRlZ3JhdGlvbi9wbGF0Zm9ybVByb3BlcnRpZXMvMi4wLjAvc2NoZW1hLmpzb24iLAogICJtZXRhZGF0YSI6IHsKICAgICJ0eXBlIjogIk1hcCIsCiAgICAiZGlzcGxheU5hbWUiOiAiTWFwXzEyMzQ1NjciLAogICAgImRlc2NyaXB0aW9uIjogImRlc2NyIgogIH0sCiAgImNvbmZpZyI6IHsKICAgICJ2ZXJzaW9uIjogIjIuMCIsCiAgICAibG9naWNhbElkIjogIjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIKICB9Cn0=",
        "payloadType": "InlineBase64"
      }
    ]
  }
}
```
