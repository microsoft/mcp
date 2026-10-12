# cspell:ignore zipball

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-BlobHash {
    param([string] $Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    # Git blob IDs hash the object header as well as the unmodified file bytes.
    $hash = [System.Security.Cryptography.IncrementalHash]::CreateHash([System.Security.Cryptography.HashAlgorithmName]::SHA1)
    try {
        $hash.AppendData([System.Text.Encoding]::UTF8.GetBytes("blob $($bytes.Length)`0"))
        $hash.AppendData($bytes)
        return [System.BitConverter]::ToString($hash.GetHashAndReset()).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $hash.Dispose()
    }
}

function Get-ManagedSnapshot {
    param([string] $Root, [string[]] $Directories)

    $snapshot = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::Ordinal)
    foreach ($directory in $Directories) {
        $path = Join-Path $Root $directory
        if (-not (Test-Path -LiteralPath $path)) {
            continue
        }
        $items = @(Get-Item -LiteralPath $path) + @(Get-ChildItem -LiteralPath $path -Force -Recurse)
        foreach ($item in $items) {
            if ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                throw "Refusing to follow a link in the managed resources: $($item.FullName)"
            }
            if (-not $item.PSIsContainer) {
                $relativePath = [System.IO.Path]::GetRelativePath($Root, $item.FullName).Replace('\', '/')
                $snapshot.Add($relativePath, (Get-BlobHash $item.FullName))
            }
        }
    }
    return ,$snapshot
}

function Get-SnapshotChange {
    param(
        [System.Collections.Generic.Dictionary[string, string]] $Before,
        [System.Collections.Generic.Dictionary[string, string]] $After
    )

    foreach ($path in $After.Keys | Sort-Object -CaseSensitive) {
        if (-not $Before.ContainsKey($path)) {
            [pscustomobject]@{ Change = 'Added'; Path = $path }
        }
        elseif ($Before[$path] -cne $After[$path]) {
            [pscustomobject]@{ Change = 'Modified'; Path = $path }
        }
    }
    foreach ($path in $Before.Keys | Sort-Object -CaseSensitive) {
        if (-not $After.ContainsKey($path)) {
            [pscustomobject]@{ Change = 'Removed'; Path = $path }
        }
    }
}

function Save-UpstreamSnapshot {
    param([string] $Repository, [string] $Revision, [string] $Directory, [scriptblock] $SelectFiles)

    $api = "https://api.github.com/repos/$Repository"
    $token = & gh auth token --hostname github.com
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($token)) {
        throw 'Authenticate the GitHub CLI with access to both source repositories before refreshing.'
    }
    $headers = @{
        Accept = 'application/vnd.github+json'
        'User-Agent' = 'mcp-fabric-docs-refresh'
        Authorization = "Bearer $token"
    }
    if ($Revision -eq 'main') {
        $Revision = (Invoke-RestMethod -Uri "$api/commits/main" -Headers $headers).sha
    }
    if ($Revision -notmatch '^[0-9a-fA-F]{40}$') {
        throw "Upstream $Repository did not return a full commit SHA."
    }
    $Revision = $Revision.ToLowerInvariant()
    Write-Host "Source: https://github.com/$Repository/tree/$Revision"
    $tree = Invoke-RestMethod -Uri "$api/git/trees/${Revision}?recursive=1" -Headers $headers
    if ($tree.truncated) {
        throw "The upstream tree for $Repository is truncated; no resources have been changed."
    }
    $files = @(& $SelectFiles $tree.tree)
    if ($files.Count -eq 0) {
        throw "No source files selected from $Repository."
    }
    [System.IO.Directory]::CreateDirectory($Directory) | Out-Null
    $archivePath = Join-Path $Directory 'source.zip'
    Invoke-WebRequest -Uri "$api/zipball/$Revision" -Headers $headers -OutFile $archivePath
    $archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
    $pathCasing = [System.Collections.Generic.Dictionary[string, string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    try {
        $roots = @($archive.Entries | ForEach-Object { $_.FullName.Split('/')[0] } | Sort-Object -Unique -CaseSensitive)
        if ($roots.Count -ne 1) {
            throw "Unexpected archive layout for $Repository."
        }
        foreach ($file in $files) {
            if ($file.mode -notin @('100644', '100755') -or
                $file.path -cnotmatch '^(?:[A-Za-z0-9_-]+/)*[A-Za-z0-9_.-]+$' -or
                $file.sha -notmatch '^[0-9a-f]{40}$') {
                throw "Unsupported upstream file or Git mode: $($file.path) ($($file.mode))."
            }
            $prefix = ''
            foreach ($segment in $file.path.Split('/')) {
                $prefix += $segment
                if ($pathCasing.ContainsKey($prefix) -and $pathCasing[$prefix] -cne $prefix) {
                    throw "Upstream paths differ only in case: $prefix and $($pathCasing[$prefix])."
                }
                $pathCasing[$prefix] = $prefix
                $prefix += '/'
            }
            $entry = $archive.GetEntry("$($roots[0])/$($file.path)")
            if ($null -eq $entry) {
                throw "The download is missing $($file.path); no resources have been changed."
            }
            $stagedFile = Join-Path $Directory $file.path
            [System.IO.Directory]::CreateDirectory((Split-Path $stagedFile -Parent)) | Out-Null
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $stagedFile)
            if ((Get-BlobHash $stagedFile) -cne $file.sha) {
                throw "Downloaded bytes do not match the upstream Git blob for $($file.path)."
            }
            if ($file.path.EndsWith('.json', [System.StringComparison]::Ordinal)) {
                try {
                    $document = [System.Text.Json.JsonDocument]::Parse([System.IO.File]::ReadAllText($stagedFile))
                    $document.Dispose()
                }
                catch {
                    throw "Upstream JSON is invalid in $($file.path): $($_.Exception.Message). No resources have been changed."
                }
            }
        }
    }
    finally {
        $archive.Dispose()
    }
    return [pscustomobject]@{ Repository = "https://github.com/$Repository"; Commit = $Revision; Directory = $Directory; Files = $files }
}

