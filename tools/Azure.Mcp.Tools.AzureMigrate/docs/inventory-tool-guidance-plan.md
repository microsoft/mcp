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

## Open question: who decides the text-only path

Today the agent (LLM) chooses the path by setting the `text-only` flag on `createorupdate`, based on the
user's words and the tool description. The MCP server only validates the call (`session-id` and `message` are required
when `text-only` is true) and branches on the flag; it never infers intent from the message.

| User says | Agent's likely choice |
|---|---|
| "I want to upload my inventory" | Flag omitted: file path, upload command returned |
| "Set web-01's OS to Windows Server 2025" (with an earlier `sessionId`) | `text-only: true`: message sent to the Discovery Service |
| "Update my inventory" (ambiguous) | Either path, or the agent asks the user |

**Risk:** if the agent forgets the flag on a text change, it takes the file path and the user gets an upload command
they do not need. Nothing on the server catches this.

**Decision:** keep a single `createorupdate` tool with the `text-only` flag; do not split it into separate file and text
tools. The tool description remains the agent's guide for when to set the flag. Any MCP Apps upload widget on this tool
will therefore also render for text-only calls, so it must handle that case (for example, show status only).

## MCP Apps upload widget (in design)

Decisions so far:

- Upload happens **directly from the browser** (the widget PUTs the file to the write-only SAS URL).
- The azcopy command was to be removed. Update (2026-11-18): it is kept only as a fallback; see "Widget upload and status
  tracking" below.

Finding (verified 2026-11-17): a CORS preflight (`OPTIONS`, method `PUT`, headers `x-ms-blob-type,content-type`) to
`discoverysvccanecyrepsa.blob.core.windows.net` from `https://m365.cloud.microsoft`, `https://copilot.microsoft.com`
and `https://example.com` returns **403 CorsPreflightFailure: no CORS rules match**. So a browser upload is blocked today.
The SAS token authorises the write but does not bypass CORS. No Discovery Service code change is needed, but its
storage account(s) need a Blob CORS rule allowing `PUT`/`OPTIONS` and headers `x-ms-blob-type`, `content-type`
from the widget's origin, in every region the Discovery Service uses for upload. The widget's origin inside Cowork's
sandboxed iframe is not yet known.
The plugin package must also list the storage domain in the widget's `connectDomains` and in `validDomains`.

Open: confirm the widget's origin in Cowork, whether a file picker works in the sandboxed iframe, whether azmcp
supports tool `_meta` and `resources/read`, and whether the widget keeps running when scrolled away. Removing the
azcopy fallback means a user whose widget does not render (for example, a client without MCP Apps support) has no
upload path.

### Exploring: local helper app launched by a custom link (spike 2026-11-17)

To avoid both CORS and the azcopy command, the widget opens (via `ui/open-link`, https only) an azmcp page that
hands off to an installed helper through `azmigrate-upload://`. The helper shows a file picker and uploads natively.
Production design: the link carries a short-lived token that the helper exchanges with azmcp for the write-only SAS;
the SAS never appears in a URL.

Spike result (local, Windows): a WinForms helper registered `azmigrate-upload://` under HKCU (no admin). Launching
the link started the helper, which PUT the file to a real SAS from `createorupdate` (201 Created). The status tool then
went Running → Succeeded and returned the Discovery Service summary. Not yet tested: the browser's "Open app?"
prompt from an https page, `ui/open-link` from a Cowork widget, the token exchange, macOS, and distribution/signing.

### Plan: integrate the helper into createorupdate (in discussion)

Target flow: `createorupdate` (file path) returns `uploadLink` (a short-lived token link) instead of `uploadCommand`.
The widget's **Upload inventory** button opens `https://<azmcp>/upload/launch?t=…`, which opens
`azmigrate-upload://upload?t=…`. The helper shows a file picker, exchanges the token at `POST /upload/resolve` for the
write-only SAS and uploads. The widget tracks status and posts `ui/message` on completion. Text-only is unchanged.

Phases:
1. azmcp: `uploadLink` in the result, `/upload/launch` and `/upload/resolve` endpoints, updated instructions.
2. Helper: token resolve, file picker, size check, retry, completion message; MSI/winget (Windows first).
3. Cowork widget: `_meta.ui.resourceUri` on `createorupdate`, Upload button via `ui/open-link`, status polling,
   `ui/message`; plugin repackage with the azmcp domain in `validDomains`.

