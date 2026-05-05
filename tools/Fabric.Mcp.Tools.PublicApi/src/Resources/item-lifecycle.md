---
title: Item Lifecycle Management for Fabric REST APIs
description: Learn best practices for managing the complete lifecycle of Fabric items including creation, updates, versioning, deployment pipelines, Git integration, and deletion.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to manage Fabric item lifecycles effectively using APIs.
---

# Item Lifecycle Management

Managing the complete lifecycle of Microsoft Fabric items—from creation through deployment to retirement—is essential for maintaining healthy, well-organized Fabric environments. This guide covers patterns for item management, versioning strategies, CI/CD integration, and governance.

## Item Lifecycle Overview

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   Create    │───▶│   Develop   │───▶│   Deploy    │───▶│   Retire    │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
      │                  │                  │                  │
      ▼                  ▼                  ▼                  ▼
  - Initial          - Edit              - Promote          - Archive
    creation         - Test              - Pipeline         - Delete
  - From            - Version           - Git sync         - Cleanup
    template        - Validate          - Approvals
```

## Creating Items

### Basic Item Creation

```csharp
public async Task<Item> CreateItemAsync(
    HttpClient client,
    string workspaceId,
    string displayName,
    string itemType,
    object? definition = null)
{
    var request = new
    {
        displayName = displayName,
        type = itemType,
        definition = definition
    };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items",
        request);
    
    // Handle long-running operation
    if (response.StatusCode == HttpStatusCode.Accepted)
    {
        return await PollForCompletionAsync<Item>(client, response);
    }
    
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Item>()
        ?? throw new Exception("Failed to deserialize created item");
}
```

### Creating from Templates

```csharp
public class ItemTemplateManager
{
    private readonly Dictionary<string, object> _templates = new();
    
    public void RegisterTemplate(string templateName, object definition)
    {
        _templates[templateName] = definition;
    }
    
