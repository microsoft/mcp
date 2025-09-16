# Troubleshooting Guide

This document is a concise troubleshooting reference for the Microsoft Fabric MCP Server. It focuses on practical, verifiable steps to diagnose common local-server issues and avoids prescriptive or unverified command examples. It was trimmed to remove Azure-specific or speculative content and to align with the project's current experience running the server.

## Quick checklist
- Verify .NET SDK (9.x) is installed and on PATH
- Build the server: `dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj`
- Run the server and confirm it starts without errors
- If a command or flag looks unfamiliar, confirm it first by running the server `--help` output

## Common Problems and Verifications

### .NET SDK or runtime not found
Symptoms
- Errors complaining about a missing `Microsoft.NETCore.App` or that a compatible framework is not available.

How to verify
```bash
dotnet --version
dotnet --list-runtimes
```

Resolution
- Install the .NET 9 SDK (or later) and restart your terminal. See: https://dotnet.microsoft.com/download/dotnet/9.0

### Build or missing artifact issues
Symptoms
- Build failures, missing resource or JSON files at runtime, or the server not exposing expected tools.

<<<<<<< HEAD
How to verify and fix
=======
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
>>>>>>> origin/main
```bash
dotnet clean servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj
dotnet build servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj --configuration Release
```
- Confirm resource files used by the public API tool are present under `tools/Fabric.Mcp.Tools.PublicApi/src/Resources/`.

### Server starts but tools aren't available to clients
Troubleshooting steps
1. Confirm the server process started and did not exit with an error
2. Check server output and logs (see Logging section below)
3. Verify your MCP client configuration (paths and args) matches the server invocation you intend to run

Important: example commands or flags (for listing tools or filtering workloads) vary by release. Do not assume a flag exists; run the server's help to confirm supported arguments:

```bash
# Shows documented flags and subcommands for this server
dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- --help
```

If a `--list-tools` or similar command is supported in your build, confirm it via `--help` before documenting it in this repository.

## Configuration and path issues
- Prefer `dotnet run --project <path-to-csproj>` over directly invoking csproj paths as commands in MCP configuration.
- Use absolute paths if running from an environment where relative paths may not resolve.
- Verify permissions for files referenced by the server (especially on Unix-like systems).

## Logging and diagnostics
- To collect environment information for a bug report:
```bash
dotnet --info
uname -a   # platform info
```
- To enable more verbose server logging (if supported by the build): set a log-level environment variable or consult the server's `--help` output to learn supported logging options.

## Collecting a useful bug report
When opening an issue, include:
- Operating system and version
- `dotnet --version` output
- MCP client type and version (e.g., VS Code MCP extension) and the MCP configuration used to launch the server
- Exact command used to start the server
- Any server output or log lines, and full stack traces if present
- Steps to reproduce the issue

## Notes about workload selection and planned features
- Workload-selection and advanced filtering are subject to change. Where possible we avoid documenting unverified flags here. If you are evaluating workload-selection options, confirm supported commands via the server `--help` output and coordinate additions to this troubleshooting guide with the maintainers.

### Fabric-Specific Issues

#### Listing available workloads
If you want to list available Fabric workloads from the local server build, confirm the command in your build and run it. The current code exposes a `publicapis` command group that contains a `list` command to enumerate workload names.

To verify available commands for your build, run the server `--help` and look for the `publicapis` group and its subcommands. Example (verify first via `--help`):

- `publicapis list` — returns workload names that other commands can use to fetch their API specs.

---

This file intentionally focuses on stable, verifiable checks and directs readers to verify CLI capabilities before relying on specific flags. For more extensive troubleshooting patterns used by other MCP implementations, consult upstream MCP guidance and adapt selectively.