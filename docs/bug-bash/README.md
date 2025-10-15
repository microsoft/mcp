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
- **Installation and setup** across different IDEs
- **Performance and stability** under real-world usage
- **Authentication** across different environments
- **End-to-end scenarios** that developers commonly encounter

## Bug Bash Goals

The primary goals of this bug bash are to:

1. **Validate cross-platform compatibility** - Ensure the server works reliably on Windows, macOS, and Linux
2. **Verify installation experience** - Test installation across VS Code, Visual Studio, and IntelliJ IDEA
3. **Assess performance** - Monitor memory consumption and CPU usage under typical workloads
4. **Validate authentication** - Ensure auth works consistently across all platforms
5. **Test server modes** - Verify single, namespace, and all modes work as expected
6. **Validate feature flags** - Test enabling/disabling server features
7. **Exercise real-world scenarios** - Run through common developer workflows

## What to Test

We encourage you to test the following areas:

### Platform Testing
- [ ] **Windows** - Test on Windows 10/11
- [ ] **macOS** - Test on macOS (Intel and Apple Silicon)
- [ ] **Linux** - Test on Ubuntu, Fedora, or other distributions

### IDE Installation
- [ ] **VS Code** - Stable and Insiders versions
- [ ] **Visual Studio 2022** - Community, Professional, or Enterprise
- [ ] **IntelliJ IDEA** - Ultimate or Community editions

### Performance Monitoring
- [ ] Monitor **memory consumption** during typical operations
- [ ] Monitor **CPU usage** during command execution
- [ ] Test with **multiple concurrent operations**
- [ ] Observe behavior during **long-running sessions**

### Authentication Testing
- [ ] Test **Azure CLI authentication** (`az login`)
- [ ] Test **Azure PowerShell authentication** (`ConnectAzAccount`)
- [ ] Test **Interactive browser authentication**
- [ ] Test authentication across **multiple tenants**
- [ ] Test authentication with **service principals**

### Server Mode Testing
- [ ] **All mode** - All tools exposed individually
- [ ] **Namespace mode** - Tools grouped by Azure service
- [ ] **Single mode** - Single dynamic proxy tool
- [ ] **Filtered namespaces** - Specific services only

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
- **IDE**: VS Code/Visual Studio/IntelliJ (include version)
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
9. **[Agent Building](scenarios/agent-building.md)** - Create and deploy Azure Foundry agents

### Quick Start Scenarios

If you're short on time, try these quick scenarios:

- **5 minutes**: Install Azure MCP extension and verify tools are loaded
- **10 minutes**: List your Azure resources (subscriptions, resource groups, storage accounts)
- **15 minutes**: Create a simple storage account and upload a file
- **30 minutes**: Create a basic web app and deploy it to Azure App Service

## Resources

### Documentation
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