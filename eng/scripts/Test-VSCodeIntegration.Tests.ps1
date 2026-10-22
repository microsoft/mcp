# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
#
# Pester tests for Test-VSCodeIntegration.ps1 artifact selection and executable resolution.
# Run with:
#   Invoke-Pester -Path eng/scripts/Test-VSCodeIntegration.Tests.ps1

Describe "Test-VSCodeIntegration executable resolution and artifact selection" {
    BeforeAll {
        $sourceScript = Join-Path $PSScriptRoot "Test-VSCodeIntegration.ps1"
        $originalServerExecutable = $env:MCP_SERVER_EXECUTABLE
        $originalPipelineWorkspace = $env:PIPELINE_WORKSPACE
    }

    AfterAll {
        $env:MCP_SERVER_EXECUTABLE = $originalServerExecutable
        $env:PIPELINE_WORKSPACE = $originalPipelineWorkspace
    }

    BeforeEach {
        $env:MCP_SERVER_EXECUTABLE = $null
        $env:PIPELINE_WORKSPACE = $null

        $fixtureRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString("N"))
        $scriptsDirectory = Join-Path $fixtureRoot "eng\scripts"
        $commonDirectory = Join-Path $fixtureRoot "eng\common\scripts"
        $vscodeDirectory = Join-Path $fixtureRoot "servers\Azure.Mcp.Server\vscode"
        $artifactsDirectory = Join-Path $fixtureRoot "artifacts"

        New-Item -ItemType Directory -Path $scriptsDirectory, $commonDirectory, $vscodeDirectory, $artifactsDirectory -Force | Out-Null
        $scriptPath = Join-Path $scriptsDirectory "Test-VSCodeIntegration.ps1"
        Copy-Item -LiteralPath $sourceScript -Destination $scriptPath

        $commandLogPath = Join-Path $fixtureRoot "commands.txt"
        @'
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
function LogWarning { param([string]$Message) Write-Warning $Message }
function LogError { param([string]$Message) throw $Message }
function Invoke-LoggedCommand {
    param([string]$Command)
    Add-Content -LiteralPath (Join-Path $RepoRoot "commands.txt") -Value "CMD:$Command;EXE:$env:MCP_SERVER_EXECUTABLE"
}
'@ | Set-Content -LiteralPath (Join-Path $commonDirectory "common.ps1")
    }

    It "resolves windows-x64 artifact when both x64 and arm64 artifacts exist" {
        $x64Dir = Join-Path $artifactsDirectory "Azure.Mcp.Server-windows-x64"
        $arm64Dir = Join-Path $artifactsDirectory "Azure.Mcp.Server-windows-arm64"
        New-Item -ItemType Directory -Path $x64Dir, $arm64Dir -Force | Out-Null

        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        $x64Exe = Join-Path $x64Dir $exeName
        $arm64Exe = Join-Path $arm64Dir $exeName
        Set-Content -LiteralPath $x64Exe -Value "fake-x64"
        Set-Content -LiteralPath $arm64Exe -Value "fake-arm64"

        & $scriptPath -ArtifactsPath $artifactsDirectory -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"

        $commands = Get-Content -LiteralPath (Join-Path $fixtureRoot "commands.txt")
        $commands | Should -Match "windows-x64"
        $commands | Should -Not -Match "windows-arm64"
    }

    It "rejects ambiguous artifacts when multiple windows-x64 binaries exist" {
        $dir1 = Join-Path $artifactsDirectory "run1\Azure.Mcp.Server-windows-x64"
        $dir2 = Join-Path $artifactsDirectory "run2\Azure.Mcp.Server-windows-x64"
        New-Item -ItemType Directory -Path $dir1, $dir2 -Force | Out-Null

        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        Set-Content -LiteralPath (Join-Path $dir1 $exeName) -Value "fake1"
        Set-Content -LiteralPath (Join-Path $dir2 $exeName) -Value "fake2"

        {
            & $scriptPath -ArtifactsPath $artifactsDirectory -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"
        } | Should -Throw "*Ambiguous MCP server executable*"
    }

    It "fails when no windows-x64 artifact exists in artifacts directory" {
        $arm64Dir = Join-Path $artifactsDirectory "Azure.Mcp.Server-windows-arm64"
        New-Item -ItemType Directory -Path $arm64Dir -Force | Out-Null

        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        Set-Content -LiteralPath (Join-Path $arm64Dir $exeName) -Value "fake-arm64"

        {
            & $scriptPath -ArtifactsPath $artifactsDirectory -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"
        } | Should -Throw "*MCP server executable not found under*"
    }

    It "uses explicit -ServerExecutablePath when provided" {
        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        $customExe = Join-Path $fixtureRoot "custom_$exeName"
        Set-Content -LiteralPath $customExe -Value "custom"

        & $scriptPath -ServerExecutablePath $customExe -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"

        $commands = Get-Content -LiteralPath (Join-Path $fixtureRoot "commands.txt")
        $commands | Should -Match "custom_$exeName"
    }

    It "uses `$env:MCP_SERVER_EXECUTABLE when set in environment" {
        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        $envExe = Join-Path $fixtureRoot "env_$exeName"
        Set-Content -LiteralPath $envExe -Value "env"
        $env:MCP_SERVER_EXECUTABLE = $envExe

        & $scriptPath -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"

        $commands = Get-Content -LiteralPath (Join-Path $fixtureRoot "commands.txt")
        $commands | Should -Match "env_$exeName"
    }

    It "runs lifecycle regression tests before outerloop test" {
        $exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }
        $customExe = Join-Path $fixtureRoot "custom_$exeName"
        Set-Content -LiteralPath $customExe -Value "custom"

        & $scriptPath -ServerExecutablePath $customExe -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"

        $commands = Get-Content -LiteralPath (Join-Path $fixtureRoot "commands.txt")
        $commands[0] | Should -Match "npm ci --omit=optional"
        $commands[1] | Should -Match "node --test src/test/outerloop/mcpProcessLifecycle.test.js"
        $commands[2] | Should -Match "npm run outerloop-test"
    }

    It "fails when no executable can be found" {
        {
            & $scriptPath -VsCodeProjectPath "servers\Azure.Mcp.Server\vscode"
        } | Should -Throw "*MCP server executable not found. Please provide -ServerExecutablePath*"
    }
}
