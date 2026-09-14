<!-- Copyright (c) Microsoft Corporation. -->
<!-- Licensed under the MIT License. -->
<!-- cspell:ignore autorest typespec projectable -->

# Lightweight generated management SDK proof of concept

## Status

Proposed.

The first proof of concept (POC) targets `Azure.ResourceManager.CosmosDB`. No production package reference should be replaced until the POC validates source provenance, operation selection, resource hierarchy, generated API usability, and distribution-size improvement.

## Summary

Azure MCP currently consumes released Azure SDK NuGet packages and wraps a small part of their APIs as MCP commands. Management packages can contain many resources, operations, and models that Azure MCP never uses.

This proposal generates a service-specific, lightweight .NET management SDK from the same Azure REST API specification revision associated with the released .NET SDK package. The generation process excludes operations that Azure MCP does not need while preserving complete selected resources and their resource hierarchy.

The workflow must preserve the current AI-assisted contribution experience. A command developer should continue to describe the command and required Azure SDK package in a prompt. The coding agent, guided by the repository instructions, is responsible for:

- selecting and adding the package version;
- using the repository's existing command, service, options, test, and registration patterns;
- identifying the SDK members used by the implementation;
- resolving those members to REST/TypeSpec operations;
- updating the lightweight SDK input manifest;
- regenerating and validating the projected SDK; and
- adding any required SDK access helpers and Azure CLI validation commands.

Developers should not need to understand TypeSpec operation IDs, edit `client.tsp`, or maintain resource-hierarchy metadata manually.

## Goals

1. Reduce the Azure MCP published artifact size by replacing large service-specific management packages with generated projections.
2. Generate from the specification commit corresponding to the selected .NET SDK package, rather than from a moving `azure-rest-api-specs/main` branch.
3. Fetch specifications with Git sparse checkout.
4. Retain all operations belonging to a selected ARM resource and all in-service ancestor resources required for its hierarchy.
5. Make operation selection deterministic, reviewable, and reproducible.
6. Preserve the public SDK shapes used by Azure MCP, or make required migration changes explicit.
7. Integrate projection maintenance into the existing AI prompt-driven command-authoring pattern.
8. Keep normal builds offline from the specification and generation repositories by committing generated C#.

## Non-goals for the initial POC

- Replacing `Azure.Core`, `Azure.Identity`, or the base `Azure.ResourceManager` package.
- Replacing Cosmos DB data-plane packages such as `Microsoft.Azure.Cosmos`.
- Supporting every Azure MCP management package in the first change.
- Producing or publishing a general-purpose Azure SDK NuGet package.
- Preserving the entire public API of the released service package. The compatibility requirement is the API used by Azure MCP.
- Generating from an unpinned latest specification or emitter.
- Adding a second Swagger-based projection system in the Cosmos DB POC.

## Existing Azure MCP authoring pattern

The contribution workflow asks a developer to use an agent prompt similar to:

```text
create [namespace] [resource] [operation] command using #new-command.md as a reference
```

The detailed pattern lives in `docs/new-command.md`, and repository-wide coding guidance lives in `.github/copilot-instructions.md`. Today the agent adds the package to `Directory.Packages.props` and the area project, then implements commands, service methods, options, tests, documentation, and registrations.

The projection workflow must extend this pattern instead of introducing a manual prerequisite. `docs/new-command.md` and `.github/copilot-instructions.md` will eventually tell the agent that adding or using a service-specific management package also requires running the SDK projection update command. The developer-facing prompt remains focused on the desired command and package.

The POC should prove the tooling before those instructions are changed globally.

## Source provenance

### Required provenance chain

Every generated service must have a machine-readable chain from the Azure MCP dependency to the specification:

```text
Azure MCP package ID and version
  -> azure-sdk-for-net release tag and commit
  -> azure-rest-api-specs repository, commit, and paths
  -> pinned TypeSpec emitter and compiler packages
  -> generated lightweight SDK
```

For a package such as `Azure.ResourceManager.CosmosDB` version `1.5.0`, the corresponding `azure-sdk-for-net` release tag is normally:

```text
Azure.ResourceManager.CosmosDB_1.5.0
```

At that tag, provenance is resolved from:

- `tsp-location.yaml` for a TypeSpec-generated package; or
- `src/autorest.md` for a Swagger-generated package.

For TypeSpec, `tsp-location.yaml` supplies:

