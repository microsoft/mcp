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

## 📂 MCP Server Repositories

Below are Microsoft's official MCP server implementations:

### 🔷 Azure MCP Server

- **Repository**: [azure/azure-mcp](https://github.com/azure/azure-mcp)
- **Description**: Implements the MCP standard to manage Azure resources, enabling declarative provisioning and integration with AI workflows.

---

### 🧪 Playwright MCP

- **Repository**: [microsoft/playwright-mcp](https://github.com/microsoft/playwright-mcp)
- **Description**: An MCP server for orchestrating Playwright test runs, facilitating automated browser testing within AI-driven development pipelines.

---

### 📁 Files MCP Server

- **Repository**: [microsoft/files-mcp-server](https://github.com/microsoft/files-mcp-server)
- **Description**: Provides a declarative control plane for managing file-based resources, supporting AI workflows that involve static files and documentation synchronization.

---

## 🚀 Contributing

Interested in contributing a new MCP server or enhancing an existing one? Fork the relevant repository and submit a pull request!

---

## 📎 Related Resources

- [MCP Pattern Overview](https://modelcontextprotocol.io/introduction)
- [MCP SDKs and Building Blocks](https://modelcontextprotocol.io/sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/specification/2025-03-26/)

---

## 🛠 Maintainers

This list is curated by Microsoft engineers specializing in AI integrations and infrastructure automation. For suggestions or issues, please open an issue in the respective repository.



## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
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
