BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Update-Solution.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count -ne 0) {
        throw "Unable to parse $scriptPath"
    }

    foreach ($name in @('Get-ProjectsWithReferences', 'Update-Solution', 'Normalize-Solution')) {
        $definition = $ast.Find({
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq $name
        }, $true)
        . ([scriptblock]::Create($definition.Extent.Text))
    }

    function New-TestProject {
        param (
            [string] $Name,
            [string[]] $References = @(),
            [string] $Directory = $Name
        )

        $directory = Join-Path $TestDrive $Directory
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        $projectPath = Join-Path $directory "$Name.csproj"
        $items = foreach ($reference in $References) {
            '<ProjectReference Include="' + [System.Security.SecurityElement]::Escape($reference) + '" />'
        }
        @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net11.0</TargetFramework></PropertyGroup>
  <ItemGroup>$($items -join "`n")</ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $projectPath
        return $projectPath
    }
}

Describe 'Solution project reference discovery' {
    BeforeEach {
        $ErrorActionPreference = 'Stop'
        $PSNativeCommandUseErrorActionPreference = $true
    }

    It 'keeps a project without references' {
        $project = New-TestProject -Name 'Standalone'

        $projects = @(Get-ProjectsWithReferences -ProjectPaths $project)

        $projects | Should -HaveCount 1
        $projects | Should -Contain $project
    }

    It 'includes transitive references exactly once across shared dependencies and repeated inputs' {
        $shared = New-TestProject -Name 'Shared'
        $left = New-TestProject -Name 'Left' -References $shared
        $right = New-TestProject -Name 'Right' -References $shared
        $root = New-TestProject -Name 'Root' -References @($left, $right)

        $projects = @(Get-ProjectsWithReferences -ProjectPaths @($root, $root))

        $projects | Should -HaveCount 4
        foreach ($project in @($root, $left, $right, $shared)) {
            $projects | Should -Contain $project
        }
    }

    It 'respects evaluated reference conditions' {
        $project = New-TestProject -Name 'Conditional'
        [xml] $xml = Get-Content -LiteralPath $project -Raw
        $reference = $xml.CreateElement('ProjectReference')
        $reference.SetAttribute('Include', 'Missing.csproj')
        $reference.SetAttribute('Condition', "'false' == 'true'")
        [void]$xml.SelectSingleNode('/Project/ItemGroup').AppendChild($reference)
        $xml.Save($project)

        $projects = @(Get-ProjectsWithReferences -ProjectPaths $project)

        $projects | Should -HaveCount 1
        $projects | Should -Contain $project
    }

    It 'fails explicitly when a project does not exist' {
        $project = Join-Path $TestDrive 'Missing.csproj'

        { Get-ProjectsWithReferences -ProjectPaths $project } | Should -Throw
    }

    It 'generates a server solution with shared source and test dependencies exactly once' {
        $shared = New-TestProject -Name 'Shared' -Directory 'core\Shared\src'
        $server = New-TestProject -Name 'Test.Mcp.Server' -Directory 'servers\Test.Mcp.Server\src' -References $shared
        $null = New-TestProject -Name 'Test.Mcp.Server.Tests' -Directory 'servers\Test.Mcp.Server\tests' -References @($server, $shared)
        $RepoRoot = $TestDrive
        $Verify = $false
        $serverDirectory = Join-Path $TestDrive 'servers\Test.Mcp.Server'

        Push-Location $TestDrive
        try {
            Update-Solution -serverDirectory $serverDirectory
        }
        finally {
            Pop-Location
        }

        [xml] $solution = Get-Content -LiteralPath (Join-Path $serverDirectory 'Test.Mcp.Server.slnx') -Raw
        $paths = @($solution.SelectNodes('//Project') | ForEach-Object { $_.GetAttribute('Path') })
        $paths | Should -HaveCount 3
        $paths | Should -Contain '../../core/Shared/src/Shared.csproj'
        $paths | Should -Contain './src/Test.Mcp.Server.csproj'
        $paths | Should -Contain './tests/Test.Mcp.Server.Tests.csproj'
    }
}
