=== Additional requirements for PostgreSQL:
{{VersionRules}}
- Don't create a database naming postgres, it's a built-in db.
- If you have error provisioning a PostgreSQL flexible server: 'An unexpected error while processing the request'. Check quota for Microsoft.DBforPostgreSQL/flexibleServers in the region with tool. If current region is not available, 1)choose an available region and 2)add an index to the postgresql name suffix and retry.
{{DatabaseCommonRules}}
{{ToolSpecificRules}}