- `repo`;
- `commit`;
- `directory`;
- `additionalDirectories`; and
- the emitter package manifest used by the .NET repository.

The generation command must fail if it cannot establish this chain. It must never silently fall back to `azure-rest-api-specs/main`.

### Complete generation contract

Specification provenance is necessary but not sufficient for reproducible generation. The committed generation inputs must collectively determine the output and include:

- the Node.js version, pinned by a committed version file and enforced by `package.json`;
- `package-lock.json`, installed with `npm ci`;
- exact TypeSpec compiler, library, and management emitter versions;
- the emitter configuration and every command-line option;
- the selected service API version;
- the `new-project`, output-directory, and saved-input settings;
- the source specification repository, commit, primary directory, and additional directories;
- the corresponding `azure-sdk-for-net` release tag and commit;
- the temporary emitter project properties, consuming MCP project properties, and Azure runtime package versions;
- any shared source required by the emitter, with its repository and commit provenance; and
- any service-specific custom source required by MCP, with its provenance, or a documented determination that MCP does not use it.

The authoritative values may be distributed among `spec.lock.json`, the service configuration, the Node version file, `package-lock.json`, the temporary emitter project template, the consuming MCP project, and generation scripts. `spec.lock.json` must identify or hash those committed inputs. The resulting complete identity is the content-addressed cache key, so an artifact can be reused only for the exact inputs that produced it.

Reproducibility and MCP compatibility are separate requirements:

- **Reproducibility** means two clean generations from the complete input identity have no unexplained output differences.
- **MCP compatibility** means the full generated baseline supplies the SDK APIs used by MCP and sends equivalent requests for the focused scenarios. It does not require reproducing the released package's entire public API.

Before projection, compare the generated full SDK with the selected package and its `azure-sdk-for-net` source. Identify handwritten customizations and shared source. Include the parts required by MCP or document, with usage evidence, why omission is safe. The full baseline must pass focused compile and request-behavior checks before scoped output is evaluated.

### Cosmos DB bootstrap issue

Azure MCP currently references:

```text
Azure.ResourceManager.CosmosDB 1.4.0-beta.13
```

Its release snapshot points to Azure REST API specifications commit:

```text
2afa5b356adf6cf51209d2cf28d38644c69d9832
```

and to the Swagger entry point:

```text
specification/cosmos-db/resource-manager/readme.md
```

The `Microsoft.DocumentDB/DocumentDB/client.tsp` TypeSpec source does not exist at that commit. Therefore the current package cannot be projected with `@@scope` from its exact source revision.

`Azure.ResourceManager.CosmosDB` version `1.5.0` is TypeSpec-generated and its release snapshot contains:

```yaml
directory: specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB
commit: 60d0f02991387ea7ed7483d5e70f061f962216cf
repo: Azure/azure-rest-api-specs
additionalDirectories:
```

The Cosmos DB POC should consequently use two explicit stages:

1. validate Azure MCP against the full TypeSpec-generated `1.5.0` API; and
2. project the lightweight SDK from that package's pinned specification commit.

This keeps an SDK/spec migration separate from operation removal. If the move to `1.5.0` is not acceptable, the POC must stop rather than using an unrelated TypeSpec revision.

## Sparse specification checkout

Generation must not clone a full working tree of `azure-rest-api-specs`. The fetch command should maintain a disposable or cached partial clone outside the generated source directory:

```bash
git clone \
  --filter=blob:none \
  --no-checkout \
  https://github.com/Azure/azure-rest-api-specs.git \
  .generation-cache/azure-rest-api-specs

git -C .generation-cache/azure-rest-api-specs sparse-checkout init --cone
git -C .generation-cache/azure-rest-api-specs sparse-checkout set \
  specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB
git -C .generation-cache/azure-rest-api-specs checkout --detach \
  60d0f02991387ea7ed7483d5e70f061f962216cf
```

The production script should set the sparse paths before or as part of checking out the detached commit. The paths must be taken from the resolved package provenance, not hard-coded to Cosmos DB.

The sparse set includes:

1. the `directory` from `tsp-location.yaml`;
2. every `additionalDirectories` entry;
3. local directories required by relative imports; and
4. repository files proven necessary for generation, if any.

The resolver should validate relative imports and fail with the missing path when the sparse set is incomplete. It should update the sparse set only from explicit provenance or dependency information, not broaden it to the entire repository.

Generated C# and lock manifests are committed. The sparse checkout and unmodified specification snapshot are build artifacts and are not committed. Normal `dotnet build` and `dotnet publish` do not fetch specifications.

