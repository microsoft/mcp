# Fabric MCP Server tool selection prompts

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

`none` means the prompt supplies the inputs needed for invocation, not that permission to mutate resources has been granted. Workspace creation and updates require separate authorization for live tests; offline tests substitute HTTP and credentials. Standard mutation confirmation still applies. The resource UUIDs below are examples, not provisioned test resources.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_create-workspace | Create a Microsoft Fabric workspace called Sales Planning. | none |
| core_create-workspace | Create a new Fabric workspace named Finance Sandbox with description Quarterly planning experiments. | none |
| core_create-workspace | Create a Fabric workspace called Capacity Analytics and assign it to existing capacity f4031b2e-318f-4a14-9a3e-103e9bcfc953 during creation. | none |
| core_create-workspace | Create a Microsoft Fabric workspace named Domain Analytics on capacity f4031b2e-318f-4a14-9a3e-103e9bcfc953 and assign it to domain 88d8f15b-5105-449b-98d3-681345f00326 in the same request. | none |
| core_get-capacity | Get metadata for Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | Show the SKU, region, and state of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | What is the display name of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357? | none |
| core_get-workspace | Get metadata for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | Retrieve metadata for the specified workspace. |
| core_get-workspace | Show the capacity, domain, and workspace identity for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | Retrieve available workspace metadata without querying related resources. |
| core_get-workspace | Get Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with workspace-specific API and OneLake endpoints. | Retrieve workspace metadata with the endpoint preference set to true; do not contact returned endpoints. |
| core_list-capacities | List the Fabric capacities where I am an administrator or contributor. | Return one page of accessible capacity metadata without modifying capacities. |
| core_list-capacities | Show one page of my accessible Fabric capacity IDs, display names, SKUs, regions, and states. | List capacity metadata through the Fabric Core API, not an ARM inventory. |
| core_list-capacities | Get the next page of Fabric capacities using continuation token ABCsMTAwMDAwLDA%3D from the previous response. | Retrieve one page using the unchanged token without following returned URIs. |
| core_list-items | List Fabric item metadata in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, including nested folders. | Returns one workspace inventory page with any continuation information. |
| core_list-items | List the Lakehouse items in Fabric workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb. | Applies the item-type filter to one metadata page. |
| core_list-items | Show only items directly in Fabric folder bbbbbbbb-1111-2222-3333-cccccccccccc within workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb, not its nested folders. | Uses the root folder and disables recursive listing. |
| core_list-items | Get the next page of Fabric items in workspace aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb using the continuation token from the previous listing and the same filters. | Requests exactly one additional metadata page. |
| core_list-workspaces | List the Microsoft Fabric workspaces I can access and show their workspace IDs, types, capacity, and domain metadata. | Read-only |
| core_list-workspaces | List my Fabric workspaces where I am an Admin or Member using the Core management API, not the OneLake storage listing. | Read-only |
| core_list-workspaces | List accessible Fabric workspaces and include their workspace-specific API endpoints. | Read-only |
| core_list-workspaces | Get the next page of accessible Fabric workspace management metadata using the continuation token from the preceding list, keeping the same role filter and endpoint preference. | Read-only |
| core_update-workspace | Rename Fabric workspace \<workspace-id> to 'Finance Analytics' without changing its description. | none |
| core_update-workspace | Set the description of Fabric workspace \<workspace-id> to 'Quarterly reporting' and leave its name unchanged. | none |
| core_update-workspace | Clear the description of Fabric workspace \<workspace-id> without renaming it. | none |
| core_update-workspace | Update Fabric workspace \<workspace-id> to the name 'Team Reporting' and description 'Shared reporting workspace'. | none |
