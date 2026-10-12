#Requires -Version 7
# cspell:ignore policyset

<#
.SYNOPSIS
Runs offline two-step resource updater regression checks with isolated Git fixtures.
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$updater = Join-Path $PSScriptRoot 'Update-FabricDocsResources.ps1'
$helpers = Join-Path $PSScriptRoot 'helpers' 'FabricDocsResourceHelpers.ps1'
$attributes = Join-Path $PSScriptRoot '..' '..' '.gitattributes'
. $helpers
$testRoot = [System.IO.Directory]::CreateDirectory(
    (Join-Path ([System.IO.Path]::GetTempPath()) "fabric-rest-api-tests-$([guid]::NewGuid().ToString('N'))")).FullName
$script:commit = 'a' * 40
$script:articleCommit = 'b' * 40
$script:passed = 0

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) {
        throw $Message
    }
}

function Assert-Fails {
    param([scriptblock] $Action, [string] $MessagePattern)
    try {
        & $Action | Out-Null
    }
    catch {
        Assert-True ($_.Exception.Message -like $MessagePattern) "Unexpected error: $($_.Exception.Message)"
        return
    }
    throw "Expected an error matching: $MessagePattern"
}

function Write-FixtureFile {
    param([string] $Path, [string] $Content)
    [System.IO.Directory]::CreateDirectory((Split-Path $Path -Parent)) | Out-Null
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

function Invoke-FixtureGit {
    param([string[]] $GitArguments)
    & git -C $script:repo @GitArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Fixture Git command failed: $GitArguments"
    }
}

function Get-FixtureSnapshot {
    return (Get-ChildItem -LiteralPath $script:resourceRoot -File -Recurse -Force |
        Sort-Object FullName -CaseSensitive | ForEach-Object {
            "$([System.IO.Path]::GetRelativePath($script:resourceRoot, $_.FullName)) $((Get-FileHash -LiteralPath $_.FullName).Hash)"
        }) -join "`n"
}

function New-FixtureArchive {
    param([switch] $Articles)
    $archivePath = if ($Articles) { $script:articleArchivePath } else { $script:archivePath }
    $files = if ($Articles) { $script:sourceArticles } else { $script:sourceFiles }
    $commit = if ($Articles) { $script:articleCommit } else { $script:commit }
    $repository = if ($Articles) { 'fabric-rest-apis-docs' } else { 'fabric-rest-api-specs' }
    if (Test-Path -LiteralPath $archivePath) {
        [System.IO.File]::Delete($archivePath)
    }
    $zip = [System.IO.Compression.ZipFile]::Open($archivePath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $entries = foreach ($path in $files.Keys) {
            $sourceFile = Join-Path $script:repo $repository $path
            Write-FixtureFile $sourceFile $files[$path]
            $hash = Invoke-FixtureGit @('hash-object', '--no-filters', '--', $sourceFile)
            $entry = $zip.CreateEntry("$repository-$commit/$path")
            $stream = $entry.Open()
            try {
                $bytes = [System.IO.File]::ReadAllBytes($sourceFile)
                $stream.Write($bytes, 0, $bytes.Length)
            }
            finally {
                $stream.Dispose()
            }
            [pscustomobject]@{ path = $path; type = 'blob'; mode = '100644'; sha = $hash }
        }
        $tree = [pscustomobject]@{ truncated = $false; tree = @($entries) }
        if ($Articles) {
            $script:articleTree = $tree
            $script:fixtureState.ArticleTree = $tree
        }
        else {
            $script:tree = $tree
            $script:fixtureState.Tree = $tree
        }
    }
    finally {
        $zip.Dispose()
    }
}

# These scoped substitutes make any unexpected network request fail the test.
function gh {
    Assert-True (($args -join ' ') -ceq 'auth token --hostname github.com') 'Unexpected GitHub CLI call.'
    return 'fixture-token'
}

