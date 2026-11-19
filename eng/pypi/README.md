# PyPI Packaging for MCP Servers

This document describes how MCP servers are packaged for distribution on [PyPI](https://pypi.org), enabling installation via `pip`, `pipx`, or execution via `uvx`. The examples below use the Azure MCP Server.

## Package Architecture

Azure MCP Server is published as a small, dependency-free Python launcher (`msmcp-azure`, and `msmcp-azure-native` when native builds are available). The wheel contains Python code and a release manifest, not .NET binaries. Other MCP servers continue to use platform-specific wheels with bundled binaries.

### Azure launcher wheels

Each Azure package has one universal wheel, for example `msmcp_azure-1.2.3b4-py3-none-any.whl`. At runtime the launcher selects Windows, macOS, or Linux and x64 or ARM64 from the platforms included in that release. Unsupported platforms fail with an explicit error.

The installed Python package version is pinned to the corresponding server release: package `1.2.3b4` downloads from `Azure.Mcp.Server-1.2.3-beta.4`, never from a moving `latest` endpoint. The launcher downloads the exact signed ZIP uploaded to the official `microsoft/mcp` GitHub Release. Archive size, SHA-256, and per-file size and SHA-256 values are embedded at packaging time. Downloaded archives and cached files must match these values before execution.

### Cache and offline use

The first invocation needs HTTPS access to `github.com` and GitHub's release asset hosts (`release-assets.githubusercontent.com` and `objects.githubusercontent.com`). Downloads use Python's standard proxy and certificate configuration. Installation with pip alone does not download the executable.

The cache defaults to:

| OS | Cache root |
| --- | --- |
| Windows | `%LOCALAPPDATA%\Microsoft\AzureMcp` |
| macOS | `~/Library/Caches/msmcp` |
| Linux | `$XDG_CACHE_HOME/msmcp`, or `~/.cache/msmcp` |

Set `AZURE_MCP_CACHE_DIR` to override the root. Cache entries are isolated by package (standard/native), Python package version, platform, and archive digest. Concurrent launchers serialize downloads using an OS file lock; interrupted downloads are cleaned up and are never promoted into the cache. The launcher rejects unsafe archive paths and symbolic links. Every invocation checks all cached files against the embedded hashes; invalid entries are replaced rather than executed.

To prepare an offline environment, invoke the same package version once on a network-connected machine with the same OS and architecture, then copy its cache to the offline machine and configure `AZURE_MCP_CACHE_DIR` if needed. A valid cache needs no network access. Delete the package's cache directory to reclaim space or force a fresh download when no server is running.

Download messages and errors use stderr, leaving stdout exclusively for the server's MCP traffic. Arguments, standard streams, and exit status are forwarded to the server.

## Installation Methods

### Using pip

```bash
# Install the Python launcher
pip install msmcp-azure

# Run
azmcp server start
```

### Using uvx (recommended for MCP servers)

```bash
# Run directly without installation
uvx --from msmcp-azure azmcp server start
```

### Using pipx

```bash
# Install as a global tool
pipx install msmcp-azure

# Run
azmcp server start
```

## Configuration with MCP Clients

### VS Code / GitHub Copilot

```json
{
  "mcpServers": {
    "azure": {
      "command": "uvx",
      "args": ["--from", "msmcp-azure", "azmcp", "server", "start"]
    }
  }
}
```

### Claude Desktop

```json
{
  "mcpServers": {
    "azure": {
      "command": "uvx",
      "args": ["--from", "msmcp-azure", "azmcp", "server", "start"]
    }
  }
}
```

## Building PyPI Packages

[Pack-Pypi.ps1](../scripts/Pack-Pypi.ps1) uses each server's `cliName` from `build_info.json` to configure its console entry point and executable name. Azure uses [azure_launcher.py](./azure_launcher.py) and a generated `release.json`. Other servers retain the bundled [entry point](./__init__.py). Native packages use the same executable name.

For Azure, pass `-ReleaseArtifactsPath` pointing at the **exact ZIPs that will be uploaded to GitHub Releases**. Do not recompress them after packaging: doing so changes the pinned digest. Missing ZIPs for any configured platform fail packaging, even in local runs. The release pipeline downloads `binaries_signed_zipped` for manifest generation and uploads those same archives before publishing PyPI packages. Its existing GitHub Release dependency prevents PyPI publication before the assets are available.

### Prerequisites

1. Python 3.10+ with `pip` and `build` package
2. .NET SDK for building server binaries
3. PowerShell 7+

### Build Steps

```powershell
# 1. Create build info
./eng/scripts/New-BuildInfo.ps1 -PublishTarget internal

# 2. Build the server binaries for all platforms
./eng/scripts/Build-Code.ps1 -OperatingSystems windows,linux,macos -Architectures x64,arm64

# 3. Create PyPI packages
.\eng\scripts\Pack-Zip.ps1 -OutputPath .\.work\binaries_signed_zipped
.\eng\scripts\Pack-Pypi.ps1 -ReleaseArtifactsPath .\.work\binaries_signed_zipped

# Output will be in .work/packages_pypi/
```

### Local Development

For quick local testing:

```powershell
# Build Azure for the current platform and narrow build_info.json to the built platforms
.\eng\scripts\Build-Local.ps1 -ServerName Azure.Mcp.Server

# Create PyPI packages
.\eng\scripts\Pack-Zip.ps1 -OutputPath .\.work\binaries_signed_zipped
.\eng\scripts\Pack-Pypi.ps1 -ReleaseArtifactsPath .\.work\binaries_signed_zipped

# Install locally for testing
pip install .work/packages_pypi/Azure.Mcp.Server/*.whl
```

Local wheels still reference the generated official release tag and do not download unpublished local artifacts. Use the offline regression tests below to exercise downloads with local ZIP fixtures, or run the local server binary directly when developing unreleased server changes.

### Testing

Run the packaging regression tests with:

```powershell
python -m unittest discover -s .\eng\pypi -p test_azure_launcher.py
Invoke-Pester -Path .\eng\scripts\Pack-Pypi.Tests.ps1,.\eng\scripts\Pack-PypiLauncher.Tests.ps1
```

The tests cover platform selection, pinned downloads, archive and cache integrity, unsafe archives, concurrent downloads, offline reuse, and argument/stream/exit-code forwarding. Packaging regression tests preserve existing bundled-server behavior and verify Azure standard/native manifests. Set `MCP_PYPI_TEST_PYTHON` to the path of a Python interpreter with `build` and `hatchling` installed to additionally build real wheels and check their universal tags, contents, and size (under 100 KiB for the fixtures); otherwise wheel building is stubbed out.

## Publishing to PyPI

### Test PyPI (recommended for testing)

```bash
# Upload to Test PyPI
twine upload --repository testpypi .work/packages_pypi/Azure.Mcp.Server/*.whl

# Test installation
pip install --index-url https://test.pypi.org/simple/ msmcp-azure
```

### Production PyPI

```bash
# Upload to production PyPI
twine upload .work/packages_pypi/Azure.Mcp.Server/*.whl
```

## Project Structure

```
eng/
├── pypi/
│   ├── __init__.py              # Package entry point with binary execution
│   ├── azure_launcher.py        # Azure on-demand release launcher
│   ├── test_azure_launcher.py   # Offline launcher regression tests
│   ├── pyproject.toml.template  # Template for pyproject.toml
│   └── README.md                # This file
└── scripts/
    ├── Pack-Pypi.ps1            # Packaging script
    ├── Pack-Pypi.Tests.ps1      # Bundled-server packaging regression tests
    └── Pack-PypiLauncher.Tests.ps1 # Azure launcher packaging regression tests
```

## Debugging

The Azure launcher always sends download diagnostics and failures to stderr. For other servers' bundled launchers, set the `DEBUG` environment variable to enable verbose logging:

```bash
# Enable debug output
DEBUG=true fabmcp server start

# Or
DEBUG=mcp fabmcp server start
```

## Troubleshooting

### Unsupported platform

If you see an error about an unsupported platform, check that your OS and architecture combination is in the supported list above.

### Download or integrity failure

Verify access to the official release hosts and that the installed package's release assets have been published. SHA-256 failures are fatal: the launcher does not fall back to an unverified executable or a different server version.

### Permission denied

Choose a writable cache using `AZURE_MCP_CACHE_DIR`. On Unix, the cache must also be on a filesystem that permits executable files; the launcher sets executable permissions after verification.

### Reporting Issues

If you encounter issues not covered here, please report them at:
https://github.com/microsoft/mcp/issues
