---
name: create-assessment
description: Creates an Azure Migrate assessment (datacenter migration proposal, also called a DC proposal or business case estimate). Use when the user asks to create, run or generate an Azure Migrate assessment, a DC or datacenter migration proposal, a migration cost estimate, or wants to know what their servers would cost in Azure for a region, currency or savings option.
---

# Create an Azure Migrate assessment

An assessment (DC proposal) takes the on-premises server inventory and works out, for a target Azure region:
- whether each server is ready to move to Azure,
- the recommended Azure VM size for each server (right-sized on CPU use),
- on-premises vs Azure monthly cost and the expected savings.

The Azure Migrate Discovery Service authorizes the request as the signed-in user. In this POC the inventory is a fixed dummy set of 8 servers and prices are illustrative.

## Rules
- Never mention tool names, function names, argument names, connector names or JSON field names to the user. Describe actions in plain language, e.g. "Creating the assessment...".
- Show settings with friendly names (West Europe, EUR, 3-year reserved instances), not raw values.

## When to use
- "Create an assessment", "Create a DC proposal for West Europe in EUR".
- "How much would we save moving to Azure in Central India with 3-year reserved instances?"

## Settings
| Setting | Tool argument | Required | Allowed values | Default |
|---|---|---|---|---|
| Target region | `location` | Yes | Azure region name, e.g. `eastus`, `westeurope`, `centralindia` | — |
| Currency | `currency` | No | `USD`, `EUR`, `GBP`, `INR`, `JPY`, `AUD`, `CAD` | `USD` |
| Savings option | `savings-option` | No | `None` (pay-as-you-go), `RI1Year`, `RI3Year`, `ASP1Year`, `ASP3Year` | `RI3Year` |
| Name | `assessment-name` | No | Any text | Generated |

## How
1. Tell the user in one sentence what an assessment does (see above), and which settings it uses.
2. Collect the settings:
   - If the user did not give a region, ask for it. Do not guess.
   - Map friendly names to values: "West Europe" → `westeurope`, "rupees" → `INR`, "3-year reserved instances" → `RI3Year`, "1-year savings plan" → `ASP1Year`, "pay as you go" → `None`.
   - If currency or savings option is missing, use the defaults and say so.
3. Confirm the settings in one line, e.g. "Creating an assessment for West Europe in EUR with 3-year reserved instances."
4. Call the Azure Migrate plugin's create-assessment tool with those arguments.
5. If `success` is `true`, show the `proposal`:
   - A heading with `name`, `createdBy`, and the settings.
   - **Summary:** servers assessed, on-premises monthly cost, Azure monthly cost, monthly and annual savings, and savings %. Use the `cost.currency` symbol or code.
   - **Servers:** a table with name, OS, workload, cores, memory GB, recommended SKU, readiness, on-premises and Azure monthly cost.
   - The `notes`, including that the inventory and prices are POC data.
6. If `success` is `false`:
   - Validation errors (unsupported currency or savings option): show the error and ask the user to pick an allowed value.
   - `discoveryService.status` 401 or 403: Discovery Service did not accept the user's sign-in. Suggest reconnecting the Azure Migrate plugin.
   - Any other error: quote `error` and `discoveryService.status`.
