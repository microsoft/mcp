#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string] $ReadMePath,
    [Parameter(Mandatory=$true)]
    [ValidateSet('nuget','npm','vsix','docker')]
    [string] $PackageType
)

$readMeText = Get-Content $ReadMePath
$appendMode = $true

foreach ($line in $readMeText) {
    if ($line -match "<!--\s*BEGIN-([^>]+?)\s*-->") {
        $payload = $matches[1]
        $parts = $payload -split ';'
        foreach ($pair in $parts) {
            $kv = $pair -split ':'
            if ($kv[0] -eq $PackageType) {
                if ($kv[1] -eq 'R') {
                    $appendMode = $false
                } else {
                    $appendMode = $true
                }
            }
        }
    }

    if ($line -match "<!--\s*END-([^>]+?)\s*-->") {
        $appendMode = $true
    }
}
