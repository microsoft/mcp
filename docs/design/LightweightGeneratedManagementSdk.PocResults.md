<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec -->

# Cosmos DB lightweight management SDK POC results

## Status

Initial feasibility, build, hierarchy, Linux Native AOT, and controlled `linux-x64` distribution measurements completed. The projection proves a same-version distribution benefit and passes the operation-removal threshold, but it does not meet the plan's preliminary 1 MiB same-version threshold. Under the current acceptance criteria, the POC is not yet successful.

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

## Preliminary assembly sizes

| Configuration | Assembly bytes |
| --- | ---: |
| Released 1.4.0-beta.13 | 3,503,176 |
| Released 1.5.0 | 3,366,208 |
| Full standalone generated 1.5.0 | 3,619,328 |
| Projected standalone generated 1.5.0 | 1,123,840 |

The projection reduced the standalone service assembly by 2,495,488 bytes, or 68.95%, exceeding the preliminary 50% assembly threshold.

## Controlled `linux-x64` distribution measurements

Each configuration used a Release, self-contained, untrimmed `linux-x64` CLI publish and identical gzip compression. Configurations 2 through 4 used Azure Core 1.60.0, Azure Identity 1.21.0, Azure ResourceManager 1.14.0, and the same MCP source.

| Configuration | Publish bytes | Compressed bytes |
| --- | ---: | ---: |
| 1. Current released 1.4.0-beta.13 package | 264,832,015 | 111,754,353 |
| 2. Released 1.5.0 package | 263,256,263 | 110,936,676 |
| 3. Full standalone generated 1.5.0 | 265,927,610 | 112,861,973 |
| 4. Projected standalone generated 1.5.0 | 261,320,018 | 110,598,962 |

Relevant deltas:

| Comparison | Publish reduction | Compressed reduction |
| --- | ---: | ---: |
| 3 to 4: operation removal | 4,607,592 bytes (1.733%) | 2,263,011 bytes (2.005%) |
| 2 to 4: same-version package to projection | 1,936,245 bytes (0.735%) | 337,714 bytes (0.304%) |
| 1 to 4: current user-visible result | 3,511,997 bytes (1.326%) | 1,155,391 bytes (1.034%) |

The 3-to-4 result exceeds the 1% operation-removal threshold. The 2-to-4 result is a real same-version reduction but is below the preliminary 1 MiB threshold. The 1-to-4 result exceeds 1 MiB, but the design correctly does not use that comparison alone to establish projection benefit.

The standalone full generation is larger than the released package because it carries generation/runtime support differently. This is why both the 2-to-4 and 3-to-4 controls are necessary.

## Remaining validation and decision

Before declaring the POC complete:

1. decide whether a 337,714-byte same-version compressed reduction justifies the maintenance cost or retain the 1 MiB threshold and stop the rollout;
2. repeat production measurements for the other supported RIDs if the POC continues;
3. add automated mocked request tests to the normal test suite, beyond the AOT smoke executable;
4. implement semantic SDK-member discovery for adding a previously excluded resource;
5. complete content-addressed full-SDK cache reuse and invalidation tests; and
6. rerun `Build-Local.ps1 -VerifyNpx` after the unrelated `NU1902` restore gate is resolved or waived.
