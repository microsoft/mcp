---
title: Workspace and Capacity Management for Fabric REST APIs
description: Learn best practices for creating and managing workspaces, assigning capacities, understanding resource limits, and performing cross-workspace operations in Microsoft Fabric.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to manage workspaces and capacities effectively using Fabric APIs.
---

# Workspace and Capacity Management

Microsoft Fabric organizes resources into workspaces that are assigned to capacities. Understanding how to effectively manage workspaces and capacities is essential for building scalable Fabric solutions.

## Workspace Overview

A workspace is a container for Fabric items (lakehouses, notebooks, pipelines, reports, etc.) that provides:

- **Collaboration**: Team members can share and work on items together
- **Security**: Role-based access control for workspace contents
- **Governance**: Centralized management of related resources
- **Billing**: Resource usage is tracked per workspace

## Creating Workspaces

### Basic Workspace Creation

```csharp
public async Task<Workspace> CreateWorkspaceAsync(
    HttpClient client,
    string displayName,
    string? description = null)
{
    var request = new
    {
        displayName = displayName,
        description = description
    };
    
    var response = await client.PostAsJsonAsync(
        "https://api.fabric.microsoft.com/v1/workspaces",
        request);
    
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<Workspace>();
}
```

### Workspace with Capacity Assignment

```csharp
public async Task<Workspace> CreateWorkspaceWithCapacityAsync(
    HttpClient client,
    string displayName,
    string capacityId,
    string? description = null)
{
    var request = new
    {
        displayName = displayName,
        description = description,
        capacityId = capacityId
    };
    
    var response = await client.PostAsJsonAsync(
        "https://api.fabric.microsoft.com/v1/workspaces",
        request);
    
    if (response.StatusCode == HttpStatusCode.BadRequest)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to create workspace: {error}");
    }
    
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Workspace>();
}
```

### Python Example

```python
import requests

def create_workspace(
    access_token: str,
    display_name: str,
    capacity_id: str | None = None,
    description: str | None = None
) -> dict:
    """Create a new Fabric workspace."""
    
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
    }
    
    payload = {
        "displayName": display_name
    }
    
    if description:
        payload["description"] = description
    
    if capacity_id:
        payload["capacityId"] = capacity_id
    
    response = requests.post(
        "https://api.fabric.microsoft.com/v1/workspaces",
        headers=headers,
        json=payload
    )
    
    response.raise_for_status()
    return response.json()
```

## Workspace Naming Best Practices

### Naming Conventions

| Pattern | Example | Use Case |
|---------|---------|----------|
| `{Team}-{Project}-{Environment}` | `Analytics-Sales-Prod` | Team-based organization |
| `{Department}-{Function}` | `Finance-Reporting` | Departmental separation |
| `{Project}-{Stage}` | `CustomerInsights-Dev` | Project lifecycle |

### Validation Rules

```csharp
public static class WorkspaceNameValidator
{
    private static readonly Regex ValidNamePattern = new(@"^[a-zA-Z0-9][a-zA-Z0-9\s\-_]{0,254}$");
    
    public static bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        
        if (name.Length > 256)
            return false;
        
        // Cannot start or end with whitespace
        if (name != name.Trim())
            return false;
        
        return ValidNamePattern.IsMatch(name);
    }
    
    public static string SanitizeName(string name)
    {
        // Remove invalid characters and trim
        var sanitized = Regex.Replace(name.Trim(), @"[^\w\s\-]", "");
        
        // Truncate if necessary
        if (sanitized.Length > 256)
            sanitized = sanitized.Substring(0, 256);
        
        return sanitized;
    }
}
```

## Capacity Management

### Understanding Capacity SKUs

| SKU | CU (Capacity Units) | Typical Use Case |
|-----|---------------------|------------------|
| F2 | 2 | Development, small workloads |
| F4 | 4 | Small teams, testing |
| F8 | 8 | Department-level workloads |
| F16 | 16 | Medium enterprise workloads |
| F32 | 32 | Large enterprise workloads |
| F64 | 64 | High-performance analytics |
| F128+ | 128+ | Enterprise-scale deployments |