## Inputs and generated artifacts

A proposed layout is:

```text
eng/sdk-generation/
  package.json
  package-lock.json
  README.md
  services/
    cosmosdb.json
  scripts/
    Resolve-SdkProvenance.ps1
    Checkout-Spec.ps1
    Generate-FullSdk.ps1
    Resolve-OperationClosure.ps1
    Apply-OperationScopes.ps1
    Generate-ProjectedSdk.ps1
    Validate-ProjectedSdk.ps1
    Update-ServiceProjection.ps1

areas/cosmos/src/Azure.Mcp.Tools.Cosmos/
  Azure.Mcp.Tools.Cosmos.csproj
  GeneratedSdk/
    spec.lock.json
    roots.json
    expanded-operations.json
    expected-hierarchy.json
    src/
      Generated/
      Shared/
```

Names and location can change during implementation, but the separation between human intent, resolved inputs, and generated outputs must remain.

### Service configuration

`eng/sdk-generation/services/cosmosdb.json` describes stable service identity and policy, for example:

```json
{
  "packageId": "Azure.ResourceManager.CosmosDB",
  "packageVersion": "1.5.0",
  "areaProjects": [
    "tools/Azure.Mcp.Tools.Cosmos/src/Azure.Mcp.Tools.Cosmos.csproj"
  ],
  "selectionPolicy": "allOperationsForSelectedResource"
}
```

The package version should normally match `Directory.Packages.props` while migrating. After generated source replaces the package in normal builds, the service configuration remains the source of the baseline package identity.

### Specification lock

`spec.lock.json` is generated from package provenance:

```json
{
  "package": {
    "id": "Azure.ResourceManager.CosmosDB",
    "version": "1.5.0"
  },
  "azureSdkForNet": {
    "tag": "Azure.ResourceManager.CosmosDB_1.5.0",
    "commit": "<resolved release commit>"
  },
  "azureRestApiSpecs": {
    "repository": "Azure/azure-rest-api-specs",
    "commit": "60d0f02991387ea7ed7483d5e70f061f962216cf",
    "directory": "specification/cosmos-db/resource-manager/Microsoft.DocumentDB/DocumentDB",
    "additionalDirectories": []
  },
  "tooling": {
    "node": "<exact version>",
    "packageLockSha256": "<hash>",
    "emitter": "<exact package version>",
    "compiler": "<exact package version>",
    "apiVersion": "<selected API version>",
    "emitterOptionsSha256": "<hash>"
  },
  "source": {
    "sharedSource": [],
    "serviceCustomizations": []
  },
  "destination": {
    "projectSha256": "<hash>"
  }
}
```

### Root operation manifest

`roots.json` records operations directly needed by Azure MCP and evidence for each mapping. For Cosmos DB the initial roots are expected to be:

```json
{
  "operations": [
    {
      "methodId": "Microsoft.DocumentDB.DatabaseAccounts.list",
      "sdkMembers": [
        "SubscriptionResource.GetCosmosDBAccountsAsync"
      ],
      "usedBy": [
        "areas/cosmos/src/Azure.Mcp.Tools.Cosmos/Services/CosmosService.cs"
      ]
    },
    {
      "methodId": "Microsoft.DocumentDB.DatabaseAccounts.listKeys",
      "sdkMembers": [
        "CosmosDBAccountResource.GetKeysAsync"
      ],
      "usedBy": [
        "areas/cosmos/src/Azure.Mcp.Tools.Cosmos/Services/CosmosService.cs"
      ]
    }
  ]
}
```

This file expresses direct MCP requirements. It does not contain manually expanded resource operations.

### Expanded operation manifest

`expanded-operations.json` is derived from the unscoped `tspCodeModel.json`. It records:

- direct roots;
- owning resources;
- resource ancestors;
- all operations retained by policy;
- why each operation was retained; and
- non-resource methods, if selected.

The generated diff must make newly added or removed service operations visible during a package/spec update.

## SDK member and operation discovery

### Full generation and discovery mode

Operation selection cannot be based only on names in the currently released assembly or existing projection. The full SDK from the pinned specification is the semantic discovery reference for every projection update, including an update that changes only MCP command usage.

The orchestration command generates the full SDK or reuses a content-addressed cache entry only when its key and integrity metadata match the complete generation-input identity. An empty cache, or the presence of valid entries for older or different identities, is an ordinary cache miss: generate and store the current identity without requiring manual cache deletion. A cache may retain multiple valid identities.

