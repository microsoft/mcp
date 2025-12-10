# Azure Storage Sync Tools

Azure File Sync enables you to centralize your file shares in Azure Files while keeping the flexibility, performance, and compatibility of a Windows file server. Transform your on-premises file servers into hybrid endpoints that sync with Azure file shares, with support for cloud tiering to archive less frequently used files to the cloud.

## Overview

This toolset provides comprehensive operations for managing Azure File Sync resources including:
- **StorageSyncServices**: Top-level resources that serve as management containers
- **SyncGroups**: Groups that define sync topology between cloud and server endpoints
- **CloudEndpoints**: References to Azure file shares participating in sync
- **ServerEndpoints**: Local server paths that participate in sync
- **RegisteredServers**: Servers registered to a storage sync service
- **PrivateEndpointConnections**: Private network connectivity for storage sync services

A storage sync service is the top-level resource for Azure File Sync and a single server can only be registered to one storage sync service. Plan to create as few storage sync services as necessary to differentiate distinct groups of servers in your organization.

## PowerShell Cmdlets

The following table lists all available PowerShell cmdlets for Azure Storage Sync management:

### Storage Sync Service Management

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `New-AzStorageSyncService` | Creates a new storage sync service in a resource group | `-ResourceGroupName`, `-Name`, `-Location`, `-IncomingTrafficPolicy`, `-AssignIdentity`, `-Tag` |
| `Get-AzStorageSyncService` | Lists all storage sync services within a subscription or resource group | `-ResourceGroupName`, `-Name` |
| `Set-AzStorageSyncService` | Updates properties of an existing storage sync service | `-ResourceGroupName`, `-Name`, `-IncomingTrafficPolicy`, `-Tag` |
| `Set-AzStorageSyncServiceIdentity` | Manages the identity of a storage sync service | `-ResourceGroupName`, `-Name`, `-IdentityType`, `-UserAssignedIdentityId` |
| `Remove-AzStorageSyncService` | Deletes a storage sync service | `-ResourceGroupName`, `-Name` |

### Sync Group Management

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `New-AzStorageSyncGroup` | Creates a new sync group within a storage sync service | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |
| `Get-AzStorageSyncGroup` | Lists all sync groups within a storage sync service | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |
| `Remove-AzStorageSyncGroup` | Deletes a sync group | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |

### Cloud Endpoint Management

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `New-AzStorageSyncCloudEndpoint` | Creates an Azure File Sync cloud endpoint in a sync group | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name`, `-StorageAccountResourceId`, `-AzureFileShareName`, `-StorageAccountTenantId` |
| `Get-AzStorageSyncCloudEndpoint` | Lists all cloud endpoints within a sync group | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |
| `Set-AzStorageSyncCloudEndpointPermission` | Manages permissions for a cloud endpoint | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |
| `Remove-AzStorageSyncCloudEndpoint` | Deletes a cloud endpoint from a sync group | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |
| `Invoke-AzStorageSyncChangeDetection` | Manually triggers change detection on cloud endpoints (share-level, directory, or file-level) | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name`, `-DirectoryPath`, `-Path`, `-Recursive` |

### Server Endpoint Management

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `New-AzStorageSyncServerEndpoint` | Creates a new server endpoint on a registered server | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name`, `-ServerResourceId`, `-ServerLocalPath`, `-CloudTiering`, `-VolumeFreeSpacePercent`, `-TierFilesOlderThanDays`, `-InitialDownloadPolicy`, `-InitialUploadPolicy` |
| `Get-AzStorageSyncServerEndpoint` | Lists all server endpoints within a sync group | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |
| `Set-AzStorageSyncServerEndpoint` | Updates server endpoint properties (cloud tiering policies) | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name`, `-CloudTiering`, `-VolumeFreeSpacePercent`, `-TierFilesOlderThanDays` |
| `Set-AzStorageSyncServerEndpointPermission` | Manages permissions for a server endpoint | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |
| `Remove-AzStorageSyncServerEndpoint` | Deletes a server endpoint | `-ResourceGroupName`, `-StorageSyncServiceName`, `-SyncGroupName`, `-Name` |

### Server Registration

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `Register-AzStorageSyncServer` | Registers a server to a storage sync service (creates trust relationship) | `-ResourceGroupName`, `-StorageSyncServiceName` |
| `Get-AzStorageSyncServer` | Lists all servers registered to a storage sync service | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |
| `Set-AzStorageSyncServer` | Updates registered server properties | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |
| `Reset-AzStorageSyncServerCertificate` | Resets the certificate of a registered server | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |
| `Unregister-AzStorageSyncServer` | Unregisters a server from a storage sync service | `-ResourceGroupName`, `-StorageSyncServiceName`, `-Name` |

### Operational Tools

| Cmdlet | Description | Main Parameters |
|--------|-------------|-----------------|
| `Invoke-AzStorageSyncCompatibilityCheck` | Checks for potential compatibility issues between your system and Azure File Sync | System-level checks (Windows version, PowerShell version, network connectivity) |

## Key Concepts

### Cloud Endpoints
A cloud endpoint is a reference to an existing Azure file share and represents the cloud participant in a sync group. Each sync group requires at least one cloud endpoint. The cloud endpoint defines which Azure file share will be the reference point for sync operations.

**Important**: Once a cloud endpoint is created, the namespace metadata syncs immediately. File content downloads follow based on configured policies.

### Server Endpoints
A server endpoint represents a folder path on a registered server that participates in sync. When created, the specified path starts syncing files with other endpoints in the sync group.

