
# Database Operations Testing Scenario

> **MCP Tool Support Notice**
> Azure MCP Server provides **read-only database inspection and querying** capabilities. Database creation, data insertion, and schema modifications require Azure CLI or Portal. This scenario guides you through complete end-to-end workflows, clearly marking when to use MCP tools vs external tools.

## Objectives

- Test end-to-end Cosmos DB inspection and querying workflows
- Test end-to-end PostgreSQL/MySQL database operations and querying
- Verify MCP tool accuracy for listing, inspecting, and querying databases
- Validate query execution and schema inspection across database types

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

---

## Scenario 1: Cosmos DB End-to-End Workflow

**Objective**: Complete workflow from resource setup through querying with Azure MCP Server

### Step 1: Setup Resources (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-cosmosdb-rg' in eastus, create a Cosmos DB account, create a database named 'ProductCatalog', and create a container named 'Products' with partition key '/category'
```

Then use Azure Portal Data Explorer to insert sample data:
```json
{"id": "1", "name": "Laptop", "category": "Electronics", "price": 1299.99}
{"id": "2", "name": "Mouse", "category": "Electronics", "price": 29.99}
{"id": "3", "name": "Desk", "category": "Furniture", "price": 499.99}
```

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-cosmosdb-rg --location eastus

# Create Cosmos DB account (this takes 5-10 minutes)
az cosmosdb create \
  --name bugbash-cosmos-$RANDOM \
  --resource-group bugbash-cosmosdb-rg \
  --locations regionName=eastus

# Create database
az cosmosdb sql database create \
  --account-name <account-name-from-above> \
  --resource-group bugbash-cosmosdb-rg \
  --name ProductCatalog

# Create container with partition key
az cosmosdb sql container create \
  --account-name <account-name-from-above> \
  --resource-group bugbash-cosmosdb-rg \
  --database-name ProductCatalog \
  --name Products \
  --partition-key-path "/category"
```

**Insert sample data** using Azure Portal Data Explorer:
```json
{"id": "1", "name": "Laptop", "category": "Electronics", "price": 1299.99}
{"id": "2", "name": "Mouse", "category": "Electronics", "price": 29.99}
{"id": "3", "name": "Desk", "category": "Furniture", "price": 499.99}
```

### Step 2: Discovery with Azure MCP Server

**2.1 List all Cosmos DB accounts** (uses `azmcp_cosmos_account_list`):
```
List all Cosmos DB accounts in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_account_list`
- [ ] Your newly created account appears in the list
- [ ] Account properties shown (name, location, capabilities)

**2.2 Alternative phrasing**:
```
Show me my Cosmos DB accounts
```

### Step 3: Database Inspection with Azure MCP Server

**3.1 List databases** (uses `azmcp_cosmos_database_list`):
```
List all databases in Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_list`
- [ ] 'ProductCatalog' database is listed
- [ ] Database properties displayed

**3.2 List containers** (uses `azmcp_cosmos_database_container_list`):
```
List all containers in database 'ProductCatalog' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_container_list`
- [ ] 'Products' container is listed
- [ ] Partition key path '/category' is shown

### Step 4: Query Data with Azure MCP Server

**4.1 Query all items** (uses `azmcp_cosmos_database_container_item_query`):
```
Query all items from container 'Products' in database 'ProductCatalog' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Tool invoked: `azmcp_cosmos_database_container_item_query`
- [ ] All 3 sample items returned
- [ ] Item properties correctly displayed

**4.2 Query with filter**:
```
Show me items in the Products container where category is 'Electronics' in database 'ProductCatalog' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Query executed successfully
- [ ] Only 2 items returned (Laptop and Mouse)
- [ ] Filter correctly applied

**4.3 Search query**:
```
Show me items that contain the word 'Laptop' in container 'Products' in database 'ProductCatalog' for Cosmos DB account '<account-name>'
```

**Verify**:
- [ ] Text search works correctly
- [ ] Laptop item returned
- [ ] Query syntax accepted

