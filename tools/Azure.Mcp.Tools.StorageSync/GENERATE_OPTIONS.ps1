# Batch Options Classes Generation Script
# Run from: d:\code\ab\mcp\tools\Azure.Mcp.Tools.StorageSync\src\Options

# StorageSyncService Options
@{
    "StorageSyncServiceUpdateOptions" = @{
        description = "Options for StorageSyncServiceUpdateCommand"
        command = "StorageSyncServiceUpdateCommand"
        properties = @(
            @{ name = "ResourceGroup"; type = "string?"; description = "Gets or sets the resource group (required)." }
            @{ name = "Name"; type = "string?"; description = "Gets or sets the name of the storage sync service." }
            @{ name = "IncomingTrafficPolicy"; type = "string?"; description = "Gets or sets the incoming traffic policy." }
            @{ name = "Tags"; type = "Dictionary<string, string>?"; description = "Gets or sets tags for the resource." }
        )
    }
    "StorageSyncServiceDeleteOptions" = @{
        description = "Options for StorageSyncServiceDeleteCommand"
        command = "StorageSyncServiceDeleteCommand"
        properties = @(
            @{ name = "ResourceGroup"; type = "string?"; description = "Gets or sets the resource group (required)." }
            @{ name = "Name"; type = "string?"; description = "Gets or sets the name of the storage sync service." }
        )
    }
} | GetEnumerator | ForEach-Object {
    $className = $_.Key
    $details = $_.Value

    $content = @"
namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Options for $($details.command).
/// </summary>
public class $className : BaseStorageSyncOptions
{
"@

    foreach ($prop in $details.properties) {
        $content += @"

    /// <summary>
    /// $($prop.description)
    /// </summary>
    public $($prop.type) $($prop.name) { get; set; }
"@
    }

    $content += @"
}
"@

    $filePath = "StorageSyncService\$className.cs"
    Write-Host "Would create: $filePath"
}

Write-Host "`nScript for batch generation. Execute manually or adapt for actual file creation."
