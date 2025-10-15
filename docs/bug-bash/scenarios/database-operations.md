# Database Operations Testing Scenario

Test Azure MCP Server's database management capabilities across Cosmos DB, PostgreSQL, and Azure SQL.

## Objectives

- Create and manage Cosmos DB accounts and containers
- Work with PostgreSQL flexible servers
- Manage Azure SQL databases
- Test query execution and data operations
- Verify connection string management
- Test performance and scaling

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] GitHub Copilot with Agent mode enabled

## Test Scenarios

### Scenario 1: Cosmos DB Operations

**Objective**: Test Cosmos DB account, database, and container management

#### Phase 1: Create Cosmos DB Resources

1. **Create Cosmos DB account**:
   ```
   Create a Cosmos DB account named 'bugbash-cosmos-<random>' in 
   resource group '<your-rg>' in East US with SQL API
   ```

2. **Verify account creation**:
   ```
   List all Cosmos DB accounts in my subscription
   ```

3. **Check account details**:
   ```
   Show me the details of Cosmos DB account 'bugbash-cosmos-<random>'
   ```

**Verify**:
- [ ] Account created successfully
- [ ] API type is SQL (Core)
- [ ] Location is correct
- [ ] Account is accessible

#### Phase 2: Database and Container Management

4. **Create database**:
   ```
   Create a database named 'ProductCatalog' in Cosmos DB account 
   'bugbash-cosmos-<random>'
   ```

5. **List databases**:
   ```
   List all databases in Cosmos DB account 'bugbash-cosmos-<random>'
   ```

6. **Create container**:
   ```
   Create a container named 'Products' in database 'ProductCatalog' 
   with partition key '/category' and 400 RU/s throughput
   ```

7. **List containers**:
   ```
   List all containers in database 'ProductCatalog'
   ```

**Verify**:
- [ ] Database created successfully
- [ ] Container created with correct partition key
- [ ] Throughput set correctly
- [ ] Resources are listed properly

#### Phase 3: Data Operations

8. **Insert sample data**:
   ```
   Add the following items to the Products container in Cosmos DB 
   'bugbash-cosmos-<random>', database 'ProductCatalog':
   
   Item 1:
   {
     "id": "1",
     "name": "Laptop",
     "category": "Electronics",
     "price": 999.99,
     "stock": 50
   }
   
   Item 2:
   {
     "id": "2",
     "name": "Desk Chair",
     "category": "Furniture",
     "price": 199.99,
     "stock": 100
   }
   ```

9. **Query data**:
   ```
   Query all items from the Products container where category is 'Electronics'
   ```

10. **Query with search**:
    ```
    Show me all items that contain the word 'Laptop' in the Products 
    container in database 'ProductCatalog'
    ```

**Verify**:
- [ ] Data inserted successfully
- [ ] Queries return correct results
- [ ] Partition key filtering works
- [ ] Search functionality works

**Expected Results**:
- Cosmos DB account created
- Database and container created
- Data operations work correctly
- Queries return expected results

---

### Scenario 2: PostgreSQL Operations

**Objective**: Test Azure Database for PostgreSQL flexible server management

#### Phase 1: Create PostgreSQL Server

1. **Create PostgreSQL server**:
   ```
   Create an Azure Database for PostgreSQL flexible server:
   - Name: 'bugbash-postgres-<random>'
   - Resource group: '<your-rg>'
   - Admin username: 'dbadmin'
   - Password: '<generate-secure-password>'
   - Version: PostgreSQL 14
   - Tier: Burstable B1ms
   - Storage: 32 GB
   - Region: East US
   - Allow Azure services and resources to access this server
   ```

2. **List PostgreSQL servers**:
   ```
   List all PostgreSQL servers in my subscription
   ```

3. **Get server details**:
   ```
   Show me the details of PostgreSQL server 'bugbash-postgres-<random>'
   ```

**Verify**:
- [ ] Server created successfully
- [ ] Version is PostgreSQL 14
- [ ] Tier and storage correct
- [ ] Firewall allows Azure services

#### Phase 2: Database and Table Operations

4. **Create database**:
   ```
   Create a database named 'inventory' on PostgreSQL server 
   'bugbash-postgres-<random>'
   ```

5. **List databases**:
   ```
   List all databases in PostgreSQL server 'bugbash-postgres-<random>'
   ```

6. **Create tables**:
   ```
   In PostgreSQL database 'inventory', create a table named 'products' with:
   - id: SERIAL PRIMARY KEY
   - name: VARCHAR(100) NOT NULL
   - description: TEXT
   - price: DECIMAL(10,2)
   - stock_quantity: INTEGER DEFAULT 0
   - created_at: TIMESTAMP DEFAULT CURRENT_TIMESTAMP
   ```

7. **List tables**:
   ```
   List all tables in PostgreSQL database 'inventory' on server 
   'bugbash-postgres-<random>'
   ```

8. **Get table schema**:
   ```
   Show me the schema of table 'products' in database 'inventory'
   ```

**Verify**:
- [ ] Database created
- [ ] Tables created with correct schema
- [ ] Columns have correct data types
- [ ] Constraints are applied

#### Phase 3: Data Operations and Queries

9. **Insert data**:
   ```
   Insert the following records into the products table:
   - Laptop, High-performance laptop, 1299.99, 25
   - Mouse, Wireless mouse, 29.99, 100
   - Keyboard, Mechanical keyboard, 89.99, 50
   ```

10. **Query all data**:
    ```
    Show me all products in the inventory database
    ```

