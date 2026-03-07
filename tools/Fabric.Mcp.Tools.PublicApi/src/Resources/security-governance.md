---
title: Security and Governance for Fabric REST APIs
description: Learn best practices for implementing security controls, data governance, audit logging, and compliance requirements when building applications with Microsoft Fabric REST APIs.
ms.date: 01/25/2026
#customer intent: As a Microsoft Fabric developer I want to learn how to implement security and governance controls in my Fabric applications.
---

# Security and Governance

Building secure and compliant applications with Microsoft Fabric requires understanding security controls, data governance features, and audit capabilities. This guide covers best practices for implementing robust security and governance in your Fabric solutions.

## Security Principles

### Defense in Depth

Implement multiple layers of security:

1. **Identity & Access** - Authentication and authorization
2. **Network** - Private endpoints, firewalls
3. **Data** - Encryption, sensitivity labels
4. **Application** - Input validation, secure coding
5. **Monitoring** - Audit logs, alerts

### Principle of Least Privilege

Grant only the minimum permissions required:

```csharp
public class PermissionManager
{
    public WorkspaceRole DetermineMinimumRole(RequiredPermissions permissions)
    {
        // Only grant what's needed
        if (permissions.NeedsManagement)
            return WorkspaceRole.Admin;
        
        if (permissions.NeedsSharing)
            return WorkspaceRole.Member;
        
        if (permissions.NeedsEditing)
            return WorkspaceRole.Contributor;
        
        return WorkspaceRole.Viewer;
    }
}
```

## Row-Level Security (RLS)

### Understanding RLS

Row-Level Security filters data based on user identity, ensuring users only see data they're authorized to access.

### Implementing RLS in Semantic Models

```dax
// DAX filter expression for RLS role
[Region] = USERPRINCIPALNAME()

// More complex: lookup user's allowed regions
VAR UserEmail = USERPRINCIPALNAME()
VAR AllowedRegions = 
    FILTER(
        UserRegionMapping,
        UserRegionMapping[UserEmail] = UserEmail
    )
RETURN
    [Region] IN SELECTCOLUMNS(AllowedRegions, "Region", [Region])
```

### Dynamic RLS with Security Tables

```csharp
public class RlsSecurityTable
{
    public string UserEmail { get; set; }
    public string Region { get; set; }
    public string Department { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

// Keep security tables updated
public async Task UpdateSecurityTableAsync(
    HttpClient client,
    string workspaceId,
    string lakehouseId,
    List<RlsSecurityTable> securityMappings)
{
    // Upload security mappings to lakehouse table
    var csvData = ConvertToCsv(securityMappings);
    
    await UploadToOneLakeAsync(
        client,
        workspaceId,
        lakehouseId,
        "Tables/SecurityMappings",
        csvData);
}
```

### Testing RLS

```csharp
public async Task<bool> TestRlsAsync(
    HttpClient client,
    string workspaceId,
    string datasetId,
    string userEmail,
    string roleName)
{
    // Execute query as specific user to test RLS
    var request = new
    {
        queries = new[]
        {
            new { query = "EVALUATE ROW(\"Count\", COUNTROWS(SalesData))" }
        },
        impersonatedUserName = userEmail,
        roleName = roleName
    };
    
    var response = await client.PostAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/datasets/{datasetId}/executeQueries",
        request);
    
    return response.IsSuccessStatusCode;
}
```

## Data Sensitivity Labels

### Applying Sensitivity Labels

```csharp
public async Task ApplySensitivityLabelAsync(
    HttpClient client,
    string workspaceId,
    string itemId,
    string labelId)
{
    var request = new
    {
        labelId = labelId
    };
    
    var response = await client.PatchAsJsonAsync(
        $"https://api.fabric.microsoft.com/v1/workspaces/{workspaceId}/items/{itemId}/sensitivityLabel",
        request);
    
    response.EnsureSuccessStatusCode();
}
```

### Sensitivity Label Hierarchy

| Label | Protection Level | Typical Use |
|-------|-----------------|-------------|
| Public | None | Marketing materials, public data |
| General | Basic | Internal business data |
| Confidential | Encryption | Customer data, financial records |
| Highly Confidential | Strict controls | PII, trade secrets, regulated data |

### Checking Label Requirements

```csharp
public class SensitivityLabelValidator
{
    private readonly Dictionary<string, string> _dataTypeToMinimumLabel = new()
    {
        ["CustomerPII"] = "Confidential",
        ["FinancialData"] = "Confidential",
        ["HealthData"] = "HighlyConfidential",
        ["PublicData"] = "General"
    };
    
    public bool ValidateLabelForDataType(string dataType, string appliedLabel)
    {
        if (!_dataTypeToMinimumLabel.TryGetValue(dataType, out var minimumLabel))
            return true; // No specific requirement
        
        return GetLabelPriority(appliedLabel) >= GetLabelPriority(minimumLabel);
    }
    
    private int GetLabelPriority(string label) => label switch
    {
        "Public" => 0,
        "General" => 1,
        "Confidential" => 2,
        "HighlyConfidential" => 3,
        _ => -1
    };
}
```

