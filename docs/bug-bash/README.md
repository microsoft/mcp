# Azure MCP Server - Bug Bash Welcome Guide

Welcome to the Azure MCP Server Bug Bash! We're excited to have you help us improve the quality and reliability of Azure MCP Server across different platforms and scenarios.

## Table of Contents

- [Introduction](#introduction)
- [Bug Bash Goals](#bug-bash-goals)
- [What to Test](#what-to-test)
- [How to Report Issues](#how-to-report-issues)
- [Testing Scenarios](#testing-scenarios)
- [Resources](#resources)

## Introduction

The Azure MCP Server enables AI agents to interact with Azure services through natural language commands. As we continue to enhance the server, we need your help to identify issues across different platforms, IDEs, and usage scenarios.

This bug bash focuses on:
- **Multi-platform compatibility** (Windows, macOS, Linux)
- **Installation and setup** across different IDEs and package managers
- **Resource discovery and inspection** - testing how well MCP finds and retrieves information
- **Querying capabilities** - testing database queries and data retrieval
- **Monitoring and diagnostics** - testing log/metric access and health checks
- **Deployment guidance** - testing how MCP helps plan and guide deployments
- **Authentication** across different environments
- **End-to-end scenarios** that developers commonly encounter

## Bug Bash Goals

The primary goals of this bug bash are to:

1. **Exercise real-world scenarios** - Run through common developer workflows
2. **Validate cross-platform compatibility** - Ensure the server works reliably on Windows, macOS, and Linux
3. **Verify installation experience** - Test installation across VS Code, Visual Studio, and IntelliJ IDEA
4. **Assess performance** - Monitor memory consumption and CPU usage under typical workloads
5. **Validate authentication** - Ensure auth works consistently across all platforms
6. **Test server modes** - Verify single, namespace, and all modes work as expected
7. **Validate feature flags** - Test enabling/disabling server features

## What to Test

We encourage you to test the following areas:

### Platform Testing
- [ ] **Windows** - Test on Windows 11
- [ ] **macOS** - Test on macOS (Intel and Apple Silicon)
- [ ] **Linux** - Test on Ubuntu, Fedora, or other distributions

### IDE Installation
- [ ] **VS Code** - Stable and Insiders versions
- [ ] **Visual Studio 2022** - Community, Professional, or Enterprise
- [ ] **IntelliJ IDEA** - Ultimate or Community editions (2025.2+)
- [ ] **Claude Desktop** - macOS and Windows
- [ ] **Cursor** - AI-first code editor
- [ ] **Windsurf** - Codeium Cascade editor
- [ ] **Amazon Q Developer** - AWS IDE integration
- [ ] **Claude Code** - Web-based Claude interface

### Performance Monitoring

**How to Monitor Performance:**

**Windows:**
- Open Task Manager (Ctrl+Shift+Esc)
- Find `azmcp.exe` process
- Monitor Memory and CPU columns during operations

**macOS:**
- Open Activity Monitor (Applications → Utilities → Activity Monitor)
- Search for `azmcp` process
- Monitor CPU % and Memory columns

**Linux:**
- Use `htop` or `top` command
- Filter for `azmcp` process
- Monitor %CPU and RES (memory) columns

**What to Test:**
- [ ] Monitor **memory consumption** during typical operations
- [ ] Monitor **CPU usage** during command execution
- [ ] Test with **multiple concurrent operations**
- [ ] Observe behavior during **long-running sessions** (2+ hours):
  - Memory usage trends (stable, growing, or leaking)
  - Server responsiveness (does it slow down over time?)
  - Error frequency (do errors increase with time?)
- [ ] Record baseline memory usage at startup
- [ ] Check for memory leaks after extended use

### Authentication Testing
- [ ] Test **Azure CLI authentication** (`az login`)
- [ ] Test **Azure PowerShell authentication** (`Connect-AzAccount`)
- [ ] Test **Interactive browser authentication** - Set `AZURE_MCP_ONLY_USE_BROKER_CREDENTIAL=true` and sign in through the broker/browser when innvoking tools via Azure MCP Server. See [Authentication Guide](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md#authentication-fundamentals) for details.
- [ ] Test authentication across **multiple tenants** - Switch between different Azure AD tenants

### Server Mode Testing

See [Server Mode Testing](installation-testing.md#server-mode-testing) for detailed instructions.

- [ ] **Namespace mode** (default) - Tools grouped by Azure service (~40-50 tools)
- [ ] **All mode** - All tools exposed individually (100+ tools)
- [ ] **Single mode** - Single unified tool with internal routing
- [ ] **Read-only mode** - Blocks all write/destructive operations
- [ ] **Namespace filtering** - Expose specific services only (e.g., storage, keyvault)

### Feature Flag Testing
- [ ] Enable/disable server
- [ ] Test read-only mode
- [ ] Test with different namespace configurations
- [ ] Test tool filtering

## How to Report Issues

When you find a bug or issue, please report it on GitHub:

### Report Issues Here: [https://github.com/microsoft/mcp/issues](https://github.com/microsoft/mcp/issues)

**Steps to Report:**

1. Go to the [issues page](https://github.com/microsoft/mcp/issues)
2. Click **"New Issue"**
3. Select **"Azure MCP - Bug Bash Report"** template
4. Fill in the following information:

**Required Information:**
- **Platform**: Windows/macOS/Linux (include version)
- **IDE/Client**: VS Code/Visual Studio/IntelliJ/Claude Desktop/Cursor/Windsurf/Amazon Q/Claude Code (include version)
- **Azure MCP Server Version**: Found in extension details or `azmcp --version`
- **Node.js Version** (if using npm): Run `node --version`
- **Description**: Clear description of the issue
- **Steps to Reproduce**: Detailed steps to reproduce the problem
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Logs**: Include relevant error messages or logs (see [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md#logging-and-diagnostics))
- **Screenshots**: If applicable, include screenshots

## Testing Scenarios

We've prepared detailed testing guides for common scenarios:

### Scenario Guides

1. **[Installation Testing](installation-testing.md)** - Test installation across different platforms and IDEs
2. **[Infrastructure as Code](scenarios/infra-as-code.md)** - Generate and deploy Azure infrastructure
3. **[PaaS Services](scenarios/paas-services.md)** - Work with App Service, Container Apps, and Functions
4. **[Storage Operations](scenarios/storage-operations.md)** - Test blob storage and file operations
5. **[Database Operations](scenarios/database-operations.md)** - Work with Cosmos DB, PostgreSQL, and Azure SQL
6. **[Deployment Scenarios](scenarios/deployment.md)** - Deploy resources and applications
7. **[Full Stack Applications](scenarios/full-stack-apps.md)** - Build complete apps with database backends
8. **[Agent Building](scenarios/agent-building.md)** - Create and deploy Azure Foundry agents

### Quick Start Scenarios

If you're short on time, try these quick scenarios:

**5 minutes: Install and Verify**
- Install Azure MCP extension/server in your IDE
- Verify tools are loaded
- Try these prompts:
  - `"What Azure MCP tools are available?"`
  - `"Show me my subscriptions"`
  - `"List all resource groups in my subscription"`

**10 minutes: Resource Discovery**
- List your Azure resources (subscriptions, resource groups, storage accounts)
- Try these prompts:
  - `"List all storage accounts in my subscription"`
  - `"Show me my Key Vaults"`
  - `"List all App Services in resource group <name>"`

**15 minutes: Database Inspection**
- Inspect existing databases and query data
- Try these prompts:
  - `"List all cosmosdb accounts in my subscription"`
  - `"Show me databases in cosmosdb account <name>"`
  - `"List all containers in database <name> in cosmosdb account <name>"`

**30 minutes: End-to-End Scenario**
- Follow one of the [detailed scenario guides](scenarios/) like Storage Operations or Database Operations

## Resources

### Bug Bash Documentation
- [Installation Testing Guide](installation-testing.md) - Test installation across platforms and IDEs
- [Testing Scenarios](scenarios/) - Detailed end-to-end testing scenarios

### Azure MCP Server Documentation
- [Azure MCP Server Documentation](https://learn.microsoft.com/azure/developer/azure-mcp-server/)
- [Installation Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md#installation)
- [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md)
- [Authentication Guide](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md)
- [Command Reference](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/azmcp-commands.md)

### Test Prompts
- [E2E Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md) - Sample prompts for testing

### Support
- [GitHub Issues](https://github.com/microsoft/mcp/issues) - Report bugs and issues
- [Discussions](https://github.com/microsoft/mcp/discussions) - Ask questions and share feedback