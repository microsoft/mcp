# Known Issues

This document tracks known issues and limitations in the Azure MCP Server. Each section describes a specific issue, its impact, and any available workarounds.

- [On-Behalf-Of (OBO) Authentication - Unsupported Tools](#on-behalf-of-obo-authentication---unsupported-tools)

## On-Behalf-Of (OBO) Authentication - Unsupported Tools

The Azure MCP Server can be launched in HTTP transport mode using the On-Behalf-Of (OBO) authentication strategy:

```shell
azmcp server start --transport http --outgoing-auth-strategy UseOnBehalfOf
```

In this mode, the server acts [on behalf of the signed-in user](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-on-behalf-of-flow) by exchanging the user's access token for a downstream service token via Microsoft Entra ID.

### Delegated scope requirement

For the OBO token exchange to succeed, the downstream Azure service must expose a **delegated permission scope** (typically a `user_impersonation` scope) that can be configured on the Azure MCP server's app registration. Microsoft Entra ID uses this delegated scope to issue an access token for the downstream service on behalf of the user.

The following tools call downstream Azure service APIs that currently do **not** expose a delegated permission scope. These tools will not work when the server is running with the `UseOnBehalfOf` authentication strategy.

| Namespace | Impacted Tools |
|-----------|----------------|
| monitor | `azmcp_monitor_healthmodels_entity_gethealth` |
| communication | `azmcp_communication_sms_send`, `azmcp_communication_email_send` |
| confidentialledger | `azmcp_confidentialledger_entries_append`, `azmcp_confidentialledger_entries_get` |

### Workaround

If you need to use tools for the affected services in a remote HTTP deployment, consider using the `UseHostingEnvironmentIdentity` authentication strategy instead:

```shell
azmcp server start --transport http --outgoing-auth-strategy UseHostingEnvironmentIdentity
```

In this mode, the server uses its own hosting environment identity (e.g., Azure Managed Identity) to authenticate with downstream services, bypassing the need for delegated permission scopes. Note that all users will share the server's identity and permissions in this mode.
