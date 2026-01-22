# Azure Storage Management Plane Operations - Requirements Document

## Overview

This document outlines the requirements for implementing Azure Storage management plane operations using the `Azure.ResourceManager.Storage` SDK. These operations complement the existing data plane operations by providing comprehensive storage account, container, queue, and table service management capabilities.

### Scope

**Current Implementation (Partial Management + Data Plane):**
- Azure.ResourceManager.Storage - Storage account and container basic operations
- Azure.Storage.Blobs - Blob data operations (upload, list)
- Azure.Data.Tables - Table data operations (list tables)
- Commands: Account get/create, Blob get/upload, Container get/create, Table list

**This Document (Complete Management Plane):**
- Azure.ResourceManager.Storage - Full storage management operations
- Storage account advanced management (keys, SAS, networking, encryption)
- Blob service configuration (properties, CORS, lifecycle management)
- Container management (policies, leases, immutability)
- Queue service and queue management
- Table service and table management
- Management policies (lifecycle, object replication, inventory)
- Tools: `Azure.Mcp.Tools.Storage` - Expand with comprehensive management operations

## SDK Analysis

### Azure.ResourceManager.Storage Package

**Primary Resource Types:**
1. `StorageAccountResource` - Storage account management
2. `BlobServiceResource` - Blob service configuration
3. `BlobContainerResource` - Container management
4. `FileServiceResource` - **File service configuration**
5. `FileShareResource` - **File share management (within storage accounts)**
6. `QueueServiceResource` - Queue service configuration
7. `QueueResource` - Queue management
8. `TableServiceResource` - Table service configuration
9. `TableResource` - Table management
10. `ManagementPolicyResource` - Lifecycle management policies
11. `ObjectReplicationPolicyResource` - Object replication configuration
12. `BlobInventoryPolicyResource` - Inventory policy management
13. `EncryptionScopeResource` - Encryption scope management

**Key Capabilities:**
- Storage account lifecycle (create, update, delete, regenerate keys)
- Access key and SAS token management
- Network security (firewall rules, virtual network rules, private endpoints)
- Encryption configuration (customer-managed keys, encryption scopes)
- Lifecycle management policies (tiering, deletion rules)
- CORS configuration for services
- Immutability policies for containers
- Queue and table resource management
- Diagnostic settings and monitoring configuration
- Failover and redundancy management

## Command Design Patterns

### Command Naming Convention
Follow the established pattern: `azmcp storage <resource> <operation>`

### Resource Hierarchy
```
storage
├── account
│   ├── get                  # Get account details (existing)
│   ├── create               # Create account (existing)
│   ├── update               # Update account properties
│   ├── delete               # Delete account
│   ├── keys
│   │   ├── list             # List access keys
│   │   ├── regenerate       # Regenerate access key
│   ├── sas
│   │   └── generate         # Generate account SAS token
│   ├── network
│   │   ├── rule-add         # Add firewall/vnet rule
│   │   ├── rule-remove      # Remove firewall/vnet rule
│   │   └── rule-list        # List network rules
│   └── failover
│       └── initiate         # Initiate account failover
├── blob
│   ├── service
│   │   ├── properties-get   # Get blob service properties
│   │   ├── properties-set   # Set blob service properties
│   │   ├── cors-set         # Set CORS rules
│   │   └── cors-get         # Get CORS rules
│   └── container
│       ├── get              # Get container (existing)
│       ├── create           # Create container (existing)
│       ├── delete           # Delete container
│       ├── update           # Update container properties
│       ├── list             # List containers
│       ├── lease
│       │   ├── acquire      # Acquire container lease
│       │   ├── renew        # Renew container lease
│       │   ├── release      # Release container lease
│       │   └── break        # Break container lease
│       └── policy
│           ├── set          # Set immutability policy
│           ├── get          # Get immutability policy
│           └── delete       # Delete immutability policy
├── file
│   ├── service
│   │   ├── properties-get   # Get file service properties
│   │   ├── properties-set   # Set file service properties
│   │   ├── cors-set         # Set CORS rules
│   │   └── cors-get         # Get CORS rules
│   └── share
│       ├── create           # Create file share
│       ├── delete           # Delete file share
│       ├── get              # Get file share properties
│       ├── update           # Update file share properties
│       ├── list             # List file shares
│       └── quota
│           └── update       # Update share quota
├── queue
│   ├── service
│   │   ├── properties-get   # Get queue service properties
│   │   └── properties-set   # Set queue service properties
│   └── queue
│       ├── create           # Create queue
│       ├── delete           # Delete queue
│       ├── get              # Get queue properties
│       ├── update           # Update queue metadata
│       └── list             # List queues
├── table
│   ├── service
│   │   ├── properties-get   # Get table service properties
│   │   └── properties-set   # Set table service properties
│   └── table
│       ├── create           # Create table
│       ├── delete           # Delete table
│       ├── get              # Get table properties
│       ├── update           # Update table ACLs
│       └── list             # List tables (existing)
└── management
    ├── policy
    │   ├── create           # Create lifecycle policy
    │   ├── delete           # Delete lifecycle policy
    │   ├── get              # Get lifecycle policy
    │   └── update           # Update lifecycle policy
    ├── replication
    │   ├── create           # Create object replication policy
    │   ├── delete           # Delete object replication policy
    │   ├── get              # Get object replication policy
    │   └── list             # List object replication policies
    └── inventory
        ├── create           # Create blob inventory policy
        ├── delete           # Delete blob inventory policy
        └── get              # Get blob inventory policy
```

