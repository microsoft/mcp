# cspell:ignore pathlib urllib kwargs rsplit returncode

BeforeAll {
    $repoRoot = (Resolve-Path "$PSScriptRoot\..\..").Path
    $pythonCommand = (Get-Command python -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source

    function New-AzurePackagingFixture([bool] $Native) {
        $fixtureRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString("N"))
        $scriptsDirectory = Join-Path $fixtureRoot "eng\scripts"
        $commonDirectory = Join-Path $fixtureRoot "eng\common\scripts"
        $pypiDirectory = Join-Path $fixtureRoot "eng\pypi"
        $releaseDirectory = Join-Path $fixtureRoot "releases"
        New-Item -ItemType Directory -Path $scriptsDirectory, $commonDirectory, $pypiDirectory, $releaseDirectory -Force | Out-Null
        Copy-Item -LiteralPath "$repoRoot\eng\scripts\Pack-Pypi.ps1" -Destination $scriptsDirectory
        Copy-Item -LiteralPath "$repoRoot\eng\common\scripts\SemVer.ps1" -Destination $commonDirectory
        Copy-Item -LiteralPath "$repoRoot\eng\pypi\__init__.py", "$repoRoot\eng\pypi\azure_launcher.py",
            "$repoRoot\eng\pypi\pyproject.toml.template" -Destination $pypiDirectory
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
        if ($env:MCP_PYPI_TEST_PYTHON) {
            & $env:MCP_PYPI_TEST_PYTHON -m build --wheel --no-isolation
            if ($LASTEXITCODE -ne 0) { throw "Wheel build failed." }
        }
    }
}
'@ | Set-Content -LiteralPath (Join-Path $commonDirectory "common.ps1")
        @'
param($Command, $InputReadMePath, $PackageType, $InsertPayload, $OutputDirectory)
"Fixture README" | Set-Content -LiteralPath (Join-Path $OutputDirectory "README.md")
'@ | Set-Content -LiteralPath (Join-Path $scriptsDirectory "Process-PackageReadMe.ps1")
        "Fixture license" | Set-Content -LiteralPath (Join-Path $fixtureRoot "LICENSE")
        "Fixture notices" | Set-Content -LiteralPath (Join-Path $fixtureRoot "NOTICE.txt")
        $platforms = @(
            foreach ($os in @(
                @{ Node = 'win32'; Dotnet = 'win'; Extension = '.exe' }
                @{ Node = 'darwin'; Dotnet = 'osx'; Extension = '' }
                @{ Node = 'linux'; Dotnet = 'linux'; Extension = '' }
            )) {
                foreach ($arch in @('x64', 'arm64')) {
                    $suffix = if ($Native) { '-native' } else { '' }
                    $name = "Azure.Mcp.Server-$($os.Dotnet)-$arch$suffix.zip"
                    $archive = [System.IO.Compression.ZipFile]::Open(
                        (Join-Path $releaseDirectory $name), [System.IO.Compression.ZipArchiveMode]::Create)
                    try {
                        $entry = $archive.CreateEntry("azmcp$($os.Extension)")
                        $stream = [System.IO.StreamWriter]::new($entry.Open())
                        $stream.Write("Fixture executable for $($os.Node)-$arch")
                        $stream.Dispose()
                    }
                    finally {
                        $archive.Dispose()
                    }
                    @{
                        artifactPath = "Azure.Mcp.Server/$($os.Node)-$arch"
                        native = $Native
                        specialPurpose = if ($Native) { 'native' } else { $false }
                        nodeOs = $os.Node
                        dotnetOs = $os.Dotnet
                        architecture = $arch
                        extension = $os.Extension
                    }
                }
            }
        )
        @{
            servers = @(@{
                name = "Azure.Mcp.Server"
                artifactPath = "Azure.Mcp.Server"
                version = "1.2.3-beta.4"
                releaseTag = "Azure.Mcp.Server-1.2.3-beta.4"
                description = "Azure MCP server"
                cliName = "azmcp"
                pypiPackageName = "msmcp-azure"
                pypiPackageKeywords = @("mcp")
                readmeUrl = "https://github.com/microsoft/mcp"
                readmePath = "README.md"
                mcpRepositoryName = "io.github.microsoft/azure"
                platforms = $platforms
            })
        } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $fixtureRoot "build_info.json")
        return $fixtureRoot
    }

    function Invoke-AzureFixturePackaging([string] $FixtureRoot) {
        & (Join-Path $FixtureRoot "eng\scripts\Pack-Pypi.ps1") `
            -ArtifactsPath (Join-Path $FixtureRoot "no-binaries-needed") `
            -BuildInfoPath (Join-Path $FixtureRoot "build_info.json") `
            -ReleaseArtifactsPath (Join-Path $FixtureRoot "releases") `
            -OutputPath (Join-Path $FixtureRoot "packages") 6>$null 3>$null
    }
}

