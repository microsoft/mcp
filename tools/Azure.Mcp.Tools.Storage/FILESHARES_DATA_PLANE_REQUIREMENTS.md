# Azure Storage File Shares Data Plane Operations - Requirements Document

## Overview

This document outlines the requirements for implementing Azure Storage File Shares data plane operations using the `Azure.Storage.Files.Shares` SDK. These operations complement the existing management plane operations (Azure.ResourceManager.Storage and Azure.ResourceManager.FileShares) by providing direct access to file share contents.

### Scope

**Current Implementation (Management Plane):**
- Azure.ResourceManager.Storage - Storage account management
- Azure.ResourceManager.FileShares - File share resource management (create, delete, update, snapshots)
- Tools: `Azure.Mcp.Tools.FileShares` - File share lifecycle operations

**This Document (Data Plane):**
- Azure.Storage.Files.Shares - Direct file and directory operations within file shares
- Tools: `Azure.Mcp.Tools.Storage` - Add file share data operations

## SDK Analysis

### Azure.Storage.Files.Shares Package

**Primary Client Classes:**
1. `ShareServiceClient` - Service-level operations (list shares, get properties)
2. `ShareClient` - Share-level operations (create/delete directories, manage metadata)
3. `ShareDirectoryClient` - Directory operations (list files/subdirectories, manage directories)
4. `ShareFileClient` - File operations (upload, download, delete, copy files)

**Key Capabilities:**
- File upload/download with progress tracking
- Directory creation and navigation
- File and directory metadata management
- File range operations (read/write specific byte ranges)
- File leasing for concurrency control
- SAS token generation for secure access
- Share snapshots (data plane view)
- Permission management (NTFS ACLs)

## Command Design Patterns

### Command Naming Convention
Follow the established pattern: `azmcp storage <resource> <operation>`

### Resource Hierarchy
```
storage
├── share (data plane operations)
│   ├── get              # Get share properties
│   ├── list             # List shares in account
│   ├── directory
│   │   ├── create       # Create directory
│   │   ├── delete       # Delete directory
│   │   ├── list         # List directory contents
│   │   └── get          # Get directory properties
│   └── file
│       ├── upload       # Upload file
│       ├── download     # Download file
│       ├── delete       # Delete file
│       ├── list         # List files in directory
│       ├── copy         # Copy file
│       └── get          # Get file properties
```

## Proposed Commands

### 1. File Upload Operations

#### Command: `azmcp storage share file upload`

**Description:**
Upload a file to an Azure File Share. Supports both small files (direct upload) and large files (chunked upload with progress tracking). Automatically creates parent directories if they don't exist.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--source` (required): Local file path to upload
- `--destination` (required): Remote file path in share (e.g., "folder/file.txt")
- `--subscription` (optional): Azure subscription
- `--overwrite` (optional): Overwrite if file exists (default: false)
- `--metadata` (optional): File metadata as JSON key-value pairs
- `--content-type` (optional): Content type header
- `--progress` (optional): Show upload progress (default: true)
- `--chunk-size` (optional): Upload chunk size in MB (default: 4)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Upload configuration files to shared storage
- Deploy application assets to file shares
- Backup local files to Azure File Shares
- Synchronize local directories to cloud storage

**Example:**
```bash
azmcp storage share file upload \
  --account mystorageaccount \
  --share myfileshare \
  --source "C:\local\data.json" \
  --destination "config/data.json" \
  --overwrite true
```

**Return Model:**
```csharp
public record FileUploadResult
{
    public string FileName { get; init; }
    public string Path { get; init; }
    public long SizeBytes { get; init; }
    public string ETag { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public bool Overwritten { get; init; }
}
```

---

### 2. File Download Operations

#### Command: `azmcp storage share file download`