Decisions:
- **Fallback when the helper is not installed:** give the user the azcopy command. The launch page shows it behind
  "Can't install the helper?" (assumed; not in the chat).
- **Token:** reusable until expiry. Stateless and encrypted (holds the SAS, sessionId, operationId and user id), expiring
  with the 30-minute upload window; the helper can retry with the same link.
- **Delivery order:** ship everything together (azmcp link and endpoints, helper, Cowork widget); no interim chat-link release.
- **Helper:** Windows only first, packaged as a per-user MSIX (no admin; registers `azmigrate-upload://` in its manifest;
  auto-updates) signed through ESRP. The Azure Migrate team owns it; it is downloaded from the launch page. macOS users
  get the azcopy fallback until there is demand.

All plan decisions are settled; implementation has not started.

**Status: rejected (2026-11-18).** Distributing and installing a helper app on each customer's machine is not
acceptable. The helper approach is dropped; the spike stays only as evidence that native upload to the SAS works.

### Options without install, chunking or streaming (2026-11-18)

Constraints: no app installed on the customer's machine; no chunked or streamed upload.

| Option | How the file reaches storage | Catch |
| --- | --- | --- |
| A. Cowork workspace file | Tool parameter with `contentEncoding: base64`; Cowork sends the file in one `tools/call`; azmcp PUTs it | 150 MiB per file/call (Cowork limit) |
| **B. Storage CORS rule (chosen, 2026-11-18)** | Widget PUTs directly to the SAS | Storage CORS change in every region |
| C. Single-request relay (earlier choice, now fallback) | Widget sends the whole file in one request to azmcp; azmcp PUTs it to the SAS | File passes through azmcp |
| D. azcopy command | User runs the command | Was to be removed |

**Decision (updated 2026-11-18): option B.** The widget uploads the file straight to the write-only SAS from the browser,
the same way the Azure Portal's CSV import already does (see "How the portal's CSV import uploads" below). The file never
passes through azmcp, so azmcp's body-size limits and memory use don't apply. The only change is adding the Cowork widget
origin to the existing CORS rule on the DS report storage accounts.

**Earlier decision: option C**, kept as the fallback if the CORS change is not approved. The widget would upload the whole
file to an azmcp endpoint in one HTTPS request; azmcp would then do one `PUT` to the write-only SAS.

- **Relay mode:** azmcp pipes the incoming request body straight into the SAS `PUT` (no buffering in memory or on disk).
  The client still sends one request and azmcp makes one outgoing request; this is not chunked upload.
- **Auth:** `createorupdate` returns a short-lived encrypted upload token (SAS, sessionId, operationId, user id) that
  the widget sends with the upload; the iframe never sees user credentials.
- **Verified in azmcp code (2026-11-18):** no `resources/list` / `resources/read` handlers exist yet (widget resource
  must be added); tool `_meta` is built in `CommandFactoryToolLoader` (can carry `ui.resourceUri`); Kestrel's
  default 30 MB request-body limit is not overridden, so the upload endpoint must raise it for that route only.
- **To verify:** App Service request-size and request-timeout limits; whether a file picker works in Cowork's
  sandboxed iframe. With azcopy removed, a client that cannot render the widget has no upload path.

### How the portal's CSV import uploads (verified 2026-11-18)

The Azure Portal already uploads from the browser straight to a Discovery Service SAS, the same way as option B:

1. The portal calls `POST …/ImportUri`. DS creates a write-only SAS on the report storage account and starts the
   import job (`ImportSitesController`, `reportStorageClient.GetSASUri(blobName, SasBlobPermissions.Write, …)`).
2. The browser uploads the file with `@azure/storage-blob` `BlockBlobClient.uploadData(file)`
   (AzureMigrate-Portal `Client/React/Views/Data/Common/Ajax.ts`, `uploadFileContentsToBlobStorage`, called from
   `ServerImportOnboardingUtilities.tsx` `uploadCsvAndPollJob`). The portal then polls the job.

The MCP upload SAS (`McpController`, `GetV1Client`) uses the same report storage account (`ReportStorageClient`,
`ReportStorageAccountManager`). The only difference is the browser origin making the call.

Preflight checks (unauthenticated `OPTIONS`, method `PUT`) against `discoverysvccanecyrepsa` (EUS2EUAP; Stamp04's
account not tested):

