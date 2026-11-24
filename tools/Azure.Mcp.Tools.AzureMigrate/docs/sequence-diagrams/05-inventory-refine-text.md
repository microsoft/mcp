# Inventory: refine with a text change (as implemented)

How the user corrects or extends an existing inventory in plain words, for example "e2e-old-01 is retired, mark it
powered off". Replaces the earlier "refine inventory" diagram. Differences from that diagram:

- The MCP server no longer PUTs the session first: it invokes the existing session directly, so an unknown session
  fails with `404` instead of starting an empty inventory. A session with no completed import fails with `400`.
- The agent tracks the change itself with the wait tool. The user no longer has to ask for the status.

```mermaid
sequenceDiagram
    autonumber
    actor U as User
    participant A as Cowork agent
    participant M as MCP server
    participant D as Discovery Service (inventory)
    participant T as TEE workflow + Foundry agent
    participant C as Summary card

    U->>A: "e2e-old-01 is retired ... update to powered off"
    A-->>U: Approval card (session-id, message, text-only=true), if the host asks
    U->>A: Approve
    A->>M: inventory_createorupdate(session-id, message, text-only=true)
    M->>D: POST /sessions/{id}/invoke {inputType: Text, message}
    alt Session busy with an earlier request
        D-->>M: Conflict
        M-->>A: Busy, "try again once it finishes"
    else Accepted
        D->>T: InvokeSkillWorkflow (no SAS, no upload wait)
        D-->>M: 202 operationId (no SAS)
        M-->>A: sessionId, operationId, "call the wait tool"
        T-->>D: Updated inventory and reply
        loop inventory_wait, until isCompleted
            A->>M: inventory_wait(sessionId, operationId)
            M->>D: GET /sessions/{id}/operationstatus/{opId}
            D-->>M: Running / Succeeded
            M-->>A: status, isCompleted
        end
        A->>M: inventory_status(sessionId, operationId), once
        M->>D: GET /sessions/{id}/operationstatus/{opId}
        D-->>M: Succeeded + reply
        M-->>A: Summary and reply
        A-->>U: What changed, answers to questions
        A->>C: Tool result
        C-->>U: Inventory summary card
    end
```
