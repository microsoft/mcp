# Troubleshooting Guide

This document provides practical troubleshooting steps for the Microsoft Fabric MCP Server. It focuses on verifiable solutions for common issues you might encounter when setting up or running the server locally.

## Quick Checklist
- Verify .NET 9.x SDK: `dotnet --version`
- Build the server: `dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj`
- Test server startup: `dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --help`
- Verify available commands: `dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list`

## Common Issues

### .NET SDK Not Found
**Symptoms:** Errors about missing `Microsoft.NETCore.App` or incompatible framework.

**Verification:**
```bash
dotnet --version
dotnet --list-runtimes
```

**Resolution:**
- Install .NET 9.x SDK from https://dotnet.microsoft.com/download/dotnet/9.0
- Restart your terminal after installation
- Check `global.json` in repository root for any pinned SDK versions

### Build Issues
**Symptoms:** Build failures, missing resources, or server not exposing expected tools.

**Verification and Resolution:**
```bash
# Clean and rebuild
dotnet clean servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release

# Verify resource files exist
ls tools/Fabric.Mcp.Tools.PublicApi/src/Resources/
```

**Common causes:**
- SDK version mismatch with `global.json`
- Missing resource files in `tools/Fabric.Mcp.Tools.PublicApi/src/Resources/`

### Server Starts But Tools Aren't Available
**Troubleshooting steps:**
1. **Verify server startup:**
   ```bash
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --help
   ```

2. **Test available commands:**
   ```bash
   # List available workloads
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list
   
   # Get workload details
   dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis get --workload-type notebook
   ```

3. **Check MCP client configuration** matches your server setup
4. **Review MCP client logs** for connection issues

> **Important:** Always verify command availability with `--help` before using. Command flags and subcommands are code-driven and may vary between builds.

## VS Code Integration Issues

### 128-Tool Limit Issue

VS Code Copilot has a 128-tool limit per request. The Fabric MCP Server is designed to stay well within this limit by organizing tools into logical groups.

**Current tool count:** ~15-20 tools (well below the limit)

If you're hitting the limit with multiple MCP servers, prefer one of the following approaches:

**Option 1: Use namespace mode (recommended)**
- Configure the server with `--mode namespace` instead of `--mode all` to group tools into areas and reduce tool count:
```json
{
  "servers": {
    "Fabric MCP (namespace mode)": {
      "command": "/path/to/Fabric.Mcp.Server",
      "args": ["--mode", "namespace"]
    }
  }
}
```

**Option 2: Use server commands to retrieve specific context**
- Instead of exposing all workloads at once, use the server's `publicapis` commands to fetch a workload's OpenAPI or examples on-demand (for example, use `publicapis list` to discover workloads and `publicapis get --workload-type <name>` to fetch a workload's spec).

**Option 3: Client-side filtering or multiple chat modes**
- Use client-side grouping or separate chat modes to restrict the number of tools presented to the assistant for a given task.

> Key point: avoid relying on unverified CLI flags in documentation. Confirm available commands for your build with `--help` and prefer code-driven commands such as `publicapis list` and `publicapis get --workload-type <name>` for reproducible automation.

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

## MCP Configuration Issues

### Invalid Configuration Examples

**Common configuration errors:**

1. **Published executable setup:**
   ```json
   // ❌ Wrong - missing server start command
   {
     "command": "/path/to/Fabric.Mcp.Server"
   }
   
   // ✅ Correct - includes server start arguments
   {
     "command": "/path/to/Fabric.Mcp.Server", 
     "args": ["server", "start", "--mode", "all"]
   }
   ```

2. **Development (source) setup:**
   ```json
   // ❌ Wrong - missing --project flag
   {
     "command": "dotnet",
     "args": ["run", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
   }
   
   // ✅ Correct - includes --project flag
   {
     "command": "dotnet",
     "args": ["run", "--project", "servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj"]
   }
   ```

### Path Resolution Problems

**Issue:** Server fails to start with path-related errors.

**Resolution:**
1. **Use absolute paths** in MCP configuration
2. **Verify working directory** is correct  
3. **Check file permissions** on the executable/project files

**Example with absolute paths:**
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
# Clean and rebuild if permissions seem corrupted
dotnet clean servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release

# Make executable if using published binary
chmod +x bin/Release/net9.0/{your-rid}/publish/Fabric.Mcp.Server
```

**Verification:**
- Confirm resource files exist: `ls tools/Fabric.Mcp.Tools.PublicApi/src/Resources/`
- Test server startup with `--help` flag

## Diagnostics and Logging
**Environment information for bug reports:**
```bash
dotnet --info
uname -a  # Platform information
```

**Enable verbose logging:**
- Check server `--help` for supported logging options
- Set environment variables if supported by your build

## Fabric-Specific Commands

### Listing Available Workloads
To see what Fabric workloads are available in your build:

```bash
# List all workloads  
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list

# Get specific workload details
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis get --workload-type notebook

# Get platform APIs
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis platform get
```

> **Note:** Always verify commands with `--help` first. Available commands are code-driven and may change between builds.

## Filing Bug Reports

When opening an issue, include:

- **System information:**
  - Operating system and version
  - `dotnet --version` output
  - `dotnet --info` output

- **Configuration details:**
  - MCP client type and version (VS Code extension, Claude Desktop, etc.)
  - Your MCP configuration (redact sensitive paths)
  - Exact command used to start the server

- **Error details:**
  - Server output and logs
  - Full stack traces if present
  - Steps to reproduce the issue

**Example useful command output:**
```bash
# Include this output in your bug report
dotnet --version
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --help
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list
```

---

> **Note:** This guide focuses on practical, verifiable troubleshooting steps. For additional MCP implementation patterns, consult the [official MCP documentation](https://modelcontextprotocol.io).