### Listing Available Capacities

```csharp
public async Task<List<Capacity>> GetAvailableCapacitiesAsync(HttpClient client)
{
    var response = await client.GetAsync(
        "https://api.fabric.microsoft.com/v1/capacities");
    
    response.EnsureSuccessStatusCode();
    
    var result = await response.Content.ReadFromJsonAsync<CapacityListResponse>();
    return result?.Value ?? new List<Capacity>();
}

public record Capacity(
    string Id,
    string DisplayName,
    string Sku,
    string Region,
    string State);
```

### Assigning Workspace to Capacity

```csharp
public async Task AssignWorkspaceToCapacityAsync(
    HttpClient client,
    string workspaceId,
    string capacityId)
{
    var request = new { capacityId = capacityId };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/assignToCapacity",
        request);
    
    if (response.StatusCode == HttpStatusCode.Accepted)
    {
        // Long-running operation - poll for completion
        var operationUrl = response.Headers.Location?.ToString();
        await PollForCompletionAsync(client, operationUrl);
    }
    else
    {
        response.EnsureSuccessStatusCode();
    }
}
```

### Unassigning from Capacity

```csharp
public async Task UnassignWorkspaceFromCapacityAsync(
    HttpClient client,
    string workspaceId)
{
    var response = await client.PostAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/unassignFromCapacity",
        null);
    
    response.EnsureSuccessStatusCode();
}
```

## Resource Limits and Quotas

### Workspace Limits

| Limit | Value | Notes |
|-------|-------|-------|
| Max items per workspace | 1,000+ | Varies by item type |
| Max workspace name length | 256 characters | |
| Max description length | 4,000 characters | |
| Max members per workspace | 1,000+ | Check current limits |

### Checking Workspace Usage

```csharp
public async Task<WorkspaceUsage> GetWorkspaceUsageAsync(
    HttpClient client,
    string workspaceId)
{
    // Get all items to calculate usage
    var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
    
    return new WorkspaceUsage
    {
        TotalItems = items.Count,
        ItemsByType = items
            .GroupBy(i => i.Type)
            .ToDictionary(g => g.Key, g => g.Count()),
        LastUpdated = DateTime.UtcNow
    };
}
```

## Cross-Workspace Operations

### Moving Items Between Workspaces

```csharp
public async Task MoveItemAsync(
    HttpClient client,
    string sourceWorkspaceId,
    string itemId,
    string targetWorkspaceId)
{
    // Note: Not all item types support moving
    // Check API documentation for supported types
    
    var request = new
    {
        targetWorkspaceId = targetWorkspaceId
    };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{sourceWorkspaceId}/items/{itemId}/move",
        request);
    
    if (response.StatusCode == HttpStatusCode.BadRequest)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Cannot move item: {error}");
    }
    
    response.EnsureSuccessStatusCode();
}
```

### Copying Items Across Workspaces

```csharp
public async Task<Item> CopyItemAsync(
    HttpClient client,
    string sourceWorkspaceId,
    string itemId,
    string targetWorkspaceId,
    string newDisplayName)
{
    // Get item definition
    var definition = await GetItemDefinitionAsync(client, sourceWorkspaceId, itemId);
    
    // Create in target workspace
    var createRequest = new
    {
        displayName = newDisplayName,
        type = definition.Type,
        definition = definition.Definition
    };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{targetWorkspaceId}/items",
        createRequest);
    
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Item>();
}
```

### Listing Items Across Multiple Workspaces

```csharp
public async Task<List<ItemWithWorkspace>> GetItemsAcrossWorkspacesAsync(
    HttpClient client,
    List<string> workspaceIds,
    string? itemType = null)
{
    var allItems = new List<ItemWithWorkspace>();
    
    // Execute in parallel for better performance
    var tasks = workspaceIds.Select(async workspaceId =>
    {
        var items = await GetWorkspaceItemsAsync(client, workspaceId, itemType);
        return items.Select(item => new ItemWithWorkspace(item, workspaceId));
    });
    
    var results = await Task.WhenAll(tasks);
    
    foreach (var workspaceItems in results)
    {
        allItems.AddRange(workspaceItems);
    }
    
    return allItems;
}
```