## Secure Credential Handling

### Never Hardcode Secrets

```csharp
// ❌ NEVER do this
var connectionString = "Server=myserver;Password=MyP@ssw0rd!";

// ✅ Use environment variables
var connectionString = Environment.GetEnvironmentVariable("FABRIC_CONNECTION_STRING");

// ✅ Better: Use Azure Key Vault
public class SecureConfigProvider
{
    private readonly SecretClient _secretClient;
    
    public SecureConfigProvider(string keyVaultUrl)
    {
        _secretClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
```

### Secure Connection String Patterns

```csharp
public class SecureConnectionBuilder
{
    public string BuildSecureConnectionString(
        string server,
        string database,
        bool useManagedIdentity = true)
    {
        if (useManagedIdentity)
        {
            // Use Managed Identity - no secrets needed
            return $"Server={server};Database={database};Authentication=Active Directory Managed Identity;";
        }
        
        // Fall back to Key Vault for credentials
        var password = GetFromKeyVault("db-password");
        var username = GetFromKeyVault("db-username");
        
        return $"Server={server};Database={database};User Id={username};Password={password};Encrypt=true;";
    }
}
```

## Audit Logging

### Enabling Audit Logs

Fabric automatically logs administrative and user activities. Access audit logs through:

- Microsoft Purview compliance portal
- Microsoft 365 unified audit log
- Fabric Admin APIs

### Querying Audit Events

```csharp
public async Task<List<AuditEvent>> GetAuditEventsAsync(
    HttpClient client,
    DateTime startTime,
    DateTime endTime,
    string? activityType = null)
{
    var url = $"https://api.fabric.microsoft.com/v1/admin/activityevents?" +
        $"startDateTime={startTime:yyyy-MM-ddTHH:mm:ssZ}&" +
        $"endDateTime={endTime:yyyy-MM-ddTHH:mm:ssZ}";
    
    if (!string.IsNullOrEmpty(activityType))
    {
        url += $"&activityEventType={activityType}";
    }
    
    var allEvents = new List<AuditEvent>();
    string? continuationToken = null;
    
    do
    {
        var requestUrl = continuationToken != null 
            ? $"{url}&continuationToken={continuationToken}" 
            : url;
        
        var response = await client.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<AuditEventResponse>();
        allEvents.AddRange(result?.ActivityEventEntities ?? new List<AuditEvent>());
        continuationToken = result?.ContinuationToken;
        
    } while (!string.IsNullOrEmpty(continuationToken));
    
    return allEvents;
}
```

### Common Audit Event Types

| Event Type | Description |
|------------|-------------|
| `CreateWorkspace` | Workspace created |
| `DeleteWorkspace` | Workspace deleted |
| `UpdateWorkspaceAccess` | Permissions changed |
| `CreateReport` | Report created |
| `ViewReport` | Report viewed |
| `ExportReport` | Report exported |
| `RefreshDataset` | Dataset refreshed |
| `GetDatasources` | Data source info accessed |

### Building Audit Reports

```csharp
public class AuditReporter
{
    public async Task<AuditSummary> GenerateSecurityAuditAsync(
        HttpClient client,
        DateTime startDate,
        DateTime endDate)
    {
        var events = await GetAuditEventsAsync(client, startDate, endDate);
        
        return new AuditSummary
        {
            Period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            TotalEvents = events.Count,
            
            // Security-relevant activities
            PermissionChanges = events.Count(e => 
                e.Activity.Contains("Access") || e.Activity.Contains("Permission")),
            
            DataExports = events.Count(e => 
                e.Activity.Contains("Export") || e.Activity.Contains("Download")),
            
            FailedAuthentications = events.Count(e => 
                e.Activity.Contains("Failed") && e.Activity.Contains("Auth")),
            
            // Group by user for review
            TopActiveUsers = events
                .GroupBy(e => e.UserId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new UserActivity(g.Key, g.Count()))
                .ToList(),
            
            // Sensitive operations
            SensitiveOperations = events
                .Where(e => IsSensitiveOperation(e.Activity))
                .ToList()
        };
    }
    
    private bool IsSensitiveOperation(string activity)
    {
        var sensitiveActivities = new[]
        {
            "Delete", "Export", "Share", "UpdateAccess", 
            "AddMember", "RemoveMember", "CreateGateway"
        };
        
        return sensitiveActivities.Any(s => activity.Contains(s));
    }
}
```

## Compliance Considerations

### Data Residency

