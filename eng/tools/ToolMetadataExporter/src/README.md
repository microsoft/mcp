# Tool Metadata Exporter

Extracts tool metadata from the MCP server and exports it to an Azure Data Explorer (Kusto) database for further analysis and reporting.

## Development Setup

### 1. Create Datastore

1. Create an Azure Data Explorer Cluster (e.g. "mcp-test-instance")
1. Create a database (e.g. "McpToolMetadata") within the cluster
1. In https://dataexplorer.azure.com/, connect to cluster
1. Click on "Query" tab
1. Execute [`CreateTable.kql`](./Resources/queries/CreateTable.kql) against the cluster and database

### 2. Configure Application

1. Open [`appsettings.Development.json`](./appsettings.Development.json)
1. Update "IngestionEndpoint", "QueryEndpoint", "DatabaseName" with the appropriate cluster and database names. Using the example from previous step, it would look like this:
   ```json
     "AppConfig": {
        "IngestionEndpoint": "https://ingest-mcp-test-instance.westus2.kusto.windows.net",
        "QueryEndpoint": "https://mcp-test-instance.westus2.kusto.windows.net",
        "DatabaseName": "McpToolMetadata"
    }
   ```
   To find values for "QueryEndpoint" and "IngestionEndpoint", navigate to the Azure Data Explorer Cluster in the Azure portal, and in the "Essentials" panel window, look for. Other settings in `appsettings.json` can be overridden here as needed.
    1. "IngestionEndpoint" is "Data Ingestion URI"
    1. "QueryEndpoint" is "URI"

Additional configuration settings and their documentation can be found in [AppConfiguration.cs](./src/AppConfiguration.cs).

### 3. Run Application

1. Open a terminal in the project src directory: [$RepositoryRoot/eng/tools/ToolMetadataExporter/src](./)
1. Run the application using the command:
   ```bash
   dotnet run --environment DOTNET_ENVIRONMENT=Development
   ```
   The environment variable `DOTNET_ENVIRONMENT` is set to `Development` to ensure the application uses [`appsettings.Development.json`](./appsettings.Development.json)
