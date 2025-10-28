using Azure.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public class DebugOneLakeItemsXmlOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? ContinuationToken { get; set; }
}