11. **Query with filter**:
    ```
    Show me all products with price less than 100 in the inventory database
    ```

12. **Search query**:
    ```
    Show me all items that contain the word 'laptop' in the products 
    table in PostgreSQL database 'inventory'
    ```

**Verify**:
- [ ] Data inserted successfully
- [ ] Queries return correct results
- [ ] Filters work properly
- [ ] Text search works

#### Phase 4: Server Configuration

13. **Get server configuration**:
    ```
    Show me the configuration of PostgreSQL server 'bugbash-postgres-<random>'
    ```

14. **Check server parameters**:
    ```
    Show me if the PostgreSQL server 'bugbash-postgres-<random>' has 
    replication enabled
    ```

15. **Get connection information**:
    ```
    Get the connection string for PostgreSQL server 'bugbash-postgres-<random>' 
    database 'inventory'
    ```

**Verify**:
- [ ] Configuration retrieved
- [ ] Parameters are accessible
- [ ] Connection string provided

**Expected Results**:
- PostgreSQL server created
- Database and tables created
- Data operations work
- Queries execute correctly
- Configuration accessible

---

### Scenario 3: Azure SQL Database Operations

**Objective**: Test Azure SQL Database and server management

#### Phase 1: Create SQL Server and Database

1. **Create SQL server**:
   ```
   Create an Azure SQL server:
   - Name: 'bugbash-sqlserver-<random>'
   - Resource group: '<your-rg>'
   - Admin login: 'sqladmin'
   - Password: '<secure-password>'
   - Location: East US
   - Allow Azure services and resources to access this server
   ```

2. **List SQL servers**:
   ```
   List all Azure SQL servers in resource group '<your-rg>'
   ```

3. **Get server details**:
   ```
   Show me the details of Azure SQL server 'bugbash-sqlserver-<random>' 
   in resource group '<your-rg>'
   ```

**Verify**:
- [ ] Server created successfully
- [ ] Admin login configured
- [ ] Location is correct
- [ ] Firewall configured

#### Phase 2: Create and Manage Databases

4. **Create database**:
   ```
   Create a SQL database named 'OrdersDB' on server 
   'bugbash-sqlserver-<random>' with Basic tier
   ```

5. **List databases**:
   ```
   List all databases in Azure SQL server 'bugbash-sqlserver-<random>'
   ```

6. **Show database details**:
   ```
   Get the configuration details for SQL database 'OrdersDB' on 
   server 'bugbash-sqlserver-<random>'
   ```

7. **Create another database**:
   ```
   Create a SQL database named 'TestDB' on server 
   'bugbash-sqlserver-<random>' with Standard tier (S1)
   ```

**Verify**:
- [ ] Databases created with correct tiers
- [ ] Database properties are correct
- [ ] Databases are accessible

#### Phase 3: Firewall Rules

8. **List firewall rules**:
   ```
   List all firewall rules for SQL server 'bugbash-sqlserver-<random>'
   ```

9. **Create firewall rule**:
   ```
   Create a firewall rule for Azure SQL server 'bugbash-sqlserver-<random>' 
   to allow access from my current IP address
   ```

10. **Verify firewall rule**:
    ```
    Show me the firewall rules for SQL server 'bugbash-sqlserver-<random>'
    ```

**Verify**:
- [ ] Firewall rules listed
- [ ] New rule created
- [ ] Rule parameters correct

#### Phase 4: Database Configuration

11. **Update database tier**:
    ```
    Update the performance tier of SQL database 'OrdersDB' on server 
    'bugbash-sqlserver-<random>' to Standard S2
    ```

12. **Rename database**:
    ```
    Rename the SQL database 'TestDB' to 'StagingDB' on server 
    'bugbash-sqlserver-<random>'
    ```

13. **Check elastic pools**:
    ```
    List all elastic pools in SQL server 'bugbash-sqlserver-<random>'
    ```

14. **List Entra ID administrators**:
    ```
    List Microsoft Entra ID administrators for SQL server 
    'bugbash-sqlserver-<random>'
    ```

**Verify**:
- [ ] Tier update successful
- [ ] Database renamed correctly
- [ ] Elastic pools listed (if any)
- [ ] Admin info retrieved

#### Phase 5: Database Deletion

15. **Delete test database**:
    ```
    Delete the SQL database 'StagingDB' from server 
    'bugbash-sqlserver-<random>'
    ```

16. **Verify deletion**:
    ```
    List databases in server 'bugbash-sqlserver-<random>' to confirm 
    'StagingDB' is deleted
    ```

**Verify**:
- [ ] Database deleted successfully
- [ ] Deletion confirmed

**Expected Results**:
- SQL server created
- Multiple databases created
- Firewall rules managed
- Database configuration updated
- Database operations work correctly

## Common Issues to Watch For

- **Connection String Format**: Different formats for different database types
- **Authentication Failures**: Credential or permission issues
- **Firewall Blocking**: Access blocked by firewall rules
- **Partition Key Issues**: Cosmos DB partition key mismatches
- **Query Syntax**: Different SQL dialects across databases
- **Resource Limits**: RU limits (Cosmos), connection limits (SQL/PostgreSQL)
- **Timeout Issues**: Long-running queries
- **Case Sensitivity**: PostgreSQL vs SQL Server differences

## Related Resources

- [Azure Cosmos DB Documentation](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/)
- [Azure SQL Database](https://learn.microsoft.com/azure/azure-sql/)
- [Database Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md#azure-database-for-mysql)
- [Report Issues](https://github.com/microsoft/mcp/issues)

**Next**: [Deployment Scenarios Testing](deployment.md)
