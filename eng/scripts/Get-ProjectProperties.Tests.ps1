Describe 'Get-ProjectProperties.ps1' {
    BeforeAll {
        $scriptPath = Join-Path $PSScriptRoot 'Get-ProjectProperties.ps1'

        function global:dotnet {
            param(
                [Parameter(ValueFromRemainingArguments = $true)]
                [object[]] $Arguments
            )

            $global:CapturedDotnetArguments = @($Arguments)
            $propertyArgument = $Arguments | Where-Object { $_ -like '-getProperty:*' }

            switch ($propertyArgument) {
                '-getProperty:TargetFramework' { 'net10.0' }
                '-getProperty:DefinitelyNotAProjectProperty' { return }
                '-getProperty:BrokenProperty' { $global:LASTEXITCODE = 1; return }
                default { '{"Properties":{"TargetFramework":"net10.0","OutputType":"Exe"}}' }
            }
        }
    }

    BeforeEach {
        $global:CapturedDotnetArguments = @()
        $global:LASTEXITCODE = 0
    }

    AfterAll {
        Remove-Item Function:\dotnet -ErrorAction SilentlyContinue
        Remove-Variable CapturedDotnetArguments -Scope Global -ErrorAction SilentlyContinue
    }

    It 'gets only the requested properties' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        $result = & $scriptPath -Path $projectPath -Properties @('TargetFramework', 'OutputType')

        ($global:CapturedDotnetArguments | Where-Object { $_ -like '-getProperty:*' }) |
        Should -Be '-getProperty:TargetFramework,OutputType'
        @($result.PSObject.Properties.Name) -join ',' | Should -Be 'TargetFramework,OutputType'
    }

    It 'gets one requested property' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        $result = & $scriptPath -Path $projectPath -Properties 'TargetFramework'

        ($global:CapturedDotnetArguments | Where-Object { $_ -like '-getProperty:*' }) |
        Should -Be '-getProperty:TargetFramework'
        @($result.PSObject.Properties.Name) | Should -Be 'TargetFramework'
        $result.TargetFramework | Should -Be 'net10.0'
    }

    It 'requires a value when Properties is passed' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        { & $scriptPath -Path $projectPath -Properties } |
        Should -Throw -ExpectedMessage "*Missing an argument for parameter 'Properties'*"
        $global:CapturedDotnetArguments | Should -BeNullOrEmpty
    }

    It 'returns an empty value for an unknown property' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        $result = & $scriptPath -Path $projectPath -Properties 'DefinitelyNotAProjectProperty'

        @($result.PSObject.Properties.Name) | Should -Be 'DefinitelyNotAProjectProperty'
        $result.DefinitelyNotAProjectProperty | Should -Be ''
    }

    It 'fails when dotnet cannot evaluate a requested property' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        { & $scriptPath -Path $projectPath -Properties 'BrokenProperty' } |
        Should -Throw -ExpectedMessage '*dotnet build*'
    }

    It 'rejects an empty property name' {
        $projectPath = Join-Path $TestDrive 'Test.csproj'
        Set-Content -Path $projectPath -Value '<Project />'

        { & $scriptPath -Path $projectPath -Properties '' } |
        Should -Throw -ExpectedMessage "*Cannot validate argument on parameter 'Properties'*"
        $global:CapturedDotnetArguments | Should -BeNullOrEmpty
    }
}