function Invoke-RestMethod {
    param([string] $Uri, [hashtable] $Headers)
    Assert-True ($Headers.Accept -eq 'application/vnd.github+json') 'Missing GitHub API headers.'
    Assert-True ($Headers.Authorization -ceq 'Bearer fixture-token') 'Missing scoped GitHub authorization.'
    switch ($Uri) {
        'https://api.github.com/repos/microsoft/fabric-rest-api-specs/commits/main' {
            $fixtureState.ResolutionCount++
            return [pscustomobject]@{ sha = $fixtureState.Commit }
        }
        "https://api.github.com/repos/microsoft/fabric-rest-api-specs/git/trees/$($fixtureState.Commit)?recursive=1" {
            return $fixtureState.Tree
        }
        'https://api.github.com/repos/MicrosoftDocs/fabric-rest-apis-docs/commits/main' {
            $fixtureState.ArticleResolutionCount++
            return [pscustomobject]@{ sha = $fixtureState.ArticleCommit }
        }
        "https://api.github.com/repos/MicrosoftDocs/fabric-rest-apis-docs/git/trees/$($fixtureState.ArticleCommit)?recursive=1" {
            return $fixtureState.ArticleTree
        }
        default { throw "Unexpected API request: $Uri" }
    }
}

function Invoke-WebRequest {
    param([string] $Uri, [hashtable] $Headers, [string] $OutFile)
    Assert-True ($Headers.Authorization -ceq 'Bearer fixture-token') 'Archive download is not authenticated.'
    if ($Uri -ceq "https://api.github.com/repos/microsoft/fabric-rest-api-specs/zipball/$($fixtureState.Commit)") {
        [System.IO.File]::Copy($fixtureState.ArchivePath, $OutFile)
        if ($null -ne $fixtureState.AfterDownload) {
            & $fixtureState.AfterDownload $OutFile
        }
    }
    elseif ($Uri -ceq "https://api.github.com/repos/MicrosoftDocs/fabric-rest-apis-docs/zipball/$($fixtureState.ArticleCommit)") {
        [System.IO.File]::Copy($fixtureState.ArticleArchivePath, $OutFile)
        if ($null -ne $fixtureState.AfterArticleDownload) {
            & $fixtureState.AfterArticleDownload $OutFile
        }
    }
    else {
        throw "Unexpected or unpinned archive request: $Uri"
    }
}

