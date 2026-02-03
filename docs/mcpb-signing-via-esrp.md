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
# Install via dotnet (recommended for our pipeline)
dotnet tool install --global Mcpb.Cli

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

The MCPB CLI is required for packaging and verification. Install via dotnet tool:

```bash
dotnet tool install --global Mcpb.Cli
```

The CLI provides essential commands:
- `mcpb init` - Create manifest.json interactively
- `mcpb validate` - Validate manifest against schema
- `mcpb pack` - Package directory into .mcpb file. Updates the manifest to include latest tool info.
- `mcpb sign` - Sign with certificate (not used - we use ESRP)
- `mcpb verify` - Verify signature of signed .mcpb
- `mcpb info` - Display information about .mcpb file

#### 2. PowerShell Module: `New-McpbPackage.ps1`

Location: `eng/scripts/New-McpbPackage.ps1`

**Parameters:**
- `-ServerName`: Name of the server (e.g., Azure.Mcp.Server)
- `-Platform`: Target platform (win-x64, linux-x64, osx-x64, osx-arm64)
- `-PublishPath`: Path to trimmed binaries from dotnet publish
- `-ManifestPath`: Path to the server's pre-created manifest.json file
- `-IconPath`: Path to server icon (optional, defaults to icon next to manifest)
- `-OutputPath`: Output directory for .mcpb file

**Workflow:**
1. Create staging directory structure
2. Copy trimmed binaries to `server/` subdirectory
3. Copy `manifest.json` (uses `platform_overrides` in mcp_config for cross-platform support)
4. Copy icon and assets. Rename icon to `servericon.png`.
5. Validate with `mcpb validate`
6. Package with `mcpb pack`

#### 3. PowerShell Module: `Sign-McpbWithEsrp.ps1`

Location: `eng/scripts/Sign-McpbWithEsrp.ps1`

**Parameters:**
- `-McpbFile`: Path to the unsigned .mcpb file
- `-OutputFile`: Path for the signed .mcpb file (optional, defaults to in-place)
- `-SignatureFile`: Path to the .p7s file from ESRP (if already signed)
- `-EsrpConfig`: ESRP configuration file path (for pipeline integration)
- `-SkipVerification`: Skip mcpb verify after signing

**Functions:**
- `Stage-McpbForSigning`: Creates a copy with `.signature.p7s` extension for ESRP processing
- `Invoke-EsrpSigning`: Submits to ESRP using Pkcs7DetachedSign operation and retrieves .p7s
- `Convert-P7sToMcpbSig`: Wraps .p7s in MCPB signature format
- `Merge-McpbSignature`: Appends signature block to .mcpb
- `Test-McpbSignature`: Verifies using mcpb verify

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

Location: `eng/pipelines/templates/pack-and-sign-mcpb.yml`

**Template parameters:**
- `serverName`: Name of the server (e.g., Azure.Mcp.Server)
- `publishArtifact`: Name of artifact containing trimmed binaries
- `platforms`: List of platforms (win-x64, linux-x64, osx-x64, osx-arm64)

**Steps:**
1. Install MCPB CLI tool
2. Download trimmed binaries artifact
3. For each platform:
   - Prepare MCPB directory structure
   - Validate and pack with MCPB CLI
4. Submit to ESRP for signing
5. Apply signatures to .mcpb files
6. Verify with `mcpb verify`
7. Publish signed .mcpb artifacts

---

## Implementation Details

### File Structure

```
eng/
├── scripts/
│   ├── Pack-Mcpb.ps1                   # MCPB packaging script (uses build_info.json)
│   ├── Sign-McpbWithEsrp.ps1           # Main signing script
│   ├── Convert-P7sToMcpbSignature.ps1  # Signature conversion utility
│   └── Test-McpbSignature.ps1          # Verification wrapper
├── pipelines/
│   └── templates/
│       └── pack-and-sign-mcpb.yml      # Pipeline template
└── docs/
    └── mcpb-packaging.md               # Documentation
servers/
└── {ServerName}/
    └ mcpb/
      ├── manifest.json                 # MCPB manifest (pre-created)
      └── servericon.png                # Server icon
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

**Example:** See [servers/Azure.Mcp.Server/manifest.json](../servers/Azure.Mcp.Server/manifest.json) for a complete manifest.

### Script: `Pack-Mcpb.ps1`

This is the main packaging script that follows the same patterns as `Pack-Npm.ps1`, `Pack-Vsix.ps1`, etc.

```powershell
<#
.SYNOPSIS
    Creates MCPB (MCP Bundle) packages from trimmed server binaries.

.DESCRIPTION
    This script packages MCP servers into the MCPB format using the official MCPB CLI tool.
    It reads server and platform information from build_info.json and creates unsigned .mcpb
    files for each platform. The packages can then be signed using ESRP in a separate step.

.PARAMETER ArtifactsPath
    Path to the build artifacts directory containing the trimmed server binaries.
    Defaults to ".work/build" in the repo root.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.
    Defaults to ".work/build_info.json" in the repo root.

