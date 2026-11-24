# Start a new session: archive inventory and business case (as implemented)

The Discovery Service allows one active session per user and skill type. When the user chooses to start over, the
session archive tool closes both the inventory session and the business case session, in parallel, and waits up to
45 s (checking every 3 s) until neither blocks a new one.

```mermaid
sequenceDiagram
    autonumber
    actor U as User
    participant A as Cowork agent
    participant M as MCP server
    participant DI as Discovery Service (inventory)
    participant DB as Discovery Service (business case)

    U->>A: "Start a new session"
    A->>M: session_archive
    par Inventory
        M->>DI: GET /mcprestapi/sessions
        DI-->>M: sessions (skillType, sessionState)
        loop Each non-archived inventory session
            M->>DI: POST /mcprestapi/sessions/{id}/archive
        end
        loop Every 3 s, up to 45 s, until all are Archived
            M->>DI: GET /mcprestapi/sessions/{id}
            DI-->>M: sessionState
        end
    and Business case
        M->>DB: PUT /mcprestapi/sessions/{new id} (skillType BusinessCase)
        alt 201 Created: no active session
            Note over M,DB: Nothing to close
        else 409 ActiveSessionAlreadyExists (id)
            M->>DB: POST /mcprestapi/sessions/{active id}/archive
            loop Every 3 s, up to 45 s, until it no longer blocks
                M->>DB: PUT /mcprestapi/sessions/{new id} (probe for the active session)
                DB-->>M: 201, or 409 with the active session id
            end
        end
    end
    M-->>A: inventoryArchived, businessCaseArchived
    A-->>U: "Closed; you can start a new inventory"
```

The business case side has no session list, so a create probe finds the active session: a `409` carries its id, and a
`201` means none is active.