A corrupt or incomplete matching entry must never be compiled. The implementation may discard and regenerate it when the integrity policy can do so safely; otherwise it fails with an actionable integrity diagnostic. Any attempt to use an artifact under a mismatched identity is an error. Missing or incomplete authoritative generation inputs fail before cache lookup or generation.

After obtaining the full SDK source, the command builds the configured MCP projects in a discovery mode that substitutes the full generated `src` directory for the committed projected source. The substitution must be controlled by an MSBuild property or isolated discovery project; it must not rewrite committed project files.

Discovery mode must ensure that full and projected source are never compiled together and that the released service package is not simultaneously referenced. On success or failure, it leaves no temporary compile items, package references, or project changes. A normal build after discovery must compile only the committed projection. This supports adding a command for an operation or resource absent from the current projection without first weakening that projection.

Generating the full SDK also separates API migration failures from projection failures. If the full baseline cannot compile the affected MCP projects, the workflow reports an SDK migration issue before operation scopes are applied.

### Semantic SDK usage inventory

The reusable implementation should use a Roslyn/MSBuild analyzer against discovery mode to record service SDK symbols referenced by configured Azure MCP projects, including:

- methods and extension methods;
- constructors;
- properties and fields;
- resource, data, model, and enum types; and
- generic type arguments.

For the initial scope feasibility experiment, a reviewed static inventory is sufficient. Before the workflow is generalized, prove the semantic path by starting from an existing projection, adding a fixture that uses an excluded resource without changing the package baseline, and regenerating a projection that contains it.

### Mapping SDK members to operations

Generated management methods include REST operation IDs and request paths in their documentation and forward through generated resource, collection, extension, and `Mockable*` types. The mapper should combine:

- generated C# method metadata;
- forwarding relationships;
- generated operation ID and request path documentation; and
- `tspCodeModel.json` method IDs.

An unresolved or ambiguous mapping is an error requiring agent review. The agent writes the reviewed result to `roots.json`; a developer is not expected to determine a TypeSpec operation ID from the prompt.

### Model-only dependencies

For the Cosmos DB POC, a model, enum, constructor, or property referenced by MCP must be reachable from a retained resource or operation. If it is not reachable, generation fails with an actionable diagnostic identifying the symbol and source location. The tool must not retain an unrelated operation merely to make the model reachable.

The agent must then either revise the MCP implementation to use a reachable contract or stop and request an explicit model-root capability. A future design may add a separate, reviewed `modelRoots` manifest and C# usage customizations, but only after an isolated fixture proves deterministic model retention. Model roots, if introduced, remain distinct from operation roots and must not silently expand the operation set.

The Cosmos account-listing and key-retrieval scenarios remain the behavioral anchors while a separate fixture exercises this failure policy.

## Resource-aware operation closure

The management emitter's ARM provider schema is the source of truth. For each root operation:

1. Find the operation in `resources[].methods` or `nonResourceMethods`.
2. If it belongs to an ARM resource, select the owning resource.
3. Retain every operation associated with that resource.
4. Walk `parentResourceId` or the resource ID hierarchy and select every ancestor implemented by the same service SDK.
5. Retain every operation associated with each selected in-service ancestor.
6. Verify that every selected resource retains its `Read` operation.
7. Preserve resource type, ID pattern, scope, parent, singleton status, and generated C# name.
8. Treat predefined parents supplied by `Azure.ResourceManager`, such as subscription and resource group, as external hierarchy anchors rather than service operations.
9. Retain selected non-resource methods individually and validate their target scope.

The all-operations policy is intentionally conservative. A hierarchy technically needs fewer operations, but the POC follows the requirement that selecting a resource retains its complete operation set. The size report should show the cost of this policy.

For the initial Cosmos roots, both operations belong to:

```text
Microsoft.DocumentDB/databaseAccounts
```

The current full TypeSpec model associates 32 operations with that resource. The exact number must be recalculated from the pinned `1.5.0` specification rather than copied from a newer specification checkout.

## Applying C# scopes

The fetched specification remains unchanged. The tool creates a temporary working copy and generates an MCP-owned block in `client.tsp` containing the complement of the expanded allowlist:

```typespec
// BEGIN AZURE MCP GENERATED OPERATION SCOPES
@@scope(Microsoft.DocumentDB.SomeGroup.unneededOperation, "!csharp");
// END AZURE MCP GENERATED OPERATION SCOPES
```

