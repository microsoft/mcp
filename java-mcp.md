# Java / Maven Distribution for the Azure MCP Server

## Overview

This document details a plan to produce a Java package, hosted on Maven, that allows the
Azure MCP server to be started locally using methods familiar to the Java ecosystem —
mirroring the existing PyPI (`eng/pypi`) and npm (`eng/npm`) distributions.

The package ships a small Java **launcher** compiled into a JAR. At runtime the launcher
detects the host OS/architecture, extracts the bundled native `azmcp` binary, and spawns
it — exactly the role `eng/pypi/__init__.py` plays for Python. A new PowerShell pack
script (`Pack-Maven.ps1`) assembles the JAR + POM from the same `dotnet publish` binaries
the npm and PyPI packers already consume.

### Scope

| Decision | Choice |
| --- | --- |
| API surface | **Launcher / CLI parity only** (no programmatic builder API) |
| Work scope | **Package skeleton + `Pack-Maven.ps1` only** — no pipeline YAML wiring |
| Maven coordinates | groupId `com.microsoft.mcp`, artifactId `azure-mcp` |
| Packaging model | **Fat JAR** for the skeleton; classifier-JAR alternative documented |
| Java target | Java 11 LTS (broad compatibility) |

---

## How the existing distributions work (reference)

- **npm** — A thin wrapper (`eng/npm/wrapper/index.js`) plus per-platform packages
  (`@azure/mcp-<os>-<arch>`) selected at runtime via npm's `os`/`cpu` fields. The platform
  package launcher (`eng/npm/platform/index.js`) spawns the native binary with inherited
  stdio. Packed by `eng/scripts/Pack-Npm.ps1`.
- **PyPI** — `eng/pypi/__init__.py` detects the platform and runs the bundled
  `bin/azmcp[.exe]` via `subprocess`. Metadata comes from `eng/pypi/pyproject.toml.template`;
  one wheel is produced per platform by `eng/scripts/Pack-Pypi.ps1`.
- **Binaries** — Produced by `dotnet publish` via `eng/scripts/Build-Code.ps1` into
  `.work/build/<Server>/<os>-<arch>/azmcp[.exe]`.
- **Metadata / versioning** — `eng/scripts/New-BuildInfo.ps1` generates
  `.work/build_info.json`, sourcing per-server properties (`CliName`, `NpmPackageName`,
  `PypiPackageName`, version, etc.) from the server `.csproj` via
  `eng/scripts/Get-ProjectProperties.ps1`.
- **Invocation** — Pack scripts run locally from `Build-Local.ps1`, and in the release
  pipeline from `sign-and-pack.yml` + the per-target release jobs.

The Maven package follows the **PyPI bundle-and-launch model** most closely.

---

## Implementation Plan

### Phase 1 — Launcher + package template (`eng/maven/`)

1. **`eng/maven/src/main/java/com/microsoft/mcp/azure/Launcher.java`**
   - `main(String[] args)` maps `os.name` / `os.arch` to a platform key:
     `windows-x86_64`, `windows-aarch64`, `linux-x86_64`, `linux-aarch64`,
     `macos-x86_64`, `macos-aarch64`.
   - Reads the bundled resource `/native/<platform>/azmcp[.exe]`, extracts it to a cache
     directory, sets the executable bit, then
     `new ProcessBuilder(...).inheritIO().start()`, waits, and exits with the child's code.
   - Supports an override (env var / system property) to point at an existing binary.
   - Targets Java 11.
2. **`eng/maven/pom.xml.template`** — placeholders for groupId / artifactId / version /
   description / keywords; MIT license, SCM, and developer metadata; `Main-Class` declared
   via the JAR manifest. Analogous to `eng/pypi/pyproject.toml.template`.
3. **`eng/maven/README.md`** — usage instructions
   (`java -jar azure-mcp-<ver>.jar server start`) and the packaging-model pros/cons.

### Phase 2 — Pack script (`eng/scripts/Pack-Maven.ps1`) — *depends on Phases 1 & 3*

4. Mirror `eng/scripts/Pack-Pypi.ps1`: params `ArtifactsPath` / `BuildInfoPath` /
   `OutputPath` / `UsePaths`; `ignoreMissingArtifacts` when not running under `TF_BUILD`.
   Add a `Get-JdkCommand` helper that locates `javac` / `jar` (erroring if absent), mirroring
   `Get-PythonCommand`.
