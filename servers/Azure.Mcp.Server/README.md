# <img height="36" width="36" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> Azure MCP Server

The Azure MCP Server implements the [MCP specification](https://modelcontextprotocol.io) to create a seamless connection between AI agents and Azure services.  Azure MCP Server can be used alone or with the [GitHub Copilot for Azure extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-github-copilot) in VS Code.  This project is in Public Preview and implementation may significantly change prior to our General Availability.


>[!WARNING]
>**Deprecation Notice: SSE transport mode has been removed in version [0.4.0 (2025-07-15)](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/CHANGELOG.md#040-2025-07-15).**
>
> SSE was deprecated in MCP `2025-03-26` due to [security vulnerabilities and architectural limitations](https://blog.fka.dev/blog/2025-06-06-why-mcp-deprecated-sse-and-go-with-streamable-http/). Users must discontinue use of SSE transport mode and upgrade to version `0.4.0` or newer to maintain compatibility with current MCP clients.


## Table of Contents
- Install
    - [VS Code Install Guide (Recommended)](#vs-code-install-guide-recommended)
    - [IntelliJ Install Guide](#intellij-install-guide)
    - [Advanced Install Scenarios (Optional)](#advanced-install-scenarios-optional)
    - [Upgrading](#upgrading)
- Usage
    - [Quick Start](#quick-start)
    - [What can you do with the Azure MCP Server?](#what-can-you-do-with-the-azure-mcp-server)
    - [Currently Supported Tools](#currently-supported-tools)
- Support & Reference
    - [Data Collection](#data-collection)
    - [Troubleshooting](#troubleshooting)
    - [Security Note](#security-note)
    - [Contributing](#contributing)
    - [Code of Conduct](#code-of-conduct)

## <a id="vs-code-install-guide-recommended"></a>✅ VS Code Install Guide (Recommended)

1. Install either the stable or Insiders release of VS Code:
   * [💫 Stable release](https://code.visualstudio.com/download)
   * [🔮 Insiders release](https://code.visualstudio.com/insiders)
1. Install the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions
1. Install the [Azure MCP Server](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server) extension


## <a id="intellij-install-guide"></a> IntelliJ Install Guide

1. Install either the [IntelliJ IDEA Ultimate](https://www.jetbrains.com/idea/download) or [IntelliJ IDEA Community](https://www.jetbrains.com/idea/download) edition.
1. Install the [GitHub Copilot](https://plugins.jetbrains.com/plugin/17718-github-copilot) plugin.
1. Install the [Azure Toolkit for Intellij](https://plugins.jetbrains.com/plugin/8053-azure-toolkit-for-intellij) plugin.


## <a id="quick-start"></a> 🚀 Quick Start

1. Open GitHub Copilot in [VS Code](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode) or [IntelliJ](https://github.blog/changelog/2025-05-19-agent-mode-and-mcp-support-for-copilot-in-jetbrains-eclipse-and-xcode-now-in-public-preview/#agent-mode) and switch to Agent mode.
1. Click `refresh` on the tools list
    - You should see the Azure MCP Server in the list of tools
1. Try a prompt that tells the agent to use the Azure MCP Server, such as `List my Azure Storage containers`
    - The agent should be able to use the Azure MCP Server tools to complete your query
1. Check out the [documentation](https://learn.microsoft.com/azure/developer/azure-mcp-server/) and review the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md) for commonly asked questions
1. We're building this in the open. Your feedback is much appreciated, and will help us shape the future of the Azure MCP server
    - 👉 [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose)

## <a id="what-can-you-do-with-the-azure-mcp-server"></a> ✨ What can you do with the Azure MCP Server?

The Azure MCP Server supercharges your agents with Azure context. Here are some cool prompts you can try:

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
* "Show me my container registries in the 'myproject' resource group"
* "List all my Azure Container Registry repositories"

### ☸️ Azure Kubernetes Service (AKS)

* "List my AKS clusters in my subscription"
* "Show me all my Azure Kubernetes Service clusters"
* "List the node pools for my AKS cluster"
* "Get details for the node pool 'np1' of my AKS cluster 'my-aks-cluster' in 'my-resource-group' resource group"

### 📊 Azure Cosmos DB

* "Show me all my Cosmos DB databases"
* "List containers in my Cosmos DB database"

### 🧮 Azure Data Explorer

* "Get Azure Data Explorer databases in cluster 'mycluster'"
* "Sample 10 rows from table 'StormEvents' in Azure Data Explorer database 'db1'"

### 📣 Azure Event Grid

* "List all Event Grid topics in subscription 'my-subscription'"
* "Show me the Event Grid topics in my subscription"
* "List all Event Grid topics in resource group 'my-resourcegroup' in my subscription"

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
* "Create a firewall rule for my Azure SQL server 'myserver'"
* "Delete a firewall rule from my Azure SQL server 'myserver'"
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


## <a id="currently-supported-tools"></a> 🛠️ Currently Supported Tools

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

### 🧮 Azure Foundry

* List Azure Foundry models
* Deploy foundry models
* List foundry model deployments
* List knowledge indexes

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

### 🗄️ Azure SQL Elastic Pool

* List elastic pools in SQL servers

### 🗄️ Azure SQL Server

* List Microsoft Entra ID administrators for SQL servers

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

</details>

For detailed command documentation and examples, see [Azure MCP Commands](https://github.com/microsoft/mcp/blob/main/docs/azmcp-commands.md).

## <a id="upgrading"></a> 🔄️ Upgrading

<details>
<summary>How to stay current with releases of Azure MCP Server</summary>

#### NPX

If you use the default package spec of `@azure/mcp@latest`, npx will look for a new version on each server start. If you use just `@azure/mcp`, npx will continue to use its cached version until its cache is cleared.

#### NPM

If you globally install the cli via `npm install -g @azure/mcp` it will use the installed version until you manually update it with `npm update -g @azure/mcp`.

#### Docker

There is no version update built into the docker image.  To update, just pull the latest from the repo and repeat the [docker installation instructions](#docker-install).

#### VS Code

Installation in VS Code should be in one of the previous forms and the update instructions are the same. If you installed the mcp server with the `npx` command and  `-y @azure/mcp@latest` args, npx will check for package updates each time VS Code starts the server. Using a docker container in VS Code has the same no-update limitation described above.

#### IntelliJ

If the Azure MCP server is configured by Azure Toolkit for IntelliJ plugin, the version is automatically updated to the latest version when the IntelliJ project starts. If the Azure MCP server is manually configured with `npx` command and `-y @azure/mcp@latest` args, npx will check for package updates each time IntelliJ starts the server. Using a docker container in IntelliJ has the same no-update limitation described above.

</details>


## <a id="advanced-install-scenarios-optional"></a> ⚙️ Advanced Install Scenarios (Optional)

<details>
<summary>Docker containers, custom MCP clients, and manual install options</summary>

### 🐋 Docker Install Steps (Optional)

Microsoft publishes an official Azure MCP Server Docker container on the [Microsoft Artifact Registry](https://mcr.microsoft.com/artifact/mar/azure-sdk/azure-mcp).

For a step-by-step Docker installation, follow these instructions:

1. Create an `.env` file with environment variables that [match one of the `EnvironmentCredential`](https://learn.microsoft.com/dotnet/api/azure.identity.environmentcredential) sets.  For example, a `.env` file using a service principal could look like:

    ```bash
    AZURE_TENANT_ID={YOUR_AZURE_TENANT_ID}
    AZURE_CLIENT_ID={YOUR_AZURE_CLIENT_ID}
    AZURE_CLIENT_SECRET={YOUR_AZURE_CLIENT_SECRET}
    ```

2. Add `.vscode/mcp.json` or update existing MCP configuration. Replace `/full/path/to/.env` with a path to your `.env` file.

    ```json
    {
      "servers": {
        "Azure MCP Server": {
          "command": "docker",
          "args": [
            "run",
            "-i",
            "--rm",
            "--env-file",
            "/full/path/to/.env"
            "mcr.microsoft.com/azure-sdk/azure-mcp:latest",
          ]
        }
      }
    }
    ```

Optionally, use `--env` or `--volume` to pass authentication values.

### 🤖 Custom MCP Client Install Steps (Optional)

You can easily configure your MCP client to use the Azure MCP Server. Have your client run the following command and access it via standard IO.

```bash
npx -y @azure/mcp@latest server start
```

### 🔧 Manual Install Steps (Optional)

For a step-by-step installation, follow these instructions:

1. Add `.vscode/mcp.json`:

    ```json
    {
      "servers": {
        "Azure MCP Server": {
          "command": "npx",
          "args": [
            "-y",
            "@azure/mcp@latest",
            "server",
            "start"
          ]
        }
      }
    }
    ```

    You can optionally set the `--namespace <namespace>` flag to install tools for the specified Azure product or service.

1. Add `.vscode/mcp.json`:

    ```json
    {
      "servers": {
        "Azure Best Practices": {
          "command": "npx",
          "args": [
            "-y",
            "@azure/mcp@latest",
            "server",
            "start",
            "--namespace",
            "bestpractices" // Any of the available MCP servers can be referenced here.
          ]
        }
      }
    }
    ```

More end-to-end MCP client/agent guides are coming soon!
</details>


## <a id="data-collection"></a> Data Collection

The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft's [privacy statement](https://www.microsoft.com/privacy/privacystatement). You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.

### Telemetry Configuration

Telemetry collection is on by default.

To opt out, set the environment variable `AZURE_MCP_COLLECT_TELEMETRY` to `false` in your environment.


## <a id="troubleshooting"></a> 📝 Troubleshooting

See [Troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md) for help with common issues and logging.

### 🔑 Authentication

<details>
<summary>Authentication options including DefaultAzureCredential flow, RBAC permissions, troubleshooting, and production credentials</summary>

The Azure MCP Server uses the Azure Identity library for .NET to authenticate to Microsoft Entra ID. For detailed information, see [Authentication Fundamentals](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md#authentication-fundamentals).

If you're running into any issues with authentication, visit our [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md#authentication).

For enterprise authentication scenarios, including network restrictions, security policies, and protected resources, see [Authentication Scenarios in Enterprise Environments](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md#authentication-scenarios-in-enterprise-environments).
</details>

## <a id="security-note"></a> 🛡️ Security Note

Your credentials are always handled securely through the official [Azure Identity SDK](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md) - **we never store or manage tokens directly**.

MCP as a phenomenon is very novel and cutting-edge. As with all new technology standards, consider doing a security review to ensure any systems that integrate with MCP servers follow all regulations and standards your system is expected to adhere to. This includes not only the Azure MCP Server, but any MCP client/agent that you choose to implement down to the model provider.



## <a id="contributing"></a> 👥 Contributing

We welcome contributions to the Azure MCP Server! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for guidelines on:

* 🛠️ Setting up your development environment
* ✨ Adding new commands
* 📝 Code style and testing requirements
* 🔄 Making pull requests


## <a id="code-of-conduct"></a> 🤝 Code of Conduct

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [open@microsoft.com](mailto:open@microsoft.com)
with any additional questions or comments.
