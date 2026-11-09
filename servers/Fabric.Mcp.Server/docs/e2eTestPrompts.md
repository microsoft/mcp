# Fabric MCP Server tool selection prompts
<!-- cspell:words pbism pbir TMDL -->

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

`none` means the prompt supplies the inputs needed for invocation, not that permission to mutate resources has been granted. Workspace creation and updates require separate authorization for live tests; offline tests substitute HTTP and credentials. Standard mutation confirmation still applies. The resource UUIDs below are examples, not provisioned test resources.

These prompts describe tool selection, not live test execution. Delete Item is tested only with mocked Fabric HTTP responses; never execute these destructive prompts against real Fabric during development or testing. Live Fabric, real OBO authorization, and recorded playback have not been exercised for this tool.

Delete Workspace likewise uses substituted HTTP and credentials only. Its prompts describe deletion of the specified workspace and the items under it, not authorization to execute a real deletion.

For every deletion prompt, explain the scope and request consent before sending one request. Omitted or false `hard-delete` uses the default behavior, with soft deletion dependent on item-type support; do not promise recovery or fall back to permanent deletion. Explicit true requests permanent deletion, which cannot be recovered and requires workspace Admin. Workspace deletion includes its items; do not promise recovery or permanent-deletion semantics, and do not prefetch, delete items individually, retry, or poll.

Workspace capacity assignment changes a live resource. Use only with an explicitly approved workspace, capacity, and identity; do not run these assignments as automated development checks.

Definition updates replace the complete definition of an existing item. Review the full selected parts and obtain authorization before a live update; these prompts are not permission to modify a workspace. Never upload only changed files, merge with a fetched definition, or retrieve an operation result after a no-result update succeeds.

Folder updates require explicit workspace/folder UUIDs, a replacement display name, and normal
mutation consent. Send one PATCH without moving the folder, fetching its old name, or checking
uniqueness. Do not trim/sanitize names, invent a replacement after a duplicate-name error, or
resubmit after confirmed update with unavailable metadata. After an uncertain send, a separate
explicit Get Folder can inspect the known folder; no hidden verification or retry is authorized.
These prompts do not authorize live mutations or fixture discovery.

Folder movement requires explicit workspace/folder UUIDs and normal mutation consent. A supplied
target-folder-id selects an existing folder in the same workspace; omission or JSON null selects
the workspace root. Send one POST without resolving names, creating a destination, walking the
hierarchy, or moving items individually. Fabric handles the subtree and enforces hierarchy/depth/name
conflicts. Do not retry an uncertain move blindly or resubmit confirmed movement with unavailable
metadata. These prompts do not authorize moving real folders or discovering a live fixture.

Job-instance lookup requires explicitly supplied workspace, item, and existing job-instance IDs. These prompts do not authorize starting a job or discovering a live fixture. Job-instance IDs are not Fabric operation IDs, and a successful snapshot read does not mean the job succeeded.

Job-instance listing requires explicit workspace/item UUIDs and returns one complete page per call.
Use only a returned continuation token with the same IDs for another explicit call; never follow
continuation URLs, fetch each row, or infer a total count/full history. For most items, the API reports
up to 100 recently completed instances with no corresponding limit for active instances; this is not a page cap.
List needs no mutation consent, but selecting a result for Cancel still needs separate authorization.
Absent or null continuation fields mark a final page, whether its rows are empty or nonempty.
A valid token still enables explicit continuation when its URI is absent/null or its page is empty.
The latest listed job cannot identify an earlier ambiguous Run submission or one without a tracking ID.
No live workspace/item fixture is assumed or discovered for these prompts.

Schedule listing additionally requires an explicit job type and returns one complete page, with
optional configuration, owner, creation time, and execution data preserved. These outputs can be
sensitive, so the host's disclosure confirmation still applies in read-only mode. There is no
name/job-type discovery, automatic continuation, per-schedule lookup, scheduling mutation, or live
fixture discovery. Cron interval values are minutes, not cron expressions; do not infer a local
time zone or next execution time. Schedule IDs are not job-instance IDs or operation IDs.
Absent/null terminal continuation and token-only continuation follow the same pagination rules
above. Job-instance history limits do not apply to schedule pages.

