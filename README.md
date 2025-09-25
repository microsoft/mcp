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
| Microsoft Fabric MCP | [Fabric MCP README]  | [Fabric MCP Source Code] | [Fabric MCP CHANGELOG] | [Fabric MCP Releases] | [Fabric Documentation] | [Fabric MCP Troubleshooting] | [Fabric MCP Support] |

[Azure MCP README]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md
[Azure MCP CHANGELOG]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/CHANGELOG.md
[Azure MCP Source Code]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server
[Azure MCP Releases]: https://github.com/microsoft/mcp/releases?q=Azure.Mcp.Server-0
[Azure MCP Documentation]: https://learn.microsoft.com/azure/developer/azure-mcp-server/
[Azure MCP Troubleshooting]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md
[Azure MCP Support]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/SUPPORT.md

[Fabric MCP README]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/README.md
[Fabric MCP CHANGELOG]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/CHANGELOG.md
[Fabric MCP Source Code]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server
[Fabric MCP Releases]: https://github.com/microsoft/mcp/releases?q=Fabric.Mcp.Server-0
[Fabric Documentation]: https://learn.microsoft.com/fabric/
[Fabric MCP Troubleshooting]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/TROUBLESHOOTING.md
[Fabric MCP Support]: https://github.com/microsoft/mcp/blob/main/servers/Fabric.Mcp.Server/SUPPORT.md


## 📚 Which MCP Servers are available from Microsoft?

