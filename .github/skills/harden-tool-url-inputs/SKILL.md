---
name: harden-tool-url-inputs
description: 'Audit and harden one or more tool directories against URL hijacking and SSRF by tracing MCP inputs to URL construction and applying EndpointValidator with cloud-specific Azure endpoint allow-lists. Use when: validate URL inputs, secure endpoints, prevent URL hijacking, SSRF hardening, audit tool directories, apply EndpointValidator.'
argument-hint: 'Provide one or more repo-relative directories under tools/ (for example: tools/Azure.Mcp.Tools.Acr tools/Azure.Mcp.Tools.Compute)'
user-invocable: true
disable-model-invocation: false
---

# Harden Tool URL Inputs

Audit and update every requested directory so MCP-influenced values cannot hijack a URL, URI, host, or client endpoint. Use `EndpointValidator` at the final URL boundary, with the configured Azure cloud and the narrowest appropriate allow-list.

This is a scoped implementation skill, not a report-only audit. Make the fixes and add tests unless the user explicitly requests analysis only.

## Required Input and Scope

1. Accept one or many repository-relative directories under `tools/`.
2. Resolve every path and confirm it is inside the repository's `tools/` directory. Reject files or directories outside that boundary.
3. If no directory is supplied, ask the user for at least one. If a supplied path does not exist or its intended scope is ambiguous, ask before editing.
4. Treat each supplied directory as an exhaustive scope. Do not stop after finding the first vulnerable URL flow.
5. Do not modify other tool directories. Shared changes to `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.AllowLists.cs` and its tests are allowed when a scoped tool needs a new Azure service allow-list.
6. Preserve unrelated worktree changes. Read the repository and nearest scoped instructions before editing.

## Pattern Sources

Before implementing, inspect the current versions of:

- `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.cs`
- `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.AllowLists.cs`
- `core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.Tests/Helpers/EndpointValidatorTests.cs`
- Commit `54e99bf`

Use the current API as the source of truth and `54e99bf` as the pattern anchor. Its representative cases include:

- ACR: validate the exact `Uri` passed to the data-plane SDK and expose a small pure helper for focused tests.
- App Configuration and Communication: validate a discovered or user-supplied endpoint immediately before client construction.
- Compute: distinguish URL input from a resource ID and validate only the URL branch with the `storage-blob` allow-list.
- Cosmos and Foundry Extensions: try a small explicit set of acceptable Azure service types, then fail closed; an early any-cloud option check never replaces the authoritative configured-cloud service check.
- Load Testing: use `ValidatePublicTargetUrl` for a deliberately arbitrary public target.
- Service Bus: reject URL-component characters in a bare host and still validate the completed endpoint.

## Threat Model

- Treat every MCP option as untrusted, including values named `account`, `cluster`, `endpoint`, `host`, `namespace`, `registry`, `resource`, `server`, `url`, `uri`, or `workspace`.
- Track derived values. Validation is required when an untrusted name is interpolated into a host even if the option itself is not described as a URL.
- Treat endpoint-like Azure resource metadata as a trust boundary before using it in a data-plane client. Resource Graph or ARM returning a string does not make that string safe for network use.
- Validate the completed absolute endpoint, not only the resource name or one string fragment.
- Treat path, query, and "relative" inputs as potential authority changes. APIs such as `new Uri(baseUri, value)` accept absolute and network-path references that can replace the base host.
- URL parsing, escaping, encoding, or rejecting a few special characters is not a replacement for host allow-list validation.
- `EndpointValidator` authorizes the scheme and host; it does not make untrusted path or query syntax safe. Build those components with typed SDK APIs or correctly escaped path/query helpers.
- A successful `Uri` parse proves syntax, not authorization to contact the host.

## Workflow

### 1. Establish the baseline

1. Inspect `git status` and the scoped diff before editing.
2. Read command registration, options, commands, services, helpers, and tests in every supplied directory.
3. Identify the top-level MCP tool namespace from registration code. Do not infer it only from the directory name.
4. Check whether each Azure service endpoint already has a key in `EndpointValidator.AllowLists.cs`.

### 2. Inventory input-to-URL flows

Search the full scope for both sources and sinks. Useful candidates include:

