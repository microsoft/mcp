<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec -->

# Cosmos DB lightweight management SDK POC results

## Status

Initial feasibility, build, hierarchy, Linux Native AOT, shared-consumer integration, and `linux-x64` product measurement completed. Cosmos and Quota now reference one generated account-and-location SDK, and the released `Azure.ResourceManager.CosmosDB` package is absent from the server dependency graph.

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

The package lock and complete provenance are committed under `eng/sdk-generation` and `eng/generated/Azure.Mcp.Generated.CosmosDB/spec.lock.json`.

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
- `Microsoft.DocumentDB.LocationGetResults.list`

The four account roots belong to `Microsoft.DocumentDB/databaseAccounts`; Quota's location-list root belongs to `Microsoft.DocumentDB/locations`. Applying `minimumHierarchyClosure` added only the location `Read` required to preserve that resource:

| Metric | Full | Projected |
| --- | ---: | ---: |
| ARM resources | 46 | 2 |
| Resource operations | 233 | 6 |
| Non-resource operations | 3 | 0 |
| Operations scoped out | — | 230 |
| Generated C# files | 1,147 | 146 |
| Generated C# bytes | 9,461,426 | 859,865 |

The emitter reported 24 `resource-model-not-associated-with-arm-resource` warnings for models whose operations were deliberately scoped out. They were non-blocking. The projected code model contains exactly the account and location resources. It retains five directly used operations plus the location `Read` required by `minimumHierarchyClosure`.

## Hierarchy validation

The projected `Microsoft.DocumentDB/databaseAccounts` and `Microsoft.DocumentDB/locations` resources retained their exact:

- resource type;
- resource ID pattern;
- resource-group or subscription scope;
- parent relationship;
- singleton status; and
- generated `CosmosDBAccountResource` or `CosmosDBLocationResource` name.

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
Shared generated SDK build
Azure.Mcp.Tools.Cosmos build against the shared projection
Azure.Mcp.Tools.Quota build against the shared projection
Cosmos tests: 201 passed
Quota tests: 23 passed
```

A full `Microsoft.Mcp.slnx` build was attempted but was blocked by a `401 Unauthorized` response while the unrelated `GitHub.Copilot.SDK` target downloaded `copilot-linux-x64-1.0.78.tgz` from the Azure SDK npm feed.

The Native AOT smoke test references the shared generated project, uses mocked transport, and executes:

- subscription account listing;
- pageable response enumeration;
- account resource deserialization and `Data.Name`;
- account key retrieval;
- key response deserialization and `PrimaryMasterKey`; and
- location listing and region-access deserialization.

A `linux-x64` Native AOT publish and execution passed. Its output directory was approximately 30 MiB. This is validation coverage, not a production distribution comparison.

## Affected service footprint

The current footprint is the Cosmos and Quota tool assemblies plus the released management assembly. The projected footprint is the same tool assemblies plus one shared generated assembly.

| Configuration | Assembly bytes | Individually compressed bytes |
| --- | ---: | ---: |
| Current `microsoft/mcp` main | 3,618,656 | 850,330 |
| Shared minimum projection | 649,216 | 224,960 |
| Reduction | 2,969,440 (82.06%) | 625,370 (73.54%) |

The generated project disables Release PDB output, matching the released package's production symbol footprint.

## `linux-x64` product comparison

Both configurations used a Release, self-contained, untrimmed `linux-x64` server publish and deterministic tar/gzip compression with sorted entries, fixed timestamps, and normalized ownership.

| Product configuration | Publish bytes | Compressed bytes |
| --- | ---: | ---: |
| Current `microsoft/mcp` main | 308,284,413 | 121,247,863 |
| Shared minimum projection | 305,324,653 | 120,378,574 |
| Reduction | 2,959,760 bytes (0.960%) | 618,251 bytes (0.511%) |

`dotnet list package --include-transitive` confirms that `Azure.ResourceManager.CosmosDB` is absent as a NuGet dependency. The MCP-owned projection is emitted as `Azure.Mcp.Generated.CosmosDB.dll` to distinguish it from the released SDK package.

## Remaining validation

Before generalizing the POC:

1. repeat production measurements for the other supported RIDs;
2. add the mocked management requests to the normal tool test suites, beyond the AOT smoke executable;
3. implement semantic SDK-member-to-operation discovery for proposing root changes;
4. add isolated cache invalidation and corruption fixtures; and
5. run the current repository's required full validation commands.
