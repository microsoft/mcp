# cspell:ignore runpy pathlib returncode

Describe "PyPI package version conversion" {
    BeforeAll {
        . "$PSScriptRoot/../common/scripts/SemVer.ps1"

        $scriptPath = Join-Path $PSScriptRoot "Pack-Pypi.ps1"
        $parseTokens = $null
        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $scriptPath, [ref]$parseTokens, [ref]$parseErrors)
        if ($parseErrors.Count -ne 0) {
            throw "Unable to parse $scriptPath"
        }

        # Load the production converter without running packaging or installing Python dependencies.
        $converter = $ast.Find({
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq "ConvertTo-PythonPackageVersion"
        }, $true)
        if (!$converter) {
            throw "Unable to find ConvertTo-PythonPackageVersion in $scriptPath"
        }
        . ([scriptblock]::Create($converter.Extent.Text))

        $pyprojectTemplate = Get-Content (Join-Path $PSScriptRoot "../pypi/pyproject.toml.template") -Raw
    }

    It "returns only the Python version for <Version>" -TestCases @(
        @{ Version = "1.2.3"; Expected = "1.2.3" }
        @{ Version = "1.2.3-beta.4"; Expected = "1.2.3b4" }
        @{ Version = "1.2.3-alpha.4"; Expected = "1.2.3a4" }
        @{ Version = "1.2.3-alpha.20261019.1"; Expected = "1.2.3a202610191" }
    ) {
        param($Version, $Expected)

        $result = ConvertTo-PythonPackageVersion $Version

        $result | Should -BeOfType ([string])
        @($result) | Should -HaveCount 1
        $result | Should -Be $Expected
        $pyprojectContent = $pyprojectTemplate -replace '{{VERSION}}', $result.ToString()
        $pyprojectContent | Should -Match "(?m)^version = `"$([regex]::Escape($Expected))`"\r?$"
    }

    It "returns no version for invalid input" {
        ConvertTo-PythonPackageVersion "not-a-version" | Should -BeNullOrEmpty
    }
}

# cspell:ignore runpy pathlib returncode

BeforeAll {
    $repoRoot = (Resolve-Path "$PSScriptRoot\..\..").Path
    $pythonCommand = (Get-Command python -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
}

Describe "Pack-Pypi executable configuration" {
    It "launches <CliName> on <System> (native: <Native>)" -ForEach @(
        foreach ($cliName in @("azmcp", "fabmcp", "mcptmp")) {
            foreach ($system in @("Windows", "Linux", "Darwin")) {
                foreach ($native in @($false, $true)) {
                    @{ CliName = $cliName; System = $system; Native = $native }
                }
            }
        }
    ) {
        $fixtureRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString("N"))
        $scriptsDirectory = Join-Path $fixtureRoot "eng\scripts"
        $commonDirectory = Join-Path $fixtureRoot "eng\common\scripts"
        $pypiDirectory = Join-Path $fixtureRoot "eng\pypi"
        $platformDirectory = Join-Path $fixtureRoot "artifacts\test-server\platform"
        New-Item -ItemType Directory -Path $scriptsDirectory, $commonDirectory, $pypiDirectory, $platformDirectory -Force | Out-Null
        Copy-Item -LiteralPath "$repoRoot\eng\scripts\Pack-Pypi.ps1" -Destination $scriptsDirectory
        Copy-Item -LiteralPath "$repoRoot\eng\common\scripts\SemVer.ps1" -Destination $commonDirectory
        Copy-Item -LiteralPath "$repoRoot\eng\pypi\__init__.py", "$repoRoot\eng\pypi\pyproject.toml.template" -Destination $pypiDirectory

        @'
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
. "$PSScriptRoot\SemVer.ps1"
function LogError { param([string]$Message) throw $Message }
function Invoke-LoggedCommand {
    param([string]$Command)
    if ($Command -like "* -m build --wheel") {
        $stagedDirectory = Join-Path $RepoRoot "staged"
        New-Item -ItemType Directory -Path $stagedDirectory -Force | Out-Null
        Copy-Item -Path (Join-Path (Get-Location) "src\*") -Destination $stagedDirectory -Recurse -Force
        Copy-Item -LiteralPath (Join-Path (Get-Location) "pyproject.toml") -Destination $stagedDirectory
    }
}
'@ | Set-Content -LiteralPath (Join-Path $commonDirectory "common.ps1")
        @'
param($Command, $InputReadMePath, $PackageType, $InsertPayload, $OutputDirectory)
'@ | Set-Content -LiteralPath (Join-Path $scriptsDirectory "Process-PackageReadMe.ps1")
        "Fixture license" | Set-Content -LiteralPath (Join-Path $fixtureRoot "LICENSE")
        "Fixture notices" | Set-Content -LiteralPath (Join-Path $fixtureRoot "NOTICE.txt")

        $extension = if ($System -eq "Windows") { ".exe" } else { "" }
        $nodeOs = switch ($System) {
            "Windows" { "win32" }
            "Linux" { "linux" }
            "Darwin" { "darwin" }
        }
        $executableName = "$CliName$extension"
        "Fixture executable" | Set-Content -LiteralPath (Join-Path $platformDirectory $executableName)
        $buildInfo = @{
            servers = @(
                @{
                    name = "Test.Mcp.Server"
                    artifactPath = "test-server"
                    version = "1.2.3"
                    description = "Test MCP server"
                    cliName = $CliName
                    pypiPackageName = "msmcp-test"
                    pypiPackageKeywords = @("mcp")
                    readmeUrl = "https://github.com/microsoft/mcp"
                    readmePath = "README.md"
                    mcpRepositoryName = "io.github.microsoft/test"
                    platforms = @(
                        @{
                            artifactPath = "test-server\platform"
                            native = $Native
                            specialPurpose = $false
                            nodeOs = $nodeOs
                            architecture = "x64"
                            extension = $extension
                        }
                    )
                }
            )
        }
        $buildInfoPath = Join-Path $fixtureRoot "build_info.json"
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & (Join-Path $scriptsDirectory "Pack-Pypi.ps1") `
            -ArtifactsPath (Join-Path $fixtureRoot "artifacts") `
            -BuildInfoPath $buildInfoPath `
            -OutputPath (Join-Path $fixtureRoot "packages") 6>$null 3>$null

        $LASTEXITCODE | Should -Be 0
        $moduleName = if ($Native) { "msmcp_test_native" } else { "msmcp_test" }
        $initPyPath = Join-Path $fixtureRoot "staged\$moduleName\__init__.py"
        (Get-Content -LiteralPath $initPyPath -Raw).Contains("{{CLI_NAME}}") | Should -BeFalse
        Get-Content -LiteralPath (Join-Path $fixtureRoot "staged\pyproject.toml") -Raw |
            Should -Match ([regex]::Escape("$CliName = `"${moduleName}:main`""))

        $pythonTest = @'
import runpy
import sys
from pathlib import Path
from unittest.mock import patch

launcher = runpy.run_path(sys.argv[1])
expected = Path(sys.argv[1]).parent / "bin" / sys.argv[2]
with patch("platform.system", return_value=sys.argv[3]):
    actual = launcher["get_executable_path"]()
    assert actual == expected, f"Expected {expected}, got {actual}"
    assert actual.is_file(), f"Packaged executable missing: {actual}"
    with patch("subprocess.run") as run:
        run.return_value.returncode = 17
        assert launcher["run_executable"](["server", "start"]) == 17
        run.assert_called_once_with(
            [str(expected), "server", "start"],
            stdin=sys.stdin,
            stdout=sys.stdout,
            stderr=sys.stderr,
        )
'@
        $pythonOutput = & $pythonCommand -c $pythonTest $initPyPath $executableName $System 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($pythonOutput -join [Environment]::NewLine)
    }
}
