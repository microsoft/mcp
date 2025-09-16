# Troubleshooting Guide

This guide helps you diagnose and resolve common issues with the Microsoft Fabric MCP Server. Since this is a **local-first server** that doesn't connect to live Fabric environments, most issues are related to server setup, .NET dependencies, or MCP client configuration.

> **💡 Quick Tip**: Most issues can be resolved by ensuring you have .NET 9 SDK installed and your MCP client configuration is correct.

## Table of Contents

- [Troubleshooting](#troubleshooting)
  - [Table of Contents](#table-of-contents)
  - [Common Issues](#common-issues)
    - [Console window is empty when running Fabric MCP Server](#console-window-is-empty-when-running-fabric-mcp-server)
    - [.NET 9 Runtime Not Found](#net-9-runtime-not-found)
    - [Can I select specific Fabric workloads to load?](#can-i-select-specific-fabric-workloads-to-load)
    - [Server starts but no tools are available](#server-starts-but-no-tools-are-available)
    - [API specifications seem outdated](#api-specifications-seem-outdated)
  - [VS Code Integration Issues](#vs-code-integration-issues)
    - [128-Tool Limit Issue](#128-tool-limit-issue)
    - [VS Code only shows a subset of tools available](#vs-code-only-shows-a-subset-of-tools-available)
    - [VS Code Cache Problems](#vs-code-cache-problems)
  - [Configuration Issues](#configuration-issues)
    - [Invalid MCP Configuration](#invalid-mcp-configuration)
    - [Path Resolution Problems](#path-resolution-problems)
    - [Permission Denied Errors](#permission-denied-errors)
  - [Fabric-Specific Issues](#fabric-specific-issues)
    - [Missing Workload APIs](#missing-workload-apis)
    - [JSON Schema Validation Errors](#json-schema-validation-errors)
    - [Example Files Not Found](#example-files-not-found)
    - [Best Practices Content Missing](#best-practices-content-missing)
  - [Logging and Diagnostics](#logging-and-diagnostics)
    - [Enabling Debug Logging](#enabling-debug-logging)
    - [Collecting Diagnostic Information](#collecting-diagnostic-information)
  - [Development Environment](#development-environment)
    - [Building from Source](#building-from-source)
    - [Running in Development Mode](#running-in-development-mode)

## Common Issues

### Console window is empty when running Fabric MCP Server

**This is normal behavior.** The Fabric MCP Server communicates with MCP clients via standard input/output (stdio). The console appears empty because all communication happens through the MCP protocol rather than displaying output to the console.

**To verify the server is working correctly:**
1. **Check the process starts without errors**:
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
   ```
2. **Look for startup error messages** - the server should start silently
3. **Test with an MCP client** to verify functionality (VS Code, Claude Desktop, etc.)
4. **Check server logs** if your MCP client supports log viewing

### .NET 9 Runtime Not Found

The Fabric MCP Server requires **.NET 9.0 SDK** or later.

**Common Error Messages:**
```bash
It was not possible to find any compatible framework version
The framework 'Microsoft.NETCore.App', version '9.0.0' was not found
The specified framework 'Microsoft.NETCore.App', version '9.0.0', apply_patches=false, version_compatibility_range=minor is not available
```

**Resolution Steps:**
1. **Install .NET 9 SDK**:
   - Download from [Microsoft .NET Downloads](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Choose the **SDK** (not just Runtime) for your operating system

2. **Verify installation**:
   ```bash
   dotnet --version
   # Should output 9.0.x or later
   ```

3. **Check available runtimes**:
   ```bash
   dotnet --list-runtimes
   # Look for Microsoft.NETCore.App 9.0.x
   ```

4. **If issues persist**, restart your terminal/command prompt after installation

### Can I select specific Fabric workloads to load?

**Yes!** You can configure the server to expose only specific Fabric workloads, which is useful for reducing tool count or focusing on specific scenarios.

**Example configuration for selective loading:**

```json
{
  "servers": {
    "Fabric Platform APIs": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj",
        "--",
        "--workload-filter",
        "platform"
      ]
    },
    "Fabric Best Practices": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj",
        "--",
        "--workload-filter",
        "bestpractices"
      ]
    }
  }
}
```

**Available workload filters:**
- `platform` - Core platform APIs and workspace management
- `bestpractices` - Best practices, examples, and guidance
- `analytics` - Lakehouse, Warehouse, KQL Database, Notebook workloads
- `integration` - Data Pipeline, Dataflow, Copy Job workloads
- `realtime` - Eventstream, Reflex, Eventhouse workloads
- `reporting` - Report, Dashboard, Semantic Model workloads

> **Note**: This is a planned feature for a future release. Currently, all workloads are loaded by default.

### Server starts but no tools are available

**Possible causes:**
1. **Embedded resources not loaded properly**
2. **Build artifacts missing**
3. **Resource file corruption**
4. **MCP client not detecting tools correctly**

**Resolution steps:**

1. **Clean and rebuild the project**:
   ```bash
   dotnet clean servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
   dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release
   ```

2. **Verify embedded resources exist**:
   ```bash
   # Check if resource files are present (Linux/macOS)
   ls -la tools/Fabric.Mcp.Tools.PublicApi/src/Resources/
   
   # Windows PowerShell
   Get-ChildItem tools/Fabric.Mcp.Tools.PublicApi/src/Resources/ -Recurse
   ```

3. **Test the server directly** (if supported):
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --list-tools
   ```

4. **Check MCP client logs** for connection issues

5. **Verify MCP client configuration** matches the expected format

### API specifications seem outdated

The Fabric MCP Server includes embedded API specifications that are current as of the release date.

**To check embedded API version:**
1. **Look for version information** in resource files:
   ```bash
   find tools/Fabric.Mcp.Tools.PublicApi/src/Resources/ -name "*.json" -exec grep -l "version\|timestamp" {} \;
   ```

2. **Check the CHANGELOG** for the last API specification update date

3. **Compare with latest documentation**:
   - [Fabric REST API Reference](https://learn.microsoft.com/rest/api/fabric/)
   - [Official Fabric API Repository](https://github.com/microsoft/fabric-docs)

**To get the latest specifications:**
1. **Update to the latest server release** - specifications are updated with each release
2. **Build from source** using the latest code for the most recent specifications
3. **Check release notes** in the [CHANGELOG](CHANGELOG.md) for API specification updates
4. **Report missing APIs** as feature requests if new Fabric APIs aren't included

## VS Code Integration Issues

### 128-Tool Limit Issue

VS Code Copilot has a 128-tool limit per request. The Fabric MCP Server is designed to stay well within this limit by organizing tools into logical groups.

**Current tool count:** ~15-20 tools (well below the limit)

If you're hitting the limit with multiple MCP servers:

**Option 1: Use Selective Server Loading**
```json
{
  "servers": {
    "Fabric APIs": {
      "command": "dotnet",
      "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj", "--", "--workload", "platform"]
    },
    "Fabric Examples": {
      "command": "dotnet", 
      "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj", "--", "--workload", "bestpractices"]
    }
  }
}
```

**Option 2: Use VS Code Custom Chat Modes**
Create different chat modes for different scenarios (API development vs. best practices research).

### VS Code only shows a subset of tools available

The Fabric MCP Server provides different tool sets based on configuration:

- **Default mode**: All Fabric tools (~15-20 tools)
- **Platform mode**: Only platform APIs (~8 tools)
- **Best practices mode**: Only examples and guidance (~5 tools)

Verify your MCP configuration matches your expectations.

### VS Code Cache Problems

If you encounter issues with stale configurations:

1. **Reload VS Code window:**
   - Press Ctrl+Shift+P (or Cmd+Shift+P on macOS)
   - Select "Developer: Reload Window"

2. **Clear VS Code caches:**
   ```bash
   # Windows
   rmdir /s "%APPDATA%\Code\Cache"
   rmdir /s "%APPDATA%\Code\CachedData"
   
   # macOS
   rm -rf ~/Library/Application\ Support/Code/Cache
   rm -rf ~/Library/Application\ Support/Code/CachedData
   
   # Linux  
   rm -rf ~/.config/Code/Cache
   rm -rf ~/.config/Code/CachedData
   ```

## Configuration Issues

### Invalid MCP Configuration

**Common configuration errors:**

1. **Incorrect file paths:**
   ```json
   // ❌ Wrong - relative path
   "command": "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"
   
   // ✅ Correct - dotnet run with project path
   "command": "dotnet",
   "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
   ```

2. **Missing required arguments:**
   ```json
   // ❌ Wrong - missing project argument
   "args": ["run", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
   
   // ✅ Correct - includes --project flag
   "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
   ```

### Path Resolution Problems

**Issue:** Server fails to start with path-related errors.

**Resolution:**
1. Use absolute paths in MCP configuration
2. Ensure the working directory is correct
3. Verify file permissions

```json
{
  "servers": {
    "Fabric MCP": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/full/path/to/servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"
      ]
    }
  }
}
```

### Permission Denied Errors

**On Unix-like systems (Linux/macOS):**
```bash
# Make sure you have execute permissions
chmod +x scripts/build.sh

# Check file ownership
ls -la servers/Fabric.Mcp.Server/src/
```

**On Windows:**
- Run as Administrator if needed
- Check antivirus software isn't blocking execution
- Verify .NET installation is accessible

## Fabric-Specific Issues

### Missing Workload APIs

**Issue:** Expected Fabric workload APIs are not available.

**Diagnosis:**
1. Check if the workload is supported:
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- platform list-workloads
   ```

2. Verify embedded resources:
   ```bash
   ls -la tools/Fabric.Mcp.Tools.PublicApi/src/Resources/fabric-rest-api-specs/contents/
   ```

**Resolution:**
- Ensure you're using the latest version
- Check if the workload was recently added to Fabric
- Report missing workloads as feature requests

### JSON Schema Validation Errors

**Issue:** Item definition schemas are invalid or missing properties.

**Common causes:**
1. Schema files corrupted during build
2. Outdated schema definitions
3. Invalid JSON formatting

**Resolution:**
1. Validate JSON schema files:
   ```bash
   # Use a JSON validator tool
   jq . tools/Fabric.Mcp.Tools.PublicApi/src/Resources/item-definitions/notebook-definition.md
   ```

2. Rebuild from clean state:
   ```bash
   git clean -fdx
   dotnet build
   ```

### Example Files Not Found

**Issue:** API example files are missing or empty.

**Check embedded examples:**
```bash
ls -la tools/Fabric.Mcp.Tools.PublicApi/src/Resources/fabric-rest-api-specs/contents/*/examples/
```

**Resolution:**
1. Verify example files exist in source
2. Check build process includes example files
3. Report missing examples if they should exist

### Best Practices Content Missing

**Issue:** Best practices documentation is empty or incomplete.

**Check embedded content:**
```bash
ls -la tools/Fabric.Mcp.Tools.PublicApi/src/Resources/*.md
```

**Resolution:**
1. Verify resource files are properly embedded
2. Check if specific topics are available
3. Contribute additional best practices content

## Logging and Diagnostics

### Enabling Debug Logging

The Fabric MCP Server uses structured logging. To enable debug output:

1. **Set log level via environment variable:**
   ```bash
   export DOTNET_LOGGING_LEVEL=Debug
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
   ```

2. **View logs in VS Code:**
   - Open Command Palette (Ctrl+Shift+P)
   - Run "MCP: List Servers"
   - Select "Fabric MCP Server"
   - Select "Show Output"
   - Change log level to "Debug" or "Trace"

### Collecting Diagnostic Information

**For bug reports, include:**

1. **Environment information:**
   ```bash
   dotnet --info
   echo $DOTNET_ROOT
   ```

2. **Server version and build info:**
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --version
   ```

3. **MCP client configuration:**
   ```json
   // Include your .vscode/mcp.json or equivalent
   ```

4. **Error messages and stack traces**

5. **Steps to reproduce the issue**

## Development and Debugging

For development setup, building from source, and debugging information, see [SUPPORT.md](SUPPORT.md).

---

## Getting Help

If you're still experiencing issues:

1. **Check the [GitHub Issues](https://github.com/microsoft/mcp/issues)** for known problems
2. **Search [GitHub Discussions](https://github.com/microsoft/mcp/discussions)** for community solutions
3. **Create a new issue** with detailed diagnostic information
4. **Join the community** for real-time help and collaboration

**When reporting issues, please include:**
- Operating system and version
- .NET version (`dotnet --version`)
- MCP client type and version
- Complete error messages and stack traces
- Steps to reproduce the problem
- Your MCP configuration (redacted for sensitive information)

For detailed support information, see [SUPPORT.md](SUPPORT.md).