Schedule lookup requires explicit workspace/item/schedule UUIDs and a job type, with sensitive-data
disclosure confirmation even in read-only mode. It reads exactly one matching snapshot, retaining
available settings, owner, and static execution data without inventing optional fields. No List,
name/job-type discovery, enrichment, polling, mutation, or live fixture discovery is authorized.
The snapshot is not job execution status or proof of an ambiguous earlier mutation; reading does
not authorize updating, deleting, running, or canceling anything.

Schedule creation requires separate authorization for the exact workspace/item/job type, enabled
state, timing, and time zone. Before requesting consent, explain that an enabled schedule with a
past start time may trigger a job immediately and consume compute or change data. Require explicit
enabled true/false and configuration; never infer a cadence, machine time zone, or job type.
Send one POST with optional static execution data intact. No count preflight, retry, polling,
operation lookup, follow-up run, or live fixture discovery is authorized by these prompts.
HTTP 201 is creation, not execution. Unusable metadata after 201 does not undo creation;
inspect schedules separately before any new creation after uncertainty, never blindly resubmit.

Job submission requires explicit execution authorization, workspace/item UUIDs, and a supported job type.
Run prompts below are not permission to start a real job. No non-production execution fixture is assumed.
Submit once, return acceptance rather than completion, and never retry automatically or strip unsupported
parameters. Wait at least the receipt's valid Retry-After guidance before a separate Get call. An accepted
receipt without a trustworthy job-instance ID cannot be recovered by listing, inventing an ID, or resubmitting.
After an ambiguous network failure, timeout, or cancellation, inspect job history before any new submission.

Job cancellation requires separate authorization for the exact workspace, item, and existing job-instance
UUIDs. These prompts are not permission to cancel a real job. Obtain mutation consent, send one POST
without a body, and return cancellation requested rather than a Cancelled state. Never retry automatically, promise
rollback, or infer success/failure from JobAlreadyCompleted. After uncertain cancellation, check the known
job with a separate Get; wait at least valid Retry-After guidance before another request. Missing response
metadata does not undo acceptance or require a Location-derived ID.

Schedule deletion requires explicit workspace/item/schedule UUIDs, a job type, and separate mutation
authorization. Send one DELETE without a body; only HTTP 200 confirms deletion, returning the requested
context rather than a schedule snapshot. Never treat 404 as success, retry automatically, perform a
hidden verification read, follow response URLs, or cancel running jobs. Deletion does not promise
rollback; cancellation remains a separate tool and separate authorization. These prompts are not
permission to discover or delete live schedules. After an uncertain send, inspect separately before
considering another deletion; do not retry blindly.

Core-only filtering does not expose Jobs. Use `--namespace jobs --mode all` for job submission, cancellation, instance listing/lookup, and schedule lookup/listing/creation/updates/deletion alone,
or `--namespace core --namespace jobs --mode all` for workflows combining item/definition tools and job status.
The same namespace selection applies with `--mode namespace` or `--mode single`.