## Proposed Commands

### Storage Account Management Commands

#### 1. Update Storage Account

**Command:** `azmcp storage account update`

**Description:**
Update properties of an existing Azure Storage account, including SKU, access tier, network rules, and security settings.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--sku` (optional): SKU name (Standard_LRS, Standard_GRS, etc.)
- `--access-tier` (optional): Access tier (Hot, Cool, Cold, Archive)
- `--enable-https-only` (optional): Require HTTPS (default: true)
- `--min-tls-version` (optional): Minimum TLS version (TLS1_0, TLS1_1, TLS1_2)
- `--allow-blob-public-access` (optional): Allow blob public access
- `--enable-nfs-v3` (optional): Enable NFSv3 support
- `--tags` (optional): Resource tags as JSON
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Change storage account SKU for better redundancy
- Modify access tier for cost optimization
- Update security settings (TLS, HTTPS)
- Add or modify resource tags

**Return Model:**
```csharp
public record StorageAccountUpdateResult
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Location { get; init; }
    public string SkuName { get; init; }
    public string AccessTier { get; init; }
    public bool EnableHttpsOnly { get; init; }
    public string MinimumTlsVersion { get; init; }
    public Dictionary<string, string> Tags { get; init; }
}
```

---

#### 2. Delete Storage Account

**Command:** `azmcp storage account delete`

**Description:**
Delete an Azure Storage account. This is a destructive operation that permanently removes the account and all its data.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--force` (optional): Skip confirmation prompt
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Clean up test storage accounts
- Remove unused storage accounts
- Decommission old storage infrastructure

**Return Model:**
```csharp
public record StorageAccountDeleteResult
{
    public string AccountName { get; init; }
    public bool Deleted { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
```

---

#### 3. List Storage Account Keys

**Command:** `azmcp storage account keys list`

**Description:**
List access keys for a storage account. Returns both key1 and key2 with their permissions.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Retrieve connection strings for applications
- Audit key rotation status
- Verify key permissions

**Return Model:**
```csharp
public record StorageAccountKeysResult
{
    public List<StorageAccountKey> Keys { get; init; }
}

public record StorageAccountKey
{
    public string KeyName { get; init; }
    public string Value { get; init; }
    public string Permissions { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
}
```

---

#### 4. Regenerate Storage Account Key

**Command:** `azmcp storage account keys regenerate`

**Description:**
Regenerate an access key for a storage account. This invalidates the old key and generates a new one.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--key-name` (required): Key to regenerate (key1, key2)
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Rotate keys for security compliance
- Revoke compromised keys
- Regular key rotation policies

**Return Model:**
```csharp
public record StorageAccountKeyRegenerateResult
{
    public string KeyName { get; init; }
    public string NewValue { get; init; }
    public DateTimeOffset RegeneratedAt { get; init; }
}
```

---

#### 5. Generate Account SAS Token

**Command:** `azmcp storage account sas generate`

**Description:**
Generate a Shared Access Signature (SAS) token for a storage account with specified permissions and expiration.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--services` (required): Services to grant access (b=blob, f=file, q=queue, t=table)
- `--resource-types` (required): Resource types (s=service, c=container, o=object)
- `--permissions` (required): Permissions (r=read, w=write, d=delete, l=list, etc.)
- `--expiry` (required): Expiration time (ISO 8601 format or duration like "1h", "7d")
- `--start` (optional): Start time (default: now)
- `--ip-range` (optional): IP address or range (e.g., "203.0.113.0/24")
- `--protocol` (optional): Protocol (https, https,http)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Grant temporary access to storage resources
- Share data with external partners securely
- Enable application access without sharing keys

**Return Model:**
```csharp
public record SasTokenResult
{
    public string SasToken { get; init; }
    public string FullUri { get; init; }
    public DateTimeOffset StartsOn { get; init; }
    public DateTimeOffset ExpiresOn { get; init; }
    public string Services { get; init; }
    public string ResourceTypes { get; init; }
    public string Permissions { get; init; }
}
```

---

#### 6. Add Network Rule

**Command:** `azmcp storage account network rule-add`

**Description:**
Add a firewall rule or virtual network rule to restrict storage account access.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--ip-address` (optional): IP address or CIDR range
- `--vnet-resource-id` (optional): Virtual network subnet resource ID
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Restrict access to specific IP addresses
- Configure virtual network integration
- Implement network security policies

**Return Model:**
```csharp
public record NetworkRuleResult
{
    public List<string> IpRules { get; init; }
    public List<string> VirtualNetworkRules { get; init; }
    public string DefaultAction { get; init; } // Allow or Deny
}
```

---

#### 7. Remove Network Rule

**Command:** `azmcp storage account network rule-remove`

**Description:**
Remove a firewall rule or virtual network rule from a storage account.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--ip-address` (optional): IP address or CIDR range to remove
- `--vnet-resource-id` (optional): Virtual network subnet resource ID to remove
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record NetworkRuleRemoveResult
{
    public string RemovedRule { get; init; }
    public bool Success { get; init; }
}
```

---

#### 8. Initiate Account Failover

**Command:** `azmcp storage account failover initiate`

**Description:**
Initiate a manual failover for a geo-redundant storage account to the secondary region.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--force` (optional): Skip confirmation (default: false)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Test disaster recovery procedures
- Respond to regional outages
- Planned maintenance failovers

