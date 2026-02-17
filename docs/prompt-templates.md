# Prompt Templates for Azure MCP Server

Prompt templates help you set your Azure tenant and subscription context once at the beginning of your Copilot session, so you don't have to repeat them in every request.

## Where to Use These Templates

You can place these templates in different locations depending on your IDE:

| IDE | Location | Instructions |
|-----|----------|-------------|
| **VS Code** | `.github/copilot-instructions.md` in your workspace | Create this file in your project root. Copilot will automatically use it as context for all conversations in that workspace. |
| **Visual Studio 2022/2026** | `.github/copilot-instructions.md` in your solution | Create this file at your solution root. GitHub Copilot will reference it automatically. |
| **IntelliJ IDEA** | Start of each chat session | Copy the template as your first message in a new Copilot chat session. |
| **Eclipse** | Start of each chat session | Copy the template as your first message in a new Copilot chat session. |

> **Tip:** For VS Code and Visual Studio, using `.github/copilot-instructions.md` means you only set up your context once per workspace/solution, and it applies to all future chat sessions automatically.

## Basic Template: Tenant and Subscription Context

**Why use this?** When your Azure subscription is in a different tenant than your default, or you work with specific subscriptions regularly, this template ensures Copilot always uses the correct credentials.

### Template

```markdown
## Azure Context

I'm working with the following Azure environment:

- **Tenant:** contoso.onmicrosoft.com
- **Subscription:** Production-Subscription

When calling Azure MCP tools, always include the `--tenant` parameter to ensure proper authentication with the tenant above.
```

### Example: Setting Context in `.github/copilot-instructions.md`

**File: `.github/copilot-instructions.md`**
```markdown
# Project Copilot Instructions

## Azure Context

I'm working with the following Azure environment:

- **Tenant:** contoso.onmicrosoft.com
- **Subscription:** Production-Subscription
- **Primary Resource Group:** myapp-prod-rg
- **Primary Region:** East US

When calling Azure MCP tools, always use these values and include the `--tenant` parameter for authentication.
```

Once this file is in your workspace, all your Copilot chats automatically know your Azure context.

### Example: Using as First Message (For IDEs without persistent instructions)

If your IDE doesn't support `.github/copilot-instructions.md`, start each chat session with:

```
I'm working with Azure tenant: contoso.onmicrosoft.com and subscription: Production-Subscription. 

Please use these values for all Azure operations and include --tenant parameter when calling Azure MCP tools.
```

Then your subsequent questions are simple:

```
Show me all storage accounts in East US
```

```
List virtual machines in the finance-prod resource group
```

Copilot will remember the context throughout your conversation.

## Why Tenant Matters

When you authenticate to Azure, Entra ID issues tokens scoped to a specific **tenant** (directory). If your subscription is in a different tenant than your default, you must specify the tenant to ensure:

1. **Correct authentication**: Token is issued for the right directory
2. **Proper permissions**: Your RBAC permissions are evaluated in the correct tenant
3. **Resource access**: You can see and manage the subscription's resources

Most Azure MCP tools support these parameters:
- `--tenant` or `-t`: Tenant ID or domain name
- `--subscription` or `-s`: Subscription ID or name
- `--resource-group` or `-g`: Resource group name

## Troubleshooting

**"Authentication failed" or "Subscription not found"**

Verify your context and update if needed:
```
Please use tenant: correct-tenant.onmicrosoft.com and subscription: Correct-Sub
```

**Context seems forgotten mid-conversation**

Remind Copilot:
```
Reminder: We're working with tenant contoso.onmicrosoft.com and subscription Production-Sub
```

## Additional Resources

- [Authentication and Security for Azure MCP Server](Authentication.md)
- [Azure MCP Commands Reference](../servers/Azure.Mcp.Server/docs/azmcp-commands.md)
- [Azure RBAC Documentation](https://learn.microsoft.com/azure/role-based-access-control/overview)