**Key Points**:
- Namespace metadata syncs first, then file data
- If files already exist, a reconciliation process determines if they are the same
- Starting with an empty location enables fast disaster recovery
- Cloud tiering can be enabled to manage local cache vs cloud storage

### Cloud Tiering
Cloud tiering allows servers to act as a cache, keeping frequently accessed files locally while archiving less frequently used files to Azure. Configure with:
- `-CloudTiering`: Enable cloud tiering
- `-VolumeFreeSpacePercent`: Reserve percentage of volume space (1-99%, default 20%)
- `-TierFilesOlderThanDays`: Archive files not accessed for specified days

### Change Detection
Manual change detection can be triggered at three scopes:
1. **Share-level**: Detects all changes in the entire Azure file share
2. **Directory-level**: Limited to 10,000 items per execution
3. **File-level**: Specify individual files (max 10,000 per execution)

**Limitation**: Directory and file-level detection does not detect file deletions or moves. Use share-level detection for complete change tracking.

## Common Workflows

### Setting up Azure File Sync

1. **Create Storage Sync Service**
   ```
   New-AzStorageSyncService -ResourceGroupName "myRG" -Name "mySyncService" -Location "EastUS"
   ```

2. **Create Sync Group**
   ```
   New-AzStorageSyncGroup -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" -Name "mySyncGroup"
   ```

3. **Create Cloud Endpoint**
   ```
   New-AzStorageSyncCloudEndpoint -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
     -SyncGroupName "mySyncGroup" -Name "myCloudEndpoint" `
     -StorageAccountResourceId "/subscriptions/.../storageAccounts/myStorage" `
     -AzureFileShareName "myShare"
   ```

4. **Register Server**
   ```
   Register-AzStorageSyncServer -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService"
   ```
   *(Must run on the server to be registered)*

5. **Create Server Endpoint**
   ```
   $server = Get-AzStorageSyncServer -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService"
   New-AzStorageSyncServerEndpoint -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
     -SyncGroupName "mySyncGroup" -Name "myServerEndpoint" `
     -ServerResourceId $server.ResourceId -ServerLocalPath "D:\SyncFolder" `
     -CloudTiering -VolumeFreeSpacePercent 20 -TierFilesOlderThanDays 7
   ```

### Managing Cloud Tiering

```
Set-AzStorageSyncServerEndpoint -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
  -SyncGroupName "mySyncGroup" -Name "myServerEndpoint" `
  -VolumeFreeSpacePercent 30 -TierFilesOlderThanDays 14
```

### Triggering Change Detection

```
# Full share change detection
Invoke-AzStorageSyncChangeDetection -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
  -SyncGroupName "mySyncGroup" -Name "myCloudEndpoint"

# Directory-level change detection
Invoke-AzStorageSyncChangeDetection -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
  -SyncGroupName "mySyncGroup" -Name "myCloudEndpoint" `
  -DirectoryPath "myFolder" -Recursive

# File-level change detection
Invoke-AzStorageSyncChangeDetection -ResourceGroupName "myRG" -StorageSyncServiceName "mySyncService" `
  -SyncGroupName "mySyncGroup" -Name "myCloudEndpoint" `
  -Path "folder/file1.txt", "folder/file2.txt"
```

## System Requirements

**For Server Registration and Operations:**
- Must run on Windows Server (2012 R2 or later)
- PowerShell 5.1 or later
- .NET Framework 4.6 or later
- Network connectivity to Azure

**For Cloud Tiering:**
- Requires compatible NTFS volume
- Sufficient free disk space for local cache

## Best Practices

### Server Planning
1. **One Service Per Organization Group**: Create separate storage sync services for different departments or business units
2. **Register Servers First**: Always register servers before creating endpoints
3. **Plan Sync Topology**: Design your sync group structure before creating endpoints

### Endpoint Configuration
1. **Cloud Endpoint**: Must be created before server endpoints
2. **Server Endpoint Paths**: Use empty folders to enable fast disaster recovery
3. **Cloud Tiering**: Enable only when needed to manage storage space

### Change Detection
1. **Share-Level for Complete Coverage**: Use share-level detection for critical sync operations
2. **Directory/File-Level for Performance**: Use scoped detection for large shares to complete faster
3. **Monitor 10,000 Item Limit**: Scope directory detection appropriately

### Troubleshooting
- Check network connectivity between server and Azure
- Verify server registration status with `Get-AzStorageSyncServer`
- Review server certificate validity with `Reset-AzStorageSyncServerCertificate` if needed
- Monitor sync health using the Azure portal or PowerShell

## Related Documentation

- [Azure File Sync Overview](https://learn.microsoft.com/en-us/azure/storage/file-sync/file-sync-introduction)
- [Azure File Sync Deployment Guide](https://learn.microsoft.com/en-us/azure/storage/file-sync/file-sync-deployment-guide)
- [Azure File Sync Troubleshooting](https://learn.microsoft.com/en-us/azure/storage/files/storage-sync-files-troubleshoot)
- [Azure Storage Sync PowerShell Reference](https://learn.microsoft.com/en-us/powershell/module/az.storagesync)

## Support

For issues or questions regarding Azure Storage Sync:
- Check the [troubleshooting guide](https://learn.microsoft.com/en-us/azure/storage/files/storage-sync-files-troubleshoot)
- Review [Azure Support](https://azure.microsoft.com/support/) documentation
- Contact Microsoft Support for production issues
