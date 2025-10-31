using Azure.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public class DebugOneLakeRawOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string? Path { get; set; }
    public bool Recursive { get; set; } = false;
}