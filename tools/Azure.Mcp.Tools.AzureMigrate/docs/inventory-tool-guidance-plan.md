# Inventory tool descriptions and instructions

This plan standardizes how the three inventory tools guide the agent. It changes only the MCP server
(`createorupdate`, `status` and `export`). The Discovery Service and the assessment tools are unchanged.

## Description vs instructions

- **Description**: static text in `tools/list`. The agent reads it before calling a tool to decide when and how to call it.
- **`instructions`**: a field in each tool result, computed from that result. It tells the agent what to do next.

Neither is enforced; both are guidance the agent follows.

## 1. Description template

```
<What the tool does, one line>
When: <user intents that should trigger it>
How: <required inputs and where they come from; optional inputs>
Returns: <key result fields>
Next: <what to do with the result>
Follow 'instructions'. Do not mention tool or parameter names to the user.
```

### createorupdate

> Starts or updates the signed-in user's Azure Migrate server inventory; the required first step before export or assessment.
> When: the user has a new or updated inventory file (CMDB, CSV, Excel or zip), wants to correct or ask about their inventory
> in text, or wants to start again after a failure or expiry.
> How: call it straight away; do not ask the user to attach the file in the chat. Omit 'session-id' for a new inventory; pass it
> to update one. For a text change, pass 'session-id', 'message' (the user's words) and 'text-only' true.
> Returns: 'sessionId', 'operationId' and, for a file, 'uploadCommand'. Show 'uploadCommand' exactly as returned, in a code block.
> Next: success means the inventory has started, not finished. Start tracking it right away with the inventory status tool.

### status

> Tracks an inventory operation started by the inventory create-or-update tool.
> When: right after create-or-update succeeds, and again while it has not completed.
> How: 'session-id' and 'operation-id' from the latest create-or-update result; ask the user to start an inventory if there is none.
> Returns: 'status', 'isCompleted' and, when finished, 'result' or 'errorCode'/'errorMessage'. Each call waits about 25 seconds.
> Next: while 'isCompleted' is false, call it again quietly. When done, summarise 'result' in plain language without inventing details.

### export

> Gets a download link for a finished inventory.
> When: the user wants to download or export their inventory, after the status tool reports it as completed.
> How: 'session-id' from the latest create-or-update result; ask the user to start an inventory if there is none. 'output' is
> 'refined' (default) or 'offline' (zip); ask if it is unclear.
> Returns: 'downloadUrl' and 'downloadCommand'.
> Next: share the link or command.

## 2. createorupdate: instructions on success

| Path | Instruction |
|---|---|
| File, new session | Show the upload command verbatim (30 minutes, 500 MB, AzCopy v10). Start tracking now; do not wait for the user to say the upload has finished. |
| File, reused session | Tell the user this updates their existing inventory, not a new one. Then as above. |
| Text-only | Tell the user their change is being applied and nothing needs to be uploaded. Start tracking now. |

The user no longer has to say "uploaded": the agent starts tracking straight away.

## 3. createorupdate: instructions on failure

Failures previously returned no `instructions`.

| Case | Instruction |
|---|---|
| Validation | Tell the user which input is wrong and ask for the correct value. |
| Auth | Ask the user to sign in again. |
| 409, a different session is active | Tell the user inventory {id} is already active and offer to continue with it. |
| Text-only, 404 | The inventory was not found; offer to start a new one with a file. |
| Text-only, 400 | The inventory has no completed import yet; ask the user to upload a file first. |
| Discovery Service 5xx or unreachable | Temporary service problem; offer to try once more. |
| Upload URL missing | Unexpected error; offer to try once more. |
| Any other failure | The inventory could not be started; quote the error. |

## 4. status: instructions per state

| State | Instruction |
|---|---|
| Running | Tell the user once that you are waiting for their upload or processing it, then keep checking quietly. |
| Succeeded | Summarise 'result' and offer to export the inventory or create an assessment. |
| Failed, `NoFileUploadedForImport` | The file was not uploaded within the upload window; offer to start again. |
| Other failure | Quote 'errorMessage' and offer to start again. |
| 404 | The inventory was not found; offer to start a new one. |

Each status call now waits about 25 seconds (checks after 2, 4, 8 and 11 seconds) instead of about 60, so the user
sees progress sooner. The HA status tool shares the parser but keeps its existing instructions.

## 5. Delivery

1. Update `CreateOrUpdateInventoryCommand`, `GetInventoryStatusCommand` and `ExportInventoryCommand`.
2. Run the unit tests and build.
3. Deploy azmcp to App Service and sync the plugin's `mcp-tools.json` from the deployed `tools/list`.

## Deferred (needs Discovery Service changes)

- A structured `activeSessionId` on the 409 response, replacing the regex on the error message.
- A separate "waiting for upload" stage in the operation status. Today the status is `Running` both while the Discovery
  Service waits for the file and while it processes it, and the upload SAS is write-only, so the MCP server cannot tell
  the two apart.
