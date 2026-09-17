# Fabric MCP End-to-End Test Prompts

These prompts cover the Fabric tools listed below. For live execution, replace the workspace and
item placeholders with an approved fixture that the caller can read. Each prompt supplies both
required IDs and should use the metadata tool without listing items, reading definitions, or making
changes. These prompts are not evidence of a completed live test or description evaluation.

## Core

| Tool Name | Test Prompt | Interaction |
|:----------|:------------|:------------|
| core_get-item | Get the metadata for Fabric item '<item-id>' in workspace '<workspace-id>' without reading its data or definition. | none |
| core_get-item | Show the display name, description, and type of Fabric item '<item-id>' in workspace '<workspace-id>'. | none |
| core_get-item | Inspect the existing Fabric item '<item-id>' in workspace '<workspace-id>' and return only its generic metadata. Do not modify it. | none |