    public async Task<Item> CreateFromTemplateAsync(
        HttpClient client,
        string workspaceId,
        string templateName,
        string displayName,
        Dictionary<string, string>? parameters = null)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }
        
        // Apply parameter substitutions
        var definition = ApplyParameters(template, parameters);
        
        return await CreateItemAsync(client, workspaceId, displayName, 
            GetItemTypeFromTemplate(templateName), definition);
    }
    
    private object ApplyParameters(object template, Dictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return template;
        
        var json = JsonSerializer.Serialize(template);
        
        foreach (var (key, value) in parameters)
        {
            json = json.Replace($"{{{{{key}}}}}", value);
        }
        
        return JsonSerializer.Deserialize<object>(json)!;
    }
}
```

### Bulk Item Creation

```csharp
public async Task<List<ItemCreationResult>> CreateItemsBulkAsync(
    HttpClient client,
    string workspaceId,
    List<ItemCreateRequest> items)
{
    var results = new List<ItemCreationResult>();
    var semaphore = new SemaphoreSlim(5); // Limit concurrent creations
    
    var tasks = items.Select(async itemRequest =>
    {
        await semaphore.WaitAsync();
        try
        {
            var item = await CreateItemAsync(
                client, workspaceId, 
                itemRequest.DisplayName, 
                itemRequest.Type, 
                itemRequest.Definition);
            
            return new ItemCreationResult
            {
                DisplayName = itemRequest.DisplayName,
                Success = true,
                Item = item
            };
        }
        catch (Exception ex)
        {
            return new ItemCreationResult
            {
                DisplayName = itemRequest.DisplayName,
                Success = false,
                Error = ex.Message
            };
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    return (await Task.WhenAll(tasks)).ToList();
}
```

## Updating Items

### Update Item Properties

```csharp
public async Task UpdateItemAsync(
    HttpClient client,
    string workspaceId,
    string itemId,
    string? newDisplayName = null,
    string? newDescription = null)
{
    var updates = new Dictionary<string, object>();
    
    if (newDisplayName != null)
        updates["displayName"] = newDisplayName;
    
    if (newDescription != null)
        updates["description"] = newDescription;
    
    if (updates.Count == 0)
        return;
    
    var response = await client.PatchAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items/{itemId}",
        updates);
    
    response.EnsureSuccessStatusCode();
}
```

### Update Item Definition

```csharp
public async Task UpdateItemDefinitionAsync(
    HttpClient client,
    string workspaceId,
    string itemId,
    object newDefinition)
{
    var request = new { definition = newDefinition };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items/{itemId}/updateDefinition",
        request);
    
    // Handle long-running operation
    if (response.StatusCode == HttpStatusCode.Accepted)
    {
        await PollForCompletionAsync(client, response);
        return;
    }
    
    response.EnsureSuccessStatusCode();
}
```

### Safe Update Pattern with Validation

```csharp
public class SafeItemUpdater
{
    public async Task<UpdateResult> SafeUpdateDefinitionAsync(
        HttpClient client,
        string workspaceId,
        string itemId,
        object newDefinition)
    {
        // 1. Get current definition as backup
        var currentDefinition = await GetItemDefinitionAsync(client, workspaceId, itemId);
        
        // 2. Validate new definition
        var validationResult = await ValidateDefinitionAsync(newDefinition);
        if (!validationResult.IsValid)
        {
            return UpdateResult.Failed($"Validation failed: {validationResult.Error}");
        }
        
        try
        {
            // 3. Apply update
            await UpdateItemDefinitionAsync(client, workspaceId, itemId, newDefinition);
            
            // 4. Verify update succeeded
            var updatedDefinition = await GetItemDefinitionAsync(client, workspaceId, itemId);
            
            return UpdateResult.Success(currentDefinition, updatedDefinition);
        }
        catch (Exception ex)
        {
            // 5. Attempt rollback on failure
            try
            {
                await UpdateItemDefinitionAsync(client, workspaceId, itemId, currentDefinition);
                return UpdateResult.RolledBack(ex.Message);
            }
            catch
            {
                return UpdateResult.Failed($"Update failed and rollback failed: {ex.Message}");
            }
        }
    }
}
```

## Version Control Patterns

### Item Version Tracking

```csharp
public class ItemVersionTracker
{
    public record ItemVersion(
        string ItemId,
        int VersionNumber,
        DateTime Timestamp,
        string ModifiedBy,
        object Definition,
        string? Comment);
    
    private readonly Dictionary<string, List<ItemVersion>> _versions = new();
    
    public async Task TrackVersionAsync(
        HttpClient client,
        string workspaceId,
        string itemId,
        string modifiedBy,
        string? comment = null)
    {
        var definition = await GetItemDefinitionAsync(client, workspaceId, itemId);
        
        var versions = _versions.GetValueOrDefault(itemId) ?? new List<ItemVersion>();
        var nextVersion = versions.Count + 1;
        
        versions.Add(new ItemVersion(
            itemId,
            nextVersion,
            DateTime.UtcNow,
            modifiedBy,
            definition,
            comment));
        
        _versions[itemId] = versions;
    }
    
    public async Task RollbackToVersionAsync(
        HttpClient client,
        string workspaceId,
        string itemId,
        int versionNumber)
    {
        if (!_versions.TryGetValue(itemId, out var versions))
        {
            throw new Exception("No versions tracked for this item");
        }
        
        var targetVersion = versions.FirstOrDefault(v => v.VersionNumber == versionNumber)
            ?? throw new Exception($"Version {versionNumber} not found");
        
        await UpdateItemDefinitionAsync(client, workspaceId, itemId, targetVersion.Definition);
    }
    
    public List<ItemVersion> GetVersionHistory(string itemId)
    {
        return _versions.GetValueOrDefault(itemId) ?? new List<ItemVersion>();
    }
}
```

### Definition Comparison

```csharp
public class DefinitionComparer
{
    public DiffResult CompareDefinitions(object oldDefinition, object newDefinition)
    {
        var oldJson = JsonSerializer.Serialize(oldDefinition, 
            new JsonSerializerOptions { WriteIndented = true });
        var newJson = JsonSerializer.Serialize(newDefinition, 
            new JsonSerializerOptions { WriteIndented = true });
        
        if (oldJson == newJson)
        {
            return new DiffResult { HasChanges = false };
        }
        
        // Generate detailed diff
        var oldLines = oldJson.Split('\n');
        var newLines = newJson.Split('\n');
        
        return new DiffResult
        {
            HasChanges = true,
            AddedLines = newLines.Except(oldLines).ToList(),
            RemovedLines = oldLines.Except(newLines).ToList()
        };
    }
}
```

## Deployment Pipelines

### Cross-Environment Deployment

```csharp
public class DeploymentPipeline
{
    public record Environment(string Name, string WorkspaceId);
    
    private readonly List<Environment> _environments;
    
    public DeploymentPipeline(params Environment[] environments)
    {
        _environments = environments.ToList();
    }
    
    public async Task<DeploymentResult> PromoteAsync(
        HttpClient client,
        string itemId,
        string sourceEnvironment,
        string targetEnvironment)
    {
        var source = _environments.Find(e => e.Name == sourceEnvironment)
            ?? throw new ArgumentException($"Environment '{sourceEnvironment}' not found");
        var target = _environments.Find(e => e.Name == targetEnvironment)
            ?? throw new ArgumentException($"Environment '{targetEnvironment}' not found");
        
        // 1. Get source item and definition
        var sourceItem = await GetItemAsync(client, source.WorkspaceId, itemId);
        var sourceDefinition = await GetItemDefinitionAsync(client, source.WorkspaceId, itemId);
        
        // 2. Check if item exists in target
        var existingItem = await FindItemByNameAsync(client, target.WorkspaceId, 
            sourceItem.DisplayName, sourceItem.Type);
        
        if (existingItem != null)
        {
            // Update existing item
            await UpdateItemDefinitionAsync(client, target.WorkspaceId, 
                existingItem.Id, sourceDefinition);
            
            return new DeploymentResult
            {
                Success = true,
                Action = "Updated",
                TargetItemId = existingItem.Id
            };
        }
        else
        {
            // Create new item in target
            var newItem = await CreateItemAsync(client, target.WorkspaceId,
                sourceItem.DisplayName, sourceItem.Type, sourceDefinition);
            
            return new DeploymentResult
            {
                Success = true,
                Action = "Created",
                TargetItemId = newItem.Id
            };
        }
    }
    
    public async Task<List<DeploymentResult>> PromoteWorkspaceAsync(
        HttpClient client,
        string sourceEnvironment,
        string targetEnvironment,
        string[]? itemTypes = null)
    {
        var source = _environments.Find(e => e.Name == sourceEnvironment)!;
        var items = await GetAllWorkspaceItemsAsync(client, source.WorkspaceId);
        
        if (itemTypes != null)
        {
            items = items.Where(i => itemTypes.Contains(i.Type)).ToList();
        }
        
        var results = new List<DeploymentResult>();
        
        foreach (var item in items)
        {
            try
            {
                var result = await PromoteAsync(client, item.Id, 
                    sourceEnvironment, targetEnvironment);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new DeploymentResult
                {
                    Success = false,
                    Error = ex.Message,
                    SourceItemId = item.Id
                });
            }
        }
        
        return results;
    }
}
```

### Environment-Specific Configuration

```csharp
public class EnvironmentConfigManager
{
    private readonly Dictionary<string, Dictionary<string, string>> _envConfigs = new();
    