## Workspace Access Management

### Adding Workspace Members

```csharp
public async Task AddWorkspaceMemberAsync(
    HttpClient client,
    string workspaceId,
    string userEmail,
    WorkspaceRole role)
{
    var request = new
    {
        principal = new
        {
            type = "User",
            id = userEmail  // Can be email or object ID
        },
        role = role.ToString()
    };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/roleAssignments",
        request);
    
    response.EnsureSuccessStatusCode();
}

public enum WorkspaceRole
{
    Admin,
    Member,
    Contributor,
    Viewer
}
```

### Workspace Role Permissions

| Role | View | Edit | Share | Manage |
|------|------|------|-------|--------|
| Viewer | ✅ | ❌ | ❌ | ❌ |
| Contributor | ✅ | ✅ | ❌ | ❌ |
| Member | ✅ | ✅ | ✅ | ❌ |
| Admin | ✅ | ✅ | ✅ | ✅ |

## Workspace Lifecycle Management

### Workspace Cleanup Script

```csharp
public async Task CleanupOldWorkspacesAsync(
    HttpClient client,
    TimeSpan inactivityThreshold)
{
    var workspaces = await GetAllWorkspacesAsync(client);
    var cutoffDate = DateTime.UtcNow - inactivityThreshold;
    
    foreach (var workspace in workspaces)
    {
        // Check last activity (implementation depends on your tracking)
        var lastActivity = await GetLastActivityAsync(client, workspace.Id);
        
        if (lastActivity < cutoffDate)
        {
            Console.WriteLine($"Workspace '{workspace.DisplayName}' inactive since {lastActivity}");
            
            // Option 1: Archive (if supported)
            // Option 2: Notify owners
            // Option 3: Delete (with confirmation)
        }
    }
}
```

### Workspace Backup Strategy

```csharp
public async Task BackupWorkspaceDefinitionsAsync(
    HttpClient client,
    string workspaceId,
    string backupPath)
{
    var items = await GetAllWorkspaceItemsAsync(client, workspaceId);
    
    foreach (var item in items)
    {
        try
        {
            var definition = await GetItemDefinitionAsync(client, workspaceId, item.Id);
            
            var filePath = Path.Combine(backupPath, item.Type, $"{item.DisplayName}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            
            await File.WriteAllTextAsync(filePath, 
                JsonSerializer.Serialize(definition, new JsonSerializerOptions { WriteIndented = true }));
            
            Console.WriteLine($"Backed up: {item.Type}/{item.DisplayName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to backup {item.DisplayName}: {ex.Message}");
        }
    }
}
```

## Key Takeaways

1. **Plan workspace organization** - Use consistent naming conventions aligned with your organization structure
2. **Right-size capacities** - Choose SKUs appropriate for workload requirements
3. **Manage access carefully** - Apply principle of least privilege for workspace roles
4. **Monitor resource usage** - Track item counts and capacity utilization
5. **Implement lifecycle management** - Archive or clean up unused workspaces
6. **Use parallel operations** - Improve performance for cross-workspace queries
7. **Handle capacity assignment as LRO** - Capacity operations may be long-running
8. **Back up item definitions** - Maintain copies of critical workspace configurations

## Additional Resources

- [Microsoft Fabric Workspaces Documentation](https://learn.microsoft.com/fabric/get-started/workspaces)
- [Fabric Capacity and SKUs](https://learn.microsoft.com/fabric/enterprise/licenses)
- [Workspace Roles and Permissions](https://learn.microsoft.com/fabric/get-started/roles-workspaces)
- [Microsoft Fabric REST API - Workspaces](https://learn.microsoft.com/rest/api/fabric/core/workspaces)
