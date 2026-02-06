# Design Document: MSIX Packaging for Windows MCP Server Registration

## Problem Statement

Windows provides a native on-device agent registry (ODR) that allows AI agents to discover and connect to MCP servers. To leverage this registry with **secure containment** (running in a separate agent session with limited privileges), MCP servers must be packaged using **MSIX** with package identity.

Currently, our MCP servers can be installed via:
- npm/npx
- NuGet/dotnet tool
- Docker
- ZIP archives
- MCPB bundles (for Claude Desktop)

None of these formats provide **Windows package identity**, which means:
1. Users must manually register servers or use reduced security settings
2. Servers cannot run in the secure contained agent session
3. Servers don't appear in the Windows on-device agent registry by default
4. No automatic registration/unregistration on install/uninstall

---

## Goals

1. **Package MCP servers as MSIX** for Windows on-device agent registry integration
2. **Enable secure containment** - servers run in isolated agent sessions
3. **Automatic registration** - servers register with Windows ODR on install
4. **Support Microsoft Store distribution** (future)
5. **Integrate with existing pipeline** - leverage signed binaries and build_info.json
6. **Provide proper manifest metadata** including static responses for containment

## Non-Goals

1. Replacing existing distribution formats (npm, NuGet, MCPB) - MSIX is additive
2. Supporting non-Windows platforms via MSIX
3. Building a full WinUI/WPF application - we're packaging the CLI server only

---

## Background

### Windows On-Device Agent Registry (ODR)

Windows provides a registry for AI agents to discover MCP servers. Servers can register via:

| Method | Package Identity | Secure Containment | Auto-Register |
|--------|-----------------|-------------------|---------------|
| MSIX Package | ✅ Yes | ✅ Yes | ✅ Yes |
| MSIX with External Location | ✅ Yes | ✅ Yes | ✅ Yes |
| MCPB Bundle | ❌ No | ❌ No | ❌ Manual |
| Manual Registration | ❌ No | ❌ No | ❌ Manual |

### Why MSIX?

MSIX provides:
- **Package Identity**: Required for secure containment
- **Automatic Registration**: OS registers/unregisters server on install/uninstall
- **Secure Containment**: Server runs in separate agent session with limited privileges
- **Enterprise Management**: IT admins can manage via MDM/Intune
- **Microsoft Store Ready**: Can be published to Windows Store

### Secure Containment Requirements

For a server to run in the contained agent session, it must:
1. Be implemented as a binary (.exe) server
2. Have package identity (MSIX)
3. Provide a valid `manifest.json` with:
   - `manifest_version`, `name`, `version`, `description`, `author`, `server`
   - `_meta.com.microsoft.windows.static_responses` with `initialize` and `tools/list` responses

### MSIX Package Structure

```
MyMcpServer.msix
├── AppxManifest.xml           # Package manifest with app extension
├── Assets/
│   ├── manifest.json          # MCP bundle manifest (for ODR registration)
│   ├── StoreLogo.png          # Store logo (50x50)
│   ├── Square44x44Logo.png    # App icon (44x44)
│   ├── Square150x150Logo.png  # Medium tile (150x150)
│   └── Wide310x150Logo.png    # Wide tile (310x150)
├── server/
│   ├── azmcp.exe              # Server executable
│   └── [dependencies]         # All required DLLs
└── [Content_Types].xml        # MSIX content types
```

---

## Proposed Solution

