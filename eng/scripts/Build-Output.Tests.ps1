# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# cspell:ignore nologo

BeforeAll {
    $targetsPath = Join-Path $PSScriptRoot '..\..\Directory.Build.targets'
    $propsPath = Join-Path $PSScriptRoot '..\..\Directory.Build.props'

    function New-CleanFixture {
        param(
            [string] $Root,
            [switch] $CustomOutput
        )

        $output = Join-Path $Root '.work\test-runtime\Azure.Mcp.Server\Debug\net10.0'
        if ($CustomOutput) {
            $output = Join-Path $Root 'custom-output'
        }
        $intermediate = Join-Path $Root 'obj'
        New-Item -ItemType Directory -Path $output, $intermediate -Force | Out-Null
        $files = @(
            (Join-Path $output 'Azure.Mcp.Core.Tests.dll'),
            (Join-Path $output 'Azure.Mcp.Core.Tests.deps.json'),
            (Join-Path $output 'Azure.Mcp.Server.Tests.dll'),
            (Join-Path $output 'AzureMcpServer.path'),
            (Join-Path $output 'appsettings.json'),
            (Join-Path $output 'Azure.Mcp.Core.dll'),
            (Join-Path $intermediate 'generated.cache')
        )
        foreach ($file in $files) {
            Set-Content -LiteralPath $file -Value 'fixture'
        }
        $listPath = Join-Path $intermediate 'fixture.FileListAbsolute.txt'
        Set-Content -LiteralPath $listPath -Value $files
        $project = Join-Path $Root 'Azure.Mcp.Core.Tests.proj'
        $escapedRoot = [System.Security.SecurityElement]::Escape($Root)
        $escapedOutput = [System.Security.SecurityElement]::Escape($output)
        $escapedTargets = [System.Security.SecurityElement]::Escape($targetsPath)
        Set-Content -LiteralPath $project -Value @"
<Project>
  <PropertyGroup>
    <RepoRoot>$escapedRoot\</RepoRoot>
    <TargetName>Azure.Mcp.Core.Tests</TargetName>
    <TargetDir>$escapedOutput\</TargetDir>
    <TargetPath>$escapedOutput\Azure.Mcp.Core.Tests.dll</TargetPath>
    <OutDir>$escapedOutput\</OutDir>
    <IntermediateOutputPath>$escapedRoot\obj\</IntermediateOutputPath>
    <CleanFile>fixture.FileListAbsolute.txt</CleanFile>
  </PropertyGroup>
  <Import Project="`$(MSBuildToolsPath)\Microsoft.Common.CurrentVersion.targets" />
  <Import Project="$escapedTargets" />
</Project>
"@
        return @{
            Project = $project
            Files = $files
            ListPath = $listPath
        }
    }

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

Describe 'Shared Azure runtime clean ownership' {
    It 'cleans only the current host and intermediates, retaining another host and shared content' {
        $fixture = New-CleanFixture -Root (Join-Path $TestDrive 'shared runtime')

        $result = & dotnet msbuild $fixture.Project -target:CoreClean -nologo -verbosity:quiet 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)

        Test-Path -LiteralPath $fixture.Files[0] | Should -BeFalse
        Test-Path -LiteralPath $fixture.Files[1] | Should -BeFalse
        Test-Path -LiteralPath $fixture.Files[2] | Should -BeTrue
        Test-Path -LiteralPath $fixture.Files[3] | Should -BeTrue
        Test-Path -LiteralPath $fixture.Files[4] | Should -BeTrue
        Test-Path -LiteralPath $fixture.Files[5] | Should -BeTrue
        Test-Path -LiteralPath $fixture.Files[6] | Should -BeFalse
        @(Get-Content -LiteralPath $fixture.ListPath).Count | Should -Be 0
    }

    It 'preserves standard clean behavior for explicitly selected project-local output' {
        $fixture = New-CleanFixture -Root (Join-Path $TestDrive 'custom runtime') -CustomOutput

        $result = & dotnet msbuild $fixture.Project -target:CoreClean -nologo -verbosity:quiet 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)

        foreach ($file in $fixture.Files) {
            Test-Path -LiteralPath $file | Should -BeFalse
        }
    }
}

Describe 'Shared Azure runtime output selection' {
    It 'does not override the SDK artifacts layout' {
        $root = Join-Path $TestDrive 'artifacts layout'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $project = Join-Path $root 'Azure.Mcp.Core.Tests.proj'
        $escapedProps = [System.Security.SecurityElement]::Escape($propsPath)
        Set-Content -LiteralPath $project -Value @"
<Project>
  <Import Project="$escapedProps" />
</Project>
"@

        $result = & dotnet msbuild $project -nologo -property:UseArtifactsOutput=true -getProperty:BaseOutputPath 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
        ($result -join '').Trim() | Should -BeNullOrEmpty
    }

    It 'respects an explicit base output path' {
        $root = Join-Path $TestDrive 'base output'
        New-Item -ItemType Directory -Path $root -Force | Out-Null
        $project = Join-Path $root 'Azure.Mcp.Core.Tests.proj'
        $escapedProps = [System.Security.SecurityElement]::Escape($propsPath)
        Set-Content -LiteralPath $project -Value @"
<Project>
  <Import Project="$escapedProps" />
</Project>
"@

        $result = & dotnet msbuild $project -nologo -property:BaseOutputPath=custom -getProperty:BaseOutputPath 2>&1
        $LASTEXITCODE | Should -Be 0 -Because ($result -join [Environment]::NewLine)
        ($result -join '').Trim() | Should -Be 'custom'
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

Describe 'Production output remains independent of shared test runtimes' {
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
