# Azure MCP management SDK projection

This directory contains the POC tooling for generating a lightweight Cosmos DB management SDK. The generated SDK is committed under `sdk/generated/Azure.ResourceManager.CosmosDB` so normal builds do not access the specification repository.

## Pinned baseline

The Cosmos projection is based on `Azure.ResourceManager.CosmosDB` 1.5.0:

- `azure-sdk-for-net` tag: `Azure.ResourceManager.CosmosDB_1.5.0`
- SDK commit: `6f7e0bb7c1e2b849503aa6daf1ecc9d4f3adecc3`
- specification commit: `60d0f02991387ea7ed7483d5e70f061f962216cf`
- specification directory: `specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB`
- API version: `2026-03-15`

The complete machine-readable provenance is in `sdk/generated/Azure.ResourceManager.CosmosDB/spec.lock.json`.

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

`roots.json` contains operations directly required by MCP. The POC keeps every operation associated with a resource that owns a root operation. For Cosmos DB, account listing and key retrieval select `Microsoft.DocumentDB/databaseAccounts`, retaining all 32 operations associated with that resource at the pinned specification revision.

The projected SDK intentionally omits upstream service customizations that MCP does not use. It includes pinned Azure SDK shared source required to compile generated clients outside `azure-sdk-for-net`.

## Full-SDK discovery

`AzureMcp.Cosmos.csproj` accepts `CosmosManagementSdkProject`. Setting it to a complete generated SDK project substitutes that project for the committed projection without loading both assemblies. Set `UseCosmosManagementSdkPackage=true` to use the released package for controlled comparisons.