**Return Model:**
```csharp
public record FailoverResult
{
    public string AccountName { get; init; }
    public string PrimaryRegion { get; init; }
    public string SecondaryRegion { get; init; }
    public DateTimeOffset FailoverInitiatedAt { get; init; }
    public string Status { get; init; }
}
```

---

### Blob Service Management Commands

#### 9. Get Blob Service Properties

**Command:** `azmcp storage blob service properties-get`

**Description:**
Retrieve blob service properties including versioning, soft delete, change feed, and default service version.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record BlobServicePropertiesResult
{
    public bool? IsVersioningEnabled { get; init; }
    public int? DeleteRetentionDays { get; init; }
    public bool? DeleteRetentionEnabled { get; init; }
    public int? ContainerDeleteRetentionDays { get; init; }
    public bool? ContainerDeleteRetentionEnabled { get; init; }
    public bool? ChangeFeedEnabled { get; init; }
    public int? ChangeFeedRetentionDays { get; init; }
    public string DefaultServiceVersion { get; init; }
    public bool? RestoreEnabled { get; init; }
    public int? RestoreDays { get; init; }
}
```

---

#### 10. Set Blob Service Properties

**Command:** `azmcp storage blob service properties-set`

**Description:**
Configure blob service properties including versioning, soft delete, and change feed settings.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--enable-versioning` (optional): Enable blob versioning
- `--enable-soft-delete` (optional): Enable soft delete for blobs
- `--soft-delete-retention-days` (optional): Retention days (1-365)
- `--enable-container-soft-delete` (optional): Enable container soft delete
- `--container-delete-retention-days` (optional): Container retention days
- `--enable-change-feed` (optional): Enable change feed
- `--change-feed-retention-days` (optional): Change feed retention days
- `--enable-restore` (optional): Enable point-in-time restore
- `--restore-days` (optional): Restore days (1-365)
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record BlobServicePropertiesUpdateResult
{
    public bool Success { get; init; }
    public Dictionary<string, object> UpdatedProperties { get; init; }
}
```

---

#### 11. Set CORS Rules

**Command:** `azmcp storage blob service cors-set`

**Description:**
Configure Cross-Origin Resource Sharing (CORS) rules for blob service.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--rules` (required): CORS rules as JSON array
- `--retry-policy` (optional): Retry policy options

**Example CORS Rules JSON:**
```json
[
  {
    "allowedOrigins": ["https://example.com"],
    "allowedMethods": ["GET", "POST"],
    "allowedHeaders": ["*"],
    "exposedHeaders": ["x-ms-*"],
    "maxAgeInSeconds": 3600
  }
]File Service Management Commands

#### 12. Get File Service Properties

**Command:** `azmcp storage file service properties-get`

**Description:**
Retrieve file service properties including CORS rules, protocol settings, and SMB settings for the storage account.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Audit file service configuration
- Verify CORS settings
- Check SMB protocol settings

**Return Model:**
```csharp
public record FileServicePropertiesResult
{
    public List<CorsRule> CorsRules { get; init; }
    public ProtocolSettings ProtocolSettings { get; init; }
    public string ShareDeleteRetentionPolicy { get; init; }
    public int? ShareDeleteRetentionDays { get; init; }
}

public record CorsRule
{
    public List<string> AllowedOrigins { get; init; }
    public List<string> AllowedMethods { get; init; }
    public List<string> AllowedHeaders { get; init; }
    public List<string> ExposedHeaders { get; init; }
    public int MaxAgeInSeconds { get; init; }
}

public record ProtocolSettings
{
    public SmbSettings Smb { get; init; }
}21

public record SmbSettings
{
    public string Versions { get; init; }
    public string AuthenticationMethods { get; init; }
    public string KerberosTicketEncryption { get; init; }
    public string ChannelEncryption { get; init; }
}
```

---

#### 13. Set File Service Properties

**Command:** `azmcp storage file service properties-set`

**Description:**
Configure file service properties including CORS rules, SMB settings, and soft delete for file shares.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--enable-smb-multichannel` (optional): Enable SMB Multichannel
- `--smb-versions` (optional): Allowed SMB versions (e.g., "SMB2.1;SMB3.0;SMB3.1.1")
- `--smb-authentication-methods` (optional): SMB authentication methods
- `--enable-share-delete-retention` (optional): Enable soft delete for shares
- `--share-delete-retention-days` (optional): Retention days (1-365)
- `--22try-policy` (optional): Retry policy options

**Use Cases:**
- Configure SMB security settings
- Enable share soft delete protection
- Update protocol settings

**Return Model:**
```csharp
public record FileServicePropertiesUpdateResult
{
    public bool Success { get; init; }
    public Dictionary<string, object> UpdatedProperties { get; init; }
}
```

---

#### 14. Set File Service CORS Rules

**Command:** `azmcp storage file service cors-set`

**Description:**
Configure Cross-Origin Resource Sharing (CORS) rules for file service to enable browser-based applications to access files.

