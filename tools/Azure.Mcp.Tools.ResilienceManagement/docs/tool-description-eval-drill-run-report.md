# Drill run report tools: tool description evaluation

Results of the [Tool Description Evaluator](https://github.com/microsoft/mcp/blob/main/eng/tools/ToolDescriptionEvaluator/Quickstart.md) for the two tools added for drill run reports:

- `resiliency_drill_run_generate-report`
- `resiliency_drill_run_get-report-download-url`

- **Run date:** 2026-10-22
- **Embedding model:** `text-embedding-3-large`
- **Server build:** `Azure.Mcp.Server` Debug build of this branch (418 tools loaded)
- **Prompts:** the rows for these two tools in `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`
- **Pass criteria:** the expected tool ranks in the top 3 (ideally #1) with a score of at least `0.60` (the repository minimum is `0.40`)

## Result

**Pass. All 4 prompts rank #1 and score at least 0.618.**

| Metric | Value |
|--------|-------|
| Tools evaluated | 2 (both new) |
| Prompts evaluated | 4 |
| Expected tool ranked #1 | 4 / 4 |
| Score ≥ 0.60 | 4 / 4 |
| Lowest score | 0.618 |

## Score per tool

| Tool | Prompts | Min score | Avg score | Worst rank | Result |
|------|---------|-----------|-----------|------------|--------|
| `resiliency_drill_run_generate-report` | 2 | 0.628 | 0.661 | 1 | ✅ |
| `resiliency_drill_run_get-report-download-url` | 2 | 0.618 | 0.671 | 1 | ✅ |

## Score per prompt

| Tool | Prompt | Rank | Score | Closest competitor (score) |
|------|--------|------|-------|----------------------------|
| `generate-report` | Generate the report for drill run `<drill_run_name>` of resilience drill `<drill_name>` in service group `<service_group>` | 1 | 0.628 | `drill_run_get` (0.625) |
| `generate-report` | Regenerate the resilience drill run report for run `<drill_run_name>` of drill `<drill_name>` in service group `<service_group>` and show its generation status | 1 | 0.694 | `drill_run_get` (0.565) |
| `get-report-download-url` | Get the download URL for the generated report of drill run `<drill_run_name>` of resilience drill `<drill_name>` in service group `<service_group>` | 1 | 0.723 | `drill_run_get` (0.582) |
| `get-report-download-url` | Give me a link to download the HTML report for resilience drill run `<drill_run_name>` of drill `<drill_name>` in service group `<service_group>` | 1 | 0.618 | `drill_run_generate-report` (0.546) |

## Notes

- **Thin margin on one prompt.** "Generate the report for drill run ..." scores 0.628 for `generate-report` and 0.625 for `drill_run_get`. It passes the criteria, but a future change to either description could reorder them. The first description draft named `drill run get` and the sibling tool inside the description; that lowered the margin, so the final descriptions do not reference other tools.
- **Final descriptions evaluated:**
  - `generate-report`: generates or refreshes the report for a resilience drill run (a finalized report is left unchanged), starts the operation, and returns the operation ID, whether it completed, and the report summary when available.
  - `get-report-download-url`: gets a short-lived, read-only download URL for the generated report of a drill run, with the format and expiry; the report must already have been generated; the URL is a secret.
- Both tools are in their own consolidated tool entries in `consolidated-tools.json`, so consolidated-mode selection does not compete with the existing drill run tools.

## Reproduce

```pwsh
dotnet build servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj -c Debug -o <output-dir>
cd eng/tools/ToolDescriptionEvaluator/src
dotnet run -- --server-exe <output-dir>/azmcp.exe --area "resiliency_drill_run_generate-report,resiliency_drill_run_get-report-download-url" --top 5 --output-file-name report-eval
```

Credentials come from `eng/tools/ToolDescriptionEvaluator/.env` (`AOAI_ENDPOINT`, `TEXT_EMBEDDING_API_KEY`).
