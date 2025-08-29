# How the .NET Tool packaging process works

Much like the [npm packages](https://github.com/azure/azure-mcp/blob/main/eng/npm/README.md), the Azure MCP server is published as a .NET Tool that supports specific platforms. This feature is new as of [.NET 10 preview 6](https://github.com/dotnet/core/blob/main/release-notes/10.0/preview/preview6/sdk.md#platform-specific-net-tools).

To make platform-specific .NET Tools work, it is necessary to publish

* a platform-agnostic tool that contains a manifest with all supported platform-specific tool packages
* N different platform-specific tool packages

In .NET 10, all of the orchestration required to generate these is contained in three gestures
* Setting relevant `<RuntimeIdentifiers>` values in the project file
* Setting the `<PackAsTool>` property to `true` in the project file
* building the packages with `dotnet pack`

As long as the application is not using AOT, this single gesture will create all of the required NuGet packages, which can then be published to feeds as necessary.

## Supporting AOT packages

The .NET Tools feature does support AOT platform-specific packages (aka setting `<PublishAot>` to true in the project file), but because the .NET Toolchain does not support cross-platform AOT compilation, the individual platform-specific packages must be built on each platform, often through a CI/CD system's ability to matrix across platforms.  In this case, the command to build the platform-specific packages would be

```
dotnet pack -r <runtime identifier>
```

In most cases, you can rely on the .NET SDK to fill in the appropriate RID for the current host by using

```
dotnet pack --use-current-runtime
```

In addition, you will need to create the 'wrapper' package separately via the following command:

```
dotnet pack
```

Once you have all N packages you can publish them to feeds as you would any package.