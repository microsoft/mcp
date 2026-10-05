# Fabric MCP Server tool selection prompts

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_get-capacity | Get metadata for Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | Show the SKU, region, and state of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | none |
| core_get-capacity | What is the display name of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357? | none |
| core_get-workspace | Get metadata for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | Retrieve metadata for the specified workspace. |
| core_get-workspace | Show the capacity, domain, and workspace identity for Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa. | Retrieve available workspace metadata without querying related resources. |
| core_get-workspace | Get Fabric workspace aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa with workspace-specific API and OneLake endpoints. | Retrieve workspace metadata with the endpoint preference set to true; do not contact returned endpoints. |
| core_list-capacities | List the Fabric capacities where I am an administrator or contributor. | Return one page of accessible capacity metadata without modifying capacities. |
| core_list-capacities | Show one page of my accessible Fabric capacity IDs, display names, SKUs, regions, and states. | List capacity metadata through the Fabric Core API, not an ARM inventory. |
| core_list-capacities | Get the next page of Fabric capacities using continuation token ABCsMTAwMDAwLDA%3D from the previous response. | Retrieve one page using the unchanged token without following returned URIs. |
| core_list-workspaces | List the Microsoft Fabric workspaces I can access and show their workspace IDs, types, capacity, and domain metadata. | Read-only |
| core_list-workspaces | List my Fabric workspaces where I am an Admin or Member using the Core management API, not the OneLake storage listing. | Read-only |
| core_list-workspaces | List accessible Fabric workspaces and include their workspace-specific API endpoints. | Read-only |
| core_list-workspaces | Get the next page of accessible Fabric workspace management metadata using the continuation token from the preceding list, keeping the same role filter and endpoint preference. | Read-only |
