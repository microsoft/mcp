# Azure MCP Server — Java / Maven Distribution

This package distributes the [Azure MCP Server](https://github.com/microsoft/mcp) as an
executable JAR for the Java ecosystem, mirroring the existing
[npm](../npm) and [PyPI](../pypi) distributions.

The JAR ships a small Java **launcher** that, at runtime, detects the host
OS/architecture, extracts the bundled native `azmcp` binary, marks it executable, and
spawns it with inherited stdio — propagating the child's exit code.

## Usage

```bash
# Start the server (stdio transport)
java -jar azure-mcp-<version>.jar server start

# Show help
java -jar azure-mcp-<version>.jar --help
```

Any arguments after the JAR are passed through to the underlying `azmcp` binary.

### Maven coordinates

```xml
<dependency>
    <groupId>com.microsoft.mcp</groupId>
    <artifactId>azure-mcp</artifactId>
    <version>VERSION</version>
</dependency>
```

### Overriding the bundled binary

To run an existing `azmcp` binary instead of the bundled one, set either:

- the `AZURE_MCP_EXECUTABLE_PATH` environment variable, or
- the `azure.mcp.executable.path` system property
  (`java -Dazure.mcp.executable.path=/path/to/azmcp -jar azure-mcp-<version>.jar ...`).

### Binary extraction / caching

The bundled binary is extracted once per version to `~/.azure-mcp/<version>/<platform>/`
(falling back to the system temp directory when no home directory is available). The
extraction directory can be overridden with the `azure.mcp.cache.dir` system property.
Subsequent launches reuse the cached binary.

### Supported platforms

| Platform key       | OS      | Architecture |
| ------------------ | ------- | ------------ |
| `windows-x86_64`   | Windows | x64          |
| `windows-aarch64`  | Windows | arm64        |
| `linux-x86_64`     | Linux   | x64          |
| `linux-aarch64`    | Linux   | arm64        |
| `macos-x86_64`     | macOS   | x64          |
| `macos-aarch64`    | macOS   | arm64        |

## Packaging model

This skeleton ships a **Fat JAR** that bundles every platform binary under
`native/<platform>/azmcp[.exe]`.

| Model                                   | Pros                                                              | Cons                                                                              |
| --------------------------------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| **Fat JAR** (this package)              | Single dependency, simplest UX, works offline, simplest packer   | Large artifact — every consumer downloads all platform binaries                   |
| Per-platform classifier JARs (future)   | Small per-platform downloads, cleaner Maven Central footprint     | Consumers need `os-maven-plugin` / an explicit classifier; more JARs to publish   |

## Building locally

The JAR is produced by [`eng/scripts/Pack-Maven.ps1`](../scripts/Pack-Maven.ps1), which
consumes the same `dotnet publish` binaries used by the npm and PyPI packers.

The packer bundles one platform folder for every platform present under `.work/build`. To
build the **uber JAR containing all platforms**, generate the platform manifest, build
every platform from it, then pack. The six standard platforms are framework-dependent
(non-AOT), so a single host can cross-publish all of them:

```powershell
# 1. Generate build_info.json (lists all standard platforms + the maven* fields)
./eng/scripts/New-BuildInfo.ps1

# 2. Build every platform listed in build_info.json into .work/build
./eng/scripts/Build-Code.ps1 -BuildInfoPath .work/build_info.json -ReleaseBuild

# 3. Pack the JAR + POM (bundles all non-native, non-special-purpose platforms)
./eng/scripts/Pack-Maven.ps1
```

To build only the **current platform** instead (faster for local testing):

```powershell
# 1. Build the native binary for the current OS/architecture
./eng/scripts/Build-Code.ps1

# 2. Generate build_info.json (includes the maven* fields)
./eng/scripts/New-BuildInfo.ps1

# 3. Pack the JAR + POM
./eng/scripts/Pack-Maven.ps1
```

The output is written to `.work/packages_maven/Azure.Mcp.Server/`.

`Pack-Maven.ps1` requires a JDK (`javac` and `jar`) on the `PATH`.

## Requirements

- Java 11 or later (the launcher targets Java 11 LTS for broad compatibility).
