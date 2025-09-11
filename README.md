# 🌟 Microsoft MCP Servers


## 📘 What is MCP?

**Model Context Protocol (MCP)** is an open protocol that standardizes how applications provide context to large language models (LLMs). It allows AI applications to connect with various data sources and tools in a consistent manner, enhancing their capabilities and flexibility. MCP follows a client-server architecture:

- **MCP Hosts**: Applications like AI assistants or IDEs that initiate connections.
- **MCP Clients**: Connectors within the host application that maintain 1:1 connections with servers.
- **MCP Servers**: Services that provide context and capabilities through the standardized MCP.

For more details, visit the [official MCP website](https://modelcontextprotocol.io).

## 📁 Which MCP Servers are built from this repository?

This repository contains core libraries, test frameworks, engineering systems, pipelines, and tooling for Microsoft MCP Server contributors to unify engineering investments; and reduce duplication and divergence:

| MCP Server           |  README              | Source Code             |    CHANGELOG          | Releases             | Documentation             | Troubleshooting             | Support             |
|:---------------------|:--------------------:|:-----------------------:|:---------------------:|:--------------------:|:-------------------------:|:---------------------------:|:-------------------:|
| Azure MCP            | [Azure MCP README]   | [Azure MCP Source Code] | [Azure MCP CHANGELOG] | [Azure MCP Releases] | [Azure MCP Documentation] | [Azure MCP Troubleshooting] | [Azure MCP Support] |

[Azure MCP README]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md
[Azure MCP CHANGELOG]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/CHANGELOG.md
[Azure MCP Source Code]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server
[Azure MCP Releases]: https://github.com/microsoft/mcp/releases?q=Azure.Mcp.Server-0
[Azure MCP Documentation]: https://learn.microsoft.com/azure/developer/azure-mcp-server/
[Azure MCP Troubleshooting]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md
[Azure MCP Support]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/SUPPORT.md


## 📚 Which MCP Servers are available from Microsoft?

### <img height="18" width="18" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> Azure
- **REPOSITORY**: [microsoft/mcp](https://github.com/microsoft/mcp/tree/main/servers/Azure.Mcp.Server#readme)
- **DESCRIPTION**: All Azure MCP tools in a single server.  The Azure MCP Server implements the MCP specification to create a seamless connection between AI agents and Azure services.  Azure MCP Server can be used alone or with the GitHub Copilot for Azure extension in VS Code.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`

### ✨ Azure AI Foundry
- **REPOSITORY**: [azure-ai-foundry/mcp-foundry](https://github.com/azure-ai-foundry/mcp-foundry)
- **DESCRIPTION**: A Model Context Protocol server for Azure AI Foundry, providing a unified set of tools for models, knowledge, evaluation, and more.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`
  
### <img height="18" width="18" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/1062064-Products-1.2-24x24" alt="Microsoft Azure DevOps Logo" /> Azure DevOps
- **REPOSITORY**: [Azure DevOps MCP Server - Public Preview](https://github.com/microsoft/azure-devops-mcp)
- **DESCRIPTION**: This TypeScript project provides a local MCP server for Azure DevOps, enabling you to perform a wide range of Azure DevOps tasks directly from your code editor.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`

### ☸️ Azure Kubernetes Service (AKS)
- **REPOSITORY**: [Azure/aks-mcp](https://github.com/Azure/aks-mcp)
- **DESCRIPTION**: An MCP server that enables AI assistants to interact with Azure Kubernetes Service (AKS) clusters. It serves as a bridge between AI tools and AKS, translating natural language requests into AKS operations and returning the results in a format the AI tools can understand.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`

### <img height="18" width="18" src="https://github.githubassets.com/assets/GitHub-Mark-ea2971cee799.png" alt="GitHub Logo" /> GitHub
- **REPOSITORY**: [github/github-mcp-server](https://github.com/github/github-mcp-server)
- **DESCRIPTION**: Access GitHub repositories, issues, and pull requests through secure API integration.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `REMOTE` - `https://api.githubcopilot.com/mcp`

### 📝 Markitdown
- **REPOSITORY**: [microsoft/markitdown](https://github.com/microsoft/markitdown)
- **DESCRIPTION**: A specialized MCP server for Markdown processing and manipulation. Enables AI models to read, write, and transform Markdown content with robust parsing and formatting capabilities.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
  
### 💻 Microsoft 365 Agents Toolkit
- **REPOSITORY**: [OfficeDev/microsoft-365-agents-toolkit](https://github.com/OfficeDev/microsoft-365-agents-toolkit/)
- **DESCRIPTION**: The Microsoft 365 Agents Toolkit MCP Server is a Model Context Protocol (MCP) server that provides a seamless connection between AI agents and developers for building apps and agents for Microsoft 365 and Microsoft 365 Copilot.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`

### 📊 Microsoft Clarity
- **REPOSITORY**: [microsoft/clarity-mcp-server](https://github.com/microsoft/clarity-mcp-server)
- **DESCRIPTION**: This is a Model Context Protocol (MCP) server for the Microsoft Clarity data export API. It allows you to fetch analytics data from Clarity using Claude for Desktop or other MCP-compatible clients.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`

### 🗃️ Microsoft Dataverse
- **REPOSITORY**: [Microsoft Dataverse](https://go.microsoft.com/fwlink/?linkid=2320176)
- **DESCRIPTION**: Chat over your business data using NL - Discover tables, run queries, retrieve data, insert or update records, and execute custom prompts grounded in business knowledge and context.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`

### 💻 Microsoft Dev Box
- **REPOSITORY**: [@microsoft/devbox-mcp](https://www.npmjs.com/package/@microsoft/devbox-mcp?activeTab=readme)
- **DESCRIPTION**: An MCP server for Microsoft Dev Box. Enables natural language interactions for developer-focused operations like managing Dev Boxes, configuring environments, and handling pools.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`

### 📁 Microsoft Files
- **REPOSITORY**: [microsoft/files-mcp-server](https://github.com/microsoft/files-mcp-server)
- **DESCRIPTION**: Provides a declarative control plane for managing file-based resources, supporting AI workflows that involve static files and documentation synchronization.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`

### 🛢️ Microsoft Fabric Real-Time Intelligence
- **REPOSITORY**: [RTI MCP Server](https://aka.ms/rti.mcp.repo)
- **DESCRIPTION**: This server enables AI agents to interact with Fabric RTI services by providing tools through the MCP interface, allowing for seamless data querying and analysis capabilities.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`

### 📚 Microsoft Learn
- **REPOSITORY**: [microsoftdocs/mcp](https://github.com/microsoftdocs/mcp)
- **DESCRIPTION**: AI assistant with real-time access to official Microsoft documentation.
- **CATEGORY**: `PRODUCTIVITY`
- **TYPE**: `REMOTE` - `https://learn.microsoft.com/api/mcp`

### 🛢️ Microsoft SQL
- **REPOSITORY**: [MSSQL MCP Server](https://aka.ms/MssqlMcp)
- **DESCRIPTION**: Chat with your business data the new agentic way using natural language and AI. Connect to any SQL database—from ground (on-premises) to Azure cloud to Microsoft Fabric via a simple connection string. Discover and define table schemas, manage tables, and perform CRUD operations through conversational prompts.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`

### 🎭 Playwright
- **REPOSITORY**: [microsoft/playwright-mcp](https://github.com/microsoft/playwright-mcp)
- **DESCRIPTION**: This server enables LLMs to interact with web pages through structured accessibility snapshots, bypassing the need for screenshots or visually-tuned models.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`


## 🏗️ Looking for starter templates that use MCP? 
Check out the [Azure Developer CLI (azd) templates](https://azure.github.io/awesome-azd/?tags=mcp) tagged with MCP.

## 📎 Related Resources
- [Microsoft MCP Resources](https://github.com/microsoft/mcp/tree/main/Resources)
- [MCP Pattern Overview](https://modelcontextprotocol.io/introduction)
- [MCP SDKs and Building Blocks](https://modelcontextprotocol.io/sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/specification/)

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
