<!-- BEGIN MICROSOFT SECURITY.MD V0.0.9 BLOCK -->

## Security

Microsoft takes the security of our software products and services seriously, which includes all source code repositories managed through [our GitHub organizations](https://opensource.microsoft.com/), which include [Microsoft](https://github.com/Microsoft), [Azure](https://github.com/Azure), [DotNet](https://github.com/dotnet), and [AspNet](https://github.com/aspnet).

If you believe you have found a security vulnerability in any Microsoft-owned repository that meets [Microsoft's definition of a security vulnerability](https://aka.ms/security.md/definition), please report it to us as described in [Reporting Security Issues](#reporting-security-issues).

## Policy

Microsoft follows the principle of [Coordinated Vulnerability Disclosure](https://aka.ms/security.md/cvd).

## Reporting Security Issues

> [!IMPORTANT]
> **Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them to the Microsoft Security Response Center (MSRC) at [https://msrc.microsoft.com/report/vulnerability/new](https://msrc.microsoft.com/report/vulnerability/new).

If you are unable to sign in, follow the alternative submission instructions on that page.

You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Additional information can be found at [https://www.microsoft.com/msrc](https://www.microsoft.com/msrc).

### Evidence Required for a Report

Please include the following information to help us understand the nature and scope of the possible issue. This information will help us triage your report more quickly.

* Type of issue (e.g. authentication bypass, command injection, server-side request forgery, etc.)

* The affected MCP server, tool, or command and full paths of source files related to the issue

* The specific execution mode (`stdio`, `http`, or CLI mode) and deployment environment in which the issue occurs

* The exact revision of the affected source code in [`microsoft/mcp`](https://github.com/microsoft/mcp), with a direct URL if available. Eligible revisions are the current HEAD of `main` or an active release branch. Reports against archived repositories (such as `Azure/azure-mcp`) or code already removed from all eligible branches are not eligible.

* The attacker's access and the applicable [trust boundaries](#trust-boundaries) crossed, including how input controlled by an untrusted party reaches the affected operation in its intended environment

* The full command-line invocation and arguments used to start or run the server or CLI, along with any special configuration required to reproduce the issue (including environment variables, authentication settings, or caller customizations)

* Step-by-step instructions and a reproducible demonstration of the issue

* Proof-of-concept or exploit code (if possible)

* Evidence of impact, including how an attacker might exploit the issue

* (Optional) An example of another product or library (especially another Microsoft product or Azure SDK library) that avoids the issue in a comparable scenario

Verify claims against the current code and authoritative documentation, and clearly highlight what you could not verify. Reports must reproduce on an eligible revision, as findings limited to other revisions are not vulnerabilities or defense-in-depth enhancements. A hypothetical deployment or a caller deliberately supplying unsafe configuration is not sufficient evidence of a vulnerability.

### Preferred Languages

We prefer all communications to be in English.

## Trust Boundaries

The servers and tools in this repository (including the Azure MCP Server and Microsoft Fabric MCP Server) implement the [Model Context Protocol (MCP)](https://modelcontextprotocol.io) and companion CLI commands (such as `azmcp` and `fabmcp`). They act as a bridge between MCP hosts (such as IDEs, AI assistants, and agent runtimes) or CLI callers and downstream Azure and Microsoft services, using official Azure SDK client libraries, service REST APIs, and local developer tools.

The MCP server is not a boundary for enforcing Azure service security checks, Azure RBAC rules, database permissions, or downstream payload sanitization. Likewise, when running locally, the server relies on the isolation and access controls of the host operating system and the MCP host application. Security reports must distinguish a failure in an MCP server's own responsibilities from a failure of the host environment, MCP client, downstream Azure service, pipeline sandbox, or caller configuration on which it relies.

### Scope of Reports

* An in-scope vulnerability report must demonstrate that an untrusted party can violate a security responsibility owned by an MCP server, tool, or its development infrastructure in its intended environment. The sections below identify the protections the MCP server owns and those owned by the host environment, MCP clients, callers, and Azure services.

* Do not report vulnerabilities or defense-in-depth enhancements that require first compromising the host machine, the local user account, the MCP host application, the network, or an Azure service. These preconditions already cross the relevant trust boundary.

* Do not report issues that involve manipulating requests or parameters sent through the MCP server when the same operation or request manipulation can be performed directly against the downstream service or local environment without the MCP server using the same credentials or access.

* Do not report findings that require the authentication or authorization privileges of repository maintainers, Azure subscription administrators, or trusted partners. Additional restrictions on actions available only to these trusted parties are, at most, low-risk and low-value defense-in-depth enhancements rather than vulnerabilities.

### Local Execution (`stdio` and CLI Mode) and the Host Environment

* By default, the MCP server runs locally over standard input/output (`--transport stdio`) or as a local command-line tool (such as `azmcp` or `fabmcp`) within the developer's workstation, container, or CI pipeline environment. It is not a multi-tenant local service or a privilege boundary between processes running on the same machine.

* In local execution, the server runs under the caller's operating system account and uses the credentials, environment variables, token caches, and file system permissions available in that environment (resolved through `Azure.Identity` credential chains such as Azure CLI, Azure PowerShell, Azure Developer CLI, Visual Studio, VS Code, environment variables, or the OS authentication broker).

* The server cannot guard against inspection or tampering by other processes, users, or monitoring tools on the local host. Findings that require local machine access—such as reading local process listings (`argv` or process command-line arguments), inspecting child-process state, reading or sharing local environment variables and same-user token caches, planting binaries in the current working directory or `PATH`, or modifying local binaries and configuration files—already cross the host trust boundary and are not MCP server vulnerabilities.

* Certain tools invoke or interact with local developer CLIs (such as `az`, `azd`, or `azqr`), emitters, or extensions installed in the host environment. Callers are responsible for ensuring that tools on their `PATH`, their working directory, and extensions they configure are trusted. Executing intended CLI commands or loading extensions in the local environment does not cross a security boundary.

### Remote Execution (`http` Mode) and Hosted Deployments

* When hosted remotely using HTTP transport (`--transport http`), the MCP server relies on two authentication layers:
  1. **Inbound authentication:** Requests to the server are protected by Microsoft Entra ID bearer token validation (`Microsoft.Identity.Web`), verifying the token signature, issuer, audience, and required scope (`Mcp.Tools.ReadWrite`) or app role (`Mcp.Tools.ReadWrite.All`). The inbound authentication model defines a single read-write access level; it does not define separate read-only vs. read-write caller roles.
  2. **Outbound authentication:** The server authenticates to downstream Azure services using either the configured On-Behalf-Of flow (`--outgoing-auth-strategy UseOnBehalfOf`, default) or the hosting environment's identity (`--outgoing-auth-strategy UseHostingEnvironmentIdentity`).

* Hosting administrators are responsible for selecting the outbound authentication strategy appropriate for their tenancy and auditing requirements. In `UseHostingEnvironmentIdentity` mode, all authorized HTTP callers share the server's Managed Identity or service principal permissions by design; administrators must scope that identity's Azure RBAC roles to the minimum required and restrict inbound Entra ID access accordingly.

* Hosting administrators are responsible for configuring TLS termination, network isolation, Entra ID application registrations, and least-privilege Azure RBAC assignments for hosted deployments, as described in the [Authentication documentation](https://github.com/microsoft/mcp/blob/main/docs/Authentication.md). Reference deployment templates and sample repositories (such as those linked from [`servers/Azure.Mcp.Server/azd-templates`](https://github.com/microsoft/mcp/tree/main/servers/Azure.Mcp.Server/azd-templates)) are starting points for customer self-hosting rather than managed Microsoft services; administrators remain responsible for reviewing and adapting their security configuration for production use.

* Running HTTP mode (`--transport http`) with incoming authentication disabled (`--dangerously-disable-http-incoming-auth`)—including on a local machine or shared network—is strongly discouraged outside isolated development and testing scenarios, as it exposes the server and its configured credentials to any caller able to reach the HTTP endpoint.

* When the server acts as a client to external MCP servers configured in a registry, it enforces HTTPS endpoints and non-empty OAuth scopes for OAuth-protected entries. Downstream resource servers remain responsible for validating the issuer, audience, and scopes of tokens presented to them.

### AI Agents, MCP Hosts, and Operational Guardrails (`--read-only`, Elicitation, and Query Builders)

* Invoking an operation through an AI agent or MCP host does not create a separate identity or authorization boundary. The user, MCP host, and deployment administrator control which MCP servers and tools are enabled (for example, via `--namespace`, `--tool`, `--mode`, `--read-only`, and `--disable-proxy-tools`), which credentials and Azure RBAC permissions are granted, and whether human approval or elicitation is required.

* MCP tools are designed to inspect, create, modify, and delete Azure resources and query data stores (such as Azure Resource Graph, Azure Monitor Log Analytics, Azure Data Explorer/Kusto, Cosmos DB, PostgreSQL, MySQL, and SQL) according to the authenticated identity's permissions. An authorized invocation of an intended tool capability is not, by itself, a vulnerability or privilege escalation.

* **Tool annotations, `--read-only`, elicitation, and query helpers are operational guardrails, not authorization boundaries:** In the MCP specification, tool annotations (such as read-only, destructive, or idempotent hints) and internal `CommandMetadata` (`ReadOnly`, `Destructive`, `Secret`) are advisory hints for UX and tool selection rather than security guarantees. Options such as `--read-only` (which filters the exposed tool list using that `ReadOnly` metadata), confirmation/elicitation prompts, and lightweight query-shape checks (such as `SqlQueryValidator` or KQL query builders) are operational guardrails to reduce accidental actions—not security authorization boundaries or substitutes for least-privilege access control. The authoritative authorization boundary is always the permissions of the underlying Azure RBAC identity or database user. Mislabeled tool metadata (for example, a mutating tool marked read-only or not marked destructive), proxy paths that do not trigger elicitation, or SQL/KQL expressions that bypass client-side query helpers without exceeding the identity's backing permissions are treated as functional bugs or defense-in-depth improvements rather than security vulnerabilities.

* Reports involving AI-generated tool calls or prompt injection must demonstrate a violation of a security responsibility owned by the MCP server across a genuine trust boundary in its intended environment. Influencing a model's response or demonstrating that an agent can invoke a tool or query data that the configured identity already has permission to access is not sufficient.

### Service Requests, Responses, and Underlying Azure SDK Clients

* MCP tools communicate with Azure and Microsoft services through official Azure SDK client libraries and service REST APIs, and inherit the [Azure SDK client trust boundaries](https://github.com/Azure/azure-sdk-for-net/blob/main/SECURITY.md#trust-boundaries) where SDK clients are used.

* The server and its underlying clients treat credentials (keys, SAS tokens, and Microsoft Entra ID tokens) as opaque values and do not determine their validity or evaluate the permissions they grant. Downstream Azure services own authentication validation, authorization, and request payload validation.

* Azure service responses are implicitly trusted by design, including pagination `nextLink` / continuation values, long-running operation polling URLs (`Operation-Location`, `Azure-AsyncOperation`, `Location`), `Retry-After` headers, error payloads, and service metadata used for protocol follow-up requests. If an issue involves a compromised or attacker-controlled Azure service response, that issue originates in the downstream service and is outside the MCP server's trust boundary (and is routed to the owning Azure service team). Any outbound endpoint or SSRF checks performed by the MCP server on service responses are defense-in-depth measures rather than a security boundary. By contrast, if untrusted data stored within a legitimate Azure resource triggers an unsafe parsing, deserialization, or execution flaw within the MCP server itself, that flaw remains in scope.

* Callers and users control the resource identifiers, endpoints, cluster URIs, and parameters passed to tools and are responsible for validating that they correspond to the intended Azure resource. Built-in endpoint and SSRF checks in the MCP server (such as blocking loopback or private network targets or validating well-known service hostnames) are defense-in-depth measures, not a security boundary. A demonstration in which a caller supplies an attacker-owned endpoint or untrusted parameter—or identifies a tool where additional endpoint validation could be added—is at most a defense-in-depth enhancement rather than a vulnerability.

* It is the responsibility of callers, users, and MCP hosts to inspect, validate, and sanitize inputs and returned data before using them in ways that may impact their environment. Endpoint, URL, and SSRF validations performed by the MCP server are provided as defense in depth rather than a security boundary and do not shift validation responsibility away from the caller.

### Safe Defaults and Opt-In Dangerous Flags

* The MCP server provides safe defaults and defense-in-depth guardrails, including requiring inbound authentication in HTTP mode, prompting for confirmation (elicitation) on high-risk secret-retrieval operations where supported, applying defense-in-depth SSRF protections, and bounding retry policies.

* The server exposes explicit opt-in flags prefixed with `--dangerously-*` for specialized troubleshooting, testing, or custom hosting scenarios.

* Unsafe behavior that is the intended effect of enabling a `--dangerously-*` flag, disabling certificate validation, or overriding default security controls is a consequence of explicit caller configuration and is not reportable as an MCP server vulnerability. Unintended security side effects outside the specific protection disabled by that flag remain in scope.

### Diagnostic Logging and Telemetry

* The MCP server and its underlying client libraries redact and sanitize well-known sensitive headers and URL query parameters by default in standard diagnostic logging and telemetry.

* The server supports two distinct telemetry and diagnostic streams, as documented in [Telemetry Configuration](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/README.md#telemetry-configuration):
  1. **Microsoft telemetry:** By default in release builds, the server sends product usage and operational telemetry to Microsoft to help improve and secure the MCP servers. Users can opt out of Microsoft telemetry by setting `AZURE_MCP_COLLECT_TELEMETRY_MICROSOFT=false` or `AZURE_MCP_COLLECT_TELEMETRY=false`.
  2. **Customer-controlled diagnostic logs and telemetry sinks:** Customer-configured sinks—such as console/`stderr` output, support log files, a caller-configured Application Insights resource (`APPLICATIONINSIGHTS_CONNECTION_STRING`), or custom OpenTelemetry exporters—may include more detailed diagnostic data (particularly in HTTP mode) to support customer troubleshooting and auditing. These customer-only sinks operate within the customer's or host environment's trust boundary, and callers and administrators are responsible for protecting those destinations, file permissions, and access controls. If a deployment makes its customer-controlled logs or telemetry accessible to untrusted parties, that exposure is a configuration issue, not an MCP server vulnerability.

### Development Tools, Test Infrastructure, and Automation

* Repository scripts, build tooling, test runners, pipeline configurations, and agentic tooling (such as agent instructions, skills, prompts, and workflow configurations) are intended for MCP server development and automation. Development scripts and agentic workflows are intentionally permissive to reduce friction during development; more restrictive input handling in development tooling is generally a low-risk, low-value defense-in-depth enhancement rather than a vulnerability when confined to the local operator context or the intended CI sandbox.

* Tools such as the Azure SDK Test Proxy, recorded test fixtures, and evaluation harnesses (such as Vally) are used by developers and CI pipelines for testing. They are not production services. Shared localhost development certificates and sanitized test recordings are public testing assets, not production credentials.

* Pull request validation and test runs in the ephemeral Azure DevOps and GitHub Actions sandboxes have no access to production secrets, internal Microsoft resources, or the corporate network. The CI sandbox is the primary trust boundary for pull request builds. Findings confined to that sandbox (such as script argument handling during a PR build where contributor code already executes) are defense-in-depth enhancements rather than vulnerabilities, whereas flaws that escape the sandbox, expose production or release credentials, or impact external resources remain in scope.

## Automated Security Scanning

AI-based scanners and other automated security tools must apply the [trust boundaries](#trust-boundaries) above before classifying or reporting a finding. Automated findings must meet the same [evidence requirements](#evidence-required-for-a-report) as other security reports.

Reporters are responsible for reviewing and validating generated findings before submitting them. A suspicious code pattern or an unverified model response is not evidence of a vulnerability.

* Run the proposed reproduction against an eligible revision (`main` or an active release branch) of [`microsoft/mcp`](https://github.com/microsoft/mcp) and confirm that its observed result supports the claimed impact.

* Check the generated analysis against the affected code and the intended execution mode (`stdio`, `http`, or CLI). Confirm the attacker's access and the path across the identified trust boundary.

* Remove unsupported claims and state any remaining uncertainty. Do not present assumptions or model-generated conclusions as verified facts.

* Submit a concise report using the shared [evidence requirements](#evidence-required-for-a-report), rather than forwarding unverified scanner output.

<!-- END MICROSOFT SECURITY.MD BLOCK -->
