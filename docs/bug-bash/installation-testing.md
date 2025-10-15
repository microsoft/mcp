# Installation Testing Guide

This guide covers testing Azure MCP Server installation across different platforms and IDEs.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Platform-Specific Testing](#platform-specific-testing)
- [IDE Installation Testing](#ide-installation-testing)
- [Verification Steps](#verification-steps)

## Prerequisites

Before testing installation, ensure you have:

- [ ] **Azure Subscription** - Access to an Azure subscription

## Platform-Specific Testing

### Windows Testing

#### Installation Steps

1. **Install Prerequisites**
   ```powershell
   # Check if Node.js is installed
   node --version
   
   # If not installed, download from https://nodejs.org/
   ```

2. **Install via VS Code Extension** (Recommended)
   - Open VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "Azure MCP Server"
   - Click Install

3. **Install via NPM** (Alternative)
   ```powershell
   npm install -g @azure/mcp@latest
   ```

4. **Install via NuGet** (Alternative)
   ```powershell
   dotnet tool install Azure.Mcp
   ```

#### Verification
```powershell
# Check Azure MCP version
azmcp --version

# Verify Azure CLI
az --version

# Test basic command
azmcp server start --help
```

#### Things to Test
- [ ] Installation completes without errors
- [ ] Binary is accessible from command line
- [ ] Extension loads in VS Code
- [ ] Server starts successfully
- [ ] Memory usage after installation: _____ MB
- [ ] Installation time: _____ minutes

### macOS Testing

#### Installation Steps

1. **Install Prerequisites**
   ```bash
   # Check if Node.js is installed
   node --version
   
   # Install via Homebrew if needed
   brew install node
   ```

2. **Install via VS Code Extension** (Recommended)
   - Open VS Code
   - Go to Extensions (Cmd+Shift+X)
   - Search for "Azure MCP Server"
   - Click Install

3. **Install via NPM** (Alternative)
   ```bash
   npm install -g @azure/mcp@latest
   ```

4. **Install via .NET Tool** (Alternative)
   ```bash
   dotnet tool install Azure.Mcp
   ```

#### Verification
```bash
# Check Azure MCP version
azmcp --version

# Verify Azure CLI
az --version

# Test basic command
azmcp server start --help
```

#### Platform-Specific Checks
- [ ] **Intel Macs**: Installation works on x64 architecture
- [ ] **Apple Silicon**: Installation works on ARM64 architecture

#### Things to Test
- [ ] Installation completes without errors
- [ ] Binary has correct permissions (executable)
- [ ] Extension loads in VS Code
- [ ] Server starts successfully
- [ ] No "unidentified developer" warnings
- [ ] Memory usage after installation: _____ MB
- [ ] Installation time: _____ minutes

---

### Linux Testing

#### Installation Steps

1. **Install Prerequisites**
   ```bash
   # Check if Node.js is installed
   node --version
   
   # Ubuntu/Debian
   sudo apt update
   sudo apt install nodejs npm
   
   # Fedora
   sudo dnf install nodejs npm
   ```

2. **Install via VS Code Extension** (Recommended)
   - Open VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "Azure MCP Server"
   - Click Install

3. **Install via NPM** (Alternative)
   ```bash
   npm install -g @azure/mcp@latest
   ```

4. **Install via .NET Tool** (Alternative)
   ```bash
   dotnet tool install Azure.Mcp
   ```

#### Verification
```bash
# Check Azure MCP version
azmcp --version

# Verify Azure CLI
az --version

# Test basic command
azmcp server start --help
```

#### Things to Test
- [ ] Installation completes without errors
- [ ] Binary has correct permissions
- [ ] Extension loads in VS Code
- [ ] Server starts successfully
- [ ] No dependency conflicts
- [ ] Memory usage after installation: _____ MB
- [ ] Installation time: _____ minutes

---

## IDE Installation Testing

### VS Code

#### Versions to Test
- [ ] **VS Code Stable** (latest)
- [ ] **VS Code Insiders** (latest)

#### Installation Testing

1. **Install Extension**
   - Method 1: Via Extension Marketplace
   - Method 2: Via Command Palette (`ext install ms-azuretools.vscode-azure-mcp-server`)
   - Method 3: Via [Installation Link](https://vscode.dev/redirect?url=vscode:extension/ms-azuretools.vscode-azure-mcp-server)

2. **Verify Installation**
   ```
   1. Open Command Palette (Ctrl+Shift+P / Cmd+Shift+P)
   2. Run "MCP: List Servers"
   3. Verify "Azure MCP Server" appears in the list
   4. Click "Start Server"
   5. Check Output window for startup logs
   ```

3. **Test Configuration**
   - Open `.vscode/mcp.json` (if it exists)
   - Verify server configuration
   - Test with custom settings

#### Things to Test
- [ ] Extension installs without errors
- [ ] Extension appears in Extensions list
- [ ] Server starts successfully
- [ ] Tools appear in GitHub Copilot Chat
- [ ] Tool count matches expectations
- [ ] Configuration changes are applied
- [ ] Server restarts successfully
- [ ] Logs are visible in Output window

#### Performance Checks
- [ ] Extension activation time: _____ ms
- [ ] Memory usage (extension): _____ MB
- [ ] Memory usage (server): _____ MB
- [ ] CPU usage during idle: _____ %

---

### Visual Studio 2022

#### Versions to Test
- [ ] **Visual Studio 2022 Community**
- [ ] **Visual Studio 2022 Professional**
- [ ] **Visual Studio 2022 Enterprise**

#### Installation Testing

1. **Install Extension**
   ```
   1. Open Visual Studio 2022
   2. Go to Extensions > Manage Extensions
   3. Search for "GitHub Copilot for Azure"
   4. Click Install
   5. Restart Visual Studio
   ```

2. **Verify Installation**
   ```
   1. Open a solution or project
   2. Open GitHub Copilot Chat
   3. Switch to Agent mode
   4. Verify Azure MCP tools are available
   ```

#### Things to Test
- [ ] Extension installs without errors
- [ ] Extension appears in Extensions list
- [ ] Extension integrates with Copilot
- [ ] Tools are accessible in chat
- [ ] Server responds to commands

#### Performance Checks
- [ ] Extension load time: _____ seconds
- [ ] Memory usage: _____ MB
- [ ] CPU usage during idle: _____ %

---

### IntelliJ IDEA

#### Versions to Test
- [ ] **IntelliJ IDEA Ultimate** (2024.3+)
- [ ] **IntelliJ IDEA Community** (2024.3+)

#### Installation Testing

1. **Install Plugins**
   ```
   1. Open IntelliJ IDEA
   2. Go to Settings/Preferences > Plugins
   3. Search for "GitHub Copilot"
   4. Install GitHub Copilot plugin
   5. Search for "Azure Toolkit for IntelliJ"
   6. Install Azure Toolkit
   7. Restart IDE
   ```

2. **Verify Installation**
   ```
   1. Open GitHub Copilot Chat
   2. Verify Azure MCP tools are available
   3. Test a simple command
   ```

#### Things to Test
- [ ] Plugins install without conflicts
- [ ] Plugins appear in Plugins list
- [ ] Azure MCP integrates with Copilot
- [ ] Tools are accessible
- [ ] Server responds to commands

#### Performance Checks
- [ ] Plugin load time: _____ seconds
- [ ] Memory usage: _____ MB
- [ ] CPU usage during idle: _____ %

---

## Verification Steps

After installation on any platform/IDE, verify:

### 1. Server Status
```bash
# Check if server is running
# In VS Code: Check Output > MCP: Azure MCP Server
# Look for: "Server started successfully"
```

### 2. Tool Discovery
```bash
# In GitHub Copilot Chat (Agent mode):
# Ask: "What Azure MCP tools are available?"
# Verify: Tools are listed
```

### 3. Authentication
```bash
# Login to Azure
az login

# Test authentication
# Ask Copilot: "List my Azure subscriptions"
```

### 4. Basic Functionality
```bash
# Test a simple command
# Ask Copilot: "List my Azure resource groups"
# Verify: Resource groups are returned
```

## Related Resources

- [Main Bug Bash Guide](README.md)
- [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md)
- [Installation Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md#installation)
- [Report Issues](https://github.com/microsoft/mcp/issues)

---

**Next Steps**: After completing installation testing, proceed to [Scenario Testing](scenarios/README.md).
