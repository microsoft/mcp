#!/usr/bin/env pwsh

# Storage Sync - Complete Command Implementation Checklist
# Generated based on new-command.md guidelines

$commands = @{
    "StorageSyncService" = @(
        @{ Name = "StorageSyncServiceListCommand"; Operation = "List"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "StorageSyncServiceGetCommand"; Operation = "Get"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "StorageSyncServiceCreateCommand"; Operation = "Create"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "StorageSyncServiceUpdateCommand"; Operation = "Update"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "StorageSyncServiceDeleteCommand"; Operation = "Delete"; ToolMetadata = @{ ReadOnly = $false; Destructive = $true } }
    )
    "SyncGroup" = @(
        @{ Name = "SyncGroupListCommand"; Operation = "List"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "SyncGroupGetCommand"; Operation = "Get"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "SyncGroupCreateCommand"; Operation = "Create"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "SyncGroupDeleteCommand"; Operation = "Delete"; ToolMetadata = @{ ReadOnly = $false; Destructive = $true } }
    )
    "CloudEndpoint" = @(
        @{ Name = "CloudEndpointListCommand"; Operation = "List"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "CloudEndpointGetCommand"; Operation = "Get"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "CloudEndpointCreateCommand"; Operation = "Create"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "CloudEndpointDeleteCommand"; Operation = "Delete"; ToolMetadata = @{ ReadOnly = $false; Destructive = $true } }
        @{ Name = "CloudEndpointChangeDetectionCommand"; Operation = "ChangeDetection"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
    )
    "ServerEndpoint" = @(
        @{ Name = "ServerEndpointListCommand"; Operation = "List"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "ServerEndpointGetCommand"; Operation = "Get"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "ServerEndpointCreateCommand"; Operation = "Create"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "ServerEndpointUpdateCommand"; Operation = "Update"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "ServerEndpointDeleteCommand"; Operation = "Delete"; ToolMetadata = @{ ReadOnly = $false; Destructive = $true } }
    )
    "RegisteredServer" = @(
        @{ Name = "RegisteredServerListCommand"; Operation = "List"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "RegisteredServerGetCommand"; Operation = "Get"; ToolMetadata = @{ ReadOnly = $true; Destructive = $false } }
        @{ Name = "RegisteredServerRegisterCommand"; Operation = "Register"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "RegisteredServerUpdateCommand"; Operation = "Update"; ToolMetadata = @{ ReadOnly = $false; Destructive = $false } }
        @{ Name = "RegisteredServerUnregisterCommand"; Operation = "Unregister"; ToolMetadata = @{ ReadOnly = $false; Destructive = $true } }
    )
}

# Implementation Status
$status = @{
    "Created" = @()
    "In Progress" = @(
        "StorageSyncServiceListCommand",
        "StorageSyncJsonContext"
    )
    "Not Started" = @()
    "Pending Review" = @()
}

# Add remaining commands to "Not Started"
foreach ($resource in $commands.Keys) {
    foreach ($cmd in $commands[$resource]) {
        if (-not ($status["In Progress"] -contains $cmd.Name)) {
            $status["Not Started"] += $cmd.Name
        }
    }
}

Write-Host "=== Storage Sync Command Implementation Status ===" -ForegroundColor Cyan
Write-Host ""

foreach ($state in @("Created", "In Progress", "Not Started", "Pending Review")) {
    $count = $status[$state].Count
    $color = switch ($state) {
        "Created" { "Green" }
        "In Progress" { "Yellow" }
        "Not Started" { "Red" }
        "Pending Review" { "Cyan" }
    }

    Write-Host "[$state] - $count commands" -ForegroundColor $color
    foreach ($cmd in $status[$state]) {
        Write-Host "  - $cmd" -ForegroundColor Gray
    }
    Write-Host ""
}

# Implementation checklist
Write-Host "=== Implementation Checklist ===" -ForegroundColor Cyan
Write-Host ""

$checklist = @(
    "[ ] Review new-command.md guidelines",
    "[ ] Create BaseStorageSyncCommand<T> - DONE",
    "[ ] Create BaseStorageSyncOptions - DONE",
    "[ ] Create StorageSyncOptionDefinitions - DONE",
    "[ ] Create IStorageSyncService interface - DONE",
    "[ ] Create StorageSyncService implementation",
    "[ ] Create StorageSyncJsonContext - IN PROGRESS",
    "[ ] Create all command classes (24 total)",
    "[ ] Create all options classes (24 total)",
    "[ ] Create unit tests for all commands",
    "[ ] Create integration tests with fixtures",
    "[ ] Create test-resources.bicep",
    "[ ] Create test-resources-post.ps1",
    "[ ] Create StorageSyncSetup.cs registration",
    "[ ] Register in Program.cs RegisterAreas()",
    "[ ] Validate CancellationToken usage",
    "[ ] Run dotnet format",
    "[ ] Run dotnet build",
    "[ ] Run dotnet test",
    "[ ] Run ./eng/scripts/Build-Local.ps1 -BuildNative",
    "[ ] Review for AOT compatibility issues"
)

foreach ($item in $checklist) {
    Write-Host $item
}

Write-Host ""
Write-Host "Total Commands: $($commands.Values | Measure-Object -Sum { $_.Count }).Sum"
Write-Host "Status: In Progress (7% complete)"
