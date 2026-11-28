# JSON Schema enum parameters

## Decision

Adopt enum-typed options as the default for small, truly closed sets of named string values, and migrate eligible parameters incrementally. Do not convert open catalogs, extensible SDK string enums, or examples into closed contracts.

The five-tool pilot establishes stronger machine-readable contracts, shared validation, and modest schema overhead. With description cleanup, the measured definition budget falls **7.32%**. It does **not** establish better LLM generation accuracy or fewer inference/retry tokens; this experiment deliberately uses local tests and tokenization, not model inference.

## Pattern

Use the existing two-generic command and `[Option]` binding pattern:

```csharp
public enum DrillExecutionMode
{
    Failover,
    TestFailover,
}
```

In the options class, in its own file:

```csharp
[Option(Description = "Whether to perform a real failover or a test failover.")]
public required DrillExecutionMode Mode { get; set; }
```

This produces:

```json
{
  "type": "string",
  "enum": ["Failover", "TestFailover"],
  "description": "Whether to perform a real failover or a test failover."
}
```

The parameter also appears in the enclosing schema's `required` array. Use `DrillExecutionMode?` for an optional parameter: omission must remain omission, not the enum's first member or an inferred customer choice. The pilot leaves service-side defaults unchanged by forwarding `null` for omitted optional enums.

Rules:

1. Reuse an existing appropriate enum, including a BCL enum such as `DayOfWeek`, or put a new enum in its own file.
2. Member names are the canonical wire values. Preserve existing spelling and parameter names. JSON Schema enum matching is case-sensitive; the existing binder remains case-insensitive for CLI/MCP compatibility.
3. Use `required` when an explicit choice is necessary. For optional choices, use a nullable enum and retain the existing defaulting layer. This pilot does not introduce enum-valued `[Option(DefaultValue = ...)]` attributes.
4. Do not add a per-command membership validator, `Enum.TryParse`, or a second allowed-values list. Keep validation for dependent fields, resource names, ranges, selectors, and consent requirements.
5. Keep existing service contracts unchanged when they accept strings; convert the validated enum with `ToString()` at the command boundary.
6. Describe meaning, defaults, consequences, units, and dependencies in prose. Do not repeat a bare membership list in both the option and command descriptions.
7. Input-only enums do not need a JSON converter or an output serialization registration. Register models in the source-generated JSON context if they are serialized as responses.
8. Test the actual advertised schema and the complete parsing/binding path, not just direct construction of a valid options object.

### Supported scope and compatibility boundaries

The existing binder supports individual enum names and arrays of names. It rejects numeric representations, unknown names, empty/whitespace values, and comma-separated composite names. This is not a pattern for `[Flags]` combinations or numeric JSON enums.

Do not silently rename values containing punctuation into C# identifiers. JSON-only naming policies or `JsonStringEnumMemberName` attributes are not interpreted by the current binder. Hyphenated values, significant empty strings, aliases that are not member names, and finite numeric domains need explicit wire-value support before migration.

For example, the sampled deployment rules tool's `iac-type` intentionally accepts `""`, with existing tests covering Azure CLI scripts and the Azure Developer CLI default. It remains a string. Its comma-separated `resource-types` parameter also remains a string; changing it to an enum array would change the wire shape.

Validation deliberately tightens previously permissive, undocumented inputs. Document and test that change; preserve documented aliases, omission behavior, defaults, and conditional requirements. A familiar list of SKUs, regions, languages, versions, or resource types is not necessarily a closed domain.

### Shared implementation

[OptionTypeHandler](../../core/Microsoft.Mcp.Core/src/Options/OptionTypeHandler.cs) already creates string-backed CLI options for enum properties, validates every token against `Enum.GetNames`, supplies completions, and binds names to enum values. No new enum validator is added.

Previously, [OptionSchemaGenerator](../../core/Microsoft.Mcp.Core/src/Areas/Server/Commands/OptionSchemaGenerator.cs) saw only the CLI option's `string` or `string[]` value type, so the enum constraint disappeared. The handler now retains those same names as option-associated metadata. The generator emits them as `enum` for scalars or `items.enum` for arrays. No additional reflection or dynamic generic construction is introduced.