The algorithm is:

```text
all full-code-model operations
  - expanded retained operations
  = operations scoped out for C#
```

Existing upstream `@@scope` decorators must be respected. If an MCP root is already excluded from C#, generation fails.

The generated scope block is an intermediate artifact. Human-reviewed intent remains in `roots.json`; users and agents should not hand-edit the block.

## Resource hierarchy validation

The sibling `azure-sdk-for-net` repository provides:

```text
eng/packages/http-client-csharp-mgmt/eng/scripts/Get-ResourceHierarchy.ps1
eng/packages/http-client-csharp-mgmt/eng/scripts/Get-ResourceHierarchyFromTspCodeModel.ps1
eng/packages/http-client-csharp-mgmt/eng/scripts/Compare-ResourceHierarchy.ps1
```

These upstream helpers are useful inputs but their exit codes do not implement this proposal's strict contract. At the Cosmos `1.5.0` release, the extractor can fall back to heuristic C# names, while the comparator does not compare resource ID patterns, permits additional resources and scopes, and uses case-insensitive PowerShell name comparison. Any reused helper source must be pinned as part of the tooling inputs.

An MCP-owned strict wrapper or replacement validator must require generated-name extraction from both full and projected output and reject missing, duplicate, or incomplete mappings instead of accepting heuristic fallback. It compares resource sets bidirectionally, resource ID patterns exactly, scope and parent sets exactly, singleton status, and generated C# names with ordinal case-sensitive comparison.

The full released package hierarchy cannot be compared directly with the projection because most resources are intentionally removed. Validation instead uses this sequence:

1. Generate the full SDK from the pinned specification.
2. Extract its resource hierarchy from `tspCodeModel.json`.
3. Filter it to the selected resource and ancestor closure.
4. Save that result as `expected-hierarchy.json`.
5. Generate the scoped SDK.
6. Extract the projected hierarchy.
7. Compare expected and projected hierarchies.

For every retained resource, compare:

- ARM resource type and the exact bidirectional resource set;
- resource ID pattern using exact comparison;
- exact parent resource type set;
- exact scope set;
- singleton status; and
- generated C# resource name using ordinal case-sensitive comparison.

Also assert that:

- generated-name mappings are available, unique, and complete for both inputs;
- every root operation remains;
- the projected operation set equals `expanded-operations.json`;
- no unexpected resource remains;
- no expected resource disappears; and
- each retained method has the expected operation path and kind.

Keep independent strict-validator regression tests for resource ID drift, an additional scope, a casing-only C# rename, an additional resource, a missing resource, parent drift, singleton drift, and an unavailable generated-name mapping. Each negative fixture must fail for its intended reason, while an unchanged full-baseline subset and projection must pass.

This comparison is first exercised as an early go/no-go gate immediately after the reproducible full baseline exists. Using a reviewed static allowlist, retain the complete database-account resource and scope out unrelated resource operations. Record the exact inputs, command, diagnostics, operation set, and hierarchy. Confirm unrelated resources disappear without blocking missing-`Read` diagnostics. If the experiment fails, operation-level `@@scope` is insufficient: record the diagnostic and required emitter or TypeSpec support, then stop before implementing reusable discovery or orchestration tooling.

## Generated source integration

The projection does not need a production SDK project. The management emitter treats its output directory as a project root and creates project scaffolding when no project exists, even with `new-project=false`. Generation therefore uses a temporary project-shaped directory with a minimal pre-existing `.csproj` solely to preserve emitter behavior. The temporary project, solution, and metadata are not committed or shipped.

After generation and validation, copy only emitter-owned and required shared source into the Cosmos area:

```text
tools/Azure.Mcp.Tools.Cosmos/src/GeneratedSdk/
  src/
    Generated/
    Shared/
```

`Azure.Mcp.Tools.Cosmos.csproj` compiles these files directly into `Azure.Mcp.Tools.Cosmos.dll`. The generated types retain their `Azure.ResourceManager.CosmosDB` namespaces; their assembly identity is not a compatibility requirement because they are an internal implementation dependency of the Cosmos area.

The consuming project directly references required runtime packages, initially `Azure.Core` and `Azure.ResourceManager`, and continues to reference `Microsoft.Azure.Cosmos` for data-plane operations. Normal builds do not reference the released `Azure.ResourceManager.CosmosDB` package. A controlled comparison property may exclude generated source and restore the released package, but both must never be compiled together.