### High-Level Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MSIX Packaging Workflow for MCP Servers                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. Build Phase (existing pipeline)                                          │
│     ┌──────────────────────┐                                                 │
│     │ dotnet publish       │                                                 │
│     │ (win-x64 binaries)   │ ──────► /publish/win-x64/                       │
│     └──────────────────────┘                                                 │
│                                                                              │
│  2. Prepare MSIX Structure                                                   │
│     ┌────────────────────────┐                                               │
│     │ Create AppxManifest.xml│                                               │
│     │ Copy server binaries   │ ──────► /msix-staging/                        │
│     │ Copy manifest.json     │         ├── AppxManifest.xml                  │
│     │ Generate assets        │         ├── Assets/manifest.json              │
│     └────────────────────────┘         ├── server/azmcp.exe                  │
│                                        └── Assets/*.png                      │
│                                                                              │
│  3. Package with MakeAppx                                                    │
│     ┌──────────────────────────────┐                                         │
│     │ MakeAppx.exe pack            │ ──────► {ServerName}.msix               │
│     └──────────────────────────────┘         (unsigned)                      │
│                                                                              │
│  4. Sign with ESRP (Authenticode)                                            │
│     ┌──────────────────────┐                                                 │
│     │ SignTool.exe sign    │ ──────► {ServerName}.msix                       │
│     │ (via ESRP)           │         (signed)                                │
│     └──────────────────────┘                                                 │
│                                                                              │
│  5. Distribution                                                             │
│     ┌──────────────────────┐                                                 │
│     │ GitHub Release       │                                                 │
│     │ Microsoft Store      │ (future)                                        │
│     │ Winget               │ (future)                                        │
│     └──────────────────────┘                                                 │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Component Design

#### 1. AppxManifest.xml Template

Each server needs an `AppxManifest.xml` that:
- Declares package identity (Name, Publisher, Version)
- Registers the MCP server extension (`com.microsoft.windows.ai.mcpServer`)
- Declares an execution alias for the server executable
- Requests necessary capabilities (internet access, file access)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:uap3="http://schemas.microsoft.com/appx/manifest/uap/windows10/3"
         xmlns:uap5="http://schemas.microsoft.com/appx/manifest/uap/windows10/5"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap uap3 uap5 rescap">

  <Identity Name="Microsoft.Azure.Mcp.Server"
            Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
            Version="1.0.0.0"
            ProcessorArchitecture="x64" />

  <Properties>
    <DisplayName>Azure MCP Server</DisplayName>
    <PublisherDisplayName>Microsoft Corporation</PublisherDisplayName>
    <Description>Azure Model Context Protocol Server for AI agents</Description>
    <Logo>Assets\StoreLogo.png</Logo>
  </Properties>

  <Dependencies>
    <!-- MinVersion 10.0.26100.0 - MCP ODR and TrustedLaunch require Windows 11 24H2+ -->
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.26100.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>

  <Resources>
    <Resource Language="en-us" />
  </Resources>

  <Applications>
    <Application Id="AzureMcpServer" Executable="server\azmcp.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="Azure MCP Server"
                          Description="Azure Model Context Protocol Server"
                          BackgroundColor="transparent"
                          Square150x150Logo="Assets\Square150x150Logo.png"
                          Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" />
      </uap:VisualElements>

      <Extensions>
        <!-- MCP Server Registration -->
        <uap3:Extension Category="windows.appExtension">
          <uap3:AppExtension Name="com.microsoft.windows.ai.mcpServer"
                             Id="AzureMcpServer"
                             DisplayName="Azure MCP Server"
                             PublicFolder="Assets">
            <uap3:Properties>
              <Registration>manifest.json</Registration>
            </uap3:Properties>
          </uap3:AppExtension>
        </uap3:Extension>

        <!-- Execution Alias -->
        <uap5:Extension Category="windows.appExecutionAlias">
          <uap5:AppExecutionAlias>
            <uap5:ExecutionAlias Alias="azmcp.exe" />
          </uap5:AppExecutionAlias>
        </uap5:Extension>
      </Extensions>
    </Application>
  </Applications>

  <Capabilities>
    <Capability Name="internetClient" />
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
```

#### 2. MCP Manifest with Static Responses

The Windows ODR requires a `manifest.json` with `_meta.com.microsoft.windows.static_responses` for secure containment. This allows the ODR to validate server responses without launching the server.

```json
{
  "manifest_version": "0.3",
  "name": "azure-mcp-server",
  "version": "1.0.0",
  "description": "Azure Model Context Protocol Server - Access Azure services through AI agents",
  "author": {
    "name": "Microsoft Corporation"
  },
  "server": {
    "type": "binary",
    "entry_point": "server/azmcp.exe",
    "mcp_config": {
      "command": "server/azmcp.exe",
      "args": ["server", "start"]
    }
  },
  "tools": [...],
  "tools_generated": true,
  "_meta": {
    "com.microsoft.windows": {
      "static_responses": {
        "initialize": {
          "protocolVersion": "2025-06-18",
          "capabilities": {
            "logging": {},
            "tools": {
              "listChanged": true
            }
          },
          "serverInfo": {
            "name": "azmcp",
            "version": "1.0.0"
          }
        },
        "tools/list": {
          "tools": [
            {
              "name": "storage_account_list",
              "description": "List Azure Storage accounts",
              "inputSchema": {
                "type": "object",
                "properties": {
                  "subscription": {
                    "type": "string",
                    "description": "Azure subscription ID or name"
                  }
                },
                "required": ["subscription"]
              }
            }
            // ... more tools
          ]
        }
      }
    }
  }
}
```

#### 3. PowerShell Script: `Pack-Msix.ps1`

Location: `eng/scripts/Pack-Msix.ps1`

**Parameters:**
- `-ServerName`: Name of the server (e.g., Azure.Mcp.Server)
- `-ArtifactsPath`: Path to build artifacts
- `-BuildInfoPath`: Path to build_info.json
- `-OutputPath`: Output directory for .msix files
- `-CertificatePath`: Path to signing certificate (for local testing)

**Workflow:**
1. Read server metadata from build_info.json
2. Create staging directory structure
3. Copy win-x64 server binaries to `server/`
4. Generate/copy `AppxManifest.xml` with version from build_info
5. Generate/copy `manifest.json` with static responses
6. Copy/generate asset images
7. Run `MakeAppx.exe pack` to create .msix
8. (Optional) Sign with test certificate for local testing

#### 4. Pipeline Integration

Location: `eng/pipelines/templates/jobs/msix/pack-and-sign-msix.yml`

**Jobs:**
1. **PackMsix**: Creates unsigned .msix packages
2. **SignMsix**: Signs packages using ESRP Authenticode signing
3. **PublishMsix**: Uploads to GitHub Release (and future Store submission)

---

## Implementation Details

### File Structure

```
eng/
├── scripts/
│   └── Pack-Msix.ps1                         # MSIX packaging script (reuses MCPB manifest)
├── pipelines/
│   └── templates/
│       └── jobs/
│           └── msix/
│               ├── pack-and-sign-msix.yml    # Pack and sign pipeline
│               └── release-msix.yml          # Release pipeline
servers/
└── {ServerName}/
    ├── mcpb/
    │   └── manifest.json                     # Shared MCP manifest (source of truth)
    └── msix/
        ├── AppxManifest.template.xml         # MSIX-specific package manifest template
        └── Assets/                           # Optional MSIX-specific assets (falls back to mcpb/)
            └── *.png                         # Logo files (if different from mcpb/)
```

### MCP Manifest Reuse

The `Pack-Msix.ps1` script **reuses the MCPB `manifest.json`** as the base and transforms it for Windows:

1. **Removes** `platform_overrides` (MSIX is Windows-only)
2. **Updates** `compatibility.platforms` to `["win32"]`
3. **Removes** `claude_desktop` requirement
4. **Preserves or Adds** `_meta.com.microsoft.windows.static_responses` section for ODR containment
5. **Updates** paths (removes `${__dirname}` prefix)

### Static Responses: Best Practice

**Recommended workflow**: Run `Pack-Mcpb.ps1 -KeepStagingDirectory` first, then `Pack-Msix.ps1 -McpbStagingPath`:

```powershell
# Step 1: Run MCPB packaging with --update to auto-discover tools
./eng/scripts/Pack-Mcpb.ps1 -ServerName "Azure.Mcp.Server" -KeepStagingDirectory

# Step 2: Run MSIX packaging using MCPB staging (inherits _meta with all tools)
./eng/scripts/Pack-Msix.ps1 -ServerName "Azure.Mcp.Server" -McpbStagingPath ".work/temp_mcpb"
```

This approach reuses the `_meta.com.microsoft.windows.static_responses` section that `mcpb pack --update` auto-generates by:
1. Running the MCP server with `tools/list` request
2. Capturing all 50+ tools with their full `inputSchema`
3. Embedding this in the manifest for Windows ODR containment validation

**Alternative**: If MCPB staging is not available, you can use an existing `.mcpb` package:

```powershell
# Use existing .mcpb package (will be unpacked to extract manifest)
./eng/scripts/Pack-Msix.ps1 -McpbPackagePath ".work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb"
```

**Fallback**: If neither MCPB staging nor package is provided, the script falls back to the source manifest and generates a minimal `_meta` section with an empty tools list. This is not recommended for production as it limits Windows ODR validation capabilities.

### Signing Requirements

MSIX packages must be signed with a certificate that:
1. Chains to a trusted root (Microsoft Trusted Root Program)
2. Has the correct Publisher DN matching `AppxManifest.xml`
3. Uses Authenticode signing (not PKCS#7 detached like MCPB)

For ESRP, use:
- **Operation**: `SigntoolSign`
- **Key Code**: `CP-230012` (Microsoft Corporation) or appropriate code signing cert
- **Tool**: SignTool.exe

### Windows Version Requirements

- **Minimum**: Windows 10 version 2004 (Build 19041) for basic MSIX
- **For Agent Registry**: Windows 11 Build 26220 or higher
- **For Containment**: Windows 11 Build 26220 or higher

---

## Comparison: MCPB vs MSIX

| Feature | MCPB | MSIX |
|---------|------|------|
| **Target** | Claude Desktop | Windows AI Agents |
| **Package Identity** | No | Yes |
| **Secure Containment** | No | Yes |
| **Auto-Registration** | No (manual install) | Yes (on install) |
| **Signing** | PKCS#7 embedded | Authenticode |
| **Store Distribution** | No | Yes (future) |
| **Enterprise Management** | No | Yes (MDM/Intune) |
| **Cross-Platform** | Yes (win/linux/mac) | Windows only |

**Recommendation**: Ship both formats:
- **MCPB**: For Claude Desktop and cross-platform scenarios
- **MSIX**: For Windows native agent integration with full security

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Static responses out of sync | High | Generate at build time from server |
| Windows version too new | Medium | Document requirements; provide MCPB fallback |
| Certificate Publisher mismatch | High | Validate Publisher DN matches manifest |
| Large package size | Low | Use trimmed binaries; single architecture |
| Store submission complexity | Medium | Start with sideloading; Store is future phase |

---

## TrustedLaunch Requirements

**TrustedLaunch is REQUIRED for MCP server apps** to prevent identity spoofing and ensure secure agent execution.

### What TrustedLaunch Does

TrustedLaunch restricts which executables can run under an MSIX package's identity to **only** those explicitly covered by the package's `CodeIntegrity.cat` catalog. This:

- Prevents package identity spoofing
- Ensures agent/MCP processes cannot be impersonated
- Makes identity-bearing execution cryptographically enforceable

### Required Manifest Elements

Both elements are **MANDATORY** in `<Package><Properties>`:

```xml
<Package xmlns:uap10="http://schemas.microsoft.com/appx/manifest/uap/windows10/10"
         xmlns:trustedlaunch="http://schemas.microsoft.com/appx/manifest/trustedlaunch/windows10">
  <Properties>
    <trustedlaunch:TrustedLaunch>true</trustedlaunch:TrustedLaunch>
    <uap10:PackageIntegrity>
      <uap10:Content Enforcement="on" />
    </uap10:PackageIntegrity>
  </Properties>
</Package>
```

If either is missing, TrustedLaunch is **not active**.

### Signing Requirements

TrustedLaunch relies on **AppxSip-based signing** to:
- Generate `CodeIntegrity.cat` containing hashes of all package executables
- Embed `PackageFullName` into the catalog
- Define which binaries are considered "inside" the package

Standard MSIX signing (including ESRP) is compatible as long as AppxSip is used.

### Enforcement

At process creation time, Windows Code Integrity verifies:
- The EXE hash exists in `CodeIntegrity.cat`
- The process can run under the package identity

Any EXE not covered will fail to launch under the MSIX identity.

### External Binaries (Not Applicable for Azure MCP Server)

For sparse packages that launch binaries outside the MSIX layout, a `CodeIntegrityExternal.cat` is required. Azure MCP Server includes all binaries inside the package, so this is not needed.

---

## Open Questions

1. **Should we generate static_responses at build time or maintain manually?**
   - Recommendation: Build-time generation for accuracy

2. **Do we need ARM64 MSIX packages?**
   - Recommendation: Start with x64 only; add ARM64 based on demand

3. **Should we pursue Microsoft Store distribution?**
   - Recommendation: Phase 2 - start with GitHub Release and sideloading

4. **How do we handle tool discovery for 100+ Azure tools?**
   - Recommendation: Generate tools/list from server at build time

5. **Should MSIX include all tools or allow namespace filtering?**
   - Recommendation: Full server; namespace filtering via runtime args

---

## Implementation Phases

### Phase 1: Core Infrastructure (Week 1-2)
- [x] Create `Pack-Msix.ps1` packaging script (reuses MCPB manifest)
- [x] Create `AppxManifest.template.xml` for Azure.Mcp.Server
- [x] Implement dynamic `_meta.com.microsoft.windows.static_responses` generation
- [x] Asset fallback to MCPB servericon.png
- [x] Test local packaging with MakeAppx.exe/WinAppCli (SDK 10.0.26100.0)
- [x] Test local signing with self-signed certificate
  - WinAppCli has issues with complex Publisher DNs (O=, L=, S=, C= components)
  - Solution: Use SignTool.exe for signing instead of WinAppCli's built-in signing
- [ ] Test ODR registration with `odr.exe mcp list`
- [ ] Validate registration with Windows ODR

### Phase 2: Pipeline Integration (Week 2-3)
- [ ] Create `pack-and-sign-msix.yml` pipeline template
- [ ] Integrate ESRP Authenticode signing
- [ ] Add MSIX to GitHub Release artifacts
- [ ] Update `common.yml` with PackageMSIX parameter
- [ ] Test end-to-end pipeline

### Phase 3: Multi-Server and Store (Week 3-4)
- [ ] Add MSIX support for Fabric.Mcp.Server
- [ ] Add MSIX support for Template.Mcp.Server
- [ ] Document installation and usage
- [ ] Explore Microsoft Store submission requirements
- [ ] Create Winget manifest (future)

---

## Testing Strategy

### Prerequisites

- **Windows build 26220.7262+** (Insider Preview with ODR support)
- **Node.js** - `winget install OpenJS.NodeJS`
- **Windows SDK 10.0.26100.0** (for SignTool/MakeAppx) - `winget install Microsoft.WindowsSDK.10.0.26100`
- **WinAppCli** (optional, preferred for TrustedLaunch) - `winget install Microsoft.WinAppCli`

### Step 1: Create and Install MSIX Package

```powershell
# 1. Build server (self-contained for Windows x64)
dotnet publish servers/Azure.Mcp.Server/src -c Release -r win-x64 --self-contained

# 2. Build MCPB first to get _meta with all tools
./eng/scripts/Pack-Mcpb.ps1 -ServerName "Azure.Mcp.Server" -KeepStagingDirectory

# 3. Create test certificate (first time only)
# NOTE: Subject must match Publisher DN in AppxManifest.template.xml exactly
$certSubject = "CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"
$cert = New-SelfSignedCertificate -Type Custom `
  -Subject $certSubject `
  -KeyUsage DigitalSignature `
  -FriendlyName "Azure MCP Test Certificate" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Export to PFX
$password = ConvertTo-SecureString -String "<test-password>" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".work/test-cert.pfx" -Password $password

# 4. Create signed MSIX package (uses MCPB staging for _meta with tools)
./eng/scripts/Pack-Msix.ps1 -ServerName "Azure.Mcp.Server" `
  -McpbStagingPath ".work/temp_mcpb" `
  -CertificatePath ".work/test-cert.pfx" `
  -CertificatePassword "<test-password>"

# 5. Install the MSIX package (sideload - requires Developer Mode or trusted cert)
Add-AppxPackage -Path ".work/packages_msix/Azure.Mcp.Server/Azure.Mcp.Server-x64.msix"
```

> **Note**: For sideloading to work, either enable Developer Mode in Windows Settings, or
> install the test certificate to the Trusted People store:
> ```powershell
> Import-PfxCertificate -FilePath ".work/test-cert.pfx" -Password $password -CertStoreLocation Cert:\LocalMachine\TrustedPeople
> ```

### Step 2: Verify Windows ODR Registration

```powershell
# List all registered MCP servers
odr.exe mcp list

# Expected output should include:
# - Azure MCP Server
# - Package: Microsoft.Azure.Mcp.Server_2.0.0.0_x64__...
```

If the server appears in this list, registration succeeded.

### Step 3: Test with MCP Client

Use Microsoft's JavaScript client to test the registered server:

```powershell
# Clone the samples repo
git clone https://github.com/microsoft/mcp-on-windows-samples.git
cd mcp-on-windows-samples/mcp-client-js

# Install dependencies and run
npm install
npm start
```

The client will:
1. List all registered MCP servers on Windows
2. Allow you to select a server and invoke tools
3. Display responses for verification

### Step 4: Test Secure Containment

Verify the server runs in the contained agent session:

1. Invoke a tool that requires file access (e.g., storage blob operations)
2. Verify the server runs with limited privileges
3. Check that declared capabilities (e.g., `internetClient`) are respected

### Step 5: Uninstall and Verify Cleanup

```powershell
# Get full package name
Get-AppxPackage -Name "Microsoft.Azure.Mcp.Server"

# Uninstall
Remove-AppxPackage -Package "Microsoft.Azure.Mcp.Server_1.0.0.0_x64__..."

# Verify server is no longer registered
odr.exe mcp list
```

### Quick Reference

| Task | Command |
|------|---------|
| Test server standalone | `npx @modelcontextprotocol/inspector .\azmcp.exe server start` |
| Create MCPB (with tools discovery) | `./eng/scripts/Pack-Mcpb.ps1 -ServerName "Azure.Mcp.Server" -KeepStagingDirectory` |
| Create MSIX (using MCPB staging) | `./eng/scripts/Pack-Msix.ps1 -McpbStagingPath ".work/temp_mcpb"` |
| Create MSIX (from .mcpb file) | `./eng/scripts/Pack-Msix.ps1 -McpbPackagePath ".work/packages_mcpb/Azure.Mcp.Server/Azure.Mcp.Server-win-x64.mcpb"` |
| Create MSIX (minimal, no tools) | `./eng/scripts/Pack-Msix.ps1 -ServerName "Azure.Mcp.Server"` |
| Install MSIX | `Add-AppxPackage -Path ".\Azure.Mcp.Server.msix"` |
| Verify registration | `odr.exe mcp list` |
| Uninstall | `Remove-AppxPackage -Package "Microsoft.Azure.Mcp.Server_..."` |

### Reference Samples

- **[mcp-on-windows-samples](https://github.com/microsoft/mcp-on-windows-samples)**:
  - `mcp-client-js` - JavaScript client to test registered servers
  - `mcp-server-csharp` - C# server example (MSIX or MCPB)
  - `msix-app-with-server` - WinUI 3 app with embedded MCP server

---

## References

- [MCP servers on Windows overview](https://learn.microsoft.com/en-us/windows/ai/mcp/servers/mcp-server-overview)
- [Register an MCP server from an app with package identity](https://learn.microsoft.com/en-us/windows/ai/mcp/servers/mcp-windows-identity)
- [Securely containing MCP servers on Windows](https://learn.microsoft.com/en-us/windows/ai/mcp/servers/mcp-containment)
- [MCP on Windows Samples](https://github.com/microsoft/mcp-on-windows-samples)
- [What is MSIX?](https://learn.microsoft.com/en-us/windows/msix/overview)
- [Package Identity Overview](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/package-identity-overview)
- [App Extension Declaration](https://learn.microsoft.com/en-us/uwp/schemas/appxpackage/uapmanifestschema/element-uap3-appextension-manual)
- [MCPB Manifest Specification](https://github.com/modelcontextprotocol/mcpb/blob/main/MANIFEST.md)

---

## Approval

- [ ] Windows platform team review
- [ ] Security review (containment requirements)
- [ ] Pipeline team review
- [ ] Documentation review

---

**Author:** Azure MCP Team  
**Date:** February 5, 2026  
**Status:** Draft - Initial Planning
