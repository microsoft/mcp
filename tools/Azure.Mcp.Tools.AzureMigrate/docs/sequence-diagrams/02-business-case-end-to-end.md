# Business case: create to ROI report (as implemented)

How a business case runs today: 2 stages (`Assessment`, then `HaSkill`), tracked by the agent with the wait tool and
finished with one status call that shows the report card. The MCP server keeps no run state: every call carries the
`nextStatusParameters` of the previous result. The only memory is a per-process start gate, so the agent's wait tool
and a card's status call never start the final stage twice. Background and decisions:
[business-case-plan.md](../business-case-plan.md).

Hosts: inventory Discovery Service on stamp04, business case Discovery Service on stamp03, Business Case Service (BCS)
and HA dataplane, and the Assessment Service (AS).

```mermaid
sequenceDiagram
    autonumber
    actor U as User
    participant A as Cowork agent
    participant M as MCP server
    participant DI as Discovery Service (inventory)
    participant AS as Assessment Service
    participant B as BCS
    participant H as HA dataplane
    participant DB as Discovery Service (business case)
    participant S as Blob storage (SAS)
    participant W as Report card

    U->>A: "Create a business case"
    A->>M: businesscase_create(name, region, settings, session-id or inventory-sas-uri)
    opt No inventory SAS URL given
        M->>DI: POST export?artifacts=internal (offline zip)
        DI-->>M: inventory SAS URL
    end
    M->>AS: PUT machineAssessmentsFromZip/{name}
    AS-->>M: 202 asOperationId
    M-->>A: Running, stage 1 of 2, nextStatusParameters, "call the wait tool"
    A-->>U: "Business case started, usually 2 to 5 minutes"

    loop businesscase_wait, each call up to 150 s, one pass every 8 s, until isCompleted
        A->>M: businesscase_wait(nextStatusParameters)
        alt Stage 1 (no ds-session-id yet)
            M->>AS: GET operations/{asOperationId}
            AS-->>M: Running, or Succeeded + output SAS
            opt Assessment succeeded
                M->>B: POST /dataplane/onprem-tco (synchronous)
                B-->>M: TCO manifest SAS
                M->>H: POST /dataplane/ha/evaluate (synchronous)
                H-->>M: HA manifest SAS
                M->>DB: PUT session bc-{hash(asOperationId)} (skillType BusinessCase)
                alt 409: user already has an active business case session
                    M->>DB: PUT the active session (read its state)
                    Note over M,DB: Still running an earlier business case: stay in stage 1 and retry later
                end
                M->>DB: POST invoke
                DB-->>M: dsOperationId + write SAS
                M->>S: PUT mini zip (manifest.json with the 4 SAS URLs)
                Note over DB,S: The Discovery Service starts the business case skill, which reads the mini zip and the 4 SAS URLs
                Note over M: Stage 2 starts: result adds ds-session-id and ds-operation-id
            end
        else Stage 2 (ds-session-id and ds-operation-id)
            M->>DB: GET operationstatus/{dsOperationId}
            DB-->>M: Running / Succeeded / Failed
        end
        M-->>A: status, stage, isCompleted, nextStatusParameters
    end

    A->>M: businesscase_status(nextStatusParameters), once
    M->>DB: GET operationstatus/{dsOperationId}
    DB-->>M: Succeeded
    M->>DB: GET export?artifacts=external
    DB-->>M: ROI.md SAS
    M->>DB: GET export?artifacts=internal
    DB-->>M: artifacts ZIP SAS
    M->>S: GET ROI.md (server side, shown inline)
    S-->>M: ROI.md
    M-->>A: Succeeded, report, artifacts link
    A-->>U: ROI summary
    A->>W: Tool result
    W-->>U: ROI.md preview + "Download artifacts"
```

Notes:

- Transient errors (no response, 408, 429, 5xx) leave the business case `Running`; the next pass repeats the step.
  BCS and HA reuse earlier results for identical requests, so repeating them is safe.
- Any other error finishes the business case `Failed` with `failedStep` (for example `onPremTco`, `haEvaluate`,
  `invokeSession`, `uploadInput`).
- With `AZUREMIGRATE_HA_MOCK=true`, stage 1 ends by reading the HA manifest's summary and finishes `Succeeded` without
  the Discovery Service session.
- [01](01-business-case-roi-client-preview.md) shows the planned change: the card reads ROI.md in the browser instead of
  the server.
