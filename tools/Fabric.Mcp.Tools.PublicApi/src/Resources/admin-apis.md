---
title: Microsoft Fabric Admin APIs - Usage Guidelines
description: Learn when and how to use Fabric Admin APIs, understand permission requirements, and follow best practices for building applications that respect user roles and permissions.
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


## Key Takeaways

1. **Admin APIs are for Administrators Only** - Don't use them in general-purpose applications
2. **Always Request Explicit Permission** - Never silently use admin APIs
3. **Provide Clear Alternatives** - Suggest workspace-scoped operations when possible

## Additional Resources

- [Microsoft Fabric REST API Documentation](https://learn.microsoft.com/rest/api/fabric/)
- [Fabric Administrator Roles](https://learn.microsoft.com/fabric/admin/roles)
- [Authentication and Authorization](https://learn.microsoft.com/fabric/security/security-overview)
- [Principle of Least Privilege](https://learn.microsoft.com/azure/active-directory/develop/secure-least-privileged-access)