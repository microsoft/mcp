#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string[]]$ReleaseOwners,

    [Parameter(Mandatory)]
    [datetime]$ReleaseDate,

    [Parameter(Mandatory)]
    [uri]$BuildUrl
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot\..\common\scripts\common.ps1"
. "$PSScriptRoot\helpers\ReleasePrepHelpers.ps1"

Invoke-ReleasePrepPullRequest `
    -RepositoryRoot $RepoRoot.Path `
    -ReleaseOwners $ReleaseOwners `
    -ReleaseDate $ReleaseDate `
    -BuildUrl $BuildUrl `
    -GitHubToken $env:GH_TOKEN `
    -PushToken $env:GH_PUSH_TOKEN