**Parameters:**
- `--23count` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--rules` (required): CORS rules as JSON array
- `--retry-policy` (optional): Retry policy options

**Example CORS Rules JSON:**
```json
[
  {
    "allowedOrigins": ["https://example.com"],
    "allowedMethods": ["GET", "HEAD"],
    "allowedHeaders": ["*"],
    "exposedHeaders": ["x-ms-*"],
    "maxAgeInSeconds": 3600
  }
]
```

**Return Model:**
```csharp
public record FileServiceCorsRulesResult
{
    public int RuleCount { get; init; }
    public bool Success { get; init; }
}
```

---24

### File Share Management Commands

#### 15. Create File Share

**Command:** `azmcp storage file share create`

**Description:**
Create a new file share within a storage account for SMB or NFS file access.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--quota` (optional): Maximum share size in GB (5120 default, up to 102400 for premium)
- `--access-tier` (optional): Access tier (TransactionOptimized, Hot, Cool, Premium)
- `--enabled-protocols` (optional): Enabled protocols (SMB, NFS)
- `--root-squash` (optional): Root squash mode for NFS (NoRootSquash, RootSquash, AllSquash)
- `--metadata` (optional): Share metadata as JSON
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Create file shares for application data
- Set up SMB shares for Windows/Linux access
- Configure NFS shares for Linux workloads
- Provision shares with specific quotas

**Return Model:**
```csharp
publi25record FileShareCreateResult
{
    public string Name { get; init; }
    public string ResourceId { get; init; }
    public int QuotaGB { get; init; }
    public string AccessTier { get; init; }
    public string EnabledProtocols { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
```

---

#### 16. Delete File Share

**Command:** `azmcp storage file share delete`

**Description:**
Delete a file share from a storage account. Idempotent operation that succeeds even if the share doesn't exist.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Cle26 up unused file shares
- Remove test file shares
- Decommission old storage

**Return Model:**
```csharp
public record FileShareDeleteResult
{
    public string ShareName { get; init; }
    public bool Deleted { get; init; }
}
```

---

#### 17. Get File Share Properties

**Command:** `azmcp storage file share get`

