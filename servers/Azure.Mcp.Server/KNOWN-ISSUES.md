# Known Issues

Here are some known issues with the Azure MCP Server. If available, we list a workaround to help troubleshoot the issue.

<details>
<summary><h3>Some tools are not supported in HTTP transport mode with OBO authentication</h3></summary>

**Impacted versions:** Azure MCP Server 2.0+

The following tools are not supported when the Azure MCP Server is running in HTTP mode with the `UseOnBehalfOf` authentication strategy.

| Namespace | Impacted Tools |
|-----------|----------------|
| monitor | `azmcp_monitor_healthmodels_entity_gethealth` |
| communication | `azmcp_communication_sms_send`, `azmcp_communication_email_send` |
| confidentialledger | `azmcp_confidentialledger_entries_append`, `azmcp_confidentialledger_entries_get` |

**Workaround:**

**Option 1:** Run the server locally in stdio mode:

```shell
azmcp server start --transport stdio
```

**Option 2:** Run in HTTP mode with `UseHostingEnvironmentIdentity` authentication strategy:

```shell
azmcp server start --transport http --outgoing-auth-strategy UseHostingEnvironmentIdentity
```

Note that with this authentication strategy, all users share the server's identity and permissions.

**Why doesn't OBO work?**

The Azure services behind the tools listed above do not expose a [delegated permission scope](https://learn.microsoft.com/entra/identity-platform/v2-oauth2-on-behalf-of-flow) required for `UseOnBehalfOf` authentication strategy.

</details>
