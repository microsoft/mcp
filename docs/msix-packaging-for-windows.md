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
│   ├── Pack-Msix.ps1                         # MSIX packaging script
│   └── New-MsixManifest.ps1                  # Generate AppxManifest.xml
├── pipelines/
│   └── templates/
│       └── jobs/
│           └── msix/
│               ├── pack-and-sign-msix.yml    # Pack and sign pipeline
│               └── release-msix.yml          # Release pipeline
servers/
└── {ServerName}/
    └── msix/
        ├── AppxManifest.template.xml         # Template manifest
        ├── manifest.json                     # MCP manifest with static_responses
        └── Assets/
            ├── StoreLogo.png                 # 50x50
            ├── Square44x44Logo.png           # 44x44
            ├── Square150x150Logo.png         # 150x150
            └── Wide310x150Logo.png           # 310x150
```

### Static Responses Generation

The `_meta.com.microsoft.windows.static_responses` must match what the server returns at runtime. We have two options:

**Option A: Manual Maintenance**
- Manually maintain the `tools/list` response in manifest.json
- Pros: Simple, no build-time generation
- Cons: Can get out of sync with actual server

**Option B: Build-Time Generation** (Recommended)
- Generate static responses by querying the server during build
- Run `azmcp server start` → send `initialize` and `tools/list` requests → capture responses
- Inject responses into manifest.json
- Pros: Always in sync
- Cons: Requires running server during build

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
- [ ] Create `Pack-Msix.ps1` packaging script
- [ ] Create `AppxManifest.template.xml` for Azure.Mcp.Server
- [ ] Generate `manifest.json` with static_responses from server
- [ ] Create asset images (logos)
- [ ] Test local packaging with MakeAppx.exe
- [ ] Test local signing with self-signed certificate
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
- **Windows SDK** (for SignTool/MakeAppx) - `winget install Microsoft.WindowsSDK.10.0.26100`

### Step 1: Create and Install MSIX Package

```powershell
# 1. Build server (self-contained for Windows x64)
dotnet publish servers/Azure.Mcp.Server/src -c Release -r win-x64 --self-contained

# 2. Create MSIX package
./eng/scripts/Pack-Msix.ps1 -ServerName "Azure.Mcp.Server"

# 3. Create test certificate (first time only)
$cert = New-SelfSignedCertificate -Type Custom `
  -Subject "CN=Azure MCP Test, O=Microsoft Corporation, C=US" `
  -KeyUsage DigitalSignature `
  -FriendlyName "Azure MCP Test Certificate" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")

# 4. Sign package with test certificate
SignTool.exe sign /fd SHA256 /a /f "TestCert.pfx" /p "password" `
  ".work/packages_msix/Azure.Mcp.Server.msix"

# 5. Install the MSIX package (sideload)
Add-AppxPackage -Path ".work/packages_msix/Azure.Mcp.Server.msix"
```

### Step 2: Verify Windows ODR Registration

```powershell
# List all registered MCP servers
odr.exe mcp list

# Expected output should include:
# - Azure MCP Server
# - Package: Microsoft.Azure.Mcp.Server_1.0.0.0_x64__...
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

| Test | Command |
|------|---------|
| Test server standalone | `npx @modelcontextprotocol/inspector .\azmcp.exe server start` |
| Create MSIX | `./eng/scripts/Pack-Msix.ps1 -ServerName "Azure.Mcp.Server"` |
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