`context-required` means the caller must supply the required resource context and authorization. Sections and tool names are sorted alphabetically.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_assign-workspace-to-capacity | Submit a request to assign existing Fabric workspace \<workspace-id> to capacity \<capacity-id>; return acceptance without waiting for completion. | context-required |
| core_assign-workspace-to-capacity | Move workspace \<workspace-id> to Fabric capacity \<capacity-id> with one submission only, without polling or retries. | context-required |
| core_assign-workspace-to-capacity | Assign workspace \<workspace-id> to capacity \<capacity-id> and return the requested IDs with an accepted/pending state, not a claim that assignment finished. | context-required |
| core_create-folder | Create one logical folder named Sales at the root of Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. Send one request without creating OneLake or local directories. | none |
| core_create-folder | Create a folder named Q3 under existing folder bbbbbbbb-1111-2222-3333-cccccccccccc in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. Do not look up or create its parent. | none |
| core_create-folder | Create the folder using my supplied workspace UUID and display name, with parent-folder-id null to use the workspace root. Do not trim or sanitize the name or perform duplicate-name checks. | context-required |
| core_create-folder | Create the requested folder once. If Fabric reports a duplicate name, keep the error instead of returning an existing folder; if creation is confirmed but metadata is unavailable, do not repeat it. | context-required |
| core_create-folder | After the uncertain result of my earlier folder creation, explain why a new create should not be submitted blindly. Any folder inspection must be a separate explicit list-folders call. | context-required |
| core_create-item | Create a Lakehouse named Sales in Fabric workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 and return its metadata or operation receipt without waiting. | none |
| core_create-item | Create a Lakehouse named Sales in Fabric workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 and wait up to 120 seconds for it, returning an operation ID if it takes longer. | none |
| core_create-item | Create a Lakehouse named Planning in Fabric workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229, waiting up to 60 seconds and honoring the first server wait hint rather than checking early. | none |
| core_create-item | Create a Notebook named Sales from the saved sales.ipynb in local folder C:\Fabric\Sales.Notebook in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229. Explicitly map just that file to artifact.content.ipynb, use format ipynb, and do not upload other files in the folder. | none |
| core_create-item | Create a Notebook in workspace \<workspace-id> from the inline definition JSON I supplied, without reading any files on the HTTP server. Return the item or operation receipt without polling. | context-required |
| core_create-item | Create a SemanticModel in workspace \<workspace-id> using my selected item folder, definition.pbism, and the explicit TMDL file list I supplied. Do not scan for additional files or create dependencies; wait up to 120 seconds after submission. | context-required |
| core_create-item | Create a Report in workspace \<workspace-id> from my inline PBIR-Legacy definition containing definition.pbir and report.json. Preserve the existing semantic-model binding and do not make a preflight API call. | context-required |
| core_create-item | Create a Notebook in workspace \<workspace-id> from the inline definition I supplied, preserving its format, content, and safe part names. Leave filename and notebook-language validation to Fabric without a preflight call. | context-required |
| core_create-item | Create a SemanticModel in workspace \<workspace-id> from my selected local files with the supplied TMDL format. Package only the mapped files and let Fabric validate required parts and format combinations. | context-required |
| core_create-item | Create a Lakehouse named Sales with schemas enabled using creationPayload enableSchemas=true, in existing folder bbbbbbbb-1111-2222-3333-cccccccccccc in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229. Do not send a definition or look up the folder. | none |
| core_create-item | Create the item from the inline definition and workload-specific options JSON I supplied in workspace \<workspace-id>. Keep those API options separate from sync and wait up to 120 seconds for creation. | context-required |
| core_create-item | Create a Lakehouse named Classified in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 with sensitivity label b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f and ApplyOrFail. If the label cannot be applied, do not retry without it or use Ignore. | none |
| core_create-item | Create a Lakehouse named Draft in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 and request label b7b4f4d9-3f0d-4b3e-8f3d-4f6d3f4f3f4f with the explicitly chosen Ignore strategy, allowing Fabric to proceed if the label cannot be applied. | none |
| core_create-workspace | Create a Microsoft Fabric workspace called Sales Planning. | none |
| core_create-workspace | Create a new Fabric workspace named Finance Sandbox with description Quarterly planning experiments. | none |
| core_create-workspace | Create a Fabric workspace called Capacity Analytics and assign it to existing capacity f4031b2e-318f-4a14-9a3e-103e9bcfc953 during creation. | none |
| core_create-workspace | Create a Microsoft Fabric workspace named Domain Analytics on capacity f4031b2e-318f-4a14-9a3e-103e9bcfc953 and assign it to domain 88d8f15b-5105-449b-98d3-681345f00326 in the same request. | none |
| core_delete-item | Delete Fabric item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb from workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa using the default deletion behavior. Do not request permanent deletion. | none |
| core_delete-item | Delete Fabric item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with hard-delete explicitly set to false. | none |
| core_delete-item | Permanently delete Fabric item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb from workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Explicitly set hard-delete to true. | none |
| core_delete-workspace | Delete Fabric workspace cfafbeb1-8037-4d0c-896e-a46fb27ff222 and the items under it. | none |
| core_delete-workspace | Remove Microsoft Fabric workspace cfafbeb1-8037-4d0c-896e-a46fb27ff222, including its items. | none |
| core_delete-workspace | Use workspace UUID cfafbeb1-8037-4d0c-896e-a46fb27ff222 to delete only that Fabric workspace and its contained items. | none |
| core_get-capacity | Get metadata for Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | Show the SKU, region, and state of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | What is the display name of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357? | none |
| core_get-folder | Get metadata for folder bbbbbbbb-1111-2222-3333-cccccccccccc in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb with one GET. Do not list folders first. | none |
| core_get-folder | Show the display name and available parent folder ID for folder bbbbbbbb-1111-2222-3333-cccccccccccc in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. Do not fetch its parent or enumerate children or items. | none |
| core_get-folder | Read the logical Fabric folder using my supplied workspace and folder UUIDs in read-only mode. This is not a OneLake directory or local filesystem lookup. | context-required |
| core_get-folder | Inspect the folder once using the IDs I supplied. Leave missing parent metadata omitted, preserve its returned display name, and return throttling guidance without automatically retrying or polling. | context-required |
| core_get-item | Get the metadata for Fabric item '<item-id>' in workspace '<workspace-id>' without reading its data or definition. | none |
| core_get-item | Show the display name, description, and type of Fabric item '<item-id>' in workspace '<workspace-id>'. | none |
| core_get-item | Inspect the existing Fabric item '<item-id>' in workspace '<workspace-id>' and return only its generic metadata. Do not modify it. | none |
| core_get-item-definition | Get the definition of item 5b218778-e7a5-4d73-8187-f10824047715 in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 as inline parts. Do not write files or wait if accepted. | none |
| core_get-item-definition | Retrieve that Fabric item's definition in ipynb format, waiting up to 120 seconds; return an operation ID if it is still pending. | context-required |
| core_get-item-definition | Export the definition for item \<item-id> in workspace \<workspace-id> to the new local directory C:\Fabric\Sales.Notebook. Preserve all bytes, nested paths, binary assets, and .platform; never overwrite existing files. | context-required |
| core_get-item-definition | The Get Item Definition operation 0acd697c-1550-43cd-b998-91bfbfbd47c6 is already Succeeded. Retrieve its typed definition with one result GET, without another POST or workspace/item lookup. | none |
| core_get-item-definition | My definition operation has succeeded. Use its saved operation ID to export the result to the new local directory I specified, without restarting retrieval or guessing a format. | context-required |
| core_get-item-definition | Through the HTTP Fabric MCP server, retrieve my item's definition inline without reading or writing any host files. Request consent for sensitive definition content. | context-required |
| core_get-operation-result | Get the JSON result of completed Fabric operation 0acd697c-1550-43cd-b998-91bfbfbd47c6. | none |
| core_get-operation-result | The Fabric item creation operation in the previous receipt succeeded. Retrieve its result using that operation ID. | context-required |
| core_get-operation-state | Check the current state and progress of Fabric operation 0acd697c-1550-43cd-b998-91bfbfbd47c6 once. | none |
| core_get-operation-state | My Fabric item creation returned an accepted receipt. Check that operation's state and tell me when I should check again. | context-required |
| core_get-operation-state | The wait for my Fabric item timed out. Resume checking the operation from its receipt instead of creating the item again. | context-required |
| core_get-workspace | Get metadata for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | none |
| core_get-workspace | Show the capacity, domain, and workspace identity for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | none |
| core_get-workspace | Get Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with workspace-specific API and OneLake endpoints. | none |
| core_list-capacities | List the Fabric capacities where I am an administrator or contributor. | none |
| core_list-capacities | Show one page of my accessible Fabric capacity IDs, display names, SKUs, regions, and states. | none |
| core_list-capacities | Get the next page of Fabric capacities using continuation token ABCsMTAwMDAwLDA%3D from the previous response. | none |
| core_list-folders | List one page of logical folders in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, including nested folders and their parent IDs. Do not list OneLake paths or local directories. | none |
| core_list-folders | List only the direct child folders of bbbbbbbb-1111-2222-3333-cccccccccccc in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. | none |
| core_list-folders | Get the next page of Fabric folders using the unchanged token from the previous response, keeping the same workspace, root folder, and recursive settings even if the previous page was empty. | context-required |
| core_list-folders | Through the read-only HTTP server, list folders in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb with one request and no folder creation or filesystem access. | none |
| core_list-items | List Fabric item metadata in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, including nested folders. | none |
| core_list-items | List the Lakehouse items in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. | none |
| core_list-items | Show only items directly in Fabric folder bbbbbbbb-1111-2222-3333-cccccccccccc within workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, not its nested folders. | none |
| core_list-items | Get the next page of Fabric items in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb using the continuation token from the previous listing and the same filters. | context-required |
| core_list-workspaces | List the Microsoft Fabric workspaces I can access and show their workspace IDs, types, capacity, and domain metadata. | none |
| core_list-workspaces | List my Fabric workspaces where I am an Admin or Member using the Core management API, not the OneLake storage listing. | none |
| core_list-workspaces | List accessible Fabric workspaces and include their workspace-specific API endpoints. | none |
| core_list-workspaces | Get the next page of accessible Fabric workspace management metadata using the continuation token from the preceding list, keeping the same role filter and endpoint preference. | context-required |
| core_move-folder | Move logical folder bbbbbbbb-1111-2222-3333-cccccccccccc into existing folder aaaaaaaa-6666-7777-8888-bbbbbbbbbbbb within Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. Send one POST after mutation consent; do not enumerate or move its children individually. | none |
| core_move-folder | Move folder bbbbbbbb-1111-2222-3333-cccccccccccc to the root of workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb by omitting target-folder-id. Do not rename it or create a destination. | none |
| core_move-folder | Move the known folder to the workspace root using my supplied workspace and folder UUIDs and target-folder-id null. Do not operate on OneLake paths or local directories. | context-required |
| core_move-folder | Move the specified folder once within its workspace. If Fabric reports a hierarchy, depth, or duplicate-name conflict, retain the failure without trying another target or renaming anything. | context-required |
| core_move-folder | Explain the confirmed folder movement whose metadata was unreadable; do not repeat it or infer a missing parent. Any inspection must be a separate explicit get-folder call. | context-required |
| core_move-folder | After my earlier folder move returned an uncertain outcome, explain why it must not be blindly repeated. Use a separate explicit Get Folder only if I request inspection with the known IDs. | context-required |
| core_update-folder | Rename logical folder bbbbbbbb-1111-2222-3333-cccccccccccc in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb to Quarterly Sales. Send one PATCH after mutation consent; do not move the folder or inspect it first. | none |
| core_update-folder | Update the display name of the Fabric folder using my supplied workspace UUID, folder UUID, and exact replacement name. Do not trim or sanitize it or change OneLake or local directories. | context-required |
| core_update-folder | Rename the specified folder once and return its actual updated metadata, leaving unavailable parent metadata omitted. Do not resolve names, check duplicates, poll, or retry. | context-required |
| core_update-folder | Update the known folder to my requested name. If Fabric reports FolderDisplayNameAlreadyInUse, keep that failure instead of generating another name. | context-required |
| core_update-folder | Explain the confirmed folder update whose returned metadata was unreadable; do not repeat it. Any inspection of the supplied workspace and folder IDs must be a separate explicit get-folder call. | context-required |
| core_update-item | Rename Fabric item 5b218778-e7a5-4d73-8187-f10824047715 in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 to ProjectNotebook, leaving its description unchanged. | none |
| core_update-item | Set the description of Fabric item 5b218778-e7a5-4d73-8187-f10824047715 in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 to "Monthly reporting", without changing its name or definition. | none |
| core_update-item | Clear only the description of Fabric item 5b218778-e7a5-4d73-8187-f10824047715 in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 by setting it to an empty string. | none |
| core_update-item-definition | Replace the complete definition of item 5b218778-e7a5-4d73-8187-f10824047715 in workspace cfafbeb1-8037-4d0c-896e-a46fb27ff229 using saved sales.ipynb from C:\Fabric\Sales.Notebook, mapped to artifact.content.ipynb with format ipynb. Preserve the format and bytes, upload no other files, and return without polling. | none |
| core_update-item-definition | Update item \<item-id> in workspace \<workspace-id> from the complete inline definition JSON I supplied. Do not read any files on the HTTP server or look up the item's type. | context-required |
| core_update-item-definition | Submit my complete definition with its supplied format and safe custom part layout to item \<item-id> in workspace \<workspace-id>. Keep generic safety checks but leave supported formats, required filenames, and content validation to Fabric. | context-required |
| core_update-item-definition | Replace the definition of item \<item-id> in workspace \<workspace-id> with the full explicit saved-file mapping I supplied, including .platform. Preserve those bytes but do not request applying its metadata. | context-required |
| core_update-item-definition | Upload my complete definition to item \<item-id> in workspace \<workspace-id> and explicitly set update-metadata to true to apply metadata from the supplied .platform part. Do not change the sensitivity label or add creation properties. | context-required |
| core_update-item-definition | Replace item \<item-id>'s definition in workspace \<workspace-id> with my inline content and item-specific options JSON. Wait up to 120 seconds and finish when operation state says Succeeded, without requesting /result or getting the item. | context-required |
| core_update-item-definition | Submit my complete replacement definition once for item \<item-id> in workspace \<workspace-id>, wait at most 60 seconds, and honor the initial Retry-After rather than polling early. If it remains pending, return the operation ID instead of submitting the update again. | context-required |
| core_update-workspace | Rename Fabric workspace \<workspace-id> to 'Finance Analytics' without changing its description. | none |
| core_update-workspace | Set the description of Fabric workspace \<workspace-id> to 'Quarterly reporting' and leave its name unchanged. | none |
| core_update-workspace | Clear the description of Fabric workspace \<workspace-id> without renaming it. | none |
| core_update-workspace | Update Fabric workspace \<workspace-id> to the name 'Team Reporting' and description 'Shared reporting workspace'. | none |

