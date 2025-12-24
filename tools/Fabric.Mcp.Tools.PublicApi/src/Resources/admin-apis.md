---
title: Microsoft Fabric Admin APIs - Usage Guidelines
description: Learn when and how to use Fabric Admin APIs, understand permission requirements, and follow best practices for building applications that respect user roles and permissions.
author: fabricteam
ms.author: fabricteam
ms.service: fabric
ms.topic: concept-article
ms.date: 12/24/2024
#customer intent: As a Microsoft Fabric developer I want to understand when to use Admin APIs and how to request explicit permission from users.
---

# Microsoft Fabric Admin APIs - Usage Guidelines

Microsoft Fabric provides both **standard APIs** and **Admin APIs**. Understanding the differences and appropriate use cases is critical for building applications that respect user permissions and organizational security policies.

## What are Admin APIs?

Admin APIs are a special category of Microsoft Fabric REST APIs that require elevated administrative privileges. These APIs provide access to tenant-wide operations, cross-workspace management, and administrative functions that are restricted to Fabric administrators.

### Key Characteristics

- **Require Admin Privileges**: Only users with Fabric Administrator roles can call these APIs
- **Tenant-Wide Scope**: Often operate across all workspaces and resources in a tenant
- **Elevated Permissions**: Access sensitive configuration and management operations
- **Restricted by Default**: Should not be used without explicit user approval

## Admin API vs Standard API

| Aspect | Standard APIs | Admin APIs |
|--------|--------------|------------|
| **Scope** | User's accessible resources | Tenant-wide resources |
| **Permissions** | Standard workspace roles | Fabric Administrator role |
| **Use Cases** | App functionality | Administrative tasks |
| **User Approval** | Implicit (login) | **Explicit required** |
| **Typical Users** | All Fabric users | Fabric Admins only |

## When NOT to Use Admin APIs

❌ **Avoid Admin APIs for:**

- Standard application functionality
- Operations within user's own workspaces
- Creating or managing user's personal resources
- Reading data the user already has access to
- Building general-purpose applications for non-admin users

### Why This Matters

Using Admin APIs in regular applications causes:

1. **Access Denied Errors**: Non-admin users cannot execute the operations
2. **Security Violations**: Unnecessarily requests elevated privileges
3. **Failed Deployments**: Applications fail for 99% of users
4. **Compliance Issues**: Violates principle of least privilege

## When to Use Admin APIs

✅ **Use Admin APIs only for:**

- **Tenant Administration Tools**: Applications explicitly designed for Fabric administrators
- **Compliance and Auditing**: Cross-tenant reporting and monitoring
- **Capacity Management**: Managing Fabric capacities across the organization
- **User Administration**: Managing workspace access and permissions at scale
- **Governance Tools**: Enforcing organizational policies and standards

### Required Conditions

Before using Admin APIs, ensure:

1. ✅ The application is **explicitly designed for administrators**
2. ✅ The user has **confirmed they have admin privileges**
3. ✅ The use case **requires tenant-wide operations**
4. ✅ Alternative standard APIs **cannot accomplish the task**

## How to Request User Approval

When an Admin API is truly necessary, follow these steps:

### 1. Detect the Need for Admin APIs

```typescript
// Example: Detecting if admin API is needed
function requiresAdminApi(operation: string): boolean {
    const adminOperations = [
        'list-all-workspaces-in-tenant',
        'get-capacity-usage-across-tenant',
        'manage-tenant-settings',
        'audit-all-user-activities'
    ];
    
    return adminOperations.includes(operation);
}
```

### 2. Request Explicit Permission

**Bad Example (Never do this):**
```typescript
// ❌ DON'T: Automatically use admin API without asking
async function listWorkspaces() {
    // This will fail for non-admin users!
    return await fabricAdminApi.listAllWorkspaces();
}
```

**Good Example:**
```typescript
// ✅ DO: Request explicit permission first
async function listWorkspaces(userContext: UserContext) {
    const needsAdminApi = requiresAdminApi('list-all-workspaces-in-tenant');
    
    if (needsAdminApi) {
        // Request explicit permission
        const hasPermission = await requestAdminPermission(userContext, {
            operation: 'List all workspaces across tenant',
            reason: 'This operation requires Fabric Administrator privileges',
            scope: 'tenant-wide'
        });
        
        if (!hasPermission) {
            // Fall back to user-scoped API
            return await fabricApi.listMyWorkspaces();
        }
        
        // User approved and has admin role
        return await fabricAdminApi.listAllWorkspaces();
    }
    
    // Use standard API for non-admin operations
    return await fabricApi.listMyWorkspaces();
}
```

