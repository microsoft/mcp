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

The package lock and complete provenance are committed under `eng/sdk-generation` and `sdk/generated/Azure.ResourceManager.CosmosDB/spec.lock.json`.

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
Full generated 1.5.0 SDK build
Projected generated 1.5.0 SDK build
AzureMcp.Cosmos build against the full generated SDK
AzureMcp.Cosmos build against the projected generated SDK
Complete AzureMcp.sln build against the projection
Cosmos unit tests: 3 passed
```

The local machine does not have the .NET 9 ASP.NET Core runtime. Cosmos tests were therefore executed with `DOTNET_ROLL_FORWARD=Major` and ran on the installed newer runtime.

`Build-Local.ps1 -VerifyNpx` was attempted but stopped during restore because the repository's existing `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.12.0 vulnerability warning (`NU1902`) is treated as an error. The solution build succeeds when NuGet audit is disabled; this POC did not change the unrelated OpenTelemetry dependency.

The Native AOT smoke test uses mocked transport and executes:

- subscription account listing;
- pageable response enumeration;
- account resource deserialization and `Data.Name`;
- account key retrieval; and
- key response deserialization and `PrimaryMasterKey`.

A `linux-x64` Native AOT publish and execution passed. Its output directory was approximately 30 MiB. This is validation coverage, not a production distribution comparison.

## Service assembly sizes

| Configuration | Assembly bytes | Individually compressed bytes |
| --- | ---: | ---: |
| Released 1.4.0-beta.13 | 3,555,376 | 793,092 |
| Released 1.5.0 | 3,372,384 | 756,892 |
| Full standalone generated 1.5.0 | 3,313,152 | 732,159 |
| Projected standalone generated 1.5.0 | 1,042,432 | 271,715 |

The projection reduced the standalone generated service assembly by 2,270,720 bytes, or 68.54%. Compared with the released 1.5.0 package, the projected service assembly is 64.10% smaller after individual compression. Both exceed the 50% POC thresholds.

The generated project sets `DebugType=none` and `DebugSymbols=false`. It does not publish a PDB, matching the released package's production output. Before this correction, generated PDBs obscured the service-only and whole-distribution comparisons.

## Controlled `linux-x64` distribution measurements

Each configuration used a Release, self-contained, untrimmed `linux-x64` CLI publish and identical gzip compression. Configurations 2 through 4 used Azure Core 1.60.0, Azure Identity 1.21.0, Azure ResourceManager 1.14.0, and the same MCP source.

| Configuration | Publish bytes | Compressed bytes |
| --- | ---: | ---: |
| 1. Current released 1.4.0-beta.13 package | 264,832,015 | 111,754,353 |
| 2. Released 1.5.0 package | 263,256,263 | 110,936,676 |
| 3. Full standalone generated 1.5.0 | 263,196,774 | 110,913,819 |
| 4. Projected standalone generated 1.5.0 | 260,926,054 | 110,446,632 |

Relevant deltas:

| Comparison | Publish reduction | Compressed reduction |
| --- | ---: | ---: |
| 3 to 4: operation removal | 2,270,720 bytes (0.863%) | 467,187 bytes (0.421%) |
| 2 to 4: same-version package to projection | 2,330,209 bytes (0.885%) | 490,044 bytes (0.442%) |
| 1 to 4: current user-visible result | 3,905,961 bytes (1.475%) | 1,307,721 bytes (1.170%) |

The same-version distribution is smaller, and the service assembly itself is reduced by more than 60% both raw and compressed. The whole-product percentage is necessarily smaller because a single management SDK is one component of an approximately 263 MB self-contained application. Product-level value should be evaluated cumulatively as additional management SDKs are projected.

## Remaining validation

Before generalizing the POC:

1. repeat production measurements for the other supported RIDs;
2. add automated mocked request tests to the normal test suite, beyond the AOT smoke executable;
3. implement semantic SDK-member discovery for adding a previously excluded resource;
4. complete content-addressed full-SDK cache reuse and invalidation tests; and
5. rerun `Build-Local.ps1 -VerifyNpx` after the unrelated `NU1902` restore gate is resolved or waived.