## Jobs

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| jobs_cancel-item-job-instance | Request cancellation exactly once for the explicitly authorized job \<job-instance-id> of item \<item-id> in workspace \<workspace-id>. Obtain consent and return cancellation requested, not confirmation of a Cancelled state. Do not poll, wait, or retry. | context-required |
| jobs_cancel-item-job-instance | Cancel this authorized existing job using only the three supplied UUIDs. Do not supply job-type, a body, an operation ID, or a response URL. Keep acceptance if response metadata is missing or malformed. | context-required |
| jobs_cancel-item-job-instance | Request this authorized cancellation once. If Fabric reports JobAlreadyCompleted, return the failure without inferring the job's final success or failure status. Do not resubmit. | context-required |
| jobs_cancel-item-job-instance | After requesting cancellation for the authorized job, wait at least the receipt's valid Retry-After guidance, then use its known IDs with jobs_get-item-job-instance in a separate call. The job may still be InProgress. | context-required |
| jobs_cancel-item-job-instance | In the jobs namespace router, request the authorized cancellation once with workspace-id, item-id, and job-instance-id in the parameters object. On timeout, check the known job separately rather than retrying cancellation or starting a job. | context-required |
| jobs_create-item-schedule | Create one disabled schedule for job type DefaultJob on item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with enabled false and configuration {"type":"Cron","startDateTime":"2030-01-01T00:00:00Z","endDateTime":"2030-02-01T00:00:00Z","localTimeZoneId":"UTC","interval":10}. Obtain consent and send one POST only. | none |
| jobs_create-item-schedule | Create the explicitly authorized enabled schedule for my supplied workspace/item/job type and configuration. Before consent, explain that the supplied past start time may run a job immediately, consuming compute or changing data. Do not change the timing or infer a time zone. | context-required |
| jobs_create-item-schedule | Create one schedule using my supplied Daily, Weekly, or Monthly configuration and static execution-data JSON. Preserve extensible fields and opaque values. Do not preflight the schedule count, strip fields, retry, poll, or run the job separately. | context-required |
| jobs_create-item-schedule | In the jobs namespace router, create the approved schedule with explicit workspace-id, item-id, job-type, enabled, and configuration in parameters. Return creation, not execution; do not pass its schedule ID to a job-instance or operation tool. | context-required |
| jobs_create-item-schedule | If this approved creation receives HTTP 201 but its metadata cannot be read, report confirmed creation with unavailable metadata. If confirmation is lost to a timeout, report uncertainty. Never resubmit blindly or automatically delete another schedule after ScheduleExceedsLimit. | context-required |
| jobs_delete-item-schedule | Delete schedule eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee for job type DefaultJob on item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Explain that running jobs are not canceled, obtain consent, and send one DELETE without a body. | none |
| jobs_delete-item-schedule | Delete only the explicitly authorized schedule using my supplied workspace/item/schedule IDs and job type. Return the HTTP 200 deletion receipt with those IDs, not a fabricated snapshot. Do not fetch settings, run/cancel jobs, or verify with another request. | context-required |
| jobs_delete-item-schedule | In the jobs namespace router, delete the approved schedule with workspace-id, item-id, job-type, and schedule-id in parameters. Do not ask for configuration, enabled, or execution-data. Do not use the schedule ID as a job-instance or operation ID. | context-required |
| jobs_delete-item-schedule | If deletion returns 404, report the failure rather than claiming success. If it is throttled, preserve valid Retry-After guidance and safe error codes even when optional error JSON is unreadable. Never retry automatically. | context-required |
| jobs_delete-item-schedule | If the authorized DELETE times out after sending, report an uncertain outcome and suggest a separate schedule lookup before any further deletion. If HTTP 200 already arrived, retain confirmed deletion despite late cancellation or irrelevant body data. | context-required |
| jobs_get-item-job-instance | Read job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc for item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa once, without polling. | none |
| jobs_get-item-job-instance | Show the status, invocation type, and available start and end times for job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc of item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not fetch job results. | none |
| jobs_get-item-job-instance | Get safe failure metadata and any Retry-After guidance for job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc of item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not start, retry, cancel, or schedule a job. | none |
| jobs_get-item-job-instance | Use the workspace, item, and job-instance UUIDs I supplied to read the latest snapshot once. Preserve a future status string instead of treating it as successful completion. | context-required |
| jobs_get-item-job-instance | Check that existing item's job instance using its saved job-instance ID, not the generic Fabric operation-state or operation-result tools. Return the snapshot without waiting. | context-required |
| jobs_get-item-job-instance | With the server configured using --namespace core --namespace jobs --mode all, use the workspace and item UUIDs returned by core_get-item and the existing job-instance UUID I supplied to read its status once. Do not discover or start a job. | context-required |
| jobs_get-item-schedule | Read schedule eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee for job type DefaultJob on item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa once after disclosure confirmation. Return only its snapshot; do not list, enrich, poll, or change anything. | none |
| jobs_get-item-schedule | Show the enabled state and available configuration, owner, and static execution data for the workspace/item/schedule UUIDs and job type I supplied. Preserve missing optional fields and future types without inventing defaults or converting time zones. | context-required |
| jobs_get-item-schedule | In read-only Jobs mode, inspect this explicitly identified schedule after sensitive-data confirmation. Cron interval is minutes, not an expression. Do not start or cancel a job or treat enabled state as execution success. | context-required |
| jobs_get-item-schedule | Use the jobs namespace router with command jobs_get-item-schedule and my workspace-id, item-id, job-type, and schedule-id in parameters. Do not pass the schedule ID to a job-instance or operation lookup. | context-required |
| jobs_get-item-schedule | Read this known schedule once and report valid Retry-After guidance without retrying. Reject a different returned schedule ID instead of replacing my target or claiming a successful create or update. | context-required |
| jobs_get-item-schedule | Inspect the explicitly authorized schedule after an earlier ambiguous update, but do not claim that the snapshot proves that update occurred or authorize another mutation from the read. | context-required |
| jobs_list-item-job-instances | List one page of job instances for item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not fetch individual jobs or another page automatically. | none |
| jobs_list-item-job-instances | Show one page of that item's jobs with statuses, available UTC timing, and safe failure metadata. Do not treat HTTP success as job success or assume the page is full history. | context-required |
| jobs_list-item-job-instances | Fetch the next page using the unchanged continuation token from the previous response and the same explicit workspace/item UUIDs. Do not use a continuation URL or change filters. | context-required |
| jobs_list-item-job-instances | Read one page for my supplied workspace/item UUIDs. Preserve all final-page rows when continuation metadata is absent or null; if a token is present, return it without fetching another page. | context-required |
| jobs_list-item-job-instances | In read-only Jobs mode, list one page for the workspace and item UUIDs I supplied, then stop. More than 100 active rows must not be capped locally. | context-required |
| jobs_list-item-job-instances | List one page of jobs for this known item; I will explicitly select a returned job-instance ID for a separate Get. Do not infer the ID of an ambiguous Run submission or authorize cancellation from the listing. | context-required |
| jobs_list-item-schedules | List one page of schedules for job type DefaultJob on item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Preserve settings and available owner/execution data after sensitive-data confirmation; do not fetch individual schedules or another page. | none |
| jobs_list-item-schedules | Show the enabled state and available creation time and configuration of schedules for the explicit workspace, item, and job type I supplied. Do not discover IDs or infer that an enabled schedule ran successfully. | context-required |
| jobs_list-item-schedules | Fetch one more schedule page using the unchanged returned continuation token and the same workspace/item UUIDs and job type. Do not follow a continuation URL, change context, or apply job-history limits. | context-required |
| jobs_list-item-schedules | In read-only Jobs mode, list one schedule page for my supplied context. Keep all final-page rows when continuation metadata is absent or null; return any usable token from an empty page without automatically continuing. | context-required |
| jobs_list-item-schedules | Show this item's returned schedule configuration without converting time zones. Interpret Cron interval as minutes, not a cron expression; preserve unknown schedule and owner types. Do not create, update, delete, run, or cancel anything. | context-required |
| jobs_list-item-schedules | Use the jobs namespace router to list schedules with workspace-id, item-id, and job-type in parameters. Do not pass a returned schedule ID to a job-instance or operation lookup. | context-required |
| jobs_run-on-demand-item-job | Submit exactly one explicitly authorized job of type \<job-type> for item \<item-id> in workspace \<workspace-id>. Return the accepted receipt without polling, waiting, fetching results, or retrying. | context-required |
| jobs_run-on-demand-item-job | Run the authorized item job once with the execution-data JSON and job-parameters JSON array I supplied. Preserve omitted fields and opaque values; do not strip parameters or resubmit if Fabric reports FeatureNotAvailable. | context-required |
| jobs_run-on-demand-item-job | Submit this authorized job with no optional body. If Location is missing, invalid, or identifies only a collection, report accepted but not trackable; never invent an ID or use x-ms-operation-id as the job-instance ID. | context-required |
| jobs_run-on-demand-item-job | In the jobs namespace router, submit the authorized job once, keeping the outer parameters object separate from job-parameters. Return its job-instance ID and Retry-After guidance for a later independent Get call. | context-required |
| jobs_run-on-demand-item-job | Submit the explicitly authorized job once. After waiting at least the receipt's valid Retry-After guidance, hand its workspaceId, itemId, and trusted jobInstanceId to jobs_get-item-job-instance in a separate call; do not use operation-state or operation-result tools. | context-required |
| jobs_update-item-schedule | Update schedule eeeeeeee-eeee-4eee-8eee-eeeeeeeeeeee for job type DefaultJob on item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with enabled false and full configuration {"type":"Cron","startDateTime":"2030-01-01T00:00:00Z","endDateTime":"2030-02-01T00:00:00Z","localTimeZoneId":"UTC","interval":10}. Obtain consent and send exactly one PATCH; disabling does not cancel running jobs. | none |
| jobs_update-item-schedule | Update only the explicitly selected schedule using my supplied workspace/item/schedule IDs, job type, enabled state, and full Daily, Weekly, or Monthly configuration. Do not infer timing or a time zone, fetch existing settings, merge a partial payload, or execute a job. | context-required |
| jobs_update-item-schedule | Before consenting to the approved enabled schedule update, confirm the calling identity and explain that the supplied past start may execute immediately and the schedule owner identifies its creator or last modifier. Cron is an interval in minutes, not a Unix expression. | context-required |
| jobs_update-item-schedule | Update this authorized schedule with my full configuration and static execution-data object, preserving opaque and future fields. If I omit execution-data, omit it from the request without promising preservation or clearing. Do not send JSON null, strip fields, or retry. | context-required |
| jobs_update-item-schedule | I only supplied a new enabled state. Ask me for the full intended configuration or a separately authorized List Schedules call; do not hide a Get/merge/PATCH inside Update. | context-required |
| jobs_update-item-schedule | In the jobs namespace router, update the authorized schedule with workspace-id, item-id, job-type, schedule-id, enabled, and configuration in parameters. Return Updated, not job success or cancellation; do not use the schedule ID as a job-instance or operation ID. | context-required |
| jobs_update-item-schedule | If HTTP 200 arrives but the update response is unreadable, too deep, or identifies a different schedule, report confirmed update with unusable metadata. If sending times out, report uncertainty. Keep valid Retry-After guidance and never resubmit blindly, poll, fetch results, or run/cancel a job. | context-required |
