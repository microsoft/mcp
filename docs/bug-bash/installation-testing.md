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

3. **Install via NPM**
   ```powershell
   npm install -g @azure/mcp@latest
   ```

4. **Install via NuGet**
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

2. **Install via NPM**
   ```bash
   npm install -g @azure/mcp@latest
   ```

3. **Install via .NET Tool**
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

2. **Install via NPM** (Alternative)
   ```bash
   npm install -g @azure/mcp@latest
   ```

3. **Install via .NET Tool** (Alternative)
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
   - Method 2: Via [Installation Link](https://vscode.dev/redirect?url=vscode:extension/ms-azuretools.vscode-azure-mcp-server)

2. **Verify Installation**
   ```
   1. Open Command Palette (Ctrl+Shift+P / Cmd+Shift+P)
   2. Run "MCP: List Servers"
   3. Verify "Azure MCP Server" appears in the list
   4. Click "Start Server"
   5. Check Output window for startup logs
   ```

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

1. **Install GitHub Copilot for Azure Extension**
   ```
   1. Open Visual Studio 2022
   2. Go to Extensions → Manage Extensions...
   3. Switch to the Browse tab in Extension Manager
   4. Search for "GitHub Copilot for Azure"
   5. Click Install
   6. Restart Visual Studio when prompted
   ```

2. **Verify Installation**
   ```
   1. Open a solution or project
   2. Open GitHub Copilot Chat
   3. Select Agent Mode
   4. Click the tools icon to view available tools
   5. Search for "Azure MCP Server" to filter results
   6. Verify Azure MCP tools appear in the list
   ```

#### Things to Test
- [ ] GitHub Copilot for Azure extension installs successfully
- [ ] Extension appears in Extensions list after restart
- [ ] GitHub Copilot Chat opens correctly
- [ ] Can switch to Agent Mode
- [ ] Tools icon shows available MCP tools
- [ ] Can search/filter for "Azure MCP Server" tools
- [ ] Server responds to commands in Agent mode

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

1. **Install GitHub Copilot Plugin**
   ```
   1. Open IntelliJ IDEA
   2. Go to Settings/Preferences > Plugins
   3. Search for "GitHub Copilot"
   4. Install GitHub Copilot plugin
   5. Restart IDE
   ```

2. **Configure Azure MCP Server**
   ```
   1. Go to File → Settings (or IntelliJ IDEA → Preferences on macOS)
   2. Navigate to Tools → AI Assistant → Model Context Protocol (MCP)
   3. Click the + icon to add a new MCP server
   4. In the "New MCP Server" dialog, enter:
      - Name: Azure MCP Server
      - Command: npx (or azmcp if installed via dotnet tool)
      - Arguments: -y @azure/mcp@latest server start
   5. Click OK to save
   6. Restart AI Assistant if prompted
   ```

3. **Verify Installation**
   ```
   1. Open GitHub Copilot Chat
   2. Verify Azure MCP tools are available
   3. Test a simple command (e.g., "List my Azure subscriptions")
   ```

#### Things to Test
- [ ] GitHub Copilot plugin installs without errors
- [ ] Plugin appears in Plugins list
- [ ] MCP configuration dialog is accessible
- [ ] Can add Azure MCP Server configuration
- [ ] Server configuration saves successfully
- [ ] Azure MCP integrates with Copilot Chat
- [ ] Tools are accessible in chat
- [ ] Server responds to commands

#### Performance Checks
- [ ] Plugin load time: _____ seconds
- [ ] Memory usage: _____ MB
- [ ] CPU usage during idle: _____ %


## UI-Based Mode Toggling Testing (Distribution-Specific)

Different distribution mechanisms provide different ways to configure server modes. Test the UI controls for each distribution method.

### Distribution Method 1: VS Code Extension

**Objective**: Test VS Code Settings UI for mode toggling

#### Available Settings:

1. **Azure Mcp: Enabled Services**
   - Location: Settings → Extensions → Azure MCP
   - Purpose: Filter which Azure service namespaces are exposed
   - Type: Multi-select list
   - Options: storage, keyvault, cosmos, sql, etc.

2. **Azure Mcp: Read Only**
   - Location: Settings → Extensions → Azure MCP
   - Purpose: Enable read-only mode (blocks write operations)
   - Type: Checkbox
   - Default: Unchecked (read-write mode)

3. **Azure Mcp: Server Mode**
   - Location: Settings → Extensions → Azure MCP
   - Purpose: Choose tool exposure strategy
   - Type: Dropdown
   - Options: `single`, `namespace` (default), `all`

#### Test 1: Change Server Mode via UI

**Steps:**
1. Open VS Code Settings (Ctrl+, or Cmd+,)
2. Search for "Azure MCP"
3. Locate "Azure Mcp: Server Mode" dropdown
4. Select "namespace" (default)
5. Note current tool count in Copilot Chat
6. Change to "all"
7. Restart server: Command Palette → "MCP: List Servers" → Azure MCP → Start/Restart
8. Check tool count again

**Verify:**
- [ ] Setting UI is accessible and clear
- [ ] Dropdown shows all three options: single, namespace, all
- [ ] Default is "namespace"
- [ ] Changing mode requires server restart (prompt shown)
- [ ] Tool count changes after restart:
  - namespace: ~40-50 tools
  - all: 100+ tools
  - single: 1 tool

#### Test 2: Toggle Read-Only Mode via UI

**Steps:**
1. Open VS Code Settings
2. Search for "Azure MCP"
3. Locate "Azure Mcp: Read Only" checkbox
4. Check the box to enable read-only mode
5. Restart server
6. Test read operation: "List my storage accounts"
7. Test write operation: "Create a storage account"
8. Uncheck the box to disable read-only mode
9. Restart server
10. Test write operation again

**Verify:**
- [ ] Checkbox is visible and labeled clearly
- [ ] Default state is unchecked (read-write)
- [ ] Enabling requires server restart
- [ ] Read operations work in both modes
- [ ] Write operations blocked when enabled
- [ ] Write operations allowed when disabled
- [ ] Clear error messages when operations are blocked

#### Test 3: Filter Enabled Services via UI

**Steps:**
1. Open VS Code Settings
2. Search for "Azure MCP"
3. Locate "Azure Mcp: Enabled Services" setting
4. Click "Add Item" button
5. Add "storage" to the list
6. Add "keyvault" to the list
7. Restart server
8. Check available tools in Copilot Chat

**Verify:**
- [ ] Setting shows "Add Item" button
- [ ] Can add multiple service namespaces
- [ ] Can remove items from list
- [ ] Empty list = all services enabled
- [ ] Non-empty list = only specified services enabled
- [ ] Tool count reflects filtered services
- [ ] Test prompt for allowed service: "List storage accounts" 
- [ ] Test prompt for filtered service: "List Cosmos DB accounts" 

#### Test 4: Combine Multiple Settings

**Steps:**
1. Set "Enabled Services" to: storage, keyvault
2. Set "Server Mode" to: namespace
3. Enable "Read Only" checkbox
4. Restart server
5. Test various operations

**Verify:**
- [ ] All three settings applied correctly
- [ ] Only Storage and Key Vault namespaces exposed (~2 tools)
- [ ] Read operations work for allowed services
- [ ] Write operations blocked by read-only mode
- [ ] Filtered services not accessible

#### Test 5: Settings Persistence

**Steps:**
1. Configure settings: mode=namespace, readOnly=true, services=[storage]
2. Restart VS Code completely
3. Check if settings persisted
4. Open Settings UI and verify values
5. Start MCP server
6. Verify configuration is applied

**Verify:**
- [ ] Settings persist across VS Code restarts
- [ ] Settings shown correctly in UI after restart
- [ ] Server starts with saved configuration
- [ ] No need to reconfigure

---

### Distribution Method 2: NPM Package

**Objective**: Test CLI flag-based mode configuration with NPM-installed package

**Installation:**
```bash
npm install -g @azure/mcp@latest
```

#### Test 1: Server Mode via CLI Flags

**Test namespace mode:**
```bash
azmcp server start --mode namespace
```

**Verify:**
- [ ] Server starts successfully
- [ ] ~40-50 namespace-level tools exposed
- [ ] Tools grouped by service

**Test all mode:**
```bash
azmcp server start --mode all
```

**Verify:**
- [ ] Server starts successfully
- [ ] 100+ individual tools exposed
- [ ] Each operation has dedicated tool

**Test single mode:**
```bash
azmcp server start --mode single
```

**Verify:**
- [ ] Server starts successfully
- [ ] Exactly 1 tool exposed
- [ ] Internal routing functional

#### Test 2: Read-Only Mode via CLI Flag

**Enable read-only:**
```bash
azmcp server start --read-only
```

**Verify:**
- [ ] Server starts in read-only mode
- [ ] Read operations allowed
- [ ] Write operations blocked

**Combine with mode:**
```bash
azmcp server start --mode namespace --read-only
```

**Verify:**
- [ ] Both flags applied correctly
- [ ] Namespace mode + read-only restrictions

#### Test 3: Namespace Filtering via CLI Flags

**Filter to specific services:**
```bash
azmcp server start --namespace storage --namespace keyvault
```

**Verify:**
- [ ] Only specified namespaces exposed
- [ ] Other services filtered out
- [ ] Can specify multiple --namespace flags

#### Test 4: Tool Filtering via CLI Flags

**Filter to specific tools:**
```bash
azmcp server start --tool azmcp_subscription_list --tool azmcp_group_list
```

**Verify:**
- [ ] Only specified tools exposed
- [ ] Automatically switches to all mode
- [ ] Can specify multiple --tool flags

---

### Distribution Method 3: .NET Tool (DNX)

**Objective**: Test CLI flag-based configuration with .NET tool installation

**Installation:**
```bash
dotnet tool install --global Azure.Mcp
```

#### Test 1: Server Mode via CLI Flags

**Test namespace mode:**
```bash
azmcp server start --mode namespace
```

**Verify:**
- [ ] .NET tool version works identically to NPM version
- [ ] All mode flags functional
- [ ] Tool exposure matches expectations

#### Test 2: Configuration File Support

**Create config file** (`azmcp.config.json`):
```json
{
  "serverMode": "namespace",
  "readOnly": true,
  "enabledServices": ["storage", "keyvault"]
}
```

**Start with config:**
```bash
azmcp server start --config azmcp.config.json
```

**Verify:**
- [ ] Config file loaded successfully
- [ ] Settings from file applied
- [ ] CLI flags override config file if specified

---

### Distribution Method 4: IntelliJ IDEA Plugin

**Objective**: Test MCP Server configuration dialog for mode configuration

**Installation:**
1. IntelliJ IDEA → Settings/Preferences → Plugins
2. Search "Model Context Protocol (MCP)"
3. Install plugin
4. Restart IDE

#### Configuration Dialog Fields:

Based on IntelliJ's MCP configuration dialog:
- **Name**: Display name for the MCP server (e.g., "Azure MCP Server")
- **Command**: Executable command (e.g., `npx` or `azmcp`)
- **Arguments**: Command-line arguments including mode flags
- **Environment variables**: Key-value pairs for configuration
- **Working directory**: Optional working directory path

#### Test 1: Configure via MCP Server Dialog

**Steps:**
1. Navigate to File → Settings
2. Go to Tools → AI Assistant → Model Context Protocol (MCP)
3. Click the `+` icon to open "New MCP Server" dialog
4. Enter the following values:
   - **Name**: `Azure MCP Server`
   - **Command**: `npx`
   - **Arguments**: `-y @azure/mcp@latest server start`
5. Leave other fields blank for default configuration
6. Click OK to save
7. Restart AI Assistant or IDE if prompted

**Verify:**
- [ ] Dialog opens correctly with all fields
- [ ] Can enter server name
- [ ] Command and arguments fields accept input
- [ ] Configuration saves successfully
- [ ] Server appears in MCP server list
- [ ] Server starts when AI Assistant is opened

#### Test 2: Configure Namespace Mode via Arguments

**Steps:**
1. Open MCP Server configuration dialog
2. Edit existing "Azure MCP Server" entry
3. Update **Arguments** field to:
   ```
   -y @azure/mcp@latest server start --mode namespace
   ```
4. Click OK and restart
5. Test tool availability

**Verify:**
- [ ] Arguments field accepts mode flag
- [ ] Server restarts with new configuration
- [ ] Namespace mode applied (~40-50 tools)
- [ ] Tools grouped by service namespace

#### Test 3: Configure Read-Only Mode via Arguments

**Steps:**
1. Edit "Azure MCP Server" configuration
2. Update **Arguments** field to:
   ```
   -y @azure/mcp@latest server start --mode namespace --read-only
   ```
3. Click OK and restart
4. Test read operation: "List my storage accounts"
5. Test write operation: "Create a storage account"

**Verify:**
- [ ] Read-only flag accepted in arguments
- [ ] Read operations work
- [ ] Write operations blocked
- [ ] Error messages displayed

#### Test 4: Configure Service Filtering via Arguments

**Steps:**
1. Edit "Azure MCP Server" configuration
2. Update **Arguments** field to:
   ```
   -y @azure/mcp@latest server start --namespace storage --namespace keyvault
   ```
3. Click OK and restart
4. Test allowed service: "List storage accounts"
5. Test filtered service: "List Cosmos DB accounts"

**Verify:**
- [ ] Multiple namespace flags work
- [ ] Only specified services exposed
- [ ] Allowed services functional
- [ ] Filtered services not accessible

#### Test 5: Configuration Persistence

**Steps:**
1. Configure server with specific mode and flags
2. Close IntelliJ completely
3. Reopen IntelliJ
4. Navigate to MCP settings
5. Verify configuration still present
6. Start AI Assistant and test

**Verify:**
- [ ] Configuration persists across IDE restarts
- [ ] No need to reconfigure
- [ ] Server starts with saved settings

//TODO: Add VS, CLaude, Docker instructions.

## What to Report for UI Issues

When logging UI configuration issues, include:
- [ ] Distribution method (VS Code ext, NPM, .NET tool, etc.)
- [ ] IDE/Platform version
- [ ] Screenshot of settings UI
- [ ] Expected vs actual behavior
- [ ] Steps to reproduce
- [ ] Settings file content (if applicable)
- [ ] Server startup logs
- [ ] Whether restart was performed


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

- [Main Bug Bash Guide](https://github.com/g2vinay/mcp/tree/add-bug-bash-docs/docs/bug-bash/README.md)
- [Troubleshooting Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/TROUBLESHOOTING.md)
- [Installation Guide](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md#installation)
- [Report Issues](https://github.com/microsoft/mcp/issues)

**Next Steps**: After completing installation testing, proceed to [Scenario Testing](https://github.com/g2vinay/mcp/tree/add-bug-bash-docs/docs/bug-bash/scenarios).
