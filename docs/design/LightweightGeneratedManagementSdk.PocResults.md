<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec -->

# Cosmos DB lightweight management SDK POC results

## Status

Initial feasibility, build, hierarchy, Linux Native AOT, and controlled `linux-x64` distribution measurements completed. The projection substantially reduces both the raw and compressed Cosmos service assembly and provides a positive same-version production-distribution benefit. It meets the percentage-based POC size criteria.

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

- `Microsoft.DocumentDB.DatabaseAccounts.list`
- `Microsoft.DocumentDB.DatabaseAccounts.listKeys`

Both roots belong to `Microsoft.DocumentDB/databaseAccounts`. Applying the all-operations-for-selected-resource policy produced:

| Metric | Full | Projected |
| --- | ---: | ---: |
| ARM resources | 46 | 1 |
| Resource operations | 233 | 32 |
| Non-resource operations | 3 | 0 |
| Operations scoped out | — | 204 |
| Generated C# files | 1,147 | 414 |
| Generated C# bytes | 9,461,426 | 2,837,734 |

The emitter reported 25 `resource-model-not-associated-with-arm-resource` warnings for models whose operations were deliberately scoped out. They were non-blocking. The projected code model contains exactly one resource and the expected 32 operations.

## Hierarchy validation

The projected `Microsoft.DocumentDB/databaseAccounts` resource retained its exact:

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
Complete AzureMcp.sln build against the projection
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

Generated source is compiled directly into `Azure.Mcp.Tools.Cosmos.dll`. Package configurations therefore include both the area assembly and released management assembly, while generated configurations contain one merged area assembly.

| Configuration | Service assembly bytes | Individually compressed bytes |
| --- | ---: | ---: |
| Current 1.4.0-beta.13 package plus Cosmos area | 3,618,352 | 818,308 |
| Released 1.5.0 package plus Cosmos area | 3,435,360 | 782,111 |
| Full generated 1.5.0 source in Cosmos area | 3,376,128 | 759,765 |
| Projected generated source in Cosmos area | 1,103,360 | 293,305 |

The projection reduced the merged full-generated Cosmos area assembly by 2,272,768 bytes, or 67.32%. Compared with the released 1.5.0 package plus its area assembly, the merged projected assembly is 62.50% smaller after individual compression. Both exceed the 50% POC thresholds.

The Cosmos project sets `DebugType=none` and `DebugSymbols=false` for Release builds, so it does not publish a PDB containing generated symbols. Before this correction, generated PDBs obscured the service-only and whole-distribution comparisons.

## Controlled `linux-x64` distribution measurements

Each configuration used a Release, self-contained, untrimmed `linux-x64` CLI publish and identical gzip compression. Configurations 2 through 4 used Azure Core 1.60.0, Azure Identity 1.21.0, Azure ResourceManager 1.14.0, and the same MCP source.

| Configuration | Publish bytes | Compressed bytes |
| --- | ---: | ---: |
| 1. Current released 1.4.0-beta.13 package | 264,782,113 | 111,719,121 |
| 2. Released 1.5.0 package | 263,206,361 | 110,907,741 |
| 3. Full generated 1.5.0 source | 263,146,370 | 110,883,385 |
| 4. Projected generated source | 260,873,602 | 110,413,764 |

Relevant deltas:

| Comparison | Publish reduction | Compressed reduction |
| --- | ---: | ---: |
| 3 to 4: operation removal | 2,272,768 bytes (0.864%) | 469,621 bytes (0.424%) |
| 2 to 4: same-version package to projection | 2,332,759 bytes (0.886%) | 493,977 bytes (0.445%) |
| 1 to 4: current user-visible result | 3,908,511 bytes (1.476%) | 1,305,357 bytes (1.168%) |

The same-version distribution is smaller, and the service assembly itself is reduced by more than 60% both raw and compressed. The whole-product percentage is necessarily smaller because a single management SDK is one component of an approximately 263 MB self-contained application. Product-level value should be evaluated cumulatively as additional management SDKs are projected.

## Remaining validation

Before generalizing the POC:

1. repeat production measurements for the other supported RIDs;
2. add automated mocked request tests to the normal test suite, beyond the AOT smoke executable;
3. implement semantic SDK-member discovery for adding a previously excluded resource;
4. complete content-addressed full-SDK cache reuse and invalidation tests; and
5. rerun `Build-Local.ps1 -VerifyNpx` after the unrelated `NU1902` restore gate is resolved or waived.
