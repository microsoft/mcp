# Finding Rules

Use these rules to turn candidate findings into a concise draft review.

## Candidate Format

```yaml
severity: blocking | warning | nit
path: path/to/file.cs
line: 42
summary: Short statement of the issue
detail: Concrete consequence and required correction
lens: security
confidence: high | medium | low
```

`path` and `line` may be omitted only for a cross-cutting issue that cannot be anchored to a changed line.

## Qualification

Keep a finding only when all of these are true:

1. The changed code causes or exposes the issue.
2. The concern is verified against the diff and relevant surrounding code.
3. The consequence is concrete.
4. The author can take a specific action.
5. The finding is worth a maintainer's time.
6. The finding is not already covered by an existing review thread.

Drop a finding when any of these are true:

- It is a style preference with no correctness or maintenance impact.
- It restates the pull request description.
- It relies on an unlikely hypothetical with no supporting evidence.
- It concerns untouched code and is not a regression caused by the change.
- It asks for a broad refactor when a local correction is sufficient.
- It only says to add tests without naming the unverified behavior or risk.
- It is low confidence after reading the relevant implementation.

Use only high-confidence findings by default. A medium-confidence finding is acceptable only when it asks a precise question about a material risk. Drop low-confidence findings.

## Severity

- `blocking`: Security vulnerability, correctness bug, deterministic build or runtime failure, data loss, cross-user leakage, required merge artifact missing, or unplanned public contract break.
- `warning`: Material reliability, maintainability, test evidence, documentation, or design gap that should be addressed but does not meet the blocking threshold.
- `nit`: Optional polish with a concrete codebase-consistency or clarity benefit. Prefix the inline text with `nit:`.

Do not inflate severity to attract attention.

## Merge and Order

1. Merge candidates that describe the same root cause.
2. Prefer the finding with the clearest consequence and smallest complete fix.
3. Group findings by file and order them by line.
4. If several lenses support one finding, keep one comment and record the strongest lens.
5. Limit the final review to the findings that materially improve the change.

## Diff Anchoring

- Anchor each inline comment to a changed line on the new side of the diff.
- Use `startLine` only when the full changed range is needed to understand the issue.
- Never invent a line number or attach a comment to an unrelated changed line.
- Put a verified cross-cutting issue in the review body when no valid changed line exists.

## Voice

- Lead with the technical issue.
- Be direct, collaborative, concrete, and concise.
- Explain the consequence before or with the requested fix.
- Include a complete suggestion block when the correction is obvious, local, and fully representable.
- Do not use generic praise, filler, rhetorical questions, or coaching language.
- Do not use bullets or numbered lists inside an inline comment.
- Do not use em dashes or doubled hyphens.
- Do not add reviewer tags, generated-by text, or authorship attribution.

## Output

Return a draft review in this structure:

```yaml
body: "One concise sentence. Include a cross-cutting finding here only when it cannot be anchored to the diff."
comments:
  - path: "relative/path/to/file.cs"
    line: 42
    startLine: 39
    severity: blocking
    body: "Inline comment text."
```

Omit `startLine` for a single-line comment. If no findings survive triage, return:

```yaml
body: "No blocking findings identified."
comments: []
```
