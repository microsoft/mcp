BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Test-ToolSelectionPrompts.ps1'
    $parseTokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$parseTokens, [ref]$parseErrors)
    if ($parseErrors.Count -ne 0) {
        throw "Unable to parse $scriptPath"
    }

    $statements = foreach ($variableName in @('toolsJson', 'toolsResult')) {
        $matches = @($ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $node.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
            $node.Left.VariablePath.UserPath -eq $variableName
        }, $true))
        if ($matches.Count -ne 1) {
            throw "Expected one assignment to $variableName in $scriptPath"
        }
        $matches[0].Extent.Text
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
[Console]::Out.WriteLine('{"status":200,"results":{"names":["core_get-item"]}}')
'@
    }

    It 'parses a catalog written to stdout' {
        . $script:ReadCatalog

        $toolsResult.status | Should -Be 200
        @($toolsResult.results.names) | Should -HaveCount 1
        $toolsResult.results.names | Should -Contain 'core_get-item'
    }

    It 'keeps native stderr diagnostics visible without adding them to the JSON' {
        $script:ServerCode = '[Console]::Error.WriteLine("info: test server diagnostic");' + $script:ServerCode

        $output = @(& {
            . $script:ReadCatalog
            $toolsJson.Trim() | Should -Be '{"status":200,"results":{"names":["core_get-item"]}}'
            $toolsResult
        } 2>&1)

        $catalog = @($output | Where-Object { $null -ne $_.PSObject.Properties['results'] })
        $catalog | Should -HaveCount 1
        $catalog[0].results.names | Should -Contain 'core_get-item'
        ($output | Out-String) | Should -Match 'info: test server diagnostic'
    }

    It 'rejects invalid stdout even when stderr contains a valid catalog' {
        $script:ServerCode = @'
[Console]::Error.WriteLine('{"status":200,"results":{"names":["core_get-item"]}}')
[Console]::Out.WriteLine('not-json')
'@

        { . $script:ReadCatalog } | Should -Throw
    }

    It 'does not strip non-JSON text from stdout to manufacture a valid catalog' {
        $script:ServerCode = '[Console]::Out.WriteLine("info: invalid stdout diagnostic");' + $script:ServerCode

        { . $script:ReadCatalog } | Should -Throw
    }
}
