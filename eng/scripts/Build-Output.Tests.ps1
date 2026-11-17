# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# cspell:ignore nologo

BeforeAll {
    $targetsPath = Join-Path $PSScriptRoot '..\..\Directory.Build.targets'
    $propsPath = Join-Path $PSScriptRoot '..\..\Directory.Build.props'

    function New-LocationFixture {
        param([string] $Root)

        $serverDirectory = Join-Path $Root 'server'
        $output = Join-Path $Root 'test output'
        New-Item -ItemType Directory -Path $serverDirectory, $output -Force | Out-Null
        $serverProject = Join-Path $serverDirectory 'Azure.Mcp.Server.csproj'
        Set-Content -LiteralPath $serverProject -Value @'
<Project>
  <ItemGroup>
    <ServerOutput Include="$(MSBuildProjectDirectory)\custom output\$(Configuration)\$(TargetFramework)\$(RuntimeIdentifier)\azmcp.dll"
                  Condition="'$(NoOutput)' != 'true'" />
  </ItemGroup>
  <Target Name="GetTargetPath" Returns="@(ServerOutput)" />
</Project>
'@
        $project = Join-Path $Root 'Consumer.proj'
        $escapedRoot = [System.Security.SecurityElement]::Escape($Root)
        $escapedOutput = [System.Security.SecurityElement]::Escape($output)
        $escapedTargets = [System.Security.SecurityElement]::Escape($targetsPath)
        Set-Content -LiteralPath $project -Value @"
<Project>
  <PropertyGroup>
    <TargetDir>$escapedOutput\</TargetDir>
    <Configuration>Debug</Configuration>
    <RuntimeIdentifier>linux-arm64</RuntimeIdentifier>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$escapedRoot\server\Azure.Mcp.Server.csproj">
      <SetConfiguration>Configuration=Release</SetConfiguration>
      <SetTargetFramework>TargetFramework=net10.0</SetTargetFramework>
    </ProjectReference>
  </ItemGroup>
  <Target Name="ResolveProjectReferences">
    <ItemGroup>
      <_MSBuildProjectReferenceExistent Include="@(ProjectReference)" />
    </ItemGroup>
  </Target>
  <Import Project="$escapedTargets" />
</Project>
"@
        return @{
            Project = $project
            Marker = Join-Path $output 'AzureMcpServer.path'
            RelativeServerPath = [IO.Path]::GetRelativePath($output,
                (Join-Path $serverDirectory 'custom output\Release\net10.0\linux-arm64'))
        }
    }
}

Describe 'Evaluated Azure server locations' {
    It 'uses the referenced configuration, target framework, RID and custom server output' {
        $fixture = New-LocationFixture -Root (Join-Path $TestDrive 'location with spaces')

        $result = & dotnet msbuild $fixture.Project -target:WriteAzureMcpServerLocation -property:RuntimeIdentifier=linux-arm64 -nologo -verbosity:quiet 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
        $relativePath = [IO.Path]::TrimEndingDirectorySeparator((Get-Content -LiteralPath $fixture.Marker -Raw).Trim())
        $relativePath | Should -Be $fixture.RelativeServerPath
    }

    It 'fails explicitly instead of writing a misleading marker when no server output is returned' {
        $fixture = New-LocationFixture -Root (Join-Path $TestDrive 'missing output')

        $result = & dotnet msbuild $fixture.Project -target:WriteAzureMcpServerLocation -property:NoOutput=true -nologo -verbosity:quiet 2>&1
        $LASTEXITCODE | Should -Not -Be 0
        ($result -join [Environment]::NewLine) | Should -Match 'Expected one Azure MCP server output'
        Test-Path -LiteralPath $fixture.Marker | Should -BeFalse
    }

    It 'does not materialize runtime locations during design-time builds' {
        $fixture = New-LocationFixture -Root (Join-Path $TestDrive 'design time')

        $result = & dotnet msbuild $fixture.Project -target:WriteAzureMcpServerLocation -property:DesignTimeBuild=true -nologo -verbosity:quiet 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
        Test-Path -LiteralPath $fixture.Marker | Should -BeFalse
    }
}

