# Deploy Area — Complete Analysis & Recommendations

## Root Cause: Why Deploy Tools Aren't Being Called

There are two compounding problems:

### 1. Claude models prefer answering directly over calling tools

This happens when:

- The prompt is a **question** ("How do I...") rather than an action request
- The tool returns **static templates** that Claude can produce natively from training data
- The **parameter barrier is high** (7 required params for `deploy_plan_get`)
- The description **discourages calling** ("This command does not scan...", "the caller must first...", "Before calling this tool, confirm with the user...")

### 2. The deploy tools compete with Claude's native capabilities

Four of the five tools (`plan get`, `iac rules get`, `pipeline guidance get`, `architecture diagram generate`) are template renderers returning Markdown. Claude can generate equivalent deployment plans, IaC guidelines, and pipeline configs from training data — **faster, with no parameter gathering**.

The only tool providing unique value Claude can't replicate is **`deploy_app_logs_get`** (live Azure Log Analytics queries).

---

## Area-Level

### Current area description (DeploySetup.cs)

> "Deploy operations – Commands for deploying applications to Azure. Provides sub-commands to generate deployment plans, offer infrastructure-as-code (Bicep/Terraform) guidance, fetch application logs, generate CI/CD pipeline guidance, and produce Azure architecture diagrams based on application topology."

### Current consolidated description (consolidated-tools.json)

> "Deploy resources and applications to Azure. Retrieve application logs, access Bicep and Terraform rules, get CI/CD pipeline guidance, and generate deployment plans."

### Problems

- Passive feature list — no workflow guidance for agents
- Sounds like generic advice Claude already knows
- Consolidated tool **omits `deploy_architecture_diagram_generate`** from its `mappedToolList`
- No signal that these tools provide curated, Microsoft-official, azd-ecosystem-integrated output

### Suggestions

- Lead with unique value: these are *Microsoft's official deployment patterns* for azd, not generic advice. The templates include azd-specific commands, `.azure/` folder conventions, and tested IaC rules that differ from what Claude would generate from training data
- Add workflow sequence so agents know the calling order
- Add `deploy_architecture_diagram_generate` back into the consolidated deploy tool group
- Example:

> "Generate Microsoft-official deployment plans, architecture diagrams, IaC rules, and CI/CD pipeline guidance for Azure. These tools return curated templates aligned with Azure Developer CLI (azd) conventions and .azure/ project structure — use them instead of generating deployment guidance from scratch. Recommended workflow: analyze workspace → architecture diagram → deployment plan → IaC rules → pipeline guidance."

---

## Tool 1: `deploy_plan_get`

### Current description

> "Creates a deployment plan for deploying an application to Azure using the options provided by the caller. Use this tool when the user wants a formatted, step-by-step deployment plan (including suggested Azure resources, infrastructure as code (IaC) templates, and deployment instructions) based on a target Azure hosting service (for example, Container Apps, App Service, or AKS) and a chosen provisioning tool (such as Azure Developer CLI (azd) or Azure CLI with Bicep or Terraform). This command does not scan the workspace or automatically recommend Azure services. Instead, the caller or agent must first analyze the workspace, determine the services, frameworks, and dependencies to deploy, select the appropriate Azure hosting service, provisioning tool, IaC type, and deployment option, and then pass those chosen values into this tool to generate the deployment plan."

### Current test prompt

> "How do I create a step-by-step deployment plan for my application to Azure using azd with Bicep or Terraform IaC?"

### Problems

- **(Root cause)** Claude can answer "How do I create a deployment plan" from training. No incentive to call a tool for this
- 7 required parameters create high friction — even if Claude wants to call it, it must gather all values first
- Description leads with negatives ("does not scan"), reads as a barrier
- Test prompt is a question, not an action request
- `--workspace-folder` is required but never passed to `GetPlanTemplate()` (dead in execution, though serves as workflow contract — the agent needs it to know where to save `.azure/plan.copilotmd` after)

### Suggestions — Description (Option A: signal unique value)

Emphasize the output is an azd-ecosystem-integrated plan saved to `.azure/plan.copilotmd` with tested step-by-step commands, architecture diagrams, and IaC scaffolding that follows Microsoft's deployment conventions — not a generic howto. Replace negatives with a positive prerequisite statement.

