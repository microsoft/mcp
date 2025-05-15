# Microsoft Model Context Protocol (MCP) Servers

This repository catalogs various Microsoft implementations of the Model Context Protocol (MCP), an open standard that facilitates seamless integration between AI applications and external data sources and tools. MCP enables AI models to access the context they need to perform tasks effectively.

---

## 📘 What is MCP?

**Model Context Protocol (MCP)** is an open protocol that standardizes how applications provide context to large language models (LLMs). It allows AI applications to connect with various data sources and tools in a consistent manner, enhancing their capabilities and flexibility. MCP follows a client-server architecture where:

- **MCP Hosts**: Applications like AI assistants or integrated development environments (IDEs) that initiate connections.
- **MCP Clients**: Connectors within the host application that maintain 1:1 connections with servers.
- **MCP Servers**: Services that provide context and capabilities through the standardized MCP.

For more details, visit the [official MCP website](https://modelcontextprotocol.io).

---

## 📂 Microsoft MCP Servers

Below are Microsoft's official MCP server implementations:

### 🔷 Azure MCP Server

- **Repository**: [azure/azure-mcp](https://github.com/azure/azure-mcp)
- **Description**: Implements the MCP standard to manage Azure resources, enabling declarative provisioning and integration with AI workflows.

---

### 🧪 Playwright MCP

- **Repository**: [microsoft/playwright-mcp](https://github.com/microsoft/playwright-mcp)
- **Description**: An MCP server for browsing the internet. Enables LLMs to interact with web pages through structured accessibility snapshots. Useful for web navigation and form-filling, data extraction from structured content, automated testing driven by LLMs, and general-purpose browser interaction for agents.

---

### 📁 Files MCP Server

- **Repository**: [microsoft/files-mcp-server](https://github.com/microsoft/files-mcp-server)
- **Description**: Provides a declarative control plane for managing file-based resources, supporting AI workflows that involve static files and documentation synchronization.

---

### 📝 Markitdown MCP Server

- **Repository**: [microsoft/markitdown](https://github.com/microsoft/markitdown)
- **Description**: A specialized MCP server for Markdown processing and manipulation. Enables AI models to read, write, and transform Markdown content with robust parsing and formatting capabilities.

---

### 📊 Clarity MCP Server

- **Repository**: [@microsoft/clarity-mcp-server](https://www.npmjs.com/package/@microsoft/clarity-mcp-server)
- **Description**: An MCP server for Microsoft Clarity analytics integration. Enables AI models to access web analytics data, heatmaps, and session recordings to understand user behavior and site performance.

---

## 📎 Related Resources

- [Microsoft MCP Resources](https://github.com/microsoft/mcp/tree/main/Resources)
- [MCP Pattern Overview](https://modelcontextprotocol.io/introduction)
- [MCP SDKs and Building Blocks](https://modelcontextprotocol.io/sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/specification/2025-03-26/)

---

## 🏗️ Templates

Looking for starter templates that use MCP? Check out the [Azure Developer CLI (azd) templates](https://azure.github.io/awesome-azd/?tags=mcp) tagged with MCP.

---

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
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