.PARAMETER OutputPath
    Output directory for the .mcpb files.
    Defaults to ".work/packages_mcpb" in the repo root.

.EXAMPLE
    # Package all servers for all platforms (uses build_info.json)
    ./Pack-Mcpb.ps1

.EXAMPLE
    # Specify custom paths
    ./Pack-Mcpb.ps1 -ArtifactsPath ".work/build" -OutputPath ".work/packages_mcpb"
#>
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

# ... (see eng/scripts/Pack-Mcpb.ps1 for full implementation)
```

**Key behaviors:**
- Reads server list and platform definitions from `build_info.json`
- Filters to trimmed, non-native, non-special-purpose platforms
- Creates `.mcpb` files named `{ServerName}-{os}-{arch}.mcpb` (e.g., `Azure.Mcp.Server-win-x64.mcpb`)
- Validates manifest with `mcpb validate` before packaging
- Copies LICENSE and NOTICE.txt into each bundle

### Script: `Sign-McpbWithEsrp.ps1`

```powershell
<#
.SYNOPSIS
    Signs an MCPB file using ESRP detached PKCS#7 signing.

.DESCRIPTION
    This script enables signing of MCPB files using ESRP's Pkcs7DetachedSign
    operation. It submits the MCPB file for signing and converts the resulting
    .p7s signature to MCPB's embedded signature format.

.PARAMETER McpbFile
    Path to the unsigned .mcpb file.

.PARAMETER OutputFile
    Path for the signed output file. Defaults to replacing the input file.

.PARAMETER SignatureFile
    Path to an existing .p7s signature file (skips ESRP submission).

.EXAMPLE
    ./Sign-McpbWithEsrp.ps1 -McpbFile ./Azure.Mcp.Server.mcpb
    
.EXAMPLE
    ./Sign-McpbWithEsrp.ps1 -McpbFile ./Azure.Mcp.Server.mcpb -SignatureFile ./signature.p7s
#>
```

### Signature Block Creation (PowerShell)

```powershell
function Convert-P7sToMcpbSignature {
    param(
        [Parameter(Mandatory)]
        [string]$P7sFile,
        
        [Parameter(Mandatory)]
        [string]$McpbFile,
        
        [Parameter(Mandatory)]
        [string]$OutputFile
    )
    
    # Read signature bytes
    $signatureBytes = [System.IO.File]::ReadAllBytes($P7sFile)
    
    # Create length prefix (4-byte little-endian)
    $lengthBytes = [BitConverter]::GetBytes([uint32]$signatureBytes.Length)
    
    # Create markers
    $sigV1Marker = [System.Text.Encoding]::ASCII.GetBytes("MCPB_SIG_V1")
    $sigEndMarker = [System.Text.Encoding]::ASCII.GetBytes("MCPB_SIG_END")
    
    # Read original MCPB content
    $mcpbContent = [System.IO.File]::ReadAllBytes($McpbFile)
    
    # Combine: MCPB + MCPB_SIG_V1 + length + signature + MCPB_SIG_END
    $signedContent = $mcpbContent + $sigV1Marker + $lengthBytes + $signatureBytes + $sigEndMarker
    
    # Write signed file
    [System.IO.File]::WriteAllBytes($OutputFile, $signedContent)
}
```

### Pipeline Template: `pack-and-sign-mcpb.yml`

```yaml
parameters:
  - name: buildInfoArtifact
    type: string
    default: 'build-info'
  - name: publishArtifact
    type: string
    default: 'publish-trimmed'

