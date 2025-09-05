# Microsoft MCP Servers



Welcome to the Microsoft MCP Servers repository! This repo catalogs official and community MCP Server implementations that enable AI agents to connect with external data, tools, and services.

---

## 📑 Table of Contents

- [What is MCP?](#what-is-mcp)
- [Featured MCP Servers](#featured-mcp-servers)
  - [Azure MCP Server](#azure-mcp-server)
  - [Microsoft Fabric MCP Server](#microsoft-fabric-mcp-server)
  - [Request a New MCP Server](#request-a-new-mcp-server)
- [Microsoft MCP Server Catalog](#mcp-server-catalog)
- [Related Resources](#related-resources)
- [Templates](#templates)
- [Contributing](#contributing)
- [Trademarks](#trademarks)

---

## 📘 What is MCP?

**Model Context Protocol (MCP)** is an open protocol that standardizes how applications provide context to large language models (LLMs). It allows AI applications to connect with various data sources and tools in a consistent manner, enhancing their capabilities and flexibility. MCP follows a client-server architecture:

- **MCP Hosts**: Applications like AI assistants or IDEs that initiate connections.
- **MCP Clients**: Connectors within the host application that maintain 1:1 connections with servers.
- **MCP Servers**: Services that provide context and capabilities through the standardized MCP.

For more details, visit the [official MCP website](https://modelcontextprotocol.io).

---

## 🌟 Featured MCP Servers

### 🔷 [Azure MCP Server](servers/Azure.Mcp.Server/README.md)

Implements the MCP standard to manage Azure resources, enabling declarative provisioning and integration with AI workflows. See the [Azure MCP Server README](servers/Azure.Mcp.Server/README.md) for install instructions, usage, and troubleshooting.

### 🛢️ [Microsoft Fabric MCP Server](servers/Fabric.Mcp.Server/README.md)

Provides MCP-based access to Microsoft Fabric Real-Time Intelligence and related services. See the [Microsoft Fabric MCP Server README](servers/Fabric.Mcp.Server/README.md) for details.

---

## 📚 Microsoft MCP Server Catalog

Looking for the full list of MCP Servers? See the [Microsoft MCP Server Catalog](CATALOG.md) for all the latest MCP Servers from Microsoft, including public preview and experimental servers.

---

## 🆕 Request a New MCP Server

Want to add a new MCP Server to this repository? [Open an MCP Server Request](issues/new?template=02_mcp_server_request.yml) using our issue template.

---

## 📎 Related Resources

- [Microsoft MCP Resources](https://github.com/microsoft/mcp/tree/main/Resources)
- [MCP Pattern Overview](https://modelcontextprotocol.io/introduction)
- [MCP SDKs and Building Blocks](https://modelcontextprotocol.io/sdk)
- [MCP Specification](https://spec.modelcontextprotocol.io/specification/)

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
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
