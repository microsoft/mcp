#!/usr/bin/env python3
"""
Azure MCP Server - Platform-specific package.

This module contains the platform-specific binary and provides the run_executable
function to execute the Azure MCP Server CLI.
"""

import os
import platform
import subprocess
import sys
from pathlib import Path

__version__ = "0.0.0"  # Will be replaced during packaging

# Debug mode check
DEBUG = os.environ.get("DEBUG", "").lower() in ("true", "1", "*") or "mcp" in os.environ.get("DEBUG", "")


def debug_log(*args, **kwargs):
    """Print debug messages to stderr if DEBUG is enabled."""
    if DEBUG:
        print(*args, file=sys.stderr, **kwargs)


def get_executable_path():
    """Get the path to the platform-specific executable."""
    # The binary is located in the bin subdirectory of this package
    package_dir = Path(__file__).parent
    bin_dir = package_dir / "bin"

    # Determine the executable name based on platform
    system = platform.system().lower()
    if system == "windows":
        executable_name = "azmcp.exe"
    else:
        executable_name = "azmcp"

    executable_path = bin_dir / executable_name

    debug_log(f"Package directory: {package_dir}")
    debug_log(f"Binary directory: {bin_dir}")
    debug_log(f"Executable path: {executable_path}")

    return executable_path


def run_executable(args=None):
    """
    Run the platform-specific executable with the given arguments.
    
    Args:
        args: List of command-line arguments to pass to the executable.
              Defaults to an empty list if not provided.
    
    Returns:
        The exit code from the executable.
    """
    if args is None:
        args = []

    debug_log("\nPlatform package starting")
    debug_log(f"Arguments: {args}")

    executable_path = get_executable_path()

    if not executable_path.exists():
        print(f"Error: Executable not found at {executable_path}", file=sys.stderr)
        return 1

    debug_log(f"Starting {executable_path}")

    try:
        # Run the executable and inherit stdio
        result = subprocess.run(
            [str(executable_path)] + list(args),
            stdin=sys.stdin,
            stdout=sys.stdout,
            stderr=sys.stderr,
        )
        return result.returncode
    except PermissionError:
        # Try to make it executable on Unix systems
        if platform.system().lower() != "windows":
            try:
                os.chmod(executable_path, 0o755)
                result = subprocess.run(
                    [str(executable_path)] + list(args),
                    stdin=sys.stdin,
                    stdout=sys.stdout,
                    stderr=sys.stderr,
                )
                return result.returncode
            except Exception as e:
                print(f"Error: Failed to execute {executable_path}: {e}", file=sys.stderr)
                return 1
        else:
            print(f"Error: Permission denied executing {executable_path}", file=sys.stderr)
            return 1
    except FileNotFoundError:
        print(f"Error: Executable not found: {executable_path}", file=sys.stderr)
        return 1
    except Exception as e:
        print(f"Error executing {executable_path}: {e}", file=sys.stderr)
        return 1


def main():
    """Main entry point when run directly."""
    exit_code = run_executable(sys.argv[1:])
    sys.exit(exit_code)


if __name__ == "__main__":
    main()
