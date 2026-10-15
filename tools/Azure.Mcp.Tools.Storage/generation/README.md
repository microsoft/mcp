# Storage MCP generation prototype

This is a working, opt-in **container-create vertical slice**, not a regeneration
of the complete Storage toolset or the upstream Storage API specification.
`storage.tsp` is an explicit, bounded HTTP operation projection. Its unselected
container-get operation demonstrates curation; the existing get/list/prefix
composition remains handwritten.

The stock `@typespec/http-client-csharp` frontend produces the real TCGC code
model. A MEF-exported `CodeModelGenerator` and `OutputLibrary` then use the existing
C# type, property, method, statement and attribute providers and standard writer.
They generate no HTTP client. The result in `..\src\Generated` consists of:

| Generated | Handwritten customization |
| --- | --- |
| Catalog identity, draft description, reviewed safety flags, registration | `ContainerCreateCommand.CustomizedDescription` and command orchestration |
| Required Account/Container options and optional Tenant | Existing authentication and account-to-endpoint resolution |
| The real SDK `GetBlobContainerClient` / `CreateAsync(None)` binding | Follow-up properties read and `ContainerInfo` output shaping |
| Result wrapper, AOT-safe JSON context, status-code error default | RBAC, already-exists and not-found error overrides |

`storage.tools.json` selects by the TypeSpec cross-language operation ID, preserves
the existing public command identity/group and declares handwritten composites.
`useDescriptionOverride: false` selects the spec-derived draft; `true` selects the
existing human description. Explicit safety overrides preserve the existing
destructive/non-idempotent flags rather than assuming PUT makes this tool
idempotent. Conservative HTTP defaults are drafts, not security guarantees.

## Reproduce

Use .NET 10, PowerShell 7 and Node.js 22 LTS (22.18 or later is recommended for all
transitive package engine requirements). From this directory:

```powershell
npm ci --ignore-scripts
.\Generate.ps1
.\Generate.ps1 -Verify
```

The toolchain is pinned in `package-lock.json`. The generator assemblies come from
the emitter's npm bundle: the documented generator NuGets are not available on the
public feed. The script builds the Storage prerequisite and extension explicitly,
then loads the prebuilt plugin directory. This avoids the current emitter's
isolated auto-build handling of an already-built plugin project's `obj` files.
Saved frontend inputs at the Storage toolset root are ignored; generated C# is
checked in. Only generated files belong in `src\Generated`, because the shared
generator replaces that directory.

The shared SDK writer predates C# `required` and nullable-reference syntax. A small
Roslyn syntax adapter restores those modifiers on generated options. This matters:
the MCP binder uses `RequiredMemberAttribute`, not just the declared CLR type.

The original implementation remains the default. Enable the generated path with:

```powershell
dotnet build ..\src\Azure.Mcp.Tools.Storage.csproj -p:UseGeneratedStorageTools=true
```

The build switch changes the options, catalog, registration, SDK create call,
serialization context and generic error default together. Both variants expose the
same command path, options, human description, safety flags and successful output.
Only the generated variant adds a status-code prefix to otherwise-unhandled SDK
errors; specialized human errors still take precedence.

## Validate and compare

From the repository root:

```powershell
dotnet build tools\Azure.Mcp.Tools.Storage\generation\Tests\StorageMcp.Generator.Tests.csproj
dotnet test --project tools\Azure.Mcp.Tools.Storage\generation\Tests\StorageMcp.Generator.Tests.csproj --no-build
dotnet test --project tools\Azure.Mcp.Tools.Storage\tests\Azure.Mcp.Tools.Storage.Tests\Azure.Mcp.Tools.Storage.Tests.csproj -p:UseGeneratedStorageTools=false --filter-namespace Azure.Mcp.Tools.Storage.Tests.Blob.Container
dotnet test --project tools\Azure.Mcp.Tools.Storage\tests\Azure.Mcp.Tools.Storage.Tests\Azure.Mcp.Tools.Storage.Tests.csproj -p:UseGeneratedStorageTools=true --filter-namespace Azure.Mcp.Tools.Storage.Tests.Blob.Container
```

From this directory, `.\Compare.ps1` builds both real server variants, saves their
complete `tools list` responses in `comparison`, and requires exact catalog/schema
parity. When `useDescriptionOverride` is false, it permits only the intentional
create-description difference, so the evaluator can compare the draft with the
human original. The entire tool universe is retained so selection evaluation has the same
competitors in both variants. It does not call Azure or score prompts.

`.\Compare.ps1 -Evaluate` additionally runs the existing ToolDescriptionEvaluator
against both saved catalogs and the canonical Storage prompts in
`servers\Azure.Mcp.Server\docs\e2eTestPrompts.md`. It requires `AOAI_ENDPOINT`,
`TEXT_EMBEDDING_API_KEY`, and access to the evaluator's configured embedding model.
Missing credentials cause an explicit failure, not synthetic scores or fallback
tools. Description parity alone does not measure model quality.

From the repository root, the existing recorded/live test remains the end-to-end
Azure behavior check:

```powershell
dotnet test --project tools\Azure.Mcp.Tools.Storage\tests\Azure.Mcp.Tools.Storage.Tests\Azure.Mcp.Tools.Storage.Tests.csproj -p:UseGeneratedStorageTools=false --filter-class '*StorageCommandTests' --filter-method Should_create_container
dotnet test --project tools\Azure.Mcp.Tools.Storage\tests\Azure.Mcp.Tools.Storage.Tests\Azure.Mcp.Tools.Storage.Tests.csproj -p:UseGeneratedStorageTools=true --filter-class '*StorageCommandTests' --filter-method Should_create_container
```

These commands require the existing recorded-test proxy/assets or an explicitly
configured `.testsettings.json` and authorized Storage resources. They must not
implicitly deploy resources. Offline SDK substitutes are unit coverage, not a
claim that Azure end-to-end prompts or model evaluation passed.

## Deliberate limits

The pilot supports one audited private-container-create SDK binding and the
existing group path. Unsupported mappings, ambiguous/missing operation IDs,
different wire parameters, paging and LROs fail explicitly. A full Storage pilot
still needs reviewed bindings for the remaining ordinary operations, an upstream
spec-source decision, and real prompt/evaluator measurements. The existing
published Storage SDK is consumed as-is; this prototype does not establish that
the package was generated from this projection. Resource Graph queries and
composite tools are deliberately not regenerated.
