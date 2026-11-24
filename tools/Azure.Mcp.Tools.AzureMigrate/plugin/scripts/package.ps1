#requires -Version 5.1
<#
.SYNOPSIS
  Builds out/<Variant>/cowork.zip (Copilot Cowork plugin), out/<Variant>/app.zip (sideloadable app package)
  and out/<Variant>/skills/<name>.zip (one per skill folder, if any, for standalone skill upload).
.PARAMETER Variant
  Local        -> "Azure Migrate (Local)", for an azmcp on the developer box (via devtunnel) that calls a OneBox Discovery Service.
  PrivateStamp -> "Azure Migrate (Private Stamp)", for the hosted azmcp that calls the private stamp Discovery Service.
  Each variant has its own app ID so both can be installed side by side.
.PARAMETER McpUrl
  Public HTTPS URL of the MCP endpoint, e.g. https://<id>.devtunnels.ms/ or https://<app>.azurewebsites.net/
.PARAMETER AuthReferenceId
  Optional. OAuth client registration ID from Teams Developer Portal (Tools > OAuth client registration).
  When set, the connector uses "OAuthPluginVault" instead of "None". See docs/AUTH.md.
#>
param(
  [Parameter(Mandatory = $true)][ValidateSet('Local', 'PrivateStamp')][string]$Variant,
  [Parameter(Mandatory = $true)][string]$McpUrl,
  [string]$AuthReferenceId
)
$ErrorActionPreference = 'Stop'
if ($McpUrl -notmatch '^https://') { throw 'McpUrl must be an https:// URL.' }

$variants = @{
  # ConnectorId must be unique across all installed Cowork plugins; a duplicate stops every plugin using it from loading.
  Local        = @{ AppId = '5b0e7c2d-8f41-4e6a-9c3b-2d7a1f4e6b90'; AppName = 'Azure Migrate (Local)'; EnvDescription = 'local OneBox Discovery Service'; ConnectorId = 'azuremigratelocal' }
  PrivateStamp = @{ AppId = '1a4cadc6-df4c-40a2-bcee-e7be94d7e8de'; AppName = 'Azure Migrate (Private Stamp)'; EnvDescription = 'private stamp Discovery Service'; ConnectorId = 'azuremigrateprivatestamp' }
}
$v = $variants[$Variant]
function Set-Placeholders([string]$text) {
  $text.Replace('__MCP_URL__', $McpUrl).Replace('__APP_ID__', $v.AppId).Replace('__APP_NAME__', $v.AppName).Replace('__ENV_DESCRIPTION__', $v.EnvDescription).Replace('__CONNECTOR_ID__', $v.ConnectorId)
}

$authRegex = '"(authorization|auth)":\s*\{\s*"type":\s*"None"\s*\}'
function Set-Auth([string]$json, [string]$refKey) {
  if (-not $AuthReferenceId) { return $json }
  if ($json -notmatch $authRegex) { throw "No `"type`": `"None`" auth block found to replace." }
  return [regex]::Replace($json, $authRegex, { param($m) "`"$($m.Groups[1].Value)`": { `"type`": `"OAuthPluginVault`", `"$refKey`": `"$AuthReferenceId`" }" })
}

$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root "out\$Variant"
$stage = Join-Path $out 'stage'
Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory $stage | Out-Null

Copy-Item (Join-Path $root 'appPackage\*') $stage -Recurse
foreach ($f in 'manifest.json', 'declarativeAgent.json') {
  $p = Join-Path $stage $f
  (Set-Placeholders (Get-Content $p -Raw)) | Set-Content $p -Encoding UTF8
}

$plugin = Join-Path $stage 'ai-plugin.json'
(Set-Auth (Set-Placeholders (Get-Content $plugin -Raw)) 'reference_id') | Set-Content $plugin -Encoding UTF8

# Placeholder icons if none provided.
Add-Type -AssemblyName System.Drawing
function New-Icon($path, $size, $fill) {
  if (Test-Path $path) { return }
  $bmp = New-Object System.Drawing.Bitmap $size, $size
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.Clear([System.Drawing.ColorTranslator]::FromHtml($fill))
  $g.Dispose(); $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
}
New-Icon (Join-Path $stage 'color.png') 192 '#0078D4'
New-Icon (Join-Path $stage 'outline.png') 32 '#FFFFFF'

Compress-Archive -Path (Join-Path $stage '*') -DestinationPath (Join-Path $out 'app.zip')
$skillsRoot = Join-Path $root 'skills'
$skillDirs = @(Get-ChildItem $skillsRoot -Directory -ErrorAction SilentlyContinue | Where-Object { Test-Path (Join-Path $_.FullName 'SKILL.md') })
if ($skillDirs.Count -gt 0) {
  New-Item -ItemType Directory (Join-Path $out 'skills') | Out-Null
  foreach ($d in $skillDirs) {
    Compress-Archive -Path (Join-Path $d.FullName '*') -DestinationPath (Join-Path $out "skills\$($d.Name).zip")
  }
}

# Copilot Cowork plugin: manifest.json (agentConnectors [+ agentSkills]) + icons + mcp-tools.json [+ skills/<name>/SKILL.md]
$cowork = Join-Path $out 'cowork-stage'
New-Item -ItemType Directory $cowork | Out-Null
$manifest = Set-Auth (Set-Placeholders (Get-Content (Join-Path $root 'coworkPackage\manifest.json') -Raw)) 'referenceId'
$listed = @(($manifest | ConvertFrom-Json).agentSkills | ForEach-Object { $_.folder })
foreach ($d in $skillDirs) {
  if ($listed -notcontains "skills/$($d.Name)") { Write-Warning "skills/$($d.Name) is not listed in coworkPackage\manifest.json agentSkills." }
  New-Item -ItemType Directory (Join-Path $cowork 'skills') -Force | Out-Null
  Copy-Item $d.FullName (Join-Path $cowork 'skills') -Recurse
}
foreach ($l in $listed) { if (-not (Test-Path (Join-Path $root $l))) { throw "agentSkills lists '$l' but it does not exist." } }
$manifest | Set-Content (Join-Path $cowork 'manifest.json') -Encoding UTF8
Copy-Item (Join-Path $stage 'color.png'), (Join-Path $stage 'outline.png'), (Join-Path $stage 'mcp-tools.json') $cowork

# Build zips with forward-slash entry names (Compress-Archive on Windows PowerShell 5.1 writes backslashes).
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
function New-Zip($srcDir, $zipPath) {
  $srcDir = (Resolve-Path $srcDir).Path.TrimEnd('\') + '\'
  $zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Create')
  try {
    Get-ChildItem $srcDir -Recurse -File | ForEach-Object {
      $name = $_.FullName.Substring($srcDir.Length).Replace('\', '/')
      [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $name) | Out-Null
    }
  } finally { $zip.Dispose() }
}
New-Zip $cowork (Join-Path $out 'cowork.zip')

Remove-Item $stage, $cowork -Recurse -Force
$authNote = if ($AuthReferenceId) { "auth=OAuthPluginVault ($AuthReferenceId)" } else { 'auth=None' }
Write-Host "Built $($v.AppName) ($($v.AppId)): $out\cowork.zip, $out\app.zip ($($skillDirs.Count) skills, $authNote, MCP $McpUrl)"