### 3. Display Clear Permission Prompts

```typescript
interface AdminPermissionRequest {
    operation: string;
    reason: string;
    scope: string;
}

async function requestAdminPermission(
    userContext: UserContext, 
    request: AdminPermissionRequest
): Promise<boolean> {
    // Display clear UI prompt
    const userResponse = await showDialog({
        title: 'Administrator Permission Required',
        message: `
            This operation requires Fabric Administrator privileges:
            
            Operation: ${request.operation}
            Scope: ${request.scope}
            Reason: ${request.reason}
            
            Do you have Fabric Administrator role and want to proceed?
            
            ⚠️ If you are not a Fabric Administrator, this operation will fail.
            ℹ️ Alternative: Use standard workspace operations instead.
        `,
        buttons: ['Yes, I am an Admin', 'No, Use Standard Access']
    });
    
    return userResponse === 'Yes, I am an Admin';
}
```

## Graceful Fallback Patterns

Always provide fallback to standard APIs when possible:

### Pattern 1: Scoped Fallback

```csharp
public async Task<List<Workspace>> GetWorkspacesAsync(UserContext user, bool allowAdmin = false)
{
    if (allowAdmin && user.HasAdminRole)
    {
        try
        {
            // Try admin API for tenant-wide view
            return await _fabricAdminClient.GetAllWorkspacesAsync();
        }
        catch (UnauthorizedException)
        {
            // Fall back to user-scoped API
            return await _fabricClient.GetMyWorkspacesAsync(user);
        }
    }
    
    // Use standard API by default
    return await _fabricClient.GetMyWorkspacesAsync(user);
}
```

### Pattern 2: Feature Detection

```python
async def get_capacity_metrics(fabric_client, user_context):
    """Get capacity metrics with automatic fallback."""
    
    # Check if user has admin capabilities
    if user_context.is_admin:
        try:
            # Attempt tenant-wide metrics
            return await fabric_client.admin.get_all_capacity_metrics()
        except ForbiddenError:
            print("Admin API unavailable, falling back to user scope")
    
    # Fallback: Get only user's accessible capacities
    return await fabric_client.get_my_capacity_metrics(user_context.user_id)
```

### Pattern 3: Progressive Permission

```typescript
class FabricWorkspaceManager {
    async getWorkspaces(options: WorkspaceQueryOptions): Promise<Workspace[]> {
        // Start with standard API
        let workspaces = await this.client.workspaces.list();
        
        // Check if user wants tenant-wide view
        if (options.scope === 'tenant-wide') {
            const approval = await this.requestAdminAccess(
                'View all workspaces in the tenant'
            );
            
            if (approval && await this.verifyAdminRole()) {
                workspaces = await this.adminClient.workspaces.listAll();
            } else {
                throw new Error(
                    'Tenant-wide workspace access requires Fabric Administrator role. ' +
                    'Using your accessible workspaces instead.'
                );
            }
        }
        
        return workspaces;
    }
}
```

## Error Handling for Admin APIs

Provide clear, actionable error messages:

```csharp
public async Task<AdminOperationResult> ExecuteAdminOperationAsync(string operation)
{
    try
    {
        return await _adminClient.ExecuteAsync(operation);
    }
    catch (FabricAdminException ex) when (ex.StatusCode == 403)
    {
        return new AdminOperationResult
        {
            Success = false,
            ErrorMessage = @"
                This operation requires Fabric Administrator privileges.
                
                Current Issue: Your account does not have the required administrator role.
                
                Solutions:
                1. Contact your Fabric Administrator to grant you admin access
                2. Use workspace-scoped operations instead (recommended)
                3. Request the admin to perform this operation on your behalf
                
                Alternative: Most operations can be accomplished using standard APIs 
                within your own workspaces without admin privileges.
            "
        };
    }
    catch (FabricAdminException ex) when (ex.StatusCode == 401)
    {
        return new AdminOperationResult
        {
            Success = false,
            ErrorMessage = "Authentication failed. Please sign in again."
        };
    }
}
```

## Best Practices Summary

### For LLM Code Generation

When generating code for Fabric applications:

1. **Default to Standard APIs**: Always use standard, user-scoped APIs unless explicitly required
2. **Detect Admin Requirements**: Analyze if the operation truly needs admin privileges
3. **Request Explicit Permission**: Never silently use admin APIs - always ask the user first
4. **Provide Alternatives**: Suggest workspace-scoped alternatives when possible
5. **Handle Gracefully**: Implement proper error handling and fallback patterns
6. **Document Requirements**: Clearly state when admin privileges are needed

### Decision Tree

```
Does the operation need tenant-wide access?
├─ NO → Use standard APIs (workspace-scoped)
└─ YES → Is user a Fabric Administrator?
    ├─ NO → Provide alternative or explain limitation
    └─ YES → Request explicit permission
        ├─ DENIED → Use standard APIs as fallback
        └─ APPROVED → Use Admin API with error handling
```

## Common Admin API Patterns

### ❌ Anti-Pattern: Implicit Admin Usage

```typescript
// DON'T: This will fail for 99% of users
async function exportAllData() {
    const allWorkspaces = await fabricAdmin.getWorkspaces();
    // This assumes user is admin!
}
```

### ✅ Correct Pattern: Explicit Permission with Fallback

```typescript
// DO: Request permission and provide alternatives
async function exportData(scope: 'my-workspaces' | 'all-workspaces') {
    if (scope === 'all-workspaces') {
        const hasAdminRole = await confirmAdminRole();
        if (!hasAdminRole) {
            console.warn('Admin role required for tenant-wide export. Using accessible workspaces.');
            scope = 'my-workspaces';
        }
    }
    
    const workspaces = scope === 'all-workspaces'
        ? await fabricAdmin.getWorkspaces()
        : await fabric.getMyWorkspaces();
    
    return await exportWorkspacesData(workspaces);
}
```

## Identifying Admin APIs

Admin APIs typically have these characteristics:

### URL Patterns
- Contain `/admin/` in the path
- Example: `https://api.fabric.microsoft.com/v1/admin/workspaces`

### Documentation Indicators
- Marked as "Admin API" in API documentation
- Require "Fabric Administrator" role in prerequisites
- Scope listed as "Tenant" or "Organization-wide"

### Permission Requirements
- Require `Tenant.Read.All` or `Tenant.ReadWrite.All` scopes
- Listed under "Administrator Operations" in API docs

## Testing Admin API Code

```typescript
// Create separate test contexts for admin and non-admin users
describe('Workspace Operations', () => {
    it('should use standard API for regular users', async () => {
        const regularUser = createTestUser({ isAdmin: false });
        const workspaces = await getWorkspaces(regularUser);
        
        expect(workspaces).toEqual(regularUser.accessibleWorkspaces);
        expect(mockStandardApi).toHaveBeenCalled();
        expect(mockAdminApi).not.toHaveBeenCalled();
    });
    
    it('should request permission before using admin API', async () => {
        const adminUser = createTestUser({ isAdmin: true });
        const workspaces = await getWorkspaces(adminUser, { scope: 'tenant' });
        
        expect(mockPermissionDialog).toHaveBeenCalled();
        expect(mockAdminApi).toHaveBeenCalledAfter(mockPermissionDialog);
    });
    
    it('should fallback gracefully when admin denied', async () => {
        const adminUser = createTestUser({ isAdmin: true });
        mockPermissionDialog.mockReturnValue(false);
        
        const workspaces = await getWorkspaces(adminUser, { scope: 'tenant' });
        
        expect(workspaces).toEqual(adminUser.accessibleWorkspaces);
        expect(mockAdminApi).not.toHaveBeenCalled();
    });
});
```

## Key Takeaways

1. **Admin APIs are for Administrators Only** - Don't use them in general-purpose applications
2. **Always Request Explicit Permission** - Never silently use admin APIs
3. **Provide Clear Alternatives** - Suggest workspace-scoped operations when possible
4. **Implement Graceful Fallbacks** - Handle permission denied scenarios elegantly
5. **Default to Standard APIs** - Use admin APIs only when absolutely necessary
6. **Document Admin Requirements** - Clearly state when admin privileges are needed
7. **Test with Both User Types** - Verify behavior for admin and non-admin users

## Additional Resources

- [Microsoft Fabric REST API Documentation](https://learn.microsoft.com/rest/api/fabric/)
- [Fabric Administrator Roles](https://learn.microsoft.com/fabric/admin/roles)
- [API Throttling Best Practices](throttling.md)
- [Authentication and Authorization](https://learn.microsoft.com/fabric/security/security-overview)
- [Principle of Least Privilege](https://learn.microsoft.com/azure/active-directory/develop/secure-least-privileged-access)