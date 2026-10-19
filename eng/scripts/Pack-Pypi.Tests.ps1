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

Describe "PyPI package version conversion" {
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