**Description:**
Retrieve properties and metadata of a specific file share including quota, usage, access tier, and protocol settings.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--27source-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--expand` (optional): Expand stats (include usage data)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Check share quota and usage
- Verify access tier configuration
- Audit share settings
- Monitor share capacity

**Return Model:**
```csharp
public record FileSharePropertiesResult
{
    public string Name { get; init; }
    public string ResourceId { get; init; }
    public int QuotaGB { get; init; }
    public int? UsageGB { get; init; }
    public string AccessTier { get; init; }
    public string EnabledProtocols { get; init; }
    public string RootSquash { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string ETag { get; init; }
}
```
8
---

#### 18. Update File Share

**Command:** `azmcp storage file share update`

**Description:**
Update file share properties including quota, access tier, and metadata.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--quota` (optional): Update share quota in GB
- `--access-tier` (optional): Update access tier (TransactionOptimized, Hot, Cool)
- `--metadata` (optional): Update metadata as JSON
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Increase share quota for growing data
- Change access tier for cost optimization
- Update share metadata

**Return Model:**
```csharp
public record FileShareUpdateResult
{
    public string Name { get; init; }
    public int? QuotaGB { get; init; }
    public string AccessTier { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public DateTimeOffset LastModified { get; init; }
}9
```

---

#### 19. List File Shares

**Command:** `azmcp storage file share list`

**Description:**
List all file shares in a storage account with optional filtering and usage statistics.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--prefix` (optional): Filter by share name prefix
- `--expand` (optional): Include usage statistics (snapshots, deleted)
- `--max-results` (optional): Maximum number of results
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Discover file shares in storage account
- Audit share configurations
- Monitor share usage
- Generate inventory reports

**Return Model:**
```csharp
public record FileShareListResult
{
    public List<FileShareInfo> Shares { get; init; }
}

public record FileShareInfo
{
    public string Name { get; init; }
    public string ResourceId { get; init; }
    public int QuotaGB { get; init; }
    public int? UsageGB { get; init; }
    public string AccessTier { get; init; }
    public string EnabledProtocols { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
```

---

### Container Management Commands

#### 20
**Return Model:**
```csharp
public record CorsRulesResult
{
    public int RuleCount { get; init; }
    public bool Success { get; init; }
}
```

---

### Container Management Commands

#### 12. Delete Container

**Command:** `azmcp storage blob container delete`

**Description:**
Delete a blob container from a storage account. Idempotent operation.

**Parameters:**
- `--account` (required): Storage account name
- `--container` (required): Container name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ContainerDeleteResult
{
    public string ContainerName { get; init; }
    public bool Deleted { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
```

---

#### 13. Update Container

**Command:** `azmcp storage blob container update`

**Description:**
Update container properties including metadata and public access level.

**Parameters:**
- `--account` (required): Storage account name
- `--container` (required): Container name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--public-access` (optional): Public access level (None, Blob, Container)
- `--metadata` (optional): Container metadata as JSON
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ContainerUpdateResult
{
    public string ContainerName { get; init; }
    public string PublicAccess { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
```

---

#### 14. List Containers

**Command:** `azmcp storage blob container list`

**Description:**
List all blob containers in a storage account with filtering options.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--prefix` (optional): Filter by container name prefix
- `--include-deleted` (optional): Include soft-deleted containers
- `--max-results` (optional): Maximum number of results
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ContainerListResult
{
    public List<ContainerInfo> Containers { get; init; }
}
```

---

#### 15. Acquire Container Lease

**Command:** `azmcp storage blob container lease acquire`

**Description:**
Acquire a lease on a container to lock it for exclusive access.

**Parameters:**
- `--account` (required): Storage account name
- `--container` (required): Container name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--lease-duration` (optional): Lease duration in seconds (15-60, or -1 for infinite)
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ContainerLeaseResult
{
    public string LeaseId { get; init; }
    public string ContainerName { get; init; }
    public int? LeaseDuration { get; init; }
    public DateTimeOffset AcquiredAt { get; init; }
}
```

---

#### 16. Set Immutability Policy

**Command:** `azmcp storage blob container policy set`

**Description:**
Set time-based retention policy or legal hold for immutable blob storage (WORM - Write Once, Read Many).

**Parameters:**
- `--account` (required): Storage account name
- `--container` (required): Container name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--immutability-period-days` (optional): Retention period in days
- `--legal-hold` (optional): Enable legal hold
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ImmutabilityPolicyResult
{
    public string ContainerName { get; init; }
    public int? ImmutabilityPeriodDays { get; init; }
    public bool HasLegalHold { get; init; }
    public string State { get; init; } // Locked, Unlocked
}
```

---

### Queue Management Commands

#### 17. Create Queue

**Command:** `azmcp storage queue create`

**Description:**
Create a new queue in a storage account for message-based communication.

**Parameters:**
- `--account` (required): Storage account name
- `--queue` (required): Queue name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--metadata` (optional): Queue metadata as JSON
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record QueueCreateResult
{
    public string QueueName { get; init; }
    public string ResourceId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
}
```

---

#### 18. Delete Queue

**Command:** `azmcp storage queue delete`

**Description:**
Delete a queue from a storage account. Idempotent operation.

**Parameters:**
- `--account` (required): Storage account name
- `--queue` (required): Queue name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record QueueDeleteResult
{
    public string QueueName { get; init; }
    public bool Deleted { get; init; }
}
```

---

#### 19. Get Queue Properties

**Command:** `azmcp storage queue get`

**Description:**
Retrieve properties and metadata of a specific queue.

**Parameters:**
- `--account` (required): Storage account name
- `--queue` (required): Queue name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record QueuePropertiesResult
{
    public string QueueName { get; init; }
    public string ResourceId { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public int? ApproximateMessageCount { get; init; }
}
```

---

#### 20. List Queues

**Command:** `azmcp storage queue list`

**Description:**
List all queues in a storage account.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--prefix` (optional): Filter by queue name prefix
- `--max-results` (optional): Maximum number of results
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record QueueListResult
{
    public List<QueueInfo> Queues { get; init; }
}

public record QueueInfo
{
    public string Name { get; init; }
    public string ResourceId { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
}
```

---

### Table Management Commands

#### 21. Create Table

**Command:** `azmcp storage table create`

**Description:**
Create a new table in a storage account for NoSQL data storage.

**Parameters:**
- `--account` (required): Storage account name
- `--table` (required): Table name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record TableCreateResult
{
    public string TableName { get; init; }
    public string ResourceId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
```

---

#### 30. Delete Table

**Command:** `azmcp storage table delete`

**Description:**
Delete a table from a storage account. Idempotent operation.

**Parameters:**
- `--account` (required): Storage account name
- `--table` (required): Table name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record TableDeleteResult
{
    public string TableName { get; init; }
    public bool Deleted { get; init; }
}
```

---

### Management Policy Commands

#### 31. Create Lifecycle Management Policy

**Command:** `azmcp storage management policy create`

**Description:**
Create a lifecycle management policy to automatically tier or delete blobs based on rules.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--policy` (required): Policy definition as JSON file path or inline JSON
- `--retry-policy` (optional): Retry policy options

**Example Policy JSON:**
```json
{
  "rules": [
    {
      "name": "MoveToCool",
      "type": "Lifecycle",
      "definition": {
        "filters": {
          "blobTypes": ["blockBlob"],
          "prefixMatch": ["logs/"]
        },
        "actions": {
          "baseBlob": {
            "tierToCool": {
              "daysAfterModificationGreaterThan": 30
            },
            "tierToArchive": {
              "daysAfterModificationGreaterThan": 90
            },
            "delete": {
              "daysAfterModificationGreaterThan": 365
            }
          }
        }
      }
    }
  ]
}
```

**Return Model:**
```csharp
public record ManagementPolicyResult
{
    public string PolicyId { get; init; }
    public int RuleCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<string> RuleNames { get; init; }
}
```

---

#### 32. Get Lifecycle Management Policy

**Command:** `azmcp storage management policy get`

**Description:**
Retrieve the current lifecycle management policy for a storage account.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ManagementPolicyGetResult
{
    public string PolicyId { get; init; }
    public string Policy { get; init; } // JSON representation
    public DateTimeOffset LastModified { get; init; }
    public List<PolicyRuleSummary> Rules { get; init; }
}

public record PolicyRuleSummary
{
    public string Name { get; init; }
    public string Type { get; init; }
    public List<string> Actions { get; init; }
}
```

---

#### 33. Delete Lifecycle Management Policy

**Command:** `azmcp storage management policy delete`

**Description:**
Delete the lifecycle management policy from a storage account.

**Parameters:**
- `--account` (required): Storage account name
- `--resource-group` (required): Resource group name
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Return Model:**
```csharp
public record ManagementPolicyDeleteResult
{
    public bool Deleted { get; init; }
    public DateTimeOffset DeletedAt { get; init; }
}
```

---

## Authentication and Authorization

### Authentication Strategy
Use the existing `IAzureTokenCredentialProvider` pattern from Azure.Mcp.Core with ARM client:

```csharp
var credential = await GetCredentialAsync(tenant, cancellationToken);
var armClient = new ArmClient(credential);
var subscription = armClient.GetSubscriptionResource(subscriptionId);
```

### Required Permissions
- **Reader Role**: List resources, get properties
- **Contributor Role**: Create, update, delete resources
- **Storage Account Key Operator Role**: Manage access keys
- **Storage Account Contributor**: Full storage account management

## Service Implementation Pattern

### Extend Existing StorageService
Add new methods to `IStorageService` and `StorageService`:

```csharp
public interface IStorageService
{
    // Existing methods...
    
    // Account management
    Task<StorageAccountUpdateResult> UpdateStorageAccountAsync(...);
    Task<StorageAccountDeleteResult> DeleteStorageAccountAsync(...);
    Task<StorageAccountKeysResult> ListStorageAccountKeysAsync(...);
    Task<StorageAccountKeyRegenerateResult> RegenerateStorageAccountKeyAsync(...);
    Task<SasTokenResult> GenerateAccountSasTokenAsync(...);
    
    // Network management
    Task<NetworkRuleResult> AddNetworkRuleAsync(...);
    Task<NetworkRuleRemoveResult> RemoveNetworkRuleAsync(...);
    Task<NetworkRuleResult> ListNetworkRulesAsync(...);
    
    // Failover
    Task<FailoverResult> InitiateAccountFailoverAsync(...);
    
    // Blob service
    Task<BlobServicePropertiesResult> GetBlobServicePropertiesAsync(...);
    Task<BlobServicePropertiesUpdateResult> SetBlobServicePropertiesAsync(...);
    Task<CorsRulesResult> SetBlobServiceCorsRulesAsync(...);
    
    // File service
    Task<FileServicePropertiesResult> GetFileServicePropertiesAsync(...);
    Task<FileServicePropertiesUpdateResult> SetFileServicePropertiesAsync(...);
    Task<FileServiceCorsRulesResult> SetFileServiceCorsRulesAsync(...);
    
    // File share management
    Task<FileShareCreateResult> CreateFileShareAsync(...);
    Task<FileShareDeleteResult> DeleteFileShareAsync(...);
    Task<FileSharePropertiesResult> GetFileSharePropertiesAsync(...);
    Task<FileShareUpdateResult> UpdateFileShareAsync(...);
    Task<FileShareListResult> ListFileSharesAsync(...);
    
    // Container management
    Task<ContainerDeleteResult> DeleteContainerAsync(...);
    Task<ContainerUpdateResult> UpdateContainerAsync(...);
    Task<ContainerListResult> ListContainersAsync(...);
    Task<ContainerLeaseResult> AcquireContainerLeaseAsync(...);
    Task<ImmutabilityPolicyResult> SetContainerImmutabilityPolicyAsync(...);
    
    // Queue management
    Task<QueueCreateResult> CreateQueueAsync(...);
    Task<QueueDeleteResult> DeleteQueueAsync(...);
    Task<QueuePropertiesResult> GetQueuePropertiesAsync(...);
    Task<QueueListResult> ListQueuesAsync(...);
    
    // Table management
    Task<TableCreateResult> CreateTableAsync(...);
    Task<TableDeleteResult> DeleteTableAsync(...);
    
    // Lifecycle management
    Task<ManagementPolicyResult> CreateManagementPolicyAsync(...);
    Task<ManagementPolicyGetResult> GetManagementPolicyAsync(...);
    Task<ManagementPolicyDeleteResult> DeleteManagementPolicyAsync(...);
}
```

### Service Implementation Example
```csharp
public async Task<StorageAccountKeysResult> ListStorageAccountKeysAsync(
    string account,
    string resourceGroup,
    string subscription,
    string? tenant = null,
    RetryPolicyOptions? retryPolicy = null,
    CancellationToken cancellationToken = default)
{
    ValidateRequiredParameters(
        (nameof(account), account),
        (nameof(resourceGroup), resourceGroup),
        (nameof(subscription), subscription));

    try
    {
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(subscription));
        
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(
            resourceGroup, cancellationToken);
        
        var storageAccount = await resourceGroupResource.Value
            .GetStorageAccountAsync(account, cancellationToken);
        
        var keys = await storageAccount.Value.GetKeysAsync(cancellationToken: cancellationToken);
        
        var keyList = new List<StorageAccountKey>();
        foreach (var key in keys.Value)
        {
            keyList.Add(new StorageAccountKey
            {
                KeyName = key.KeyName,
                Value = key.Value,
                Permissions = key.Permissions.ToString(),
                CreatedOn = key.CreationTime
            });
        }
        
        return new StorageAccountKeysResult { Keys = keyList };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error listing storage account keys. Account: {Account}", account);
        throw;
    }
}
```

## Command Implementation Pattern

### Example Command: List Storage Account Keys
```csharp
public sealed class AccountKeysListCommand(
    ILogger<AccountKeysListCommand> logger,
    IStorageService storageService)
    : BaseStorageCommand<AccountKeysListOptions>(logger, storageService)
{
    public override string Id => "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
    public override string Name => "list";
    public override string Description =>
        "List access keys for an Azure Storage account. Returns both primary and secondary keys with their permissions.";
    public override string Title => "List Storage Account Keys";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = true  // Returns sensitive key values
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Account.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _storageService.ListStorageAccountKeysAsync(
                options.Account!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                StorageJsonContext.Default.StorageAccountKeysResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list storage account keys. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
```

## Option Definitions

### New Option Definitions for Storage
```csharp
public static class StorageOptionDefinitions
{
    // Existing options...
    public static readonly OptionDefinition Account = new() { ... };
    public static readonly OptionDefinition Container = new() { ... };
    
    // New options
    public static readonly OptionDefinition Queue = new()
    {
        Name = "queue",
        Description = "Queue name",
        ValueName = "queue-name",
        Required = true
    };
    
    public static readonly OptionDefinition Table = new()
    {
        Name = "table",
        Description = "Table name",
        ValueName = "table-name",
        Required = true
    };
    
    public static readonly OptionDefinition KeyName = new()
    {
        Name = "key-name",
        Description = "Key name to regenerate (key1, key2)",
        ValueName = "key-name",
        Required = true
    };
    
    public static readonly OptionDefinition Services = new()
    {
        Name = "services",
        Description = "Services for SAS token (b=blob, f=file, q=queue, t=table)",
        ValueName = "services",
        Required = true
    };
    
    public static readonly OptionDefinition Permissions = new()
    {
        Name = "permissions",
        Description = "Permissions for SAS token (r=read, w=write, d=delete, etc.)",
        ValueName = "permissions",
        Required = true
    };
    
    public static readonly OptionDefinition Expiry = new()
    {
        Name = "expiry",
        Description = "Expiration time (ISO 8601 or duration like '1h', '7d')",
        ValueName = "expiry",
        Required = true
    };
    
    public static readonly OptionDefinition IpAddress = new()
    {
        Name = "ip-address",
        Description = "IP address or CIDR range",
        ValueName = "ip-address",
        Required = false
    };
    
    public static readonly OptionDefinition PublicAccess = new()
    {
        Name = "public-access",
        Description = "Public access level (None, Blob, Container)",
        ValueName = "access-level",
        Required = false
    };
    
    public static readonly OptionDefinition LeaseDuration = new()
    {
        Name = "lease-duration",
        Description = "Lease duration in seconds (15-60, or -1 for infinite)",
        ValueName = "seconds",
        Required = false
    };
    
    public static readonly OptionDefinition PolicyFile = new()
    {
        Name = "policy",
        Description = "Policy definition as JSON file path or inline JSON",
        ValueName = "policy-json",
        Required = true
    };
}
```

## Error Handling

### Standard Error Scenarios
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException reqEx when reqEx.Status == 404 =>
        "Storage resource not found. Verify the account, container, queue, or table name.",
    RequestFailedException reqEx when reqEx.Status == 403 =>
        "Access denied. Verify you have appropriate permissions (Storage Account Contributor).",
    RequestFailedException reqEx when reqEx.Status == 409 =>
        "Resource already exists or operation conflicts with current state.",
    RequestFailedException reqEx when reqEx.ErrorCode == "AccountBeingCreated" =>
        "Storage account is being created. Wait a few moments and try again.",
    RequestFailedException reqEx when reqEx.ErrorCode == "StorageAccountNotFound" =>
        "Storage account not found in the specified resource group and subscription.",
    ArgumentException argEx =>
        $"Invalid parameter value: {argEx.Message}",
    _ => base.GetErrorMessage(ex)
};

protected override int GetStatusCode(Exception ex) => ex switch
{
    RequestFailedException reqEx => reqEx.Status,
    ArgumentException => 400,
    _ => base.GetStatusCode(ex)
};
```

## Testing Requirements

### Unit Tests
Each command requires comprehensive unit tests following the patterns in AGENTS.md.

### Live Test Infrastructure
Enhance existing `test-resources.bicep`:
```bicep
// Add queue resource
resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/testqueue'
  properties: {}
}

// Add table resource
resource table 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  name: '${storageAccount.name}/default/testtable'
  properties: {}
}

// Add lifecycle management policy
resource managementPolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-01-01' = {
  name: '${storageAccount.name}/default'
  properties: {
    policy: {
      rules: [
        {
          name: 'testRule'
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: ['blockBlob']
              prefixMatch: ['test/']
            }
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 30
                }
              }
            }
          }
        }
      ]
    }
  }
}
```file_share_create | Create a file share named myshare in storage account mystorageaccount |
| storage_file_share_delete | Delete file share myshare from storage account mystorageaccount |
| storage_file_share_list | List all file shares in storage account mystorageaccount |
| storage_file_share_update | Update quota for file share myshare to 1024 GB |
| storage_file_service_properties_get | Show file service properties for storage account mystorageaccount |
| storage_

### E2E Test Prompts
Add to `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`:
```markdown
## Azure Storage (Management Plane)

