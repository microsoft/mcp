# Inventory: create from a file, upload and tracking (as implemented)

How a new inventory is created from a file. Replaces the earlier "create or update inventory" diagram, in which the
user ran azcopy and then asked for the status.

- **Upload panel** (when `AZMIGRATE_INVENTORY_WIDGET_DOMAINS` is set): the panel PUTs the file straight into blob
  storage from the browser. The storage CORS rule must allow `PUT`.
- **azcopy fallback** (no panel, or the browser upload is blocked): the user runs the returned `azcopy copy` command.

Either way, the agent then waits with the wait tool and calls the status tool once, so the summary card appears
together with its reply. Background: [inventory-tool-guidance-plan.md](../inventory-tool-guidance-plan.md).

```mermaid
sequenceDiagram
    autonumber
    actor U as User
    participant A as Cowork agent
    participant P as Upload panel
    participant M as MCP server
    participant D as Discovery Service (inventory)
    participant S as Blob storage (SAS)
    participant T as TEE workflow + Foundry agent
    participant C as Summary card

    U->>A: "Create my Azure Migrate inventory"
    A->>M: inventory_createorupdate(session-id?, message?)
    M->>D: PUT /mcprestapi/sessions/{id} (user token)
    alt New session
        D-->>M: 201 Created
    else Active session exists
        D-->>M: 409 ActiveSessionAlreadyExists (id)
        Note over M: Reuse that session id
    end
    M->>D: POST /sessions/{id}/invoke
    D->>T: Start InvokeSkillWorkflow
    D-->>M: 202 operationId + write-only SAS
    M-->>A: sessionId, operationId, uploadCommand, "call the wait tool"

    alt Upload panel
        A->>P: Tool result (renders the panel)
        U->>P: Chooses the file (up to 500 MB, within 30 min)
        P->>S: PUT blob (x-ms-blob-type BlockBlob, needs CORS PUT)
        S-->>P: 201 Created
        P-->>U: "Uploaded. Copilot posts the result when it is ready"
    else azcopy fallback
        A-->>U: azcopy copy "<file>" "<SAS>" (run within 30 min, up to 500 MB)
        U->>S: Runs azcopy on own machine
    end

    loop Every 30 s, up to 30 min
        T->>S: File uploaded?
    end
    T->>T: Run the Foundry agent on the file

    loop inventory_wait, each call up to 150 s, until isCompleted
        A->>M: inventory_wait(sessionId, operationId)
        loop Status passes of up to 25 s (checks after 2, 4, 8 and 11 s)
            M->>D: GET /sessions/{id}/operationstatus/{opId}
            D-->>M: Running / Succeeded / Failed
        end
        M-->>A: status, isCompleted
    end

    A->>M: inventory_status(sessionId, operationId), once
    M->>D: GET /sessions/{id}/operationstatus/{opId}
    D-->>M: Succeeded + result (or Failed + reason)
    M-->>A: Summary
    A-->>U: Reply (servers, changes, answers) or the failure reason
    A->>C: Tool result
    C-->>U: Inventory summary card
```

The agent writes nothing while it waits, so no progress text reaches the chat after the panel already shows the upload.
The status is `Running` both while the Discovery Service waits for the file and while it processes it.
