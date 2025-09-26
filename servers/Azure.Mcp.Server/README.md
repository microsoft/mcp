# <img height="36" width="36" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> Azure MCP Server

All Azure MCP tools in a single server. The Azure MCP Server implements the [MCP specification](https://modelcontextprotocol.io) to create a seamless connection between AI agents and Azure services. Azure MCP Server can be used alone or with the [GitHub Copilot for Azure extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-github-copilot) in VS Code.  This project is in Public Preview and implementation may significantly change prior to our General Availability.

[![Install Azure MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Azure_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=github-copilot-azure.GitHubCopilotForAzure2022)  [![Install Azure MCP Server](https://img.shields.io/badge/IntelliJ%20IDEA-Install%20Azure%20MCP%20Server-1495b1?style=flat-square&logo=intellijidea&logoColor=white)](https://plugins.jetbrains.com/plugin/8053)


## Table of Contents
- [Overview](#overview)
- [Installation](#installation)
    - [IDE](#ide)
        - [VS Code (Recommended)](#vs-code-recommended)
        - [Visual Studio 2022](#visual-studio-2022)
        - [IntelliJ IDEA](#intellij-idea)
        - [Manual Setup](#manual-setup)
    - [Package Manager](#package-manager)
        - [NuGet](#nuget)
        - [NPM](#npm)
        - [Docker](#docker)
- [Usage](#usage)
    - [Getting Started](#getting-started)
    - [What can you do with the Azure MCP Server?](#what-can-you-do-with-the-azure-mcp-server)
    - [Complete List of Supported Azure Services](#complete-list-of-supported-azure-services)
- [Support and Reference](#support-and-reference)
    - [Documentation](#documentation)
    - [Feedback and Support](#feedback-and-support)
    - [Security](#security)
    - [Data Collection](#data-collection)
    - [Contributing and Code of Conduct](#contributing)

# Overview

**Azure MCP Server** supercharges your agents with Azure context across **30+ different Azure services**.

# Installation

Install Azure MCP Server using either an IDE extension or package manager. Choose one method below.

## IDE

Start using Azure MCP with your favorite IDE.  We recommend VS Code:

### VS Code (Recommended)

1. Install either the stable or Insiders release of VS Code:
   * [💫 Stable release](https://code.visualstudio.com/download)
   * [🔮 Insiders release](https://code.visualstudio.com/insiders)
1. Install the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions
1. Install the [Azure MCP Server](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server) extension

### Visual Studio 2022

From within Visual Studio 2022 install [GitHub Copilot for Azure (VS 2022)](https://marketplace.visualstudio.com/items?itemName=github-copilot-azure.GitHubCopilotForAzure2022):
1. Go to `Extensions | Manage Extensions...`
2. Switch to the `Browse` tab in `Extension Manager`
3. Search for `Github Copilot for Azure`
4. Click `Install`

### IntelliJ IDEA

1. Install either the [IntelliJ IDEA Ultimate](https://www.jetbrains.com/idea/download) or [IntelliJ IDEA Community](https://www.jetbrains.com/idea/download) edition.
1. Install the [GitHub Copilot](https://plugins.jetbrains.com/plugin/17718-github-copilot) plugin.
1. Install the [Azure Toolkit for Intellij](https://plugins.jetbrains.com/plugin/8053-azure-toolkit-for-intellij) plugin.

### Manual Setup
Azure MCP Server can also be configured across other IDEs, CLIs, and MCP clients:

<details>
<summary>Manual setup instructions</summary>

#### Sample Configuration

Copy this configuration to your client's MCP configuration file:
```json
{
    "mcpServers": {
        "azure-mcp-server": {
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
**Note:** When manually configuring Visual Studio and Visual Studio Code, use `servers` instead of `mcpServers` as the root object.

**Client-Specific Configuration**
| IDE | File Location | Documentation Link |
|-----|---------------|-------------------|
| **Amazon Q Developer** | `~/.aws/amazonq/mcp.json` (global)<br>`.amazonq/mcp.json` (workspace) | [AWS Q Developer MCP Guide](https://docs.aws.amazon.com/amazonq/latest/qdeveloper-ug/qdev-mcp.html) |
| **Claude Code** | `~/.claude.json` or `.mcp.json` (project) | [Claude Code MCP Configuration](https://scottspence.com/posts/configuring-mcp-tools-in-claude-code) |
| **Claude Desktop** | `~/.claude/claude_desktop_config.json` (macOS)<br>`%APPDATA%\Claude\claude_desktop_config.json` (Windows) | [Claude Desktop MCP Setup](https://support.claude.com/en/articles/10949351-getting-started-with-local-mcp-servers-on-claude-desktop) |
| **Cursor** | `~/.cursor/mcp.json` or `.cursor/mcp.json` | [Cursor MCP Documentation](https://docs.cursor.com/context/model-context-protocol) |
| **IntelliJ IDEA** | Built-in MCP server (2025.2+)<br>Settings > Tools > MCP Server | [IntelliJ MCP Documentation](https://www.jetbrains.com/help/ai-assistant/mcp.html) |
| **Visual Studio** | `.mcp.json` (solution/workspace) | [Visual Studio MCP Setup](https://learn.microsoft.com/visualstudio/ide/mcp-servers?view=vs-2022) |
| **VS Code** | `.vscode/mcp.json` (workspace)<br>`settings.json` (user) | [VS Code MCP Documentation](https://code.visualstudio.com/docs/copilot/chat/mcp-servers) |
| **Windsurf** | `~/.codeium/windsurf/mcp_config.json` | [Windsurf Cascade MCP Integration](https://docs.windsurf.com/windsurf/cascade/mcp) |
</details>


## Package Manager
Package manager installation offers several advantages over IDE-specific setup, including centralized dependency management, CI/CD integration, support for headless/server environments, version control, and project portability.

Install Azure MCP Server via a package manager:

### NuGet

Install the .NET Tool: [Azure.Mcp](https://www.nuget.org/packages/Azure.Mcp).

```bash
dotnet tool install --global Azure.Mcp
```

### NPM

Install the Node.js package: [@azure/mcp](https://www.npmjs.com/package/@azure/mcp).

```bash
npm install -g @azure/mcp
```

### Docker

Pull the Docker image: [mcr.microsoft.com/azure-sdk/azure-mcp](https://mcr.microsoft.com/artifact/mar/azure-sdk/azure-mcp).

```bash
docker pull mcr.microsoft.com/azure-sdk/azure-mcp
```

<details>
<summary>Docker instructions</summary>

#### Create an env file with Azure credentials

1. Create a `.env` file with Azure credentials ([see EnvironmentCredential options](https://learn.microsoft.com/dotnet/api/azure.identity.environmentcredential)):

```bash
AZURE_TENANT_ID={YOUR_AZURE_TENANT_ID}
AZURE_CLIENT_ID={YOUR_AZURE_CLIENT_ID}
AZURE_CLIENT_SECRET={YOUR_AZURE_CLIENT_SECRET}
```

#### Configure your MCP client to use Docker:

2. Add or update existing `mcp.json`.
    - Replace `/full/path/to/your.env` with the actual `.env` file path.
    - Optionally, use `--env` or `--volume` to pass authentication values.
    - **Note:** When manually configuring Visual Studio and Visual Studio Code, use `servers` instead of `mcpServers` as the root object.

```json
   {
      "mcpServers": {
         "Azure MCP Server": {
            "command": "docker",
            "args": [
               "run",
               "-i",
               "--rm",
               "--env-file",
               "/full/path/to/your.env",
               "mcr.microsoft.com/azure-sdk/azure-mcp:latest"
            ]
         }
      }
   }
```

</details>

# Usage

## Getting Started

1. Open GitHub Copilot in [VS Code](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode) or [IntelliJ](https://github.blog/changelog/2025-05-19-agent-mode-and-mcp-support-for-copilot-in-jetbrains-eclipse-and-xcode-now-in-public-preview/#agent-mode) and switch to Agent mode.
1. Click `refresh` on the tools list
    - You should see the Azure MCP Server in the list of tools
1. Try a prompt that tells the agent to use the Azure MCP Server, such as `List my Azure Storage containers`
    - The agent should be able to use the Azure MCP Server tools to complete your query
1. Check out the [documentation](https://learn.microsoft.com/azure/developer/azure-mcp-server/) and review the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md) for commonly asked questions
1. We're building this in the open. Your feedback is much appreciated, and will help us shape the future of the Azure MCP server
    - 👉 [Open an issue in the public repository](https://github.com/microsoft/mcp/issues/new/choose)

## What can you do with the Azure MCP Server?

✨ The Azure MCP Server supercharges your agents with Azure context. Here are some cool prompts you can try:

### 🧮 Azure AI Foundry

* List Azure Foundry models
* Deploy foundry models
* List foundry model deployments
* List knowledge indexes
* Get knowledge index schema configuration
  
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
* "List all Event Grid topics in resource group 'my-resourcegroup' in my subscription"
* "List Event Grid subscriptions for topic 'my-topic' in resource group 'my-resourcegroup'"
* "List Event Grid subscriptions for topic 'my-topic' in subscription 'my-subscription'"
* "List Event Grid Subscriptions in subscription 'my-subscription'"
* "List Event Grid subscriptions for topic 'my-topic' in location 'my-location'"
* "Publish an event with data '{\"name\": \"test\"}' to topic 'my-topic' using CloudEvents schema"
* "Send custom event data to Event Grid topic 'analytics-events' with EventGrid schema"

### 🔑 Azure Key Vault

* "List all secrets in my key vault 'my-vault'"
* "Create a new secret called 'apiKey' with value 'xyz' in key vault 'my-vault'"
* "List all keys in key vault 'my-vault'"
* "Create a new RSA key called 'encryption-key' in key vault 'my-vault'"
* "List all certificates in key vault 'my-vault'"
* "Import a certificate file into key vault 'my-vault' using the name 'tls-cert'"
* "Get the account settings for my key vault 'my-vault'"

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

* "List all SQL servers in my subscription"
* "List all SQL servers in my resource group 'my-resource-group'"
* "Show me details about my Azure SQL database 'mydb'"
* "List all databases in my Azure SQL server 'myserver'"
* "Update the performance tier of my Azure SQL database 'mydb'"
* "Rename my Azure SQL database 'mydb' to 'newname'"
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
* "Get details about my Storage container"
* "Upload my file to the blob container"


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

# Support and Reference

## Documentation

- See our [official documentation on learn.microsoft.com](https://learn.microsoft.com/azure/developer/azure-mcp-server/) to learn how to use the Azure MCP Server to interact with Azure resources through natural language commands from AI agents and other types of clients.
- For additional command documentation and examples, see [Azure MCP Commands](https://github.com/microsoft/mcp/blob/main/docs/azmcp-commands.md).

## Feedback and Support

- Check the [Troubleshooting guide](https://aka.ms/azmcp/troubleshooting) to diagnose and resolve common issues with the Azure MCP Server.
- We're building this in the open. Your feedback is much appreciated, and will help us shape the future of the Azure MCP server.
    - 👉 [Open an issue](https://github.com/microsoft/mcp/issues) in the public GitHub repository — we’d love to hear from you!

## Security

Your credentials are always handled securely through the official [Azure Identity SDK](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md) - **we never store or manage tokens directly**.

MCP as a phenomenon is very novel and cutting-edge. As with all new technology standards, consider doing a security review to ensure any systems that integrate with MCP servers follow all regulations and standards your system is expected to adhere to. This includes not only the Azure MCP Server, but any MCP client/agent that you choose to implement down to the model provider.

## Data Collection

The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft's [privacy statement](https://www.microsoft.com/privacy/privacystatement). You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.

### Telemetry Configuration

Telemetry collection is on by default.

To opt out, set the environment variable `AZURE_MCP_COLLECT_TELEMETRY` to `false` in your environment.



## Contributing

We welcome contributions to the Azure MCP Server! Whether you're fixing bugs, adding new features, or improving documentation, your contributions are welcome.

Please read our [Contributing Guide](https://github.com/microsoft/mcp/blob/main/CONTRIBUTING.md) for guidelines on:

* 🛠️ Setting up your development environment
* ✨ Adding new commands
* 📝 Code style and testing requirements
* 🔄 Making pull requests


## Code of Conduct

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information, see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [open@microsoft.com](mailto:open@microsoft.com)
with any additional questions or comments.
