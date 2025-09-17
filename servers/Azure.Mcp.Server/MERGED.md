# <!--REMOVECHUNKSTART-Nuget;VSIX--><img height="36" width="36" src="https://cdn-dynmedia-1.microsoft.com/is/content/microsoftcorp/acom_social_icon_azure" alt="Microsoft Azure Logo" /> <!--REMOVECHUNKEND-->Azure MCP Server<!--INSERTCHUNK{{ToolTitle}}-->

The Azure MCP Server implements the [MCP specification](https://modelcontextprotocol.io) to create a seamless connection between AI agents and Azure services. Azure MCP Server can be used alone or with GitHub Copilot Extension for VSCode or InteliJ IDEA.  This project is in Public Preview and implementation may significantly change prior to our General Availability.

<!--REMOVESECTIONSTART-Nuget;VSIX;Npm-->
>[!WARNING]
>**Deprecation Notice: SSE transport mode has been removed in version [0.4.0 (2025-07-15)](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/CHANGELOG.md#040-2025-07-15).**
>
> SSE was deprecated in MCP `2025-03-26` due to [security vulnerabilities and architectural limitations](https://blog.fka.dev/blog/2025-06-06-why-mcp-deprecated-sse-and-go-with-streamable-http/). Users must discontinue use of SSE transport mode and upgrade to version `0.4.0` or newer to maintain compatibility with current MCP clients.
<!--REMOVESECTIONEND-->

## Table of Contents
- [Overview](#overview)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)

## Overview

**Azure MCP Server** provides smart, context-aware capabilities to GitHub Copilot to help you work more efficiently with Azure resources. The Azure MCP Server enables AI agents and clients with Azure context across **30+ different Azure services**.

## Getting Started

### Prerequisites

- You will need a supported IDE (Visual Studio Code or IntelliJ IDEA) with the GitHub Copilot extension/plugin installed.

    >|| Visual Studio Code | IntelliJ IDEA |
    >|-------|-----|-----|
    >|1| Install either the [Stable](https://code.visualstudio.com/download) or [Insiders](https://code.visualstudio.com/insiders) release of VS Code | Install either the [IntelliJ IDEA Ultimate](https://www.jetbrains.com/idea/download) or [IntelliJ IDEA Community](https://www.jetbrains.com/idea/download) edition. |
    >|2| Install the [GitHub Copilot](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot) and [GitHub Copilot Chat](https://marketplace.visualstudio.com/items?itemName=GitHub.copilot-chat) extensions | Install the [GitHub Copilot](https://plugins.jetbrains.com/plugin/17718-github-copilot) plugin. |

<!--REMOVESECTIONSTART-Npm;VSIX-->
- To use Azure MCP server from .NET, you must have [.NET 10 Preview 6 or later](https://dotnet.microsoft.com/download/dotnet/10.0) installed. This version of .NET adds a command, dnx, which is used to download, install, and run the MCP server from [nuget.org](https://www.nuget.org).
To verify your .NET version, run the following command in your terminal: `dotnet --info`
<!--REMOVESECTIONEND-->
<!--REMOVESECTIONSTART-Nuget;VSIX-->
- To use Azure MCP server from node you must have Node.js (LTS) installed — this provides both `npm` and `npx`. We recommend Node.js 20 LTS or later. To verify your installation run: `node --version`, `npm --version`, and `npx --version`.
<!--REMOVESECTIONEND-->

### Configuration

You can configure the Azure MCP Server either by installing the [Visual Studio Code extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server) (recommended for interactive configuration on vs code) or by updating the `mcp.json` configuration file directly in your IDE.

<!--REMOVESECTIONSTART-VSIX-->
<!--REMOVECHUNKSTART-Nuget;Npm--><details><!--REMOVECHUNKEND-->
<!--REMOVECHUNKSTART-Nuget;Npm--><summary>Find mcp.json file for your IDE</summary><!--REMOVECHUNKEND-->
<!--INSERTCHUNK{{#### Find mcp.json file for your IDE}}-->

>||| Find mcp.json in Visual Studio Code | Find mcp.json in IntelliJ IDEA |
>|-------|-----|-----|-----|
>|1| Open GitHub Copilot in your IDE | View > Chat  | Tools > GitHub Copilot > Open Chat  |
>|2| Switch to Agent Mode then click on the Tools Configuration button  |  ![VSCode](../../eng/images/VisualStudioCodeUI.png) | ![IntelliJ](../../eng/images/IntelliJIDEAUI.png)  |
>|2| Click on the button for configuring or adding tools  | Gear icon button if a previous `mcp.json` tool has been configured. Otherwise you can create a new `mcp.json` in your project | + Add More Tools |
</details>
<!--REMOVESECTIONEND-->

<!--REMOVESECTIONSTART-VSIX;Npm-->
<!--REMOVECHUNKSTART-Nuget--><details><!--REMOVECHUNKEND-->
<!--REMOVECHUNKSTART-Nuget--><summary>Configure Azure MCP Server using .NET Tool</summary><!--REMOVECHUNKEND-->
<!--INSERTCHUNK{{#### Configure Azure MCP Server in mcp.json}}-->

To use the latest version enter the following snippet in your `mcp.json`
```json
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

You can also specific a version using the --version argument, like so:

```json
"servers": {
  "Azure MCP Server": {
    "command": "dnx",
    "args": [
      "Azure.Mcp",
      "--source",
      "https://api.nuget.org/v3/index.json",
      "--version",
      "<version>",
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
</details>
<!--REMOVESECTIONEND-->

<!--REMOVESECTIONSTART-VSIX;Nuget-->
<!--REMOVECHUNKSTART-Npm--><details><!--REMOVECHUNKEND-->
<!--REMOVECHUNKSTART-Npm--><summary>Configure Azure MCP Server using node tool</summary><!--REMOVECHUNKEND-->
<!--INSERTCHUNK{{#### Configure Azure MCP Server in mcp.json}}-->

To use the latest version enter the following snippet in your mcp.json

```json
"servers": {
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
```

You can also install a targeted version

```json
"servers": {
  "azure-mcp-server": {
    "command": "npx",
    "args": [
      "-y",
      "@azure/mcp@<version>",
      "server",
      "start"
    ]
  }
}
```
</details>
<!--REMOVESECTIONEND-->

<!--REMOVECHUNKSTART-Npm;Nuget;VSIX--><details><!--REMOVECHUNKEND-->
<!--REMOVECHUNKSTART-Npm;Nuget;VSIX--><summary>Start (or Auto-Start) the MCP Server</summary><!--REMOVECHUNKEND-->
<!--INSERTCHUNK{{#### Start (or Auto-Start) the MCP Server}}-->


</details>
