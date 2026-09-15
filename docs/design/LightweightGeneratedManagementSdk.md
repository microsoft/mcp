<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec projectable -->

# Lightweight generated management SDK

## Status

Proposed. The first proof of concept (POC) targets `Azure.ResourceManager.CosmosDB` 1.5.0. Generated source must not replace the released package in production until provenance, behavior, hierarchy, AOT compatibility, and product-size reduction are validated for every package consumer.

## Motivation

Azure MCP wraps a small subset of many Azure management SDKs, while the released packages contain resources, operations, and models that MCP never calls. The proposal generates a service-specific projection from the exact specification revision associated with the selected .NET SDK package and scopes unrelated operations out of C# generation.

Goals:

- reduce the published MCP size;
- preserve complete selected ARM resources and their hierarchy;
- make generation pinned, reproducible, reviewable, and independent of normal builds;
- commit generated C# rather than fetching specifications during build; and
- preserve the existing AI-assisted contribution workflow.

The POC does not replace `Azure.Core`, `Azure.Identity`, `Azure.ResourceManager`, or Cosmos data-plane packages. It does not require preserving public APIs that MCP does not use, publishing a general-purpose SDK, or supporting Swagger-based projection.

## Developer and agent experience

A developer should continue to request a command using the repository's normal prompt pattern, for example:

```text
Create the Cosmos DB account key command using the repository command-authoring skill. Use Azure.ResourceManager.CosmosDB.
```

The developer does not supply specification SHAs, TypeSpec paths, operation IDs, scope decorators, or hierarchy metadata. The agent must:

1. identify all projects using the requested package;
2. resolve package provenance and sparse-checkout the pinned specification;
3. generate or reuse the matching full SDK for semantic discovery;
4. map referenced SDK members to TypeSpec operation IDs;
5. update reviewed root-operation evidence;
6. expand roots to complete resource closures;
7. generate and validate the projection; and
8. update helpers, tests, and Azure CLI validation steps required by the command.

If the POC succeeds, this workflow should be added to the repository's Copilot instructions and command-authoring skill.

## Reproducible source provenance

Every projection has a machine-readable chain:

```text
MCP package ID and version
  -> azure-sdk-for-net release tag and commit
  -> azure-rest-api-specs repository, commit, and paths
  -> pinned Node, TypeSpec libraries, and management emitter
  -> generated source
```

Resolve provenance from the matching `azure-sdk-for-net` release tag:

- `tsp-location.yaml` for TypeSpec-generated packages;
- `src/autorest.md` for Swagger-generated packages.

Generation must fail rather than fall back to `azure-rest-api-specs/main` when provenance is missing.

The complete input identity includes:

- Node version and `package-lock.json` hash;
- exact compiler, TypeSpec library, and emitter versions;
- emitter options and selected API version;
- specification repository, commit, directory, and additional directories;
- .NET SDK release tag and commit;
- temporary emitter project template and consuming project settings;
- Azure runtime package versions; and
- required shared or custom source with provenance.

A cache key must cover this complete identity. Two clean generations must have no unexplained differences. Reproducibility is separate from compatibility: compatibility means the full generated baseline supplies MCP's required API and request behavior, not that it recreates the released package's entire API.

## Cosmos baseline

The active `microsoft/mcp` repository uses `Azure.ResourceManager.CosmosDB` 1.5.0. Its release metadata identifies:

```yaml
azure-sdk-for-net tag: Azure.ResourceManager.CosmosDB_1.5.0
azure-sdk-for-net commit: 6f7e0bb7c1e2b849503aa6daf1ecc9d4f3adecc3
repo: Azure/azure-rest-api-specs
commit: 60d0f02991387ea7ed7483d5e70f061f962216cf
directory: specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB
api-version: 2026-03-15
emitter: @azure-typespec/http-client-csharp-mgmt 1.0.0-alpha.20260722.2
```

The projection is generated directly from this package's corresponding TypeSpec revision; no package-version migration is part of the product comparison.

## Sparse checkout

Use a partial clone and sparse checkout of only:

1. `tsp-location.yaml: directory`;
2. every `additionalDirectories` entry;
3. directories required by relative imports; and
4. repository files proven necessary for generation.

For Cosmos, only the TypeSpec and configuration files under the DocumentDB directory are needed; example JSON files are excluded. The checkout is detached at the pinned commit and stored under ignored generation work directories. Normal `dotnet build` and `dotnet publish` never access the specification repository.

Missing relative imports fail with an actionable path instead of broadening the checkout to the entire repository.

## Inputs and outputs

