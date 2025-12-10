# PyPI Packaging for Azure MCP Server

This document describes how the Azure MCP Server is packaged for distribution on [PyPI](https://pypi.org), enabling installation via `pip` or execution via `uvx`.

## Package Structure

Similar to the [npm package structure](../npm/wrapperBinariesArchitecture.md), the Azure MCP Server is distributed as multiple PyPI packages:

### Wrapper Package

The main package (e.g., `azmcp`) is a cross-platform wrapper that:

1. Detects the current platform (OS and architecture)
2. Loads the appropriate platform-specific package
3. Delegates CLI execution to the platform package

The wrapper package includes:
- `__init__.py` - Main entry point with platform detection logic
- Minimal dependencies for maximum compatibility

### Platform Packages

Platform-specific packages (e.g., `azmcp-win32-x64`, `azmcp-darwin-arm64`) contain:

1. The compiled .NET binaries for that specific platform
2. An `__init__.py` with the `run_executable()` function
3. Platform-specific metadata

Supported platforms:
- `win32-x64` - Windows x64
- `win32-arm64` - Windows ARM64
- `darwin-x64` - macOS x64 (Intel)
- `darwin-arm64` - macOS ARM64 (Apple Silicon)
- `linux-x64` - Linux x64
- `linux-arm64` - Linux ARM64

## Installation Methods

### Using pip

```bash
# Install the wrapper package
pip install azmcp

# The platform-specific package will be auto-installed on first run
azmcp --version
```

### Using uvx (recommended for MCP servers)

```bash
# Run directly without installation
uvx azmcp server start

# Or with specific platform extra
uvx --with azmcp[darwin-arm64] azmcp server start
```

### Using pipx

```bash
# Install as a global tool
pipx install azmcp

# Run
azmcp server start
```

## Configuration with MCP Clients

### VS Code / Copilot

```json
{
  "mcpServers": {
    "azure": {
      "command": "uvx",
      "args": ["azmcp", "server", "start"]
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
      "args": ["azmcp", "server", "start"]
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
pip install .work/packages_pypi/Azure.Mcp.Server/wrapper/azmcp/azmcp-*.whl
pip install .work/packages_pypi/Azure.Mcp.Server/platform/azmcp-<platform>/azmcp-*.whl
```

## Publishing to PyPI

### Test PyPI (recommended for testing)

```bash
# Upload to Test PyPI
twine upload --repository testpypi .work/packages_pypi/**/*.whl .work/packages_pypi/**/*.tar.gz

# Test installation
pip install --index-url https://test.pypi.org/simple/ azmcp
```

### Production PyPI

```bash
# Upload to production PyPI
twine upload .work/packages_pypi/**/*.whl .work/packages_pypi/**/*.tar.gz
```

## Project Structure

```
eng/
├── pypi/
│   ├── wrapper/
│   │   ├── __init__.py              # Cross-platform entry point
│   │   └── pyproject.toml.template  # Template for wrapper pyproject.toml
│   ├── platform/
│   │   ├── __init__.py              # Platform binary executor
│   │   └── pyproject.toml.template  # Template for platform pyproject.toml
│   └── README.md                    # This file
└── scripts/
    └── Pack-Pypi.ps1                # Packaging script
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

### Platform package not found

If you see an error about missing platform packages:

1. Install the platform package manually:
   ```bash
   pip install azmcp-<os>-<arch>
   ```

2. Check your platform is supported (see list above)

3. Try installing with the platform extra:
   ```bash
   pip install azmcp[darwin-arm64]
   ```

### Permission denied on Unix

The package automatically sets executable permissions, but if you encounter issues:

```bash
chmod +x $(python -c "import azmcp_darwin_arm64; print(azmcp_darwin_arm64.get_executable_path())")
```

### Reporting Issues

If you encounter issues not covered here, please report them at:
https://github.com/microsoft/mcp/issues
