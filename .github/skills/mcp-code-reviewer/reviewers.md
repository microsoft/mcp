# Reviewer Lenses

Apply each relevant lens to the same pull request context and change map. These are perspectives for one review, not independent reviewers.

## Shared Rules

- Produce zero to three candidate findings per lens.
- Flag only issues a maintainer should act on before or soon after merge.
- Prove each concern from the diff, surrounding code, repository guidance, or linked requirements.
- Do not summarize the change or repeat checklist items that the pull request satisfies.
- Do not flag style preferences without a concrete correctness or maintenance consequence.
- Use the candidate format from [findings.md](findings.md).

## Fixed Lenses

### 1. Security

Focus on:

- Hardcoded credentials, tokens, connection strings, or sensitive test recordings
- Injection through commands, queries, paths, templates, URLs, or resource identifiers
- Unsafe endpoint handling, server-side request forgery, and missing host or cloud validation
- Authentication, authorization, tenant boundaries, and least-privilege RBAC
- Sensitive data in logs, telemetry, errors, or tool responses
- Unsafe workflow permissions, unpinned actions, dependency changes, and executable downloads
- Input validation for user-controlled names, identifiers, sizes, and allowed values

A remotely exploitable flaw, credential exposure, privilege escalation, or data leak is blocking.

### 2. Correctness, C#, and AOT

Focus on:

- Incorrect control flow, null handling, collection behavior, and exception paths
- Cancellation propagation, async behavior, disposal, and resource lifetime
- Warning-producing code in projects that treat warnings as errors
- Source-generated `System.Text.Json` coverage for response and model types
- Reflection, dynamic activation, or serialization patterns that are unsafe for trimming or native compilation
- Command naming, primary constructors, sealed command classes, and static members when deviations create repository inconsistency
- Error handling that bypasses `HandleException` or returns a success-shaped failure

Treat a deterministic compile failure, runtime failure, or AOT break as blocking.

### 3. Azure Service Integration

Focus on:

- Correct Azure SDK client, API version, resource scope, and operation semantics
- Cloud-aware endpoints and sovereign cloud behavior
- Credential, tenant, subscription, and resource group propagation
- Retry policies, polling, pagination, throttling, and cancellation
- Resource Graph use for read operations when it matches established patterns
- Idempotency and cleanup for write operations
- Cost or data-loss consequences from resource defaults and destructive operations

Verify uncertain SDK behavior against current official documentation when documentation tools are available.

### 4. Architecture and Remote Execution

Focus on:

- Transport-agnostic command behavior
- Stateless, thread-safe command and service implementations
- Dependency injection lifetimes and per-request state
- Direct `HttpContext` access or transport-specific branching
- On-behalf-of identity and hosting-identity behavior across concurrent users
- Abstractions that duplicate existing helpers or put responsibilities in the wrong layer
- Public API, option, wire-format, and command compatibility
- Failure modes, rollback safety, and blast radius

A cross-user data leak, shared mutable request state, or unplanned breaking change is blocking.

### 5. Testing and Validation

Focus on:

- Tests for changed success, validation, authorization, and failure paths
- Assertions that verify behavior rather than only successful execution
- Recorded live tests for commands that interact with Azure resources
- Required test infrastructure, assets metadata, and post-deployment setup
- Correct use of `RecordedCommandTestsBase` when transitioning live tests
- `IHttpClientFactory.CreateClient` use in clients involved in recorded tests
- Deterministic fixtures, sanitization, cleanup, and playback behavior
- RBAC coverage for remote and on-behalf-of scenarios when permissions affect behavior
- Validation claims in the pull request that are absent, stale, or contradicted by checks

Missing tests are blocking only when the untested behavior is required for correctness, security, or an established merge gate.

### 6. Tool Contract and Repository Completeness

Cross-reference the repository instructions and pull request checklist. Focus on:

- Command and service registration
- Flat option classes, required interfaces, and established option names
- Response model registration in the correct JSON serialization context
- Command reference and README updates
- End-to-end prompt coverage
- Consolidated tool mappings for new, renamed, or removed tools
- Tool description evaluation evidence when descriptions change
- Changelog entry schema and required entries
- Rename, compatibility, and breaking-change requirements

Report the consequence of a missing surface, such as a command not being discoverable, a build gate failing, or a client contract drifting.

### 7. Tool Experience and Documentation

Focus on:

- Tool names, descriptions, options, and examples that guide reliable selection and invocation
- Actionable error messages that do not disclose sensitive details
- Documentation accuracy for changed behavior
- Consistency between command behavior, generated command reference, README content, and examples
- Clear behavior for optional inputs, defaults, empty results, and partial failures
- Discoverability for new capabilities

Do not suggest generic wording changes. Flag text only when it can mislead a user or an agent about behavior.

### 8. Product Intent

Focus on:

- Alignment with the linked issue and pull request description
- Missing acceptance criteria or incomplete end-to-end behavior
- Scope added without a stated requirement or compatibility plan
- Customer impact, migration needs, and support burden
- Whether tests and documentation demonstrate the promised outcome

Do not turn product preferences into blockers. Report only a concrete mismatch with stated intent or established behavior.

## Dynamic Lenses

Apply an additional domain lens when changed files match these signals:

| Changed files | Additional focus |
| --- | --- |
| `.github/workflows/**`, `eng/pipelines/**`, pipeline scripts | CI permissions, event trust, script injection, secrets, generated-file boundaries |
| `*.bicep`, `*.tf`, ARM templates, `test-resources*` | deployment scope, RBAC, secure defaults, cleanup, regional behavior |
| Paths containing `auth`, `identity`, `credential`, `token`, or `tenant` | identity flow, token audience, tenant isolation, secret handling |
| JSON contexts, serializers, response models, trimming configuration | AOT reachability, type registration, wire compatibility |
| Project files, package manifests, lock files, packaging scripts | dependency necessity, version consistency, supply chain, package contents |
| Paths containing `telemetry`, `metrics`, `tracing`, or `logging` | privacy, cardinality, correlation, sensitive data, failure isolation |

For a dynamic lens:

1. List the matching files.
2. Trace the domain-specific behavior they change.
3. Apply current repository conventions and official platform guidance.
4. Produce no finding when the change is correct and complete.