```text
eng/sdk-generation/
  README.md
  environments/
    cosmosdb/
      .nvmrc
      package.json
      package-lock.json
  services/cosmosdb.json
  shared-source-files.json
  scripts/
    New-ServiceProjection.ps1
    Resolve-PackageConsumers.ps1
    Resolve-PackageProvenance.ps1
    Resolve-MinimumHierarchyClosure.ps1
    New-ScopedSpec.ps1
    Export-SelectedHierarchy.ps1
    Test-ResourceHierarchy.ps1
    Update-ServiceProjection.ps1
  tests/
    Test-PackageConsumers.Tests.ps1
    Test-MinimumHierarchyClosure.Tests.ps1
    Test-ResourceHierarchy.Tests.ps1
    CosmosProjectionAotSmoke/

eng/generated/Azure.ResourceManager.CosmosDB/
  Azure.ResourceManager.CosmosDB.csproj
  spec.lock.json
  roots.json
  expanded-operations.json
  expected-hierarchy.json
  src/
    Generated/
    Shared/
```

Separation of ownership:

- `roots.json` is reviewed intent and evidence;
- `expanded-operations.json` and hierarchy metadata are derived;
- `src/Generated` is emitter-owned;
- `src/Shared` contains pinned shared Azure SDK source required outside `azure-sdk-for-net`; and
- temporary specification copies, code models, projects, and caches are not committed.

## Discovering required operations

### Full-source discovery mode

The existing projection may not contain an API needed by a new command. Therefore every update makes the full pinned SDK source available for semantic discovery, even when the package version is unchanged.

The orchestrator generates it or reuses an integrity-checked content-addressed cache entry. It then substitutes the full `src` directory for projected source through an MSBuild property or isolated discovery project. It must never compile full and projected source together or reference the released service package simultaneously. Success and failure must leave committed project files unchanged.

### Symbol inventory and mapping

A reusable Roslyn/MSBuild analyzer should record referenced service SDK:

- methods and extension methods;
- constructors, properties, and fields;
- resources, data types, models, and enums; and
- generic type arguments.

Map symbols using generated forwarding relationships, operation-ID/request-path documentation, and `tspCodeModel.json`. Ambiguous or missing mappings require agent review and fail automation.

For the POC, reviewed static roots are acceptable. Current direct roots are:

- `Microsoft.DocumentDB.DatabaseAccounts.get`;
- `Microsoft.DocumentDB.DatabaseAccounts.list`;
- `Microsoft.DocumentDB.DatabaseAccounts.listByResourceGroup`;
- `Microsoft.DocumentDB.DatabaseAccounts.listKeys`; and
- `Microsoft.DocumentDB.LocationGetResults.list` for Quota region discovery.

A model-only dependency must be reachable from a retained resource or operation. Otherwise generation fails; it must not silently retain unrelated operations. A future explicit `modelRoots` contract requires separate design and tests.

## Resource-aware closure

The emitter's ARM provider schema is authoritative. For every root:

1. locate it in `resources[].methods` or `nonResourceMethods`;
2. select its owning resource;
3. retain every operation on that resource;
4. select in-service ancestors from `parentResourceId` or the resource ID hierarchy;
5. retain every operation on those ancestors;
6. require a `Read` for each selected resource;
7. preserve resource type, ID pattern, scope, parent, singleton status, and C# name; and
8. retain selected non-resource methods individually with their target scope.

Predefined parents supplied by `Azure.ResourceManager`, such as subscription and resource group, are external hierarchy anchors rather than service operations.

The Cosmos closure currently contains:

- `Microsoft.DocumentDB/databaseAccounts`: 32 operations;
- `Microsoft.DocumentDB/locations`: 2 operations.

## Applying scopes

The pinned specification remains unchanged. The tool copies it to a temporary directory and appends the complement of the retained operation set to `client.tsp`:

```typespec
// BEGIN AZURE MCP GENERATED OPERATION SCOPES
@@scope(Microsoft.DocumentDB.SomeGroup.unneededOperation, "!csharp");
// END AZURE MCP GENERATED OPERATION SCOPES
```

Existing upstream scopes are respected. A required root already excluded from C# is an error. Users and agents edit `roots.json`, never the generated scope block.

The first go/no-go gate confirms that scoping unselected resource reads removes those resources without blocking diagnostics. If operation-level scopes cannot do this for a service, stop and define the required emitter or TypeSpec support.

## Strict hierarchy validation

The `azure-sdk-for-net` hierarchy scripts are useful inputs but are not strict enough alone: they may infer names heuristically, omit resource ID comparison, permit extra scopes/resources, and miss casing-only name changes.