| Tool Name | Test Prompt |
|:----------|:----------|
| storage_account_keys_list | Show me the access keys for my storage account |
| storage_account_keys_regenerate | Regenerate key1 for storage account mystorageaccount |
| storage_account_sas_generate | Generate a SAS token for blob read access valid for 1 hour |
| storage_queue_create | Create a queue named myqueue in storage account mystorageaccount |
| storage_queue_list | List all queues in storage account mystorageaccount |
| storage_table_create | Create a table named mytable in storage account mystorageaccount |
| storage_management_policy_create | Create a lifecycle policy to tier old blobs to cool storage |
```

## JSON Serialization Context

All response models must be registered for AOT compatibility:
```csharpFileServicePropertiesResult))]
[JsonSerializable(typeof(FileServicePropertiesUpdateResult))]
[JsonSerializable(typeof(FileServiceCorsRulesResult))]
[JsonSerializable(typeof(FileShareCreateResult))]
[JsonSerializable(typeof(FileShareDeleteResult))]
[JsonSerializable(typeof(FileSharePropertiesResult))]
[JsonSerializable(typeof(FileShareUpdateResult))]
[JsonSerializable(typeof(FileShareListResult))]
[JsonSerializable(typeof(FileShareInfo))]
[JsonSerializable(typeof(
[JsonSerializable(typeof(StorageAccountUpdateResult))]
[JsonSerializable(typeof(StorageAccountDeleteResult))]
[JsonSerializable(typeof(StorageAccountKeysResult))]
[JsonSerializable(typeof(StorageAccountKeyRegenerateResult))]
[JsonSerializable(typeof(SasTokenResult))]
[JsonSerializable(typeof(NetworkRuleResult))]
[JsonSerializable(typeof(NetworkRuleRemoveResult))]
[JsonSerializable(typeof(FailoverResult))]
[JsonSerializable(typeof(BlobServicePropertiesResult))]
[JsonSerializable(typeof(BlobServicePropertiesUpdateResult))]
[JsonSerializable(typeof(CorsRulesResult))]
[JsonSerializable(typeof(ContainerDeleteResult))]
[JsonSerializable(typeof(ContainerUpdateResult))]
[JsonSerializable(typeof(ContainerListResult))]
[JsonSerializable(typeof(ContainerLeaseResult))]
[JsonSerializable(typeof(ImmutabilityPolicyResult))]
[JsonSerializable(typeof(QueueCreateResult))]
[JsonSerializable(typeof(QueueDeleteResult))]
[JsonSerializable(typeof(QueuePropertiesResult))]
[JsonSerializable(typeof(QueueListResult))]
[JsonSerializable(typeof(TableCreateResult))]
[JsonSerializable(typeof(TableDeleteResult))]
[JsonSerializable(typeof(ManagementPolicyResult))]
[JsonSerializable(typeof(ManagementPolicyGetResult))]
[JsonSerializable(typeof(ManagementPolicyDeleteResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class StorageJsonContext : JsonSerializerContext;
```

## Project Configuration

Azure.ResourceManager.Storage is already referenced in the project. No additional package installation needed.

## Documentation Requirements

### Update Command Reference
Add new commands to `servers/Azure.Mcp.Server/docs/azmcp-commands.md` following the established format.

### Update README
Update `tools/Azure.Mcp.Tools.Storage/README.md` with comprehensive management capabilities.

### Create Changelog Entry
Use `docs/changelog-entries.md` pattern with `-ChangelogPath servers/Azure.Mcp.Server/CHANGELOG.md`.

## Implementation Priorities

### Phase 1: Account Management (High Priority)
1. List Storage Account Keys
2. Regenerate Storage Account Key
3. Generate Account SAS Token
4. Update Storage Account
5. Delete StoFile Service Configuration (Medium Priority)
12. Get File Service Properties
13. Set File Service Properties
14. Set File Service CORS Rules

### Phase 5: File Share Management (High Priority)
15. Create File Share
16. Delete File Share
17. Get File Share Properties
18. Update File Share
19. List File Shares

### Phase 6: Container Management (Medium Priority)
20. Delete Container
21. Update Container
22. List Containers
23. Container Lease Operations

### Phase 7: Queue Management (Medium Priority)
24. Immutability Policy
25. Create Queue
26. Delete10: Advanced Features (Future)
33. Object Replication Policies
34. Blob Inventory Policies
35. Encryption Scopes
36# Phase 8: Table Management (Lower Priority)
29. Create Table
30. Delete Table

### Phase 9: Advanced Management (Lower Priority)
31. Lifecycle Management Policy
32. Create Queue
17. Delete Queue
18. Get Queue Properties
19. List Queues

### Phase 6: Table Management (Lower Priority)
20. Create Table
21. Delete Table

### Phase 7: Advanced Management (Lower Priority)
22. Lifecycle Management Policy
23. Immutability Policy
24. Account Failover

### Phase 8: Advanced Features (Future)
25. Object Replication Policies
26. Blob Inventory Policies
27. Encryption Scopes
28. Private Endpoint Management

## Success Criteria

- [ ] All commands follow established patterns in AGENTS.md
- [ ] Commands use primary constructors and static OptionDefinitions
- [ ] Service extends existing StorageService pattern
- [ ] All response models registered in JSON context (AOT-safe)
- [ ] Comprehensive unit tests with >80% coverage
- [ ] Enhanced live test infrastructure with Bicep templates
- [ ] Tool description validation (top 3 ranking, ≥0.4 confidence)
- [ ] Documentation updated (azmcp-commands.md, e2eTestPrompts.md)
- [ ] Spelling check passes
- [ ] One tool per PR for faster review cycles
- [ ] Changelog entries created for user-facing changes

## References

- **Azure.ResourceManager.Storage SDK**: https://learn.microsoft.com/dotnet/api/azure.resourcemanager.storage
- **Storage Management REST API**: https://learn.microsoft.com/rest/api/storagerp/
- **Existing patterns**: `tools/Azure.Mcp.Tools.Storage/src/Commands/Account/AccountGetCommand.cs`
- **Architecture guide**: `AGENTS.md`
- **Implementation guide**: `servers/Azure.Mcp.Server/docs/new-command.md`
