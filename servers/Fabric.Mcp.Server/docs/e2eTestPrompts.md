# Fabric MCP Server tool selection prompts
<!-- cspell:words pbism pbir TMDL -->

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

`none` means the prompt supplies the inputs needed for invocation, not that permission to mutate resources has been granted. Workspace creation and updates require separate authorization for live tests; offline tests substitute HTTP and credentials. Standard mutation confirmation still applies. The resource UUIDs below are examples, not provisioned test resources.

These prompts describe tool selection, not live test execution. Delete Item is tested only with mocked Fabric HTTP responses; never execute these destructive prompts against real Fabric during development or testing. Live Fabric, real OBO authorization, and recorded playback have not been exercised for this tool.

Delete Workspace likewise uses substituted HTTP and credentials only. Its prompts describe deletion of the specified workspace and the items under it, not authorization to execute a real deletion.

For every deletion prompt, explain the scope and request consent before sending one request. Omitted or false `hard-delete` uses the default behavior, with soft deletion dependent on item-type support; do not promise recovery or fall back to permanent deletion. Explicit true requests permanent deletion, which cannot be recovered and requires workspace Admin. Workspace deletion includes its items; do not promise recovery or permanent-deletion semantics, and do not prefetch, delete items individually, retry, or poll.

Workspace capacity assignment changes a live resource. Use only with an explicitly approved workspace, capacity, and identity; do not run these assignments as automated development checks.

Definition updates replace the complete definition of an existing item. Review the full selected parts and obtain authorization before a live update; these prompts are not permission to modify a workspace. Never upload only changed files, merge with a fetched definition, or retrieve an operation result after a no-result update succeeds.

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

Core-only filtering does not expose Jobs. Use `--namespace jobs --mode all` for job submission, cancellation, listing, and lookup alone,
or `--namespace core --namespace jobs --mode all` for workflows combining item/definition tools and job status.
The same namespace selection applies with `--mode namespace` or `--mode single`.

`context-required` means the caller must supply the required resource context and authorization. Sections and tool names are sorted alphabetically.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_assign-workspace-to-capacity | Submit a request to assign existing Fabric workspace \<workspace-id> to capacity \<capacity-id>; return acceptance without waiting for completion. | context-required |
| core_assign-workspace-to-capacity | Move workspace \<workspace-id> to Fabric capacity \<capacity-id> with one submission only, without polling or retries. | context-required |
| core_assign-workspace-to-capacity | Assign workspace \<workspace-id> to capacity \<capacity-id> and return the requested IDs with an accepted/pending state, not a claim that assignment finished. | context-required |
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
| core_list-items | List Fabric item metadata in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, including nested folders. | none |
| core_list-items | List the Lakehouse items in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. | none |
| core_list-items | Show only items directly in Fabric folder bbbbbbbb-1111-2222-3333-cccccccccccc within workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, not its nested folders. | none |
| core_list-items | Get the next page of Fabric items in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb using the continuation token from the previous listing and the same filters. | context-required |
| core_list-workspaces | List the Microsoft Fabric workspaces I can access and show their workspace IDs, types, capacity, and domain metadata. | none |
| core_list-workspaces | List my Fabric workspaces where I am an Admin or Member using the Core management API, not the OneLake storage listing. | none |
| core_list-workspaces | List accessible Fabric workspaces and include their workspace-specific API endpoints. | none |
| core_list-workspaces | Get the next page of accessible Fabric workspace management metadata using the continuation token from the preceding list, keeping the same role filter and endpoint preference. | context-required |
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
| jobs_get-item-job-instance | Read job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc for item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa once, without polling. | none |
| jobs_get-item-job-instance | Show the status, invocation type, and available start and end times for job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc of item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not fetch job results. | none |
| jobs_get-item-job-instance | Get safe failure metadata and any Retry-After guidance for job instance cccccccc-cccc-4ccc-8ccc-cccccccccccc of item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not start, retry, cancel, or schedule a job. | none |
| jobs_get-item-job-instance | Use the workspace, item, and job-instance UUIDs I supplied to read the latest snapshot once. Preserve a future status string instead of treating it as successful completion. | context-required |
| jobs_get-item-job-instance | Check that existing item's job instance using its saved job-instance ID, not the generic Fabric operation-state or operation-result tools. Return the snapshot without waiting. | context-required |
| jobs_get-item-job-instance | With the server configured using --namespace core --namespace jobs --mode all, use the workspace and item UUIDs returned by core_get-item and the existing job-instance UUID I supplied to read its status once. Do not discover or start a job. | context-required |
| jobs_list-item-job-instances | List one page of job instances for item bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb in workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. Do not fetch individual jobs or another page automatically. | none |
| jobs_list-item-job-instances | Show one page of that item's jobs with statuses, available UTC timing, and safe failure metadata. Do not treat HTTP success as job success or assume the page is full history. | context-required |
| jobs_list-item-job-instances | Fetch the next page using the unchanged continuation token from the previous response and the same explicit workspace/item UUIDs. Do not use a continuation URL or change filters. | context-required |
| jobs_list-item-job-instances | Read one page for my supplied workspace/item UUIDs. Preserve all final-page rows when continuation metadata is absent or null; if a token is present, return it without fetching another page. | context-required |
| jobs_list-item-job-instances | In read-only Jobs mode, list one page for the workspace and item UUIDs I supplied, then stop. More than 100 active rows must not be capped locally. | context-required |
| jobs_list-item-job-instances | List one page of jobs for this known item; I will explicitly select a returned job-instance ID for a separate Get. Do not infer the ID of an ambiguous Run submission or authorize cancellation from the listing. | context-required |
| jobs_run-on-demand-item-job | Submit exactly one explicitly authorized job of type \<job-type> for item \<item-id> in workspace \<workspace-id>. Return the accepted receipt without polling, waiting, fetching results, or retrying. | context-required |
| jobs_run-on-demand-item-job | Run the authorized item job once with the execution-data JSON and job-parameters JSON array I supplied. Preserve omitted fields and opaque values; do not strip parameters or resubmit if Fabric reports FeatureNotAvailable. | context-required |
| jobs_run-on-demand-item-job | Submit this authorized job with no optional body. If Location is missing, invalid, or identifies only a collection, report accepted but not trackable; never invent an ID or use x-ms-operation-id as the job-instance ID. | context-required |
| jobs_run-on-demand-item-job | In the jobs namespace router, submit the authorized job once, keeping the outer parameters object separate from job-parameters. Return its job-instance ID and Retry-After guidance for a later independent Get call. | context-required |
| jobs_run-on-demand-item-job | Submit the explicitly authorized job once. After waiting at least the receipt's valid Retry-After guidance, hand its workspaceId, itemId, and trusted jobInstanceId to jobs_get-item-job-instance in a separate call; do not use operation-state or operation-result tools. | context-required |