- `[Option]` properties and legacy option definitions
- `http://`, `https://`, `Endpoint`, `Host`, `Url`, and `Uri`
- `new Uri`, target-typed `new(...)`, `UriBuilder`, and connection strings
- base-plus-relative combinations such as `new Uri(baseUri, value)` and `HttpClient` methods called with string request targets
- Azure SDK or other client constructors that accept a URI, endpoint, namespace, or host
- `HttpClient.BaseAddress`, `HttpRequestMessage`, `RequestUri`, `GetAsync`, `PostAsync`, `PutAsync`, and `SendAsync`
- interpolated REST paths, request URLs, callback URLs, webhook URLs, and upload/download sources
- helpers that append paths, ports, queries, or fragments

Search results are only candidates. Trace each MCP option through command, service, and helper calls to its final network sink. Also inspect URL construction that uses endpoint-like values returned from ARM, Resource Graph, configuration, files, or prior service calls.

For every candidate, determine one disposition:

| URL flow | Required disposition |
| --- | --- |
| Azure data-plane endpoint | `ValidateAzureServiceEndpoint` |
| Known non-Azure service with an exact host set | `ValidateExternalUrl` |
| Deliberately arbitrary public HTTP(S) target | `ValidatePublicTargetUrl` |
| Input-derived path or query on an allowed base | Reject absolute/network-path replacement, construct or escape the component with an appropriate API, then validate the final absolute URL |
| Typed Azure control-plane operation | No endpoint validation; use the typed ARM SDK and `ResourceIdentifier` rather than a raw interpolated ARM URL |
| Compile-time fixed URL with no influenced component | Document as not input-derived; no runtime validation needed |

Do not classify an Azure endpoint as an arbitrary public target merely to avoid defining a service allow-list.

### 3. Implement Azure service validation

For an Azure data-plane endpoint, validate the exact absolute URL before the SDK client, request, or other network-capable object can use it:

```csharp
string endpoint = $"https://{resource}.{serviceDomain}";

EndpointValidator.ValidateAzureServiceEndpoint(
    endpoint: endpoint,
    serviceType: "service-key",
    armEnvironment: AzureService.CloudConfiguration.ArmEnvironment,
    executingToolNamespaceName: "toolnamespace");

var client = new ServiceClient(new Uri(endpoint), credential, options);
```

Apply these rules:

1. Use named arguments. `serviceType` and `executingToolNamespaceName` have different security meanings.
2. `serviceType` is the exact key for the endpoint domain in `EndpointValidator.AllowLists.cs`.
3. `executingToolNamespaceName` is the command's registered top-level tool namespace. It may differ from `serviceType`; for example, `compute` may consume `storage-blob`, and `foundryextensions` may consume `foundry` or `azure-openai`.
4. Pass `AzureService.CloudConfiguration.ArmEnvironment` at the authoritative service boundary. Do not hardcode public cloud when configured cloud context is available.
5. If option validation cannot access the configured cloud, it may accept endpoints valid in any explicitly supported cloud for early feedback, but the service must validate again against the configured cloud before use.
6. When several Azure service endpoint families are intentionally accepted, try only a small explicit service-type set in the configured cloud. Catch only the expected validation exceptions and fail closed when none match.
7. Validate every endpoint-producing branch. A safe default branch does not protect an alternate branch.
8. Additional service-specific checks may restrict paths, ports, resource-name structure, or bare-host syntax, but they do not replace validation of the completed endpoint. Document why each bespoke check is necessary as described below.
9. When combining a trusted base URI with user input, do not pass the input directly to base-plus-relative URI resolution. Prove it is relative, reject `//`, `\\`, schemes, rooted paths, fragments, and other authority-changing forms as appropriate, then add escaped path segments or query parameters with a purpose-built API.
10. Validate the final absolute URL after combining its components so alternate-authority behavior cannot bypass the allow-list.
11. Constructing a `Uri` solely to canonicalize and then validating its `AbsoluteUri` is acceptable. Do not perform credential acquisition, client creation, DNS access, or a network call before validation.
12. Do not configure or bypass SSRF protections from tool code. Namespace-aware validators receive the namespace only so the server's explicit emergency override can be applied.

#### Document bespoke validation-boundary logic

Code immediately before or after an `EndpointValidator` invocation is security-sensitive when it transforms,
projects, or further restricts untrusted input. Add concise comments that preserve the reasoning for maintainers;
do not merely restate what an expression does. This requirement applies to every `EndpointValidator` API.