The MCP-owned validator compares the selected full baseline with projected output and requires:

- complete generated-name mappings from both outputs;
- exact bidirectional resource sets;
- exact resource ID patterns;
- exact parent and scope sets;
- identical singleton status; and
- ordinal, case-sensitive C# names.

It also verifies the exact operation set and operation path/kind. Independent negative fixtures cover resource ID drift, scope drift, casing-only rename, extra/missing resources, parent drift, singleton drift, and missing generated-name mappings.

## Source integration and multiple consumers

The emitter expects a project-shaped output and creates scaffolding if none exists, even with `new-project=false`. Generation therefore uses a temporary minimal project, but that project is not committed or shipped. After validation, only generated/shared source is copied into MCP.

When one tool is the sole package consumer, it can compile the source directly. Before integration, however, inventory every package reference. In `microsoft/mcp`, both Cosmos and Quota use `Azure.ResourceManager.CosmosDB`:

- Cosmos uses account get/list/key operations;
- Quota uses location listing for region discovery.

Changing Cosmos alone cannot remove the package from the server. The POC therefore compiles the combined projection once in an MCP-owned shared lightweight assembly referenced by both tools. Compiling identical public types into multiple tool assemblies is avoided because it duplicates code and creates conflicting type identities. The released package is removed from both consumers.

## Native AOT validation

The existing server's native build may exclude or trim a service, so it is not sufficient proof by itself. A dedicated smoke executable references the shared generated project and only its management runtime dependencies. Mocked transport roots and executes:

- account get/list and paging;
- account and key deserialization;
- `Data.Name` and `PrimaryMasterKey`; and
- location listing before final multi-consumer integration.

Record publish command, RID coverage, diagnostics, proof that projected code participated in native compilation, and successful execution.

## Cache behavior

- Empty cache or different valid identity: generate current identity.
- Exact identity with valid integrity metadata: reuse.
- Corrupt matching entry: discard and regenerate when safe, otherwise fail.
- Attempted mismatched reuse: fail.
- Incomplete authoritative inputs: fail before lookup or generation.

Tests cover empty/hit/stale/corrupt states, package and tooling changes, duplicate implementation prevention, and cleanup after failure.

## Cosmos POC phases

1. **Baseline:** pin complete inputs, sparse-checkout the spec, inspect required shared/custom source, generate twice, and build/test the full baseline.
2. **Scope gate:** use reviewed roots, generate the closure and scopes, verify diagnostics, exact operations, and hierarchy.
3. **Discovery:** implement semantic full-source substitution; prove newly referenced excluded APIs expand the projection; prove unsupported model-only use fails.
4. **Integration:** inventory Cosmos and Quota, share one account-and-location projection, remove the package from both, run unit/live/request tests, and execute Native AOT validation.
5. **Measurement:** compare current `microsoft/mcp/main` with the final projection using identical production settings.

## Product measurement and acceptance

Only shipped states are size baselines:

1. current `microsoft/mcp/main` with `Azure.ResourceManager.CosmosDB` 1.5.0; and
2. the final shared account-and-location projection with the released package absent.

The full generated SDK is a temporary validation input, not a measurement baseline.

Measure for each supported RID:

- affected service footprint;
- self-contained publish directory;
- compressed distribution; and
- native/AOT artifacts where Cosmos is included.

Acceptance requires:

- at least 50% reduction in both raw and individually compressed affected service footprint;
- positive complete compressed distribution reduction with no RID regression;
- preserved behavior, hierarchy, and AOT compatibility; and
- acceptable generation and maintenance cost.

There is no fixed absolute per-service threshold. Percentage reduction measures projection effectiveness; aggregate absolute savings determine product value as additional SDKs are migrated. Release PDBs must not distort the comparison.

## Failure conditions

Generation fails for unresolved provenance, incomplete sparse dependencies or input identity, unpinned tooling, missing/ambiguous roots, unsupported model-only dependencies, selected resources without `Read`, operation/hierarchy drift, incomplete generated names, duplicate full/projected implementations, stale temporary project state, unexpected package retention after final integration, or non-reproducible output.

Package/spec updates never silently accept renamed or removed roots; they produce a reviewable manifest diff.

## POC deliverables

- package-to-spec resolver and sparse checkout;
- pinned generation environment and reproducible full generation;
- reviewed roots and generated resource closure;
- temporary generated C# scopes;
- projected generated/shared source;
- strict hierarchy validator and negative tests;
- semantic discovery and cache-state tests;
- mocked request and Native AOT tests;
- single-copy Cosmos/Quota integration; and
- current-main-versus-projection size report.
