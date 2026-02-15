# Azure MCP Context Prompts

Use these prompt templates to set **Microsoft Entra ID tenant** and **Azure subscription** context for Azure MCP tools. Setting context once per conversation helps ensure auth tokens are acquired for the correct tenant and reduces repeated prompting.

## 1. Set Default Context (Tenant + Subscription)

> For the rest of this conversation, when using Azure MCP tools, always use:  
> **Tenant:** `<TENANT_ID>` (or `<TENANT_DOMAIN>`)  
> **Subscription:** `<SUBSCRIPTION_ID>`  
> Assume these values unless I explicitly override them.

## 2. Confirm Current Context

> Before running Azure MCP actions, confirm the active context you will use: **Tenant** and **Subscription**.  
> If either is missing, ask me to provide it.

## 3. Override Context for a Single Request

> For this request only, use:  
> **Tenant:** `<TENANT_ID>` (or `<TENANT_DOMAIN>`)  
> **Subscription:** `<SUBSCRIPTION_ID>`  
> Do not change the default context for future requests.  
> **Request:** [Insert request]

## 4. If I Provide Only a Subscription

> If I provide only a **subscription**, do **not** guess the tenant.  
> Use the default tenant if one is set; otherwise ask me for the tenant before authenticating or running Azure MCP tools.

## 5. Safety: Confirm Destructive Operations

> For this conversation, require explicit confirmation before any destructive or high-impact operations (delete, purge, restart, stop, update).  
> Read-only operations (list/get) may proceed without confirmation.

---

### Troubleshooting: Wrong Tenant / Token Acquired for Wrong Tenant

If you see auth errors or missing resources:

1. Run **Confirm Current Context** to verify the tenant/subscription you’re using.
2. Re-apply **Set Default Context** with an explicit tenant + subscription.
3. Remember: you can have access across tenants (B2B/Lighthouse), but Azure actions must use a token for the tenant that grants the relevant access to the subscription.

> Tip: prefer IDs (GUIDs) for tenant/subscription when possible to avoid ambiguity.