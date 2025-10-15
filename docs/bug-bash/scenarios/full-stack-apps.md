# Full Stack Application Testing Scenario

Test Azure MCP Server's ability to help create complete applications with database backends and deploy them to Azure.

## Objectives

- Create a complete full-stack application
- Set up database backends (Cosmos DB, PostgreSQL, or Azure SQL)
- Deploy application to Azure
- Test CRUD operations against the database
- Verify end-to-end functionality

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Development tools installed (Node.js, Python, or .NET SDK)
- [ ] GitHub Copilot with Agent mode enabled
- [ ] VS Code or preferred IDE

## Test Scenarios

### Scenario 1: Node.js Web App with Cosmos DB

**Objective**: Create a complete Node.js application with Cosmos DB backend

**Steps**:

#### Phase 1: Create the Database

1. **Ask Copilot to create a Cosmos DB account**:
   ```
   Create a Cosmos DB account named 'bugbash-cosmos-<random>' in resource 
   group '<your-rg>' in East US with SQL API
   ```

2. **Create a database**:
   ```
   Create a database named 'TasksDB' in Cosmos DB account 'bugbash-cosmos-<random>'
   ```

3. **Create a container**:
   ```
   Create a container named 'Tasks' in database 'TasksDB' with 
   partition key '/category' and 400 RU/s throughput
   ```

4. **Add sample data**:
   ```
   Add the following sample tasks to the Tasks container:
   - { "id": "1", "title": "Buy groceries", "category": "personal", "completed": false }
   - { "id": "2", "title": "Review PR", "category": "work", "completed": false }
   - { "id": "3", "title": "Call dentist", "category": "personal", "completed": true }
   ```

5. **Verify data**:
   ```
   Query all tasks from the Tasks container in Cosmos DB
   ```

#### Phase 2: Build the Application

6. **Ask Copilot to create the application structure**:
   ```
   Create a Node.js Express application for a task management system with:
   - Express.js web framework
   - Cosmos DB integration using @azure/cosmos
   - RESTful API endpoints for:
     * GET /api/tasks - list all tasks
     * GET /api/tasks/:id - get a specific task
     * POST /api/tasks - create a new task
     * PUT /api/tasks/:id - update a task
     * DELETE /api/tasks/:id - delete a task
   - A simple HTML frontend with JavaScript
   - Connection to Cosmos DB 'bugbash-cosmos-<random>'
   ```

7. **Review the generated code**:
   - [ ] Check package.json dependencies
   - [ ] Verify Cosmos DB connection configuration
   - [ ] Review API endpoint implementations
   - [ ] Check error handling
   - [ ] Verify frontend HTML/JS

8. **Ask for configuration guidance**:
   ```
   How do I configure the Cosmos DB connection string in my Node.js 
   application securely?
   ```

#### Phase 3: Test Locally

9. **Install dependencies**:
   ```bash
   npm install
   ```

10. **Set up environment variables**:
    ```
    Get the Cosmos DB connection string for 'bugbash-cosmos-<random>'
    ```
    - Copy the connection string
    - Create `.env` file with connection string

11. **Run the application locally**:
    ```bash
    npm start
    ```

12. **Test the API endpoints**:
    - [ ] GET /api/tasks returns all tasks
    - [ ] GET /api/tasks/1 returns specific task
    - [ ] POST /api/tasks creates new task
    - [ ] PUT /api/tasks/1 updates task
    - [ ] DELETE /api/tasks/1 deletes task

13. **Test the frontend**:
    - [ ] Open http://localhost:3000
    - [ ] View task list
    - [ ] Create a new task
    - [ ] Edit a task
    - [ ] Delete a task
    - [ ] Verify changes in Cosmos DB

#### Phase 4: Deploy to Azure

14. **Create App Service resources**:
    ```
    Create an Azure App Service for my Node.js application:
    - Name: 'bugbash-taskapp-<random>'
    - Resource group: '<your-rg>'
    - Runtime: Node.js 20 LTS
    - Tier: B1
    - Region: East US
    ```