function Write-ResourceProvenance {
    param([string] $Path, [System.Collections.IDictionary] $Value)
    $json = $Value | ConvertTo-Json -Depth 5
    [System.IO.File]::WriteAllText($Path, $json.Replace("`r`n", "`n") + "`n", [System.Text.UTF8Encoding]::new($false))
}

function Resolve-ArticleLink {
    param([string] $Target, [uri] $ArticleUri, [System.Collections.Generic.HashSet[string]] $Anchors)

    if ($Target.StartsWith('<') -and $Target.EndsWith('>')) {
        return '<' + (Resolve-ArticleLink $Target.Substring(1, $Target.Length - 2) $ArticleUri $Anchors) + '>'
    }
    if ($Target -match '^(?:[a-z][a-z0-9+.-]*:|//)') {
        return $Target
    }
    if ($Target.StartsWith('#')) {
        if ($Target -eq '#' -or $Anchors.Contains([uri]::UnescapeDataString($Target.Substring(1)))) {
            return $Target
        }
        return $ArticleUri.AbsoluteUri + $Target
    }
    if ($Target.StartsWith('/')) {
        return 'https://learn.microsoft.com' + $Target
    }
    if ($Target -match 'docs-ref-autogen|\.ya?ml(?:[?#]|$)') {
        throw "Unsupported relative DocFX link in $ArticleUri : $Target"
    }
    $Target = [regex]::Replace($Target, '\.md(?=[?#]|$)', '', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    return [uri]::new($ArticleUri, $Target).AbsoluteUri
}

function Convert-ArticleLink {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSReviewUnusedParameter', '', Justification = 'Parameters are captured by the Regex MatchEvaluator.')]
    [CmdletBinding()]
    param([string] $Text, [uri] $ArticleUri, [System.Collections.Generic.HashSet[string]] $Anchors)

    $patterns = @(
        '(?<prefix>\]\([ \t]*)(?<target><[^>\r\n]+>|[^ \t\r\n)]+)',
        '^(?<prefix> {0,3}\[[^\]]+\]:[ \t]*)(?<target><[^>\r\n]+>|[^ \t\r\n]+)',
        '(?<prefix>\b(?:href|src)\s*=\s*["''])(?<target>[^"'']+)'
    )
    foreach ($pattern in $patterns) {
        $Text = [regex]::Replace($Text, $pattern, {
            param($match)
            $match.Groups['prefix'].Value + (Resolve-ArticleLink $match.Groups['target'].Value $ArticleUri $Anchors)
        }, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    }
    return $Text
}

function Convert-FabricItemDefinition {
    param([string] $Text, [string] $Name)

    $frontMatter = [regex]::Match($Text, '\A---[ \t]*\r?\n.*?^---[ \t]*(?:\r?\n|\z)', 'Singleline, Multiline')
    if (-not $frontMatter.Success) {
        throw "Expected leading YAML front matter in $Name."
    }
    $body = $Text.Substring($frontMatter.Length).TrimStart([char[]]"`r`n")
    $lines = @([regex]::Matches($body, '[^\n]*(?:\n|\z)') | ForEach-Object { $_.Value })
    $kept = [System.Collections.Generic.List[object]]::new()
    $anchors = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $headingCounts = @{}
    $fence = ''
    $fenceLength = 0
    $skip = $false
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $content = $line.TrimEnd([char[]]"`r`n")
        $heading = ''
        $isCode = $fence.Length -gt 0
        if ($isCode) {
            if ($content -match ('^ {0,3}' + [regex]::Escape($fence) + '{' + $fenceLength + ',}[ \t]*$')) {
                $fence = ''
            }
        }
        elseif ($content -match '^ {0,3}(`{3,}|~{3,})') {
            $fence = $Matches[1].Substring(0, 1)
            $fenceLength = $Matches[1].Length
            $isCode = $true
        }
        elseif ($content -match '^ {0,3}#{1,6}[ \t]+(.*?)(?:[ \t]+#+[ \t]*)?$') {
            $heading = $Matches[1].Trim()
        }
        elseif ($content -match '\S' -and $i + 1 -lt $lines.Count -and
            $lines[$i + 1].TrimEnd([char[]]"`r`n") -match '^ {0,3}(?:=+|-+)[ \t]*$') {
            $heading = $content.Trim()
        }
        if ($heading) {
            # Payload examples can resume at H3, not just the platform section's H2.
            $skip = $heading -match '^(?:Platform part|PlatformDetails|Platform file)$'
        }
        if ($skip) {
            continue
        }
        $kept.Add([pscustomobject]@{ Text = $line; IsCode = $isCode })
        if ($heading) {
            $slug = [regex]::Replace($heading, '\[([^\]]+)\]\([^)]*\)', '$1')
            $slug = [System.Net.WebUtility]::HtmlDecode($slug).Replace('`', '').ToLowerInvariant()
            $slug = [regex]::Replace($slug, '[^\p{L}\p{Nd}_\-\s]', '')
            $slug = [regex]::Replace($slug, '\s', '-')
            if ($headingCounts.ContainsKey($slug)) {
                $headingCounts[$slug]++
                $anchors.Add("$slug-$($headingCounts[$slug])") | Out-Null
            }
            else {
                $headingCounts[$slug] = 0
                $anchors.Add($slug) | Out-Null
            }
        }
        if (-not $isCode) {
            foreach ($match in [regex]::Matches($line, '<a\b[^>]*\b(?:name|id)\s*=\s*["'']([^"'']+)["'']', 'IgnoreCase')) {
                $anchors.Add($match.Groups[1].Value) | Out-Null
            }
        }
    }
    if ($fence) {
        throw "Unclosed Markdown code fence in $Name."
    }

    $articleUri = [uri]("https://learn.microsoft.com/rest/api/fabric/articles/item-management/definitions/" +
        [System.IO.Path]::GetFileNameWithoutExtension($Name))
    $result = [System.Text.StringBuilder]::new()
    foreach ($line in $kept) {
        if ($line.IsCode) {
            $result.Append($line.Text) | Out-Null
            continue
        }
        $offset = 0
        foreach ($code in [regex]::Matches($line.Text, '(?<!`)(?<ticks>`+)(?!`).*?\k<ticks>(?!`)')) {
            $result.Append((Convert-ArticleLink $line.Text.Substring($offset, $code.Index - $offset) $articleUri $anchors)) | Out-Null
            $result.Append($code.Value) | Out-Null
            $offset = $code.Index + $code.Length
        }
        $result.Append((Convert-ArticleLink $line.Text.Substring($offset) $articleUri $anchors)) | Out-Null
    }
    return [pscustomobject]@{
        Content = $result.ToString()
        Author = [regex]::Match($frontMatter.Value, '(?m)^author:[ \t]*(.*)').Groups[1].Value.Trim()
        MicrosoftAuthor = [regex]::Match($frontMatter.Value, '(?m)^ms\.author:[ \t]*(.*)').Groups[1].Value.Trim()
    }
}
