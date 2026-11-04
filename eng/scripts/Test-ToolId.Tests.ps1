# Invoke-Pester -Path .\eng\scripts\Test-ToolId.Tests.ps1

BeforeAll {
    $script:ValidatorPath = Join-Path $PSScriptRoot 'Test-ToolId.ps1'

    function New-CommandSource {
        param([string]$RelativePath, [string]$Content)

        $path = Join-Path $script:ToolsDirectory $RelativePath
        New-Item -ItemType Directory -Path (Split-Path $path -Parent) -Force | Out-Null
        Set-Content -LiteralPath $path -Value $Content
        return $path
    }

    function Invoke-ToolIdValidation {
        $output = @(& $script:ValidatorPath -ToolsDirectory $script:ToolsDirectory 6>&1)
        $exitCode = $LASTEXITCODE
        $result = @($output | Where-Object { $null -ne $_.PSObject.Properties['ViolationCount'] })
        $result | Should -HaveCount 1

        return [PSCustomObject]@{
            ExitCode = $exitCode
            Result = $result[0]
            Output = $output | Out-String
        }
    }
}

Describe 'Tool command ID validation' {
    BeforeEach {
        $script:ToolsDirectory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:ToolsDirectory | Out-Null
    }

    It 'checks unique attribute IDs across toolsets and returns the checked count' {
        New-CommandSource 'First.Tool\src\FirstCommand.cs' @'
[CommandMetadata(
    Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    Name = "first")]
public class FirstCommand {}
'@ | Out-Null
        New-CommandSource 'Second.Tool\src\SecondCommand.cs' @'
[CommandMetadata(Name = "second", Id = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")]
public class SecondCommand {}
'@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 0
        $validation.Result.ToolCount | Should -Be 2
        $validation.Result.ViolationCount | Should -Be 0
        $validation.Output | Should -Match 'Tool IDs checked: 2'
    }

    It 'recognizes <AttributeName>' -ForEach @(
        @{ AttributeName = 'CommandMetadata' }
        @{ AttributeName = 'CommandMetadataAttribute' }
        @{ AttributeName = 'global::Microsoft.Mcp.Core.Commands.CommandMetadata' }
        @{ AttributeName = 'Microsoft.Mcp.Core.Commands.CommandMetadataAttribute' }
    ) {
        New-CommandSource 'Example.Tool\src\ExampleCommand.cs' @"
[$AttributeName(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class ExampleCommand {}
"@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 0
        $validation.Result.ToolCount | Should -Be 1
        $validation.Result.ViolationCount | Should -Be 0
    }

    It 'recognizes a <LiteralKind> string literal' -ForEach @(
        @{ LiteralKind = 'regular'; Literal = '"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"' }
        @{ LiteralKind = 'verbatim'; Literal = '@"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"' }
        @{ LiteralKind = 'raw'; Literal = '"""aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"""' }
    ) {
        New-CommandSource 'Example.Tool\src\ExampleCommand.cs' @"
[CommandMetadata(Id = $Literal)]
public class ExampleCommand {}
"@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 0
        $validation.Result.ToolCount | Should -Be 1
        $validation.Result.ViolationCount | Should -Be 0
    }

    It 'reports duplicate IDs with both source locations' {
        $firstPath = New-CommandSource 'First.Tool\src\FirstCommand.cs' @'
[CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class FirstCommand {}
'@
        $secondPath = New-CommandSource 'Second.Tool\src\SecondCommand.cs' @'
[CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class SecondCommand {}
'@

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 1
        $validation.Result.ToolCount | Should -Be 2
        $validation.Result.ViolationCount | Should -Be 1
        $validation.Output | Should -Match 'Tool ID: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
        $validation.Output | Should -Match ([regex]::Escape("${firstPath}:1"))
        $validation.Output | Should -Match ([regex]::Escape("${secondPath}:1"))
    }

    It 'preserves case-insensitive duplicate detection and decodes escaped literals' {
        New-CommandSource 'First.Tool\src\FirstCommand.cs' @'
[CommandMetadata(Id = "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")]
public class FirstCommand {}
'@ | Out-Null
        New-CommandSource 'Second.Tool\src\SecondCommand.cs' @'
[CommandMetadata(Id = "\u0061aaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class SecondCommand {}
'@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 1
        $validation.Result.ToolCount | Should -Be 2
        $validation.Result.ViolationCount | Should -Be 1
    }

    It 'ignores comments and string contents, including attribute delimiters in descriptions' {
        New-CommandSource 'Example.Tool\src\ExampleCommand.cs' @'
// [CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
/* [CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")] */
[CommandMetadata(
    Description = """
        Example: [CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
        """,
    Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class ExampleCommand
{
    public string Id => "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
}
'@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 0
        $validation.Result.ToolCount | Should -Be 1
        $validation.Result.ViolationCount | Should -Be 0
    }

    It 'excludes tests, build outputs, and non-command files' {
        $source = @'
[CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class ExampleCommand {}
'@
        foreach ($path in @(
            'Example.Tool\src\ExampleCommand.cs'
            'Example.Tool\src\Other.cs'
            'Example.Tool\tests\ExampleCommand.cs'
            'Example.Tool\src\bin\ExampleCommand.cs'
            'Example.Tool\src\obj\ExampleCommand.cs'
        )) {
            New-CommandSource $path $source | Out-Null
        }

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 0
        $validation.Result.ToolCount | Should -Be 1
        $validation.Result.ViolationCount | Should -Be 0
    }

    It 'rejects a <DeclarationKind> ID declaration even when another ID is valid' -ForEach @(
        @{ DeclarationKind = 'missing'; Attribute = '[CommandMetadata(Name = "get")]' }
        @{ DeclarationKind = 'empty'; Attribute = '[CommandMetadata(Id = "")]' }
        @{ DeclarationKind = 'whitespace'; Attribute = '[CommandMetadata(Id = " ")]' }
        @{ DeclarationKind = 'null'; Attribute = '[CommandMetadata(Id = null)]' }
        @{ DeclarationKind = 'non-string'; Attribute = '[CommandMetadata(Id = 123)]' }
        @{ DeclarationKind = 'non-literal'; Attribute = '[CommandMetadata(Id = CommandIds.Example)]' }
        @{ DeclarationKind = 'malformed'; Attribute = '[CommandMetadata(Id = )]' }
    ) {
        $invalidPath = New-CommandSource 'Example.Tool\src\InvalidCommand.cs' @"
$Attribute
public class InvalidCommand {}
"@
        New-CommandSource 'Example.Tool\src\ValidCommand.cs' @'
[CommandMetadata(Id = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")]
public class ValidCommand {}
'@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 1
        $validation.Result.ToolCount | Should -Be 1
        $validation.Result.ViolationCount | Should -Be 1
        $validation.Output | Should -Match ([regex]::Escape("${invalidPath}:1"))
        $validation.Output | Should -Match 'CommandMetadata.Id must be a non-empty string literal'
    }

    It 'fails when no command files are found' {
        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 1
        $validation.Result.ToolCount | Should -Be 0
        $validation.Result.ViolationCount | Should -Be 1
        $validation.Output | Should -Match 'No command metadata IDs were found'
        $validation.Output | Should -Not -Match 'Passed validation'
    }

    It 'fails when command files contain only the obsolete ID property pattern' {
        New-CommandSource 'Example.Tool\src\ExampleCommand.cs' @'
public class ExampleCommand
{
    public override string Id => "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
}
'@ | Out-Null

        $validation = Invoke-ToolIdValidation

        $validation.ExitCode | Should -Be 1
        $validation.Result.ToolCount | Should -Be 0
        $validation.Result.ViolationCount | Should -Be 1
        $validation.Output | Should -Match 'No command metadata IDs were found'
    }
}