15. **Configure application settings**:
    ```
    Add the Cosmos DB connection string as an application setting 
    named 'COSMOS_CONNECTION_STRING' in app service 'bugbash-taskapp-<random>'
    ```

16. **Deploy the application**:
    ```
    How do I deploy my Node.js application to Azure App Service 
    'bugbash-taskapp-<random>'?
    ```
    - Follow the deployment instructions provided

17. **Verify deployment**:
    ```
    What is the URL for app service 'bugbash-taskapp-<random>'?
    ```
    - Open the URL in browser
    - Test all CRUD operations

**Expected Results**:
- Cosmos DB account and container created
- Application code generated correctly
- Local testing successful
- Deployment to Azure successful
- Production app works correctly
- Database operations work in production

---

### Scenario 2: Python Web App with PostgreSQL

**Objective**: Create a Flask application with PostgreSQL backend

**Steps**:

#### Phase 1: Create PostgreSQL Database

1. **Create PostgreSQL server**:
   ```
   Create an Azure Database for PostgreSQL flexible server:
   - Name: 'bugbash-postgres-<random>'
   - Resource group: '<your-rg>'
   - Admin user: 'dbadmin'
   - Password: '<generate-secure-password>'
   - Version: PostgreSQL 14
   - Tier: Burstable B1ms
   - Region: East US
   - Allow Azure services access
   ```

2. **Create database**:
   ```
   Create a database named 'inventory' on PostgreSQL server 
   'bugbash-postgres-<random>'
   ```

3. **Create table schema**:
   ```
   In PostgreSQL database 'inventory', create a table named 'products' with:
   - id (serial primary key)
   - name (varchar 100)
   - description (text)
   - price (decimal 10,2)
   - stock (integer)
   - created_at (timestamp)
   ```

4. **Insert sample data**:
   ```
   Insert sample products into the 'products' table:
   - Laptop, High-performance laptop, 999.99, 10
   - Mouse, Wireless mouse, 29.99, 50
   - Keyboard, Mechanical keyboard, 79.99, 30
   ```

#### Phase 2: Build Python Application

5. **Create Flask application**:
   ```
   Create a Python Flask application for an inventory management system:
   - Flask web framework
   - psycopg2 for PostgreSQL connection
   - API endpoints:
     * GET /api/products - list all products
     * GET /api/products/<id> - get product details
     * POST /api/products - add new product
     * PUT /api/products/<id> - update product
     * DELETE /api/products/<id> - delete product
   - Simple HTML frontend
   - Connection to PostgreSQL 'bugbash-postgres-<random>'
   ```

6. **Review generated code**:
   - [ ] Check requirements.txt
   - [ ] Verify PostgreSQL connection
   - [ ] Review API routes
   - [ ] Check SQL queries
   - [ ] Verify error handling

#### Phase 3: Test Locally