| Origin | Result |
| --- | --- |
| `https://portal.azure.com`, `https://ms.portal.azure.com`, `https://anything.portal.azure.com` | Allowed |
| `https://azuremigrate.reactblade.portal.azure.net`, `https://azuremigrate.hosting.portal.azure.net`, `https://reactblade.portal.azure.net` | Allowed |
| `https://portal.azure.cn`, `http://portal.azure.com` | Allowed |
| `https://x.y.portal.azure.net`, `https://foo.azure.com`, `https://portal.azure.us`, `*.office.com`, `*.cloud.microsoft`, `https://example.com` | 403 |

- Allowed methods: GET, PUT, POST, DELETE. Max age: 300 seconds.
- Wildcards seem to cover only one subdomain level (`*.portal.azure.com` matches; `x.y.portal.azure.net` does not).
- The DS repo has no CORS configuration, so the rule is set outside the repo (owner not yet identified).

Implication: a browser `PUT` to this account already has a precedent. Option B would add the Cowork widget origin to the
existing rule list rather than introduce a new kind of access. The widget origin is still unknown; the origin probe
(an azmcp tool with an MCP Apps widget that reports `location.origin`) is being built to find it. Based on this
precedent, option B is now the chosen direction.

### Upload options considered: summary (2026-11-18)

| Option | Tried? | Outcome | Why not taken / status |
| --- | --- | --- | --- |
| azcopy command (current) | In use | Works; user runs the returned command on their machine | Requires the user to install and run a CLI outside Cowork. To be replaced by an in-Cowork upload. |
| **B. Widget `PUT` straight to the SAS (browser)** | Yes: preflight from `https://m365.cloud.microsoft`, `https://copilot.microsoft.com`, `https://example.com` | 403 `CorsPreflightFailure` today | **Chosen.** The portal's CSV import already uploads from the browser to the same report storage account, so this follows an existing pattern. The file doesn't go through azmcp. Needed: the Cowork widget origin (origin probe), adding it to the storage CORS rule in every DS region (the owner of the rule is not identified yet), and checking Stamp04's account. |
| Local helper app via `azmigrate-upload://` link | Yes: Windows spike (WinForms, HKCU registration) | Worked: 201 Created, status went to Succeeded | **Rejected.** (1) We would have to code-sign the exe and maintain the signing pipeline, or Windows SmartScreen and enterprise policy would block it. (2) Customers shouldn't need to install an extra exe just to upload a file: it adds friction, needs admin or IT approval in many organisations, and gives us a client binary to distribute, update and support (plus a separate build for macOS). |
| Chunked or streamed upload from the widget | No | — | **Not taken.** We would have to track how far each upload has got (block IDs and offset) and keep that state somewhere that survives a restart. If azmcp crashed or was recycled mid-upload, the upload couldn't resume without that state and the user would have to start again. That means extra state management, cleanup of uncommitted blocks, and retry logic for little benefit, since B uploads in one request. |
| A. Cowork workspace file passed to the tool (`contentEncoding: base64`) | No (analysed from the Cowork doc and azmcp code) | — | The file arrives base64-encoded inside one `tools/call` on the main MCP endpoint. Kestrel's 30 MB default body limit applies, and base64 grows the file by a third (about 21 MiB raw maximum). The SDK also buffers the whole JSON body in memory. Cowork caps it at 150 MiB per call and tool calls should finish in under 30 s. |
| C. Single-request relay through azmcp | Not built | — | **Not taken; kept as the fallback** if the CORS change is not approved. Every file would pass through azmcp, which by default hits Kestrel's 30 MB (about 28.6 MiB) request-body limit. That limit isn't overridden in azmcp today, so the upload route would need its own higher limit. It would also depend on App Service request-size and timeout limits, and use azmcp bandwidth for every upload. B avoids all of that and matches the portal. |

### Widget upload and status tracking: built and tested (2026-11-18)

Option B is built as a proof of concept (OneBox Discovery Service, local azmcp behind a dev tunnel, Cowork plugin
"Azure Migrate (Local Widget)") and tested end to end in Cowork for both a file upload and a text-only change.

**What we learned**

