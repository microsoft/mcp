using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class FileSystemUpdateOptions : BaseManagedLustreOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

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
}