[CLI option metadata](../../core/Microsoft.Mcp.Core/src/Models/Option/OptionInfo.cs) includes `enum` for scalar names and `elementEnum` for collection element names, consistent with its existing flat `elementType` convention. `elementEnum` is CLI metadata, not a JSON Schema keyword. CLI help still displays choices, for example `--deployment-tool <AzCli|AZD>`.

All-mode MCP definitions and namespace/single-mode child discovery share the schema generator. The outer namespace/single router schemas remain generic: publishing enum constraints in a child learn response does not itself make the outer router's `parameters` object constrained decoding. The server proxy forwards child schemas without rewriting them.

## Random pilot selection

Baseline: `e666257b715965b549a88c3996114ec182506a07`.

The screened cohort contains 30 registered Azure tools with at least one scalar string parameter advertising a small closed name-based domain. The full cohort and calls are in the [sample manifest](json-schema-enum-parameters.sample.json). Existing enum-typed parameters, single-choice parameters, numeric domains, compound/CSV values, punctuation-dependent domains, and catalog-only/extensible choices were outside this pilot's scope.

Rank each eligible name by `SHA256(UTF8("20261103" + LF + toolName))`, ascending hexadecimal digest, and take the first five without replacement. The seed was fixed before drawing; no tools were reselected after examining migration difficulty.

| Selected tool | Migrated parameters and canonical values |
|---|---|
| `managedlustre_fs_blob_autoimport_create` | Optional `conflict-resolution-mode`: Fail, Skip, OverwriteIfDirty, OverwriteAlways; optional `admin-status`: Enable, Disable |
| `resiliency_recoveryplan_validateforfailover` | Optional `user-consent`: Unspecified, Allowed |
| `deploy_iac_rules_get` | Required `deployment-tool`: AzCli, AZD |
| `managedlustre_fs_update` | Optional `maintenance-day`: Sunday through Saturday; optional `root-squash-mode`: All, RootOnly, None |
| `resiliency_drill_start` | Required `mode`: Failover, TestFailover |

This is seven parameters and 22 canonical values across five tools, clustered in three toolsets. It is a random sample within a screened Azure cohort, not a statistically representative sample of every Azure/Fabric tool or every possible enum domain.

## Experiment

### Method

1. Build the baseline server and capture its actual MCP `tools/list` definitions.
2. Migrate the selected parameters and capture their actual definitions again.
3. Construct an **enum-only control** by copying just the migrated enum constraints into the captured baseline definitions, leaving every original description intact.
4. Tokenize each compact protocol `Tool` JSON definition, including annotations and metadata, using the existing performance harness's GPT-4o `o200k_base` tokenizer.
5. Independently tokenize five synthetic MCP call-parameter objects (`name` plus `arguments`), one per tool, using identical canonical arguments before and after. The harness fails if the calls differ.
6. Test schema contents, parsing, binding, omission, normalization, service forwarding, and existing dependent-field validation locally. Do not execute cloud operations.

The [baseline capture](json-schema-enum-parameters.baseline.json) and [final results](json-schema-enum-parameters.results.json) retain the exact payloads and counts. Definition totals below are sums of independently tokenized tool objects, not a provider-specific request envelope. Client rewriting, tool-array framing, whitespace, tokenizer choice, caching, and router discovery strategy can change billed or effective context usage.

### Does this improve tool-call correctness?

**It improves the contract and deterministic rejection of unsupported choices; model-generation accuracy remains unmeasured.**

- All seven advertised properties now have exact string enums, preserving required/optional status.
- Tests exercise all **22 canonical values and 22 lowercase forms** through MCP dictionary parsing.
- Six malformed strings per parameter (unknown, `0`, `999`, composite `a,b`, empty, whitespace) produce **42 rejection checks**. The baseline unconstrained string schemas admit all 42; the migrated enum schemas exclude all 42, and the parser rejects them.
- Shared binder tests also reject invalid array elements and preserve nullable omission.
- Command tests check canonical service forwarding, case compatibility, existing cross-field rules, errors before service invocation, and the preserved empty `iac-type` contract.
- Drill mode and recovery consent already had manual command validation. Migrating those centralizes existing checks; it is not evidence that previously invalid values reached Azure.
- Some Managed Lustre choices were rejected later in service parsing; deployment rules could otherwise fall through to generic rules for an unsupported tool.

