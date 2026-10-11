# Fabric MCP Server tool selection prompts

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

`none` means the prompt supplies the inputs needed for invocation, not that permission to mutate resources has been granted. Workspace creation and updates require separate authorization for live tests; offline tests substitute HTTP and credentials. Standard mutation confirmation still applies. The resource UUIDs below are examples, not provisioned test resources.

These prompts describe tool selection, not live test execution. Delete Item is tested only with mocked Fabric HTTP responses; never execute these destructive prompts against real Fabric during development or testing. Live Fabric, real OBO authorization, and recorded playback have not been exercised for this tool.

Delete Workspace likewise uses substituted HTTP and credentials only. Its prompts describe deletion of the specified workspace and the items under it, not authorization to execute a real deletion.

For every deletion prompt, explain the scope and request consent before sending one request. Omitted or false `hard-delete` uses the default behavior, with soft deletion dependent on item-type support; do not promise recovery or fall back to permanent deletion. Explicit true requests permanent deletion, which cannot be recovered and requires workspace Admin. Workspace deletion includes its items; do not promise recovery or permanent-deletion semantics, and do not prefetch, delete items individually, retry, or poll.

Workspace capacity assignment changes a live resource. Use only with an explicitly approved workspace, capacity, and identity; do not run these assignments as automated development checks.

`context-required` means the caller must supply the required resource context and authorization. Sections and tool names are sorted alphabetically.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_assign-workspace-to-capacity | Submit a request to assign existing Fabric workspace \<workspace-id> to capacity \<capacity-id>; return acceptance without waiting for completion. | context-required |
| core_assign-workspace-to-capacity | Move workspace \<workspace-id> to Fabric capacity \<capacity-id> with one submission only, without polling or retries. | context-required |
| core_assign-workspace-to-capacity | Assign workspace \<workspace-id> to capacity \<capacity-id> and return the requested IDs with an accepted/pending state, not a claim that assignment finished. | context-required |
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
| core_update-workspace | Rename Fabric workspace \<workspace-id> to 'Finance Analytics' without changing its description. | none |
| core_update-workspace | Set the description of Fabric workspace \<workspace-id> to 'Quarterly reporting' and leave its name unchanged. | none |
| core_update-workspace | Clear the description of Fabric workspace \<workspace-id> without renaming it. | none |
| core_update-workspace | Update Fabric workspace \<workspace-id> to the name 'Team Reporting' and description 'Shared reporting workspace'. | none |
