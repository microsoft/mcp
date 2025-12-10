# PyPI Packaging for Azure MCP Server

This document describes how the Azure MCP Server is packaged for distribution on [PyPI](https://pypi.org), enabling installation via `pip`, `pipx`, or execution via `uvx`.

## Package Architecture

The Azure MCP Server is published as a single package (`msmcp-azure`) with platform-specific wheels. Each wheel contains the pre-compiled binary for a specific OS and architecture combination.

### Wheel Naming Convention

Wheels follow PyPI's platform tag conventions:

- `msmcp_azure-1.0.0-py3-none-win_amd64.whl` - Windows x64
- `msmcp_azure-1.0.0-py3-none-win_arm64.whl` - Windows ARM64
- `msmcp_azure-1.0.0-py3-none-macosx_11_0_x86_64.whl` - macOS x64 (Intel)
- `msmcp_azure-1.0.0-py3-none-macosx_11_0_arm64.whl` - macOS ARM64 (Apple Silicon)
- `msmcp_azure-1.0.0-py3-none-manylinux_2_17_x86_64.manylinux2014_x86_64.whl` - Linux x64
- `msmcp_azure-1.0.0-py3-none-manylinux_2_17_aarch64.manylinux2014_aarch64.whl` - Linux ARM64

When you install with `pip install msmcp-azure`, pip automatically selects the correct wheel for your platform.

## Installation Methods

### Using pip

```bash
# Install - pip automatically selects the correct wheel for your platform
pip install msmcp-azure

# Run
azmcp server start
```

### Using uvx (recommended for MCP servers)

```bash
# Run directly without installation
uvx msmcp-azure server start
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
      "args": ["msmcp-azure", "server", "start"]
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
      "args": ["msmcp-azure", "server", "start"]
    }
  }
}
```

## Building PyPI Packages

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
./eng/scripts/Pack-Pypi.ps1

# Output will be in .work/packages_pypi/
```

### Local Development

For quick local testing:

```powershell
# Build for current platform only
./eng/scripts/Build-Code.ps1

# Create PyPI packages
./eng/scripts/Pack-Pypi.ps1 -UsePaths

# Install locally for testing
pip install .work/packages_pypi/Azure.Mcp.Server/*.whl
```

## Publishing to PyPI

### Test PyPI (recommended for testing)

```bash
# Upload to Test PyPI
twine upload --repository testpypi .work/packages_pypi/Azure.Mcp.Server/*.whl .work/packages_pypi/Azure.Mcp.Server/*.tar.gz

# Test installation
pip install --index-url https://test.pypi.org/simple/ msmcp-azure
```

### Production PyPI

```bash
# Upload to production PyPI
twine upload .work/packages_pypi/Azure.Mcp.Server/*.whl .work/packages_pypi/Azure.Mcp.Server/*.tar.gz
```

## Project Structure

```
eng/
├── pypi/
│   ├── __init__.py              # Package entry point with binary execution
│   ├── pyproject.toml.template  # Template for pyproject.toml
│   └── README.md                # This file
└── scripts/
    └── Pack-Pypi.ps1            # Packaging script
```

## Debugging

Set the `DEBUG` environment variable to enable verbose logging:

```bash
# Enable debug output
DEBUG=true azmcp server start

# Or
DEBUG=mcp azmcp server start
```

## Troubleshooting

### Unsupported platform

If you see an error about an unsupported platform, check that your OS and architecture combination is in the supported list above.

### Permission denied on Unix

The package sets executable permissions during installation, but if you encounter issues:

```bash
chmod +x $(python -c "import msmcp_azure; print(msmcp_azure.get_executable_path())")
```

### Reporting Issues

If you encounter issues not covered here, please report them at:
https://github.com/microsoft/mcp/issues
