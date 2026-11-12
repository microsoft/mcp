BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Update-VallyFailureIssue.ps1'
    function global:gh {
        $global:ghCalls.Add(@($args))
        $global:LASTEXITCODE = 0

        if ($args[0] -eq 'api' -and $args[3] -like '*/actions/workflows/*/runs') {
            return $global:workflowRuns
        }
        if ($args[0] -eq 'api' -and $args[3] -like '*/pulls/*') {
            return '{"merged_at":"2026-01-01T00:00:00Z"}'
        }
        if ($args[0] -eq 'issue' -and $args[1] -eq 'list') {
            return $global:issues
        }
        if ($args[0] -eq 'issue' -and $args[1] -in @('create', 'comment')) {
            return
        }
        throw "Unexpected gh invocation: $args"
    }

    $parameters = @{
        Repository = 'example/repo'
        Workflow = 'vally-eval.yml'
        RunId = 42
        RunUrl = 'https://example.test/runs/42'
        PullRequestNumber = 10
        PullRequestUrl = 'https://example.test/pull/10'
        PullRequestAuthor = 'author'
    }
}

AfterAll {
    Remove-Item Function:\gh
    Remove-Variable ghCalls, workflowRuns, issues -Scope Global -ErrorAction SilentlyContinue
}

Describe 'Update-VallyFailureIssue' {
    BeforeEach {
        $global:ghCalls = [System.Collections.Generic.List[object]]::new()
        $global:workflowRuns = '{"workflow_runs":[]}'
        $global:issues = '[]'
    }

    It 'uses GET to find runs and creates an issue for a new failure' {
        & $scriptPath @parameters

        $global:ghCalls[0][0..2] | Should -Be @('api', '--method', 'GET')
        $global:ghCalls[-1][0..1] | Should -Be @('issue', 'create')
        $global:ghCalls[-1] | Should -Contain 'New vally failures in main for PR #10'
    }

    It 'comments on the previous failure issue when failures are consecutive' {
        $global:workflowRuns = '{"workflow_runs":[{"id":41,"conclusion":"failure","html_url":"https://example.test/runs/41","pull_requests":[{"number":9}]}]}'
        $global:issues = '[{"number":7,"title":"New vally failures in main for PR #9","body":"https://example.test/runs/41","comments":[]}]'

        & $scriptPath @parameters

        $global:ghCalls[-1][0..2] | Should -Be @('issue', 'comment', '7')
    }
}