1. At a non-HTTP network boundary, explain the downstream representation and how it differs from the URL
   representation accepted by `EndpointValidator`. For example, document when a raw database hostname is
   temporarily projected as `https://{host}` only for endpoint authorization and is then passed without that
   scheme to a connection-string builder.
2. For every normalization heuristic, explain both the signal and the security reason for the branch. If the
   presence of a dot distinguishes a short resource name from a fully qualified host, state why preserving a
   dotted value ensures validation evaluates the caller-supplied authority instead of concealing it by appending
   a trusted suffix.
3. For every pre-validation guard, identify the gap it closes between what `EndpointValidator` parses and what
   the downstream client consumes. Examples include ensuring a raw host cannot contain user-info, a port, path,
   query, fragment, scheme, or multi-host syntax that would be interpreted differently from the parsed URI host.
4. When constructing a synthetic HTTPS URI solely to call `ValidateAzureServiceEndpoint`, explicitly state that
   the scheme is for validation only and is not the value returned to or consumed by the downstream non-HTTP API.
   Also state why the shared validator is reused instead of duplicating its scheme, cloud, allow-list, bypass, or
   error behavior locally.
5. For every post-validation check, explain the narrower service invariant that the shared validator intentionally
   does not enforce. For example, a suffix allow-list may authorize its root domain while a resource-specific
   endpoint requires an additional server label.
6. Add or update XML `<exception>` documentation on extracted validation helpers so callers can distinguish
   malformed input, unsupported configuration, endpoint authorization failures, and other deliberately separate
   failure modes.
7. Treat these comments as part of the security implementation. Authors who change the normalization, validation
   projection, downstream representation, or extra invariants must update the associated comments and exception
   documentation in the same change.
8. If DNS suffixes or endpoint domains must exist in more than one location, add reciprocal comments at every
   copy that identify the counterpart file and, when practical, its symbol. Explain why each copy exists, state
   that the values must remain synchronized, and require authors changing either copy to update the other copy
   and its tests in the same change. Prefer one shared source when it does not introduce an inappropriate
   dependency, but do not silently duplicate security-sensitive domains.

### 4. Add or correct Azure allow-lists

When `serviceType` has no suitable entry:

1. Establish the official data-plane hostname suffixes for Azure Public, China, and US Government clouds from authoritative Microsoft documentation, Azure SDK constants, or resource-provider behavior already represented in the repository.
2. Never guess a sovereign-cloud suffix, copy the public suffix as a placeholder, or use an overly broad suffix such as `.azure.com`.
3. Always set `AllowedSuffixManager.Germany` to an empty collection for every service key added or directly modified by the current skill invocation. Do not add, research, infer, or preserve a Germany suffix for that key.
4. Do not retroactively clear Germany allow-lists for unrelated service keys. A service key may be changed only when it is directly required by one of the tool projects in the current invocation; do not perform side quests in other service keys or tool projects.
5. If the directly modified service key previously had a non-empty Germany collection, replace it with an empty collection and remove the Germany-related unit-test cases for that service key.
6. Never add a Germany acceptance, rejection, cross-cloud, or other Germany-specific unit test. Germany references are scheduled for removal, so new coverage would create unnecessary cleanup work.
7. If the service is unavailable in Public, China, or US Government, keep that cloud's allow-list empty and test rejection rather than falling back to a different cloud's suffix.
8. Add the narrowest suffixes that cover legitimate service endpoints. Include multiple suffixes only when the service genuinely has multiple endpoint families.
9. Read the `AllowedSuffixManager.UseLegacyCheck` documentation before choosing its value. Prefer the AntiSSRF path for new suffix-style allow-lists; retain legacy behavior only when its exact-host distinction is required or existing compatibility must be preserved.
10. Keep the central dictionary organized consistently with surrounding entries. When a tool also contains a copy of a service's DNS suffixes for endpoint construction or another distinct purpose, add reciprocal comments in the allow-list and tool code that identify one another and require synchronized updates.
11. Add central `EndpointValidatorTests` coverage for every new or changed entry:
   - valid public-cloud endpoint
   - valid endpoint for each supported China and US Government cloud
   - hostile suffix confusion such as `valid.suffix.evil.example`
   - non-HTTPS scheme
   - cross-cloud endpoint rejection
   - root-host versus subdomain behavior when that distinction matters

If authoritative domain evidence is unavailable or contradictory, stop and ask the user rather than broadening the allow-list.

### 5. Use the other validators only for their intended cases

Known external service:

