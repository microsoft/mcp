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