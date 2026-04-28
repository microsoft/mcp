# Azure Backup Policy Create — SDK & API Improvement Proposals

**To:** Azure Backup SDK Team / RSV Backend / DPP Backend / Swagger Owners
**Cc:** Azure Backup PM
**Subject:** Policy Create SDK & API improvements — learnings from MCP tool integration (28 files, 55 live tests, 4 Kusto-debugged failures)

Hi team,

We just landed `azmcp azurebackup policy create` covering all RSV and DPP workloads ([PR #2504](https://github.com/microsoft/mcp/pull/2504)). During integration we hit multiple service-side validations that return opaque errors, forcing us to build a client-side `PolicyCreateValidator` with 15+ rules just to give users actionable messages. Below are the specific issues and proposed SDK/swagger/service improvements, organized by impact.

---

## 1. Error messages that hide the actual validation failure

**Problem:** `BMSUserErrorInvalidPolicyInput` is a catch-all for ~10 different validation failures. The API returns:
> "Input for create or update policy is not in proper format. Please check format of parameters like schedule time, schedule days, retention time and retention days"

…when the *actual* server-side error (visible only in Kusto `TraceLogMessageAll`) is something specific like:
> "DaysOfTheWeek of retention schedule must be same of backup schedule DaysOfTheWeek"

**Evidence:** Request `ea296f15-e026-40c5-9391-7c0869a43f28` — Kusto showed `WeeklyRetentionSchedule.ValidateWithBackupScheduleDOW` threw `ArgumentException` with the exact field mismatch, but the API response lost this detail during the `CloudInvalidInputError → BMSUserErrorInvalidPolicyInput` error mapping in `FMComponent`.

**Ask:** Surface the inner validation message in the API error response `details` array. The service already has the specific message — it's just not propagated to the HTTP response.

---

## 2. Error message template with unpopulated parameters

**Problem:** `UserErrorLogRetentionNotInValidRangeInPolicy` returns:
> "Log retention not present in allowed range (minRange **NO_PARAM** and maxRange **NO_PARAM**)"

The `NO_PARAM` placeholders are never filled with the actual min/max values.

**Ask:** Populate the `minRange` and `maxRange` template parameters (e.g., "minRange 7 days and maxRange 35 days") so callers know the valid range without trial-and-error.

---

## 3. SQL Differential: undocumented single-day constraint

**Problem:** SQL `Differential` sub-policy only supports exactly 1 day per week. Passing `["Wednesday","Friday"]` in `scheduleRunDays` gets rejected with the generic `BMSUserErrorInvalidPolicyInput`. No documentation mentions this constraint.

**Ask:**
- Swagger: Add `maxItems: 1` on `Differential.schedulePolicy.scheduleRunDays` for `AzureWorkload` policies.
- Service: Return a specific error like `UserErrorDifferentialScheduleMultipleDaysNotSupported`.

---

## 4. Weekly schedule: retention days-of-week must match schedule — not documented

**Problem:** When `scheduleRunFrequency=Weekly` with `scheduleRunDays=["Monday"]`, the `WeeklyRetentionSchedule.daysOfTheWeek` MUST equal `["Monday"]`. If you set `["Sunday"]` (a reasonable default), the service rejects with the generic error. This constraint is enforced in `WeeklyRetentionSchedule.ValidateWithBackupScheduleDOW` but not documented in swagger or SDK.

**Ask:**
- Swagger: Add a `description` on `weeklySchedule.daysOfTheWeek`: "Must match scheduleRunDays when scheduleRunFrequency is Weekly."
- SDK: Add a `[ValidationAttribute]` or XML doc on `WeeklyRetentionSchedule.DaysOfTheWeek` noting the constraint.

---

## 5. Monthly/Yearly retention format: Weekly schedule requires relative format

**Problem:** When `scheduleRunFrequency=Weekly`, monthly and yearly retention MUST use `RetentionScheduleFormatType=Weekly` (relative: week-of-month + days-of-week). Using `Daily` format (absolute: days-of-month) is silently rejected with the generic error. No documentation mentions this constraint.

**Ask:**
- Service: Return `UserErrorRetentionFormatInvalidForWeeklySchedule` with message: "Monthly/Yearly retention must use Weekly format (week-of-month + days-of-week) when backup schedule is Weekly."
- Swagger: Add constraint description on `retentionScheduleFormatType`.

---

## 6. DPP: No client-accessible manifest for per-datasource constraints

**Problem:** Each DPP datasource has a manifest defining `allowedRetentionTagNames`, `allowedFirstTargetStores`, `storeConstraints` (which stores are supported), and `allowedScheduledTriggerFrequencies`. But this manifest is only available server-side. Callers have no way to know that:
- AzureDisk doesn't support `Yearly` retention or `ArchiveStore`
- No DPP datasource supports `ArchiveStore` today
- AzureDisk requires `OperationalStore` as first target

We had to hard-code all of this in our validator.

**Ask:** Expose a `GET /datasources/{type}/manifest` (or include it in `get-default-policy-template`) so clients can validate constraints programmatically instead of hard-coding.

---

## 7. SQL Log retention: minimum not discoverable

**Problem:** SQL Log retention has a minimum of 7 days, but this is only discoverable by getting `UserErrorLogRetentionNotInValidRangeInPolicy` (with `NO_PARAM` values). No swagger annotation, SDK doc, or API introspection endpoint exposes this constraint.

**Ask:** Add `minimum: 7` and `maximum: 35` (or whatever the actual bounds are) to the swagger definition for `Log.retentionPolicy.retentionDuration.count` when `durationType=Days`.

---

## 8. VM policy-sub-type `Enhanced` required for archive — not documented

**Problem:** VM policies with `TieringPolicy` (archive tier) require `policySubType=Enhanced` (V2). Without it, the policy is created as Standard V1 which doesn't support tiering. No error is returned — the tiering is silently ignored.

**Ask:** Service: reject `TieringPolicy` on Standard (V1) policies with a specific error, or auto-promote to V2 Enhanced when tiering is requested.

---

## Summary of client-side rules we built to compensate

| Rule | Service error it prevents |
|---|---|
| SQL Diff must be exactly 1 day | `BMSUserErrorInvalidPolicyInput` (generic) |
| SQL Log retention ≥ 7 days | `UserErrorLogRetentionNotInValidRangeInPolicy` (NO_PARAM) |
| SQL Log < Diff retention | `UserErrorLogRetentionMoreThanDiffRetentionInPolicy` |
| Weekly retention DOW must match schedule | `BMSUserErrorInvalidPolicyInput` (generic) |
| DPP: reject archive for all datasources | `BMSUserErrorInvalidInput` (generic) |
| AzureDisk: reject Yearly retention | `BMSUserErrorInvalidInput` (generic) |
| Full/Diff no day overlap | `BMSUserErrorInvalidPolicyInput` (generic) |
| Weekly schedule requires --schedule-days-of-week | SDK null-ref or generic error |
| Continuous DPP rejects schedule/retention flags | `BMSUserErrorInvalidInput` (generic) |

Every one of these could be a service-side improvement that benefits all callers (Az CLI, PowerShell, Portal, MCP, third-party tools), not just us.

Happy to share request IDs, Kusto queries, and the full JSON payloads for any of these. The PR with all the workarounds is at [microsoft/mcp#2504](https://github.com/microsoft/mcp/pull/2504).

Thanks!
