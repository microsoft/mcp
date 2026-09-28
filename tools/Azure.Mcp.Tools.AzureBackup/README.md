# Azure Backup MCP Toolset

The Azure Backup toolset exposes backup operations through `azmcp azurebackup` for Recovery Services vaults (RSV) and Backup vaults (DPP). Individual commands have workload-specific support. See the [command reference](../../servers/Azure.Mcp.Server/docs/azmcp-commands.md#policy), [end-to-end test prompts](../../servers/Azure.Mcp.Server/docs/e2eTestPrompts.md#azure-backup), and [architecture](docs/architecture.md).

## Updating an existing backup policy

`azmcp azurebackup policy update` requires `--subscription`, `--resource-group`, `--vault`, and `--policy`. The policy must already exist. Use `--vault-type rsv` to select Recovery Services explicitly; policy update does not support DPP vaults.

RSV Azure VM policies support Daily/Weekly schedules, Hourly schedules on **existing Enhanced (V2) policies**, long-term retention, instant recovery, snapshot consistency, archive tiering, and resource tags. Other RSV workloads, including SQL, SAP HANA, and Azure Files, accept only the legacy `--schedule-time` and `--daily-retention-days` update settings; VM-only options are rejected.

### Merge behavior

Updates merge into the fetched policy rather than rebuilding it from policy-create defaults. Unmentioned settings modeled by the SDK, including unmodified retention tiers, are preserved subject to the schedule reconciliation rules below. Preservation of unknown wire fields is not guaranteed. Supplying no update settings is a no-op. Existing retention times are preserved on retention-only or unrelated updates; an explicit schedule change reconciles retention times with the resulting schedule and removes incompatible schedule branches.

`--policy-tags key=value,key2=value2` merges resource tags case-insensitively: supplied keys are added or updated, and unmentioned tags remain. The merged resource can contain at most 50 tags. Updating `--instant-rp-resource-group` changes only the instant recovery resource-group **name prefix**, preserving its existing suffix.

### Eleven added VM-only options

These options extend the existing schedule, time-zone, and daily/weekly/monthly/yearly retention update options. Values below apply to **policy update**, not necessarily policy create.

| Option | Update behavior |
|:-------|:----------------|
| `--hourly-interval-hours` | `4`, `6`, `8`, or `12` hours; requires an existing Enhanced policy and an effective Hourly schedule. |
| `--hourly-window-start-time` | Local `HH:mm` converted to UTC using supplied `--time-zone`, otherwise the existing policy time zone, otherwise UTC. Uses the `2000-01-01` baseline; omission preserves the stored timestamp. |
| `--hourly-window-duration-hours` | `4`–`24` hours and at least the effective interval. |
| `--policy-sub-type` | `Standard` or `Enhanced`; asserts the existing subtype. A mismatch is rejected, **not migrated**. |
| `--instant-rp-retention-days` | `1`–`30` days; Azure additionally enforces schedule- and subtype-specific limits. |
| `--instant-rp-resource-group` | Instant recovery resource-group name prefix, not a resource ID; 1–50 letters, digits, underscores, or hyphens. Preserves the existing suffix. |
| `--snapshot-consistency` | `ApplicationConsistent` restores the default application-consistent behavior; `CrashConsistent` requests ARM `OnlyCrashConsistent`. |
| `--archive-tier-after-days` | At least `45` days; implies `TierAfter` when no mode is supplied. |
| `--archive-tier-mode` | `TierAfter` or `TierRecommended`. **`CopyOnExpiry` is not supported** for RSV VM policy updates. |
| `--smart-tier` | `true` selects `TierRecommended`; **`false` explicitly disables archive tiering** (`DoNotTier`). Omission preserves existing tiering. |
| `--policy-tags` | Comma-separated `key=value` pairs merged into existing resource tags. |

### Schedule updates and retention prerequisites

- Standard (V1) and Enhanced (V2) policies retain their subtype. `--policy-sub-type Enhanced` does not convert a Standard policy; subtype migration is a separate operation.
- On an **existing Hourly Enhanced policy**, supply any subset of the three hourly options. Omitted interval, start, and duration are taken from the existing hourly schedule, and the assembled schedule is validated. For example, changing only the interval to `6` is valid only if the retained window duration is at least 6 hours. `--schedule-frequency Hourly` need not be repeated.
- Switching an existing Enhanced Daily/Weekly policy to Hourly requires `--schedule-frequency Hourly` and **all three** hourly options. Daily retention must already exist or be supplied with `--daily-retention-days`. Hourly on Standard policies is rejected.
- Daily/Weekly updates use exactly **one** `HH:mm` time via `--schedule-times` or legacy `--schedule-time`, never both. If the existing schedule has no unambiguous time, supply one. When leaving Hourly, its window start can supply that time. Hourly schedules use `--hourly-window-start-time`, not either schedule-time flag.
- Whenever `--schedule-frequency Weekly` is explicit, `--schedule-days-of-week` is required. When updating an existing Weekly schedule without repeating its frequency, existing schedule days can be retained. Schedule days are invalid for Daily/Hourly schedules.
- Schedule or retention updates to a Weekly policy require positive weekly retention; weekly retention days must be a subset of backup schedule days. Supply `--weekly-retention-weeks` together with `--weekly-retention-days-of-week` when setting that tier.
- A transition from Daily/Hourly to Weekly that removes positive daily retention requires an **explicit weekly retention weeks-and-days pair**, even if a weekly tier already exists. The update removes daily retention; `--daily-retention-days` cannot be supplied for Weekly schedules.
- Schedule, retention, or instant recovery updates resulting in a **Standard Weekly** policy require effective instant recovery retention of **5 days**. Supply `--instant-rp-retention-days 5` unless the existing value is already 5; omission preserves the existing value and never automatically changes it. This precondition does not apply to tags-only or time-zone-only updates, or to Enhanced policies.
- Schedule or retention updates to a Daily/Hourly policy require positive daily retention. If it is absent (for example, when leaving Weekly), supply `--daily-retention-days` (`1`–`9999`).
- Monthly updates require `--monthly-retention-months` plus either absolute `--monthly-retention-days-of-month` or both relative `--monthly-retention-week-of-month` and `--monthly-retention-days-of-week`. Yearly updates require `--yearly-retention-years`, `--yearly-retention-months`, and the corresponding absolute or relative day selectors. Absolute days are `1`–`28` or `Last`; do not mix absolute and relative schemes. Unmentioned tiers remain unchanged, subject to the Weekly transition rules above.

### Time-zone semantics and limitations

- Only an explicitly supplied **hourly window start** is converted from local time to UTC, as required by the [Enhanced-policy documentation](https://learn.microsoft.com/azure/backup/backup-azure-vms-enhanced-policy#tab/powershell). For example, `08:00` in `India Standard Time` becomes `2000-01-01T02:30:00Z`. The request time zone overrides the fetched policy time zone; if both are absent, UTC is used.
- Because `HH:mm` has no date, conversion uses local **January 1, 2000** and that date's time-zone rules, not today's DST offset. Conversion can cross a UTC date boundary. Unknown time zones and invalid or ambiguous local times on that baseline are rejected. This does not calculate seasonal DST adjustments for future backup runs.
- Omitting `--hourly-window-start-time` preserves the existing timestamp exactly, including on a time-zone-only update. Supply the start again if it should be interpreted in a newly selected time zone.
- Daily/Weekly `--schedule-times` and legacy `--schedule-time` retain their existing clock-value encoding; they do **not** use the hourly UTC conversion. The [REST policy example](https://learn.microsoft.com/azure/backup/backup-azure-arm-userestapi-createorupdatepolicy#tab/azure-vm) encodes a Standard Weekly 10:00 Pacific schedule as `10:00:00Z` with `timeZone` set to `Pacific Standard Time`. This hourly fix does not establish new Enhanced Daily/Weekly semantics. When leaving Hourly without a supplied schedule time, the existing UTC window timestamp is reused without conversion; supply an explicit local Daily/Weekly time to avoid that limitation.

### Archive tiering

Use `--archive-tier-mode TierAfter` with `--archive-tier-after-days` of at least 45, or retain an existing `TierAfter` duration already expressed in days and meeting that minimum. `--archive-tier-after-days` alone selects `TierAfter`. `TierRecommended` cannot be combined with archive days.

Use `--smart-tier true` as an alternative way to select `TierRecommended`, or `--smart-tier false` to disable archive tiering. Neither value can be combined with `--archive-tier-mode` or `--archive-tier-after-days`. Omitting all archive options preserves existing tiering. Azure still enforces service-side policy limits and vault security restrictions.