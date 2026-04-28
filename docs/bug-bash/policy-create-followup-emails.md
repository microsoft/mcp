# Azure Backup `policy create` follow-up emails

Two emails, one per workload area, requesting confirmation of supported policy shapes for the three remaining deferred MCP live tests.

> **Status update (2026-04-27):** The fourth originally-deferred test (`PolicyCreate_DppDisk_VaultTierMultiTierArchive_E2E`) has been **resolved**. After re-reading the Disk DPP manifest we dropped the unsupported `--archive-tier-*` and `--yearly-*` flags, added `--enable-vault-tier-copy` with Weekly/Monthly retention, and the renamed `PolicyCreate_DppDisk_VaultTierMultiTier_E2E` now records and plays back successfully (52/6/0). The validator now also rejects `--archive-tier-*` for **all** DPP datasources (no DPP workload supports ArchiveStore today) and `--yearly-retention-*` for AzureDisk (manifest allows only Daily/Weekly/Monthly tags).

---

## Email 1 — RSV IaaSVM (Enhanced V2): Weekly + multi-tier + archive

**To:** Azure Backup IaaSVM PM / RSV Backend
**Subject:** RSV IaaSVMv2 — supported policy shape for Weekly schedule + Weekly/Monthly/Yearly retention + archive (`TieringPolicy`)

Hi team,

We are wiring up `azmcp azurebackup policy create` for the IaaSVM (Enhanced V2) backup management type and one shape is consistently rejected by the RSV control plane. Before we hand-craft the JSON for the n-th time we would like the canonical shape from you.

### What we are sending

- Backup management type: `AzureIaasVM`
- Policy type: `V2` (Enhanced)
- Schedule: `WeeklySchedule` on Sunday at 02:00 UTC
- Retention:
  - `WeeklyRetentionSchedule` — 4 weeks, Sunday
  - `MonthlyRetentionSchedule` — 12 months, day 1, Weekly format (Sunday, Week 1)
  - `YearlyRetentionSchedule` — 5 years, January, day 1, Weekly format (Sunday, Week 1)
- `TieringPolicy.ArchivedRP`: `TierAfter`, `Duration=90`, `DurationType=Days`
- We send no `DailyRetentionSchedule` (because the schedule is Weekly).

### What the service returns

```
HTTP 400 BMSUserErrorInvalidPolicyInput
"Input for create or update policy is not in proper format. Please check format
 of parameters like schedule time, schedule days, retention time and retention
 days"
```

The same vault accepts our `Daily + Daily/Weekly/Monthly/Yearly + Archive` shape, so this is specific to the Weekly+LTR+Archive combination on V2-Enhanced.

### What we need from you

1. A confirmed JSON sample (or a reference policy from Az PowerShell / Portal export) for **V2 Enhanced + Weekly + Weekly/Monthly/Yearly retention + Archive on Snapshot or Archive on Vault**, against any test vault.
2. Confirmation of:
   - whether `WeeklySchedule` requires a non-empty `DailySchedule` placeholder (we currently omit it),
   - whether `TieringPolicy` belongs at the policy root or under a `SubProtectionPolicy`,
   - whether `WeeklyRetentionFormat.DaysOfTheWeek` must equal `WeeklySchedule.ScheduleRunDays`,
   - whether `MonthlyRetentionSchedule.RetentionScheduleFormatType` must be `Weekly` (not `Daily`) for a Weekly-scheduled policy.
3. The minimum API version required for this shape (we are on `2024-04-01`).

We are happy to share request/response captures or run a fresh repro against any vault you point us at.

Thanks!

---

## Email 2 — RSV AzureWorkload (SQL in Azure IaaSVM, Windows)

**To:** Azure Backup SQL/Workload PM / RSV Backend
**Subject:** RSV SQL in Azure IaaSVM (Windows) — two unsupported policy shapes (Full+Differential+Log retention; Full Weekly + Archive on Full sub-policy)

Hi team,

Same context — we are exposing `azmcp azurebackup policy create` for the **SQL in Azure IaaSVM (Windows)** workload (`workloadType=SQLDataBase`, `backupManagementType=AzureWorkload`) and have **two** policy shapes the RSV API rejects. Both are blockers for live test coverage of common policy patterns. Please confirm the canonical JSON for each.