- **Widget origin:** `https://cowork-<id>.widget-renderer.usercontent.microsoft` (found with the origin probe). The
  storage CORS rule `https://*.widget-renderer.usercontent.microsoft`, methods `PUT`/`OPTIONS`, is enough. Verified on the
  OneBox account: Cowork origin 200, other origins 403. The scripts to check and set the rule are in Discovery Service
  [PR 17984633](https://msazure.visualstudio.com/DefaultCollection/One/_git/AzureMigrate-DiscoveryService/pullrequest/17984633)
  (`docs/Scripts/StorageCors`). The production storage accounts still need this rule, which needs sign-off.
- **The file picker works** in Cowork's sandboxed widget iframe. The browser `PUT` to the SAS returns 201.
- **One panel for both modes:** Cowork renders the widget on every `createorupdate` call, including text-only ones.
  azmcp omits null fields, so a text-only result has no `uploadCommand`. The widget recognises the result by
  `sessionId` + `success` and shows "Updating your inventory, nothing needs to be uploaded" instead of the file picker.
  The upload UI is hidden until an upload URL is present.
- **Panel height:** report the body's content height in `ui/notifications/size-changed`. `documentElement.scrollHeight`
  never drops below the frame's current height and left a large empty gap in the chat.
- **`ui/message` rejected:** a widget can wake the agent only with `ui/message`, which Cowork shows as a user message in
  the chat (queued as "Waiting" while the agent is busy). The requirement is that only the user writes chat messages,
  so the widget never posts messages.
- **Proxy timeout:** a widget `tools/call` goes through Cowork's proxy, which times out in under about 27 s. The
  status tool's 25-second wait times out there, so the widget calls it with `no-wait` (a single check).
- **Before the file arrives** the operation status is `Running`, the same as while the file is processed (see Deferred).
- **azcopy** is kept only as a fallback, shown if the user says the panel is missing or the upload failed.

**Who tracks the status: the agent, in the same turn**

`createorupdate` returns straight away. The agent starts tracking in the same reply, without waiting for the user to say
the file is uploaded, and keeps calling the status tool while the user uploads in the panel. Because the status stays
`Running` until the file arrives and is processed, the agent just keeps checking. When it completes, the agent
summarises the result in the chat. Nothing in the code loops: the agent repeats the call because of:

1. The `createorupdate` description: "Next: … Start tracking it right away with the inventory status tool … and keep
   tracking until it completes".
2. The status description: "When: right after create-or-update succeeds, and again while the operation has not
   completed" and "Next: while 'isCompleted' is false, call it again quietly …".
3. The per-result `instructions` (the strongest signal): `WidgetFileInstructions` ("Right away, in this same reply,
   start tracking … Do not wait for the user to say the file is uploaded"), the text-only result ("Start tracking the
   inventory status right away …") and every status result that is not complete ("… check the status again quietly …
   until isCompleted is true").

`isCompleted` is the stop condition. This relies on the model following the text. It worked in every test, but it is
not enforced in code, and Cowork's limits on how long one turn can run are unknown.

**Intervals (as tested on 2026-11-18, superseded; see below)**

| Caller | Interval | Per call |
| --- | --- | --- |
| Agent (`inventory_status`) | No timer: calls again as soon as the previous call returns, about every 30 to 40 s on OneBox | Waits up to 25 s on the server: checks the Discovery Service at once, then after gaps of 2, 4, 8 and 11 s. Returns early when complete; otherwise returns the latest status with `isCompleted: false` and the instructions to check again |
| Panel (display only) | Every 30 s, at most 60 times (about 30 min), until complete | `no-wait`: a single check |

Superseded: the agent now waits with `inventory_wait` and then calls `inventory_status` once to show the summary
card, and the panel no longer polls. The current sequence is in
[sequence-diagrams/03-inventory-upload-and-tracking.md](sequence-diagrams/03-inventory-upload-and-tracking.md).


**Test results in Cowork**

- File upload, made 45 s after the panel appeared: the agent was already tracking. About 35 s after the upload it
  summarised on its own; the panel showed "Inventory Succeeded"; no message was posted on the user's behalf.
- Text-only "add a new server winabc running Windows Server 2016": the request went in as text only (no SAS). The panel
  showed "Updating your inventory", and the agent tracked the change and confirmed it.

**Open**

- The panel checks only every 30 s, so it can show `Running` after the agent has already summarised.
- The agent's summary of a file upload was thin ("Files checked: 4" rather than the server count).
- The widget code, the `no-wait` option and the instruction changes are not committed yet; their unit tests have not
  been run.

## Deferred (needs Discovery Service changes)

- A structured `activeSessionId` on the 409 response, replacing the regex on the error message.
- A separate "waiting for upload" stage in the operation status. Today the status is `Running` both while the Discovery
  Service waits for the file and while it processes it, and the upload SAS is write-only, so the MCP server cannot tell
  the two apart.