    public void SetEnvironmentConfig(string environment, Dictionary<string, string> config)
    {
        _envConfigs[environment] = config;
    }
    
    public object TransformDefinitionForEnvironment(object definition, string environment)
    {
        if (!_envConfigs.TryGetValue(environment, out var config))
        {
            return definition;
        }
        
        var json = JsonSerializer.Serialize(definition);
        
        // Replace environment-specific placeholders
        foreach (var (key, value) in config)
        {
            json = json.Replace($"${{env:{key}}}", value);
        }
        
        return JsonSerializer.Deserialize<object>(json)!;
    }
}

// Usage
var configManager = new EnvironmentConfigManager();

configManager.SetEnvironmentConfig("Development", new Dictionary<string, string>
{
    ["DatabaseServer"] = "dev-sql-server.database.windows.net",
    ["StorageAccount"] = "devstorageaccount"
});

configManager.SetEnvironmentConfig("Production", new Dictionary<string, string>
{
    ["DatabaseServer"] = "prod-sql-server.database.windows.net",
    ["StorageAccount"] = "prodstorageaccount"
});
```

## Git Integration

### Syncing Items with Git

```csharp
public class GitSyncManager
{
    public async Task ExportToGitAsync(
        HttpClient client,
        string workspaceId,
        string localPath)
    {
        var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
        
        foreach (var item in items)
        {
            try
            {
                var definition = await GetItemDefinitionAsync(client, workspaceId, item.Id);
                
                // Create directory structure: {localPath}/{itemType}/{itemName}/
                var itemPath = Path.Combine(localPath, item.Type, SanitizeFileName(item.DisplayName));
                Directory.CreateDirectory(itemPath);
                
                // Write metadata
                var metadata = new ItemMetadata
                {
                    Id = item.Id,
                    DisplayName = item.DisplayName,
                    Type = item.Type,
                    Description = item.Description,
                    ExportedAt = DateTime.UtcNow
                };
                
                await File.WriteAllTextAsync(
                    Path.Combine(itemPath, "metadata.json"),
                    JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
                
                // Write definition
                await File.WriteAllTextAsync(
                    Path.Combine(itemPath, "definition.json"),
                    JsonSerializer.Serialize(definition, new JsonSerializerOptions { WriteIndented = true }));
                
                Console.WriteLine($"Exported: {item.Type}/{item.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to export {item.DisplayName}: {ex.Message}");
            }
        }
    }
    
    public async Task ImportFromGitAsync(
        HttpClient client,
        string workspaceId,
        string localPath)
    {
        var itemTypeDirs = Directory.GetDirectories(localPath);
        
        foreach (var typeDir in itemTypeDirs)
        {
            var itemType = Path.GetFileName(typeDir);
            var itemDirs = Directory.GetDirectories(typeDir);
            
            foreach (var itemDir in itemDirs)
            {
                var metadataPath = Path.Combine(itemDir, "metadata.json");
                var definitionPath = Path.Combine(itemDir, "definition.json");
                
                if (!File.Exists(metadataPath) || !File.Exists(definitionPath))
                {
                    Console.WriteLine($"Skipping incomplete item: {itemDir}");
                    continue;
                }
                
                var metadata = JsonSerializer.Deserialize<ItemMetadata>(
                    await File.ReadAllTextAsync(metadataPath))!;
                var definition = JsonSerializer.Deserialize<object>(
                    await File.ReadAllTextAsync(definitionPath))!;
                
                await CreateOrUpdateItemAsync(client, workspaceId, 
                    metadata.DisplayName, itemType, definition);
                
                Console.WriteLine($"Imported: {itemType}/{metadata.DisplayName}");
            }
        }
    }
    
    private string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid));
    }
}
```

### Git Workflow Integration

```csharp
public class GitWorkflowManager
{
    public async Task<PullRequestInfo> CreateDeploymentPullRequestAsync(
        string sourceBranch,
        string targetBranch,
        string workspaceId,
        List<string> changedItemIds)
    {
        // 1. Create feature branch with changes
        var branchName = $"deploy/{workspaceId}/{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        // 2. Generate change summary
        var changes = new List<string>();
        foreach (var itemId in changedItemIds)
        {
            var item = await GetItemAsync(itemId);
            changes.Add($"- {item.Type}: {item.DisplayName}");
        }
        
        // 3. Create pull request (using your Git provider's API)
        return new PullRequestInfo
        {
            Title = $"Deploy to {targetBranch}",
            Description = $"## Changes\n{string.Join("\n", changes)}",
            SourceBranch = branchName,
            TargetBranch = targetBranch
        };
    }
}
```

## Item Deletion and Cleanup

### Safe Deletion with Backup

```csharp
public class SafeItemDeleter
{
    private readonly string _backupPath;
    
