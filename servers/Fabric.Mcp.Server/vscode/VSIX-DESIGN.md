# Fabric MCP Server VSIX Extension

## Overview

The Fabric MCP Server VSIX extension provides Fabric MCP server integration and tooling in Visual Studio Code. It enables cross-platform support for the Fabric MCP server, allowing users on Windows, macOS, and Linux to interact with Fabric services through a unified extension experience.

---

## User Experience

- Users install the extension from the Marketplace.
- The extension registers the Fabric MCP server with VS Code on activation.
- No additional downloads or dependencies are required.
- (Optional) Users can configure enabled services in `.vscode/settings.json`:
  ```json
  "fabricMcp.enabledServices": ["publicapis"]
  ```


### Getting Started

Follow these steps to get up and running with the Fabric MCP extension:

1. **Install the extension**
	- Install from the VS Code Marketplace, or
	- Clone/download this repository and install from source.

2. **(Optional) Configure selected services**
	 - Open your workspace settings (`.vscode/settings.json`)
	 - Add or edit the following entry to enable specific Fabric MCP services:

		 ```json
		 "fabricMcp.enabledServices": [
			 "publicapis"
		 ]
		 ```
	 - The extension in next step will start the MCP server with only the selected services enabled.

3. **Start the MCP Server**
	- Open the Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`)
	- Type `MCP: List Servers` and select it
	- Choose `Fabric MCP Server ext` from the list
	- Click `Start Server`

4. **Verify the server is running**
	- Open the `Output` tab in VS Code
	- Look for messages indicating the server started successfully

You're now ready to use Fabric MCP features in VS Code!

---

## Architecture

### 1. Extension Structure

- **Main Entry Point:**
  `src/extension.ts` – Handles activation, server definition, and integration with VS Code APIs.


- **Server Binaries:**
  Platform binaries are located in a flat `server/` folder:
  - `server/fabmcp.exe` (Windows)
  - `server/fabmcp` (Linux/macOS)

- **Packaging Scripts:**
  - `/eng/scripts/New-BuildInfo.ps1` – Creates a build_info.json file used in build and pack scripts.
  - `/eng/scripts/Build-Code.ps1` – Builds server binaries using data from build_info.json.
  - `/eng/scripts/Pack-Vsix.ps1` – Runs VSIX packaging on servers binaries using data from build_info.json.
  - `package.json` – Defines build, test, and packaging scripts.

---

### 2. Build & Packaging Process

#### a. Server Build

- Run `/eng/scripts/New-BuildInfo.ps1` to prepare a build_info.json file that will provide metadata to the build and pack scripts.
- The .NET MCP server is built for each supported platform (x64 and arm64) using `/eng/scripts/Build-Code.ps1 -Server Fabric.Mcp.Server`.
- Output binaries are placed in the `.work/build/Fabric.Mcp.Server/{platform}` folders as `fabmcp.exe` (Windows) and `fabmcp` (Linux/macOS).

#### b. VSIX Packaging

- The `/eng/scripts/Pack-Vsix.ps1` script:
  1. Copies the output from `Build-Code.ps1`, along with this `vsix` directory into a temporary directory
     - By default, all files in the extension folder (including `server/<os>` binaries) are included in the VSIX unless excluded by `.vscodeignore`.
  2. Runs `vsce package` to create the VSIX.
  3. Produces output in `.work/packages_vsix/Fabric.Mcp.Server`
---

### 3. Extension Activation & Server Launch

- On activation, the extension:
  - Detects the user's platform (`process.platform`) and architecture (`process.arch`).
  - Targets the appropriate server binary from the flat `server/` folder.
  - Registers the server with VS Code using the MCP API.

#### Example (simplified):

```typescript
let binary = '';
if (process.platform === 'win32') {
    binary = 'fabmcp.exe';
} else if (process.platform === 'darwin' || process.platform === 'linux') {
    binary = 'fabmcp';
}
const serverPath = path.join(context.extensionPath, 'server', binary);
```

---


### 4. Cross-Platform Support

- The VSIX is shipped per platform (Windows, Linux, macOS), with binaries for each supported architecture included for that platform.
- Supported platforms and architectures:
  - **Windows**: x64, arm64
  - **Linux**: x64, arm64
  - **macOS**: x64, arm64
- No runtime download is required; users do not need Node.js, npx, or .NET installed.
- The extension is self-contained and works offline after installation.

---

### 5. Alternative Dynamic Download (Optimization)

- As an optimization, the extension could download the latest platform-specific server binary from a public URL at runtime.
- This reduces VSIX size but requires HTTP(S) download logic.
- Not currently implemented in the default design for maximum compatibility and offline support.
- Note: This can also be achieved by dynamically downloading the platform-specific package via npm, but this approach introduces a dependency on Node.js for the end user.
---


### 6. Exclusion & Inclusion

- `.vscodeignore` can be used to exclude files from the VSIX if needed.
- By default, all `server/` binaries are included.

---


## File/Folder Structure

```
vsix/
├── src/
│   └── extension.ts
├── server/
│   ├── fabmcp.exe
│   └── fabmcp
├── package.json
└── ...
```

---

## References

- [VS Code Extension Packaging](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [VSCE CLI](https://code.visualstudio.com/api/working-with-extensions/publishing-extension#vsce)