### Step 5: Cleanup (External - Not MCP)

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-cosmosdb-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-cosmosdb-rg --yes --no-wait
```

**Expected Results**:
- All MCP listing tools work correctly
- Database and container inspection successful
- Query operations return accurate results
- Different query patterns supported

---

## Scenario 2: PostgreSQL End-to-End Workflow

**Objective**: Complete workflow for PostgreSQL server inspection, schema viewing, and querying

### Step 1: Setup Resources (External - Not MCP)

> **External Setup Required**: Azure MCP Server cannot create resources. Use GitHub Copilot Chat to run Azure CLI commands or use Azure Portal.

**Option A: Prompt GitHub Copilot Chat** (Recommended):
```
Create an Azure resource group 'bugbash-postgres-rg' in eastus, create a PostgreSQL flexible server with admin user 'dbadmin', create a database named 'inventory', then help me create a products table with columns: id, name, description, price, stock_quantity, created_at
```

Then use Azure Portal Query Editor or GitHub Copilot to insert sample data.

**Option B: Run Azure CLI Commands Manually**:
```bash
# Create resource group
az group create --name bugbash-postgres-rg --location eastus

# Create PostgreSQL flexible server (takes 5-10 minutes)
az postgres flexible-server create \
  --name bugbash-postgres-$RANDOM \
  --resource-group bugbash-postgres-rg \
  --location eastus \
  --admin-user dbadmin \
  --admin-password <secure-password> \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 14 \
  --storage-size 32 \
  --public-access 0.0.0.0

# Create database
az postgres flexible-server db create \
  --resource-group bugbash-postgres-rg \
  --server-name <server-name-from-above> \
  --database-name inventory

# Connect and create table (use Azure Portal Query Editor or psql)
# CREATE TABLE products (
#   id SERIAL PRIMARY KEY,
#   name VARCHAR(100) NOT NULL,
#   description TEXT,
#   price DECIMAL(10,2),
#   stock_quantity INTEGER DEFAULT 0,
#   created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
# );

# Insert sample data
# INSERT INTO products (name, description, price, stock_quantity) VALUES
# ('Laptop', 'High-performance laptop', 1299.99, 25),
# ('Mouse', 'Wireless mouse', 29.99, 100),
# ('Keyboard', 'Mechanical keyboard', 89.99, 50);
```

### Step 2: Server Discovery with Azure MCP Server

**2.1 List all PostgreSQL servers** (uses `azmcp_postgres_server_list`):
```
List all PostgreSQL servers in my subscription
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_server_list`
- [ ] Your newly created server appears
- [ ] Server details shown (name, location, version, SKU)

**2.2 Get server configuration** (uses `azmcp_postgres_server_config_get`):
```
Show me the configuration of PostgreSQL server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_server_config_get`
- [ ] Configuration parameters displayed
- [ ] Server settings accessible

### Step 3: Database and Schema Inspection with Azure MCP Server

**3.1 List databases** (uses `azmcp_postgres_database_list`):
```
List all databases in PostgreSQL server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_database_list`
- [ ] 'inventory' database is listed
- [ ] System databases also shown (postgres, template0, template1)

**3.2 List tables** (uses `azmcp_postgres_table_list`):
```
List all tables in PostgreSQL database 'inventory' on server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_table_list`
- [ ] 'products' table is listed
- [ ] Table names correctly displayed

**3.3 Get table schema** (uses `azmcp_postgres_table_schema_get`):
```
Show me the schema of table 'products' in database 'inventory' on server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_table_schema_get`
- [ ] All columns listed (id, name, description, price, stock_quantity, created_at)
- [ ] Data types correctly shown
- [ ] Primary key and constraints visible

### Step 4: Query Data with Azure MCP Server

**4.1 Query all data** (uses `azmcp_postgres_database_query`):
```
Show me all products in the 'products' table in database 'inventory' on server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_database_query`
- [ ] All 3 sample records returned
- [ ] Column values correctly displayed

**4.2 Query with filter**:
```
Show me all products with price less than 100 in the 'products' table in database 'inventory' on server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Query executed successfully
- [ ] Only 2 items returned (Mouse and Keyboard)
- [ ] WHERE clause correctly applied

