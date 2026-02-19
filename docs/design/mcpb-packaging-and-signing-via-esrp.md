# Design Document: MCPB Packaging and Signing via ESRP

## Problem Statement

Microsoft's internal signing service (ESRP) does not currently support MCPB files directly. However, ESRP supports **generic PKCS#7 detached signing** (`Pkcs7DetachedSign` operation), which produces a standalone `.p7s` signature file that can be appended to any file format. Since MCPB uses PKCS#7 signatures, we can use ESRP's detached signing capability to sign MCPB files and convert the resulting signature to MCPB format.

Additionally, we need to package our MCP servers into the MCPB format using the official MCPB CLI tool. For best compatibility and performance, we should use the **trimmed version** of the server produced by the release pipeline, as it is as lightweight as we can make the server today and includes the .NET runtime. 

---

## Goals

1. **Package MCP servers into MCPB format** using the official MCPB CLI tool
2. **Use trimmed server binaries** for lightweight and self-contained packages
3. **Enable MCPB signing** for all MCP servers in this repository using existing ESRP infrastructure
4. **Automate the entire process** as part of the release pipeline
5. **Support multiple servers** (Azure.Mcp.Server, Fabric.Mcp.Server, Template.Mcp.Server, etc.)
6. **Maintain compatibility** with the official MCPB signature format per the [mcpb CLI specification](https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md)
7. **Provide verification** that the signed MCPB files are valid using `mcpb verify`

## Non-Goals

1. Modifying ESRP to natively support MCPB (out of scope - requires Microsoft-wide coordination)
2. Creating a custom signing infrastructure
3. Supporting non-Microsoft signing services

---

## Background

### MCPB Format Overview

MCP Bundles (`.mcpb`) are ZIP archives containing a local MCP server and a `manifest.json` that describes the server and its capabilities. The format is similar to Chrome extensions (`.crx`) or VS Code extensions (`.vsix`), enabling end users to install local MCP servers with a single click.

**MCPB CLI Installation:**
```bash
# Restore from local tool manifest (recommended for our pipeline, version pinned in .config/dotnet-tools.json)
dotnet tool restore

# Or via npm
npm install -g @anthropic-ai/mcpb
```

**Binary Bundle Structure:**
```
bundle.mcpb (ZIP file)
├── manifest.json         # Required: Bundle metadata and configuration
├── server/               # Server files
│   ├── azmcp             # Unix executable
│   ├── azmcp.exe         # Windows executable
│   └── [dependencies]    # All required DLLs and resources
└── servericon.png        # Bundle icon
```

### Why Trimmed Binaries?

For optimal performance with AI clients like Claude Desktop, we use **trimmed server binaries** produced by the release pipeline:

| Aspect | Full Build | Trimmed Build |
|--------|-----------|---------------|
| Size | ~150+ MB | ~100 MB |
| Startup Time | Slower | Faster |
| Dependencies | All included | Only used code |
| Client Experience | Standard | Lightweight |

The trimmed version removes unused code and dependencies through .NET's IL trimming, resulting in faster startup times critical for responsive AI client interactions.

### ESRP Pkcs7DetachedSign Operation

ESRP's `Pkcs7DetachedSign` operation creates a detached PKCS#7 signature for any file:

| Aspect | Details |
|--------|---------|
| Operation Code | `Pkcs7DetachedSign` |
| Key Code | `CP-230012` (Microsoft Corporation certificate) |
| Input | Any file (`.mcpb`, `.zip`, etc.) |
| Output | Detached `.p7s` signature file |
| Signature Type | DER-encoded PKCS#7/CMS |

The detached signature signs the entire file content and can be converted to MCPB's embedded signature format.

### MCPB Signature Structure

