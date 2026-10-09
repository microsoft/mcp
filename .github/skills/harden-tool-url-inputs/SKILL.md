---
name: harden-tool-url-inputs
description: 'Audit and harden one or more tool directories against URL hijacking and SSRF by tracing MCP inputs to URL construction and using IAzureService or injectable IEndpointValidator with cloud-specific Azure endpoint allow-lists. Use when: validate URL inputs, secure endpoints, prevent URL hijacking, SSRF hardening, audit tool directories, apply EndpointValidator.'
argument-hint: 'Provide one or more repo-relative directories under tools/ (for example: tools/Azure.Mcp.Tools.Acr tools/Azure.Mcp.Tools.Compute)'
user-invocable: true
disable-model-invocation: false
---

# Harden Tool URL Inputs

Audit and update every requested directory so MCP-influenced values cannot hijack a URL, URI, host, or client endpoint. Use `IAzureService` forwarding methods or the host's injectable `IEndpointValidator` at the final URL boundary, with the configured Azure cloud and the narrowest appropriate allow-list.

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

- `core/Microsoft.Mcp.Core/src/Helpers/IEndpointValidator.cs`
- `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.cs`
- `core/Microsoft.Mcp.Core/src/Helpers/EndpointValidator.AllowLists.cs`
- `core/Azure.Mcp.Core/src/Services/Azure/IAzureService.cs`
- `core/Microsoft.Mcp.Core/tests/Microsoft.Mcp.Core.Tests/Helpers/EndpointValidatorTests.cs`
- `core/Azure.Mcp.Core/tests/Azure.Mcp.Tests/Helpers/AzureServiceTestHelpers.cs`

When wiring dependencies or testing namespace overrides, also inspect `SsrfProtectionPolicy.cs`,
`EndpointValidationServiceCollectionExtensions.cs`, `ICommandContextAccessor.cs`, and
`EndpointValidationRegistrationTests.cs` in their corresponding core projects. Read the
executing-command-context guidance in `CONTRIBUTING.md` and transport boundaries in
`docs/design/HttpClientFactory.md`.

Use current implementations as the API and pattern source of truth. Do not copy obsolete static
policy-dependent calls or explicit namespace arguments from historical examples. Representative
current cases include:

- ACR: validate the exact `Uri` passed to the data-plane SDK through a small static helper that receives `IAzureService` for focused tests.
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
5. Identify the existing `IAzureService` dependency or, where unavailable, the `IEndpointValidator` dependency and its DI registration. Do not create a separate validator or policy at the tool boundary.

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
| Azure data-plane endpoint | `IAzureService.ValidateAzureServiceEndpoint` or injected `IEndpointValidator.ValidateAzureServiceEndpoint` |
| Known non-Azure service with an exact host set | Static `EndpointValidator.ValidateExternalUrl` |
| Deliberately arbitrary public HTTP(S) target | `IAzureService.ValidatePublicTargetUrl` or injected `IEndpointValidator.ValidatePublicTargetUrl` |
| Input-derived path or query on an allowed base | Reject absolute/network-path replacement, construct or escape the component with an appropriate API, then validate the final absolute URL |
| Typed Azure control-plane operation | Use the typed ARM SDK and `ResourceIdentifier` through shared ARM client creation; its pipeline validates each final request URI rather than requiring a redundant tool-level check |
| Compile-time fixed URL with no influenced component | Document as not input-derived; no runtime validation needed |

Do not classify an Azure endpoint as an arbitrary public target merely to avoid defining a service allow-list.

Shared ARM clients created through `AzureHelper.CreateArmClientAsync` call
`IAzureService.ConfigureArmClientOptions` and retain a configured-cloud `arm` validation policy
before transport. It validates every attempt, including retries, paging, and long-running-operation
polling, even without an active command context. Do not assume raw ARM HTTP calls or independently
constructed SDK clients have this policy. Trace those paths and validate their final URLs against
the configured-cloud `arm` allow-list before sending, including subsequent request URLs.

### 3. Implement Azure service validation

For an Azure data-plane endpoint, validate the exact absolute URL before the SDK client, request, or other network-capable object can use it:

```csharp
Uri uri = new Uri($"https://{resource}.{serviceDomain}");

AzureService.ValidateAzureServiceEndpoint(
    endpoint: uri.AbsoluteUri,
    serviceType: "service-key");

var client = new ServiceClient(uri, credential, options);
```

Apply these rules:

