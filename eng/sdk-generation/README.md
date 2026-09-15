# Azure MCP management SDK projection

This directory contains the POC tooling for generating lightweight Cosmos DB management source. The generated C# is committed under `tools/Azure.Mcp.Tools.Cosmos/src/GeneratedSdk` and compiled directly into `Azure.Mcp.Tools.Cosmos`, so normal builds do not access the specification repository or build a separate SDK project.

## Pinned baseline

The Cosmos projection is based on `Azure.ResourceManager.CosmosDB` 1.5.0:

- `azure-sdk-for-net` tag: `Azure.ResourceManager.CosmosDB_1.5.0`
- SDK commit: `6f7e0bb7c1e2b849503aa6daf1ecc9d4f3adecc3`
- specification commit: `60d0f02991387ea7ed7483d5e70f061f962216cf`
- specification directory: `specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB`
- API version: `2026-03-15`

The complete machine-readable provenance is in `tools/Azure.Mcp.Tools.Cosmos/src/GeneratedSdk/spec.lock.json`.

## Regeneration

Install Node.js 24.15.0, then run:

```pwsh
./eng/sdk-generation/scripts/Update-ServiceProjection.ps1 -Service cosmosdb
```

The script:

1. verifies the pinned Node and npm lock inputs;
2. sparsely fetches the exact specification and SDK source commits;
3. generates and builds the complete SDK baseline;
4. expands the direct roots to every operation on the selected resource;
5. adds temporary `@@scope(..., "!csharp")` decorators for other operations;
6. generates and builds the projection;
7. validates its exact operation set and resource hierarchy; and
8. replaces the committed `src/Generated` directory and derived expanded-operation manifest.

Temporary repositories and generation outputs are stored under `eng/sdk-generation/.work`, which is ignored by the repository's `.work/` rule.

## Selection policy

`roots.json` contains operations directly required by MCP. The POC keeps every operation associated with a resource that owns a root operation. For Cosmos DB, account get/list/key operations select `Microsoft.DocumentDB/databaseAccounts`, retaining all 32 operations associated with that resource. Quota's region discovery selects `Microsoft.DocumentDB/locations`, retaining both operations associated with that resource. The complete projection therefore contains 2 resources and 34 operations at the pinned specification revision.

The projected source intentionally omits upstream service customizations that MCP does not use. It includes pinned Azure SDK shared source required to compile generated clients outside `azure-sdk-for-net`.

`Azure.ResourceManager.CosmosDB` is also consumed by the Quota tool. The package cannot be removed from the complete server until Cosmos and Quota share the projected implementation. The current POC compiles source into the Cosmos tool to validate generation and behavior; the cross-tool sharing model remains an integration decision.

## Full-SDK discovery

`Azure.Mcp.Tools.Cosmos.csproj` accepts `CosmosManagementSdkSource`. Setting it to a complete generated SDK `src` directory substitutes those source files for the committed projection. Set `UseCosmosManagementSdkPackage=true` to exclude generated source and use the released package for controlled comparisons.

The emitter still receives a temporary project-shaped output directory because it creates project scaffolding when no project exists, even with `new-project=false`. The temporary project prevents emitter-owned scaffolding from entering the service area. Only `src/Generated` is copied into MCP; no generated SDK project is committed or shipped.