> Note: there is no `MSSQL` workload type in RSV — this is `SQLDataBase` under `AzureWorkload`, i.e. SQL Server running inside an Azure VM. We are not asking about Azure SQL Database / Managed Instance.

### Shape A — Full + Differential + Log with Diff retention >= Log retention

**What we send**

- Workload type: `SQLDataBase` (SQL in Azure IaaSVM, Windows), backup management type: `AzureWorkload`
- `SubProtectionPolicy[]`:
  - `Full`: `Weekly` schedule, Sunday 02:00; Weekly retention 4 weeks Sunday.
  - `Differential`: `Weekly` schedule, **Wed,Fri** (no overlap with Full); retention 30 days.
  - `Log`: `LogSchedulePolicy` every 60 minutes; retention 15 days.
- We already enforce on the client side:
  - Full and Differential never share a day;
  - Log retention < Differential retention.

**What the service returns**

```
HTTP 400 BMSUserErrorPolicyRetentionInvalid
"The retention duration provided in the backup policy is not supported.
 Verify the supported retention durations for different backup types at
 https://aka.ms/policysupport"
```

### Shape B — Full Weekly + Monthly retention + Archive (`TieringPolicy`) on the Full sub-policy

**What we send**

- Workload type: `SQLDataBase` (SQL in Azure IaaSVM, Windows), backup management type: `AzureWorkload`
- `SubProtectionPolicy[]`:
  - `Full`: `Weekly` schedule, Sunday 02:00; Weekly retention 4 weeks Sunday + Monthly retention 12 months day 1 (Weekly format Sunday Week 1); `TieringPolicy.ArchivedRP = TierAfter, 90 Days`.
  - `Log`: 60-minute frequency, 15-day retention.
- No `DailyRetentionSchedule` (schedule is Weekly).

**What the service returns**

```
HTTP 400 BMSUserErrorInvalidPolicyInput
"Input for create or update policy is not in proper format..."
```

### What we need from you

1. For **Shape A**: a confirmed retention duration matrix for SQL `Full` (Weekly) + `Differential` + `Log`. Specifically:
   - allowed range for `Differential.RetentionDuration` when `Full.SchedulePolicy = WeeklySchedule`,
   - whether `Log.RetentionDuration` must be `<= Differential.RetentionDuration` AND `<= Full.RetentionDuration`,
   - whether the `Differential` sub-policy must use `SimpleRetentionPolicy` or `LongTermRetentionPolicy`,
   - any constraint linking `Differential.SchedulePolicy.ScheduleRunDays` to `Full.SchedulePolicy.ScheduleRunDays`.
2. For **Shape B**: a confirmed JSON sample for SQL with `TieringPolicy` attached to the `Full` sub-policy (or guidance that it must live elsewhere — e.g., on a different sub-policy or at the root).
3. Whether the `MakePolicyConsistent` flag (or `policySubType=Enhanced`) is required for either shape.
4. Az PowerShell or Portal reference policies for both shapes against any SQL-protected vault.

Captures and repros available on request — this vault has other SQL policies succeeding so we have a clean side-by-side.

Thanks!

---

### Summary of the three blocked tests

| Workload (test) | Failure code |
| --- | --- |
| RSV IaaSVMv2 — `PolicyCreate_RsvVm_WeeklyMultiTierWithArchive_E2E` | `BMSUserErrorInvalidPolicyInput` |
| RSV SQL in Azure IaaSVM (Windows) — `PolicyCreate_RsvSql_FullLogDiff_E2E` | `BMSUserErrorPolicyRetentionInvalid` |
| RSV SQL in Azure IaaSVM (Windows) — `PolicyCreate_RsvSql_WithArchiveTier_E2E` | `BMSUserErrorInvalidPolicyInput` |

All three have working sibling tests against the same vaults, so vault setup / RBAC / preview-feature flags are not the issue — these are policy-shape questions only.

### Previously resolved

| Workload (test) | Resolution |
| --- | --- |
| DPP AzureDisk — `PolicyCreate_DppDisk_VaultTierMultiTier_E2E` (renamed from `*Archive*`) | Dropped unsupported `--archive-tier-*` and `--yearly-*` per DPP manifest. Added `--enable-vault-tier-copy` with Weekly/Monthly retention. Now passing (52/6/0). Validator rejects archive flags for all DPP workloads and yearly retention for AzureDisk. |