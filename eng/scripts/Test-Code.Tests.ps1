# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

BeforeAll {
    foreach ($file in @('Test-Code.ps1', 'New-BuildInfo.ps1')) {
        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot $file), [ref]$null, [ref]$parseErrors)
        if ($parseErrors.Count -ne 0) {
            throw "Unable to parse $file"
        }
        foreach ($name in @('FilterTestProjects', 'CreateTestSolution', 'Get-TestProjects')) {
            $definition = $ast.Find({
                param($node)
                $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name
            }, $true)
            if ($definition) {
                Set-Item -Path "Function:$name" -Value $definition.Body.GetScriptBlock()
            }
        }
    }

    function New-TestProjectFixture {
        param([string] $RelativePath, [bool] $Unit, [bool] $Live)

        $fullPath = Join-Path $RepoRoot $RelativePath
        New-Item -ItemType Directory -Path (Split-Path $fullPath -Parent) -Force | Out-Null
        Set-Content -LiteralPath $fullPath -Value @"
<Project>
  <PropertyGroup>
    <HasUnitTests>$Unit</HasUnitTests>
    <HasLiveTests>$Live</HasLiveTests>
  </PropertyGroup>
</Project>
"@
        return @{
            path = $RelativePath
            hasUnitTests = $Unit
            hasLiveTests = $Live
        }
    }
}

Describe 'Initialize test project manifest' {
    BeforeEach {
        $ErrorActionPreference = 'Stop'
        $RepoRoot = Join-Path $TestDrive 'checkout with spaces'
        $projects = @(
            (New-TestProjectFixture 'tools/Example/tests/Example.Tests/Example.Tests.csproj' $true $false),
            (New-TestProjectFixture 'tools/Example/tests/Additional.Tests/Additional.Tests.csproj' $false $true),
            (New-TestProjectFixture 'tools/ExampleExtra/tests/ExampleExtra.Tests/ExampleExtra.Tests.csproj' $true $false),
            (New-TestProjectFixture 'tools/Unrelated/tests/Unrelated.Tests/Unrelated.Tests.csproj' $true $false),
            (New-TestProjectFixture 'servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.Tests/Azure.Mcp.Server.Tests.csproj' $true $false),
            (New-TestProjectFixture 'tools/Example/tests/Helper.Tests/Helper.Tests.csproj' $false $false)
        )
        Mock dotnet {
            $project = [xml](Get-Content -LiteralPath $args[1] -Raw)
            @{
                Properties = @{
                    HasUnitTests = $project.Project.PropertyGroup.HasUnitTests
                    HasLiveTests = $project.Project.PropertyGroup.HasLiveTests
                }
            } | ConvertTo-Json
        }
    }

    It 'reuses known metadata while retaining additional projects, prefix matching, and the server canary' {
        $scope = @(
            @{ path = 'tools/Example'; hasUnitTests = $true; hasLiveTests = $false },
            @{ path = 'servers/Azure.Mcp.Server'; hasUnitTests = $true; hasLiveTests = $false }
        )

        $actual = @(Get-TestProjects $scope)

        $actual.path | Should -Be (@($projects[0..2].path) + $projects[4].path | Sort-Object)
        ($actual | Where-Object path -eq $projects[1].path).hasLiveTests | Should -BeTrue
        Should -Invoke dotnet -Times 3 -Exactly
    }

    It 'keeps an empty scope empty without evaluating any project' {
        @(Get-TestProjects @()).Count | Should -Be 0

        Should -Invoke dotnet -Times 0
    }
}

