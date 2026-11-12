BeforeAll {
    $workflow = Get-Content (Join-Path $PSScriptRoot '../../.github/workflows/vally-eval.yml') -Raw
    $step = ($workflow -split '(?m)^      - name: Resolve git SHAs\s*$')[1] -split '(?m)^      - name: Verify server exists\s*$'
    $script:resolveShas = [scriptblock]::Create(($step[0] -split '(?m)^        run: \|\s*$')[1])
    $step = ($workflow -split '(?m)^      - name: Verify server exists\s*$')[1] -split '(?m)^      - name: Create build_info.json\s*$'
    $script:verifyServer = [scriptblock]::Create(($step[0] -split '(?m)^        run: \|\s*$')[1])
}

Describe 'Vally workflow server validation' {
    It 'rejects servers without a configured Vally environment' {
        $previousServerName = $env:SERVER_NAME
        try {
            $env:SERVER_NAME = 'Fabric.Mcp.Server'
            { & $script:verifyServer } | Should -Throw '*No Vally environment is configured*'
        }
        finally {
            $env:SERVER_NAME = $previousServerName
        }
    }
}

Describe 'Vally workflow SHA resolution' {
    BeforeEach {
        $script:originalEnvironment = @{}
        foreach ($name in 'EVENT_NAME', 'PR_BASE_SHA', 'PR_MERGE_SHA', 'GITHUB_ENV', 'BUILD_REASON') {
            $script:originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
        }
        $env:GITHUB_ENV = Join-Path $TestDrive 'github-env'
        $env:BUILD_REASON = $null
        Set-Content -LiteralPath $env:GITHUB_ENV -Value ''
    }

    AfterEach {
        foreach ($entry in $script:originalEnvironment.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
        }
    }

    It 'sets an unquoted PR build reason and merge SHAs for merged PRs' {
        $env:EVENT_NAME = 'pull_request'
        $env:PR_BASE_SHA = 'base'
        $env:PR_MERGE_SHA = 'merge'

        & $script:resolveShas

        $output = @(Get-Content $env:GITHUB_ENV)
        $output | Should -Contain 'BUILD_REASON=PullRequest'
        $output | Should -Contain 'DIFF_HEAD_COMMIT=merge'
        $output | Should -Contain 'DIFF_BASE_COMMIT=base'
    }

    It 'does not set a PR build reason for manual runs' {
        $env:EVENT_NAME = 'workflow_dispatch'

        & $script:resolveShas

        $output = @(Get-Content $env:GITHUB_ENV)
        $output | Should -Not -Contain 'BUILD_REASON=PullRequest'
        $output | Should -Contain 'DIFF_HEAD_COMMIT=HEAD'
        $output | Should -Contain 'DIFF_BASE_COMMIT=HEAD^'
    }
}