Describe "Azure PyPI launcher packaging" {
    It "creates one small universal launcher covering all platforms (native: <Native>)" -ForEach @(
        @{ Native = $false }
        @{ Native = $true }
    ) {
        $fixtureRoot = New-AzurePackagingFixture $Native
        Invoke-AzureFixturePackaging $fixtureRoot
        $LASTEXITCODE | Should -Be 0
        $moduleName = if ($Native) { "msmcp_azure_native" } else { "msmcp_azure" }
        $package = Join-Path $fixtureRoot "staged\$moduleName"
        Test-Path (Join-Path $package "bin") | Should -BeFalse
        $init = Get-Content -LiteralPath (Join-Path $package "__init__.py") -Raw
        $init | Should -Not -Match '{{'
        $init | Should -Match '__version__ = "1.2.3b4"'
        $manifest = Get-Content -LiteralPath (Join-Path $package "release.json") -Raw | ConvertFrom-Json -AsHashtable
        $manifest.version | Should -Be "1.2.3-beta.4"
        $manifest.platforms.Count | Should -Be 6
        foreach ($key in $manifest.platforms.Keys) {
            $artifact = $manifest.platforms[$key]
            $assetName = ($artifact.url -split '/')[-1]
            $archivePath = Join-Path $fixtureRoot "releases\$assetName"
            $artifact.url | Should -Be "https://github.com/microsoft/mcp/releases/download/Azure.Mcp.Server-1.2.3-beta.4/$assetName"
            $artifact.sha256 | Should -Be (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
            $artifact.size | Should -Be (Get-Item -LiteralPath $archivePath).Length
            $artifact.files.Count | Should -Be 1
            @($artifact.files.Values)[0].sha256 | Should -Match '^[0-9a-f]{64}$'
        }
        $pythonTest = @'
import importlib.util
import io
import os
import sys
from pathlib import Path
from unittest.mock import patch

package = Path(sys.argv[1])
releases = Path(sys.argv[2])
spec = importlib.util.spec_from_file_location("packaged_launcher", package / "__init__.py")
launcher = importlib.util.module_from_spec(spec)
spec.loader.exec_module(launcher)
assert launcher.__version__ == "1.2.3b4"
with patch.dict(os.environ, {"AZURE_MCP_CACHE_DIR": str(package.parent / "cache")}), patch(
    "urllib.request.build_opener"
) as opener, patch("subprocess.run") as run:
    opener.return_value.open.side_effect = lambda request, **kwargs: io.BytesIO(
        (releases / request.full_url.rsplit("/", 1)[1]).read_bytes()
    )
    run.return_value.returncode = 17
    for key in ("win32-x64", "win32-arm64", "darwin-x64", "darwin-arm64", "linux-x64", "linux-arm64"):
        with patch.object(launcher, "_platform_key", return_value=key):
            assert launcher.run_executable(["server", "start"]) == 17
            executable = launcher.get_executable_path()
            expected = "azmcp.exe" if key.startswith("win32-") else "azmcp"
            assert executable.name == expected
            assert executable.read_text() == f"Fixture executable for {key}"
            assert run.call_args.args[0] == [str(executable), "server", "start"]
    assert opener.return_value.open.call_count == 6
'@
        $pythonOutput = & $pythonCommand -c $pythonTest $package (Join-Path $fixtureRoot "releases") 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($pythonOutput -join [Environment]::NewLine)
        # Opt into real wheel creation when the caller supplies isolated build tools.
        if ($env:MCP_PYPI_TEST_PYTHON) {
            $wheels = @(Get-ChildItem (Join-Path $fixtureRoot "packages") -Recurse -Filter "*.whl")
            $wheels.Count | Should -Be 1
            $wheels[0].Name | Should -Be "${moduleName}-1.2.3b4-py3-none-any.whl"
            $expectedDirectory = if ($Native) { 'Azure.Mcp.Server-native' } else { 'Azure.Mcp.Server' }
            $wheels[0].Directory.Name | Should -Be $expectedDirectory
            $wheels[0].Length | Should -BeLessThan 100KB
            $archive = [System.IO.Compression.ZipFile]::OpenRead($wheels[0].FullName)
            try {
                @($archive.Entries | Where-Object FullName -Match '/bin/').Count | Should -Be 0
                $archive.GetEntry("$moduleName/release.json") | Should -Not -BeNullOrEmpty
                $archive.GetEntry("$moduleName/__init__.py") | Should -Not -BeNullOrEmpty
                $reader = [System.IO.StreamReader]::new($archive.GetEntry("$moduleName-1.2.3b4.dist-info/WHEEL").Open())
                try {
                    $reader.ReadToEnd() | Should -Match 'Tag: py3-none-any'
                }
                finally {
                    $reader.Dispose()
                }
            }
            finally {
                $archive.Dispose()
            }
        }
    }

    It "fails closed when a configured release ZIP is missing" {
        $fixtureRoot = New-AzurePackagingFixture $false
        Remove-Item -LiteralPath (Join-Path $fixtureRoot "releases\Azure.Mcp.Server-linux-arm64.zip")
        { Invoke-AzureFixturePackaging $fixtureRoot } | Should -Throw "*Required signed release archive*"
    }
}