function New-TestFixture {
    $script:repo = [System.IO.Directory]::CreateDirectory((Join-Path $testRoot ([guid]::NewGuid().ToString('N')))).FullName
    $script:resourceRoot = Join-Path $script:repo 'tools' 'Fabric.Mcp.Tools.Docs' 'src' 'Resources'
    $script:resources = Join-Path $script:resourceRoot 'fabric-rest-api-specs'
    $script:fixtureUpdater = Join-Path $script:repo 'eng' 'scripts' 'Update-FabricDocsResources.ps1'
    [System.IO.Directory]::CreateDirectory((Split-Path $script:fixtureUpdater -Parent)) | Out-Null
    [System.IO.File]::Copy($updater, $script:fixtureUpdater)
    $fixtureHelpers = Join-Path (Split-Path $script:fixtureUpdater -Parent) 'helpers' 'FabricDocsResourceHelpers.ps1'
    [System.IO.Directory]::CreateDirectory((Split-Path $fixtureHelpers -Parent)) | Out-Null
    [System.IO.File]::Copy($helpers, $fixtureHelpers)
    [System.IO.File]::Copy($attributes, (Join-Path $script:repo '.gitattributes'))
    $baseline = @{
        'admin/swagger.json' = '{"old":true}'
        'platform/swagger.json' = '{"swagger":"2.0"}'
        'common/definitions.json' = '{"definitions":{}}'
        'obsolete/swagger.json' = '{"swagger":"2.0"}'
        'eventhouse/owners.txt' = 'owner'
        'eventstream/owners.txt' = 'owner'
        'kqlDatabase/owners.txt' = 'owner'
    }
    foreach ($path in $baseline.Keys) {
        Write-FixtureFile (Join-Path $script:resources 'contents' $path) $baseline[$path]
    }
    Write-FixtureFile (Join-Path $script:resourceRoot 'item-definitions' 'notebook-definition.md') '# Old notebook article'
    Write-FixtureFile (Join-Path $script:resourceRoot 'item-definitions' 'retired-definition.md') '# Removed upstream'
    $script:sibling = Join-Path $script:resourceRoot 'pagination.md'
    Write-FixtureFile $script:sibling 'Do not change this conceptual article.'
    Write-FixtureFile (Join-Path $script:repo '.gitignore') 'ignored.json'
    Invoke-FixtureGit @('init', '--quiet')
    Invoke-FixtureGit @('config', 'core.autocrlf', 'false')
    Invoke-FixtureGit @('add', '--', 'eng', 'tools', '.gitignore', '.gitattributes')
    Invoke-FixtureGit @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.com', '-c', 'commit.gpgSign=false',
        '-c', "core.hooksPath=$testRoot", 'commit', '--quiet', '-m',
        "Fixture`n`nCo-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>")
    $script:sourceFiles = [ordered]@{
        'admin/swagger.json' = '{"swagger":"2.0","info":{"title":"Admin"}}'
        'platform/swagger.json' = '{"swagger":"2.0"}'
        'common/definitions.json' = '{"definitions":{}}'
        'policySet/swagger.json' = '{"swagger":"2.0"}'
        'policySet/definitions.json' = '{"definitions":{}}'
        'policySet/examples/nested/CreatePolicySet.json' = '{"parameters":{"name":"example"}}'
        'kqlDatabase/swagger.json' = '{"swagger":"2.0"}'
        'kqlDatabase/owners.txt' = 'upstream owner'
        'LICENSE.txt' = "MIT License`nFixture license text.`n"
        'README.md' = 'Not API data'
        'package.json' = '{}'
        '.github/settings.json' = '{}'
    }
    $script:sourceArticles = [ordered]@{
        'docs-ref-conceptual/item-management/definitions/notebook-definition.md' = @'
---
title: Notebook
author: contributor
ms.author: contributor
---

# Notebook definition

Intro.

## Platform part
Metadata.

### Payload example
Kept.
'@
        'docs-ref-conceptual/item-management/definitions/policyset-definition.md' = @'
---
title: Policy set
author: contributor
---

# Policy set definition
See [Notebook](notebook-definition.md).
'@
        'docs-ref-conceptual/item-management/definitions/ontology-old-definition.md' = @'
---
title: Ontology
author: contributor
---

# Ontology JSON definition
'@
        'docs-ref-conceptual/item-management/definitions/item-definition-overview.md' = 'Overview must remain online.'
        'docs-ref-conceptual/pagination.md' = 'Unrelated article.'
        'LICENSE' = "Attribution 4.0 International`nFixture license text.`n"
    }
    $script:archivePath = Join-Path $script:repo 'fixture.zip'
    $script:articleArchivePath = Join-Path $script:repo 'articles.zip'
    $script:fixtureState = [pscustomobject]@{
        Commit = $script:commit
        ArticleCommit = $script:articleCommit
        ResolutionCount = 0
        ArticleResolutionCount = 0
        ArchivePath = $script:archivePath
        ArticleArchivePath = $script:articleArchivePath
        Resources = $script:resources
        ResourceRoot = $script:resourceRoot
        Tree = $null
        ArticleTree = $null
        AfterDownload = $null
        AfterArticleDownload = $null
    }
    New-FixtureArchive
    New-FixtureArchive -Articles
    $script:before = Get-FixtureSnapshot
}

function Invoke-FixtureRefresh {
    param(
        [string] $RestApiRevision = $script:commit,
        [string] $ItemDefinitionRevision = $script:articleCommit,
        [switch] $WhatIf
    )
    & $script:fixtureUpdater -RestApiRevision $RestApiRevision -ItemDefinitionRevision $ItemDefinitionRevision -WhatIf:$WhatIf
}

function Test-Refresh {
    param([string] $Name, [scriptblock] $Action)
    New-TestFixture
    & $Action
    $script:passed++
    Write-Host "PASS: $Name"
}