Enums do not establish that the chosen valid value matches user intent, that a resource exists, that permissions are sufficient, or that a destructive operation is authorized. In particular, do not infer `Allowed` consent or choose a drill execution mode merely because the schema offers it.

A schema-aware client can validate enum membership or constrain generation if its model supports it. A client that ignores the constraint may still generate invalid calls; the server's binder remains the enforcement boundary. No LLM success rate, retry rate, confidence score, or statistical significance is claimed.

### Tool-definition context usage

| Tool | Baseline tokens | Enum-only control | Final enums + descriptions | Final delta |
|---|---:|---:|---:|---:|
| Autoimport create | 789 | 808 | 576 | -213 |
| Recoveryplan validate for failover | 394 | 401 | 401 | +7 |
| Deployment IaC rules | 408 | 416 | 399 | -9 |
| Managed Lustre update | 543 | 568 | 578 | +35 |
| Drill start | 229 | 238 | 236 | +7 |
| **Total** | **2,363** | **2,431** | **2,190** | **-173 (-7.32%)** |

**Enums alone cost 68 tokens (+2.88%)** in this sample. They are structured constraints, not intrinsically a compression mechanism.

Description edits save 241 tokens relative to that enum-only control. Most savings come from replacing the autoimport command's duplicated required/optional parameter roster with an action-and-result summary. Some of that roster could already have been removed without enums; the total reduction must not be attributed solely to the enum keyword. The update tool grows because useful dependent-field guidance is retained rather than stripped for a favorable token count.

The cost applies whenever a client includes those definitions. With progressive discovery, it applies when the child schemas are learned, not necessarily to every outer-router turn. Large domains or repeated inline definitions can outweigh prose savings.

### Tool-calling token usage

| Synthetic call | Baseline tokens | Migrated tokens |
|---|---:|---:|
| Autoimport create | 59 | 59 |
| Recoveryplan validate for failover | 44 | 44 |
| Deployment IaC rules | 30 | 30 |
| Managed Lustre update | 63 | 63 |
| Drill start | 32 | 32 |
| **Total** | **228** | **228** |

Canonical argument strings and parameter names are unchanged, so these successful call payloads save **zero tokens**. Schema enums do not replace values with numeric indexes. Different casing or different choices can tokenize differently, but no systematic output saving follows from adding an enum.

Fewer invalid attempts could save failed-call, error-response, and repair-call tokens. That is a plausible downstream benefit, **not measured here**. Actual inference usage, reasoning tokens, latency, and retry savings require a controlled model evaluation using the same prompts, model/version, generation settings, client mode, and repeated trials.

### Description improvements unlocked

- Make the enum declaration authoritative and publish its membership in the schema rather than maintaining conflicting prose lists and membership checks.
- Replace command-level option inventories with concise purpose, applicability, and result summaries; keep service/resource keywords needed for tool selection.
- Spend parameter prose on **how to choose** between values: conflict-policy consequences, real versus test failover, defaults on omission, and required companion fields.
- Retain safety guidance: explicit consent, customer-selected operation mode, and resource selectors are not redundant enum membership text.
- Remove repeated spelling/case advice from model-facing descriptions when canonical schema values already provide the spelling, while keeping documented CLI case compatibility.
- Keep descriptions where names alone are ambiguous. A shorter definition is not a successful optimization if it removes semantics or worsens tool selection.

The pilot demonstrates these edits, but does not measure their effect on semantic tool selection. ToolDescriptionEvaluator and model-based evaluations were not run in the requested local-only experiment.

## Rollout recommendation

**A staged migration is worth doing for eligible closed name-based sets. A blind full conversion is not.**

The direct wins are discoverable constraints, earlier consistent errors, CLI completions/help, and eliminating duplicate validation. Enum-only context overhead was small in this pilot, and successful call payloads did not grow. Description cleanup can offset that overhead, but is a separate editorial improvement.

