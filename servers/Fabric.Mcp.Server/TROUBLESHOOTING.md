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

How to verify and fix
```bash
dotnet --version
dotnet --list-runtimes
```

Resolution
- If the SDK or runtime is missing or incompatible, inspect `global.json` at the repository root. Either install the requested SDK (for example, .NET 9.x) or update `global.json` to match an SDK you have available for local development.
- After installing or changing SDKs, restart your terminal so environment changes take effect.

Note: We intentionally avoid documenting unverified runtime flags or ad-hoc workload-filter examples here. To discover supported server commands and capabilities for your build, run the server `--help` and prefer the implemented, code-driven commands (for example `publicapis list`) when available.

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

3. **Test the server directly** (if supported):
```bash
# To list workloads implemented in this build, prefer the code-driven command:
# (verify via --help first)

dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list
```

4. **Check MCP client logs** for connection issues

5. **Verify MCP client configuration** matches the expected format

## VS Code Integration Issues

### 128-Tool Limit Issue

VS Code Copilot has a 128-tool limit per request. The Fabric MCP Server is designed to stay well within this limit by organizing tools into logical groups.

**Current tool count:** ~15-20 tools (well below the limit)

If you're hitting the limit with multiple MCP servers, prefer one of the following approaches:

**Option 1: Run focused server instances (recommended)**
- Publish the server as a self-contained executable and run multiple instances configured to expose only the subset of tools or contexts you need. Point your MCP client to the specific executable for each scenario.
- Example (high-level): configure multiple server entries in your MCP client, each pointing to a published executable that is preconfigured for a particular workload area.

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

3. **Test the server directly** (if supported):
```bash
# To list workloads implemented in this build, prefer the code-driven command:
# (verify via --help first)

dotnet run --project servers/Fabric.Mcp.Server/src/Fabric.Mcp.Server.csproj -- publicapis list
```

4. **Check MCP client logs** for connection issues

5. **Verify MCP client configuration** matches the expected format

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