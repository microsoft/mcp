<!--
See eng\scripts\Process-PackageReadMe.ps1 for instruction on how to annotate this README.md for package specific output
-->
# <!-- remove-section: start nuget;vsix remove_azure_logo --><img height="36" width="36" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> <!-- remove-section: end remove_azure_logo -->Azure MCP Server <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->

All Azure MCP tools in a single server. The Azure MCP Server implements the [MCP specification](https://modelcontextprotocol.io) to create a seamless connection between AI agents and Azure services. Azure MCP Server can be used alone or with the [GitHub Copilot for Azure extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-github-copilot) in VS Code.
<!-- remove-section: start nuget;vsix;npm remove_install_links -->
[![Install Azure MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Azure_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=github-copilot-azure.GitHubCopilotForAzure2022)  [![Install Azure MCP Server](https://img.shields.io/badge/IntelliJ%20IDEA-Install%20Azure%20MCP%20Server-1495b1?style=flat-square&logo=intellijidea&logoColor=white)](https://plugins.jetbrains.com/plugin/8053)

[![GitHub](https://img.shields.io/badge/github-microsoft/mcp-blue.svg?style=flat-square&logo=github&color=2787B7)](https://github.com/microsoft/mcp)
[![GitHub Release](https://img.shields.io/github/v/release/microsoft/mcp?include_prereleases&filter=Azure.Mcp.*&style=flat-square&color=2787B7)](https://github.com/microsoft/mcp/releases?q=Azure.Mcp.Server-)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square&color=2787B7)](https://github.com/microsoft/mcp/blob/main/LICENSE)

<!-- remove-section: end remove_install_links -->

## Table of Contents
- [Overview](#overview)
- [Installation](#installation)<!-- remove-section: start nuget;vsix;npm remove_installatiion_sub_sections -->
    - [IDE](#ide)
        - [VS Code (Recommended)](#vs-code-recommended)
        - [Visual Studio 2022](#visual-studio-2022)
        - [IntelliJ IDEA](#intellij-idea)
        - [Manual Setup](#manual-setup)
    - [Package Manager](#package-manager)
        - [NuGet](#nuget)
        - [NPM](#npm)
        - [Docker](#docker)<!-- remove-section: end remove_installatiion_sub_sections -->
- [Usage](#usage)
    - [Getting Started](#getting-started)
    - [What can you do with the Azure MCP Server?](#what-can-you-do-with-the-azure-mcp-server)
    - [Complete List of Supported Azure Services](#complete-list-of-supported-azure-services)
- [Support and Reference](#support-and-reference)
    - [Documentation](#documentation)
    - [Feedback and Support](#feedback-and-support)
    - [Security](#security)
    - [Data Collection](#data-collection)
    - [Contributing](#contributing)
    - [Code of Conduct](#code-of-conduct)

# Overview

**Azure MCP Server** supercharges your agents with Azure context across **40+ different Azure services**.

# Installation
<!-- insert-section: vsix {{- Install the [Azure MCP Server Visual Studio Code extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server)}} -->
<!-- insert-section: vsix {{- Start (or Auto-Start) the MCP Server}} -->
<!-- insert-section: vsix {{   > **VS Code (version 1.103 or above):** You can now configure MCP servers to start automatically using the `chat.mcp.autostart` setting, instead of manually restarting them after configuration changes.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   #### **Enable Autostart**}} -->
<!-- insert-section: vsix {{   1. Open **Settings** in VS Code.}} -->
<!-- insert-section: vsix {{   2. Search for `chat.mcp.autostart`.}} -->
<!-- insert-section: vsix {{   3. Select **newAndOutdated** to automatically start MCP servers without manual refresh.}} -->
<!-- insert-section: vsix {{   4. You can also set this from the **refresh icon tooltip** in the Chat view, which also shows which servers will auto-start.}} -->
<!-- insert-section: vsix {{      ![VS Code MCP Autostart Tooltip](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/ToolTip.png)}}-->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   #### **Manual Start (if autostart is off)**}} -->
<!-- insert-section: vsix {{   1. Open Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).}} -->
<!-- insert-section: vsix {{   2. Run `MCP: List Servers`.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{      ![List Servers](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/ListServers.png)}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   3. Select `Azure MCP Server ext`, then click **Start Server**.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{      ![Select Server](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/SelectServer.png)}} -->
<!-- insert-section: vsix {{      ![Start Server](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/StartServer.png)}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   4. **Check That It's Running**}} -->
<!-- insert-section: vsix {{      - Go to the **Output** tab in VS Code.}} -->
<!-- insert-section: vsix {{      - Look for log messages confirming the server started successfully.}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{      ![Output](https://raw.githubusercontent.com/microsoft/mcp/main/eng/vscode/resources/Walkthrough/Output.png)}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{- (Optional) Configure tools and behavior}} -->
<!-- insert-section: vsix {{    - Full options: control how tools are exposed and whether mutations are allowed:}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{       ```json}} -->
<!-- insert-section: vsix {{      // Server Mode: collapse per service (default), single tool, or expose every tool}} -->
<!-- insert-section: vsix {{      "azureMcp.serverMode": "namespace", // one of: "single" | "namespace" (default) | "all"}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{       // Filter which namespaces to expose}} -->
<!-- insert-section: vsix {{       "azureMcp.enabledServices": ["storage", "keyvault"],}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{       // Run the server in read-only mode (prevents write operations)}} -->
<!-- insert-section: vsix {{       "azureMcp.readOnly": false}} -->
<!-- insert-section: vsix {{       ```}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{   - Changes take effect after restarting the Azure MCP server from the MCP: List Servers view. (Step 2)}} -->
<!-- insert-section: vsix {{    }} -->
<!-- insert-section: vsix {{You’re all set! Azure MCP Server is now ready to help you work smarter with Azure resources in VS Code.}} -->
<!-- remove-section: start vsix remove_entire_installation_sub_section -->
<!-- remove-section: start nuget;npm remove_ide_sub_section -->
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

Use one of the following options to configure your `mcp.json`:
<!-- remove-section: end remove_ide_sub_section -->
<!-- remove-section: start npm remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_dotnet_config_sub_header -->
#### Option 1: Configure using .NET tool (dnx)<!-- remove-section: end remove_dotnet_config_sub_header -->
- To use Azure MCP server from .NET, you must have [.NET 10 Preview 6 or later](https://dotnet.microsoft.com/download/dotnet/10.0) installed. This version of .NET adds a command, dnx, which is used to download, install, and run the MCP server from [nuget.org](https://www.nuget.org).
To verify the .NET version, run the following command in the terminal: `dotnet --info`
-  Configure the `mcp.json` file with the following:

    ```json
    {
        "mcpServers": {
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
    }
    ```
<!-- remove-section: end remove_dotnet_config_sub_section -->
<!-- remove-section: start nuget remove_node_config_sub_section -->
<!-- remove-section: start npm remove_node_config_sub_header -->
#### Option 2: Configure using Node.js (npm/npx)<!-- remove-section: end remove_node_config_sub_header -->
- To use Azure MCP server from node you must have Node.js (LTS) installed and available on your system PATH — this provides both `npm` and `npx`. We recommend Node.js 20 LTS or later. To verify your installation run: `node --version`, `npm --version`, and `npx --version`.
-  Configure the `mcp.json` file with the following:

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
<!-- remove-section: end remove_node_config_sub_section -->
<!-- remove-section: start nuget remove_custom_client_config_table -->
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
<!-- remove-section: end remove_custom_client_config_table -->
<!-- remove-section: start nuget;npm remove_package_manager_section -->
</details>


## Package Manager
Package manager installation offers several advantages over IDE-specific setup, including centralized dependency management, CI/CD integration, support for headless/server environments, version control, and project portability.

Install Azure MCP Server via a package manager:

### NuGet

Install the .NET Tool: [Azure.Mcp](https://www.nuget.org/packages/Azure.Mcp).

```bash
dotnet tool install Azure.Mcp
```
or 
```bash
dotnet tool install Azure.Mcp --version <version>
```

### NPM

Install the Node.js package: [@azure/mcp](https://www.npmjs.com/package/@azure/mcp).

```bash
npm install @azure/mcp@latest
```

To install a specific version, use:

```bash
npm install @azure/mcp@<version>
```

To install and/or invoke the Azure MCP tool, use:

```bash
npx -y @azure/mcp [command]
```

<details>
<summary>Additional instructions</summary>

To troubleshoot @azure/mcp package (or respective binaries)installation, see [Troubleshooting guide](https://github.com/microsoft/mcp/blob/main/eng/npm/TROUBLESHOOTING.md)

To understand how platform-specific binaries are installed with @azure/mcp, see [Wrapper Binaries architecture](https://github.com/microsoft/mcp/blob/main/eng/npm/wrapperBinariesArchitecture.md)
</details>

### Docker

Run the Azure MCP server as a Docker container for easy deployment and isolation. The container image is available at [mcr.microsoft.com/azure-sdk/azure-mcp](https://mcr.microsoft.com/artifact/mar/azure-sdk/azure-mcp).

<details>
<summary>Docker instructions</summary>

#### Create an env file with Azure credentials

1. Create a `.env` file with Azure credentials ([see EnvironmentCredential options](https://learn.microsoft.com/dotnet/api/azure.identity.environmentcredential)):

```bash
AZURE_TENANT_ID={YOUR_AZURE_TENANT_ID}
AZURE_CLIENT_ID={YOUR_AZURE_CLIENT_ID}
AZURE_CLIENT_SECRET={YOUR_AZURE_CLIENT_SECRET}
```

#### Configure your MCP client to use Docker

2. Add or update existing `mcp.json`.  Replace `/full/path/to/your/.env` with the actual `.env` file path.

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
               "/full/path/to/your/.env",
               "mcr.microsoft.com/azure-sdk/azure-mcp:latest"
            ]
         }
      }
   }
```
</details>

To use Azure Entra ID, review the [troubleshooting guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md#using-azure-entra-id-with-docker).
<!-- remove-section: end remove_package_manager_section -->
<!-- remove-section: end remove_entire_installation_sub_section -->

# Usage

## Getting Started

1. Open GitHub Copilot in [VS Code](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode) <!-- remove-section: start vsix remove_intellij_uri -->or [IntelliJ](https://github.blog/changelog/2025-05-19-agent-mode-and-mcp-support-for-copilot-in-jetbrains-eclipse-and-xcode-now-in-public-preview/#agent-mode)<!-- remove-section: end remove_intellij_uri --> and switch to Agent mode.
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

### 🎤 Azure AI Services Speech

* "Convert this audio file to text using Azure Speech Services"
* "Recognize speech from my audio file with language detection"
* "Transcribe speech from audio with profanity filtering"
* "Transcribe audio with phrase hints for better accuracy"

### ⚙️ Azure App Configuration

* "List my App Configuration stores"
* "Show my key-value pairs in App Config"

### ⚙️ Azure App Lens

* "Help me diagnose issues with my app"

### 🕸️ Azure App Service

* "List the websites in my subscription"
* "Show me the websites in my 'my-resource-group' resource group"
* "Get the details for website 'my-website'"
* "Get the details for app service plan 'my-app-service-plan'"

### 🖥️ Azure CLI Generate

* Generate Azure CLI commands based on user intent

### 🖥️ Azure CLI Install

* Get installation instructions for Azure CLI, Azure Developer CLI and Azure Functions Core Tools CLI for your platform.

### 📞 Azure Communication Services

* "Send an SMS message to +1234567890"
* "Send SMS with delivery reporting enabled"
* "Send a broadcast SMS to multiple recipients"
* "Send SMS with custom tracking tag"
* "Send an email from 'sender@example.com' to 'recipient@example.com' with subject 'Hello' and message 'Welcome!'"
* "Send an HTML email to multiple recipients with CC and BCC using Azure Communication Services"
* "Send an email with reply-to address 'reply@example.com' and subject 'Support Request'"
* "Send an email from my communication service endpoint with custom sender name and multiple recipients"
* "Send an email to 'user1@example.com' and 'user2@example.com' with subject 'Team Update' and message 'Please review the attached document.'"

### 📦 Azure Container Apps

* "List the container apps in my subscription"
* "Show me the container apps in my 'my-resource-group' resource group"

### 🔐 Azure Confidential Ledger

* "Append entry {"foo":"bar"} to ledger contoso"
* "Get entry with id 2.40 from ledger contoso"

### 📦 Azure Container Registry (ACR)

* "List all my Azure Container Registries"
* "Show me my container registries in the 'my-resource-group' resource group"
* "List all my Azure Container Registry repositories"

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

### ☸️ Azure Kubernetes Service (AKS)

* "List my AKS clusters in my subscription"
* "Show me all my Azure Kubernetes Service clusters"
* "List the node pools for my AKS cluster"
* "Get details for the node pool 'np1' of my AKS cluster 'my-aks-cluster' in the 'my-resource-group' resource group"

### ⚡ Azure Managed Lustre

* "List the Azure Managed Lustre clusters in resource group 'my-resource-group'"
* "How many IP Addresses I need to create a 128 TiB cluster of AMLFS 500?"
* "Check if 'my-subnet-id' can host an Azure Managed Lustre with 'my-size' TiB and 'my-sku' in 'my-region'
* Create a 4 TIB Azure Managed Lustre filesystem in 'my-region' attaching to 'my-subnet' in virtual network 'my-virtual-network'

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

The Azure MCP Server provides tools for interacting with **40+ Azure service areas**:

- 🧮 **Azure AI Foundry** - AI model management, AI model deployment, and knowledge index management
- 🔎 **Azure AI Search** - Search engine/vector database operations
- 🎤 **Azure AI Services Speech** - Speech-to-text recognition
- ⚙️ **Azure App Configuration** - Configuration management
- 🕸️ **Azure App Service** - Web app hosting
- 🛡️ **Azure Best Practices** - Secure, production-grade guidance
- 🖥️ **Azure CLI Generate** - Generate Azure CLI commands from natural language
- 📞 **Azure Communication Services** - SMS messaging and communication
- 🔐 **Azure Confidential Ledger** - Tamper-proof ledger operations
- 📦 **Azure Container Apps** - Container hosting
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
- 🚌 **Azure Service Bus** - Message queuing
- 🏥 **Azure Service Health** - Resource health status and availability
- 🗄️ **Azure SQL Database** - Relational database management
- 🗄️ **Azure SQL Elastic Pool** - Database resource sharing
- 🗄️ **Azure SQL Server** - Server administration
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
- For additional command documentation and examples, see [Azure MCP Commands](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md).

## Feedback and Support

- Check the [Troubleshooting guide](https://aka.ms/azmcp/troubleshooting) to diagnose and resolve common issues with the Azure MCP Server.
- We're building this in the open. Your feedback is much appreciated, and will help us shape the future of the Azure MCP server.
    - 👉 [Open an issue](https://github.com/microsoft/mcp/issues) in the public GitHub repository — we’d love to hear from you!

## Security

Your credentials are always handled securely through the official [Azure Identity SDK](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/README.md) - **we never store or manage tokens directly**.

MCP as a phenomenon is very novel and cutting-edge. As with all new technology standards, consider doing a security review to ensure any systems that integrate with MCP servers follow all regulations and standards your system is expected to adhere to. This includes not only the Azure MCP Server, but any MCP client/agent that you choose to implement down to the model provider.

## Data Collection

<!-- remove-section: start vsix remove_data_collection_section_content -->
The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft's [privacy statement](https://www.microsoft.com/privacy/privacystatement). You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.
<!-- remove-section: end remove_data_collection_section_content -->
<!-- insert-section: vsix {{The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry by following the instructions [here](https://code.visualstudio.com/docs/configure/telemetry#_disable-telemetry-reporting).}} -->

<!-- remove-section: start vsix remove_telemetry_config_section -->
### Telemetry Configuration

Telemetry collection is on by default.

To opt out, set the environment variable `AZURE_MCP_COLLECT_TELEMETRY` to `false` in your environment.
<!-- remove-section: end remove_telemetry_config_section -->

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