**Description:**
Download a file from an Azure File Share to local storage. Supports resumable downloads and range-based downloads for large files.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--source` (required): Remote file path in share
- `--destination` (required): Local file path to download to
- `--subscription` (optional): Azure subscription
- `--overwrite` (optional): Overwrite local file if exists (default: false)
- `--range-start` (optional): Start byte offset for partial download
- `--range-end` (optional): End byte offset for partial download
- `--progress` (optional): Show download progress (default: true)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Retrieve configuration files from shared storage
- Download application logs for analysis
- Backup cloud files to local storage
- Restore files from Azure File Shares

**Example:**
```bash
azmcp storage share file download \
  --account mystorageaccount \
  --share myfileshare \
  --source "logs/app.log" \
  --destination "C:\local\logs\app.log"
```

**Return Model:**
```csharp
public record FileDownloadResult
{
    public string FileName { get; init; }
    public string LocalPath { get; init; }
    public long SizeBytes { get; init; }
    public string ContentType { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
```

---

### 3. File List Operations

#### Command: `azmcp storage share file list`

**Description:**
List files and directories within a file share directory. Supports recursive listing and filtering by prefix.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--directory` (optional): Directory path (default: root)
- `--subscription` (optional): Azure subscription
- `--recursive` (optional): List recursively (default: false)
- `--prefix` (optional): Filter by file/directory name prefix
- `--max-results` (optional): Maximum number of results per page
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Browse file share contents
- Discover files for processing
- Audit file share structure
- Generate file inventory reports

**Example:**
```bash
azmcp storage share file list \
  --account mystorageaccount \
  --share myfileshare \
  --directory "logs/2024" \
  --recursive true
```

**Return Model:**
```csharp
public record FileListResult
{
    public List<FileShareItem> Items { get; init; }
    public string ContinuationToken { get; init; }
}

public record FileShareItem
{
    public string Name { get; init; }
    public string Path { get; init; }
    public bool IsDirectory { get; init; }
    public long? SizeBytes { get; init; }
    public DateTimeOffset? LastModified { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
}
```

---

### 4. File Delete Operations

#### Command: `azmcp storage share file delete`

**Description:**
Delete a file from an Azure File Share. Idempotent operation (succeeds if file doesn't exist).

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--file` (required): File path in share to delete
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Clean up temporary files
- Remove obsolete data
- Implement file retention policies
- Free up storage space

**Example:**
```bash
azmcp storage share file delete \
  --account mystorageaccount \
  --share myfileshare \
  --file "temp/processing-data.tmp"
```

**Return Model:**
```csharp
public record FileDeleteResult
{
    public string FileName { get; init; }
    public bool Deleted { get; init; }
    public string Status { get; init; } // "Deleted" or "NotFound"
}
```

---

### 5. Directory Create Operations

#### Command: `azmcp storage share directory create`

**Description:**
Create a directory in an Azure File Share. Creates parent directories recursively if they don't exist.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--directory` (required): Directory path to create
- `--subscription` (optional): Azure subscription
- `--metadata` (optional): Directory metadata as JSON
- `--parents` (optional): Create parent directories (default: true)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Setup directory structure for applications
- Organize files before upload
- Create dated directories for logs
- Prepare storage for batch operations

**Example:**
```bash
azmcp storage share directory create \
  --account mystorageaccount \
  --share myfileshare \
  --directory "logs/2024/01" \
  --parents true
```

**Return Model:**
```csharp
public record DirectoryCreateResult
{
    public string DirectoryPath { get; init; }
    public DateTimeOffset Created { get; init; }
    public bool AlreadyExists { get; init; }
}
```

---

### 6. Directory Delete Operations

#### Command: `azmcp storage share directory delete`

**Description:**
Delete a directory from an Azure File Share. Can optionally delete non-empty directories recursively.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--directory` (required): Directory path to delete
- `--subscription` (optional): Azure subscription
- `--recursive` (optional): Delete non-empty directory (default: false)
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Clean up completed batch job directories
- Remove test data directories
- Implement directory retention policies
- Free up storage space

**Example:**
```bash
azmcp storage share directory delete \
  --account mystorageaccount \
  --share myfileshare \
  --directory "temp/batch-job-123" \
  --recursive true
```

**Return Model:**
```csharp
public record DirectoryDeleteResult
{
    public string DirectoryPath { get; init; }
    public bool Deleted { get; init; }
    public int FilesDeleted { get; init; }
    public int SubdirectoriesDeleted { get; init; }
}
```

---

### 7. Directory List Operations

#### Command: `azmcp storage share directory list`

**Description:**
List contents of a directory in an Azure File Share. Returns both files and subdirectories with their properties.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--directory` (optional): Directory path (default: root)
- `--subscription` (optional): Azure subscription
- `--max-results` (optional): Maximum number of results
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Browse directory contents
- Discover subdirectories for navigation
- Audit directory structure
- Generate directory listings

**Example:**
```bash
azmcp storage share directory list \
  --account mystorageaccount \
  --share myfileshare \
  --directory "data"
```

**Return Model:**
```csharp
public record DirectoryListResult
{
    public string DirectoryPath { get; init; }
    public List<DirectoryItem> Subdirectories { get; init; }
    public List<FileItem> Files { get; init; }
}

public record DirectoryItem
{
    public string Name { get; init; }
    public string Path { get; init; }
}

public record FileItem
{
    public string Name { get; init; }
    public string Path { get; init; }
    public long SizeBytes { get; init; }
    public DateTimeOffset LastModified { get; init; }
}
```

---

### 8. File Copy Operations

#### Command: `azmcp storage share file copy`

**Description:**
Copy a file within the same file share or between different file shares. Supports cross-account copy with proper authentication.

**Parameters:**
- `--account` (required): Source storage account name
- `--share` (required): Source file share name
- `--source` (required): Source file path
- `--destination-account` (optional): Destination account (default: same as source)
- `--destination-share` (optional): Destination share (default: same as source)
- `--destination` (required): Destination file path
- `--subscription` (optional): Azure subscription
- `--overwrite` (optional): Overwrite if destination exists (default: false)
- `--metadata` (optional): Destination file metadata
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Duplicate files for backup
- Move files between directories
- Copy files across file shares
- Create file templates

**Example:**
```bash
azmcp storage share file copy \
  --account mystorageaccount \
  --share myfileshare \
  --source "templates/config.json" \
  --destination "app-instances/instance-1/config.json"
```

**Return Model:**
```csharp
public record FileCopyResult
{
    public string SourcePath { get; init; }
    public string DestinationPath { get; init; }
    public string CopyId { get; init; }
    public string CopyStatus { get; init; }
    public long SizeBytes { get; init; }
}
```

---

### 9. File Properties Operations

#### Command: `azmcp storage share file get`

**Description:**
Get properties and metadata of a specific file in an Azure File Share without downloading the file content.

**Parameters:**
- `--account` (required): Storage account name
- `--share` (required): File share name
- `--file` (required): File path in share
- `--subscription` (optional): Azure subscription
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Check file existence
- Verify file size before download
- Retrieve file metadata
- Audit file properties

**Example:**
```bash
azmcp storage share file get \
  --account mystorageaccount \
  --share myfileshare \
  --file "data/report.pdf"
```

**Return Model:**
```csharp
public record FilePropertiesResult
{
    public string FileName { get; init; }
    public string Path { get; init; }
    public long SizeBytes { get; init; }
    public string ContentType { get; init; }
    public string ETag { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public string ContentMD5 { get; init; }
}
```

---

### 10. Share List Operations

#### Command: `azmcp storage share list`

**Description:**
List all file shares in a storage account. Provides data plane view of shares with usage statistics.

**Parameters:**
- `--account` (required): Storage account name
- `--subscription` (optional): Azure subscription
- `--prefix` (optional): Filter by share name prefix
- `--include-metadata` (optional): Include share metadata (default: false)
- `--include-snapshots` (optional): Include share snapshots (default: false)
- `--max-results` (optional): Maximum number of results
- `--retry-policy` (optional): Retry policy options

**Use Cases:**
- Discover available file shares
- Monitor share usage
- Audit storage account contents
- Generate share inventory

**Example:**
```bash
azmcp storage share list \
  --account mystorageaccount \
  --include-metadata true
```

**Return Model:**
```csharp
public record ShareListResult
{
    public List<ShareInfo> Shares { get; init; }
}

public record ShareInfo
{
    public string Name { get; init; }
    public long? QuotaGiB { get; init; }
    public DateTimeOffset? LastModified { get; init; }
    public Dictionary<string, string> Metadata { get; init; }
    public bool IsSnapshot { get; init; }
    public string SnapshotTime { get; init; }
}
```

---

## Authentication and Authorization

### Authentication Strategy
Use the existing `IAzureTokenCredentialProvider` pattern from Azure.Mcp.Core:

```csharp
var credential = await GetCredentialAsync(tenant, cancellationToken);
var shareServiceClient = new ShareServiceClient(
    new Uri($"https://{accountName}.file.core.windows.net"),
    credential
);
```

### Required Permissions
- **Reader Role**: List shares, get properties, download files
- **Contributor Role**: Upload, delete, create directories
- **Storage File Data SMB Share Contributor**: Full data plane access

### Connection String Support (Optional)
For development scenarios, support connection string authentication:
```csharp
var shareServiceClient = new ShareServiceClient(connectionString);
```

## Service Implementation Pattern

### Service Interface
```csharp
public interface IStorageFileShareService
{
    // File operations
    Task<FileUploadResult> UploadFileAsync(...);
    Task<FileDownloadResult> DownloadFileAsync(...);
    Task<FileDeleteResult> DeleteFileAsync(...);
    Task<FilePropertiesResult> GetFilePropertiesAsync(...);
    Task<FileCopyResult> CopyFileAsync(...);
    Task<FileListResult> ListFilesAsync(...);
    
    // Directory operations
    Task<DirectoryCreateResult> CreateDirectoryAsync(...);
    Task<DirectoryDeleteResult> DeleteDirectoryAsync(...);
    Task<DirectoryListResult> ListDirectoryAsync(...);
    
    // Share operations
    Task<ShareListResult> ListSharesAsync(...);
}
```

### Service Implementation
```csharp
public class StorageFileShareService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<StorageFileShareService> logger)
    : BaseAzureService(tenantService), IStorageFileShareService
{
    private async Task<ShareServiceClient> CreateShareServiceClientAsync(
        string accountName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var credential = await GetCredentialAsync(tenant, cancellationToken);
        var clientOptions = new ShareClientOptions();
        
        // Apply retry policy if provided
        if (retryPolicy != null)
        {
            clientOptions.Retry.Mode = retryPolicy.Mode == "Exponential" 
                ? RetryMode.Exponential 
                : RetryMode.Fixed;
            clientOptions.Retry.MaxRetries = retryPolicy.MaxRetries ?? 3;
            clientOptions.Retry.Delay = TimeSpan.FromSeconds(retryPolicy.RetryDelay ?? 1);
        }
        
        return new ShareServiceClient(
            new Uri($"https://{accountName}.file.core.windows.net"),
            credential,
            clientOptions
        );
    }
}
```

## Command Implementation Pattern

### Base Command
```csharp
public abstract class BaseFileShareCommand<TOptions> : SubscriptionCommand<TOptions>
    where TOptions : FileShareCommandOptions, new()
{
    protected readonly ILogger _logger;
    protected readonly IStorageFileShareService _fileShareService;

    protected BaseFileShareCommand(
        ILogger logger,
        IStorageFileShareService fileShareService)
    {
        _logger = logger;
        _fileShareService = fileShareService;
    }

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Account.AsRequired());
        command.Options.Add(StorageOptionDefinitions.Share.AsRequired());
    }
}
```

### Example Command Implementation
```csharp
public sealed class FileUploadCommand(
    ILogger<FileUploadCommand> logger,
    IStorageFileShareService service)
    : BaseFileShareCommand<FileUploadOptions>(logger, service)
{
    public override string Id => "f8a9b0c1-d2e3-4f5g-6h7i-8j9k0l1m2n3o";
    public override string Name => "upload";
    public override string Description => 
        "Upload a file to an Azure File Share with support for large files and progress tracking.";
    public override string Title => "Upload File to File Share";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,  // Modifies storage
        Idempotent = false,  // Multiple uploads create duplicates
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = true,  // Requires local file access
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FileShareOptionDefinitions.SourceFile.AsRequired());
        command.Options.Add(FileShareOptionDefinitions.DestinationPath.AsRequired());
        command.Options.Add(FileShareOptionDefinitions.Overwrite.AsOptional());
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
            var result = await _fileShareService.UploadFileAsync(
                options.Account!,
                options.Share!,
                options.SourceFile!,
                options.DestinationPath!,
                options.Subscription!,
                options.Overwrite,
                options.Metadata,
                options.ContentType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                FileShareJsonContext.Default.FileUploadResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
```

## Option Definitions

### New Option Definitions
```csharp
public static class FileShareOptionDefinitions
{
    public static readonly OptionDefinition Share = new()
    {
        Name = "share",
        Description = "File share name",
        ValueName = "file-share-name",
        Required = true
    };

    public static readonly OptionDefinition SourceFile = new()
    {
        Name = "source",
        Description = "Local file path to upload",
        ValueName = "local-path",
        Required = true
    };

    public static readonly OptionDefinition DestinationPath = new()
    {
        Name = "destination",
        Description = "Remote file path in share (e.g., 'folder/file.txt')",
        ValueName = "remote-path",
        Required = true
    };

    public static readonly OptionDefinition Directory = new()
    {
        Name = "directory",
        Description = "Directory path in file share",
        ValueName = "directory-path",
        Required = false
    };

    public static readonly OptionDefinition Overwrite = new()
    {
        Name = "overwrite",
        Description = "Overwrite if file/directory exists",
        ValueName = "true|false",
        Required = false
    };

    public static readonly OptionDefinition Recursive = new()
    {
        Name = "recursive",
        Description = "Perform operation recursively",
        ValueName = "true|false",
        Required = false
    };

    public static readonly OptionDefinition Prefix = new()
    {
        Name = "prefix",
        Description = "Filter by name prefix",
        ValueName = "prefix",
        Required = false
    };

    public static readonly OptionDefinition IncludeMetadata = new()
    {
        Name = "include-metadata",
        Description = "Include metadata in results",
        ValueName = "true|false",
        Required = false
    };
}
```

## Error Handling

### Standard Error Scenarios
```csharp
protected override string GetErrorMessage(Exception ex) => ex switch
{
    RequestFailedException reqEx when reqEx.Status == 404 =>
        "File share, directory, or file not found. Verify the path and account name.",
    RequestFailedException reqEx when reqEx.Status == 403 =>
        "Access denied. Verify you have appropriate permissions (Storage File Data SMB Share Contributor).",
    RequestFailedException reqEx when reqEx.Status == 409 =>
        "Conflict. The resource already exists. Use --overwrite to replace it.",
    FileNotFoundException =>
        "Local file not found. Verify the source file path exists.",
    UnauthorizedAccessException =>
        "Cannot access local file. Check file permissions.",
    IOException ioEx =>
        $"I/O error: {ioEx.Message}",
    _ => base.GetErrorMessage(ex)
};

protected override int GetStatusCode(Exception ex) => ex switch
{
    RequestFailedException reqEx => reqEx.Status,
    FileNotFoundException => 404,
    UnauthorizedAccessException => 403,
    IOException => 500,
    _ => base.GetStatusCode(ex)
};
```

## Testing Requirements

### Unit Tests
Each command requires comprehensive unit tests:
```csharp
[Fact] public void Constructor_InitializesCommandCorrectly()
[Theory] public async Task ExecuteAsync_ValidatesInputCorrectly(...)
[Fact] public async Task ExecuteAsync_DeserializationValidation()
[Fact] public async Task ExecuteAsync_HandlesServiceErrors()
[Fact] public void BindOptions_BindsOptionsCorrectly()
```

### Live Test Infrastructure
Create Bicep template and post-deployment script:
- `tools/Azure.Mcp.Tools.Storage/tests/test-resources-fileshare.bicep`
- `tools/Azure.Mcp.Tools.Storage/tests/test-resources-fileshare-post.ps1`

**Bicep template should provision:**
- Storage account with file share enabled
- File share with test directories
- Sample test files
- RBAC role assignments (Storage File Data SMB Share Contributor)
- Network configuration (private endpoints if needed)

### E2E Test Prompts
Add to `servers/Azure.Mcp.Server/docs/e2eTestPrompts.md`:
```markdown
## Azure Storage File Shares (Data Plane)

| Tool Name | Test Prompt |
|:----------|:----------|
| storage_share_file_upload | Upload the file config.json to my file share |
| storage_share_file_download | Download the file data.csv from file share myshare |
| storage_share_file_list | List all files in the logs directory |
| storage_share_file_delete | Delete the file temp/old-data.json |
| storage_share_directory_create | Create a new directory called backups in my file share |
| storage_share_directory_list | Show me what's in the reports directory |
```

## JSON Serialization Context

All response models must be registered for AOT compatibility:
```csharp
[JsonSerializable(typeof(FileUploadResult))]
[JsonSerializable(typeof(FileDownloadResult))]
[JsonSerializable(typeof(FileDeleteResult))]
[JsonSerializable(typeof(FileListResult))]
[JsonSerializable(typeof(DirectoryCreateResult))]
[JsonSerializable(typeof(DirectoryDeleteResult))]
[JsonSerializable(typeof(DirectoryListResult))]
[JsonSerializable(typeof(FileCopyResult))]
[JsonSerializable(typeof(FilePropertiesResult))]
[JsonSerializable(typeof(ShareListResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class FileShareJsonContext : JsonSerializerContext;
```

## Project Configuration

### Add Package Reference
Update `tools/Azure.Mcp.Tools.Storage/src/Azure.Mcp.Tools.Storage.csproj`:
```xml
<PackageReference Include="Azure.Storage.Files.Shares" />
```

### Update Directory.Packages.props
Add the package version:
```xml
<PackageVersion Include="Azure.Storage.Files.Shares" Version="12.22.0" />
```

## Documentation Requirements

### Update Command Reference
Add new commands to `servers/Azure.Mcp.Server/docs/azmcp-commands.md`:
```markdown
#### File Share Data Operations

##### Upload File
```bash
azmcp storage share file upload --account <account> --share <share> \
  --source <local-path> --destination <remote-path> [--overwrite]
```

##### Download File
```bash
azmcp storage share file download --account <account> --share <share> \
  --source <remote-path> --destination <local-path> [--overwrite]
```
...
```

### Update README
Update `tools/Azure.Mcp.Tools.Storage/README.md` with new capabilities.

### Create Changelog Entry
Use `docs/changelog-entries.md` pattern with `-ChangelogPath servers/Azure.Mcp.Server/CHANGELOG.md`.

## Implementation Priorities

### Phase 1: Core File Operations (High Priority)
1. File Upload
2. File Download
3. File List
4. File Delete

### Phase 2: Directory Operations (Medium Priority)
5. Directory Create
6. Directory Delete
7. Directory List

### Phase 3: Advanced Operations (Lower Priority)
8. File Copy
9. File Properties
10. Share List (data plane)

### Phase 4: Advanced Features (Future)
11. File Range Operations (partial upload/download)
12. File Leasing
13. SAS Token Generation
14. Permission Management (NTFS ACLs)

## Success Criteria

- [ ] All commands follow established patterns in AGENTS.md
- [ ] Commands use primary constructors and static OptionDefinitions
- [ ] Service implements BaseAzureService pattern
- [ ] All response models registered in JSON context (AOT-safe)
- [ ] Comprehensive unit tests with >80% coverage
- [ ] Live test infrastructure with Bicep templates
- [ ] Tool description validation (top 3 ranking, ≥0.4 confidence)
- [ ] Documentation updated (azmcp-commands.md, e2eTestPrompts.md)
- [ ] Spelling check passes
- [ ] One tool per PR for faster review cycles
- [ ] Changelog entries created for user-facing changes

## References

- **Azure.Storage.Files.Shares SDK**: https://learn.microsoft.com/dotnet/api/azure.storage.files.shares
- **File Share REST API**: https://learn.microsoft.com/rest/api/storageservices/file-service-rest-api
- **Existing patterns**: `tools/Azure.Mcp.Tools.Storage/src/Commands/Blob/BlobUploadCommand.cs`
- **Architecture guide**: `AGENTS.md`
- **Implementation guide**: `servers/Azure.Mcp.Server/docs/new-command.md`