try {
    Test-Refresh 'Pinned mirror, ownership exclusions, exact bytes, and same-revision no-op' {
        $result = Invoke-FixtureRefresh
        Assert-True ($result.Applied -and $result.RestApi.Files -eq 7) 'Expected seven installed API files.'
        Assert-True ($result.RestApi.Added -eq 4 -and $result.RestApi.Modified -eq 1 -and $result.RestApi.Removed -eq 4) 'Incorrect API change counts.'
        Assert-True ($result.ItemDefinitions.Files -eq 3 -and $result.ItemDefinitions.Added -eq 2 -and
            $result.ItemDefinitions.Modified -eq 1 -and $result.ItemDefinitions.Removed -eq 1) 'Incorrect article change counts.'
        Assert-True ($result.MetadataChanges -eq 4 -and $result.Changes.Count -eq 17) 'Incorrect metadata or path reporting.'
        Assert-True ($script:fixtureState.ResolutionCount -eq 0 -and $script:fixtureState.ArticleResolutionCount -eq 0) 'An explicit revision must not resolve main.'
        foreach ($path in $script:sourceFiles.Keys | Where-Object { $_.EndsWith('.json') -and $_ -notlike '.*' -and $_.Contains('/') }) {
            $actual = [System.IO.File]::ReadAllBytes((Join-Path $script:resources 'contents' $path))
            Assert-True ([System.Convert]::ToBase64String($actual) -ceq
                [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($script:sourceFiles[$path]))) "Changed source bytes: $path"
        }
        Assert-True (@(Get-ChildItem -LiteralPath $script:resources -Filter owners.txt -File -Recurse).Count -eq 0) 'Owner files remain.'
        Assert-True (-not (Test-Path (Join-Path $script:resources 'contents' 'obsolete'))) 'An obsolete directory remains.'
        Assert-True (-not (Test-Path (Join-Path $script:resources 'contents' 'package.json'))) 'Imported repository configuration.'
        Assert-True ([System.IO.File]::ReadAllText($script:sibling) -ceq 'Do not change this conceptual article.') 'Sibling article changed.'
        Assert-True (-not (Test-Path (Join-Path $script:resourceRoot 'item-definitions' 'item-definition-overview.md'))) 'Bundled the overview.'
        $notebook = [System.IO.File]::ReadAllText((Join-Path $script:resourceRoot 'item-definitions' 'notebook-definition.md'))
        Assert-True ($notebook.StartsWith('# Notebook definition') -and $notebook.Contains('### Payload example') -and
            -not $notebook.Contains('## Platform part')) 'Article transformation removed the title or payload example.'
        $provenance = Get-Content (Join-Path $script:resourceRoot 'fabric-rest-apis-docs' 'source.json') -Raw | ConvertFrom-Json
        Assert-True ($provenance.commit -ceq $script:articleCommit -and $provenance.license -ceq 'CC-BY-4.0' -and
            $provenance.articles.Count -eq 3 -and $provenance.articles[0].author -ceq 'contributor') 'Missing source, license, or creator attribution.'
        $first = Get-FixtureSnapshot
        $again = Invoke-FixtureRefresh
        Assert-True (-not $again.Applied -and $again.Changes.Count -eq 0) 'Rerun was not a no-op.'
        Assert-True ((Get-FixtureSnapshot) -ceq $first) 'Rerun changed bytes or paths.'
    }
    Test-Refresh 'Main resolves once and preview does not change resources' {
        $result = Invoke-FixtureRefresh -RestApiRevision main -ItemDefinitionRevision main -WhatIf
        Assert-True ($script:fixtureState.ResolutionCount -eq 1 -and $result.RestApi.Commit -ceq $script:commit -and
            $script:fixtureState.ArticleResolutionCount -eq 1 -and $result.ItemDefinitions.Commit -ceq $script:articleCommit) 'Each main was not pinned exactly once.'
        Assert-True (-not $result.Applied -and $result.Changes.Count -gt 0) 'Preview did not report changes.'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Preview changed resources.'
    }
    foreach ($kind in @('unstaged', 'staged', 'untracked', 'ignored')) {
        Test-Refresh "Protects $kind local edits" {
            $path = switch ($kind) {
                'untracked' { Join-Path $script:resources 'contents' 'notes.json' }
                'ignored' { Join-Path $script:resources 'contents' 'ignored.json' }
                default { Join-Path $script:resources 'contents' 'admin' 'swagger.json' }
            }
            Write-FixtureFile $path '{"local":"edit"}'
            if ($kind -eq 'staged') {
                Invoke-FixtureGit @('add', '--', $path)
            }
            $edited = Get-FixtureSnapshot
            Assert-Fails { Invoke-FixtureRefresh } '*local changes*'
            Assert-True ((Get-FixtureSnapshot) -ceq $edited) 'Local edit was overwritten.'
        }
    }
    Test-Refresh 'Rejects a truncated upstream tree' {
        $script:tree.truncated = $true
        Assert-Fails { Invoke-FixtureRefresh } '*tree*is truncated*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Changed resources after truncated tree.'
    }
    Test-Refresh 'Rejects an incomplete archive' {
        $script:fixtureState.AfterDownload = { param($Path) [System.IO.File]::WriteAllBytes($Path, [byte[]]@(1, 2, 3)) }
        Assert-Fails { Invoke-FixtureRefresh } '*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Changed resources after incomplete download.'
    }
    Test-Refresh 'Rejects bytes that disagree with the Git tree' {
        ($script:tree.tree | Where-Object path -EQ 'admin/swagger.json').sha = '0' * 40
        Assert-Fails { Invoke-FixtureRefresh } '*Git blob*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Installed unverified bytes.'
    }
    Test-Refresh 'Rejects missing archive entries' {
        $zip = [System.IO.Compression.ZipFile]::Open($script:archivePath, [System.IO.Compression.ZipArchiveMode]::Update)
        try { $zip.GetEntry("fabric-rest-api-specs-$script:commit/admin/swagger.json").Delete() } finally { $zip.Dispose() }
        Assert-Fails { Invoke-FixtureRefresh } '*download is missing*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Installed an incomplete snapshot.'
    }
    Test-Refresh 'Rejects invalid upstream JSON without rewriting it' {
        $script:sourceFiles['admin/swagger.json'] = '{"invalid":'
        New-FixtureArchive
        Assert-Fails { Invoke-FixtureRefresh } '*Upstream JSON is invalid*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Changed resources after invalid JSON.'
    }
    Test-Refresh 'Rejects an unexpected source layout' {
        $script:sourceFiles.Remove('platform/swagger.json')
        New-FixtureArchive
        Assert-Fails { Invoke-FixtureRefresh } '*Required upstream file is missing*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Changed resources after layout change.'
    }
    Test-Refresh 'Rejects unsafe paths and symbolic link modes' {
        $script:tree.tree += [pscustomobject]@{ path = 'common/../escape.json'; type = 'blob'; mode = '100644'; sha = '0' * 40 }
        Assert-Fails { Invoke-FixtureRefresh } '*Unsupported upstream file*'
        New-FixtureArchive
        ($script:tree.tree | Where-Object path -EQ 'admin/swagger.json').mode = '120000'
        Assert-Fails { Invoke-FixtureRefresh } '*Unsupported upstream file*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Changed resources for unsafe source.'
    }
    Test-Refresh 'Detects edits made during downloading' {
        $script:fixtureState.AfterDownload = {
            param($Path)
            Assert-True (Test-Path -LiteralPath $Path) 'Expected a staged download.'
            Write-FixtureFile (Join-Path $fixtureState.Resources 'contents' 'admin' 'swagger.json') '{"concurrent":true}'
        }
        Assert-Fails { Invoke-FixtureRefresh } '*local changes*'
        Assert-True ([System.IO.File]::ReadAllText((Join-Path $script:resources 'contents' 'admin' 'swagger.json')) -ceq
            '{"concurrent":true}') 'Concurrent edit was overwritten.'
    }
    Test-Refresh 'Article download failure leaves both snapshots unchanged' {
        $script:fixtureState.AfterArticleDownload = { param($Path) [System.IO.File]::WriteAllBytes($Path, [byte[]]@(1, 2, 3)) }
        Assert-Fails { Invoke-FixtureRefresh } '*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'REST files changed before the article download was verified.'
    }
    Test-Refresh 'Article transformation failure leaves both snapshots unchanged' {
        $script:sourceArticles['docs-ref-conceptual/item-management/definitions/policyset-definition.md'] = '# No front matter'
        New-FixtureArchive -Articles
        Assert-Fails { Invoke-FixtureRefresh } '*Expected leading YAML front matter*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Applied a snapshot before all articles were transformed.'
    }
    Test-Refresh 'Local article edits block both snapshots' {
        Write-FixtureFile (Join-Path $script:resourceRoot 'item-definitions' 'notebook-definition.md') '# Local article edit'
        $edited = Get-FixtureSnapshot
        Assert-Fails { Invoke-FixtureRefresh } '*local changes*'
        Assert-True ((Get-FixtureSnapshot) -ceq $edited) 'Overwrote local articles or prematurely applied REST changes.'
    }
    Test-Refresh 'License changes require review before either snapshot is applied' {
        $script:sourceArticles['LICENSE'] = 'Different license'
        New-FixtureArchive -Articles
        Assert-Fails { Invoke-FixtureRefresh } '*upstream license changed*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Applied resources without a license review.'
    }
    Test-Refresh 'Rejects source paths with conflicting directory case' {
        $script:tree.tree += [pscustomobject]@{
            path = 'policyset/swagger.json'; type = 'blob'; mode = '100644'
            sha = ($script:tree.tree | Where-Object path -CEQ 'policySet/swagger.json').sha
        }
        Assert-Fails { Invoke-FixtureRefresh } '*paths differ only in case*'
        Assert-True ((Get-FixtureSnapshot) -ceq $script:before) 'Installed paths that cannot be mirrored on Windows.'
    }
    Test-Refresh 'Rejects local directory links without following them' {
        $outside = [System.IO.Directory]::CreateDirectory((Join-Path $script:repo 'outside')).FullName
        Write-FixtureFile (Join-Path $outside 'sentinel.md') 'Do not change.'
        $link = Join-Path $script:resourceRoot 'item-definitions' 'linked'
        $linkType = if ($IsWindows) { 'Junction' } else { 'SymbolicLink' }
        New-Item -ItemType $linkType -Path $link -Target $outside | Out-Null
        Assert-Fails { Invoke-FixtureRefresh } '*Refusing to follow a link*'
        Assert-True ([System.IO.File]::ReadAllText((Join-Path $outside 'sentinel.md')) -ceq 'Do not change.') 'Touched a linked directory.'
    }
    Test-Refresh 'Applies a case-only category rename with exact source casing' {
        $oldPath = Join-Path $script:resources 'contents' 'policyset' 'swagger.json'
        Write-FixtureFile $oldPath '{"swagger":"2.0"}'
        Invoke-FixtureGit @('add', '--', $oldPath)
        Invoke-FixtureGit @('-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.com', '-c', 'commit.gpgSign=false',
            '-c', "core.hooksPath=$testRoot", 'commit', '--quiet', '-m',
            "Case rename fixture`n`nCo-authored-by: Copilot App <223556219+Copilot@users.noreply.github.com>")
        $result = Invoke-FixtureRefresh
        $names = @(Get-ChildItem -LiteralPath (Join-Path $script:resources 'contents') -Directory | ForEach-Object { $_.Name })
        Assert-True ($result.Applied -and $names -ccontains 'policySet' -and $names -cnotcontains 'policyset') 'Source directory casing was not preserved.'
    }
    Test-Refresh 'Stages exact upstream CRLF bytes even with automatic line-ending conversion enabled' {
        $path = 'policySet/examples/nested/CreatePolicySet.json'
        $script:sourceFiles[$path] += "`r`n"
        New-FixtureArchive
        Invoke-FixtureRefresh | Out-Null
        Invoke-FixtureGit @('config', 'core.autocrlf', 'true')
        $file = Join-Path $script:resources 'contents' $path
        Invoke-FixtureGit @('add', '--', $file)
        $staged = Invoke-FixtureGit @('ls-files', '--stage', '--', $file)
        $rawHash = Invoke-FixtureGit @('hash-object', '--no-filters', '--', $file)
        Assert-True ($staged.Split(' ')[1] -ceq $rawHash) 'Git normalized a source blob when staging it.'
    }

    $article = @'
---
title: Example
author: contributor
ms.author: alias
---

# Example definition

[Kept](#payload-example), [named](#required1), [removed](#platform-part).
[Sibling](variable-library-definition.md?view=test#schema "title").
[Root](/kusto/management/tables?view=microsoft-fabric&preserve-view=true).
[Overview](item-definition-overview.md#platform-file).
[Absolute](https://example.com/original.md#fragment).
[Reference][1]
[1]: /rest/api/fabric/core/items/create-item
`[Literal](relative.md)`
<a name="required1">1</a>
| .platform | Keep this row. |

## Platform part
Remove this boilerplate.
```text
### Not a real section boundary
[Unprocessed](../../../docs-ref-autogen/fabric/core/Items/Create-Item.yml)
```
### Payload example
```json
{"path":".platform","payload":"unchanged"}
```

## PlatformDetails
Remove this too.
#### Definition data
```text
## Platform file
[Literal](relative.md)
```

## Platform file
Remove through EOF.
'@
    $article = [regex]::Replace($article, '\A---(?=\r?\n)', '--- ')
    $expected = @'
# Example definition

[Kept](#payload-example), [named](#required1), [removed](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/example-definition#platform-part).
[Sibling](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/variable-library-definition?view=test#schema "title").
[Root](https://learn.microsoft.com/kusto/management/tables?view=microsoft-fabric&preserve-view=true).
[Overview](https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/item-definition-overview#platform-file).
[Absolute](https://example.com/original.md#fragment).
[Reference][1]
[1]: https://learn.microsoft.com/rest/api/fabric/core/items/create-item
`[Literal](relative.md)`
<a name="required1">1</a>
| .platform | Keep this row. |

### Payload example
```json
{"path":".platform","payload":"unchanged"}
```

#### Definition data
```text
## Platform file
[Literal](relative.md)
```
'@
    $converted = Convert-FabricItemDefinition $article 'example-definition.md'
    Assert-True ($converted.Content.TrimEnd().Replace("`r`n", "`n") -ceq $expected.TrimEnd().Replace("`r`n", "`n")) 'Article transformation did not match the golden Markdown.'
    Assert-True ($converted.Author -ceq 'contributor' -and $converted.MicrosoftAuthor -ceq 'alias') 'Lost article creator attribution.'
    $script:passed++
    Write-Host 'PASS: Golden title, section-boundary, code, attribution, and link transformations'

    $article = @'
---
title: Example
---
# Example
[Keep](#description-for-dynamic_executor_allocation-contents)

Platform file
---
Remove.

### Description for dynamic_executor_allocation Contents
Kept.
'@
    $converted = Convert-FabricItemDefinition $article 'example-definition.md'
    Assert-True ($converted.Content.Contains('(#description-for-dynamic_executor_allocation-contents)') -and
        $converted.Content.Contains('### Description for dynamic_executor_allocation Contents') -and
        -not $converted.Content.Contains('Remove.')) 'Underlined heading removal or underscore anchors failed.'
    $script:passed++
    Write-Host 'PASS: Underlined platform heading and valid underscore anchor'

    $unsupported = @'
---
title: Example
---
# Example
[API](../../../docs-ref-autogen/fabric/core/Items/Create-Item.yml)
'@
    $unclosed = @'
---
title: Example
---
# Example
```text
'@
    Assert-Fails { Convert-FabricItemDefinition $unsupported 'example-definition.md' } '*Unsupported relative DocFX link*'
    Assert-Fails { Convert-FabricItemDefinition $unclosed 'example-definition.md' } '*Unclosed Markdown code fence*'
    $script:passed++
    Write-Host 'PASS: Unsupported retained DocFX links and unclosed fences stop conversion'
    Write-Host "$script:passed offline refresh checks passed."
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