### <img height="18" width="18" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> Azure
- **REPOSITORY**: [microsoft/mcp](https://github.com/microsoft/mcp/tree/main/servers/Azure.Mcp.Server#readme)
- **DESCRIPTION**: All Azure MCP tools in a single server.  The Azure MCP Server implements the MCP specification to create a seamless connection between AI agents and Azure services.  Azure MCP Server can be used alone or with the GitHub Copilot for Azure extension in VS Code.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`
- **INSTALL**: [![Install Azure MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-azuretools.vscode-azure-mcp-server) [![Install Azure MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Azure_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://marketplace.visualstudio.com/items?itemName=github-copilot-azure.GitHubCopilotForAzure2022)

### ✨ Azure AI Foundry
- **REPOSITORY**: [azure-ai-foundry/mcp-foundry](https://github.com/azure-ai-foundry/mcp-foundry)
- **DESCRIPTION**: A Model Context Protocol server for Azure AI Foundry, providing a unified set of tools for models, knowledge, evaluation, and more.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`
- **INSTALL**: [![Install Azure AI Foundry MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_AI_Foundry_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=ffffff)](https://vscode.dev/redirect?url=vscode:mcp/install?%7B%22name%22%3A%22ai_foundry_server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22--prerelease%3Dallow%22%2C%22--from%22%2C%22git%2Bhttps%3A%2F%2Fgithub.com%2Fazure-ai-foundry%2Fmcp-foundry.git%22%2C%22run-azure-ai-foundry-mcp%22%2C%22--envFile%22%2C%22%24%7BworkspaceFolder%7D%2F.env%22%5D%7D) [![Install Azure AI Foundry in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_AI_Foundry_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=ffffff)](https://vscode.dev/redirect?url=vscode-insiders:mcp/install?%7B%22name%22%3A%22ai_foundry_server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22--prerelease%3Dallow%22%2C%22--from%22%2C%22git%2Bhttps%3A%2F%2Fgithub.com%2Fazure-ai-foundry%2Fmcp-foundry.git%22%2C%22run-azure-ai-foundry-mcp%22%2C%22--envFile%22%2C%22%24%7BworkspaceFolder%7D%2F.env%22%5D%7D) [![Install Azure AI Foundry in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Azure_AI_Foundry_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22ai_foundry_server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22--prerelease%3Dallow%22%2C%22--from%22%2C%22git%2Bhttps%3A%2F%2Fgithub.com%2Fazure-ai-foundry%2Fmcp-foundry.git%22%2C%22run-azure-ai-foundry-mcp%22%2C%22--envFile%22%2C%22%24%7BworkspaceFolder%7D%2F.env%22%5D%7D)

### <img height="18" width="18" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/1062064-Products-1.2-24x24" alt="Microsoft Azure DevOps Logo" /> Azure DevOps
- **REPOSITORY**: [Azure DevOps MCP Server - Public Preview](https://github.com/microsoft/azure-devops-mcp)
- **DESCRIPTION**: This TypeScript project provides a local MCP server for Azure DevOps, enabling you to perform a wide range of Azure DevOps tasks directly from your code editor.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Azure DevOps in VS Code](https://img.shields.io/badge/VS_Code-Install_Azure_DevOps_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=ado&type=stdio&command=npx&args=%5B%22-y%22%2C%22%40azure-devops%2Fmcp%22%2C%22%24%7Binput%3Aado_org%7D%22%5D&inputs=%5B%7B%22id%22%3A%22ado_org%22%2C%22type%22%3A%22promptString%22%2C%22description%22%3A%22Azure%20DevOps%20organization%20name%20(e.g.%20contoso)%22%7D%5D) [![Install Azure DevOps in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Azure_Devops_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=ado&quality=insiders&type=stdio&command=npx&args=%5B%22-y%22%2C%22%40azure-devops%2Fmcp%22%2C%22%24%7Binput%3Aado_org%7D%22%5D&inputs=%5B%7B%22id%22%3A%22ado_org%22%2C%22type%22%3A%22promptString%22%2C%22description%22%3A%22Azure%20DevOps%20organization%20name%20(e.g.%20contoso)%22%7D%5D) [![Install Azure DevOps in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Azure_DevOps_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://github.com/microsoft/azure-devops-mcp/blob/main/docs/GETTINGSTARTED.md#%EF%B8%8F-visual-studio-2022--github-copilot)


### ☸️ Azure Kubernetes Service (AKS)
- **REPOSITORY**: [Azure/aks-mcp](https://github.com/Azure/aks-mcp)
- **DESCRIPTION**: An MCP server that enables AI assistants to interact with Azure Kubernetes Service (AKS) clusters. It serves as a bridge between AI tools and AKS, translating natural language requests into AKS operations and returning the results in a format the AI tools can understand.
- **CATEGORY**: `CLOUD AND INFRASTRUCTURE`
- **TYPE**: `Local`
- **INSTALL**: [![Install AKS MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_AKS_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode:extension/ms-kubernetes-tools.vscode-aks-tools) [![Install AKS MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_AKS_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/ms-kubernetes-tools.vscode-aks-tools) [![Install AKS MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_AKS_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://github.com/Azure/aks-mcp)

### <img height="18" width="18" src="https://github.githubassets.com/assets/GitHub-Mark-ea2971cee799.png" alt="GitHub Logo" /> GitHub
- **REPOSITORY**: [github/github-mcp-server](https://github.com/github/github-mcp-server)
- **DESCRIPTION**: Access GitHub repositories, issues, and pull requests through secure API integration.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `REMOTE` - `https://api.githubcopilot.com/mcp`
- **INSTALL**: [![Install GitHub MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_GitHub_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=github&config=%7B%22type%22%3A%20%22http%22%2C%22url%22%3A%20%22https%3A%2F%2Fapi.githubcopilot.com%2Fmcp%2F%22%7D) [![Install GitHub MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_GitHub_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=github&config=%7B%22type%22%3A%20%22http%22%2C%22url%22%3A%20%22https%3A%2F%2Fapi.githubcopilot.com%2Fmcp%2F%22%7D&quality=insiders) [![Install GitHub MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_GitHub_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22github%22%2C%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Fapi.githubcopilot.com%2Fmcp%2F%22%7D)

### <img height="18" width="18" src="https://github.githubassets.com/assets/GitHub-Mark-ea2971cee799.png" alt="GitHub Logo" /> GitHub Awesome-Copilot
- **REPOSITORY**: [github/awesome-copilot](https://github.com/github/awesome-copilot)
- **DESCRIPTION**: Community-contributed instructions, prompts, and configurations to help you make the most of GitHub Copilot.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Awesome Copilot MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Awesome_Copilot_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://aka.ms/awesome-copilot/mcp/vscode) [![Install Awesome Copilot MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Awesome_Copilot_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://aka.ms/awesome-copilot/mcp/vscode-insiders) [![Install in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Awesome_Copilot_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/awesome-copilot/mcp/vs)

### 📝 Markitdown
- **REPOSITORY**: [microsoft/markitdown](https://github.com/microsoft/markitdown)
- **DESCRIPTION**: A specialized MCP server for Markdown processing and manipulation. Enables AI models to read, write, and transform Markdown content with robust parsing and formatting capabilities.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Markitdown MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Markitdown_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=ffffff)](https://vscode.dev/redirect?url=vscode:mcp/install?%7B%22name%22%3A%22markitdown%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22markitdown-mcp%22%5D%7D) [![Install Markitdown MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Markitdown_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=ffffff)](https://vscode.dev/redirect?url=vscode-insiders:mcp/install?%7B%22name%22%3A%22markitdown%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22markitdown-mcp%22%5D%7D) [![Install Markitdown MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Markitdown_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22markitdown%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22markitdown-mcp%22%5D%7D)
  
### 💻 Microsoft 365 Agents Toolkit
- **REPOSITORY**: [OfficeDev/microsoft-365-agents-toolkit](https://github.com/OfficeDev/microsoft-365-agents-toolkit/)
- **DESCRIPTION**: The Microsoft 365 Agents Toolkit MCP Server is a Model Context Protocol (MCP) server that provides a seamless connection between AI agents and developers for building apps and agents for Microsoft 365 and Microsoft 365 Copilot.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Microsoft 365 Agents Toolkit in VS Code](https://img.shields.io/badge/VS_Code-Install_Microsoft_365_Agents_Toolkit-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode:extension/TeamsDevApp.ms-teams-vscode-extension) [![Install Microsoft 365 Agents Toolkit in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Microsoft_365_Agents_Toolkit-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect?url=vscode-insiders:extension/TeamsDevApp.ms-teams-vscode-extension)

### 📊 Microsoft Clarity
- **REPOSITORY**: [microsoft/clarity-mcp-server](https://github.com/microsoft/clarity-mcp-server)
- **DESCRIPTION**: This is a Model Context Protocol (MCP) server for the Microsoft Clarity data export API. It allows you to fetch analytics data from Clarity using Claude for Desktop or other MCP-compatible clients.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`
- **INSTALL**: [microsoft/clarity-mcp-server](https://github.com/microsoft/clarity-mcp-server)

### 🗃️ Microsoft Dataverse
- **REPOSITORY**: [Microsoft Dataverse](https://go.microsoft.com/fwlink/?linkid=2320176)
- **DESCRIPTION**: Chat over your business data using NL - Discover tables, run queries, retrieve data, insert or update records, and execute custom prompts grounded in business knowledge and context.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`
- **INSTALL**: [Microsoft Dataverse](https://go.microsoft.com/fwlink/?linkid=2320176)

### 💻 Microsoft Dev Box
- **REPOSITORY**: [@microsoft/devbox-mcp](https://www.npmjs.com/package/@microsoft/devbox-mcp?activeTab=readme)
- **DESCRIPTION**: An MCP server for Microsoft Dev Box. Enables natural language interactions for developer-focused operations like managing Dev Boxes, configuring environments, and handling pools.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Dev Box MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Dev_Box_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=DevBox&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40microsoft%2Fdevbox-mcp%40latest%22%5D%7D) [![Install Dev Box MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Dev_Box_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=DevBox&config=%7B%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40microsoft%2Fdevbox-mcp%40latest%22%5D%7D&quality=insiders) [![Install Dev Box MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Dev_Box_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22DevBox%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40microsoft%2Fdevbox-mcp%40latest%22%5D%7D)

### <img height="18" width="18" src="https://learn.microsoft.com/fabric/media/fabric-icon.png" alt="Microsoft Fabric Logo" /> Microsoft Fabric (Public Preview)
- **REPOSITORY**: [microsoft/mcp](https://github.com/microsoft/mcp/tree/main/servers/Fabric.Mcp.Server#readme)
- **DESCRIPTION**: A local-first MCP server providing AI agents with comprehensive access to Microsoft Fabric's public APIs, item definitions, and best practices. Enables AI-assisted development for all Fabric workloads without connecting to live environments.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`
- **INSTALL**: [microsoft/mcp](https://github.com/microsoft/mcp/tree/main/servers/Fabric.Mcp.Server#readme)

### 🛢️ Microsoft Fabric Real-Time Intelligence
- **REPOSITORY**: [RTI MCP Server](https://aka.ms/rti.mcp.repo)
- **DESCRIPTION**: This server enables AI agents to interact with Fabric RTI services by providing tools through the MCP interface, allowing for seamless data querying and analysis capabilities.
- **CATEGORY**: `DATA AND ANALYTICS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Fabric RTI MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Microsoft_Fabric_RTI_MCP_Server-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=ms-fabric-rti&config=%7B%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22microsoft-fabric-rti-mcp%22%5D%7D) [![Install Fabric RTI MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Microsoft_Fabric_RTI_MCP_Server-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=ms-fabric-rti&config=%7B%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22microsoft-fabric-rti-mcp%22%5D%7D&quality=insiders) [![Install Fabric RTI MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Microsoft_Fabric_RTI_MCP_Server-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22ms-fabric-rti%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22uvx%22%2C%22args%22%3A%5B%22microsoft-fabric-rti-mcp%22%5D%7D)

### 📁 Microsoft Files
- **REPOSITORY**: [microsoft/files-mcp-server](https://github.com/microsoft/files-mcp-server)
- **DESCRIPTION**: Provides a declarative control plane for managing file-based resources, supporting AI workflows that involve static files and documentation synchronization.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [microsoft/files-mcp-server](https://github.com/microsoft/files-mcp-server)

### 📚 Microsoft Learn
- **REPOSITORY**: [microsoftdocs/mcp](https://github.com/microsoftdocs/mcp)
- **DESCRIPTION**: AI assistant with real-time access to official Microsoft documentation.
- **CATEGORY**: `PRODUCTIVITY`
- **TYPE**: `REMOTE` - `https://learn.microsoft.com/api/mcp`
- **INSTALL**: [![Install Microsoft Learn MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Microsoft_Docs_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=microsoft.docs.mcp&config=%7B%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Flearn.microsoft.com%2Fapi%2Fmcp%22%7D) [![Install Microsoft Learn MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Microsoft_Docs_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=microsoft.docs.mcp&config=%7B%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Flearn.microsoft.com%2Fapi%2Fmcp%22%7D&quality=insiders) [![Install Microsoft Learn MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Microsoft_Docs_MCP-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22microsoft.docs.mcp%22%2C%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Flearn.microsoft.com%2Fapi%2Fmcp%22%7D)

### 🛢️ Microsoft SQL
- **REPOSITORY**: [MSSQL MCP Server](https://aka.ms/MssqlMcp)
- **DESCRIPTION**: Chat with your business data the new agentic way using natural language and AI. Connect to any SQL database—from ground (on-premises) to Azure cloud to Microsoft Fabric via a simple connection string. Discover and define table schemas, manage tables, and perform CRUD operations through conversational prompts.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [MSSQL MCP Server](https://aka.ms/MssqlMcp)

### 💻 NuGet MCP Server
- **REPOSITORY**: [NuGet/Home](https://github.com/NuGet/Home)
- **DESCRIPTION**: This is a Model Context Protocol (MCP) server for NuGet, enabling advanced tooling and automation scenarios for NuGet package management.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [Nuget MCP Server](https://www.nuget.org/packages/NuGet.Mcp.Server)

### 🎭 Playwright
- **REPOSITORY**: [microsoft/playwright-mcp](https://github.com/microsoft/playwright-mcp)
- **DESCRIPTION**: This server enables LLMs to interact with web pages through structured accessibility snapshots, bypassing the need for screenshots or visually-tuned models.
- **CATEGORY**: `DEVELOPER TOOLS`
- **TYPE**: `Local`
- **INSTALL**: [![Install Playwright MCP in VS Code](https://img.shields.io/badge/VS_Code-Install_Playwright_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect?url=vscode%3Amcp%2Finstall%3F%257B%2522name%2522%253A%2522playwright%2522%252C%2522command%2522%253A%2522npx%2522%252C%2522args%2522%253A%255B%2522%2540playwright%252Fmcp%2540latest%2522%255D%257D) [![Install Playwright MCP in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_Playwright_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect?url=vscode-insiders%3Amcp%2Finstall%3F%257B%2522name%2522%253A%2522playwright%2522%252C%2522command%2522%253A%2522npx%2522%252C%2522args%2522%253A%255B%2522%2540playwright%252Fmcp%2540latest%2522%255D%257D) [![Install Playwright MCP in Visual Studio](https://img.shields.io/badge/Visual_Studio-Install_Playwright_MCP-C16FDE?style=flat-square&logo=visualstudio&logoColor=white)](https://aka.ms/vs/mcp-install?%7B%22name%22%3A%22playwright%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22%40playwright%2Fmcp%40latest%22%5D%7D)


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
