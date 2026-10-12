# Fabric Docs static resources
<!-- cspell:ignore longpaths -->

## On-demand refresh

Run from the repository root with PowerShell 7, Git, and the GitHub CLI (`gh`).
Authenticate `gh` with read access to both upstream repositories: the article
source requires authorized access. The updater uses that credential only for
GitHub API requests and archive downloads, never writing it to provenance.
No Fabric login or deployment is needed. This is a manual process, not a
scheduled workflow.

```powershell
# Preview both upstream main branches without changing bundled resources.
$preview = .\eng\scripts\Update-FabricDocsResources.ps1 -WhatIf
$preview.RestApi
$preview.ItemDefinitions
$preview.Changes

# Apply exactly the two commits reported by the preview.
.\eng\scripts\Update-FabricDocsResources.ps1 `
    -RestApiRevision $preview.RestApi.Commit `
    -ItemDefinitionRevision $preview.ItemDefinitions.Commit

# Or resolve and refresh both main branches in one run.
.\eng\scripts\Update-FabricDocsResources.ps1
```

Each revision can be `main` or a full commit SHA. The updater resolves each
`main` once, then uses that immutable commit for its Git tree and archive.
**Both downloads and all article transformations are completed and validated
before either resource tree is changed.** Selected source bytes are checked
against Git blob IDs, and all REST JSON is parsed. Missing files, truncated trees,
corrupt downloads, invalid JSON, unsupported article links, or unrecognized
license headers stop the refresh.

### REST API specifications

Source: [microsoft/fabric-rest-api-specs](https://github.com/microsoft/fabric-rest-api-specs).
Upstream root directories containing `swagger.json`, plus `common`, map directly
to `src\Resources\fabric-rest-api-specs\contents`. There is no upstream `contents`
directory.

All JSON in these directories is included: Swagger specifications, definitions,
nested examples, and supporting schemas. Root repository documentation,
governance files, and every `owners.txt` are excluded. Source bytes, relative
paths, and case are preserved, without formatting or spelling corrections.

The adjacent `source.json` records the repository and commit. `LICENSE.txt` is
the unmodified upstream Microsoft attribution and MIT license. They remain outside
`contents` so they cannot become API item categories.

### Item-definition articles

Source: [MicrosoftDocs/fabric-rest-apis-docs](https://github.com/MicrosoftDocs/fabric-rest-apis-docs/tree/main/docs-ref-conceptual/item-management/definitions).
Every top-level `*-definition.md` in that source directory maps, under the same
basename, to `src\Resources\item-definitions`. The overview remains online, not
bundled. Removed upstream articles are removed locally; the result lists them
explicitly. Both new- and old-experience Ontology articles are included.

The transformation follows earlier Fabric resource refreshes:

- Remove leading YAML front matter, including delimiters with trailing spaces.
  Retain the visible H1 and introduction.
- Remove dedicated `Platform part`, `PlatformDetails`, and `Platform file`
  sections up to the next real heading of **any level**, or EOF. In particular,
  notebook and environment H3 payload/library examples must remain.
- Keep `.platform` table rows, payloads, and other mentions outside those sections.
  Do not alter fenced code or inline code.
- Convert sibling Markdown links to canonical
  `https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/`
  URLs without `.md`. Prefix Learn-root-relative links with
  `https://learn.microsoft.com`; preserve queries and fragments. Existing absolute
  URLs remain unchanged, and reference-style link definitions are handled.
- Preserve valid same-page anchors, including HTML named anchors. Links into
  removed sections point to the online article and fragment instead. A retained
  relative DocFX YAML link stops the refresh rather than becoming a broken
  article URL.

Articles use **CC BY 4.0**, not the REST specification's MIT license.
`src\Resources\fabric-rest-apis-docs\LICENSE.txt` retains the full upstream
license. Its `source.json` records the source revision/link, creator attribution
from each article's front matter, license link, and performed transformations.
This metadata is embedded outside the item-definition article directory.

### Scope and local-edit safety

Only these three subdirectories of `src\Resources` are managed:
`fabric-rest-api-specs`, `item-definitions`, and `fabric-rest-apis-docs`.
Do not put hand-maintained files inside them. Other conceptual Markdown and
Core tools are untouched; new API categories do not imply new executable tools.

Commit or move local edits before applying a different snapshot. Staged,
unstaged, untracked, and ignored files are protected, with no force-overwrite
option. A byte-identical rerun is a no-op even before committing the refresh.
Staging protects against download/conversion failures, but a local disk/write
failure during application can leave a partial worktree. Such failures are
reported as errors and must be inspected before retrying.

The result includes per-step addition/modification/removal counts, metadata
changes, and individual paths in `Changes`; `-Verbose` prints the paths. Counts
compare raw bytes. Scoped `.gitattributes` rules disable line-ending conversion
only for these managed trees, preserving source bytes in commits and checkouts.
On a checkout created before those rules existed, the first refresh can also
restore line endings that Git previously converted.
Git operations enable long-path support per invocation, without changing Git
configuration, because deeply nested example names can exceed Windows path limits.

## Inspecting and submitting a refresh

```powershell
git -c core.longpaths=true diff --stat
git -c core.longpaths=true diff -- tools\Fabric.Mcp.Tools.Docs\src\Resources
git -c core.longpaths=true ls-files --others --exclude-standard -- tools\Fabric.Mcp.Tools.Docs\src\Resources

# Repeat the apply command with the same two revisions; expect zero changes.

# Offline updater checks, then the scoped Docs build and consumer tests.
.\eng\scripts\Test-FabricDocsResourcesRefresh.ps1
dotnet build tools\Fabric.Mcp.Tools.Docs\tests\Fabric.Mcp.Tools.Docs.Tests\Fabric.Mcp.Tools.Docs.Tests.csproj
dotnet test --project tools\Fabric.Mcp.Tools.Docs\tests\Fabric.Mcp.Tools.Docs.Tests\Fabric.Mcp.Tools.Docs.Tests.csproj
```

Review both diffs, especially removals, preserved code examples, and link targets.
Check local REST `$ref` targets against the pinned source when reviewing changed
specifications. JSON syntax and byte equality do not establish semantic reference
validity. Report upstream defects without rewriting authoritative content or
silently repairing unrelated documentation.

Add one Fabric changelog entry with
`eng\scripts\New-ChangelogEntry.ps1 -ChangelogPath servers\Fabric.Mcp.Server\CHANGELOG.md`
under `Other Changes`; do not compile the release changelog. After **both** steps
and focused checks succeed, submit one same-repository PR using the repository
template, recording both revisions, change counts, and validation results.
The script itself does not commit, push, or open a PR. No live Fabric, native,
Core, or full-repository test suites are needed for this resource-only update.