```csharp
EndpointValidator.ValidateExternalUrl(
    url: url,
    allowedHosts: ["api.example.com"]);
```

Use exact hosts and HTTPS. Do not pass broad parent domains when only one host is needed.

Arbitrary public target:

```csharp
EndpointValidator.ValidatePublicTargetUrl(
    url: targetUrl,
    logger: logger,
    executingToolNamespaceName: "toolnamespace");
```

Use this only when contacting user-selected public hosts is the feature. Preserve its DNS and private/reserved-address checks; do not add success-shaped fallbacks when validation or DNS resolution fails.

### 6. Preserve error handling and architecture

- Keep commands transport-agnostic, stateless, and thread-safe.
- Reuse `EndpointValidator`; do not create local domain-matching implementations.
- Make extracted validation helpers static when they do not need instance state.
- Allow validation exceptions to reach the established command error path. Command catch blocks must continue to call `HandleException(context, ex)`.
- If translating a `SecurityException` into an argument/validation error, retain it as the inner exception and avoid echoing an unbounded or sensitive raw endpoint.
- Do not use broad catches except at an existing command boundary. A loop over explicit service types may catch only `SecurityException` and `ArgumentException`.
- Do not add permissive fallback URLs, silently select a cloud, or continue after validation fails.
- Add `using Microsoft.Mcp.Core.Helpers;` and other imports only where needed. Reuse existing project references; ask before modifying project files or public contracts.

### 7. Follow skill-local C# typing style

- In C# code written or rewritten by this skill, use an explicit local variable type when the initializer does not clearly name the constructed type as `new SomeTypeName(...)`.
- `var` is acceptable for direct, visibly typed object creation such as `var client = new ServiceClient(...)`.
- Use explicit types for method and property results, awaited calls, literals, interpolated strings, LINQ expressions, conditional and switch expressions, collection expressions, and target-typed construction. For example, write `string endpoint = BuildEndpoint();`, `ArmEnvironment environment = AzureService.CloudConfiguration.ArmEnvironment;`, and `string[] suffixes = [];`.
- Do not modify `.editorconfig`, other settings, or untouched existing code to enforce this preference. Apply it only to C# lines added or materially rewritten by the current skill invocation.

## Required Tests

Test the production validation boundary in every scoped tool that changes. Calling `EndpointValidator` directly from a tool test does not by itself prove the service invokes it.

Prefer a focused helper or service test that proves:

1. A valid endpoint reaches the expected canonical URI.
2. A valid endpoint for each supported configured Public, China, and US Government cloud is accepted where the tool supports that cloud. Do not add Germany test cases.
3. An unrelated host is rejected.
4. An allowed suffix followed by an attacker domain is rejected.
5. User-info, fragment, path, port, or scheme tricks relevant to the construction shape cannot change the authorized host.
6. HTTP is rejected for Azure and known-external endpoints.
7. A valid endpoint from another Azure cloud is rejected in the configured cloud.
8. Rejection occurs before credential acquisition, client construction, or network access when that ordering is observable.
9. Base-plus-relative construction rejects absolute URLs, `//host` network paths, backslash variants, and rooted paths when those forms could replace or escape the intended base.

Use hostile cases such as these when applicable to the construction shape:

```text
evil.example
resource.allowed.example.evil.example
resource.allowed.example@evil.example
evil.example#resource.allowed.example
http://resource.allowed.example
```

Do not force irrelevant payloads into every test. Cover the actual way each value is combined into a URL.

## Validation

1. Review the complete scoped diff and repeat the input-to-sink inventory to catch missed branches.
2. Format only changed C# files using the repository's existing tooling.
3. Run the smallest targeted tests that exercise the changed validators and call sites.
4. Run `dotnet build` for every changed tool project. If core validation changed, build and test the affected core project plus each consuming tool project.
5. Run `./eng/scripts/Build-Local.ps1 -VerifyNpx` when PowerShell, C# project files, or npm package files changed.
6. Follow repository guidance for changelog entries and spelling checks. Do not change command documentation unless the command contract or documented behavior changed.
7. Inspect `git status` again and ensure only intended files changed.

## Completion Report

State:

- directories audited
- input-to-URL flows secured
- flows intentionally exempted and why
- allow-list keys added or changed
- targeted tests and builds completed
- any unresolved flow or domain evidence

Do not claim repository-wide coverage when only the supplied directories were audited.