1. Use named arguments to distinguish the endpoint and service allow-list key. Direct `IEndpointValidator` calls also supply the Azure cloud.
2. `serviceType` is the exact key for the endpoint domain in `EndpointValidator.AllowLists.cs`.
3. The executing tool namespace is resolved by the validator through `ICommandContextAccessor.CurrentContext.ToolNamespaceName`, not supplied by the caller. It is the original registered `IAreaSetup.Name`, even in consolidated or single-tool mode, and is independent of `serviceType`; for example, `compute` may consume `storage-blob`, and `foundryextensions` may consume `foundry` or `azure-openai`. Never infer it from request arguments, an endpoint, a routing name, or telemetry.
4. `IAzureService.ValidateAzureServiceEndpoint` always uses its own `CloudConfiguration.ArmEnvironment`; callers supply only the endpoint and service key. Direct `IEndpointValidator` calls must pass the configured cloud at an authoritative service boundary. Do not hardcode public cloud when configured cloud context is available.
5. If option validation cannot access the configured cloud, it may accept endpoints valid in any explicitly supported cloud for early feedback, but the service must validate again against the configured cloud before use.
6. When several Azure service endpoint families are intentionally accepted, try only a small explicit service-type set in the configured cloud. Catch only the expected validation exceptions and fail closed when none match.
7. Validate every endpoint-producing branch. A safe default branch does not protect an alternate branch.
8. Additional service-specific checks may restrict paths, ports, resource-name structure, or bare-host syntax, but they do not replace validation of the completed endpoint. Document why each bespoke check is necessary as described below.
9. When combining a trusted base URI with user input, do not pass the input directly to base-plus-relative URI resolution. Prove it is relative, reject `//`, `\\`, schemes, rooted paths, fragments, and other authority-changing forms as appropriate, then add escaped path segments or query parameters with a purpose-built API.
10. Validate the final absolute URL after combining its components so alternate-authority behavior cannot bypass the allow-list.
11. Constructing a `Uri` solely to canonicalize and then validating its `AbsoluteUri` is acceptable. Do not perform credential acquisition, client creation, DNS access, or a network call before validation.
12. Do not configure or bypass SSRF protections, open command scopes, or assign namespaces from tool code. Command loaders establish the scope around the complete awaited command execution, including option validation. Discovery, learn/sampling callbacks, direct CLI execution, and external proxy forwarding do not populate the accessor; a real command selected through sampling receives its own scope.

#### Dependency and policy ownership

- Services derived from `BaseAzureService` should use its existing `AzureService` property. Other code with an `IAzureService` dependency should use that dependency's forwarding methods. The Azure-service facade obtains its cloud internally; do not add a cloud argument to it or to helpers that merely forward validation.
- Commands performing early option validation and services without Azure-service access should inject `IEndpointValidator` through their primary constructor. Static validation helpers must receive `IAzureService` or `IEndpointValidator` explicitly rather than resolving services or constructing a validator themselves.
- When a toolset directly injects `IEndpointValidator`, ensure its `ConfigureServices` calls `services.AddEndpointValidation()` using `Microsoft.Mcp.Core.Extensions`. That registration supplies logging, the shared singleton accessor, and a default validator and policy without replacing existing host registrations.
- One immutable `SsrfProtectionPolicy` belongs to each host and is shared by endpoint validation, HTTP transports, and startup telemetry. Only host composition configures emergency namespace overrides. Toolsets must not register an overriding policy, initialize process-global state, or add mutation/reset APIs.
- `EndpointValidator` reads the accessor on every validation call. Cached clients and pipeline policies retain the validator or accessor, never an invocation's context, namespace, or bypass decision.
- `SsrfProtectionPolicy.AreSsrfProtectionsEnabled` is affirmative: `true` means protection remains enabled. Missing context and null, empty, or whitespace namespaces remain protected, even with `SsrfProtectionPolicy.AllNamespaces`. Overrides match case-insensitively without trimming; `" ALL "` is not the all-namespace marker.
- `EndpointValidator` owns the injected `ILogger<EndpointValidator>` for diagnostics. Do not pass a logger or an executing namespace as a validation argument. `ValidateExternalUrl` and `IsPrivateOrReservedIP` remain static, policy-independent helpers, not interface methods.
- Shared HTTP transports separately enforce external-only HTTPS and DNS-to-IP restrictions and select isolated protected/unprotected pools on each send. Those checks do not replace endpoint allow-lists or public-target validation. The factory selects one explicit proxy in `ALL_PROXY`, `HTTPS_PROXY`, then `HTTP_PROXY` order and applies it to the whole handler; explicit HTTP proxies and DEBUG recording proxies therefore override transport protection for every request, including `NO_PROXY` destinations. Only when no explicit proxy is selected does `.NET`'s `HttpClient.DefaultProxy` supply destination-specific environment, operating-system, PAC, and bypass routing, overriding transport protection only for destinations it actually proxies. Recording and explicitly unprotected clients retain normal default-proxy behavior when no explicit proxy is assigned. Proxy routing never disables endpoint validation, including public-target DNS/IP checks of the original URL. Validate the original endpoint before recording rewrites it; never add proxy or arbitrary playback hosts to an allow-list to make tests pass.
- `NoSsrfClientName` is reserved for trusted infrastructure. Never select it from tool input or use it to work around a denied endpoint.