For Release builds, the Cosmos area disables PDB output so generated symbols are not added to the shipped distribution. A separate diagnostic-symbol artifact may be considered later without changing production measurements.

### Dedicated Native AOT validation

The existing native CLI build does not validate this projection. When `BuildNative=true`, `AzureMcp.Cli.csproj` removes the Cosmos area and `Microsoft.Azure.Cosmos`, so the projected management assembly would not participate in that publish.

The POC must add a dedicated Native AOT smoke-test executable that compiles the projected management source through linked `Compile` items and references only its required management runtime packages, not the Cosmos data-plane package. Through mocked transport, the executable must root and execute subscription account listing, paged response handling, account key retrieval, response deserialization, and access to `Data.Name` and `PrimaryMasterKey`. Merely including unused source is insufficient because trimming may remove it.

Validation records the Native AOT publish command, RID coverage, diagnostics, proof that the projected SDK is part of the native compilation, and successful execution of the published application. The POC should validate the host RID first; production integration defines the supported CI RID matrix before rollout to other services.

Native CLI artifact measurements must state that Cosmos is excluded. They cannot be presented as AOT validation or size measurement of the projection. AOT projection measurements come from the dedicated smoke-test application, while non-native production distribution measurements use the normal shipped configuration that includes Cosmos.

## AI-assisted update workflow

### Developer experience

The intended prompt remains concise, for example:

```text
Create the Cosmos DB account key command using #new-command.md. Use Azure.ResourceManager.CosmosDB.
```

The developer does not provide:

- a specification SHA;
- a TypeSpec path;
- operation IDs;
- scope decorators;
- a resource hierarchy; or
- generated-source project changes.

### Agent responsibilities

After implementing the command using the existing repository patterns, the agent must:

1. determine whether the requested package is already projected;
2. select or confirm the package version;
3. resolve the `azure-sdk-for-net` release tag and specification provenance;
4. sparse-checkout the pinned specification;
5. generate or reuse a full SDK whose cache identity exactly matches all pinned inputs for every discovery run;
6. build the affected projects in full-SDK discovery mode and inventory newly referenced SDK members;
7. map those members to operations;
8. update `roots.json` with source evidence;
9. regenerate the resource closure and scoped SDK;
10. add or update service access helpers required by the command;
11. provide and, where appropriate, run Azure CLI commands used to provision or validate test resources;
12. compile the affected areas and run relevant tests; and
13. report operation, hierarchy, generated-size, and publish-size changes.

A single orchestration command should cover steps 3 through 9:

```pwsh
./eng/sdk-generation/scripts/Update-ServiceProjection.ps1 -Service cosmosdb
```

The command enters full-SDK discovery mode on every run, reusing cached full source only when its complete input identity matches. It should be idempotent, suitable for an agent to run after modifying MCP service code, leave compile items and package references unchanged on failure, and finish by proving a normal build compiles only the projection.

### Prompt instruction changes after the POC

If the POC succeeds:

- `.github/copilot-instructions.md` should require projection updates when service-specific management SDK usage changes;
- `docs/new-command.md` should replace direct package-only instructions with the projection-aware workflow;
- the generated SDK README should explain recovery for ambiguous operation mappings; and
- pull request guidance should require reviewing root and expanded operation diffs.

These changes should happen after the command and file contracts are stable, not as part of an incomplete POC.

## Cosmos DB POC phases

### Phase 1: Establish a reproducible full baseline

1. Record the current `1.4.0-beta.13` Swagger provenance and why it cannot be used with the TypeSpec `@@scope` design.
2. Resolve the `1.5.0` .NET SDK release tag and commit, `tsp-location.yaml`, specification SHA, API version, and emitter package manifest.
3. Pin Node, the dependency lock, emitter options, temporary project template, consuming project settings, runtime dependencies, and all other inputs in the complete generation contract.
4. Sparse-checkout only the DocumentDB directory and declared or validated dependencies.
5. Inspect the `1.5.0` SDK source for shared and handwritten source. Include what MCP requires or record evidence that it is not required.
6. Generate the complete standalone SDK twice from clean directories and compare outputs.
7. Compile the existing Cosmos area against the full generated SDK.
8. Run focused request-behavior checks against the full baseline.
9. Record all source changes needed for the `1.4.0-beta.13` to `1.5.0` transition separately.