Describe 'In-process Azure server test hosts' {
    # These hosts copy the full server runtime. Sharing one output directory lets a
    # parallel build overwrite assemblies another host has already loaded.
    It 'build into distinct directories under their own projects' {
        $root = Split-Path (Resolve-Path $propsPath).Path -Parent
        $projects = @(
            'core\Azure.Mcp.Core\tests\Azure.Mcp.Core.Tests\Azure.Mcp.Core.Tests.csproj',
            'servers\Azure.Mcp.Server\tests\Azure.Mcp.Server.Tests\Azure.Mcp.Server.Tests.csproj',
            'servers\Azure.Mcp.Server\perf\Azure.Mcp.Server.PerformanceBenchmarks\Azure.Mcp.Server.PerformanceBenchmarks.csproj'
        )

        $targetDirectories = foreach ($relativeProject in $projects) {
            $project = Join-Path $root $relativeProject
            $result = & dotnet msbuild $project -nologo -getProperty:TargetDir 2>&1
            $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
            $targetDir = [IO.Path]::GetFullPath(($result -join '').Trim())
            $projectDirectory = [IO.Path]::GetFullPath((Split-Path $project -Parent)) + [IO.Path]::DirectorySeparatorChar
            $targetDir.StartsWith($projectDirectory, [StringComparison]::OrdinalIgnoreCase) |
                Should -BeTrue -Because "$relativeProject should build to its own directory, not $targetDir"
            $targetDir
        }

        @($targetDirectories | Sort-Object -Unique).Count | Should -Be $projects.Count
    }
}

Describe 'Production output remains independent of test outputs' {
    BeforeDiscovery {
        $profiles = @(
            @{ Configuration = 'Debug'; RuntimeIdentifier = ''; BuildNative = 'false' }
            foreach ($runtime in @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64',
                    'osx-x64', 'osx-arm64', 'linux-musl-x64', 'linux-musl-arm64')) {
                @{ Configuration = 'Release'; RuntimeIdentifier = $runtime; BuildNative = 'false' }
            }
            @{ Configuration = 'Release'; RuntimeIdentifier = 'linux-x64'; BuildNative = 'true' }
        )
        $cases = foreach ($server in @('Azure.Mcp.Server', 'Fabric.Mcp.Server', 'Template.Mcp.Server')) {
            foreach ($profile in $profiles) {
                @{
                    Server = $server
                    Configuration = $profile.Configuration
                    RuntimeIdentifier = $profile.RuntimeIdentifier
                    BuildNative = $profile.BuildNative
                }
            }
        }
    }

    It 'preserves <Server> <Configuration> <RuntimeIdentifier> native=<BuildNative> layout and packaging properties' -ForEach $cases {
        $root = Split-Path (Resolve-Path $propsPath).Path -Parent
        $project = Join-Path $root "servers\$Server\src\$Server.csproj"
        $properties = 'MSBuildProjectDirectory,OutputPath,TargetPath,PublishDir,IsServerProject,IsPackable,PackAsTool,DebugType,CopyOutputSymbolsToPublishDirectory,PublishAot,SelfContained,PublishTrimmed'

        $result = & dotnet msbuild $project -nologo "-property:Configuration=$Configuration" `
            "-property:RuntimeIdentifier=$RuntimeIdentifier" "-property:BuildNative=$BuildNative" `
            "-getProperty:$properties" -getItem:_AzureMcpServerReference 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
        $evaluation = ($result -join [Environment]::NewLine) | ConvertFrom-Json
        $actual = $evaluation.Properties
        foreach ($property in @('MSBuildProjectDirectory', 'OutputPath', 'TargetPath', 'PublishDir')) {
            $actual.$property = $actual.$property.Replace('\', '/').Replace([char]'/', [IO.Path]::DirectorySeparatorChar)
        }
        $expectedOutput = Join-Path 'bin' $Configuration
        if ($Server -ne 'Fabric.Mcp.Server') {
            $expectedOutput = Join-Path $expectedOutput 'net10.0'
        }
        if ($RuntimeIdentifier) {
            $expectedOutput = Join-Path $expectedOutput $RuntimeIdentifier
        }

        [IO.Path]::TrimEndingDirectorySeparator($actual.OutputPath) | Should -Be $expectedOutput
        [IO.Path]::GetDirectoryName($actual.TargetPath) |
            Should -Be (Join-Path $actual.MSBuildProjectDirectory $expectedOutput)
        [IO.Path]::TrimEndingDirectorySeparator($actual.PublishDir) |
            Should -Be (Join-Path $expectedOutput 'publish')
        $actual.IsServerProject | Should -Be 'true'
        $actual.IsPackable | Should -Be 'true'
        $actual.PackAsTool | Should -Be 'true'
        $actual.DebugType | Should -Be 'portable'
        $actual.CopyOutputSymbolsToPublishDirectory | Should -Be 'true'
        @($evaluation.Items._AzureMcpServerReference).Count | Should -Be 0
        if ($BuildNative -eq 'true') {
            $actual.PublishAot | Should -Be 'true'
            $actual.SelfContained | Should -Be 'true'
            $actual.PublishTrimmed | Should -Be 'true'
        }
        else {
            $actual.PublishAot | Should -Not -Be 'true'
            $actual.SelfContained | Should -Be 'false'
            $actual.PublishTrimmed | Should -Be 'false'
        }
    }
}