Before expanding:

1. Inventory each tool's actual accepted domain, aliases, defaults, conditional rules, and whether the domain is genuinely closed. Do not freeze an expandable SDK catalog.
2. Prioritize small action/mode/policy selectors. Migrate one tool at a time with schema and parser regression tests.
3. Preserve wire names, required/optional status, omissions, documented empty/alias values, service contracts, and array shapes. Add explicit wire-value support before attempting punctuation, numeric, or empty-string domains.
4. Keep declaration, binding validation, discovery metadata, and tests sourced from the same enum. Do not reintroduce per-command membership lists.
5. Measure full definitions after every description change. Verify tool-selection quality and safety guidance separately from token counts.
6. Before claiming model accuracy or retry-token savings, add a controlled LLM evaluation, especially for generic router modes where child schemas may be supplied as text rather than native tool definitions.

Thus the repository should adopt this pattern and continue eligible migrations, but should not advertise an empirically proven LLM-accuracy improvement or automatically convert every parameter whose documentation mentions a list.

## Reproduction and validation

Use the current measurement harness against a baseline executable to refresh the baseline, or use the checked-in capture. Capturing definitions does not call any Azure tool or require cloud resources.

```powershell
dotnet build .\servers\Azure.Mcp.Server\perf\Azure.Mcp.Server.PerformanceBenchmarks -m:1 -nr:false
$exe = (Resolve-Path .\servers\Azure.Mcp.Server\src\bin\Debug\net10.0\azmcp.exe).Path
dotnet run --no-build --project .\servers\Azure.Mcp.Server\perf\Azure.Mcp.Server.PerformanceBenchmarks -- `
  --enum-parameters $exe `
  .\docs\design\json-schema-enum-parameters.sample.json `
  .\docs\design\json-schema-enum-parameters.results.json `
  .\docs\design\json-schema-enum-parameters.baseline.json
```

Omit the last argument to capture a new baseline. Only do that against an unmodified baseline executable; do not overwrite the historical capture with migrated definitions.

Focused validation commands, using .NET 10 Microsoft.Testing.Platform and xUnit v3:

```powershell
dotnet test --project .\core\Microsoft.Mcp.Core\tests\Microsoft.Mcp.Core.Tests `
  --filter-class '*EnumOptionSchemaTests' --filter-class '*OptionBinderTests'
dotnet test --project .\core\Azure.Mcp.Core\tests\Azure.Mcp.Core.Tests `
  --filter-class '*EnumParameterMigrationTests' --filter-class '*OptionSchemaGeneratorTests' `
  --filter-class '*CommandFactoryToolLoaderTests' --filter-class '*NamespaceToolLoaderTests' `
  --filter-class '*ServerToolLoaderTests' --filter-class '*SingleProxyToolLoaderTests'
dotnet test --project .\tools\Azure.Mcp.Tools.ManagedLustre\tests\Azure.Mcp.Tools.ManagedLustre.Tests `
  --filter-class '*AutoimportJobCreateCommandTests' --filter-class '*FileSystemUpdateCommandTests'
dotnet test --project .\tools\Azure.Mcp.Tools.ResilienceManagement\tests\Azure.Mcp.Tools.ResilienceManagement.Tests `
  --filter-class '*DrillStartCommandTests' --filter-class '*RecoveryPlanValidateForFailoverCommandTests'
dotnet test --project .\tools\Azure.Mcp.Tools.Deploy\tests\Azure.Mcp.Tools.Deploy.Tests `
  --filter-class '*RulesGetCommandTests'
```

The focused runs passed **60 + 307 + 44 + 28 + 13 = 452 tests**. Builds passed with zero warnings and errors, including the core project's AOT/trim analyzers. Changed-file whitespace formatting, spelling, command-reference metadata regeneration, changelog schema validation, and byte-for-byte measurement reproduction also passed.

Native publishing, live/recorded cloud tests, full-repository tests, and model inference were not run. Concurrent builds exhausted this machine's memory; rerunning serially with one build worker succeeded.