jobs:
  - job: PackMcpb
    displayName: 'Package MCPB'
    pool:
      vmImage: 'windows-latest'
    steps:
      # Install MCPB CLI
      - task: DotNetCoreCLI@2
        displayName: 'Install MCPB CLI'
        inputs:
          command: 'custom'
          custom: 'tool'
          arguments: 'install --global Mcpb.Cli'
      
      # Download build info
      - download: current
        artifact: ${{ parameters.buildInfoArtifact }}
        displayName: 'Download build info'
      
      # Download trimmed binaries
      - download: current
        artifact: ${{ parameters.publishArtifact }}
        displayName: 'Download trimmed binaries'
      
      # Package all servers for all platforms using build_info.json
      - pwsh: |
          ./eng/scripts/Pack-Mcpb.ps1 `
            -ArtifactsPath '$(Pipeline.Workspace)/${{ parameters.publishArtifact }}' `
            -BuildInfoPath '$(Pipeline.Workspace)/${{ parameters.buildInfoArtifact }}/build_info.json' `
            -OutputPath '$(Build.ArtifactStagingDirectory)/mcpb'
        displayName: 'Package MCPB bundles'
      
      # Publish unsigned MCPB artifacts
      - publish: '$(Build.ArtifactStagingDirectory)/mcpb'
        artifact: 'mcpb-unsigned'
        displayName: 'Publish unsigned MCPB artifacts'

  - job: SignMcpb
    displayName: 'Sign MCPB'
    dependsOn: PackMcpb
    pool:
      vmImage: 'windows-latest'
    steps:
      # Install MCPB CLI for verification
      - task: DotNetCoreCLI@2
        displayName: 'Install MCPB CLI'
        inputs:
          command: 'custom'
          custom: 'tool'
          arguments: 'install --global Mcpb.Cli'
      
      # Download unsigned MCPB artifacts
      - download: current
        artifact: 'mcpb-unsigned'
        displayName: 'Download unsigned MCPB artifacts'
      
      # Stage files for ESRP signing (copy with .signature.p7s extension)
      - pwsh: |
          $mcpbFiles = Get-ChildItem -Path "$(Pipeline.Workspace)/mcpb-unsigned" -Filter "*.mcpb"
          foreach ($mcpb in $mcpbFiles) {
              $stagePath = $mcpb.FullName -replace '\.mcpb$', '.signature.p7s'
              Copy-Item $mcpb.FullName $stagePath
              Write-Host "Staged: $($mcpb.Name) -> $(Split-Path $stagePath -Leaf)"
          }
        displayName: 'Stage MCPB files for signing'
      
      # Sign with ESRP using Pkcs7DetachedSign
      - task: EsrpCodeSigning@5
        displayName: 'Sign MCPB files'
        inputs:
          ConnectedServiceName: 'ESRP CodeSigning'
          FolderPath: '$(Pipeline.Workspace)/mcpb-unsigned'
          Pattern: '*.signature.p7s'
          signConfigType: 'inlineSignParams'
          inlineOperation: |
            [
              {
                "KeyCode": "CP-230012",
                "OperationCode": "Pkcs7DetachedSign",
                "Parameters": {},
                "ToolName": "sign",
                "ToolVersion": "1.0"
              }
            ]
      
      # Apply signatures to MCPB files
      - pwsh: |
          $mcpbFiles = Get-ChildItem -Path "$(Pipeline.Workspace)/mcpb-unsigned" -Filter "*.mcpb"
          foreach ($mcpb in $mcpbFiles) {
              $p7s = $mcpb.FullName -replace '\.mcpb$', '.signature.p7s'
              if (Test-Path $p7s) {
                  Write-Host "Applying signature to $($mcpb.Name)..."
                  ./eng/scripts/Sign-McpbWithEsrp.ps1 `
                      -McpbFile $mcpb.FullName `
                      -SignatureFile $p7s `
                      -OutputFile $mcpb.FullName
              } else {
                  Write-Warning "No signature found for $($mcpb.Name)"
              }
          }
        displayName: 'Apply MCPB signatures'
      
      # Verify signatures
      - pwsh: |
          $mcpbFiles = Get-ChildItem -Path "$(Pipeline.Workspace)/mcpb-unsigned" -Filter "*.mcpb"
          foreach ($mcpb in $mcpbFiles) {
              Write-Host "Verifying: $($mcpb.Name)"
              mcpb verify $mcpb.FullName
              if ($LASTEXITCODE -ne 0) {
                  throw "Signature verification failed for $($mcpb.Name)"
              }
              Write-Host "✓ $($mcpb.Name) verified successfully"
          }
        displayName: 'Verify MCPB signatures'
      
      # Publish signed MCPB artifacts
      - publish: '$(Pipeline.Workspace)/mcpb-unsigned'
        artifact: 'mcpb-signed'
        displayName: 'Publish signed MCPB artifacts'
```

---

## Server Support Matrix

| Server | Path | Notes |
|--------|------|-------|
| Azure.Mcp.Server | `servers/Azure.Mcp.Server/` | Azure server |
| Fabric.Mcp.Server | `servers/Fabric.Mcp.Server/` | Fabric server |
| Template.Mcp.Server | `servers/Template.Mcp.Server/` | Template for new servers |

The signing script will auto-discover servers based on the `servers/` directory structure and the presence of `manifest.json` files.

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

### Phase 1: Core Scripts and Templates (Week 1)
- [x] Implement `Pack-Mcpb.ps1` for packaging (integrated with build_info.json)
- [ ] Implement `Sign-McpbWithEsrp.ps1` for signing
- [ ] Implement `Convert-P7sToMcpbSignature.ps1` for signature conversion
- [x] Verify manifest.json for Azure.Mcp.Server (uses manifest_version 0.3)
- [ ] Add unit tests for signature conversion
- [x] Document local usage
- [x] Validate MCPB bundle works in Claude Desktop

### Phase 2: Pipeline Integration (Week 2)
- [ ] Create `pack-and-sign-mcpb.yml` pipeline template
- [ ] Integrate with existing release pipeline
- [ ] Test end-to-end with Azure.Mcp.Server
- [ ] Verify signatures with `mcpb verify`

### Phase 3: Multi-Server Support (Week 3)
- [ ] Create manifest.json for Fabric.Mcp.Server
- [ ] Create manifest.json for Template.Mcp.Server
- [ ] Add server auto-discovery for CI/CD
- [ ] Create release documentation
- [ ] Publish to MCPB distribution channels

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
**Date:** February 2, 2026  
**Status:** Draft - Validated with ESRP Pkcs7DetachedSign
