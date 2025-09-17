namespace Azure.Mcp.Tools.Playwright.Options;

public class PlaywrightWorkspaceUpdateOptions : BasePlaywrightOptions
{
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}
