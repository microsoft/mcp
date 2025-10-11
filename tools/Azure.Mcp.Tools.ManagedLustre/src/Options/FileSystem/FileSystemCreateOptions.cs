using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class FileSystemCreateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.sku)]
    public string? Sku { get; set; }

    [JsonPropertyName(ManagedLustreOptionDefinitions.size)]
    public int? SizeTiB { get; set; }

    [JsonPropertyName("subnet-id")]
    public string? SubnetId { get; set; }

    [JsonPropertyName("zone")]
    public string? Zone { get; set; }

    [JsonPropertyName("hsm-container")]
    public string? HsmContainer { get; set; }

    [JsonPropertyName("hsm-log-container")]
    public string? HsmLogContainer { get; set; }

    [JsonPropertyName("import-prefix")]
    public string? ImportPrefix { get; set; }

    [JsonPropertyName("maintenance-day")]
    public string? MaintenanceDay { get; set; }

    [JsonPropertyName("maintenance-time")]
    public string? MaintenanceTime { get; set; }

    [JsonPropertyName("root-squash-mode")]
    public string? RootSquashMode { get; set; }

    [JsonPropertyName("no-squash-nid-list")]
    public string? NoSquashNidLists { get; set; }

    [JsonPropertyName("squash-uid")]
    public long? SquashUid { get; set; }

    [JsonPropertyName("squash-gid")]
    public long? SquashGid { get; set; }

    [JsonPropertyName("custom-encryption")]
    public bool? EnableCustomEncryption { get; set; }

    [JsonPropertyName("key-url")]
    public string? KeyUrl { get; set; }

    [JsonPropertyName("source-vault")]
    public string? SourceVaultId { get; set; }

    [JsonPropertyName("user-assigned-identity-id")]
    public string? UserAssignedIdentityId { get; set; }
}