5. Compile the launcher with `javac` into `classes/`; build a `native/<platform>/` resource
   tree by copying each platform directory from `.work/build`; strip `*.pdb` / `*.dSYM` /
   `*.dbg`; create the JAR with `jar --create --main-class ...`; render `pom.xml` from the
   template; emit a sources JAR (and a minimal javadoc JAR, noted for later Central
   publishing). Output to `.work/packages_maven/Azure.Mcp.Server/`.

### Phase 3 — Config plumbing (minimal modifications) — *parallel with Phase 1*

6. **`servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj`** — add `MavenGroupId`
   (`com.microsoft.mcp`), `MavenArtifactId` (`azure-mcp`), `MavenDescription`, and
   `MavenPackageKeywords`, alongside the existing `PypiPackageName` / `NpmPackageName`.
7. **`eng/scripts/Get-ProjectProperties.ps1`** — add the `Maven*` names to the
   allowed-properties list.
8. **`eng/scripts/New-BuildInfo.ps1`** (server-props block, ~lines 599–608) — surface
   `mavenGroupId` / `mavenArtifactId` / `mavenDescription` / `mavenPackageKeywords` into the
   build_info server object so `Pack-Maven.ps1` can read them.

---

## Files

### To create
- `eng/maven/src/main/java/com/microsoft/mcp/azure/Launcher.java`
- `eng/maven/pom.xml.template`
- `eng/maven/README.md`
- `eng/scripts/Pack-Maven.ps1`

### To modify
- `servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj`
- `eng/scripts/Get-ProjectProperties.ps1`
- `eng/scripts/New-BuildInfo.ps1`

### Explicitly excluded (out of scope)
- Pipeline YAML: `eng/pipelines/templates/jobs/maven/pack-maven.yml`,
  `release-maven.yml`, and wiring into `sign-and-pack.yml` / `release.yml` / `common.yml`;
  ESRP Maven publishing.
- `Build-Local.ps1` wiring (manual invocation only for now).
- `New-ServerJson.ps1` MCP-registry entry for Maven.
- Programmatic Java API.

---

## Packaging Models

The skeleton implements the **Fat JAR**. The classifier-JAR alternative is documented for a
future decision.

### Fat JAR (all platforms bundled — sqlite-jdbc style)
**Pros:** single dependency, simplest consumer UX, works offline, simplest pack script.
**Cons:** large artifact (hundreds of MB); every consumer downloads all six platform binaries.

### Per-platform classifier JARs (netty-tcnative style) + thin launcher
**Pros:** small per-platform downloads, cleaner Maven Central footprint.
**Cons:** consumers need `os-maven-plugin` or an explicit classifier; more JARs to build,
sign, and publish; more complex pack script.

**Recommendation:** ship the Fat JAR now; revisit a classifier split if artifact size becomes
a problem.

---

## Consumer Options

Once published to Maven as `com.microsoft.mcp:azure-mcp:<version>`, the artifact is an
executable fat JAR (`Main-Class: com.microsoft.mcp.azure.Launcher`). Consumers can pull and
run it in several ways, from simplest to most integrated.

**Runtime requirements (all options):** Java 11+ on the `PATH`. No .NET install is needed —
the matching native `azmcp` build is bundled and extracted on first run to
`~/.azure-mcp/<version>/<platform>/` (override with `-Dazure.mcp.cache.dir=...`). Any
arguments after the coordinate/JAR (e.g. `server start`, `--namespace storage`, `--debug`,
`--help`) are forwarded to `azmcp`. Set `AZURE_MCP_EXECUTABLE_PATH` or
`-Dazure.mcp.executable.path=/path/to/azmcp` to skip extraction and use an existing binary.

### 1. Direct download + `java -jar`
Pull the artifact from the repository's standard path layout and run it:

```bash
# Maven Central layout: /<groupId-as-path>/<artifactId>/<version>/<artifactId>-<version>.jar
curl -L -o azure-mcp.jar \
  https://repo1.maven.org/maven2/com/microsoft/mcp/azure-mcp/<version>/azure-mcp-<version>.jar

java -jar azure-mcp.jar server start
```

### 2. `mvn dependency:get` (no project required)
Resolves into the local `~/.m2` cache, then run from there:

