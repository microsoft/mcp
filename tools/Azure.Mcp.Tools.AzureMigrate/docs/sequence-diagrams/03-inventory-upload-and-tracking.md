# Inventory: upload and tracking (as implemented)

How an inventory is created or refined today. The upload panel puts the file straight into blob storage from the browser
(the storage CORS rule must allow `PUT`; see [step 7 of the deployment guide](../inventory-deployment.md)). The agent
waits with the wait tool, then calls the status tool once so the summary card appears together with its reply. The
panel itself no longer polls. Background: [inventory-tool-guidance-plan.md](../inventory-tool-guidance-plan.md).

```mermaid
sequenceDiagram
    autonumber
    actor U as User
    participant A as Cowork agent
    participant P as Upload panel
    participant M as MCP server
    participant D as Discovery Service (inventory)
    participant S as Blob storage (SAS)
    participant C as Summary card

    U->>A: Inventory file, or a text change
    A->>M: inventory_createorupdate
    alt File
        M->>D: PUT session (create, or reuse the active one on 409) + POST invoke
        D-->>M: operationId + write SAS
        M-->>A: sessionId, operationId, uploadCommand, "call the wait tool"
        A->>P: Tool result (renders the panel)
        U->>P: Chooses the file
        P->>S: PUT blob (x-ms-blob-type BlockBlob, needs CORS PUT)
        S-->>P: 201 Created
        P-->>U: "Uploaded. Copilot posts the result when it is ready"
    else Text only (existing session)
        M->>D: POST invoke (inputType Text, message)
        D-->>M: operationId (no SAS)
        M-->>A: sessionId, operationId, "call the wait tool"
    end

    loop inventory_wait, each call up to 150 s, until isCompleted
        A->>M: inventory_wait(sessionId, operationId)
        loop status passes of up to 25 s (checks after 2, 4, 8 and 11 s)
            M->>D: GET operationstatus/{operationId}
            D-->>M: Running / Succeeded / Failed
        end
        M-->>A: status, isCompleted
    end

    A->>M: inventory_status(sessionId, operationId), once
    M->>D: GET operationstatus/{operationId}
    D-->>M: Succeeded + summary
    M-->>A: summary
    A-->>U: Reply (servers, changes, answers)
    A->>C: Tool result
    C-->>U: Inventory summary card
```

The agent writes nothing while it waits, so no progress text reaches the chat after the panel already shows the upload.
