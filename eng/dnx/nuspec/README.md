# Azure MCP Server .NET Tool

- Install the Azure MCP Server .NET tool from NuGet to add Model Context Protocol (MCP) capabilities to your Azure projects and enable integration with VS Code.

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

**Azure MCP Server** adds smart, context-aware AI tools right inside VS Code to help you work more efficiently with Azure resources. The Azure MCP Server supercharges your agents with Azure context across **30+ different Azure services**.

## Getting Started

### Requirements
To run the Azure MCP server, you must have [.NET 10 Preview 6 or later](https://dotnet.microsoft.com/download/dotnet/10.0) installed. This version of .NET adds a command, dnx, which is used to download, install, and run the MCP server from [nuget.org](https://www.nuget.org).

To verify your .NET version, run the following command in your terminal:

```
dotnet --info
```

### Configuration
To configure the MCP server for use with VS Code, use the following snippet and include it in your `mcp.json`
```
"servers": {
	"Azure MCP Server": {
		"command": "dnx",
		"args": [
			"Azure.Mcp",
			"--source",
			"https://api.nuget.org/v3/index.json",
            "--yes",
			"--",
			"azmcp",
			"server",
			"start"
		],
		"type": "stdio"
	}
}
```
If you'd like to use a specific version of the Azure MCP server, you can specify it with the --version argument, like so:
```
"servers": {
	"Azure MCP Server": {
		"command": "dnx",
		"args": [
			"Azure.Mcp",
			"--source",
			"https://api.nuget.org/v3/index.json",
			"--version",
			"0.8.1",
			"--yes",
			"--",
			"azmcp",
			"server",
			"start"
		],
		"type": "stdio"
	}
}
```
When configured this way, you will need to update the version as new release become available.

## What can you do with the Azure MCP Server?

Here are some cool prompts you can try across our supported Azure services:

### 🧮 Azure AI Foundry

* List Azure Foundry models
* Deploy foundry models
* List foundry model deployments
* List knowledge indexes

### 🔎 Azure AI Search

* "What indexes do I have in my Azure AI Search service 'mysvc'?"
* "Let's search this index for 'my search query'"

### ⚙️ Azure App Configuration

* "List my App Configuration stores"
* "Show my key-value pairs in App Config"

### ⚙️ Azure App Lens

* "Help me diagnose issues with my app"

### 📦 Azure Container Registry (ACR)

* "List all my Azure Container Registries"
* "Show me my container registries in the 'my-resource-group' resource group"
* "List all my Azure Container Registry repositories"

### ☸️ Azure Kubernetes Service (AKS)

* "List my AKS clusters in my subscription"
* "Show me all my Azure Kubernetes Service clusters"
* "List the node pools for my AKS cluster"
* "Get details for the node pool 'np1' of my AKS cluster 'my-aks-cluster' in the 'my-resource-group' resource group"

### 📊 Azure Cosmos DB

* "Show me all my Cosmos DB databases"
* "List containers in my Cosmos DB database"

### 🧮 Azure Data Explorer

* "Get Azure Data Explorer databases in cluster 'mycluster'"
* "Sample 10 rows from table 'StormEvents' in Azure Data Explorer database 'db1'"

### 📣 Azure Event Grid

* "List all Event Grid topics in subscription 'my-subscription'"
* "Show me the Event Grid topics in my subscription"
* "List all Event Grid topics in resource group 'my-resource-group' in my subscription"

### ⚡ Azure Managed Lustre

* "List the Azure Managed Lustre clusters in resource group 'my-resource-group'"
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
* "Create a firewall rule for my Azure SQL server 'myserver'"
* "Delete a firewall rule from my Azure SQL server 'myserver'"
* "List all elastic pools in my Azure SQL server 'myserver'"
* "List Active Directory administrators for my Azure SQL server 'myserver'"
* "Create a new Azure SQL server in my resource group 'my-resource-group'"
* "Show me details about my Azure SQL server 'myserver'"
* "Delete my Azure SQL server 'myserver'"

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

### 🚀 Azure Deploy

* Generate Azure service architecture diagrams from source code
* Create a deploy plan for provisioning and deploying the application
* Get the application service log for a specific azd environment
* Get the bicep or terraform file generation rules for an application
* Get the GitHub pipeline creation guideline for an application

### 📣 Azure Event Grid

* List Event Grid topics in subscription or resource group
* View topic configuration and status information
* Access endpoint and key details for event publishing

### ☁️ Azure Function App

* List Azure Function Apps
* Get details for a specific Function App

### 🔑 Azure Key Vault

* List, create, and import certificates
* List and create keys
* List and create secrets

### ☸️ Azure Kubernetes Service (AKS)

* List Azure Kubernetes Service clusters
* List node pools in an AKS managed cluster
* Get details of a node pool in an AKS managed cluster

### 📦 Azure Load Testing

* List, create load test resources
* List, create load tests
* Get, list, (create) run and rerun, update load test runs

### 🚀 Azure Managed Grafana

* List Azure Managed Grafana

### ⚡ Azure Managed Lustre

* List Azure Managed Lustre filesystems
* Get the number of IP addresses required for a specific SKU and size of Azure Managed Lustre filesystem
* Get information of Azure Managed Lustre SKUs available in a specific Azure region

### 🏪 Azure Marketplace

* List marketplace products available to a subscription with filtering capabilities
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

### 🏥 Azure Resource Health

* Get the availability status for a specific resource
* List availability statuses for all resources in a subscription or resource group
* List service health events in a subscription

### 🎭 Azure Role-Based Access Control (RBAC)

* List role assignments

### 🚌 Azure Service Bus

* Examine properties and runtime information about queues, topics, and subscriptions

### 🗄️ Azure SQL Database

* Show database details and properties
* List the details and properties of all databases
* List SQL server firewall rules
* Create SQL server firewall rules
* Delete SQL server firewall rules
* List elastic pools in SQL servers
* List Microsoft Entra ID administrators for SQL servers
* Create new SQL servers
* Show details and properties of SQL servers
* Delete SQL servers

### 💾 Azure Storage

* List and create Storage accounts
* Get detailed information about specific Storage accounts
* Manage blob containers and blobs
* Upload files to blobs
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

Agents and models can discover and learn best practices and usage guidelines for the `azd` MCP tool. For more information, see [AZD Best Practices](https://github.com/microsoft/mcp/tree/main/tools/Azure.Mcp.Tools.Extension/src/Resources/azd-best-practices.txt).

For detailed command documentation and examples, see [Azure MCP Commands](https://github.com/microsoft/mcp/blob/main/docs/azmcp-commands.md).

</details>

## Complete List of Supported Azure Services

The Azure MCP Server provides tools for interacting with **30+ Azure service areas**:

- 🧮 **Azure AI Foundry** - AI model management, AI model deployment, and knowledge index management
- 🔎 **Azure AI Search** - Search engine/vector database operations
- ⚙️ **Azure App Configuration** - Configuration management
- 🛡️ **Azure Best Practices** - Secure, production-grade guidance
- 📦 **Azure Container Registry (ACR)** - Container registry management
- 📊 **Azure Cosmos DB** - NoSQL database operations
- 🧮 **Azure Data Explorer** - Analytics queries and KQL
- 🐬 **Azure Database for MySQL** - MySQL database management
- 🐘 **Azure Database for PostgreSQL** - PostgreSQL database management
- 📊 **Azure Event Grid** - Event routing and management
- ⚡ **Azure Functions** - Function App management
- 🔑 **Azure Key Vault** - Secrets, keys, and certificates
- ☸️ **Azure Kubernetes Service (AKS)** - Container orchestration
- 📦 **Azure Load Testing** - Performance testing
- 🚀 **Azure Managed Grafana** - Monitoring dashboards
- 🗃️ **Azure Managed Lustre** - High-performance Lustre filesystem operations
- 🏪 **Azure Marketplace** - Product discovery
- 📈 **Azure Monitor** - Logging, metrics, and health monitoring
- ⚙️ **Azure Native ISV Services** - Third-party integrations
- 🛡️ **Azure Quick Review CLI** - Compliance scanning
- 📊 **Azure Quota** - Resource quota and usage management
- 🎭 **Azure RBAC** - Access control management
- 🔴 **Azure Redis Cache** - In-memory data store
- 🏗️ **Azure Resource Groups** - Resource organization
- 🗄️ **Azure SQL Database** - Relational database management
- 🗄️ **Azure SQL Elastic Pool** - Database resource sharing
- 🗄️ **Azure SQL Server** - Server administration
- 🚌 **Azure Service Bus** - Message queuing
- 🏥 **Azure Service Health** - Resource health status and availability
- 💾 **Azure Storage** - Blob storage
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
Check out our [contribution guide](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) to get started.

## License

This project is licensed under the [MIT License](https://github.com/microsoft/mcp/blob/main/LICENSE).
