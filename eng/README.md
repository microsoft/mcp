# Build Scripts

### [eng/scripts/New-BuildInfo.ps1](scripts/New-BuildInfo.ps1)

To simplify the work of collection server and platform metadata in build scripts, we use the common metadata file `build_info.json`. `New-BuildInfo.ps1` creates the `build_info.json` file containing server details, along with a list of projects that should be tested in a CI/PR build amd matrices for CI build and test stages.

```powershell
.\eng\scripts\New-BuildInfo.ps1
```

produces `.work/build_info.json` with contents like:
```json
{
  "buildId": 99999,
  "publishTarget": "none",
  "dynamicPrereleaseVersion": true,
  "repositoryUrl": "https://github.com/microsoft/mcp",
  "branch": "main",
  "commitSha": "8ae9e6f971b0eb97c1e64534773cc9e6045952a8",
  "servers": [
    {
      "name": "Template.Mcp.Server",
      "path": "servers/Template.Mcp.Server/src/Template.Mcp.Server.csproj",
      "artifactPath": "Template.Mcp.Server",
      "version": "0.0.12-alpha.99999",
      "releaseTag": "Template.Mcp.Server-0.0.12-alpha.99999",
      "cliName": "mcptmp",
      "assemblyTitle": "Template MCP Server",
      "description": "Template MCP Server - Basic Model Context Protocol implementation",
      "readmeUrl": "https://github.com/Microsoft/mcp/blob/main/servers/Template.Mcp.Server#readme",
      "readmePath": "servers/Template.Mcp.Server/README.md",
      "packageIcon": "eng/images/microsofticon.png",
      "npmPackageName": "@azure/mcp-template",
      "npmDescription": "",
      "npmPackageKeywords": [
        "azure",
        "mcp",
        "model-context-protocol"
      ],
      "dockerImageName": "azure-sdk/template-mcp",
      "dockerDescription": "",
      "dnxPackageId": "Microsoft.Template.Mcp",
      "dnxDescription": "",
      "dnxToolCommandName": "mcptmp",
      "dnxPackageTags": [
        "template",
        "mcp"
      ],
      "vsixPackagePrefix": "vscode-template-mcp",
      "vsixDescription": "Template project for validating the microsoft/mcp engineering system",
      "vsixPublisher": "ms-azuretools",
      "platforms": [
        {
          "name": "linux-x64",
          "artifactPath": "Template.Mcp.Server/linux-x64",
          "operatingSystem": "linux",
          "nodeOs": "linux",
          "dotnetOs": "linux",
          "architecture": "x64",
          "extension": "",
          "native": false
        },
        ...
      ]
    }
  ],
  "pathsToTest": [
    {
      "path": "core/Microsoft.Mcp.Core",
      "testResourcesPath": null,
      "hasLiveTests": false,
      "hasTestResources": false,
      "hasUnitTests": false
    },
    {
      "path": "core/Template.Mcp.Core",
      "testResourcesPath": null,
      "hasLiveTests": false,
      "hasTestResources": false,
      "hasUnitTests": false
    },
    {
      "path": "servers/Template.Mcp.Server",
      "testResourcesPath": null,
      "hasLiveTests": false,
      "hasTestResources": false,
      "hasUnitTests": false
    },
    {
      "path": "tools/Fabric.Mcp.Tools.PublicApi",
      "testResourcesPath": null,
      "hasLiveTests": false,
      "hasTestResources": false,
      "hasUnitTests": false
    }
  ],
  "matrices": {
    "linuxBuildMatrix": {
      "linux_x64": {
        "Pool": "Missing-LINUXPOOL",
        "OSVmImage": "Missing-LINUXVMIMAGE",
        "Architecture": "x64",
        "Native": false,
        "RunUnitTests": true
      },
      "linux_arm64": {
        ...
      }
    },
    "linuxSmokeTestMatrix": {
      "linux_x64": {
        "Pool": "Missing-LINUXPOOL",
        "OSVmImage": "Missing-LINUXVMIMAGE",
        "Architecture": "x64"
      }
    },
    "macosBuildMatrix": {
      "macos_x64": {},
      "macos_arm64": {},
    },
    ...
    "liveTestMatrix": {
      "tools/Azure.Mcp.Tools.Workbooks": {
        "pathToTest": "tools/Azure.Mcp.Tools.Workbooks",
        "testResourcesPath": "tools/Azure.Mcp.Tools.Workbooks/tests",
        "hasTestResources": true
      },
      ...
    }
  }
}
```

### [eng/scripts/Build-Code.ps1](scripts/Build-Code.ps1)
`Build-Code.ps1` is a common build script that compiles server projects in the repository, using the metadata collected in `build_info.json`. It supports building for multiple platforms, including native builds, and can be used in both local and CI environments.

Build-Code.ps1 defaults to:
- `-BuildInfoPath`: `.work/build_info.json`, the default output path of `New-BuildInfo.ps1`
- `-OutputPath`: `.work/build`, the default -ArtifactsPath for the Pack scripts
- `-ServerName`: empty, which builds all servers listed in `build_info.json`.
- `-OperatingSystem`, `-Architecture`, and `-AllPlatforms`: all empty or false, which builds for the current, local platform only.

For more production-like builds, you can enable the switches: `-SelfContained -SingleFile -Trimmed`

So, a parameterless `./eng/scripts/Build-Code.ps1` will build all servers listed in `build_info.json` for the local platform as framework-dependent, non-trimmed, non-single-file applications, outputting them to the default path `.work/build`.

### [eng/scripts/Compress-ForSigning.ps1](scripts/Compress-ForSigning.ps1)

`Compress-ForSigning.ps1` collect the build output from `Build-Code.ps1`, organizes it into a standardized folder structure, and compresses Mac binaries into ZIP archives suitable for signing. This script uses the metadata in `build_info.json` to determine the correct paths and naming conventions for the ZIP files.

`Compress-ForSigning.ps1` doesn't have any use in local development other than to test that its output is correct for later signing in a CI pipeline.

# Packaging scripts

Packaging scripts create distributable packages from the output of `Build-Code.ps1`, using the metadata collected in `build_info.json`.  Each packaging script targets a specific package format, such as npm, Docker, or VSIX. `build_info.json` lists an `artifactPath` for each server / platform combination.  This is the path that the packing scripts use to locate the files to package.

Packing scripts currently include:
- [eng/scripts/Pack-Npm.ps1](scripts/Pack-Npm.ps1) for creating [npm packages for use with `npx`](https://docs.npmjs.com/cli/v9/commands/npx?v=true)
- [eng/scripts/Pack-Nuget.ps1](scripts/Pack-Nuget.ps1) for creating [NuGet packages for use with `dnx`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-exec)
- [eng/scripts/Pack-Vsix.ps1](scripts/Pack-Vsix.ps1) for creating [VS Code VSIX packages](https://code.visualstudio.com/api/working-with-extensions/publishing-extension#packaging-extensions)
- [eng/scripts/Pack-Zip.ps1](scripts/Pack-Zip.ps1) for creating ZIP archives
- [eng/scripts/Prepare-Docker.ps1](scripts/Prepare-Docker.ps1) for staging the contents of Docker images



