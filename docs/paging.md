# Paging Storage account inventories

The Azure MCP Storage account command returns one page of accounts by default
when no account name is supplied:

| Tool | CLI command | Collection member |
| --- | --- | --- |
| `storage_account_get` | `azmcp storage account get` without `--account` | `accounts` |

The default page size is **50** resources. Use `--page-size` to request between
**1 and 1000** resources. A page may contain fewer resources than requested.
The existing `accounts` collection member and account models are unchanged.
Other Storage commands and non-Storage tools retain their existing behavior.

Getting one named Storage account with `--account` remains an unpaged operation.
Do not combine `--account` with `--page-size` or `--continuation-token`.

## Continuing an inventory

Responses include a `paging` member alongside the existing collection and
truncation flag. For example:

```json
{
  "accounts": [
    {
      "name": "example",
      "location": "eastus"
    }
  ],
  "areResultsTruncated": true,
  "paging": {
    "pageSize": 50,
    "continuationToken": "<opaque-token>",
    "commandId": "eb2363f1-f21f-45fc-ad63-bacfbae8c45c",
    "continuationArguments": {
      "subscription": "<resolved-subscription-id>",
      "tenant": "<resolved-tenant-id>",
      "page-size": 50,
      "continuation-token": "<opaque-token>",
      "resource-group": "example-group"
    }
  }
}
```

To retrieve the next page, call the same logical command with the exact argument
values in `paging.continuationArguments`. For the CLI, prefix each argument name
with `--`. For MCP calls, pass these values through the selected routing mode's
normal command arguments.

For example:

```powershell
azmcp storage account get --subscription <resolved-subscription-id> `
    --tenant <resolved-tenant-id> `
    --resource-group example-group `
    --page-size 50 `
    --continuation-token <opaque-token>
```

Required command arguments are still required: a continuation token alone is not
a complete invocation. The response supplies the resolved subscription, tenant,
resource group when applicable, page size, and token needed to continue.

Continue until `paging.continuationToken` is absent. An empty page can still
contain a continuation token. Do not infer completion from the number of returned
resources. Existing serializers may omit a false-valued truncation flag; the
continuation token is the authoritative indication that another page is available.

Do not change the page size or resource filters during an iteration. If the page
size is omitted on a continuation call, the size encoded by the previous page is
retained. To change scope or page size, start a new iteration without a token.

## Sharing a continuation between instances

A continuation does not depend on an MCP connection, a server session, a local
cache, a running enumerator, or a machine-specific signing key. A response can be
shared with another machine and resumed on a compatible Azure MCP server instance.

The receiving instance must:

- Support the same logical command and token version.
- Use the same Azure cloud and resource-query scope.
- Authenticate independently with sufficient permissions for that scope.
- Receive the returned continuation arguments, rather than resolve a new default
  subscription or tenant from its local environment.

`paging.commandId` is the command's stable identity, not a server URL or a routing
alias. If the receiving instance uses a different namespace, consolidated, or
single-tool routing mode, discover the logical command through that instance and
pass the same continuation arguments through its routing interface.

Tokens are opaque to callers, versioned, and bound to the resource query,
subscription, tenant, cloud, and page size. They are not credentials and do not
grant access. The current token encoding is not encryption; treat tokens and
continuation arguments as resource-scope information and share them accordingly.

Malformed tokens, unsupported versions, changed scope, and changed page size
produce explicit errors. Backend authorization and expired-token errors also
remain errors; the server does not silently restart an iteration.

Resource Graph inventories are ordered by resource ID. Paging is not a snapshot
or transaction: resource changes and Resource Graph indexing delays can affect
what is visible between calls.

## Implementation contracts

`IPagingOption` in `Microsoft.Mcp.Core` is an opt-in capability for flat command
options. Implementations declare `PageSize` and `ContinuationToken` with `[Option]`
attributes. Shared validation rejects unsupported page sizes and empty or
oversized tokens.

The Storage account command supplies a `PageRequest` whenever `--account` is
omitted, including when no paging arguments are specified. Its service uses Resource Graph `$top` and
`$skipToken` request options rather than a KQL `limit` operator. A truncated page
without a usable continuation, an oversized backend page, or malformed paging
metadata is an explicit error, not a successful terminal page.

The service page result provides canonical continuation arguments. The command maps
these into its existing result type and registers paging models with
source-generated `System.Text.Json` contexts. Paging metadata therefore remains
available in text-only, duplicated, and compact structured output, including
aggregate routing modes.

Other commands and legacy service calls that do not supply a `PageRequest` retain
their existing behavior. This first implementation does not add paging to
arbitrary SQL/KQL queries, ranked searches, generated answers, or mutation results.