    public SafeItemDeleter(string backupPath)
    {
        _backupPath = backupPath;
    }
    
    public async Task<DeleteResult> SafeDeleteAsync(
        HttpClient client,
        string workspaceId,
        string itemId,
        bool createBackup = true)
    {
        // 1. Get item details
        var item = await GetItemAsync(client, workspaceId, itemId);
        
        // 2. Create backup if requested
        if (createBackup)
        {
            var definition = await GetItemDefinitionAsync(client, workspaceId, itemId);
            
            var backupFile = Path.Combine(_backupPath, 
                $"{item.Type}_{item.DisplayName}_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            
            var backup = new
            {
                Item = item,
                Definition = definition,
                DeletedAt = DateTime.UtcNow
            };
            
            await File.WriteAllTextAsync(backupFile, 
                JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true }));
        }
        
        // 3. Delete the item
        var response = await client.DeleteAsync(
            $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items/{itemId}");
        
        response.EnsureSuccessStatusCode();
        
        return new DeleteResult
        {
            Success = true,
            DeletedItem = item,
            BackupCreated = createBackup
        };
    }
}
```

### Workspace Cleanup

```csharp
public class WorkspaceCleaner
{
    public async Task<CleanupReport> CleanupUnusedItemsAsync(
        HttpClient client,
        string workspaceId,
        TimeSpan unusedThreshold)
    {
        var report = new CleanupReport();
        var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
        var cutoffDate = DateTime.UtcNow - unusedThreshold;
        
        foreach (var item in items)
        {
            var lastActivity = await GetItemLastActivityAsync(client, workspaceId, item.Id);
            
            if (lastActivity < cutoffDate)
            {
                report.UnusedItems.Add(new UnusedItemInfo
                {
                    Item = item,
                    LastActivity = lastActivity,
                    DaysSinceLastUse = (DateTime.UtcNow - lastActivity).Days
                });
            }
        }
        
        return report;
    }
    
