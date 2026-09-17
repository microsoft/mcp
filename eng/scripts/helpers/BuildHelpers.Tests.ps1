BeforeAll {
    . (Join-Path $PSScriptRoot 'BuildHelpers.ps1')
}

Describe 'Get-CanonicalProjectPath' {
    It 'normalizes a file to its project root' {
        Get-CanonicalProjectPath 'tools/Azure.Mcp.Tools.Storage/src/StorageService.cs' |
        Should -Be 'tools/Azure.Mcp.Tools.Storage'
    }

    It 'does not treat similarly prefixed projects as the same project' {
        Get-CanonicalProjectPath 'tools/Azure.Mcp.Tools.StorageSync/src/StorageSyncService.cs' |
        Should -Be 'tools/Azure.Mcp.Tools.StorageSync'
    }

    It 'returns null for files outside project roots' {
        Get-CanonicalProjectPath 'Directory.Build.props' | Should -BeNullOrEmpty
    }
}

Describe 'Select-ServersForChangedProjectPaths' {
    BeforeAll {
        $servers = @(
            @{ name = 'Azure.Mcp.Server' }
            @{ name = 'Fabric.Mcp.Server' }
            @{ name = 'Template.Mcp.Server' }
        )
        $dependencies = @{
            'Azure.Mcp.Server'    = @('servers/Azure.Mcp.Server', 'core/Azure.Mcp.Core', 'core/Microsoft.Mcp.Core', 'tools/Azure.Mcp.Tools.Storage')
            'Fabric.Mcp.Server'   = @('servers/Fabric.Mcp.Server', 'core/Fabric.Mcp.Core', 'core/Microsoft.Mcp.Core', 'tools/Fabric.Mcp.Tools.Core')
            'Template.Mcp.Server' = @('servers/Template.Mcp.Server', 'core/Template.Mcp.Core', 'core/Microsoft.Mcp.Core')
        }
    }

    It 'selects only the server that references an Azure tool' {
        Select-ServersForChangedProjectPaths $servers @('tools/Azure.Mcp.Tools.Storage') $dependencies |
        Should -Be @('Azure.Mcp.Server')
    }

    It 'selects only the server that references a Fabric tool' {
        Select-ServersForChangedProjectPaths $servers @('tools/Fabric.Mcp.Tools.Core') $dependencies |
        Should -Be @('Fabric.Mcp.Server')
    }

    It 'selects every server that references shared core' {
        Select-ServersForChangedProjectPaths $servers @('core/Microsoft.Mcp.Core') $dependencies |
        Should -Be @('Azure.Mcp.Server', 'Fabric.Mcp.Server', 'Template.Mcp.Server')
    }
}

Describe 'Get-ServersToBuild' {
    BeforeAll {
        $servers = @(
            @{ name = 'Azure.Mcp.Server' }
            @{ name = 'Fabric.Mcp.Server' }
        )
    }

    It 'builds every server outside pull requests' {
        Get-ServersToBuild -Servers $servers -ChangedFiles @() |
        Should -Be @('Azure.Mcp.Server', 'Fabric.Mcp.Server')
    }

    It 'builds every server for shared repository changes' {
        Get-ServersToBuild -Servers $servers -ChangedFiles @('Directory.Build.props') -IsPullRequest |
        Should -Be @('Azure.Mcp.Server', 'Fabric.Mcp.Server')
    }

    It 'builds every server when a changed project root was deleted' {
        Get-ServersToBuild `
            -Servers $servers `
            -ChangedFiles @('tools/Azure.Mcp.Tools.Deleted/src/Deleted.cs') `
            -IsPullRequest `
            -RepositoryRoot $TestDrive |
        Should -Be @('Azure.Mcp.Server', 'Fabric.Mcp.Server')
    }
}