Describe 'Test-Code cached selection' {
    BeforeEach {
        $ErrorActionPreference = 'Stop'
        $RepoRoot = Join-Path $TestDrive 'different checkout & spaces'
        $projects = @(
            (New-TestProjectFixture 'tools/Example/tests/Unit & More.Tests/Unit.Tests.csproj' $true $false),
            (New-TestProjectFixture 'tools/Example/tests/Live.Tests/Live.Tests.csproj' $false $true),
            (New-TestProjectFixture 'tools/Example/tests/Both.Tests/Both.Tests.csproj' $true $true),
            (New-TestProjectFixture 'tools/Example/tests/Legacy.Tests/Legacy.Tests.csproj' $false $true),
            (New-TestProjectFixture 'tools/Other/tests/Other.Tests/Other.Tests.csproj' $true $false)
        )
        foreach ($project in $projects[1..2]) {
            Set-Content -LiteralPath (Join-Path (Split-Path (Join-Path $RepoRoot $project.path) -Parent) 'assets.json') -Value '{}'
        }
        $BuildInfo = @{
            pathsToTest = @(@{ path = 'tools/Example' })
            testProjects = $projects
        } | ConvertTo-Json -Depth 5 | ConvertFrom-Json -AsHashtable
        $useCachedTestProjects = $true
        $TestType = 'All'
        $Paths = $null
        Mock dotnet { throw 'Cached selection must not evaluate projects.' }
    }

    It 'selects <mode> projects from a JSON-round-tripped manifest' -TestCases @(
        @{ mode = 'Unit'; indexes = @(0, 2) },
        @{ mode = 'Live'; indexes = @(1, 2, 3) },
        @{ mode = 'Recorded'; indexes = @(1, 2) },
        @{ mode = 'All'; indexes = @(0, 1, 2, 3) }
    ) {
        param($mode, $indexes)
        $TestType = $mode

        $actual = @(FilterTestProjects)
        $expected = @($indexes | ForEach-Object { Join-Path $RepoRoot $projects[$_].path })

        $actual | Should -Be $expected
        Should -Invoke dotnet -Times 0
    }

    It 'preserves normalized wildcard path filters' {
        $Paths = @('Example\tests\B*')

        @(FilterTestProjects) | Should -Be @(Join-Path $RepoRoot $projects[2].path)
    }

    It 'does not fall back to discovery for an explicitly empty manifest' {
        $BuildInfo.testProjects = @()

        { FilterTestProjects } | Should -Throw '*No test projects found*'
        Should -Invoke dotnet -Times 0
    }

    It 'reports a missing manifest project' {
        $BuildInfo.testProjects[0].path = 'tools/missing/Missing.Tests.csproj'

        { FilterTestProjects } | Should -Throw '*Test project from build info not found*'
        Should -Invoke dotnet -Times 0
    }

    It 'creates a portable and XML-escaped solution without invoking dotnet' {
        $selectedProjects = @(FilterTestProjects)
        $workPath = Join-Path $RepoRoot '.work\tests'
        New-Item -ItemType Directory -Path $workPath -Force | Out-Null

        $solution = CreateTestSolution -workPath $workPath -testProjects $selectedProjects
        $xml = [xml](Get-Content -LiteralPath $solution -Raw)
        $xml.Solution.Project.Count | Should -Be $selectedProjects.Count
        $actual = @($xml.Solution.Project | ForEach-Object {
            [IO.Path]::IsPathRooted($_.Path) | Should -BeFalse
            [IO.Path]::GetFullPath((Join-Path $workPath $_.Path))
        })
        $actual | Should -Be @($selectedProjects | ForEach-Object { [IO.Path]::GetFullPath($_) })
        Should -Invoke dotnet -Times 0
    }

    It 'retains discovery for local runs and older build info' -TestCases @(
        @{ scoped = $false },
        @{ scoped = $true }
    ) {
        param($scoped)
        $useCachedTestProjects = $false
        if ($scoped) {
            $BuildInfo.Remove('testProjects')
        }
        else {
            $BuildInfo = $null
        }
        Mock dotnet {
            $project = [xml](Get-Content -LiteralPath $args[1] -Raw)
            @{
                Properties = @{
                    HasUnitTests = $project.Project.PropertyGroup.HasUnitTests
                    HasLiveTests = $project.Project.PropertyGroup.HasLiveTests
                }
            } | ConvertTo-Json
        }

        $actual = @(FilterTestProjects | ForEach-Object { $_.Replace('\', '/') } | Sort-Object)
        $expected = @($projects | Where-Object { !$scoped -or $_.path.StartsWith('tools/Example') } |
            ForEach-Object { (Join-Path $RepoRoot $_.path).Replace('\', '/') } | Sort-Object)

        $actual | Should -Be $expected
        Should -Invoke dotnet -Times $projects.Count -Exactly
    }
}