When injecting the validator directly, the corresponding call shape is:

```csharp
endpointValidator.ValidateAzureServiceEndpoint(
    endpoint: uri.AbsoluteUri,
    serviceType: "service-key",
    armEnvironment: armEnvironment);
```

Here `endpointValidator` is the injected `IEndpointValidator` and `armEnvironment` is the
configured cloud supplied by the caller. Unlike the Azure-service facade, the lower-level validator
retains this argument for explicit-cloud checks. An early any-cloud check remains only preliminary feedback.

#### Document bespoke validation-boundary logic

Code immediately before or after a validation invocation is security-sensitive when it transforms,
projects, or further restricts untrusted input. Add concise comments that preserve the reasoning for maintainers;
do not merely restate what an expression does. This requirement applies to Azure-service forwarding,
injected validator calls, and static `EndpointValidator` helpers.

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
   failure modes. Prefer multiline `<summary>` and `<returns>` sections, `<see cref="..."/>` for API references,
   `<paramref name="..."/>` for parameters, and `<see langword="null"/>` or other keyword links.
   Refer to `<see cref="SsrfProtectionPolicy.AllNamespaces"/>` instead of a magic all-namespace string in XML documentation.
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
AzureService.ValidatePublicTargetUrl(url: targetUrl);
```

Without Azure-service access, call the injected `IEndpointValidator` with
`endpointValidator.ValidatePublicTargetUrl(url: targetUrl)`. Neither form takes a logger or namespace.

Use this only when contacting user-selected public hosts is the feature. Preserve its DNS and private/reserved-address checks; do not add success-shaped fallbacks when validation or DNS resolution fails. Its preflight DNS check does not replace send-time transport protection against DNS changes.

### 6. Preserve error handling and architecture

- Keep commands transport-agnostic, stateless, and thread-safe.
- Reuse the host's `IEndpointValidator` through the established dependency; do not create local domain-matching implementations or a production validator with its own policy/accessor.
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

Test the production validation boundary in every scoped tool that changes. Calling a validator directly from a tool test does not by itself prove the service invokes it. Security tests must run as ordinary `[Fact]` or `[Theory]` tests; do not use `Explicit = true`, disable parallel execution, or require a special isolated process to test namespace overrides.

Use a real `EndpointValidator` with an independent immutable policy and `CommandContextAccessor`,
or resolve `IEndpointValidator` from an independently built test provider. For an `IAzureService`
substitute, prefer `AzureServiceTestHelpers.CreateAzureService` so its validation methods delegate
to a real validator. A no-op substitute is not evidence of endpoint rejection.

Default strict tests need no active command context. To exercise an override, share the same
accessor with the validator or Azure-service test helper and open a real test scope:

```csharp
var accessor = new CommandContextAccessor();
IEndpointValidator validator = new EndpointValidator(
    new SsrfProtectionPolicy(["compute"]),
    NullLogger<EndpointValidator>.Instance,
    accessor);

using (accessor.BeginScope(new CommandContext { ToolNamespaceName = "compute" }))
{
    validator.ValidateAzureServiceEndpoint(
        endpoint: "http://127.0.0.1",
        serviceType: "storage-blob",
        armEnvironment: ArmEnvironment.AzurePublicCloud);
}

Assert.Throws<SecurityException>(() => validator.ValidateAzureServiceEndpoint(
    endpoint: "http://127.0.0.1",
    serviceType: "storage-blob",
    armEnvironment: ArmEnvironment.AzurePublicCloud));
```

This example deliberately exercises the emergency override, not a valid Azure endpoint.
Use scoped service/helper tests to prove production wiring as well. Create scopes synchronously
in the tested asynchronous flow and dispose them in that flow. Only one scope may be active per
flow; use sequential scopes or independent asynchronous flows, not nested scopes. Do not create
process-wide setters, test-only reset hooks, or a separate accessor inside the validation boundary.

When namespace or cached-client behavior changes, verify selected namespaces independently of
service keys, absent and blank contexts under `SsrfProtectionPolicy.AllNamespaces`, strict behavior
after scope disposal, and concurrent providers/flows with different policies. Reuse one validator
or client across scopes to detect accidentally captured bypass decisions.

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