    public async Task<int> DeleteOrphanedItemsAsync(
        HttpClient client,
        string workspaceId)
    {
        var deletedCount = 0;
        var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
        
        // Find items with broken dependencies
        foreach (var item in items)
        {
            if (await HasBrokenDependenciesAsync(client, workspaceId, item))
            {
                Console.WriteLine($"Orphaned item found: {item.DisplayName} ({item.Type})");
                // Optionally delete or flag for review
                deletedCount++;
            }
        }
        
        return deletedCount;
    }
}
```

### Bulk Deletion

```csharp
public async Task<BulkDeleteResult> BulkDeleteAsync(
    HttpClient client,
    string workspaceId,
    List<string> itemIds,
    bool createBackups = true)
{
    var result = new BulkDeleteResult();
    var deleter = new SafeItemDeleter(_backupPath);
    
    foreach (var itemId in itemIds)
    {
        try
        {
            var deleteResult = await deleter.SafeDeleteAsync(
                client, workspaceId, itemId, createBackups);
            
            result.Succeeded.Add(deleteResult.DeletedItem!);
        }
        catch (Exception ex)
        {
            result.Failed.Add(new FailedDeletion
            {
                ItemId = itemId,
                Error = ex.Message
            });
        }
    }
    
    return result;
}
```

## Lifecycle Governance

### Item Lifecycle Policies

```csharp
public class LifecyclePolicyManager
{
    public record LifecyclePolicy(
        string Name,
        string ItemType,
        TimeSpan? MaxAge,
        TimeSpan? MaxInactivity,
        string Action); // "Archive", "Notify", "Delete"
    
    private readonly List<LifecyclePolicy> _policies = new();
    
    public void AddPolicy(LifecyclePolicy policy)
    {
        _policies.Add(policy);
    }
    
    public async Task<List<PolicyViolation>> EvaluatePoliciesAsync(
        HttpClient client,
        string workspaceId)
    {
        var violations = new List<PolicyViolation>();
        var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
        
        foreach (var item in items)
        {
            var applicablePolicies = _policies.Where(p => 
                p.ItemType == "*" || p.ItemType == item.Type);
            
            foreach (var policy in applicablePolicies)
            {
                var violation = await EvaluatePolicyForItemAsync(
                    client, workspaceId, item, policy);
                
                if (violation != null)
                {
                    violations.Add(violation);
                }
            }
        }
        
        return violations;
    }
    
    private async Task<PolicyViolation?> EvaluatePolicyForItemAsync(
        HttpClient client,
        string workspaceId,
        Item item,
        LifecyclePolicy policy)
    {
        // Check max age
        if (policy.MaxAge.HasValue)
        {
            var createdAt = item.CreatedDateTime ?? DateTime.MinValue;
            if (DateTime.UtcNow - createdAt > policy.MaxAge.Value)
            {
                return new PolicyViolation
                {
                    Item = item,
                    Policy = policy,
                    Reason = $"Item exceeds max age of {policy.MaxAge.Value.TotalDays} days"
                };
            }
        }
        
        // Check inactivity
        if (policy.MaxInactivity.HasValue)
        {
            var lastActivity = await GetItemLastActivityAsync(client, workspaceId, item.Id);
            if (DateTime.UtcNow - lastActivity > policy.MaxInactivity.Value)
            {
                return new PolicyViolation
                {
                    Item = item,
                    Policy = policy,
                    Reason = $"Item inactive for more than {policy.MaxInactivity.Value.TotalDays} days"
                };
            }
        }
        
        return null;
    }
}
```

## Key Takeaways

1. **Use templates for consistency** - Create reusable templates for common item types
2. **Track versions** - Maintain version history for critical items
3. **Validate before updating** - Always validate definitions before applying changes
4. **Implement rollback capability** - Keep backups before making changes
5. **Automate deployments** - Use pipelines for consistent cross-environment promotion
6. **Integrate with Git** - Store item definitions in source control
7. **Backup before deletion** - Always create backups before deleting items
8. **Apply lifecycle policies** - Automate cleanup of unused or stale items
9. **Handle dependencies** - Check for dependent items before deletion

## Additional Resources

- [Microsoft Fabric Items Documentation](https://learn.microsoft.com/fabric/get-started/fabric-items)
- [Fabric Deployment Pipelines](https://learn.microsoft.com/fabric/cicd/deployment-pipelines/intro-to-deployment-pipelines)
- [Git Integration in Fabric](https://learn.microsoft.com/fabric/cicd/git-integration/intro-to-git-integration)
- [Microsoft Fabric REST API - Items](https://learn.microsoft.com/rest/api/fabric/core/items)
