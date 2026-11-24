# Sequence diagrams

Sequence diagrams for the Azure Migrate (Azure Catalyst) MCP tools. Each file holds one flow as a Mermaid diagram,
which GitHub renders. Number new files in order (`02-…`, `03-…`) and add them to this list.

| # | Flow | State |
|---|---|---|
| 01 | [Business case ROI.md preview in the browser](01-business-case-roi-client-preview.md) | Implemented (needs CORS `GET`) |
| 02 | [Business case: create to ROI report](02-business-case-end-to-end.md) | Implemented |
| 03 | [Inventory: upload and tracking](03-inventory-upload-and-tracking.md) | Implemented |
| 04 | [Start a new session: archive inventory and business case](04-archive-session.md) | Implemented |
| 05 | [Inventory: refine with a text change](05-inventory-refine-text.md) | Implemented |

Earlier diagrams not carried forward:

- **Assessment with a form (elicitation)** and **HA assessment in 4 sections**: replaced by the business case flow in
  [02](02-business-case-end-to-end.md). The assessment tools are no longer exposed, and no form is shown.
- **Upload helper app** (`azmigrate-upload://`) and its install flow: a spike that was rejected; see the decision in
  [inventory-tool-guidance-plan.md](../inventory-tool-guidance-plan.md). The upload panel or azcopy in
  [03](03-inventory-upload-and-tracking.md) is used instead.

Keep these in step with the code: when a change alters a flow, update its diagram in the same pull request. Design
documents link here instead of holding their own copies.