Example:

> "Generates a Microsoft-official step-by-step deployment plan for Azure, following azd conventions. Returns a structured Markdown template to be saved to `.azure/plan.copilotmd`, including Azure resource architecture, IaC scaffolding instructions, and deployment commands specific to the chosen hosting service and provisioning tool. The caller must first analyze the workspace to determine services, hosting targets, and provisioning preferences before calling this tool."

### Suggestions — Parameters (Option B: reduce friction)

Make `provisioning-tool`, `iac-options`, `source-type`, `deploy-option` optional with smart defaults (most already have `DefaultValueFactory` in the option definitions — the issue is they're all marked `Required = true`).

**Minimum viable call:** `--workspace-folder` + `--project-name` + `--target-app-service`

### Better test prompts

| Prompt | Why it's better |
|--------|----------------|
| "Deploy this app to Azure Container Apps" | Action request, specifies target |
| "Create a deployment plan for my project to Azure" | Imperative, not a question |
| "I want to deploy to AKS — generate a plan" | Direct intent with target |
| "Help me set up an azd deployment for this project" | Mentions azd, triggers tool over native |

---

## Tool 2: `deploy_app_logs_get`

### Current description

> "Shows application logs specifically for Azure Developer CLI (azd) deployed applications from their associated Log Analytics workspace for Container Apps, App Services, and Function Apps. Designed exclusively for applications deployed via 'azd up' command and automatically discovers the correct workspace and resources based on the azd environment configuration. Use this tool to check deployment status or troubleshoot post-deployment issues."

### Current test prompt

> "Show me the log of the application deployed by azd"

### Problems

- **Bug:** `LocalRequired = false` but reads local `azure.yaml` via `File.ReadAllText()` — will crash in HTTP mode
- This is the *one tool* with genuine unique value (live Azure data) but the test prompt doesn't provide required params (`--workspace-folder`, `--azd-env-name`, `--subscription`)

### Suggestions

- **Fix** `LocalRequired = true`
- Add "Requires local workspace access to read azure.yaml" to description
- This tool should be easiest to invoke since it provides data Claude genuinely can't produce

### Better test prompts

| Prompt | Why it's better |
|--------|----------------|
| "Show me the logs for my azd app in the current workspace" | Agent fills workspace-folder from context |
| "Check the deployment logs for my azd environment 'dev'" | Specifies env name |
| "Are there any errors in my azd-deployed app?" | Diagnostic intent, triggers the tool |

---

## Tool 3: `deploy_iac_rules_get`

### Current description

> "Retrieves rules and best practices for creating Bicep and Terraform Infrastructure as Code (IaC) files to deploy Azure applications. Use this tool when the user asks for rules, guidelines, or best practices for writing Bicep scripts or Terraform templates for Azure resources. The rules cover Azure resource configuration standards, compatibility with Azure Developer CLI (azd) and Azure CLI, and general IaC quality requirements. Use when user asks: show me the rules and best practices for writing Bicep and Terraform IaC for Azure."

### Current test prompt

> "Show me the rules and best practices for writing Bicep and Terraform IaC for Azure"

### Problems

- **(Root cause)** Claude knows IaC best practices from training. "What are Bicep best practices?" gets answered natively
- Redundant "Use when user asks:" suffix is a literal copy of the test prompt
- No signal that these are *different* from what Claude already knows

### Suggestions — Description

- Remove "Use when user asks:" suffix
- Emphasize these are curated rules for **azd-compatible and Azure CLI-compatible** IaC — including naming conventions, resource configuration standards, and deployment tool-specific requirements that may differ from generic best practices
- Mention these rules cover azd/AzCli compatibility constraints that Claude's training data may not have

Example:

> "Retrieves curated IaC rules for creating Bicep or Terraform files compatible with Azure Developer CLI (azd) or Azure CLI deployments. Includes Azure resource naming standards, azd-specific configuration requirements, and deployment tool constraints that differ from generic IaC best practices. Specify the deployment tool, IaC type, and target resource types."

### Suggestions — Parameters

- Make `--resource-types` optional (already is) and `--iac-type` optional (already has fallback logic)
- **Minimum viable call:** `--deployment-tool` only

### Better test prompts

| Prompt | Why it's better |
|--------|----------------|
| "Get the IaC rules for my azd Bicep deployment" | Mentions azd, action request |
| "What are the azd-specific Terraform requirements for Container Apps?" | Specific to azd, not generic |
| "I need infrastructure rules before writing my Bicep templates" | Pre-action context |

---

## Tool 4: `deploy_pipeline_guidance_get`

### Current description

> "Generates CI/CD pipeline configuration and step-by-step guidance for deploying an application to Azure using GitHub Actions or Azure DevOps pipelines. Use this tool when the user wants to create a CI/CD pipeline, set up automated deployment workflows, or configure pipeline files to deploy their application to Azure. Supports both Azure Developer CLI (azd) and Azure CLI based deployments, and can generate pipelines that provision infrastructure and deploy application code. Before calling this tool, confirm with the user whether they prefer GitHub Actions or Azure DevOps, and whether they have existing Azure resources for their deployment environments. Use when user asks: how do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure?"

### Current test prompt

> "How do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure?"

### Problems

- **(Root cause)** Claude excels at generating GitHub Actions and Azure DevOps YAML. This is one of its strongest training domains. No incentive to call a tool
- **"Before calling this tool, confirm with the user"** literally tells the agent to stop and ask instead of calling — biggest single invocation blocker
- Inherits `SubscriptionCommand` making `--subscription` required, but the tool **never uses it** — pure template rendering
- Test prompt is a question

### Suggestions — Description

- **Remove** "Before calling this tool, confirm with the user" — this is the biggest invocation killer
- Emphasize unique value: azd-specific pipeline patterns including **OIDC auth setup, Managed Identity federation, environment separation**, and `.azure/pipeline-setup.md` output — specific operational patterns Claude's generic pipeline knowledge may miss

Example:

> "Generates Azure-specific CI/CD pipeline guidance for GitHub Actions or Azure DevOps, including OIDC authentication setup with Managed Identity federation, azd integration, and environment-based deployment separation. Returns guidance to be saved as `.azure/pipeline-setup.md`. Covers both azd-based and Azure CLI-based deployment pipelines with proper auth configuration."

### Suggestions — Parameters

- **Change base class** from `SubscriptionCommand` to `BaseCommand` (subscription is unused)
- This reduces required params and removes a friction point

### Better test prompts

| Prompt | Why it's better |
|--------|----------------|
| "Set up a GitHub Actions pipeline for deploying my app to Azure" | Action request with specific platform |
| "Generate an Azure DevOps pipeline for this project" | Imperative |
| "Create a CI/CD workflow with azd for my Container Apps deployment" | Mentions azd, specific target |

---

## Tool 5: `deploy_architecture_diagram_generate`

### Current description

> "Generates an Azure service architecture diagram showing the recommended Azure services and their connections for an application. Use this tool when the user asks to generate, create, or visualize an Azure architecture diagram for their application, or wants to see which Azure services to use. Renders the diagram from an application topology (AppTopology) provided as input; scan the workspace first to build this topology by detecting services, frameworks, and environment variables for connection strings, and for .NET Aspire applications, check aspireManifest.json. Do not use this tool when the user needs a detailed network topology or security design."

### Current test prompt

> "Generate the azure architecture diagram for this application"

### Problems

- **(Root cause)** Claude can generate Mermaid diagrams from training. But it can't produce one that follows the exact AppTopology schema with correct Azure service type enums and formatting — moderate unique value
- **Missing** from the `deploy_azure_resources_and_applications` consolidated tool group
- Scanning guidance is buried in a run-on sentence
- Requires structured JSON input (`AppTopology`) — high parameter complexity

### Suggestions — Description

Separate the two-phase workflow clearly: scan first, then call with structured topology. Mention the output is a Mermaid diagram with correct Azure service naming.

Example:

> "Generates a Mermaid architecture diagram showing recommended Azure services and their connections for an application. Input is a structured AppTopology JSON built by analyzing the workspace (detect services, frameworks, ports, Docker settings, dependencies from connection strings and environment variables; for .NET Aspire apps, check aspireManifest.json). Returns a formatted Mermaid diagram and lists supported Azure compute and dependency service types."

### Suggestions — Configuration

- **Add** to the `deploy_azure_resources_and_applications` consolidated tool `mappedToolList`

### Better test prompts

| Prompt | Why it's better |
|--------|----------------|
| "Show me the Azure architecture for this application" | Action, not question |
| "Draw a diagram of what Azure services my app needs" | Visual action request |
| "Visualize how my services should be deployed on Azure" | Deployment-oriented |

---

## Area Description Format Alignment

Most areas follow a consistent pattern:

> `"{Service} operations - Commands for managing [and querying/accessing] Azure {Service} resources[. Includes operations for {sub-resources}]."`

Examples from other areas:

| Area | Description |
|------|-------------|
| KeyVault | `"Key Vault operations - Commands for managing and accessing Azure Key Vault resources."` |
| Cosmos | `"Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for accounts, databases, containers, and document queries."` |
| SQL | `"Azure SQL operations - Commands for managing Azure SQL databases, servers, and elastic pools. Includes operations for listing databases, configuring server settings, managing firewall rules, Entra ID administrators, and elastic pool resources."` |
| ContainerApps | `"Azure Container Apps operations - Commands for managing Azure Container Apps resources. Includes operations for listing container apps and managing container app configurations."` |

Key conventions:
- ASCII hyphen `-` (not em-dash `–`)
- Starts with `"{Service} operations - Commands for..."`
- Declarative tone — no workflow prescriptions or "Use when..." guidance
- 1–3 sentences, typically 15–60 words
- Optional `"Includes operations for..."` clause for sub-resources

Current deploy description uses em-dash, prescriptive workflow, and external tool references — diverges from the norm.

---

## Changes Summary

| # | Item | Change | Why |
|---|------|--------|-----|
| 1 | Area description (DeploySetup.cs) | Rewrite to follow `"{Service} operations - Commands for..."` pattern with ASCII hyphen; declarative tone; drop workflow steps | Aligns with other areas' format convention |
| 2 | `deploy_plan_get` description | Replace negatives with positive prereqs; mention `.azure/plan.copilotmd` output | Reduces perceived barrier; signals unique azd-ecosystem value |
| 3 | `deploy_plan_get` params | Make `source-type` and `deploy-option` optional (both already have `DefaultValueFactory`) | Lowers friction; defaults are sensible |
| 4 | `deploy_app_logs_get` metadata | Set `LocalRequired = true` | **Bug fix** — tool reads local `azure.yaml` via `File.ReadAllText()` |
| 5 | `deploy_app_logs_get` description | Add "Requires local workspace access to read the azure.yaml project file" | Consistent with metadata fix |
| 6 | `deploy_iac_rules_get` description | Remove "Use when user asks:" suffix; emphasize azd/AzCli-specific rules | Wider semantic matching; signals content Claude doesn't have |
| 7 | `deploy_iac_rules_get` title | `"Get Iac(Infrastructure as Code) Rules"` → `"Get IaC (Infrastructure as Code) Rules"` | Casing fix + missing space |
| 8 | `deploy_pipeline_guidance_get` description | Remove "Before calling this tool, confirm with the user"; emphasize OIDC/Managed Identity patterns | **Removes invocation blocker**; signals unique auth setup guidance |
| 9 | `deploy_pipeline_guidance_get` title | `"Get CICD Pipeline Guidance"` → `"Get CI/CD Pipeline Guidance"` | Formatting fix |
| 10 | `deploy_architecture_diagram_generate` description | Restructure with clear scan→call workflow and output format | Clearer agent guidance |
| 11 | Consolidated tool config | Add `deploy_architecture_diagram_generate` to `deploy_azure_resources_and_applications` `mappedToolList` | Tool is missing from deploy group (only in `design_azure_architecture`) |
| 12 | Consolidated tool description | Update to mention architecture diagrams and azd conventions | Matches expanded tool list |
| 13 | Test prompts (e2eTestPrompts.md) | Replace question-style prompts with action requests | Questions trigger Claude's native answers; actions trigger tool calls |

---

## Exact Proposed Text

### 1. Area description (DeploySetup.cs line 36)

**Current:**
```
"Deploy operations – Commands for deploying applications to Azure. Provides sub-commands to generate deployment plans, offer infrastructure-as-code (Bicep/Terraform) guidance, fetch application logs, generate CI/CD pipeline guidance, and produce Azure architecture diagrams based on application topology."
```

**Proposed:**
```
"Deploy operations - Commands for generating Azure deployment plans, architecture diagrams, IaC (Bicep/Terraform) rules, CI/CD pipeline guidance, and retrieving application logs. Includes operations for deployment plans, infrastructure-as-code best practices, CI/CD pipeline setup, and application log retrieval aligned with Azure Developer CLI (azd) conventions."
```

### 2. `deploy_plan_get` description (GetCommand.cs)

**Current:**
```
"Creates a deployment plan for deploying an application to Azure using the options provided by the caller. Use this tool when the user wants a formatted, step-by-step deployment plan (including suggested Azure resources, infrastructure as code (IaC) templates, and deployment instructions) based on a target Azure hosting service (for example, Container Apps, App Service, or AKS) and a chosen provisioning tool (such as Azure Developer CLI (azd) or Azure CLI with Bicep or Terraform). This command does not scan the workspace or automatically recommend Azure services. Instead, the caller or agent must first analyze the workspace, determine the services, frameworks, and dependencies to deploy, select the appropriate Azure hosting service, provisioning tool, IaC type, and deployment option, and then pass those chosen values into this tool to generate the deployment plan."
```

**Proposed:**
```
"Call this tool to generate a step-by-step Azure deployment plan following azd conventions. Returns a structured Markdown template including Azure resource architecture, IaC scaffolding instructions, and deployment commands for the chosen hosting service (Container Apps, App Service, or AKS) and provisioning tool (azd or Azure CLI). Output should be saved to .azure/plan.copilotmd. Before calling, analyze the workspace to determine services, frameworks, dependencies, and select the target hosting service. Call deploy_architecture_diagram_generate first to build the AppTopology if no architecture has been determined."
```

### 3. `deploy_plan_get` params (DeployOptionDefinitions.cs)

Change `SourceType` (line ~155) `Required = true` → `Required = false`
Change `DeployOption` (line ~167) `Required = true` → `Required = false`
Both already have `DefaultValueFactory` providing sensible defaults (`"from-project"` and `"provision-and-deploy"`).

### 4 & 5. `deploy_app_logs_get` (LogsGetCommand.cs)

**Metadata fix:** `LocalRequired = false` → `LocalRequired = true`

**Current description:**
```
"Shows application logs specifically for Azure Developer CLI (azd) deployed applications from their associated Log Analytics workspace for Container Apps, App Services, and Function Apps. Designed exclusively for applications deployed via 'azd up' command and automatically discovers the correct workspace and resources based on the azd environment configuration. Use this tool to check deployment status or troubleshoot post-deployment issues."
```

**Proposed description:**
```
"Shows application logs for Azure Developer CLI (azd) deployed applications from their associated Log Analytics workspace. Supports Container Apps, App Services, and Function Apps deployed via 'azd up'. Requires local workspace access to read the azure.yaml project file. Automatically discovers the correct Log Analytics workspace and resources based on azd environment configuration. Returns timestamped log entries for checking deployment status or troubleshooting post-deployment issues."
```

### 6 & 7. `deploy_iac_rules_get` (RulesGetCommand.cs)

**Title fix:** `"Get Iac(Infrastructure as Code) Rules"` → `"Get IaC (Infrastructure as Code) Rules"`

**Current description:**
```
"Retrieves rules and best practices for creating Bicep and Terraform Infrastructure as Code (IaC) files to deploy Azure applications. Use this tool when the user asks for rules, guidelines, or best practices for writing Bicep scripts or Terraform templates for Azure resources. The rules cover Azure resource configuration standards, compatibility with Azure Developer CLI (azd) and Azure CLI, and general IaC quality requirements. Use when user asks: show me the rules and best practices for writing Bicep and Terraform IaC for Azure."
```

**Proposed description:**
```
"Retrieves curated IaC rules and best practices for creating Bicep or Terraform files compatible with Azure Developer CLI (azd) or Azure CLI deployments. Covers Azure resource configuration standards, azd-specific conventions, naming requirements, and deployment tool constraints that differ from generic IaC guidance. Returns a formatted rules document. Specify deployment tool (AzCli or AZD), IaC type (bicep or terraform), and target resource types."
```

### 8 & 9. `deploy_pipeline_guidance_get` (GuidanceGetCommand.cs)

**Title fix:** `"Get CICD Pipeline Guidance"` → `"Get CI/CD Pipeline Guidance"`

**Current description:**
```
"Generates CI/CD pipeline configuration and step-by-step guidance for deploying an application to Azure using GitHub Actions or Azure DevOps pipelines. Use this tool when the user wants to create a CI/CD pipeline, set up automated deployment workflows, or configure pipeline files to deploy their application to Azure. Supports both Azure Developer CLI (azd) and Azure CLI based deployments, and can generate pipelines that provision infrastructure and deploy application code. Before calling this tool, confirm with the user whether they prefer GitHub Actions or Azure DevOps, and whether they have existing Azure resources for their deployment environments. Use when user asks: how do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure?"
```

**Proposed description:**
```
"Generates CI/CD pipeline guidance for deploying an application to Azure using GitHub Actions or Azure DevOps. Covers OIDC authentication setup with Managed Identity federation, azd-based and Azure CLI-based deployment workflows, and environment-based deployment separation. Returns structured guidance to be saved as .azure/pipeline-setup.md. Supports pipelines that provision infrastructure and deploy application code."
```

### 10. `deploy_architecture_diagram_generate` (DiagramGenerateCommand.cs)

**Current description:**
```
"Generates an Azure service architecture diagram showing the recommended Azure services and their connections for an application. Use this tool when the user asks to generate, create, or visualize an Azure architecture diagram for their application, or wants to see which Azure services to use. Renders the diagram from an application topology (AppTopology) provided as input; scan the workspace first to build this topology by detecting services, frameworks, and environment variables for connection strings, and for .NET Aspire applications, check aspireManifest.json. Do not use this tool when the user needs a detailed network topology or security design."
```

**Proposed description:**
```
"Generates a Mermaid architecture diagram showing recommended Azure services and their connections for an application. Input is a structured AppTopology JSON built by scanning the workspace: detect services, frameworks, ports, Docker settings, and dependencies from connection strings and environment variables. For .NET Aspire applications, check aspireManifest.json. Returns a Mermaid diagram string. Supported compute types include ContainerApps, AppService, AKS, FunctionApp, SpringApps, StaticWebApp. Supported dependency types include SQL, Cosmos, Redis, Storage, ServiceBus, EventHubs, KeyVault, and others."
```

### 11 & 12. Consolidated tool config (consolidated-tools.json)

**Add** `"deploy_architecture_diagram_generate"` to `deploy_azure_resources_and_applications` `mappedToolList`.

**Update description** to:
```
"Deploy resources and applications to Azure using azd conventions. Generate architecture diagrams, deployment plans, IaC rules for Bicep and Terraform, CI/CD pipeline guidance, and retrieve application logs."
```

### 13. Test prompts (e2eTestPrompts.md)

**Current:**
```
| deploy_app_logs_get | Show me the log of the application deployed by azd |
| deploy_architecture_diagram_generate | Generate the azure architecture diagram for this application |
| deploy_iac_rules_get | Show me the rules and best practices for writing Bicep and Terraform IaC for Azure |
| deploy_pipeline_guidance_get | How do I set up a CI/CD pipeline with GitHub Actions or Azure DevOps to deploy my app to Azure? |
| deploy_plan_get | How do I create a step-by-step deployment plan for my application to Azure using azd with Bicep or Terraform IaC? |
```

**Proposed:**
```
| deploy_app_logs_get | Show me the logs for my azd app in the current workspace |
| deploy_architecture_diagram_generate | Show me the Azure architecture diagram for this application |
| deploy_iac_rules_get | Get the IaC rules for my azd Bicep deployment to Container Apps |
| deploy_pipeline_guidance_get | Set up a GitHub Actions pipeline for deploying my app to Azure |
| deploy_plan_get | Deploy this application to Azure Container Apps |
```
