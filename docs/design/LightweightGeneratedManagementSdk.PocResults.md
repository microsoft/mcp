<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec -->

# Cosmos DB lightweight management SDK POC results

## Status

Initial feasibility, build, hierarchy, and Linux Native AOT validation completed after moving the POC to `microsoft/mcp`. The generated account-management source is substantially smaller than the released Cosmos management dependency, but the complete product is not smaller yet because `Azure.Mcp.Tools.Quota` also references `Azure.ResourceManager.CosmosDB`. The package therefore remains in the server output alongside the projected source.

## Pinned inputs

| Input | Value |
| --- | --- |
| Baseline package | `Azure.ResourceManager.CosmosDB` 1.5.0 |
| `azure-sdk-for-net` tag | `Azure.ResourceManager.CosmosDB_1.5.0` |
| `azure-sdk-for-net` commit | `6f7e0bb7c1e2b849503aa6daf1ecc9d4f3adecc3` |
| `azure-rest-api-specs` commit | `60d0f02991387ea7ed7483d5e70f061f962216cf` |
| TypeSpec directory | `specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB` |
| API version | `2026-03-15` |
| C# management emitter | `1.0.0-alpha.20260722.2` |
| TypeSpec compiler | `1.13.0` |
| Node.js | 24.15.0 |

The package lock and complete provenance are committed under `eng/sdk-generation` and `tools/Azure.Mcp.Tools.Cosmos/src/GeneratedSdk/spec.lock.json`.

## Sparse checkout

The regeneration script initializes partial repositories, fetches the exact commits with `--depth 1 --filter=blob:none`, and uses non-cone sparse patterns.

The specification working tree contains 55 files, approximately 796 KiB, rather than the 10,959 files in the complete DocumentDB directory. In particular, 10,894 example JSON files are not checked out because they are not TypeSpec compiler inputs for this emitter.

The SDK source checkout includes only the released Cosmos custom C# source required to prove the complete baseline. Projected output includes the pinned shared Azure Core source required to compile generated clients outside `azure-sdk-for-net`.

## Operation selection

Direct MCP roots:

- `Microsoft.DocumentDB.DatabaseAccounts.get`
- `Microsoft.DocumentDB.DatabaseAccounts.list`
- `Microsoft.DocumentDB.DatabaseAccounts.listByResourceGroup`
- `Microsoft.DocumentDB.DatabaseAccounts.listKeys`

The four account roots belong to `Microsoft.DocumentDB/databaseAccounts`; Quota's location-list root belongs to `Microsoft.DocumentDB/locations`. Applying the all-operations-for-selected-resource policy produced:

| Metric | Full | Projected |
| --- | ---: | ---: |
| ARM resources | 46 | 2 |
| Resource operations | 233 | 34 |
| Non-resource operations | 3 | 0 |
| Operations scoped out | — | 202 |
| Generated C# files | 1,147 | 427 |
| Generated C# bytes | 9,461,426 | 2,935,928 |

The emitter reported 24 `resource-model-not-associated-with-arm-resource` warnings for models whose operations were deliberately scoped out. They were non-blocking. The projected code model contains exactly the account and location resources and their expected 34 operations.

## Hierarchy validation

The projected `Microsoft.DocumentDB/databaseAccounts` and `Microsoft.DocumentDB/locations` resources retained their exact:

- resource type;
- resource ID pattern;
- resource-group scope;
- parent relationship;
- singleton status; and
- generated `CosmosDBAccountResource` name.

The MCP-owned strict validator passed the unchanged full-baseline subset. Independent negative fixtures passed by correctly rejecting:

1. resource ID drift;
2. scope drift;
3. a casing-only generated name change;
4. an additional resource;
5. a missing resource;
6. parent drift;
7. singleton drift; and
8. a missing generated-name mapping.

## Build and behavior validation

The following checks passed:

```text
Temporary full generated 1.5.0 SDK build
Temporary projected generated 1.5.0 SDK build
Azure.Mcp.Tools.Cosmos build with full generated source
Azure.Mcp.Tools.Cosmos build with projected generated source
Complete Microsoft.Mcp.slnx build against the projection
Cosmos unit tests: 3 passed
```

The local machine does not have the .NET 9 ASP.NET Core runtime. Cosmos tests were therefore executed with `DOTNET_ROLL_FORWARD=Major` and ran on the installed newer runtime.

`Build-Local.ps1 -VerifyNpx` was attempted but stopped during restore because the repository's existing `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.12.0 vulnerability warning (`NU1902`) is treated as an error. The solution build succeeds when NuGet audit is disabled; this POC did not change the unrelated OpenTelemetry dependency.

The Native AOT smoke test compiles the projected source directly through linked compile items, uses mocked transport, and executes:

- subscription account listing;
- pageable response enumeration;
- account resource deserialization and `Data.Name`;
- account key retrieval; and
- key response deserialization and `PrimaryMasterKey`.

A `linux-x64` Native AOT publish and execution passed. Its output directory was approximately 30 MiB. This is validation coverage, not a production distribution comparison.

## Service assembly sizes

Generated source is compiled directly into `Azure.Mcp.Tools.Cosmos.dll`. The current package-based Cosmos tool footprint is the tool assembly plus the released management assembly.

| Configuration | Service assembly bytes | Individually compressed bytes |
| --- | ---: | ---: |
| Current `microsoft/mcp` main: Cosmos tool plus 1.5.0 package | 3,504,992 | 806,757 |
| Cosmos tool with projected account-and-location source | 1,213,440 | 326,010 |

If the released management assembly can be removed from the complete server dependency graph, the projected Cosmos tool footprint is 2,291,552 bytes, or 65.38%, smaller uncompressed and 480,747 bytes, or 59.59%, smaller after individual compression. Both exceed the 50% POC thresholds.

The Cosmos project sets `DebugType=none` and `DebugSymbols=false` for Release builds, so it does not publish a PDB containing generated symbols. Before this correction, generated PDBs obscured the service-only and whole-distribution comparisons.

## `linux-x64` product comparison

Both configurations used a Release, self-contained, untrimmed `linux-x64` server publish and identical gzip compression.

| Product configuration | Publish bytes | Compressed bytes |
| --- | ---: | ---: |
| Current `microsoft/mcp` main | 308,284,413 | 121,247,863 |
| Account-and-location projection branch | 309,300,450 | 121,476,909 |
| Current branch change | +1,016,037 bytes | +229,046 bytes |

The branch is currently larger because it adds projected source but cannot remove `Azure.ResourceManager.CosmosDB.dll`: `Azure.Mcp.Tools.Quota` still uses `SubscriptionResource.GetCosmosDBLocations()` for DocumentDB region discovery. Product savings must not be claimed until the operation inventory includes this consumer and the package is removed from both tool projects.

## Remaining validation

Before evaluating product savings or generalizing the POC:

1. choose a sharing model that allows Cosmos and Quota to consume one generated implementation without duplicating it or retaining the released package;
2. remove `Azure.ResourceManager.CosmosDB` from the complete server dependency graph and repeat the current-main comparison;
3. repeat production measurements for the other supported RIDs;
4. add automated mocked request tests to the normal test suite, beyond the AOT smoke executable;
5. implement semantic SDK-member discovery for adding a previously excluded resource;
6. complete content-addressed full-SDK cache reuse and invalidation tests; and
7. run the current repository's required validation commands.
