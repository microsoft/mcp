# Azure MCP Server Extension for Visual Studio Code

Easily bring the power of Model Context Protocol (MCP) to your Azure projects in VS Code.

## Table of Contents
- [Overview](#overview)
- [Getting Started](#getting-started)
- [What can you do with the Azure MCP Server?](#what-can-you-do-with-the-azure-mcp-server)
- [Complete List of Supported Azure Services](#complete-list-of-supported-azure-services)
- [Documentation](#documentation)
- [Feedback & Support](#feedback--support)
- [Contributing](#contributing)
- [License](#license)

## Overview

**Azure MCP Server** adds smart, context-aware AI tools right inside VS Code to help you work more efficiently with Azure resources. The Azure MCP Server supercharges your agents with Azure context across **28 different Azure services**.

## Getting Started

Follow these simple steps to start using Azure MCP in VS Code:

1. **Install the Extension**
   - Get it from the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server).

2. **Start (or Auto-Start) the MCP Server**

   > **VS Code (version 1.103 or above):** You can now configure MCP servers to start automatically using the `chat.mcp.autostart` setting, instead of manually restarting them after configuration changes.

   #### **Enable Autostart**
      1. Open **Settings** in VS Code.
      2. Search for `chat.mcp.autostart`.
      3. Select **newAndOutdated** to automatically start MCP servers without manual refresh.
      4. You can also set this from the **refresh icon tooltip** in the Chat view, which also shows which servers will auto-start.

         ![VS Code MCP Autostart Tooltip](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/ToolTip.png)

   #### **Manual Start (if autostart is off)**
      1. Open Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).
      2. Run `MCP: List Servers`.

         ![List Servers](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/ListServers.png)

      3. Select `Azure MCP Server ext`, then click **Start Server**.

         ![Select Server](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/SelectServer.png)
         ![Start Server](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/StartServer.png)

      4. **Check That It's Running**
         - Go to the **Output** tab in VS Code.
         - Look for log messages confirming the server started successfully.

         ![Output](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/Output.png)

3. (Optional) Configure tools and behavior
    - Full options: control how tools are exposed and whether mutations are allowed:

       ```json
      // Server Mode: collapse per service (default), single tool, or expose every tool
      "azureMcp.serverMode": "namespace", // one of: "single" | "namespace" (default) | "all"

       // Filter which namespaces to expose
       "azureMcp.enabledServices": ["storage", "keyvault"],

       // Run the server in read-only mode (prevents write operations)
       "azureMcp.readOnly": false
       ```

   - Changes take effect after restarting the Azure MCP server from the MCP: List Servers view. (Step 2)

You’re all set! Azure MCP Server is now ready to help you work smarter with Azure resources in VS Code.

## What can you do with the Azure MCP Server?

Here are some cool prompts you can try across our supported Azure services:

### 🔎 Azure AI Search

* "What indexes do I have in my Azure AI Search service 'mysvc'?"
* "Let's search this index for 'my search query'"

### ⚙️ Azure App Configuration

* "List my App Configuration stores"
* "Show my key-value pairs in App Config"

### 📦 Azure Container Registry (ACR)

* "List all my Azure Container Registries"
* "Show me my container registries in the 'myproject' resource group"
* "List all my Azure Container Registry repositories"

### ☸️ Azure Kubernetes Service (AKS)

* "List my AKS clusters in my subscription"
* "Show me all my Azure Kubernetes Service clusters"

### 📊 Azure Cosmos DB

* "Show me all my Cosmos DB databases"
* "List containers in my Cosmos DB database"

### 🧮 Azure Data Explorer

* "Get Azure Data Explorer databases in cluster 'mycluster'"
* "Sample 10 rows from table 'StormEvents' in Azure Data Explorer database 'db1'"

### ⚡ Azure Managed Lustre

* "List the Azure Managed Lustre clusters in resource group 'my-resourcegroup'"
* "How many IP Addresses I need to create a 128 TiB cluster of AMLFS 500?"

### 📊 Azure Monitor

* "Query my Log Analytics workspace"

### 🔧 Azure Resource Management

* "List my resource groups"
* "List my Azure CDN endpoints"
* "Help me build an Azure application using Node.js"

### 🗄️ Azure SQL Database

* "Show me details about my Azure SQL database 'mydb'"
* "List all databases in my Azure SQL server 'myserver'"
* "List all firewall rules for my Azure SQL server 'myserver'"
* "List all elastic pools in my Azure SQL server 'myserver'"
* "List Active Directory administrators for my Azure SQL server 'myserver'"

### 💾 Azure Storage

* "List my Azure storage accounts"
* "Get details about my storage account 'mystorageaccount'"
* "Create a new storage account in East US with Data Lake support"
* "Show me the tables in my Storage account"
* "Get details about my Storage container"
* "Upload my file to the blob container"
* "List paths in my Data Lake file system"
* "List files and directories in my File Share"
* "Send a message to my storage queue"

## 🛠️ Currently Supported Tools

<details>
<summary>The Azure MCP Server provides tools for interacting with the following Azure services</summary>

### 🔎 Azure AI Search (search engine/vector database)

* List Azure AI Search services
* List indexes and look at their schema and configuration
* Query search indexes

### ⚙️ Azure App Configuration

* List App Configuration stores
* Manage key-value pairs
* Handle labeled configurations
* Lock/unlock configuration settings

### 🛡️ Azure Best Practices

* Get secure, production-grade Azure SDK best practices for effective code generation.

### 🖥️ Azure CLI Extension

* Execute Azure CLI commands directly
* Support for all Azure CLI functionality

### 📦 Azure Container Registry (ACR)

* List Azure Container Registries and repositories in a subscription
* Filter container registries and repositories by resource group
* JSON output formatting
* Cross-platform compatibility

### 📊 Azure Cosmos DB (NoSQL Databases)

* List Cosmos DB accounts
* List and query databases
* Manage containers and items
* Execute SQL queries against containers

### 🧮 Azure Data Explorer

* List Azure Data Explorer clusters
* List databases
* List tables
* Get schema for a table
* Sample rows from a table
* Query using KQL

### 🐬 Azure Database for MySQL - Flexible Server

* List and query databases.
* List and get schema for tables.
* List, get configuration and get parameters for servers.

### 🐘 Azure Database for PostgreSQL - Flexible Server

* List and query databases.
* List and get schema for tables.
* List, get configuration and get/set parameters for servers.

### 🛠️ Azure Developer CLI (azd) Extension

* Execute Azure Developer CLI commands directly
* Support for template discovery, template initialization, provisioning and deployment
* Cross-platform compatibility

### 🚀 Azure Deploy

* Generate Azure service architecture diagrams from source code
* Create a deploy plan for provisioning and deploying the application
* Get the application service log for a specific azd environment
* Get the bicep or terraform file generation rules for an application
* Get the GitHub pipeline creation guideline for an application

### 📣 Azure Event Grid

* "List all Event Grid topics in subscription 'my-subscription'"
* "Show me the Event Grid topics in my subscription"
* "List all Event Grid topics in resource group 'my-resourcegroup' in my subscription"

### 🧮 Azure Foundry

* List Azure Foundry models
* Deploy foundry models
* List foundry model deployments
* List knowledge indexes
* Get knowledge index schema configuration

### ☁️ Azure Function App

* List Azure Function Apps
* Get details for a specific Function App

### 🔑 Azure Key Vault

* List, create, and import certificates
* List and create keys
* List and create secrets

### ☸️ Azure Kubernetes Service (AKS)

* List Azure Kubernetes Service clusters

### 📦 Azure Load Testing

* List, create load test resources
* List, create load tests
* Get, list, (create) run and rerun, update load test runs


### 🚀 Azure Managed Grafana

* List Azure Managed Grafana

### ⚡ Azure Managed Lustre

* List Azure Managed Lustre filesystems
* Get the number of IP addresses required for a specific SKU and size of Azure Managed Lustre filesystem

### 🏪 Azure Marketplace

* Get details about Marketplace products

### 📈 Azure Monitor

#### Log Analytics

* List Log Analytics workspaces
* Query logs using KQL
* List available tables

#### Health Models

* Get health of an entity

#### Metrics

* Query Azure Monitor metrics for resources with time series data
* List available metric definitions for resources

### 🏥 Azure Service Health

* Get the availability status for a specific resource
* List availability statuses for all resources in a subscription or resource group

### ⚙️ Azure Native ISV Services

* List Monitored Resources in a Datadog Monitor

### 🛡️ Azure Quick Review CLI Extension

* Scan Azure resources for compliance related recommendations

### 📊 Azure Quota

* List available regions
* Check quota usage

### 🔴 Azure Redis Cache

* List Redis Cluster resources
* List databases in Redis Clusters
* List Redis Cache resources
* List access policies for Redis Caches

### 🏗️ Azure Resource Groups

* List resource groups

### 🎭 Azure Role-Based Access Control (RBAC)

* List role assignments

### 🚌 Azure Service Bus

* Examine properties and runtime information about queues, topics, and subscriptions

### 🗄️ Azure SQL Database

* Show database details and properties
* List the details and properties of all databases
* List SQL server firewall rules

### 🗄️ Azure SQL Elastic Pool

* List elastic pools in SQL servers

### 🗄️ Azure SQL Server

* List Microsoft Entra ID administrators for SQL servers

### 💾 Azure Storage

* List and create Storage accounts
* Get detailed information about specific Storage accounts
* Manage blob containers and blobs
* Upload files to blob containers
* List and query Storage tables
* List paths in Data Lake file systems
* Get container properties and metadata
* List files and directories in File Shares

### 📋 Azure Subscription

* List Azure subscriptions

### 🏗️ Azure Terraform Best Practices

* Get secure, production-grade Azure Terraform best practices for effective code generation and command execution

### 🖥️ Azure Virtual Desktop

* List Azure Virtual Desktop host pools
* List session hosts in host pools
* List user sessions on a session host

### 📊 Azure Workbooks

* List workbooks in resource groups
* Create new workbooks with custom visualizations
* Update existing workbook configurations
* Get workbook details and metadata
* Delete workbooks when no longer needed

### 🏗️ Bicep

* Get the Bicep schema for specific Azure resource types

### 🏗️ Cloud Architect

* Design Azure cloud architectures through guided questions

</details>

For the complete list of supported services and sample prompts, see our [full documentation](https://github.com/microsoft/mcp/blob/main/README.md#-what-can-you-do-with-the-azure-mcp-server).

## Complete List of Supported Azure Services

The Azure MCP Server provides tools for interacting with **28 Azure service areas**:

- 🔎 **Azure AI Search** - Search engine/vector database operations
- ⚙️ **Azure App Configuration** - Configuration management
- 🛡️ **Azure Best Practices** - Secure, production-grade guidance
- 🖥️ **Azure CLI Extension** - Direct Azure CLI command execution
- 📦 **Azure Container Registry (ACR)** - Container registry management
- 📊 **Azure Cosmos DB** - NoSQL database operations
- 🧮 **Azure Data Explorer** - Analytics queries and KQL
- 🐘 **Azure Database for PostgreSQL** - PostgreSQL database management
- 🐬 **Azure Database for MySQL** - MySQL database management
- 🛠️ **Azure Developer CLI (azd)** - Template and deployment management
- 📊 **Azure Event Grid** - Event routing and management
- ⚡ **Azure Functions** - Function App management
- 🧮 **Azure Foundry** - AI model management, AI model deployment, and knowledge index management
- 🚀 **Azure Managed Grafana** - Monitoring dashboards
- 🗃️ **Azure Managed Lustre** - High-performance Lustre filesystem operations
- 🔑 **Azure Key Vault** - Secrets, keys, and certificates
- ☸️ **Azure Kubernetes Service (AKS)** - Container orchestration
- 📦 **Azure Load Testing** - Performance testing
- 🏪 **Azure Marketplace** - Product discovery
- 📈 **Azure Monitor** - Logging, metrics, and health monitoring
- 🏥 **Azure Service Health** - Resource health status and availability
- ⚙️ **Azure Native ISV Services** - Third-party integrations
- 🛡️ **Azure Quick Review CLI** - Compliance scanning
- 📊 **Azure Quota** - Resource quota and usage management
- 🔴 **Azure Redis Cache** - In-memory data store
- 🏗️ **Azure Resource Groups** - Resource organization
- 🎭 **Azure RBAC** - Access control management
- 🚌 **Azure Service Bus** - Message queuing
- 🗄️ **Azure SQL Database** - Relational database management
- 🗄️ **Azure SQL Elastic Pool** - Database resource sharing
- 🗄️ **Azure SQL Server** - Server administration
- 💾 **Azure Storage** - Blob, table, file, and data lake storage
- 📋 **Azure Subscription** - Subscription management
- 🏗️ **Azure Terraform Best Practices** - Infrastructure as code guidance
- 🖥️ **Azure Virtual Desktop** - Virtual desktop infrastructure
- 📊 **Azure Workbooks** - Custom visualizations
- 🏗️ **Bicep** - Azure resource templates
- 🏗️ **Cloud Architect** - Guided architecture design

## Documentation

- See our [official documentation on learn.microsoft.com](https://learn.microsoft.com/azure/developer/azure-mcp-server/) to learn how to use the Azure MCP Server to interact with Azure resources through natural language commands from AI agents and other types of clients.
- For additional command documentation and examples, see our [GitHub repository section on Azure MCP Commands](https://github.com/microsoft/mcp/blob/main/docs/azmcp-commands.md).


## Feedback & Support

- Check the [Troubleshooting guide](https://aka.ms/azmcp/troubleshooting) to diagnose and resolve common issues with the Azure MCP Server.
- We're building this in the open. Your feedback is much appreciated, and will help us shape the future of the Azure MCP server.
    - 👉 Open an issue in the public [GitHub repository](https://github.com/microsoft/mcp/issues) — we’d love to hear from you!

## Contributing

Want to contribute?
Check out our [contribution guide](https://github.com/microsoft/mcp/blob/main/eng/vscode/CONTRIBUTING.md) to get started.

## License

This project is licensed under the [MIT License](https://github.com/microsoft/mcp/blob/main/LICENSE).