```csharp
public class DataResidencyValidator
{
    private readonly HashSet<string> _allowedRegions;
    
    public DataResidencyValidator(IEnumerable<string> allowedRegions)
    {
        _allowedRegions = new HashSet<string>(allowedRegions, StringComparer.OrdinalIgnoreCase);
    }
    
    public bool ValidateCapacityRegion(Capacity capacity)
    {
        return _allowedRegions.Contains(capacity.Region);
    }
    
    public async Task<List<ComplianceViolation>> CheckDataResidencyAsync(
        HttpClient client,
        string[] workspaceIds)
    {
        var violations = new List<ComplianceViolation>();
        
        foreach (var workspaceId in workspaceIds)
        {
            var workspace = await GetWorkspaceAsync(client, workspaceId);
            
            if (workspace.CapacityId != null)
            {
                var capacity = await GetCapacityAsync(client, workspace.CapacityId);
                
                if (!ValidateCapacityRegion(capacity))
                {
                    violations.Add(new ComplianceViolation
                    {
                        WorkspaceId = workspaceId,
                        WorkspaceName = workspace.DisplayName,
                        ViolationType = "DataResidency",
                        Details = $"Workspace in non-compliant region: {capacity.Region}"
                    });
                }
            }
        }
        
        return violations;
    }
}
```

### GDPR Data Subject Requests

```csharp
public class DataSubjectRequestHandler
{
    public async Task<DataSubjectReport> GenerateDataSubjectReportAsync(
        HttpClient client,
        string userEmail)
    {
        var report = new DataSubjectReport { UserEmail = userEmail };
        
        // Find all workspaces user has access to
        report.AccessibleWorkspaces = await FindUserWorkspacesAsync(client, userEmail);
        
        // Find audit trail for user
        report.AuditTrail = await GetUserAuditTrailAsync(client, userEmail);
        
        // Find items created by user
        report.CreatedItems = await FindUserCreatedItemsAsync(client, userEmail);
        
        return report;
    }
    
    public async Task ProcessDataDeletionRequestAsync(
        HttpClient client,
        string userEmail,
        DataDeletionScope scope)
    {
        // Log the request for compliance
        await LogDataSubjectRequestAsync(userEmail, "Deletion", scope);
        
        if (scope.IncludeAuditLogs)
        {
            // Note: Audit log retention is governed by Microsoft 365 policies
            Console.WriteLine("Audit logs governed by M365 retention policies");
        }
        
        if (scope.IncludePersonalContent)
        {
            // Remove user from workspace memberships
            await RemoveUserFromAllWorkspacesAsync(client, userEmail);
        }
    }
}
```

## Network Security

### Private Endpoints

When accessing Fabric from Azure resources, use private endpoints:

```csharp
public class SecureNetworkConfiguration
{
    public string GetSecureFabricEndpoint(bool usePrivateEndpoint)
    {
        if (usePrivateEndpoint)
        {
            // Use private endpoint URL (configured in your VNet)
            return "https://your-fabric-private-endpoint.privatelink.fabric.microsoft.com";
        }
        
        return "https://api.fabric.microsoft.com";
    }
}
```

### IP Allowlisting

For service principals, consider IP restrictions:

```csharp
public class IpRestrictionValidator
{
    private readonly HashSet<string> _allowedIpRanges;
    
    public bool ValidateSourceIp(string clientIp)
    {
        // Implement CIDR range checking
        return _allowedIpRanges.Any(range => IsIpInRange(clientIp, range));
    }
}
```

## Security Checklist

### Pre-Deployment

- [ ] All secrets stored in Key Vault
- [ ] Service principals have minimum required permissions
- [ ] Sensitivity labels applied to all datasets
- [ ] RLS implemented and tested
- [ ] Network security configured (private endpoints if needed)
- [ ] Audit logging enabled and monitored

### Ongoing Operations

- [ ] Regular access reviews conducted
- [ ] Audit logs reviewed weekly
- [ ] Security patches applied promptly
- [ ] Credentials rotated regularly
- [ ] Compliance reports generated monthly

## Key Takeaways

1. **Implement defense in depth** - Multiple security layers protect against breaches
2. **Apply least privilege** - Grant minimum permissions required
3. **Use RLS for data security** - Filter data based on user identity
4. **Classify data with labels** - Apply sensitivity labels consistently
5. **Never hardcode secrets** - Use Key Vault or managed identities
6. **Monitor with audit logs** - Track all security-relevant activities
7. **Plan for compliance** - Consider GDPR, data residency requirements
8. **Secure the network** - Use private endpoints for sensitive workloads

## Additional Resources

- [Microsoft Fabric Security Documentation](https://learn.microsoft.com/fabric/security/)
- [Row-Level Security in Fabric](https://learn.microsoft.com/fabric/security/service-admin-row-level-security)
- [Sensitivity Labels in Fabric](https://learn.microsoft.com/fabric/governance/sensitivity-labels)
- [Microsoft Purview Compliance Portal](https://compliance.microsoft.com)
- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
