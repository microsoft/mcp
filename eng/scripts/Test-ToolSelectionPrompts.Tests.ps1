# Invoke-Pester -Path .\eng\scripts\Test-ToolSelectionPrompts.Tests.ps1

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Test-ToolSelectionPrompts.ps1'
    $parseTokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$parseTokens, [ref]$parseErrors)
    if ($parseErrors.Count -ne 0) {
        throw "Unable to parse $scriptPath"
    }

    # Exercise the production statements without running the build or exiting the test host.
    $statements = foreach ($variableName in @('toolsJson', 'toolsResult')) {
        $assignments = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $node.Left.VariablePath.UserPath -eq $variableName
        }, $true))
        if ($assignments.Count -ne 1) {
            throw "Expected one assignment to $variableName in $scriptPath"
        }
        $assignments[0].Extent.Text
    }
    $script:ReadCatalog = [scriptblock]::Create($statements -join "`n")
    $script:PowerShellPath = (Get-Process -Id $PID).Path

    function Invoke-TestToolServer {
        & $script:PowerShellPath -NoLogo -NoProfile -NonInteractive -Command $script:ServerCode
    }
}

Describe 'Tool catalog output streams' {
    BeforeEach {
        $ErrorActionPreference = 'Stop'
        $executablePath = 'Invoke-TestToolServer'
        $script:ServerCode = @'
[Console]::Out.WriteLine('{"status":200,"results":{"names":["test_tool"]}}')
'@
    }

    It 'parses a catalog written to stdout' {
        . $script:ReadCatalog

        $toolsResult.status | Should -Be 200
        @($toolsResult.results.names) | Should -HaveCount 1
        $toolsResult.results.names | Should -Contain 'test_tool'
    }

    It 'keeps native stderr diagnostics visible without adding them to the JSON' {
        $script:ServerCode = '[Console]::Error.WriteLine("info: test server diagnostic");' + $script:ServerCode

        $output = @(& {
            . $script:ReadCatalog
            $toolsJson.Trim() | Should -Be '{"status":200,"results":{"names":["test_tool"]}}'
            $toolsResult
        } 2>&1)

        $catalog = @($output | Where-Object { $null -ne $_.PSObject.Properties['results'] })
        $catalog | Should -HaveCount 1
        $catalog[0].results.names | Should -Contain 'test_tool'
        ($output | Out-String) | Should -Match 'info: test server diagnostic'
    }

    It 'rejects malformed stdout' {
        $script:ServerCode = '[Console]::Out.WriteLine("not-json")'

        { . $script:ReadCatalog } | Should -Throw -ErrorId '*Microsoft.PowerShell.Commands.ConvertFromJsonCommand'
    }

    It 'rejects invalid stdout even when stderr contains a valid catalog' {
        $script:ServerCode = @'
[Console]::Error.WriteLine('{"status":200,"results":{"names":["test_tool"]}}')
[Console]::Out.WriteLine('not-json')
'@

        { . $script:ReadCatalog } | Should -Throw -ErrorId '*Microsoft.PowerShell.Commands.ConvertFromJsonCommand'
    }

    It 'does not strip non-JSON text from stdout to manufacture a valid catalog' {
        $script:ServerCode = '[Console]::Out.WriteLine("info: invalid stdout diagnostic");' + $script:ServerCode

        { . $script:ReadCatalog } | Should -Throw -ErrorId '*Microsoft.PowerShell.Commands.ConvertFromJsonCommand'
    }
}
