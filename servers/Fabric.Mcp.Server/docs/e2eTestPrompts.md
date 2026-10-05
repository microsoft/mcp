# Fabric MCP Server tool selection prompts

These prompts validate tool selection. They do not imply that live Fabric calls, real OBO authorization, or recorded playback have been exercised.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_get-capacity | Get metadata for Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | Retrieve metadata for the specified capacity UUID. |
| core_get-capacity | Show the SKU, region, and state of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357. | Retrieve capacity metadata without reading billing or querying ARM. |
| core_get-capacity | What is the display name of Fabric capacity 96f3f0ff-4fe2-4712-b61b-05a456ba9357? | Retrieve the known capacity without listing or modifying capacities. |
