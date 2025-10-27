# Build Scripts

### [eng/scripts/New-BuildInfo.ps1](https://github.com/microsoft/mcp/blob/main/eng/scripts/New-BuildInfo.ps1)

To simplify the work of collection server and platform metadata in build scripts, we use the common metadata file `build_info.json`. `New-BuildInfo.ps1` creates the `build_info.json` file containing server details, along with a list of projects that should be tested in a CI/PR build and matrices for CI build and test stages.

`./eng/scripts/New-BuildInfo.ps1` produces `.work/build_info.json` with contents like:
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

### [eng/scripts/Build-Code.ps1](https://github.com/microsoft/mcp/blob/main/eng/scripts/Build-Code.ps1)
`Build-Code.ps1` is a common build script that compiles server projects in the repository, using the metadata collected in `build_info.json`. It supports building for multiple platforms, including native builds, and can be used in both local and CI environments.

Build-Code.ps1 defaults to:
- `-BuildInfoPath`: `.work/build_info.json`, the default output path of `New-BuildInfo.ps1`
- `-OutputPath`: `.work/build`, the default -ArtifactsPath for the Pack scripts
- `-ServerName`: empty, which builds all servers listed in `build_info.json`.
- `-OperatingSystem`, `-Architecture`, and `-AllPlatforms`: all empty or false, which builds for the current, local platform only.

For more production-like builds, you can enable the switches: `-SelfContained -SingleFile -Trimmed`

So, a parameterless `./eng/scripts/Build-Code.ps1` will build all servers listed in `build_info.json` for the local platform as framework-dependent, non-trimmed, non-single-file applications, outputting them to the default path `.work/build`.

### [eng/scripts/Compress-ForSigning.ps1](https://github.com/microsoft/mcp/blob/main/eng/scripts/Compress-ForSigning.ps1)
`Compress-ForSigning.ps1` collects the build output from `Build-Code.ps1`, organizes it into a standardized folder structure, and compresses Mac binaries into ZIP archives suitable for signing. This script uses the metadata in `build_info.json` to determine the correct paths and naming conventions for the ZIP files.

`Compress-ForSigning.ps1` doesn't have any use in local development other than to test that its output is correct for later signing in a CI pipeline.

# Published artifact types

### npm/npx
- packed with [`Pack-Npm.ps1`](https://github.com/microsoft/mcp/blob/main/eng/scripts/Pack-Npm.ps1)
- released via [`release-npm.yml`](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/npm/release-npm.yml)
- required project file properties:
  - `NpmPackageName`: the package name shown on npmjs.com (for example, `@azure-tools/azure-mcp`).
  - `NpmDescription`: (optional, defaults to `Description` if not set) the package description shown on npmjs.com
  - `NpmPackageKeywords`: (comma separated) an array of keywords to help users find the package on npmjs.com

Servers are packed into two npm package types:
1. A platform-specific package for each OS/architecture combination (for example, `@azure-tools/azure-mcp-linux-x64`), containing the native binaries for that platform.
2. A wrapper package (for example, `@azure-tools/azure-mcp`) that depends on the platform-specific packages and provides a unified CLI experience via `npx` (for example, `npx @azure-tools/azure-mcp@latest server start`).


### NuGet/dnx
- packed with [`Pack-Nuget.ps1`](https://github.com/microsoft/mcp/blob/main/eng/scripts/Pack-Nuget.ps1)
- released via [`release-nuget.yml`](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/nuget/release-nuget.yml)
- required project file properties:
  - `DnxPackageId`: the package name shown on nuget.org (for example, `Azure.Mcp`).
  - `DnxDescription`: (optional, defaults to `Description` if not set) the package description shown on nuget.org
  - `DnxToolCommandName`: (optional, defaults to `CliName`) the command users run after installing the package as a global tool (for example, `azmcp`).
  - `DnxPackageTags`: (comma separated) an array of keywords to help users find the package on nuget.org


### Docker
- packed with [`Prepare-Docker.ps1`](https://github.com/microsoft/mcp/blob/main/eng/scripts/Prepare-Docker.ps1)
- released via [`release-docker.yml`](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/docker/release-docker.yml)
- required project file properties:
  - `DockerImageName`: the name of the Docker image (for example, `azure-sdk/azure-mcp`).


### VSIX
- packed with [`Pack-Vsix.ps1`](https://github.com/microsoft/mcp/blob/main/eng/scripts/Pack-Vsix.ps1)
- released via [`release-vsix.yml`](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/vsix/release-vsix.yml)
- requires customized package manifest and assets under `servers/<ServerName>/vscode`


### Zip
- packed with [`Pack-Zip.ps1`](https://github.com/microsoft/mcp/blob/main/eng/scripts/Pack-Zip.ps1)
- added to GitHub releases via [`release.yml`](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/release.yml)
- no special project file properties required

Zip files for each server in a release are added to the GitHub release assets.

# Server release process

We only ship the repo's server projects, with each server stored under `servers/<ServerName>`. The release pipelines are driven by the server-specific `build.yml` files (for example, `servers/Azure.Mcp.Server/build.yml`). Those files opt a server into the artifact types we publish and pass the settings that flow through `/eng/pipelines/templates/common.yml`.

## Queuing a release run
- Manually queue the server pipeline (e.g. [`mcp - Azure.Mcp.Server`](https://dev.azure.com/azure-sdk/internal/_build?definitionId=7866&_a=summary)) with `PublishTarget` set to `none`, `internal`, or `public`.
  - `none` builds and validates only. Outputs are saved as unsigned pipeline artifacts. Servers will be given a prerelease version based on the version in the servers' project file, with a suffix like `-alpha.<buildId>`.
  - `internal` runs signing, packaging and the internal integration stage. Servers will also be given a prerelease version for internal release based on the project file version. Outputs are saved as pipeline artifacts and pushed to internal registries.
  - `public` runs signing, packaging, and the release jobs in [eng/pipelines/templates/jobs/release.yml](https://github.com/microsoft/mcp/blob/main/eng/pipelines/templates/jobs/release.yml). This creates the GitHub release tag and pushes artifacts to their public registries. Public releases use the version in the server project file as-is, not dynamic prerelease versions.
- **Expect post-release automation**: After a successful public release, `jobs/release.yml` runs `Update-Version.ps1` to increment the server version and open a PR. This PR must be merged before the next release can start.

## Server release configuration
The parameters in a server's `build.yml` decide what we produce. Set `PackageDocker: true` to include Docker images, `PackageVSIX: true` to include VS Code extensions. Every pipeline always creates the npm/npx and NuGet/dnx artifacts.

## Project file properties
Packaging and release scripts depend on metadata found in a build's build_info.json file, which is generated by `eng/scripts/New-BuildInfo.ps1` using properties from each server's project file. See [eng/scripts/Get-ProjectProperties.ps1](https://github.com/microsoft/mcp/blob/main/eng/scripts/Get-ProjectProperties.ps1) for a list of all supported project file properties.
