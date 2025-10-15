# Storage Operations Testing Scenario

Test Azure MCP Server's storage account and blob operations capabilities.

## Objectives

- Create and manage storage accounts
- Work with blob containers
- Upload and download files
- Test storage permissions
- Verify error handling

## Prerequisites

- [ ] Azure MCP Server installed and configured
- [ ] Azure CLI installed (`az --version`)
- [ ] Authenticated to Azure (`az login`)
- [ ] Active Azure subscription
- [ ] Test files ready for upload
- [ ] GitHub Copilot with Agent mode enabled

## Test Scenarios

### Scenario 1: Create a Storage Account

**Objective**: Test basic storage account creation

**Steps**:

1. Open GitHub Copilot Chat in Agent mode

2. Use this prompt:
   ```
   Create a new storage account named 'bugbashstorage<random>' in 
   resource group '<your-rg>' in East US with:
   - Standard_LRS redundancy
   - Hot access tier
   - Enable hierarchical namespace for Data Lake
   ```

3. **Monitor the creation**:
   - Watch for progress updates
   - Note any warnings or errors
   - Record creation time: _____ seconds

4. **Verify the storage account**:
   ```
   Show me the details of storage account 'bugbashstorage<random>'
   ```

5. **Check the properties**:
   - [ ] Account name is correct
   - [ ] Location is East US
   - [ ] Redundancy is Standard_LRS
   - [ ] Access tier is Hot
   - [ ] Hierarchical namespace is enabled

**Expected Results**:
- Storage account created successfully
- All properties match specifications
- Account is accessible

### Scenario 2: Manage Blob Containers

**Objective**: Test container creation and management

**Steps**:

1. **Create a container**:
   ```
   Create a blob container named 'test-data' in storage account 
   'bugbashstorage<random>' with private access
   ```

2. **Verify container creation**:
   ```
   List all containers in storage account 'bugbashstorage<random>'
   ```

3. **Check container properties**:
   - [ ] Container name is correct
   - [ ] Public access level is None (private)
   - [ ] Container is empty

4. **Create additional containers** with different access levels:
   ```
   Create containers:
   - 'public-blobs' with blob public access
   - 'public-container' with container public access
   - 'private-data' with no public access
   ```

5. **List and verify all containers**:
   ```
   Show me all containers in 'bugbashstorage<random>' with their access levels
   ```

**Expected Results**:
- All containers created
- Access levels set correctly
- Containers are accessible

---

### Scenario 3: Upload Files to Blob Storage

**Objective**: Test file upload capabilities

**Steps**:

1. **Prepare test files**:
   - Create a small text file (< 1 MB): `test-small.txt`
   - Create a medium file (10-50 MB): `test-medium.zip`
   - Create a large file (> 100 MB): `test-large.iso` (optional)

2. **Upload small file**:
   ```
   Upload the file 'test-small.txt' to container 'test-data' in 
   storage account 'bugbashstorage<random>'
   ```

3. **Verify upload**:
   ```
   List all blobs in container 'test-data'
   ```

4. **Check blob properties**:
   - [ ] File name is correct
   - [ ] File size matches original
   - [ ] Content type is detected
   - [ ] Upload time is recent

5. **Upload medium file** and measure performance:
   ```
   Upload 'test-medium.zip' to 'test-data' container
   ```
   - Upload time: _____ seconds
   - Upload speed: _____ MB/s

6. **Upload file with custom metadata**:
   ```
   Upload 'test-small.txt' as 'document.txt' to 'test-data' with 
   content type 'text/plain' and metadata 'author=BugBash'
   ```

7. **Verify metadata**:
   ```
   Show me the properties of blob 'document.txt' in container 'test-data'
   ```

**Expected Results**:
- All files upload successfully
- File integrity is maintained
- Metadata is stored correctly
- Performance is reasonable

---

### Scenario 4: Download Files from Blob Storage

**Objective**: Test file download capabilities

**Steps**:

1. **Download a file**:
   ```
   Download the blob 'test-small.txt' from container 'test-data' 
   in storage account 'bugbashstorage<random>' to 'downloaded-file.txt'
   ```

2. **Verify download**:
   - [ ] File is downloaded
   - [ ] File size matches original
   - [ ] Content is identical
   - Compare checksums if possible

3. **Download multiple files**:
   ```
   Download all blobs from container 'test-data' to folder 'downloads'
   ```

4. **Verify all files**:
   - [ ] All blobs are downloaded
   - [ ] Folder structure is correct
   - [ ] No corruption

**Expected Results**:
- Files download successfully
- Content integrity is preserved
- Reasonable download speeds

---

### Scenario 5: Storage Account Configuration

**Objective**: Test storage account settings and features

**Steps**:

1. **List all storage accounts**:
   ```
   Show me all my storage accounts in subscription '<subscription-name>'
   ```

2. **Filter by resource group**:
   ```
   List storage accounts in resource group '<your-rg>'
   ```

3. **Get account properties**:
   ```
   Show me detailed information for storage account 'bugbashstorage<random>'
   including:
   - SKU and redundancy
   - Access tier
   - Network rules
   - Encryption settings
   - HTTPS enforcement
   ```

4. **Check account keys** (if permissions allow):
   ```
   Show me the access keys for storage account 'bugbashstorage<random>'
   ```

5. **Verify security settings**:
   - [ ] HTTPS-only traffic is enforced
   - [ ] Minimum TLS version is set
   - [ ] Secure transfer is enabled
   - [ ] Public access is configured correctly

**Expected Results**:
- Account details are accurate
- Security settings are appropriate
- Configuration matches expectations

---

### Scenario 6: Test Different Storage Types

**Objective**: Test various Azure Storage services

#### 6a. Table Storage

**Steps**:
```
Does storage account 'bugbashstorage<random>' support Azure Table Storage? 
If yes, show me how to work with it.
```

**Verify**:
- [ ] Table storage capability confirmed
- [ ] Guidance provided for usage

#### 6b. Queue Storage

**Steps**:
```
Does storage account 'bugbashstorage<random>' support Azure Queue Storage? 
If yes, show me how to create and manage queues.
```

**Verify**:
- [ ] Queue storage capability confirmed
- [ ] Instructions for queue operations

#### 6c. File Shares

**Steps**:
```
Does storage account 'bugbashstorage<random>' support Azure File Shares? 
If yes, show me how to create and mount a file share.
```

**Verify**:
- [ ] File share capability confirmed
- [ ] Mount instructions provided

## Common Issues to Watch For

- **Authentication Failures**: Credential or permission issues
- **Network Timeouts**: Slow or failed uploads/downloads
- **Name Conflicts**: Duplicate blob or container names
- **Path Issues**: Platform-specific path separator problems
- **File Encoding**: Text file encoding issues across platforms
- **Large File Handling**: Memory issues with big files
- **Concurrent Operations**: Race conditions with parallel uploads
- **Permission Errors**: RBAC or access key issues


## Related Resources

- [Azure Storage Documentation](https://learn.microsoft.com/azure/storage/)
- [Azure Blob Storage Best Practices](https://learn.microsoft.com/azure/storage/blobs/storage-blobs-introduction)
- [Data Lake Storage Gen2](https://learn.microsoft.com/azure/storage/blobs/data-lake-storage-introduction)
- [Storage Test Prompts](https://github.com/microsoft/mcp/blob/main/servers/Azure.Mcp.Server/docs/e2eTestPrompts.md#azure-storage)
- [Report Issues](https://github.com/microsoft/mcp/issues)

**Next**: [Agent Building](agent-building.md)