**4.3 Search query**:
```
Show me all items that contain the word 'laptop' in the 'products' table in PostgreSQL database 'inventory' on server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Text search works correctly
- [ ] Laptop item returned
- [ ] Case-insensitive search works

**4.4 Check server parameters** (uses `azmcp_postgres_server_param_get`):
```
Show me the max_connections parameter for PostgreSQL server '<server-name>' in resource group 'bugbash-postgres-rg'
```

**Verify**:
- [ ] Tool invoked: `azmcp_postgres_server_param_get`
- [ ] Parameter value displayed
- [ ] Parameter metadata shown

### Step 5: Cleanup (External - Not MCP)

**Option A: Prompt GitHub Copilot Chat**:
```
Delete the Azure resource group 'bugbash-postgres-rg' and all its resources
```

**Option B: Run Azure CLI Command Manually**:
```bash
# Delete resource group (removes all resources)
az group delete --name bugbash-postgres-rg --yes --no-wait
```

**Expected Results**:
- Server listing and discovery works
- Database and table inspection successful
- Schema information accurately retrieved
- Query operations return correct results
- Server configuration accessible

## Common Issues to Watch For

| Issue | Description | Resolution |
|-------|-------------|------------|
| **Authentication Failures** | MCP tool can't connect to database | Verify `az login` is active and you have RBAC permissions |
| **Firewall Blocking** | Access denied errors | Add your IP to firewall rules using Azure CLI/Portal |
| **Partition Key Issues** | Cosmos DB query fails | Ensure partition key is included in queries or specify cross-partition |
| **Query Syntax Differences** | SQL queries fail | Use PostgreSQL syntax for Postgres, T-SQL for Azure SQL |
| **Resource Limits** | Timeout or throttling | Check RU limits (Cosmos), connection limits (SQL/PostgreSQL) |
| **Case Sensitivity** | Query returns no results | PostgreSQL is case-sensitive; use proper casing or ILIKE |
| **Missing Resources** | MCP tool shows empty lists | Verify resources exist and are in the correct subscription |

## What to Report

When logging issues, include:
- [ ] Exact prompt used
- [ ] Tool invoked (from MCP tool output)
- [ ] Expected vs actual results
- [ ] Error messages (if any)
- [ ] Database type and version
- [ ] Screenshots of unexpected behavior

## Related Resources

- [Azure Cosmos DB Documentation](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/)
- [Azure Database for MySQL](https://learn.microsoft.com/azure/mysql/)
- [Azure SQL Database](https://learn.microsoft.com/azure/azure-sql/)
- [MCP Command Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [E2E Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

## 💡 Quick Reference: Supported MCP Tools

### Cosmos DB
- `azmcp_cosmos_account_list` - List accounts
- `azmcp_cosmos_database_list` - List databases
- `azmcp_cosmos_database_container_list` - List containers
- `azmcp_cosmos_database_container_item_query` - Query items

### PostgreSQL
- `azmcp_postgres_server_list` - List servers
- `azmcp_postgres_server_config_get` - Get configuration
- `azmcp_postgres_server_param_get` - Get specific parameter
- `azmcp_postgres_database_list` - List databases
- `azmcp_postgres_table_list` - List tables
- `azmcp_postgres_table_schema_get` - Get table schema
- `azmcp_postgres_database_query` - Execute SELECT queries

### MySQL
- `azmcp_mysql_server_list` - List servers
- `azmcp_mysql_server_config_get` - Get configuration
- `azmcp_mysql_server_param_get` - Get specific parameter
- `azmcp_mysql_database_list` - List databases
- `azmcp_mysql_table_list` - List tables
- `azmcp_mysql_table_schema_get` - Get table schema
- `azmcp_mysql_database_query` - Execute SELECT queries

### Azure SQL
- `azmcp_sql_server_list` - List servers
- `azmcp_sql_server_show` - Get server details
- `azmcp_sql_db_list` - List databases
- `azmcp_sql_db_show` - Get database details
- `azmcp_sql_elastic-pool_list` - List elastic pools
- `azmcp_sql_server_firewall-rule_list` - List firewall rules
- `azmcp_sql_server_entra-admin_list` - List Entra admins

---

**Next**: [Deployment Scenarios Testing](https://github.com/microsoft/mcp/tree/main/docs/bug-bash/scenarios/deployment.md)