```bash
mvn dependency:get -Dartifact=com.microsoft.mcp:azure-mcp:<version>
mvn dependency:copy -Dartifact=com.microsoft.mcp:azure-mcp:<version> -DoutputDirectory=.
java -jar azure-mcp-<version>.jar server start
```

### 3. JBang (one-liner; auto-downloads + caches)
```bash
jbang com.microsoft.mcp:azure-mcp:<version> server start
```
JBang resolves the Maven coordinates, caches the JAR, and runs its `Main-Class` directly —
ideal for an MCP client `command`/`args` configuration.

### 4. Coursier
```bash
cs launch com.microsoft.mcp:azure-mcp:<version> -- server start
```

### 5. As a project dependency (Maven / Gradle)
Add the coordinate and invoke the launcher (e.g. via the exec plugin or by running the
resolved JAR):

```xml
<dependency>
  <groupId>com.microsoft.mcp</groupId>
  <artifactId>azure-mcp</artifactId>
  <version>VERSION</version>
</dependency>
```

```kotlin
// Gradle (Kotlin DSL)
dependencies { implementation("com.microsoft.mcp:azure-mcp:VERSION") }
```

### MCP client configuration examples

Using JBang (no pre-download step):

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "jbang",
      "args": ["com.microsoft.mcp:azure-mcp:<version>", "server", "start"]
    }
  }
}
```

Using a pre-downloaded JAR:

```json
{
  "servers": {
    "azure-mcp-server": {
      "type": "stdio",
      "command": "java",
      "args": ["-jar", "/path/to/azure-mcp-<version>.jar", "server", "start"]
    }
  }
}
```

**Trade-off to flag for consumers:** because this is a fat JAR, every download includes all
six platform binaries (one larger artifact) rather than a small per-platform download.

---

## Verification

1. `./eng/scripts/Build-Code.ps1` (current platform or all) → binaries under `.work/build`.
2. `./eng/scripts/New-BuildInfo.ps1` → `build_info.json` includes the `maven*` fields.
3. `./eng/scripts/Pack-Maven.ps1` → JAR + POM under
   `.work/packages_maven/Azure.Mcp.Server`.
4. `jar tf azure-mcp-<ver>.jar` shows `Launcher.class` + `native/<platform>/azmcp[.exe]`.
5. `java -jar azure-mcp-<ver>.jar server start --help` launches the underlying binary.
6. `pom.xml` is well-formed with the correct coordinates and version.

---

## Relevant Questions

Design decisions to raise with the team before / during implementation:

1. **Packaging model.** Ship a single **Fat JAR** (simplest UX, ~hundreds of MB) or
   **per-platform classifier JARs** (smaller downloads, requires `os-maven-plugin`)? The
   skeleton assumes Fat JAR — is that acceptable for the first release?
2. **Maven coordinates.** Confirm `com.microsoft.mcp:azure-mcp`. Does it need to align with
   the npm (`@azure/mcp`) / PyPI (`msmcp-azure`) naming, or follow an existing Azure SDK
   Java groupId convention (e.g. `com.azure`)?
3. **Publishing path.** Maven Central via **ESRP Maven** or **Sonatype OSSRH**? This dictates
   GPG signing, required sources/javadoc JARs, and full POM metadata
   (license / SCM / developers), and is the largest unknown for the eventual pipeline phase.
4. **Minimum Java version.** Java 11 LTS is proposed for the widest reach. Is Java 17/21
   preferred to match internal standards?
5. **Binary extraction / caching location.** Recommended: per-version cache at
   `~/.azure-mcp/<version>/` with a temp-dir fallback (avoids re-extracting each launch).
   Acceptable, or should it always use a fresh temp directory?
6. **Programmatic API.** Current scope is CLI parity only. Is a small builder API
   (e.g. `AzureMcpServer.builder().start()`) desired in a later iteration to feel more
   "native" to Java consumers?
7. **Pipeline ownership / timing.** Who owns wiring the `maven/` pipeline jobs, signing, and
   the version-bump flow once the skeleton is validated?
8. **JDK in the build environment.** `Pack-Maven.ps1` requires `javac` / `jar` on the build
   agents. Confirm a JDK is (or can be) provisioned in CI alongside Python/Node.
9. **MCP registry.** Should the Maven package be registered in the MCP server registry
   (`server.json` / `New-ServerJson.ps1`) like the npm and PyPI packages?
