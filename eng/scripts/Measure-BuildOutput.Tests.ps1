# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Measure-BuildOutput.ps1'
    function Add-OutputFile {
        param([string] $Path, [string] $Content = 'abcd')

        $fullPath = Join-Path $fixtureRoot $Path
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($fullPath)) | Out-Null
        [IO.File]::WriteAllText($fullPath, $Content)
    }
    function Get-SizeReport {
        param([long] $Threshold = 1, [string[]] $Extra = @())

        & $scriptPath -RepoRoot $fixtureRoot -Profile 'fixture profile' -MinimumDuplicateBytes $Threshold -AdditionalOutputPath $Extra 6>$null |
            ConvertFrom-Json
    }
}

Describe 'Measure-BuildOutput' {
    BeforeEach {
        $fixtureRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + ' with spaces')
        [IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
    }

    It 'counts disjoint categories once, including relocated shared outputs and package staging' {
        @(
            'tools\Example\bin\Debug\test.dll'
            'tools\Example\obj\Debug\test.dll'
            '.work\test-runtime\Azure.Mcp.Server\bin\Debug\test.dll'
            'tools\Example\bin\Release\publish\test.dll'
            '.work\build\server\test.dll'
            '.work\temp_pypi\bin\test.dll'
            '.work\docker_staged\test.dll'
            '.work\packages_npm\platform\test.dll'
            '.work\packages_npm\platform\server.tgz'
            '.work\packages_docker\server.tar'
            '.work\tests\test.log'
            'artifacts\build.log'
            'custom output\test.dll'
        ) | ForEach-Object { Add-OutputFile $_ }
        Add-OutputFile 'src\source.cs'
        Add-OutputFile '.nuget\packages\cache\bin\ignored.dll'
        Add-OutputFile 'node_modules\tool\bin\ignored.dll'
        $report = Get-SizeReport -Extra @('custom output')
        $report.total.fileCount | Should -Be 13
        $report.total.logicalBytes | Should -Be 52
        ($report.categories | Measure-Object fileCount -Sum).Sum | Should -Be 13
        ($report.categories | Measure-Object logicalBytes -Sum).Sum | Should -Be 52
        foreach ($expected in @{ bin = 1; obj = 1; testRuntime = 1; publish = 2; packageStaging = 3; finalPackagesAndImages = 2; otherBuildOutput = 3 }.GetEnumerator()) {
            ($report.categories | Where-Object name -EQ $expected.Key).fileCount | Should -Be $expected.Value
        }
        $report.skippedPaths.Count | Should -Be 2
    }

    It 'preserves intermediate archives as obj and published archives as publish' {
        Add-OutputFile 'obj\package.nupkg'
        Add-OutputFile 'bin\Release\publish\package.zip'
        Add-OutputFile '.work\temp\package.tgz'
        Add-OutputFile '.work\test-runtime\obj\package.zip'
        $report = Get-SizeReport
        ($report.categories | Where-Object name -EQ obj).fileCount | Should -Be 1
        ($report.categories | Where-Object name -EQ publish).fileCount | Should -Be 1
        ($report.categories | Where-Object name -EQ packageStaging).fileCount | Should -Be 1
        ($report.categories | Where-Object name -EQ testRuntime).fileCount | Should -Be 1
        ($report.categories | Where-Object name -EQ finalPackagesAndImages).fileCount | Should -Be 0
    }

    It 'counts packaged dependency directories but skips source dependencies and named build caches' {
        @(
            '.work\temp\node_modules\vsix\index.js'
            '.work\temp\packages\dependency\content.dat'
            '.work\build\server\node_modules\runtime\index.js'
            '.work\build\server\packages\runtime\content.dat'
            '.work\build\server\.dotnet\runtime.dll'
            'tools\Example\bin\Release\publish\node_modules\runtime\index.js'
            'tools\Example\bin\Release\publish\packages\runtime\content.dat'
            'bin\packages\runtime.dll'
            'custom output\node_modules\dependency\content.dat'
        ) | ForEach-Object { Add-OutputFile $_ }
        @(
            'node_modules\tool\bin\ignored.dll'
            'packages\dependency\bin\ignored.dll'
            '.dotnet\sdk\bin\ignored.dll'
            'src\node_modules\tool\bin\ignored.dll'
            '.work\.nuget\packages\dependency\bin\ignored.dll'
            '.work\cache\bin\ignored.dll'
            '.work\.dotnet\sdk\bin\ignored.dll'
            '.work\temp\.git\objects\ignored.dat'
        ) | ForEach-Object { Add-OutputFile $_ }
        $report = Get-SizeReport -Extra @('custom output')
        $report.total.fileCount | Should -Be 9
        $report.total.logicalBytes | Should -Be 36
        ($report.categories | Where-Object name -EQ packageStaging).fileCount | Should -Be 2
        ($report.categories | Where-Object name -EQ publish).fileCount | Should -Be 5
        ($report.categories | Where-Object name -EQ bin).fileCount | Should -Be 1
        ($report.categories | Where-Object name -EQ otherBuildOutput).fileCount | Should -Be 1
        $report.skippedPaths.Count | Should -Be 8
    }

    It 'recognizes both local and pipeline Docker staging names' {
        Add-OutputFile '.work\docker_staged\server\dist\server.dll'
        Add-OutputFile '.work\docker_staging\server\dist\server.dll'
        Add-OutputFile 'docker_staging\server\dist\server.dll'
        $report = Get-SizeReport
        $report.total.fileCount | Should -Be 3
        ($report.categories | Where-Object name -EQ packageStaging).fileCount | Should -Be 3
    }

    It 'hashes only same-size candidates and reports exact duplicate potential' {
        Add-OutputFile 'bin\one.dll' 'aaaa'
        Add-OutputFile 'obj\two.dll' 'aaaa'
        Add-OutputFile '.work\test-runtime\three.dll' 'aaaa'
        Add-OutputFile 'bin\different.dll' 'bbbb'
        Add-OutputFile 'bin\unique-size.dll' '12345'
        $report = Get-SizeReport
        $report.duplicateAnalysis.hashedFileCount | Should -Be 4
        $report.duplicateAnalysis.groups.Count | Should -Be 1
        $report.duplicateAnalysis.groups[0].fileCount | Should -Be 3
        $report.duplicateAnalysis.groups[0].fileBytes | Should -Be 4
        $report.duplicateAnalysis.potentialDuplicateBytes | Should -Be 8
        $report.duplicateAnalysis.groups[0].potentialDuplicateBytes | Should -Be 8
        $report.duplicateAnalysis.groups[0].paths[0] | Should -Be '.work/test-runtime/three.dll'
    }

    It 'does not identify distinct same-size files as duplicates' {
        Add-OutputFile 'bin\a.dll' 'aaaa'
        Add-OutputFile 'bin\b.dll' 'bbbb'
        $report = Get-SizeReport
        $report.duplicateAnalysis.hashedFileCount | Should -Be 2
        $report.duplicateAnalysis.groups.Count | Should -Be 0
        $report.duplicateAnalysis.potentialDuplicateBytes | Should -Be 0
    }

    It 'includes the threshold boundary and excludes smaller files from hashing' {
        Add-OutputFile 'bin\a.dll'
        Add-OutputFile 'obj\b.dll'
        (Get-SizeReport -Threshold 4).duplicateAnalysis.potentialDuplicateBytes | Should -Be 4
        (Get-SizeReport -Threshold 5).duplicateAnalysis.hashedFileCount | Should -Be 0
        (Get-SizeReport -Threshold 5).total.logicalBytes | Should -Be 8
        $report = & $scriptPath -RepoRoot $fixtureRoot 6>$null | ConvertFrom-Json
        $report.duplicateAnalysis.minimumFileBytes | Should -Be 1MB
        $report.duplicateAnalysis.hashedFileCount | Should -Be 0
    }

    It 'handles empty directories and empty files with zero totals and stable metadata' {
        [IO.Directory]::CreateDirectory((Join-Path $fixtureRoot 'bin\empty')) | Out-Null
        $report = Get-SizeReport
        $report.schemaVersion | Should -Be 1
        $report.profile | Should -Be 'fixture profile'
        $report.measurement | Should -Be 'logicalBytes'
        $report.total.logicalBytes | Should -Be 0
        $report.total.fileCount | Should -Be 0
        $report.topFiles.Count | Should -Be 0
        $report.topDirectories.Count | Should -Be 0
        $report.categories.Count | Should -Be 7
        Add-OutputFile 'bin\empty.dll' ''
        $report = Get-SizeReport
        $report.total.fileCount | Should -Be 1
        $report.total.logicalBytes | Should -Be 0
    }

    It 'preserves 64-bit sizes without creating large physical fixtures' {
        $file = [pscustomobject]@{
            FullName = Join-Path $fixtureRoot 'bin\large.dll'
            Name = 'large.dll'
            Extension = '.dll'
            Attributes = [IO.FileAttributes]::Normal
            PSIsContainer = $false
            Length = [long]5GB
        }
        Mock Get-ChildItem { $file }
        $report = Get-SizeReport -Threshold ([long]6GB)
        $report.total.logicalBytes | Should -Be ([long]5GB)
        $report.topFiles[0].logicalBytes | Should -Be ([long]5GB)
        $report.duplicateAnalysis.hashedFileCount | Should -Be 0
    }

    It 'writes deterministic JSON only on request, excluding its own report on subsequent runs' {
        Add-OutputFile 'bin\b.dll'
        Add-OutputFile 'bin\a.dll'
        $before = @(Get-ChildItem -LiteralPath $fixtureRoot -Recurse -Force).Count
        $null = Get-SizeReport
        @(Get-ChildItem -LiteralPath $fixtureRoot -Recurse -Force).Count | Should -Be $before
        $output = Join-Path $fixtureRoot '.work\reports\profile.json'
        $first = & $scriptPath -RepoRoot $fixtureRoot -Profile Release -OutputPath $output 6>$null
        $second = & $scriptPath -RepoRoot $fixtureRoot -Profile Release -OutputPath $output 6>$null
        $second | Should -Be $first
        [IO.File]::ReadAllText($output).TrimEnd() | Should -Be $first
        ($first | ConvertFrom-Json).topFiles[0].path | Should -Be 'bin/a.dll'
    }

    It 'limits and deterministically orders top files and directories' {
        Add-OutputFile 'bin\b.dll' 'aaaa'
        Add-OutputFile 'obj\a.dll' 'aaaa'
        Add-OutputFile '.work\large.log' 'longest'
        $report = & $scriptPath -RepoRoot $fixtureRoot -Top 1 6>$null | ConvertFrom-Json
        $report.topFiles.Count | Should -Be 1
        $report.topDirectories.Count | Should -Be 1
        $report.topFiles[0].path | Should -Be '.work/large.log'
        $report.topDirectories[0].path | Should -Be '.work'
        $report.total.fileCount | Should -Be 3
    }

    It 'fails explicitly on enumeration errors instead of reporting partial totals' {
        Mock Get-ChildItem { throw [UnauthorizedAccessException]::new('Fixture access denied') }
        { Get-SizeReport } | Should -Throw '*Cannot enumerate*Fixture access denied*'
    }

    It 'fails explicitly on file-size read errors' {
        $file = [pscustomobject]@{
            FullName = Join-Path $fixtureRoot 'bin\unreadable.dll'
            Name = 'unreadable.dll'
            Extension = '.dll'
            Attributes = [IO.FileAttributes]::Normal
            PSIsContainer = $false
        }
        $file | Add-Member -MemberType ScriptProperty -Name Length -Value { throw [IO.IOException]::new('Fixture metadata failure') }
        Mock Get-ChildItem { $file }
        { Get-SizeReport } | Should -Throw '*Cannot read size*'
    }

    It 'fails explicitly on hash errors instead of treating files as unique' {
        Add-OutputFile 'bin\a.dll'
        Add-OutputFile 'obj\b.dll'
        Mock Get-FileHash { throw [IO.IOException]::new('Fixture read failure') }
        { Get-SizeReport } | Should -Throw '*Cannot hash*Fixture read failure*'
    }

    It 'does not hash files below threshold or unique-size files' {
        Add-OutputFile 'bin\a.dll' 'abcd'
        Add-OutputFile 'obj\b.dll' 'abcd'
        Add-OutputFile '.work\unique.dll' 'abcde'
        Mock Get-FileHash { throw 'Should not hash' }
        $report = Get-SizeReport -Threshold 5
        $report.duplicateAnalysis.hashedFileCount | Should -Be 0
        Should -Invoke Get-FileHash -Times 0
    }

    It 'rejects output roots outside the repository' {
        { Get-SizeReport -Extra @('..\elsewhere') } | Should -Throw '*inside the repository*'
    }

    It 'does not traverse reparse directories or count linked files' -Skip:(-not $IsWindows) {
        Add-OutputFile 'bin\a.dll'
        $target = Join-Path $fixtureRoot 'bin'
        $link = Join-Path $fixtureRoot '.work\linked'
        [IO.Directory]::CreateDirectory((Join-Path $fixtureRoot '.work')) | Out-Null
        New-Item -ItemType Junction -Path $link -Target $target | Out-Null
        $report = Get-SizeReport
        $report.total.fileCount | Should -Be 1
        $report.skippedPaths[0].reason | Should -Be 'linkOrReparsePoint'
        { Get-SizeReport -Extra @('.work\linked') } | Should -Throw '*Link/reparse*'
    }

    It 'allows an explicit non-linked root below a parent path alias' -Skip:(-not $IsWindows) {
        $parent = Join-Path $TestDrive 'real parent'
        [IO.Directory]::CreateDirectory((Join-Path $parent 'checkout')) | Out-Null
        $alias = Join-Path $TestDrive 'parent alias'
        New-Item -ItemType Junction -Path $alias -Target $parent | Out-Null
        $fixtureRoot = Join-Path $alias 'checkout'
        Add-OutputFile 'bin\dependency.dll'

        $report = Get-SizeReport

        $report.total.fileCount | Should -Be 1
        $report.total.logicalBytes | Should -Be 4
    }
}
