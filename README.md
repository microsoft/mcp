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
[Azure MCP Releases]: https://github.com/microsoft/mcp/releases?q=Azure
[Azure MCP Documentation]: https://learn.microsoft.com/azure/developer/azure-mcp-server/
[Azure MCP Troubleshooting]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md
[Azure MCP Support]: https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/SUPPORT.md


## 📚 Looking for the full list of MCP Servers from Microsoft?
See the [Microsoft MCP Server Catalog](https://github.com/microsoft/mcp/blob/main/CATALOG.md) for all the latest MCP Servers, including public preview and experimental servers.

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
