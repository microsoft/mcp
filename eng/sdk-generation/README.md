# Azure MCP management SDK projection

This directory contains the POC tooling for generating a lightweight Cosmos DB management SDK. The generated source is committed under `eng/generated/Azure.ResourceManager.CosmosDB` and compiled once into an MCP-owned shared assembly referenced by the Cosmos and Quota tools. Normal builds do not access the specification repository.

## Pinned baseline

The projection uses the version already pinned in `Directory.Packages.props`; the tooling never upgrades it. For Cosmos DB 1.5.0:

- `azure-sdk-for-net` tag: `Azure.ResourceManager.CosmosDB_1.5.0`
- SDK commit: `6f7e0bb7c1e2b849503aa6daf1ecc9d4f3adecc3`
- specification commit: `60d0f02991387ea7ed7483d5e70f061f962216cf`
- specification directory: `specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB`
- API version: `2026-03-15`

Complete provenance is recorded in `eng/generated/Azure.ResourceManager.CosmosDB/spec.lock.json`.

## Onboard another service

Initialize a projection without changing its centrally pinned package version:

```pwsh
./eng/sdk-generation/scripts/New-ServiceProjection.ps1 `
  -Service example `
  -PackageId Azure.ResourceManager.Example `
  -SdkPath sdk/example/Azure.ResourceManager.Example `
  -ConsumerProject tools/Azure.Mcp.Tools.Example/src/Azure.Mcp.Tools.Example.csproj
```

The command creates the service configuration, package-specific TypeSpec environment, generated project skeleton, shared source, and provenance lock. The agent then reviews `roots.json`, adds the shared project reference to each consumer, removes their released package references, and runs regeneration.

## Regeneration

Each service has a tooling environment under `eng/sdk-generation/environments/<service>` so its compiler and emitter versions remain aligned with the pinned SDK release. For Cosmos, install Node.js 24.15.0, then run:

```pwsh
./eng/sdk-generation/scripts/Update-ServiceProjection.ps1 -Service cosmosdb
```

The orchestrator:

1. resolves direct package/generated-project consumers;
2. verifies the package version and its TypeSpec provenance;
3. sparsely fetches exact specification and SDK source commits;
4. generates and builds the complete temporary SDK baseline;
5. resolves `minimumHierarchyClosure` from reviewed roots;
6. creates a temporary spec with `@@scope(..., "!csharp")` exclusions;
7. generates and builds the projection;
8. validates exact operations and hierarchy; and
9. updates committed generated source and derived manifests.

Temporary repositories, project scaffolding, and code models are stored under `eng/sdk-generation/.work`, which is ignored.

## Selection policy

`roots.json` records operations directly used by MCP. `minimumHierarchyClosure` retains:

- each direct root;
- the `Read` operation required to materialize each selected resource; and
- recursively, the `Read` for every in-service ancestor.

It retains no unrelated CRUD, list, or action operations. The Cosmos projection currently contains:

- `Microsoft.DocumentDB/databaseAccounts`: four directly used operations;
- `Microsoft.DocumentDB/locations`: the directly used list plus its required read.

The result is 2 resources and 6 operations.

## Shared integration

Both `Azure.Mcp.Tools.Cosmos` and `Azure.Mcp.Tools.Quota` previously referenced `Azure.ResourceManager.CosmosDB`. They now reference one shared generated project:

```text
eng/generated/Azure.ResourceManager.CosmosDB/
  Azure.ResourceManager.CosmosDB.csproj
  src/Generated/
  src/Shared/
```

This is MCP-owned integration scaffolding, not emitter-generated project scaffolding. The emitter writes to a temporary project-shaped directory; only validated generated source is copied to the shared project.

The shared project disables Release PDB output. The server dependency graph must not contain the released `Azure.ResourceManager.CosmosDB` NuGet package after migration.

## Agent review boundary

The deterministic scripts handle initialization, provenance, consumers, sparse checkout, full generation/cache reuse, minimum hierarchy closure, scope generation, projected generation, strict validation, consumer builds, and package-absence validation. Reviewed `roots.json` remains the input for semantic intent. A future Roslyn analyzer will propose root changes from SDK symbol usage and stop for ambiguous mappings rather than guessing.
