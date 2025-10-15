# Full Stack Application Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **database inspection, querying, and resource discovery** capabilities. Application code generation, resource creation, and deployment require external tools (Azure CLI, GitHub Copilot code generation, deployment tools). This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test MCP database inspection capabilities in full-stack context
- Verify resource discovery for deployed applications
- Test database querying for application backends
- Validate end-to-end workflow combining MCP and external tools

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Development tools (Node.js or Python)
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Node.js App with Cosmos DB Backend End-to-End

**Objective**: Complete workflow from database setup through querying with a deployed application

### Step 1: Create Database Resources (External - Not MCP)

> **External Setup Required**: Use Azure CLI to create Cosmos DB resources

```bash
# Create resource group
az group create --name bugbash-fullstack-rg --location eastus

# Create Cosmos DB account
az cosmosdb create \
  --name bugbash-cosmos-$RANDOM \
  --resource-group bugbash-fullstack-rg \
  --locations regionName=eastus

# Create database
az cosmosdb sql database create \
  --account-name <account-name-from-above> \
  --resource-group bugbash-fullstack-rg \
  --name TasksDB

# Create container
az cosmosdb sql container create \
  --account-name <account-name-from-above> \
  --resource-group bugbash-fullstack-rg \
  --database-name TasksDB \
  --name Tasks \
  --partition-key-path "/category" \
  --throughput 400
```

**Insert sample data** using Azure Portal Data Explorer:
```json
{"id": "1", "title": "Buy groceries", "category": "personal", "completed": false}
{"id": "2", "title": "Review PR", "category": "work", "completed": false}
{"id": "3", "title": "Call dentist", "category": "personal", "completed": true}
```

### Step 2: Discover Database Resources with Azure MCP Server

**2.1 List Cosmos DB accounts** (uses `azmcp_cosmos_account_list`):
```
List all Cosmos DB accounts in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_account_list`
- [ ] Your Cosmos DB account appears
- [ ] Account properties displayed

**2.2 List databases** (uses `azmcp_cosmos_database_list`):
```
List all databases in Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_list`
- [ ] 'TasksDB' database listed

**2.3 List containers** (uses `azmcp_cosmos_database_container_list`):
```
List all containers in database 'TasksDB' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_container_list`
- [ ] 'Tasks' container listed
- [ ] Partition key '/category' shown

### Step 3: Query Application Data with Azure MCP Server

**3.1 Query all tasks** (uses `azmcp_cosmos_database_container_item_query`):
```
Query all items from container 'Tasks' in database 'TasksDB' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_container_item_query`
- [ ] All 3 sample tasks returned
- [ ] Task properties visible (id, title, category, completed)

**3.2 Query completed tasks**:
```
Show me all completed tasks from container 'Tasks' in database 'TasksDB' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Only completed tasks returned
- [ ] Filter correctly applied

**3.3 Query by category**:
```
Show me all work tasks from container 'Tasks' in database 'TasksDB' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Only work category tasks returned
- [ ] Category filter works

### Step 4: Build and Deploy Application (External Tools - Not MCP)

> **External Development Required**: Use GitHub Copilot for code generation

**Generate application code** (use GitHub Copilot Chat in VS Code):
```
Create a Node.js Express application for a task management system with:
- Express.js web framework
- Cosmos DB integration using @azure/cosmos
- RESTful API endpoints for CRUD operations
- Simple HTML frontend
- Connection to Cosmos DB '<account-name>'
```

**Deploy to Azure** (use Azure CLI):
```bash
# Create App Service
az webapp create \
  --name bugbash-taskapp-$RANDOM \
  --resource-group bugbash-fullstack-rg \
  --plan bugbash-plan \
  --runtime "NODE:18-lts"

# Configure connection string
az webapp config appsettings set \
  --name <webapp-name> \
  --resource-group bugbash-fullstack-rg \
  --settings COSMOS_CONNECTION_STRING="<connection-string>"

# Deploy code (zip deployment)
cd task-app
zip -r app.zip .
az webapp deployment source config-zip \
  --resource-group bugbash-fullstack-rg \
  --name <webapp-name> \
  --src app.zip
```

### Step 5: Verify Deployed Application with Azure MCP Server

**5.1 Get App Service details** (uses `azmcp_appservice_get`):
```
Show me details for App Service '<webapp-name>' in resource group 'bugbash-fullstack-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_appservice_get`
- [ ] App Service properties shown
- [ ] Runtime and configuration visible
- [ ] App URL displayed

**5.2 Verify data through application**:
- Open the App Service URL in browser
- Test the task management interface
- Create/read/update/delete tasks
- Verify operations work end-to-end

### Step 6: Query Updated Data with Azure MCP Server

**6.1 Query database to verify application changes** (uses `azmcp_cosmos_database_container_item_query`):
```
Query all items from container 'Tasks' in database 'TasksDB' to see changes made through my application
```

**Verify**:
- [ ] New tasks created via app are visible
- [ ] Updated tasks show modifications
- [ ] Deleted tasks are gone
- [ ] Database accurately reflects app operations

### Step 7: Cleanup (External - Not MCP)

```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-fullstack-rg --yes --no-wait
```

**Expected Results**:
- Database discovery works correctly
- Application data query succeeds
- App Service inspection functional
- End-to-end workflow complete
- Data changes visible through MCP queries

## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Database Connection Failures** | Can't connect to database | Verify firewall rules allow your IP and Azure services |
| **Authentication Errors** | Credential issues | Check connection string format and credentials |
| **Query Syntax Errors** | SQL queries fail | Use correct syntax for database type (T-SQL vs PostgreSQL) |
| **App Service Configuration** | App can't connect to database | Verify connection string in App Service settings |
| **CORS Errors** | Frontend can't reach API | Configure CORS in App Service or API code |
| **Missing Resources** | MCP can't find resources | Ensure resources exist in specified subscription/resource group |
| **Partition Key Issues** | Cosmos DB queries fail | Include partition key or enable cross-partition queries |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Database type and resource names
- [ ] Query that was attempted
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Cosmos DB Documentation](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure PostgreSQL Documentation](https://learn.microsoft.com/azure/postgresql/)
- [Azure SQL Documentation](https://learn.microsoft.com/azure/azure-sql/)
- [MCP Command Reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## Quick Reference: Supported MCP Tools

### Cosmos DB
- `azmcp_cosmos_account_list` - List Cosmos DB accounts
- `azmcp_cosmos_database_list` - List databases
- `azmcp_cosmos_database_container_list` - List containers
- `azmcp_cosmos_database_container_item_query` - Query container items

### PostgreSQL
- `azmcp_postgres_server_list` - List PostgreSQL servers
- `azmcp_postgres_database_list` - List databases
- `azmcp_postgres_table_list` - List tables
- `azmcp_postgres_table_schema_get` - Get table schema
- `azmcp_postgres_database_query` - Execute SELECT queries

### MySQL
- `azmcp_mysql_server_list` - List MySQL servers
- `azmcp_mysql_database_list` - List databases
- `azmcp_mysql_table_list` - List tables
- `azmcp_mysql_table_schema_get` - Get table schema
- `azmcp_mysql_database_query` - Execute SELECT queries

### Azure SQL
- `azmcp_sql_server_list` - List SQL servers
- `azmcp_sql_db_list` - List databases
- `azmcp_sql_db_show` - Get database details

### App Service
- `azmcp_appservice_get` - Get App Service details

---

**Next**: [Infrastructure as Code Testing](infra-as-code.md)