Exit criterion: the complete input identity reproduces the full SDK without unexplained differences, and existing Cosmos management behavior compiles and passes focused checks against it.

### Phase 2: Run the scope feasibility gate

1. Implement the strict hierarchy validator contract and its independent negative fixtures before relying on hierarchy results.
2. Use a reviewed static inventory for the existing Cosmos usage.
3. Confirm the direct operations:
   - `Microsoft.DocumentDB.DatabaseAccounts.list`;
   - `Microsoft.DocumentDB.DatabaseAccounts.listKeys`.
4. Resolve the database-account resource and all its operations from the pinned full code model.
5. Produce a temporary specification working copy and scope out unrelated operations.
6. Generate the projected SDK while capturing all diagnostics.
7. Verify the database-account operation set and hierarchy against the full baseline subset with the strict validator.
8. Verify unrelated resources disappear and no blocking missing-`Read` diagnostics occur.
9. Gather preliminary size results comparing the current package-based implementation with the projected source implementation defined in Phase 5.

Exit criterion: operation scopes cleanly remove unrelated resources while preserving the complete selected resource. On failure, record the exact command, inputs, diagnostic, and required support, then stop before reusable discovery or orchestration work.

### Phase 3: Implement and prove reusable discovery

1. Implement full-SDK source discovery mode without modifying committed compile items or package references.
2. Inventory all Cosmos management SDK symbols referenced by Azure MCP.
3. Capture `Data.Name` and `PrimaryMasterKey` model/property requirements.
4. Map SDK members to operations and generate the complete resource and ancestor closure.
5. Review `roots.json` and `expanded-operations.json`.
6. Start from the account-only projection and add a fixture that references an excluded resource without changing the package/spec baseline.
7. Run discovery, expand roots, and regenerate successfully without compiling full and projected source together or leaving temporary project state.
8. Add a separate fixture that references an unreachable model and verify the documented actionable failure without retaining unrelated operations.
9. Remove the fixtures after their behavior is captured in automated tooling tests; do not repurpose the account-listing or key-retrieval behavioral tests.

Exit criterion: every Cosmos SDK symbol has a reviewed operation or reachable-model explanation, new usage expands an existing projection, and unsupported model-only usage fails deterministically.

### Phase 4: Integrate and validate behavior

1. Include the projected generated and shared source directly in `Azure.Mcp.Tools.Cosmos.csproj` and remove its normal service-package reference.
2. Ensure the released package and generated source are never compiled together.
3. Build the Cosmos project and solution normally against only the projected source.
4. Run Cosmos unit tests.
5. Add mocked transport tests for:
   - subscription-level account listing, including paging; and
   - account key retrieval, including request path and API version.
6. Run the relevant Cosmos live scenarios when credentials and resources are available.
7. Publish and execute the dedicated Native AOT smoke-test application against the projected management SDK for the declared RID coverage.
8. Confirm the smoke test roots account listing, paging, key retrieval, and required serialization paths, and that the projected SDK participates in native compilation.
9. Keep the existing native CLI build as a separate check, explicitly noting that it excludes Cosmos.

Exit criterion: existing MCP Cosmos commands preserve their observable management-plane behavior, and the dedicated executable proves the projected management SDK is AOT-safe for its declared coverage.

### Phase 5: Measure current main against the projection

Use identical production publish options, RIDs, trimming, AOT, and compression settings to compare:

1. current `main`, which uses `Azure.ResourceManager.CosmosDB` 1.4.0-beta.13; and
2. the projected 1.5.0 generated source compiled directly into the Cosmos area.

The full generated SDK and released 1.5.0 package are temporary migration and validation inputs. They are not shipped states and are not size baselines.

For the two product configurations, report absolute bytes and percentage deltas for:

- the Cosmos service footprint: `AzureMcp.Cosmos.dll` plus the management package assembly on current main, versus the merged `AzureMcp.Cosmos.dll` after projection;
- self-contained publish directory bytes for supported RIDs;
- compressed distribution package bytes; and
- native/AOT artifact bytes where the product includes Cosmos.

The POC acceptance criteria are:

- at least a 50% reduction in both the uncompressed and individually compressed Cosmos service footprint; and
- a positive complete compressed production-distribution reduction with no RID regression.

The Cosmos area does not publish a Release PDB after generated source is integrated, matching the released package's production symbol footprint and preventing generated symbols from obscuring the comparison. Symbol files, if needed for a separate diagnostic artifact, must not be included in the shipped distribution measurement.