7. **Set up virtual environment**:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   pip install -r requirements.txt
   ```

8. **Configure database connection**:
   ```
   Get the PostgreSQL connection string for 'bugbash-postgres-<random>'
   ```
   - Create `.env` file with credentials

9. **Run locally**:
   ```bash
   python app.py
   ```

10. **Test endpoints**:
    - [ ] List products
    - [ ] Get product details
    - [ ] Add new product
    - [ ] Update product
    - [ ] Delete product

#### Phase 4: Deploy to Azure

11. **Create App Service for Python**:
    ```
    Create an Azure App Service for Python:
    - Name: 'bugbash-inventory-<random>'
    - Runtime: Python 3.11
    - Tier: B1
    - Region: East US
    ```

12. **Configure connection**:
    ```
    Configure the PostgreSQL connection in app service settings
    ```

13. **Deploy application**:
    ```
    Deploy my Python Flask app to 'bugbash-inventory-<random>'
    ```

14. **Test production app**:
    - Open app URL
    - Verify all CRUD operations

**Expected Results**:
- PostgreSQL database created
- Python application generated
- Local testing successful
- Azure deployment successful
- Production app functional

---

### Scenario 3: .NET Web App with Azure SQL

**Objective**: Create an ASP.NET Core application with Azure SQL backend

**Steps**:

#### Phase 1: Create Azure SQL Database

1. **Create SQL server**:
   ```
   Create an Azure SQL server:
   - Name: 'bugbash-sqlserver-<random>'
   - Resource group: '<your-rg>'
   - Admin login: 'sqladmin'
   - Password: '<secure-password>'
   - Region: East US
   - Allow Azure services
   ```

2. **Create database**:
   ```
   Create a database named 'CustomerDB' on SQL server 
   'bugbash-sqlserver-<random>' with Basic tier
   ```

3. **Create tables**:
   ```
   Create the following tables in CustomerDB:
   
   Customers table:
   - CustomerId (int primary key identity)
   - FirstName (nvarchar 50)
   - LastName (nvarchar 50)
   - Email (nvarchar 100)
   - Phone (nvarchar 20)
   - CreatedDate (datetime)
   
   Orders table:
   - OrderId (int primary key identity)
   - CustomerId (int foreign key)
   - OrderDate (datetime)
   - TotalAmount (decimal 18,2)
   - Status (nvarchar 20)
   ```

#### Phase 2: Build .NET Application

4. **Create ASP.NET Core Web API**:
   ```
   Create an ASP.NET Core 8.0 Web API application for customer management:
   - Entity Framework Core for database access
   - Controllers for Customers and Orders
   - API endpoints following REST conventions
   - Connection to Azure SQL 'bugbash-sqlserver-<random>'
   - Swagger/OpenAPI documentation
   - Include a simple React frontend
   ```

5. **Review code structure**:
   - [ ] Check Models (Customer, Order)
   - [ ] Review DbContext configuration
   - [ ] Verify Controllers
   - [ ] Check API routes
   - [ ] Review frontend React components

#### Phase 3: Test Locally

6. **Configure connection string**:
   ```
   Get the connection string for Azure SQL database 'CustomerDB'
   ```
   - Add to appsettings.Development.json

7. **Run migrations**:
   ```bash
   dotnet ef database update
   ```

8. **Start application**:
   ```bash
   dotnet run
   ```

9. **Test API**:
   - Open Swagger UI: https://localhost:5001/swagger
   - [ ] Test GET /api/customers
   - [ ] Test POST /api/customers
   - [ ] Test GET /api/orders
   - [ ] Test POST /api/orders

#### Phase 4: Deploy to Azure

10. **Create App Service**:
    ```
    Create an Azure App Service for .NET 8.0:
    - Name: 'bugbash-customerapi-<random>'
    - Runtime: .NET 8
    - Tier: B1
    ```

11. **Configure SQL connection**:
    ```
    Add SQL connection string to app service configuration
    ```

12. **Deploy application**:
    ```
    Deploy my .NET application to Azure App Service
    ```

13. **Verify deployment**:
    - Test API endpoints in production
    - Verify database connectivity
    - Test frontend functionality

**Expected Results**:
- Azure SQL database created
- .NET application generated
- EF Core migrations work
- Local testing successful
- Azure deployment successful
- Production API operational

## Common Issues to Watch For

- **Database Connection Issues**: Connection string format, firewall rules
- **Authentication/Authorization**: Missing database credentials
- **CORS Errors**: Frontend can't communicate with backend
- **Port Conflicts**: Port already in use locally
- **Dependency Issues**: Missing or incompatible packages
- **Environment Variables**: Not properly configured in production
- **Database Migrations**: Schema changes not applied
- **Build Failures**: Platform-specific compilation issues
- **Timeout Issues**: Long-running database operations
- **Connection Pool Exhaustion**: Too many concurrent connections

## Related Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Azure Cosmos DB Documentation](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure PostgreSQL Documentation](https://learn.microsoft.com/azure/postgresql/)
- [Azure SQL Documentation](https://learn.microsoft.com/azure/azure-sql/)
- [Deployment Testing](deployment.md)
- [Report Issues](https://github.com/microsoft/mcp/issues)

**Next**: [Infra As Code](infra-as-code.md)