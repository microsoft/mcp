#!/usr/bin/env python3
"""
Azure MCP Server - Cross-platform wrapper package.

This module provides a cross-platform entry point for the Azure MCP Server.
It automatically detects the current platform and delegates to the appropriate
platform-specific package.
"""

import os
import platform
import subprocess
import sys
from importlib.metadata import PackageNotFoundError
from importlib.metadata import version as get_version

__version__ = "0.0.0"  # Will be replaced during packaging

# Debug mode check
DEBUG = os.environ.get("DEBUG", "").lower() in ("true", "1", "*") or "mcp" in os.environ.get("DEBUG", "")


def debug_log(*args, **kwargs):
    """Print debug messages to stderr if DEBUG is enabled."""
    if DEBUG:
        print(*args, file=sys.stderr, **kwargs)


def get_platform_info():
    """Get the current platform and architecture."""
    system = platform.system().lower()
    machine = platform.machine().lower()

    # Map OS names to PyPI conventions
    os_map = {
        "windows": "win32",
        "darwin": "darwin",
        "linux": "linux",
    }

    # Map architecture names
    arch_map = {
        "x86_64": "x64",
        "amd64": "x64",
        "aarch64": "arm64",
        "arm64": "arm64",
    }

    pypi_os = os_map.get(system, system)
    pypi_arch = arch_map.get(machine, machine)

    return pypi_os, pypi_arch


def find_platform_package():
    """Find and return the platform-specific package module."""
    pypi_os, pypi_arch = get_platform_info()
    
    # Get the base package name from this package's name
    try:
        # Try to determine the base package name
        base_name = __name__.replace("_", "-")
        if base_name.endswith("-"):
            base_name = base_name[:-1]
    except Exception:
        base_name = "azmcp"
    
    platform_package_name = f"{base_name}-{pypi_os}-{pypi_arch}"
    module_name = platform_package_name.replace("-", "_")

    debug_log(f"Looking for platform package: {platform_package_name}")
    debug_log(f"Module name: {module_name}")

    try:
        # Try to import the platform-specific package
        platform_module = __import__(module_name)
        debug_log(f"Successfully loaded {platform_package_name}")
        return platform_module
    except ImportError as e:
        debug_log(f"Failed to import {module_name}: {e}")
        return None


def install_platform_package():
    """Attempt to install the platform-specific package."""
    pypi_os, pypi_arch = get_platform_info()
    
    try:
        base_name = __name__.replace("_", "-")
        if base_name.endswith("-"):
            base_name = base_name[:-1]
    except Exception:
        base_name = "azmcp"
    
    platform_package_name = f"{base_name}-{pypi_os}-{pypi_arch}"

    print(f"Installing missing platform package: {platform_package_name}", file=sys.stderr)

    try:
        subprocess.check_call(
            [sys.executable, "-m", "pip", "install", platform_package_name],
            stdout=subprocess.DEVNULL if not DEBUG else None,
            stderr=subprocess.DEVNULL if not DEBUG else None,
        )
        print(f"✅ Successfully installed {platform_package_name}", file=sys.stderr)
        return True
    except subprocess.CalledProcessError as e:
        debug_log(f"pip install failed: {e}")
        return False


def print_troubleshooting():
    """Print troubleshooting steps for users."""
    pypi_os, pypi_arch = get_platform_info()
    
    try:
        base_name = __name__.replace("_", "-")
        if base_name.endswith("-"):
            base_name = base_name[:-1]
    except Exception:
        base_name = "azmcp"
    
    platform_package_name = f"{base_name}-{pypi_os}-{pypi_arch}"

    print(f"""
❌ Failed to load platform-specific package '{platform_package_name}'

🔍 Troubleshooting steps:

1. Install the platform-specific package manually:
   pip install {platform_package_name}

2. If using uvx, try with the platform extra:
   uvx --with {base_name}[{pypi_os}-{pypi_arch}] {base_name}

3. Check if your platform is supported:
   - Windows x64: {base_name}-win32-x64
   - Windows ARM64: {base_name}-win32-arm64
   - macOS x64: {base_name}-darwin-x64
   - macOS ARM64: {base_name}-darwin-arm64
   - Linux x64: {base_name}-linux-x64
   - Linux ARM64: {base_name}-linux-arm64

4. If the issue persists, please report it at:
   https://github.com/microsoft/mcp/issues
""", file=sys.stderr)


def main():
    """Main entry point for the CLI."""
    debug_log("\nWrapper package starting")
    debug_log(f"Python version: {sys.version}")
    debug_log(f"Platform: {platform.system()} {platform.machine()}")
    debug_log(f"Arguments: {sys.argv[1:]}")

    # Try to find the platform package
    platform_module = find_platform_package()

    if platform_module is None:
        # Try to install it
        if install_platform_package():
            platform_module = find_platform_package()

    if platform_module is None:
        print_troubleshooting()
        sys.exit(1)

    # Run the executable from the platform package
    try:
        exit_code = platform_module.run_executable(sys.argv[1:])
        sys.exit(exit_code)
    except AttributeError:
        print(f"Error: Platform package does not have run_executable function", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Error running executable: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