There is no fixed absolute per-service threshold. Percentage reduction shows whether projection removes most of that service's unused SDK surface. Product-level value is evaluated by aggregating absolute distribution savings as additional management SDKs are projected.

The report must also assess generation complexity and ongoing maintenance cost. NuGet global package-cache size is not an acceptance metric because it includes multiple target frameworks, documentation, and cached versions.

Exit criterion: the report states whether the service-footprint thresholds are met, records the complete product-distribution reduction, and records the maintenance-cost decision.

## Validation and failure behavior

The projection update command must fail for:

- an unresolved package release tag;
- missing specification provenance;
- a non-TypeSpec baseline when TypeSpec projection is requested;
- incomplete sparse checkout dependencies;
- an incomplete generation-input identity;
- unpinned Node, TypeSpec, or emitter packages;
- a corrupt matching cache entry that cannot be safely regenerated under the integrity policy;
- any attempted reuse of a cached artifact under a mismatched identity;
- full and projected service assemblies appearing in the same discovery or build graph;
- a root operation missing from the full code model;
- ambiguous SDK-member-to-operation mapping;
- an unreachable model-only SDK dependency;
- a selected resource without a retained `Read`;
- operation-set drift;
- resource hierarchy drift;
- temporary discovery references remaining after success or failure;
- an unexpected service package assembly in the normal output; or
- generated output that is not reproducible.

An empty cache, a valid older entry, or any other absence of an exact entry is not a failure; it causes generation for the current identity. Cache tests must cover an empty cache, an exact valid hit, changed package input, changed tooling or emitter-option input, a valid older entry, a corrupt matching entry, incomplete authoritative inputs, duplicate-assembly prevention, and cleanup of temporary references after failure.

A package/spec update must not automatically accept renamed or removed roots. The tool should report them for agent and reviewer action.

## POC deliverables

The Cosmos DB POC should produce:

1. a package-to-spec provenance resolver;
2. a sparse specification checkout script;
3. the complete pinned generation-input contract;
4. a reproducible full standalone Cosmos SDK generation command;
5. a recorded early scope feasibility result;
6. a full-SDK semantic discovery mode;
7. `roots.json` for direct MCP operations;
8. generated resource-aware `expanded-operations.json`;
9. generated C# scope decorators in a temporary spec working copy;
10. projected Cosmos generated and shared source compiled directly by the service area;
11. an MCP-owned strict hierarchy validator and its positive and negative regression fixtures;
12. focused request-level, discovery-expansion, model-only-policy, and cache-state tests;
13. a dedicated projected-SDK Native AOT smoke-test application and execution record; and
14. a current-main-versus-projection size and feasibility report.

## Acceptance criteria

The POC is successful when all of the following are true:

- the exact specification commit is derived from the chosen .NET SDK package release;
- only required specification paths are sparsely checked out;
- generation is reproducible from the complete committed input identity;
- required shared and custom source is pinned, or its omission is justified against MCP usage;
- the early scope gate removes unrelated resources without blocking diagnostics;
- developers do not manually edit operation or hierarchy metadata;
- the coding agent can discover new usage against the full SDK and update an existing projection without changing the package baseline;
- unsupported model-only usage fails without silently retaining operations;
- Cosmos account listing and key retrieval remain functional;
- all operations of the selected Cosmos account resource are retained;
- unrelated Cosmos resources and operations are absent;
- the MCP-owned strict validator proves the selected resource hierarchy is unchanged from the full pinned generation and all negative fixtures fail as intended;
- a dedicated Native AOT executable roots and executes the projected management paths successfully for the declared RID coverage;
- native CLI results are not used as projection coverage while Cosmos remains excluded;
- normal builds do not access `azure-rest-api-specs`; and
- the current-main-versus-projection measurements meet the documented service-footprint and product-distribution criteria, or the POC is reported as unsuccessful.

## Follow-up after the POC

If successful, inventory every service-specific `Azure.ResourceManager.*` dependency in `Directory.Packages.props` and classify it as:

- directly projectable from its current TypeSpec-generated package;
- requiring a reviewed upgrade from a Swagger-generated package;
- requiring custom SDK source not represented by the specification; or
- currently unsuitable for this approach.

Services should then be migrated independently, prioritized by distribution-size savings and operation count. The shared command-authoring prompt and agent instructions should be updated only after the Cosmos workflow is reliable and documented.