Per the [mcpb CLI documentation](https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md#signature-format), MCPB uses PKCS#7 (Cryptographic Message Syntax) for digital signatures:

```
[Original MCPB ZIP content]
MCPB_SIG_V1
[4-byte little-endian length prefix]
[DER-encoded PKCS#7 signature]
MCPB_SIG_END
```

> **Note on documentation vs implementation**: The official CLI.md documentation shows a simplified format with Base64-encoded signatures and no length prefix. However, we verified the actual source code for both the [NPM/TypeScript implementation](https://github.com/modelcontextprotocol/mcpb/blob/main/src/node/sign.ts) and the [C#/.NET implementation](https://github.com/asklar/mcpb/blob/user/asklar/dotnet/dotnet/mcpb/Commands/SignCommand.cs) - both use **DER-encoded (raw binary) signatures with a 4-byte little-endian length prefix**, not Base64. In TypeScript, `createSignatureBlock` writes `writeUInt32LE(pkcs7Signature.length)` and `extractSignatureBlock` reads with `readUInt32LE`. In C#, `CreateSignatureBlock` uses `BitConverter.GetBytes(pkcs7.Length)` and `ExtractSignatureBlock` reads with `BitConverter.ToInt32`. This length prefix is essential for unambiguous parsing, as raw DER bytes could otherwise contain sequences that match the `MCPB_SIG_END` marker.

This approach allows:
- Backward compatibility (unsigned MCPB files are valid ZIP files)
- Easy signature verification and removal
- Support for certificate chains with intermediate certificates

### ESRP Detached Signing Output

- Input: `file.mcpb` (staged using the `.signature.p7s` extension for ESRP processing)
- Output: `file.signature.p7s` (detached PKCS#7/CMS signature)

The signature signs the entire file content. ESRP replaces the staged file with the signature, so we stage a copy with a `.signature.p7s` extension and keep the original `.mcpb` intact.

---

## Proposed Solution

### High-Level Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MCPB Packaging and Signing Workflow                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. Build Phase (existing pipeline)                                          │
│     ┌──────────────────────┐                                                 │
│     │ dotnet publish       │                                                 │
│     │ (trimmed binaries)   │ ──────► /publish/win-x64/                       │
│     └──────────────────────┘         /publish/linux-x64/                     │
│                                      /publish/osx-x64/                       │
│                                      /publish/osx-arm64/                     │
│                                                                              │
│  2. Prepare MCPB Structure                                                   │
│     ┌────────────────────────┐                                               │
│     │ Copy manifest.json     │                                               │
│     │ Bundle server binaries │ ──────► /mcpb-staging/{platform}/             │
│     │ Copy icon              │         ├── manifest.json                     │
│     └────────────────────────┘         ├── server/                           │
│                                        │   └── [trimmed binaries]            │
│                                        └── servericon.png                     │
│                                                                              │
│  3. Package with MCPB CLI                                                    │
│     ┌──────────────────────────────┐                                         │
│     │ mcpb validate                │                                         │
│     │ mcpb pack (updates manifest) │ ──────► {ServerName}-{platform}.mcpb    │
│     └──────────────────────────────┘         (unsigned)                      │
│                                                                              │
│  4. ESRP Detached Signing (Pkcs7DetachedSign)                                │
│     ┌──────────────────────┐         ┌──────────────────┐                    │
│     │ unsigned.mcpb        │ ──ESRP──► signature.p7s    │                    │
│     │ (Pkcs7DetachedSign)  │         │ (detached sig)   │                    │
│     └──────────────────────┘         └──────────────────┘                    │
│                                                                              │
│  5. Convert & Apply Signature                                                │
│     ┌──────────────────────┐  ┌──────────────┐    ┌──────────────┐           │
│     │ unsigned.mcpb        │ +│ signature.p7s│ ───► signed.mcpb  │           │
│     └──────────────────────┘  └──────────────┘    └──────────────┘           │
│                                   │                                          │
│                                   ▼                                          │
│                            ┌────────────────────────┐                        │
│                            │ MCPB_SIG_V1            │                        │
│                            │ [length][signature]    │                        │
│                            │ MCPB_SIG_END           │                        │
│                            └────────────────────────┘                        │
│                                                                              │
│  6. Verification                                                             │
│     ┌──────────────────────┐                                                 │
│     │ mcpb verify          │ ──────► ✓ Valid signature                       │
│     │ mcpb info            │                                                 │
│     └──────────────────────┘                                                 │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Component Design

#### 1. MCPB CLI Installation

The MCPB CLI is required for packaging and verification. Restore via local tool manifest (version pinned in `.config/dotnet-tools.json`):

```bash
dotnet tool restore
```

The CLI provides essential commands:
- `mcpb init` - Create manifest.json interactively
- `mcpb validate` - Validate manifest against schema
- `mcpb pack` - Package directory into .mcpb file. Updates the manifest to include latest tool info.
- `mcpb sign` - Sign with certificate (not used - we use ESRP)
- `mcpb verify` - Verify signature of signed .mcpb
- `mcpb info` - Display information about .mcpb file

#### 2. PowerShell Script: `Pack-Mcpb.ps1`

Location: `eng/scripts/Pack-Mcpb.ps1`

**Parameters:**
- `-ArtifactsPath`: Path to the build artifacts directory containing trimmed server binaries
- `-BuildInfoPath`: Path to the `build_info.json` file containing server and platform definitions
- `-OutputPath`: Output directory for `.mcpb` files
- `-KeepStagingDirectory`: Optional flag to keep staging directory for debugging

**Supported Platforms** (determined by `build_info.json`):
- `win-x64`, `win-arm64` (Windows)
- `linux-x64`, `linux-arm64` (Linux)
- `osx-x64`, `osx-arm64` (macOS)

**Workflow:**
1. Create staging directory structure
2. Copy trimmed binaries to `server/` subdirectory
3. Copy `manifest.json` (uses `platform_overrides` in mcp_config for cross-platform support)
4. Copy icon and assets. Rename icon to `servericon.png`.
5. Copy LICENSE and NOTICE.txt into each bundle
6. Validate with `mcpb validate`
7. Package with `mcpb pack --update` (auto-populates tools array)

#### 3. Modular Pipeline Scripts

The pipeline uses separate scripts for each signing phase, enabling better maintainability and testability:

**`Stage-McpbForSigning.ps1`** - Location: `eng/scripts/Stage-McpbForSigning.ps1`
- Creates copies of `.mcpb` files with `.signature.p7s` extension for ESRP processing
- Preserves original `.mcpb` files intact
- Parameters: `-ArtifactsPath`, `-StagingPath`

**`Apply-McpbSignatures.ps1`** - Location: `eng/scripts/Apply-McpbSignatures.ps1`
- Applies ESRP-generated `.p7s` signatures to MCPB files
- Contains internal `Convert-P7sToMcpbSignature` function for signature format conversion
- Wraps .p7s in MCPB signature format (MCPB_SIG_V1 + length + sig + MCPB_SIG_END)
- Parameters: `-ArtifactsPath`, `-OutputPath`

**`Verify-McpbSignatures.ps1`** - Location: `eng/scripts/Verify-McpbSignatures.ps1`
- Verifies all signed MCPB files using `mcpb verify`
- Fails pipeline if any signature verification fails
- Parameters: `-ArtifactsPath`

#### 4. Signature Conversion Logic

```
Input:  signature.p7s (DER-encoded PKCS#7)
Output: MCPB signature block

Algorithm:
1. Read .p7s file as bytes
2. Calculate length (4-byte little-endian)
3. Construct signature block:
   - "MCPB_SIG_V1" (11 bytes, ASCII)
   - Length prefix (4 bytes, little-endian)
   - Signature bytes (from .p7s)
   - "MCPB_SIG_END" (12 bytes, ASCII)
4. Append to original .mcpb content
```

#### 5. Pipeline Integration

Location: `eng/pipelines/templates/jobs/mcpb/pack-and-sign-mcpb.yml`

**Template parameters:**
- `DependsOn`: Jobs that must complete before packaging (typically `Sign`)

**Steps:**
1. Install MCPB CLI tool
2. Download signed binaries and build_info.json
3. Package all servers for all platforms using `Pack-Mcpb.ps1`
4. Stage files for ESRP signing using `Stage-McpbForSigning.ps1`
5. Submit to ESRP using Pkcs7DetachedSign operation
6. Apply signatures using `Apply-McpbSignatures.ps1`
7. Verify using `Verify-McpbSignatures.ps1`
8. Publish signed .mcpb artifacts to GitHub using inline PowerShell in `release-mcpb.yml`

---

## Implementation Details

### File Structure

```
eng/
├── scripts/
│   ├── Pack-Mcpb.ps1                         # MCPB packaging script (uses build_info.json)
│   ├── Stage-McpbForSigning.ps1              # Stage files for ESRP signing
│   ├── Apply-McpbSignatures.ps1              # Apply .p7s signatures (includes Convert-P7sToMcpbSignature)
│   ├── Verify-McpbSignatures.ps1             # Verify signatures using mcpb CLI
│   └── Update-ServerJsonMcpbHashes.ps1       # Compute SHA256 for MCP Registry
├── pipelines/
│   └── templates/
│       └── jobs/
│           └── mcpb/
│               ├── pack-and-sign-mcpb.yml    # Pack and sign pipeline template
│               └── release-mcpb.yml          # Release pipeline template
docs/
└── mcpb-packaging-and-signing-via-esrp.md    # This design document
servers/
├── Azure.Mcp.Server/
│   ├── mcpb/
│   │   ├── manifest.json                     # MCPB manifest
│   │   └── servericon.png                    # Server icon
│   └── server.json                           # MCP Registry config with MCPB entries
├── Fabric.Mcp.Server/
│   ├── mcpb/
│   │   ├── manifest.json                     # MCPB manifest
│   │   └── servericon.png                    # Server icon
│   └── server.json                           # MCP Registry config with MCPB entries
└── Template.Mcp.Server/
    ├── mcpb/
    │   ├── manifest.json                     # MCPB manifest
    │   └── servericon.png                    # Server icon
    └── server.json                           # MCP Registry config with MCPB entries
```

### Integration with Build Info

The MCPB packaging scripts integrate with the existing `build_info.json` infrastructure, which is the source of truth for all packaging scripts. This provides:

- **Consistent versioning** across all package formats (npm, NuGet, VSIX, MCPB)
- **Platform discovery** from the build matrix
- **Server metadata** (name, cliName, description, icon path)
- **Artifact paths** for locating trimmed binaries

The `build_info.json` file is created by `New-BuildInfo.ps1` and consumed by all `Pack-*.ps1` scripts.

### Manifest Requirements

Each server must have a complete `manifest.json` file in `servers/{ServerName}/mcpb/`. The manifest should:

1. **Use `platform_overrides`** in `mcp_config` to handle cross-platform executable paths (Windows `.exe` vs Unix)
2. **Include all metadata** (name, version, description, author, etc.)
3. **Reference the correct icon** path relative to the manifest location

The `mcpb pack` command will automatically populate the `tools` array from the server's MCP tool definitions during packaging.

**Important Path Handling:**
- **`entry_point`**: Must be a relative path (e.g., `server/azmcp.exe`) without `${__dirname}`. The `mcpb pack` command verifies this file exists in the staging directory.
- **`mcp_config.command`**: Should use `${__dirname}` prefix (e.g., `${__dirname}/server/azmcp.exe`) for runtime path resolution. Both dotnet and npm mcpb validators accept this.

The `Pack-Mcpb.ps1` script automatically adds the platform-specific extension (`.exe` for Windows) to both paths.

**Important Manifest Version:** The manifest must use `"manifest_version": "0.3"` or higher to support the `_meta` field that `mcpb pack --update` adds. Using `0.2` will cause Claude Desktop to reject the bundle with "Unrecognized key(s) in object: '_meta'".

**Example:** See [servers/Azure.Mcp.Server/mcpb/manifest.json](https://github.com/microsoft/mcp/blob/vcolin7/test-mcpb-signing/servers/Azure.Mcp.Server/mcpb/manifest.json) for a complete manifest.

### Script: `Pack-Mcpb.ps1`

Location: [`eng/scripts/Pack-Mcpb.ps1`](https://github.com/microsoft/mcp/blob/vcolin7/test-mcpb-signing/eng/scripts/Pack-Mcpb.ps1)

This is the main packaging script that follows the same patterns as `Pack-Npm.ps1`, `Pack-Vsix.ps1`, etc.

**Key behaviors:**
- Reads server list and platform definitions from `build_info.json`
- Filters to trimmed, non-native, non-special-purpose platforms
- Creates `.mcpb` files named `{ServerName}-{os}-{arch}.mcpb` (e.g., `Azure.Mcp.Server-win-x64.mcpb`)
- Validates manifest with `mcpb validate` before packaging
- Copies LICENSE and NOTICE.txt into each bundle

### Script: `Apply-McpbSignatures.ps1`

Location: [`eng/scripts/Apply-McpbSignatures.ps1`](https://github.com/microsoft/mcp/blob/vcolin7/test-mcpb-signing/eng/scripts/Apply-McpbSignatures.ps1)

This script applies ESRP-generated `.p7s` signatures to MCPB files. It contains the `Convert-P7sToMcpbSignature` function internally for signature format conversion.

**Key behaviors:**
- Finds all `.mcpb` files and their corresponding `.signature.p7s` files
- Converts PKCS#7 signatures to MCPB embedded format
- Preserves directory structure in output
- Reports success/failure counts

### Pipeline Template: `pack-and-sign-mcpb.yml`

Location: [`eng/pipelines/templates/jobs/mcpb/pack-and-sign-mcpb.yml`](https://github.com/microsoft/mcp/blob/vcolin7/test-mcpb-signing/eng/pipelines/templates/jobs/mcpb/pack-and-sign-mcpb.yml)

This template orchestrates the packaging and signing workflow:

1. **PackMcpb job**: Installs MCPB CLI, downloads trimmed binaries, packages all servers
2. **SignMcpb job**: Stages files, submits to ESRP, applies signatures, verifies with `mcpb verify`

**Pipeline artifacts:**
- `mcpb-unsigned`: Unsigned MCPB packages (intermediate)
- `mcpb-signed`: Signed MCPB packages (final output)

### Pipeline Template: `release-mcpb.yml`

Location: [`eng/pipelines/templates/jobs/mcpb/release-mcpb.yml`](https://github.com/microsoft/mcp/blob/vcolin7/test-mcpb-signing/eng/pipelines/templates/jobs/mcpb/release-mcpb.yml)

This template handles publishing signed MCPB packages to GitHub Release. The upload logic is inlined directly in the YAML because 1ES release jobs (`templateContext.type: releaseJob`) do not allow `checkout` steps.

---

## Server Support Matrix

| Server | Path | Notes |
|--------|------|-------|
| Azure.Mcp.Server | `servers/Azure.Mcp.Server/` | Azure server | ✅ manifest.json created |
| Fabric.Mcp.Server | `servers/Fabric.Mcp.Server/` | Fabric server | ✅ manifest.json created |
| Template.Mcp.Server | `servers/Template.Mcp.Server/` | Template for new servers | ✅ manifest.json created |

All servers have complete `manifest.json` files with:
- `manifest_version: "0.3"` for Claude Desktop compatibility
- `platform_overrides` for cross-platform executable paths
- Proper `entry_point` and `mcp_config.command` paths
- Server icon (`servericon.png`) in the mcpb directory

The signing script auto-discovers servers based on the `servers/` directory structure and the presence of `manifest.json` files.

---

## Testing Strategy

### Unit Tests

1. **Signature format validation**: Verify the generated signature block matches MCPB spec
2. **Length prefix correctness**: Test with various signature sizes
3. **Binary integrity**: Ensure no corruption during file operations

### Integration Tests

1. **Round-trip test**: Sign → Verify → Unsign → Compare with original
2. **mcpb CLI compatibility**: Verify signed files work with `mcpb verify`, `mcpb info`
3. **Cross-platform**: Test on Windows, Linux, macOS

### Pipeline Tests

1. **Dry-run mode**: Test pipeline without actual ESRP submission
2. **Mock .p7s files**: Use pre-generated signatures for testing

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Signature format changes in mcpb spec | Medium | Version the signature format; monitor mcpb CLI updates |
| ESRP rate limiting during batch signing | Low | Implement retry logic with exponential backoff |
| Certificate expiration | Medium | Monitor cert validity; integrate with cert renewal alerts |
| Trimmed binaries missing dependencies | Medium | Test thoroughly on clean systems; include all required native libraries |
| MCPB CLI version incompatibility | Low | Pin CLI version; test updates in staging before production |
| ESRP replaces file with signature | Low | Use staging pattern: copy to `.signature.p7s` before signing |

---

## Open Questions

1. **Should we also support local/self-signed certificates for development?**
   - Recommendation: Yes, use `mcpb sign --self-signed` for local testing

2. **How to handle signature verification failures in the pipeline?**
   - Recommendation: Fail the build immediately; do not publish unsigned artifacts

3. **Should we store the .p7s files separately as artifacts?**
   - Recommendation: Yes, for audit and debugging purposes

4. **How should the manifest.json tools array be populated?**
   - Recommendation: Generate dynamically from the server's tool definitions via the `mpcb pack` command

5. **Should we support per-platform manifest variations?**
   - Recommendation: Use a single manifest with platform_overrides in mcp_config for command paths

---

## Implementation Phases

### Phase 1: Core Scripts and Templates (Week 1) ✅
- [x] Implement `Pack-Mcpb.ps1` for packaging (integrated with build_info.json)
- [x] Implement modular pipeline scripts:
  - [x] `Stage-McpbForSigning.ps1` - Stage files for ESRP
  - [x] `Apply-McpbSignatures.ps1` - Apply .p7s signatures (includes `Convert-P7sToMcpbSignature` function)
  - [x] `Verify-McpbSignatures.ps1` - Verify using mcpb CLI
- [x] Verify manifest.json for Azure.Mcp.Server (uses manifest_version 0.3)
- [x] Add unit tests for signature conversion (`Apply-McpbSignatures.tests.ps1`)
- [x] Document local usage
- [x] Validate MCPB bundle works in Claude Desktop

### Phase 2: Pipeline Integration (Week 2) ✅
- [x] Create `pack-and-sign-mcpb.yml` pipeline template
- [x] Create `release-mcpb.yml` pipeline template
- [x] Integrate with existing release pipeline (`common.yml`, `sign-and-pack.yml`, `release.yml`)
- [x] Enable MCPB packaging for Azure.Mcp.Server (`build.yml`)
- [x] Test end-to-end with Azure.Mcp.Server (requires pipeline run)
- [x] Verify signatures with `mcpb verify` (requires pipeline run)

### Phase 3: Multi-Server Support and MCP Registry (Week 3) ✅
- [x] Add MCPB package entries to `server.json` with platform-specific URLs and SHA256 placeholders
- [x] Update `New-ServerJson.ps1` to populate MCPB package URLs from GitHub Release
- [x] Create `Update-ServerJsonMcpbHashes.ps1` to compute SHA256 hashes after signing
- [x] Update `update-mcp-repository.yml` to compute hashes before publishing to MCP Registry
- [x] Create manifest.json for Fabric.Mcp.Server
- [x] Create manifest.json for Template.Mcp.Server
- [ ] Test MCP Registry publishing end-to-end (requires pipeline run)

---

## MCP Registry Publishing

The MCP Registry requires MCPB packages to include:
1. A GitHub Release URL (`identifier`) pointing to the `.mcpb` file
2. A SHA256 hash (`fileSha256`) for file integrity verification

### Supported Platforms

Each server is packaged for **6 platforms**:

| Platform | OS | Architecture | MCPB Filename Suffix |
|----------|----|--------------|-----------------------|
| `win-x64` | Windows | x86_64 | `-win-x64.mcpb` |
| `win-arm64` | Windows | arm64 | `-win-arm64.mcpb` |
| `linux-x64` | Linux | x86_64 | `-linux-x64.mcpb` |
| `linux-arm64` | Linux | arm64 | `-linux-arm64.mcpb` |
| `osx-x64` | macOS | x86_64 | `-osx-x64.mcpb` |
| `osx-arm64` | macOS | arm64 | `-osx-arm64.mcpb` |

### server.json MCPB Entries

Each platform has its own MCPB package entry in `server.json`. The placeholders (`<<McpbUrl*>>` and `<<McpbSha256*>>`) are replaced at release time:

```json
{
    "registryType": "mcpb",
    "identifier": "https://github.com/microsoft/mcp/releases/download/Azure.Mcp.Server-1.0.0/Azure.Mcp.Server-win-x64.mcpb",
    "fileSha256": "fe333e598595000ae021bd27117db32ec69af6987f507ba7a63c90638ff633ce",
    "transport": {
        "type": "stdio"
    }
}
```

### SHA256 Hash Computation

The `Update-ServerJsonMcpbHashes.ps1` script computes SHA256 hashes for signed MCPB files:

```powershell
# Compute hash using PowerShell
(Get-FileHash -Path "Azure.Mcp.Server-win-x64.mcpb" -Algorithm SHA256).Hash.ToLower()

# Or using openssl
openssl dgst -sha256 Azure.Mcp.Server-win-x64.mcpb
```

### Pipeline Flow

```
PackMcpb → SignMcpb → PublishMcpb (GitHub Release) → UpdateMcpRepository
                                                            ↓
                                                    Download MCPB artifacts
                                                            ↓
                                                    Compute SHA256 hashes
                                                            ↓
                                                    Update server.json
                                                            ↓
                                                    Publish to MCP Registry
```

---

## Local Development and Testing

### Using Pack-Mcpb.ps1 (Recommended)

The easiest way to create MCPB packages locally is to use the same script the pipeline uses:

```powershell
# Step 1: Build trimmed server binaries
./eng/scripts/Build-Code.ps1 -ServerName "Azure.Mcp.Server" -SelfContained -ReleaseBuild -Trimmed

# Step 2: Generate build_info.json
./eng/scripts/New-BuildInfo.ps1 -ServerName "Azure.Mcp.Server"

# Step 3: Create MCPB packages for all platforms
./eng/scripts/Pack-Mcpb.ps1

# Output will be in .work/packages_mcpb/
```

Or with custom paths:

```powershell
./eng/scripts/Pack-Mcpb.ps1 `
    -ArtifactsPath ".work/build" `
    -BuildInfoPath ".work/build_info.json" `
    -OutputPath "./artifacts/mcpb"
```

### Validate and Inspect Packages

```bash
# Show package information
mcpb info .work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb

# Validate manifest without packing
mcpb validate servers/Azure.Mcp.Server/mcpb/
```

### Self-Signed Testing

```bash
# Sign with self-signed certificate for local testing
mcpb sign .work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb --self-signed

# Verify (will show self-signed warning)
mcpb verify .work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb
```

### Testing with Claude Desktop

1. Build and sign the MCPB package (self-signed for testing)
2. Double-click the `.mcpb` file to install in Claude Desktop
3. Verify the server appears in Claude's MCP server list
4. Test tool invocations

---

## References

- [MCPB Repository](https://github.com/modelcontextprotocol/mcpb)
- [MCPB Manifest Specification](https://github.com/modelcontextprotocol/mcpb/blob/main/MANIFEST.md)
- [MCPB CLI Documentation](https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md)
- [MCPB Signature Format Specification](https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md#signature-format)
- [PKCS#7 / CMS Standard (RFC 5652)](https://tools.ietf.org/html/rfc5652)

---

## Approval

- [ ] Security review (signing process)
- [ ] Pipeline team review
- [ ] Documentation review

---

**Author:** Azure MCP Team  
**Date:** February 9, 2026  
**Status:** Implementation Complete - Pending pipeline validation
